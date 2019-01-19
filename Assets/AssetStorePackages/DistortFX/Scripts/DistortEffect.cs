using System;
using UnityEngine;

[ExecuteInEditMode]
public class DistortEffect : MonoBehaviour {

  public enum Quality {
    High,
    Medium,
    Low,
    Off
  }

  Camera _camera;

  Material _distortMaterial;
  Material _blurMaterial;

  [SerializeField]
  DistortCamera _distortCamera;

  [SerializeField]
  LayerMask _distortLayers = -1;

  [SerializeField]
  Quality _distortQuality = Quality.High;

  [SerializeField]
  Boolean _distortFadeEnabled = true;

  [SerializeField]
  Single _distortFadeStart = 10;

  [SerializeField]
  Single _distortFadeEnd = 100;

  [SerializeField]
  Boolean _blurEnabled = true;

  [SerializeField]
  Boolean _blurUseSGX = false;

  [Range(0, 2)]
  [SerializeField]
  Int32 _blurDownsample = 1;

  [Range(0.0f, 10.0f)]
  [SerializeField]
  Single _blurSize = 3.0f;

  [Range(1, 4)]
  [SerializeField]
  Int32 _blurIterations = 2;

  void CheckKeyword(String keyword, Boolean wantedState) {
    var currentState = Shader.IsKeywordEnabled(keyword);
    if (currentState != wantedState) {
      if (wantedState) {
        Shader.EnableKeyword(keyword);
      } else {
        Shader.DisableKeyword(keyword);
      }
    }
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    if (!_camera) {
      _camera = GetComponent<Camera>();
      _camera.depthTextureMode |= DepthTextureMode.Depth;
    }

    if (!_distortCamera) {
      _distortCamera = GetComponentInChildren<DistortCamera>();
    }

    if (_camera && _camera.orthographic == false && _distortCamera && _distortQuality != Quality.Off) {
      if (!_distortMaterial) {
        _distortMaterial = new Material(Shader.Find("Distort Fx/Image Effect"));
      }

      if (!_blurMaterial) {
        _blurMaterial = new Material(Shader.Find("Hidden/FastBlur"));
      }

      CheckKeyword("DISTORTFX_BLUR", _blurEnabled);
      CheckKeyword("DISTORTFX_DISTANCE_FADE", _distortFadeEnabled);

      // distort values
      Shader.SetGlobalFloat("_DistortFxFadeStart", _distortFadeStart);
      Shader.SetGlobalFloat("_DistortFxFadeEnd", _distortFadeEnd);

      // render distort data
      _distortCamera.Render(_camera, _distortLayers, _distortQuality);

      // push into material
      _distortMaterial.SetTexture("_DistortTex", _distortCamera.targetCamera.targetTexture);

      RenderTexture blurred = null;

      // blur main texture
      if (_blurEnabled) {
        _distortMaterial.SetTexture("_MainTexBlur", blurred = Blur2(source));
      }


      // render it
      Graphics.Blit(source, destination, _distortMaterial);

      // release blurred texture if set
      if (blurred != null) {
        RenderTexture.ReleaseTemporary(blurred);
      }
    } else {
      Graphics.Blit(source, destination);
    }
  }

  RenderTexture Blur2(RenderTexture source) {
    var widthMod = 1.0f / (1.0f * (1 << _blurDownsample));

    _blurMaterial.SetVector("_Parameter", new Vector4(_blurSize * widthMod, -_blurSize * widthMod, 0.0f, 0.0f));

    source.filterMode = FilterMode.Bilinear;

    var rtW = source.width >> _blurDownsample;
    var rtH = source.height >> _blurDownsample;

    // temp rt1
    RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
    rt.filterMode = FilterMode.Bilinear;

    // temp rt2
    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
    rt2.filterMode = FilterMode.Bilinear;

    // copy from source
    Graphics.Blit(source, rt, _blurMaterial, 0);

    for (var i = 0; i < _blurIterations; i++) {
      _blurMaterial.SetVector("_Parameter", new Vector4(_blurSize * widthMod + i, -_blurSize * widthMod - i, 0.0f, 0.0f));

      // vertical blur
      Graphics.Blit(rt, rt2, _blurMaterial, _blurUseSGX ? 3 : 1);
      Switch(ref rt, ref rt2);

      // horizontal blur
      Graphics.Blit(rt, rt2, _blurMaterial, _blurUseSGX ? 4 : 2);
      Switch(ref rt, ref rt2);
    }

    RenderTexture.ReleaseTemporary(rt2);

    return rt;
  }

  static void Switch(ref RenderTexture a, ref RenderTexture b) {
    var tmp = a;
    a = b;
    b = tmp;
  }
}
