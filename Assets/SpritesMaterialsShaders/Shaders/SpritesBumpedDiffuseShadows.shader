Shader "Sprites/Bumped Diffuse with Shadows" {
    Properties {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _BumpScale("Normal Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        _Cutoff("Shadow Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader {
        Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 0
        Lighting Off
        ZWrite Off
        Fog{ Mode Off }
        Cull Back


        CGPROGRAM
            #pragma surface surf Lambert vertex:vert alpha

            sampler2D _MainTex;
            fixed4 _Color;

            struct Input {
                float2 uv_MainTex;
                fixed4 color;
            };

            void vert(inout appdata_full v, out Input o) {
                v.normal = float3(0,0,-1);
                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.color = _Color;
            }

            void surf(Input IN, inout SurfaceOutput o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
        ENDCG

        LOD 300
        Cull Back

        CGPROGRAM
            #pragma target 3.0
            #pragma surface surf Lambert vertex:vert addshadow fullforwardshadows alphatest:_Cutoff
            #pragma multi_compile DUMMY PIXELSNAP_ON

            sampler2D _MainTex;
            sampler2D _BumpMap;
            fixed4 _Color;
            float _BumpScale;

            struct Input {
                float2 uv_MainTex;
                float2 uv_BumpMap;
                fixed4 color;
            };

            void vert(inout appdata_full v, out Input o) {
                #if defined(PIXELSNAP_ON) && !defined(SHADER_API_FLASH)
                v.vertex = UnityPixelSnap(v.vertex);
                #endif
                v.normal = float3(0,0,-1);

                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.color = _Color;
            }

            void surf(Input IN, inout SurfaceOutput o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
                o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
            }
        ENDCG
    }

    Fallback "Sprites/Diffuse"
}
