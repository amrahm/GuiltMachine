Shader "Hidden/Distort Fx/Replacement"
{
  Properties
  {
  }
  SubShader
  {
    BlendOp Add, Add
    Blend One One
    Cull Off 
    Lighting Off 
    ZWrite Off
    Fog { Mode Off }
    Tags { "RenderType" = "Opaque" "Distort" = "TriPlanar" }
    
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0
      #pragma multi_compile __ DISTORTFX_DISTANCE_FADE
      
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 color : COLOR;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float3 worldNormal : NORMAL;
        float3 worldPos : TEXCOORD2;
        float4 projPos : TEXCOORD3;
        float2 depth : TEXCOORD0;
        float3 viewDir : TEXCOORD1;
        float4 color : COLOR;
      };

      sampler2D _LastCameraDepthTexture;
      sampler2D _DistortTexture;
      float4 _DistortTexture_ST;
      float4 _DistortSettings;
      float _DistortFxFadeStart;
      float _DistortFxFadeEnd;
      
      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex * _DistortSettings.z);
        o.projPos = ComputeScreenPos(o.vertex);
        o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

        o.color = v.color;
        o.worldNormal = UnityObjectToWorldNormal(v.normal);

        float3 worldDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
        o.viewDir = normalize(worldDir);

#if DISTORTFX_DISTANCE_FADE
        float worldDistance = length(worldDir);
        o.color.a = o.color.a * (1.0 - saturate((worldDistance - _DistortFxFadeStart) / (_DistortFxFadeEnd - _DistortFxFadeStart)));
#endif

        COMPUTE_EYEDEPTH(o.depth);

        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {
        float fragDepth = LinearEyeDepth(i.depth.x);
        float sceneDepth = tex2Dproj(_LastCameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;

        clip(fragDepth - sceneDepth);
        
        half3 triblend = pow(abs(i.worldNormal), 4);
        triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

        float2 uvX = (i.worldPos.zy + (_Time.xx * _DistortTexture_ST.zw)) * _DistortTexture_ST.xy;
        float2 uvY = (i.worldPos.xz + (_Time.xx * _DistortTexture_ST.wz)) * _DistortTexture_ST.xy;
        float2 uvZ = (i.worldPos.xy + (_Time.xx * _DistortTexture_ST.zz)) * _DistortTexture_ST.xy;

        half3 tnormalX = UnpackNormal(tex2D(_DistortTexture, uvX));
        half3 tnormalY = UnpackNormal(tex2D(_DistortTexture, uvY));
        half3 tnormalZ = UnpackNormal(tex2D(_DistortTexture, uvZ));

        half2 col = normalize(tnormalX * triblend.x + tnormalY * triblend.y + tnormalZ * triblend.z).rg;

        col *= _DistortSettings.x;

        float ndotv_org = dot(i.worldNormal, normalize(i.viewDir));
        float rim = saturate(abs(ndotv_org));
        rim = pow(rim, _DistortSettings.y);

        return fixed4(col * saturate(rim) * i.color.a, 0, _DistortSettings.w);
      }
      ENDCG
    }
  }
  SubShader
  {
    BlendOp Add, Add
    Blend One One
    Cull Off 
    Lighting Off 
    ZWrite Off
    Fog { Mode Off }
    Tags { "RenderType" = "Opaque" "Distort" = "UVMesh" }
    
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0
      #pragma multi_compile __ DISTORTFX_DISTANCE_FADE
      
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float3 worldNormal : NORMAL;
        float3 worldPos : TEXCOORD3;
        float4 projPos : TEXCOORD4;
        float2 uv : TEXCOORD0;
        float2 depth : TEXCOORD1;
        float3 viewDir : TEXCOORD2;
        float4 color : COLOR;
      };

      sampler2D _LastCameraDepthTexture;
      sampler2D _DistortTexture;
      float4 _DistortTexture_ST;
      float4 _DistortSettings;
      float _DistortFxFadeStart;
      float _DistortFxFadeEnd;
      
      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex * _DistortSettings.z);
        o.projPos = ComputeScreenPos(o.vertex);
        o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
        o.uv = (v.uv + (_Time.xx * _DistortTexture_ST.wz)) * _DistortTexture_ST.xy;

        o.color = v.color;
        o.worldNormal = UnityObjectToWorldNormal(v.normal);

        float3 worldDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
        o.viewDir = normalize(worldDir);

#if DISTORTFX_DISTANCE_FADE
        float worldDistance = length(worldDir);
        o.color.a = o.color.a * (1.0 - saturate((worldDistance - _DistortFxFadeStart) / (_DistortFxFadeEnd - _DistortFxFadeStart)));
#endif

        COMPUTE_EYEDEPTH(o.depth);

        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {
        float fragDepth = LinearEyeDepth(i.depth.x);
        float sceneDepth = tex2Dproj(_LastCameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;

        clip(fragDepth - sceneDepth);
        
        half2 col = UnpackNormal(tex2D(_DistortTexture, i.uv)).rg;

        col *= _DistortSettings.x;

        float ndotv_org = dot(i.worldNormal, normalize(i.viewDir));
        float rim = saturate(abs(ndotv_org));
        rim = pow(rim, _DistortSettings.y);

        return fixed4(col * saturate(rim) * i.color.a, 0, _DistortSettings.w * saturate(rim));
      }
      ENDCG
    }
  }
  SubShader
  {
    BlendOp Add, Add
    Blend One One
    Cull Off 
    Lighting Off 
    ZWrite Off
    Fog { Mode Off }
    Tags { "RenderType" = "Opaque" "Distort" = "UVPlane" }

    Pass
    {
      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 3.0
      #pragma multi_compile __ DISTORTFX_DISTANCE_FADE
      
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float4 color : COLOR;
        float2 uvMask : TEXCOORD0;
        float2 uvDistort : TEXCOORD1;
        float2 depth : TEXCOORD2;
        float4 projPos : TEXCOORD3;
      };

      sampler2D _DistortTexture;
      sampler2D _DistortTextureMask;
      sampler2D _LastCameraDepthTexture;

      float4 _DistortSettings;
      float4 _DistortTexture_ST;
      float4 _DistortTextureMask_ST;
      float _DistortFxFadeStart;
      float _DistortFxFadeEnd;
      
      v2f vert (appdata v)
      {
        v2f o;
        o.color = v.color;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.projPos = ComputeScreenPos(o.vertex);

        o.uvMask = TRANSFORM_TEX(v.uv, _DistortTextureMask);
        o.uvDistort = TRANSFORM_TEX(v.uv, _DistortTexture);
        o.uvDistort.x += _DistortSettings.y * _Time.x;
        o.uvDistort.y += _DistortSettings.z * _Time.x;

#if DISTORTFX_DISTANCE_FADE
        float3 worldDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
        float worldDistance = length(worldDir);
        o.color.a = o.color.a * (1.0 - saturate((worldDistance - _DistortFxFadeStart) / (_DistortFxFadeEnd - _DistortFxFadeStart)));
#endif

        COMPUTE_EYEDEPTH(o.depth);

        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {
        float fragDepth = LinearEyeDepth(i.depth.x);
        float sceneDepth = tex2Dproj(_LastCameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;
        
        clip(fragDepth - sceneDepth);

        fixed2 dist = UnpackNormal(tex2D(_DistortTexture, i.uvDistort));
        fixed3 mask = tex2D(_DistortTextureMask, i.uvMask);
        fixed amount = mask.r * i.color.a;

        return fixed4(dist * _DistortSettings.x * amount, 0, _DistortSettings.w * amount);
      }
      ENDCG
    }
  }
}
