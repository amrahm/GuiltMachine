Shader "Stylized FX/Shield" {
  Properties {
    _DistortTexture("Distort Texture", 2D) = "bump" {}
    [DistortSettingsProperty(Mesh)] _DistortSettings("Settings", Vector) = (10, 0, 1, 1)

    _MainTex("Main Tex (Triplanar)", 2D) = "white" {}
    _MainTexScale("Main Tex Scale", Float) = 0.0
    _MainTexScroll ("Main Tex Scroll", Float) = 0.0
    _MainTexStep("Main Tex Step", Range(2, 64)) = 4

    [HDR] _NoiseColor1("Noise Color 1", Color) = (0.5,0.5,0.5,0.5)
    [HDR] _NoiseColor2("Noise Color 2", Color) = (0.5,0.5,0.5,0.5)
    _NoiseScale("Noise Scale", Float) = 1
    _NoiseTimeScale("Noise Time Scale", Range(0, 10)) = 0.25
    _NoiseStep("Noise Step", Range(2, 64)) = 4

    [HDR] _IntersectColor("Intersect Color", Color) = (0.5,0.5,0.5,0.5)
    _IntersectPower("Intersect Power", Range(0.1,10.0)) = 1.0
    _IntersectStep("Intersect Step", Range(2, 64)) = 4

    [HDR] _RimColor("Rim Color", Color) = (0.5,0.5,0.5,0.5)
    _RimPower("Rim Power", Range(0.01,10.0)) = 1.0
    _RimStep("Rim Step", Range(2, 64)) = 4

    _FadeStrength("Fade Strength", Range(-0.2, 0.2)) = 0.0
  }

  Category {
    Tags { 
      "Queue" = "Transparent"
      "IgnoreProjector" = "True"
      "RenderType" = "Transparent"
      "PreviewType" = "Sphere" 
      "Distort" = "UVMesh"
    }

    Blend SrcAlpha One
    ColorMask RGB
    Cull Off 
    Lighting Off 
    ZWrite Off

    SubShader {
      GrabPass {
        Name "BASE"
        Tags { 
          "LightMode" = "Always" 
        }
      }

      Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0

        #include "UnityCG.cginc"

        float hash(float n) {
          return frac(sin(n)*43758.5453);
        }

        float noise(float3 x) {
          float3 p = floor(x);
          float3 f = frac(x);

          f = f*f*(3.0 - 2.0*f);
          float n = p.x + p.y*57.0 + 113.0*p.z;

          return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x), lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y), lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x), lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
        }

        fixed4 _NoiseColor1;
        fixed4 _NoiseColor2;

        fixed _NoiseScale;
        fixed _NoiseStep;
        fixed _NoiseTimeScale;

        fixed4 _IntersectColor;
        fixed _IntersectPower;
        fixed _IntersectStep;

        fixed4 _RimColor;
        fixed _RimPower;
        fixed _RimStep;

        fixed _FadeStrength;

        sampler2D _MainTex;

        fixed _MainTexScale;
        fixed _MainTexScroll;
        fixed _MainTexStep;

        struct appdata_t {
          float4 vertex : POSITION;
          float3 normal : NORMAL;
          float4 color : COLOR;
          float4 uv : TEXCOORD0;

          UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
          float4 vertex : SV_POSITION;
          float4 color : COLOR;
          float4 projPos : TEXCOORD0;
          float3 viewDir : TEXCOORD1;
          float3 worldPos : TEXCOORD2;
          float3 worldNormal : NORMAL;
          float4 uv : TEXCOORD4;

          UNITY_FOG_COORDS(1)

          UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert(appdata_t v)
        {
          v2f o;

          UNITY_SETUP_INSTANCE_ID(v);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

          o.vertex = UnityObjectToClipPos(v.vertex);
          o.worldPos = mul(unity_ObjectToWorld, v.vertex);
          o.worldNormal = UnityObjectToWorldNormal(v.normal);
          o.projPos = ComputeScreenPos(o.vertex);
          o.uv = v.uv;
          o.color = v.color;

          COMPUTE_EYEDEPTH(o.projPos.z);

          o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);

          UNITY_TRANSFER_FOG(o,o.vertex);

          return o;
        }

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);


        fixed4 frag(v2f i) : SV_Target
        {
          half time = (_Time.x * _MainTexScroll);

          half2 yUV = i.worldPos.xz / _MainTexScale;
          half2 xUV = i.worldPos.zy / _MainTexScale;
          half2 zUV = i.worldPos.xy / _MainTexScale;

          half4 yDiff = tex2D(_MainTex, yUV + time);
          half4 xDiff = tex2D(_MainTex, xUV + time);
          half4 zDiff = tex2D(_MainTex, zUV + time);

          half3 blendWeights = abs(i.worldNormal);
          blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
          
          half4 mt = (xDiff * blendWeights.x) + (yDiff * blendWeights.y) + (zDiff * blendWeights.z);

          float ndotv_org = dot(i.worldNormal, normalize(i.viewDir));
          float ndotv = 1 - abs(ndotv_org);

          float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
          float intersect = pow(saturate(1 - (sceneZ - i.projPos.z)), _IntersectPower);
          intersect = floor(intersect * _IntersectStep) / _IntersectStep;

          float rim = saturate(ndotv);
          rim = pow(rim, _RimPower);
          rim = floor(rim * _RimStep) / _RimStep;

          float ns = noise((i.worldPos + (_Time.y * _NoiseTimeScale)) * _NoiseScale);
          ns = floor(ns * _NoiseStep) / _NoiseStep;

          fixed4 col = lerp(_NoiseColor1, _NoiseColor2, ns) * (1 - rim) * (1 - intersect);
          col.rgb *= saturate(floor(mt.rgb * _MainTexStep) / _MainTexStep);

          ndotv = 1 - max(0, ndotv_org);
          col = col + (_RimColor * rim);
          col = col * max(ndotv - _FadeStrength, 0);
          col = col + (_IntersectColor * intersect);

          UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0));


          return (col * i.color);
        }

        ENDCG
      }
    }
  }
}
