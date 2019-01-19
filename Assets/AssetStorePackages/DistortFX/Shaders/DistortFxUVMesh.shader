Shader "Distort Fx/UV Mesh"
{
  Properties
  {
    _DistortTexture("Distort Texture", 2D) = "bump" {}
    [DistortSettingsProperty(Mesh)] _DistortSettings ("Distort Settings", Vector) = (10, 0, 1, 1)
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
      "PreviewType" = "Sphere" 
      "Distort" = "UVMesh"
    }
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
      };

      float _Show;

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
