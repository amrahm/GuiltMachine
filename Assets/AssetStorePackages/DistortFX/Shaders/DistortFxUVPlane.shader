Shader "Distort Fx/UV Plane"
{
  Properties
  {
    _DistortTexture ("Distort Texture", 2D) = "bump" {}
    _DistortTextureMask ("Distort Texture Mask", 2D) = "white" {}
    [DistortSettingsProperty(Plane)] _DistortSettings ("Settings", Vector) = (10, 0, 0, 0)
    _Show ("Show", Float) =  0
  }
  SubShader
  {
    Blend One One
    Cull Off
    ZWrite Off
    Tags { 
      "Queue" = "Transparent"
      "IgnoreProjector" = "True"
      "RenderType" = "Transparent"
      "Distort" = "UVPlane"
    }

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"

      fixed _Show;

      struct appdata
      {
        float4 vertex : POSITION;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {
        return _Show;
      }
      ENDCG
    }
  }
}
