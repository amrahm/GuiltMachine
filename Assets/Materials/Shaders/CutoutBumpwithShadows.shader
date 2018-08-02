Shader "Transparent/Bumped Diffuse with Shadow" {
    Properties {
	    _Color ("Main Color", Color) = (1,1,1,1)
	    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _BumpScale("Normal Scale", Float) = 1.0
	    _BumpMap ("Normalmap", 2D) = "bump" {}
	    _Cutoff("Cutoff", Float) = 0.01
    }

    SubShader {
	    Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="TransparentCutOut" }
	    LOD 300

	    CGPROGRAM
            #pragma target 3.0
            #pragma surface surf Lambert addshadow fullforwardshadows alphatest:_Cutoff
	
	        sampler2D _MainTex;
	        sampler2D _BumpMap;
	        fixed4 _Color;
            float _BumpScale;

	        struct Input {
		        float2 uv_MainTex;
		        float2 uv_BumpMap;
	        };
	
	        void surf (Input IN, inout SurfaceOutput o) {
		        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
        //        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		        o.Albedo = c.rgb;
		        o.Alpha = c.a;
	        }
	    ENDCG
    }

    Fallback "Transparent/Diffuse"
}