using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class DistortCamera : MonoBehaviour {
  [SerializeField]
  Camera _targetCamera;

  Shader _shader;
  DistortEffect.Quality _quality;

  public Camera targetCamera {
    get { return _targetCamera; }
  }

  public Int32 Width {
    get {
      switch (_quality) {
        case DistortEffect.Quality.Medium: return Screen.width / 2;
        case DistortEffect.Quality.Low: return Screen.width / 4;
      }

      return Screen.width;
    }
  }

  public Int32 Height {
    get {
      switch (_quality) {
        case DistortEffect.Quality.Medium: return Screen.height / 2;
        case DistortEffect.Quality.Low: return Screen.height / 4;
      }

      return Screen.height;
    }
  }

  protected void Start() {
    _targetCamera.enabled = false;
  }

  IEnumerator ReleaseRenderTexture(RenderTexture rt) {
    yield return null;
    yield return null;

    if (rt) {
      rt.Release();
      Destroy(rt);
    }
  }

  void CreateRenderTexture() {
    _targetCamera.targetTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat);
    _targetCamera.targetTexture.name = "Distort Volumes Texture";
  }

  void EnsureRenderTexture() {
    if (_targetCamera.targetTexture) {
      var tt = _targetCamera.targetTexture;
      if (tt.width != Width || tt.height != Height) {

        // release old texture
        ReleaseRenderTexture(tt);

        // clear it out
        _targetCamera.targetTexture = null;

        // create new texture
        CreateRenderTexture();
      }
    } else {
      CreateRenderTexture();
    }
  }

  public void Render(Camera mainCamera, LayerMask layerMask, DistortEffect.Quality quality) {
    _quality = quality;

    EnsureRenderTexture();

    _targetCamera.allowHDR = false;
    _targetCamera.allowMSAA = false;
    _targetCamera.orthographic = false;
    _targetCamera.useOcclusionCulling = false;

    _targetCamera.cullingMask = layerMask;
    _targetCamera.renderingPath = RenderingPath.Forward;
    _targetCamera.backgroundColor = new Color(1, 1, 1, 1);
    _targetCamera.clearFlags = CameraClearFlags.SolidColor;

    _targetCamera.fieldOfView = mainCamera.fieldOfView;
    _targetCamera.farClipPlane = mainCamera.farClipPlane;
    _targetCamera.nearClipPlane = mainCamera.nearClipPlane;

    // find replacement shader
    if (!_shader) {
      _shader = Shader.Find("Hidden/Distort Fx/Replacement");
    }

    _targetCamera.SetReplacementShader(_shader, "Distort");
    _targetCamera.Render();
  }
}
