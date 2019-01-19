Shader "Distort Fx/Image Effect" {
  Properties{
    _MainTex("Render Input", 2D) = "white" {}
  }
    SubShader{
      ZTest Always 
      Cull Off 
      ZWrite Off 
      Fog { Mode Off }
      
      Pass {
        CGPROGRAM
          #pragma vertex vert_img
          #pragma fragment frag
          #pragma multi_compile __ DISTORTFX_BLUR

          #include "UnityCG.cginc"

          sampler2D _MainTex;
          float4 _MainTex_TexelSize;

#if DISTORTFX_BLUR
          sampler2D _MainTexBlur;
#endif

          sampler2D _DistortTex;

          float4 frag(v2f_img IN) : COLOR {
            float4 bump = tex2D(_DistortTex, IN.uv);
            float2 uv = IN.uv + (bump.rg * float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));

#if DISTORTFX_BLUR
            float4 mt = tex2D(_MainTex, uv);
            float4 mt_blur = tex2D(_MainTexBlur, uv);
            return lerp(mt, mt_blur, min(1, bump.a));
#else
            return tex2D(_MainTex, uv);
#endif
          }
        ENDCG
      }
  }
}