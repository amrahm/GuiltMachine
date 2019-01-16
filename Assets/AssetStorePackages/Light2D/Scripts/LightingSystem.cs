using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Light2D {
    /// <inheritdoc />
    /// <summary>
    ///     Main script for lights. Should be attached to camera.
    ///     Handles lighting operation like camera setup, shader setup, merging cameras output together, blurring and some
    ///     others.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class LightingSystem : MonoBehaviour {
        /// <summary>
        ///     Size of lighting pixel in Unity meters. Controls resoultion of lighting textures.
        ///     Smaller value - better quality, but lower performance.
        /// </summary>
        public float lightPixelSize = 0.05f;

        /// <summary>
        ///     Needed for off screen lights to work correctly. Set that value to radius of largest light.
        ///     Used only when camera is in orthographic mode. Big values could cause a performance drop.
        /// </summary>
        public float lightCameraSizeAdd = 3;

        /// <summary>
        ///     Needed for off screen lights to work correctly.
        ///     Used only when camera is in perspective mode.
        /// </summary>
        public float lightCameraFovAdd = 30;

        /// <summary>
        ///     Enable/disable ambient lights. Disable it to improve performance if you not using ambient light.
        /// </summary>
        public bool enableAmbientLight = true;

        /// <summary>
        ///     LightSourcesBlurMaterial is applied to light sources texture if enabled. Disable to improve performance.
        /// </summary>
        public bool blurLightSources = true;

        /// <summary>
        ///     AmbientLightBlurMaterial is applied to ambient light texture if enabled. Disable to improve performance.
        /// </summary>
        public bool blurAmbientLight = true;

        /// <summary>
        ///     If true RGBHalf RenderTexture type will be used for light processing.
        ///     That could improve smoothness of lights. Will be turned off if device is not supports it.
        /// </summary>
        public bool hdr = true;

        /// <summary>
        ///     If true light obstacles will be rendered in 2x resolution and then downsampled to 1x.
        /// </summary>
        public bool lightObstaclesAntialiasing = true;

        /// <summary>
        ///     Set it to distance from camera to plane with light obstacles. Used only when camera in perspective mode.
        /// </summary>
        public float lightObstaclesDistance = 10;

        /// <summary>
        ///     Billinear for blurred lights, Point for pixelated lights.
        /// </summary>
        public FilterMode lightTexturesFilterMode = FilterMode.Bilinear;

        /// <summary>
        ///     Normal mapping. Not supported on mobiles.
        /// </summary>
        public bool enableNormalMapping;

        /// <summary>
        ///     If true lighting won't be seen on contents of previous cameras.
        /// </summary>
        public bool affectOnlyThisCamera;

        public Material ambientLightComputeMaterial;
        public Material lightOverlayMaterial;
        public Material lightSourcesBlurMaterial;
        public Material ambientLightBlurMaterial;
        public Camera lightCamera;
        public int lightSourcesLayer;
        public int ambientLightLayer;
        public int lightObstaclesLayer;
        public LayerMask lightObstaclesReplacementShaderLayer;
        public bool xzPlane;

        private RenderTexture _ambientEmissionTexture;
        private RenderTexture _ambientTexture;
        private RenderTexture _prevAmbientTexture;
        private RenderTexture _bluredLightTexture;
        private RenderTexture _obstaclesUpsampledTexture;
        private RenderTexture _lightSourcesTexture;
        private RenderTexture _obstaclesTexture;
        private RenderTexture _screenBlitTempTex;
        private RenderTexture _normalMapBuffer;
        private RenderTexture _singleLightSourceTexture;
        private RenderTexture _renderTargetTexture;
        private RenderTexture _oldActiveRenderTexture;

        private Camera _camera;
        private ObstacleCameraPostPorcessor _obstaclesPostProcessor;
        private Point2 _extendedLightTextureSize;
        private Point2 _smallLightTextureSize;
        private Vector3 _oldPos;
        private Vector3 _currPos;
        private RenderTextureFormat _texFormat;
        private int _aditionalAmbientLightCycles;
        private static LightingSystem _instance;
        private Shader _normalMapRenderShader;
        private Shader _lightBlockerReplacementShader;
        private Camera _normalMapCamera;
        private readonly List<LightSprite> _lightSpritesCache = new List<LightSprite>();
        private Material _normalMappedLightMaterial;
        private Material _lightCombiningMaterial;
        private Material _alphaBlendedMaterial;

        private void Reset() {
            lightSourcesLayer = LayerMask.NameToLayer("LightSources");
            ambientLightLayer = LayerMask.NameToLayer("AmbientLight");
            lightObstaclesLayer = LayerMask.NameToLayer("LightObstacles");
        }

        [ContextMenu("Create Camera")]
        private void CreateCamera() {
            if(lightCamera == null) {
                GameObject go = new GameObject("Ligt Camera", typeof(Camera));
                go.transform.SetParent(transform, false);
                lightCamera = go.GetComponent<Camera>();
            }
        }

        private float LightPixelsPerUnityMeter => 1 / lightPixelSize;

        public static LightingSystem Instance => _instance ?? (_instance = FindObjectOfType<LightingSystem>());


        private void OnEnable() {
            _instance = this;
            _camera = GetComponent<Camera>();
        }

        private void Start() {

            if(lightCamera == null) {
                Debug.LogError(
                    "Lighting Camera in LightingSystem is null. Please, select Lighting Camera camera for lighting to work.");
                enabled = false;
                return;
            }
            if(lightOverlayMaterial == null) {
                Debug.LogError(
                    "LightOverlayMaterial in LightingSystem is null. Please, select LightOverlayMaterial camera for lighting to work.");
                enabled = false;
                return;
            }
            if(affectOnlyThisCamera && _camera.targetTexture != null) {
                Debug.LogError("\"Affect Only This Camera\" will not work if camera.targetTexture is set.");
                affectOnlyThisCamera = false;
            }

            _camera = GetComponent<Camera>();

            if(enableNormalMapping && !_camera.orthographic) {
                Debug.LogError("Normal mapping is not supported with perspective camera.");
                enableNormalMapping = false;
            }

            // if both FlareLayer component and AffectOnlyThisCamera setting is enabled
            // Unity will print an error "Flare renderer to update not found" 
            FlareLayer flare = GetComponent<FlareLayer>();
            if(flare != null && flare.enabled) {
                Debug.Log("Disabling FlareLayer since AffectOnlyThisCamera setting is checked.");
                flare.enabled = false;
            }

            if(!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                hdr = false;
            _texFormat = hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;

            float lightPixelsPerUnityMeter = LightPixelsPerUnityMeter;

            if(_camera.orthographic) {
                float rawCamHeight = (_camera.orthographicSize + lightCameraSizeAdd) * 2f;
                float rawCamWidth = (_camera.orthographicSize * _camera.aspect + lightCameraSizeAdd) * 2f;

                _extendedLightTextureSize = new Point2(
                    Mathf.RoundToInt(rawCamWidth * lightPixelsPerUnityMeter),
                    Mathf.RoundToInt(rawCamHeight * lightPixelsPerUnityMeter));

                float rawSmallCamHeight = _camera.orthographicSize * 2f * lightPixelsPerUnityMeter;
                _smallLightTextureSize = new Point2(
                    Mathf.RoundToInt(rawSmallCamHeight * _camera.aspect),
                    Mathf.RoundToInt(rawSmallCamHeight));
            } else {
                {
                    float lightCamHalfFov = (_camera.fieldOfView + lightCameraFovAdd) * Mathf.Deg2Rad / 2f;
                    float lightCamSize = Mathf.Tan(lightCamHalfFov) * lightObstaclesDistance * 2;
                    //var gameCamHalfFov = _camera.fieldOfView*Mathf.Deg2Rad/2f;
                    int texHeight = Mathf.RoundToInt(lightCamSize / lightPixelSize);
                    float texWidth = texHeight * _camera.aspect;
                    _extendedLightTextureSize = Point2.Round(new Vector2(texWidth, texHeight));
                }
                {
                    float lightCamHalfFov = _camera.fieldOfView * Mathf.Deg2Rad / 2f;
                    float lightCamSize = Mathf.Tan(lightCamHalfFov) * lightObstaclesDistance * 2;
                    //LightCamera.orthographicSize = lightCamSize/2f;

                    float gameCamHalfFov = _camera.fieldOfView * Mathf.Deg2Rad / 2f;
                    float gameCamSize = Mathf.Tan(gameCamHalfFov) * lightObstaclesDistance * 2;
                    _camera.orthographicSize = gameCamSize / 2f;

                    int texHeight = Mathf.RoundToInt(lightCamSize / lightPixelSize);
                    float texWidth = texHeight * _camera.aspect;
                    _smallLightTextureSize = Point2.Round(new Vector2(texWidth, texHeight));
                }
            }

            if(_extendedLightTextureSize.x % 2 != 0)
                _extendedLightTextureSize.x++;
            if(_extendedLightTextureSize.y % 2 != 0)
                _extendedLightTextureSize.y++;

            if(_extendedLightTextureSize.x > 1024 || _extendedLightTextureSize.y > 1024 ||
               _smallLightTextureSize.x > 1024 || _smallLightTextureSize.y > 1024) {
                Debug.LogError("LightPixelSize is too small. That might have a performance impact.");
                return;
            }

            if(_extendedLightTextureSize.x < 4 || _extendedLightTextureSize.y < 4 ||
               _smallLightTextureSize.x < 4 || _smallLightTextureSize.y < 4) {
                Debug.LogError("LightPixelSize is too big. Lighting may not work correctly.");
                return;
            }

            _screenBlitTempTex = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, _texFormat) {
                filterMode = FilterMode.Point
            };

            lightCamera.orthographic = _camera.orthographic;

            if(enableNormalMapping)
                _lightSourcesTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight,
                    0, _texFormat) {
                    filterMode = FilterMode.Point
                };
            else
                _lightSourcesTexture = new RenderTexture(_smallLightTextureSize.x, _smallLightTextureSize.y,
                    0, _texFormat) {
                    filterMode = lightTexturesFilterMode
                };

            _obstaclesTexture = new RenderTexture(_extendedLightTextureSize.x, _extendedLightTextureSize.y,
                0, _texFormat);
            _ambientTexture = new RenderTexture(_extendedLightTextureSize.x, _extendedLightTextureSize.y,
                0, _texFormat) {
                filterMode = lightTexturesFilterMode
            };

            Point2 upsampledObstacleSize = _extendedLightTextureSize * (lightObstaclesAntialiasing ? 2 : 1);
            _obstaclesUpsampledTexture = new RenderTexture(
                upsampledObstacleSize.x, upsampledObstacleSize.y, 0, _texFormat);

            if(affectOnlyThisCamera) {
                _renderTargetTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32) {
                    filterMode = FilterMode.Point
                };
                _camera.targetTexture = _renderTargetTexture;
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = Color.clear;
            }

            _alphaBlendedMaterial = new Material(Shader.Find("Light2D/Internal/Alpha Blended"));

            _lightBlockerReplacementShader = Shader.Find(@"Light2D/Internal/LightBlockerReplacementShader");

            if(xzPlane)
                Shader.EnableKeyword("LIGHT2D_XZ_PLANE");
            else
                Shader.DisableKeyword("LIGHT2D_XZ_PLANE");

            _obstaclesPostProcessor = new ObstacleCameraPostPorcessor();

            LoopAmbientLight(100);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest) {
            UpdateCamera();
            RenderObstacles();
            SetupShaders();
            RenderNormalBuffer();
            RenderLightSources();
            RenderLightSourcesBlur();
            RenderAmbientLight();
            RenderLightOverlay(src, dest);
        }

        private void OnPreCull() {
            if(Application.isPlaying && affectOnlyThisCamera) _camera.targetTexture = _renderTargetTexture;
        }

        private void OnRenderObject() {
            if(Application.isPlaying && affectOnlyThisCamera) {
                _camera.targetTexture = null;
                Graphics.Blit(_renderTargetTexture, null, _alphaBlendedMaterial);
                _camera.targetTexture = _renderTargetTexture;
            }
        }

        private void RenderObstacles() {
            ConfigLightCamera(true);

            Color oldColor = lightCamera.backgroundColor;
            CameraClearFlags oldClearFlag = lightCamera.clearFlags;
            lightCamera.enabled = false;
            lightCamera.targetTexture = _obstaclesUpsampledTexture;
            lightCamera.cullingMask = 1 << lightObstaclesLayer;
            lightCamera.backgroundColor = new Color(1, 1, 1, 0);

            //normal
            _obstaclesPostProcessor.DrawMesh(lightCamera, lightObstaclesAntialiasing ? 2 : 1);
            lightCamera.Render();

            //replacement
            lightCamera.clearFlags = CameraClearFlags.Nothing;
            lightCamera.cullingMask = lightObstaclesReplacementShaderLayer;
            lightCamera.RenderWithShader(_lightBlockerReplacementShader, "RenderType");

            lightCamera.targetTexture = null;
            lightCamera.cullingMask = 0;
            lightCamera.backgroundColor = oldColor;
            lightCamera.clearFlags = oldClearFlag;

            _obstaclesTexture.DiscardContents();
            Graphics.Blit(_obstaclesUpsampledTexture, _obstaclesTexture);
        }

        private void SetupShaders() {
            float lightPixelsPerUnityMeter = LightPixelsPerUnityMeter;

            if(hdr) Shader.EnableKeyword("HDR");
            else Shader.DisableKeyword("HDR");

            if(_camera.orthographic) Shader.DisableKeyword("PERSPECTIVE_CAMERA");
            else Shader.EnableKeyword("PERSPECTIVE_CAMERA");

            Shader.SetGlobalTexture("_ObstacleTex", _obstaclesTexture);
            Shader.SetGlobalFloat("_PixelsPerBlock", lightPixelsPerUnityMeter);
            Shader.SetGlobalVector("_ExtendedToSmallTextureScale", new Vector2(
                _smallLightTextureSize.x / (float) _extendedLightTextureSize.x,
                _smallLightTextureSize.y / (float) _extendedLightTextureSize.y));
            Shader.SetGlobalVector("_PosOffset", lightObstaclesAntialiasing
                                                     ? (enableNormalMapping ? _obstaclesUpsampledTexture.texelSize * 0.75f : _obstaclesUpsampledTexture.texelSize * 0.25f)
                                                     : (enableNormalMapping ? _obstaclesTexture.texelSize : _obstaclesTexture.texelSize * 0.5f));
        }

        private void RenderNormalBuffer() {
            if(!enableNormalMapping)
                return;

            if(_normalMapBuffer == null)
                _normalMapBuffer = new RenderTexture(
                    _camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32) {
                    filterMode = FilterMode.Point
                };

            if(_normalMapRenderShader == null)
                _normalMapRenderShader = Shader.Find("Light2D/Internal/Normal Map Drawer");

            if(_normalMapCamera == null) {
                GameObject camObj = new GameObject {
                    name = "Normals Camera"
                };
                camObj.transform.parent = _camera.transform;
                camObj.transform.localScale = Vector3.one;
                camObj.transform.localPosition = Vector3.zero;
                camObj.transform.localRotation = Quaternion.identity;
                _normalMapCamera = camObj.AddComponent<Camera>();
                _normalMapCamera.enabled = false;
            }

            _normalMapBuffer.DiscardContents();
            _normalMapCamera.CopyFrom(_camera);
            _normalMapCamera.transform.position = lightCamera.transform.position;
            _normalMapCamera.clearFlags = CameraClearFlags.SolidColor;
            _normalMapCamera.targetTexture = _normalMapBuffer;
            _normalMapCamera.cullingMask = int.MaxValue;
            _normalMapCamera.backgroundColor = new Color(0.5f, 0.5f, 0, 1);
            _normalMapCamera.RenderWithShader(_normalMapRenderShader, "LightObstacle");

            Shader.SetGlobalTexture("_NormalsBuffer", _normalMapBuffer);
            Shader.EnableKeyword("NORMAL_MAPPED_LIGHTS");
        }

        private void RenderLightSources() {
            ConfigLightCamera(false);

            if(enableNormalMapping) {
                if(_singleLightSourceTexture == null)
                    _singleLightSourceTexture = new RenderTexture(
                        _smallLightTextureSize.x, _smallLightTextureSize.y, 0, _texFormat) {
                        filterMode = lightTexturesFilterMode
                    };

                if(_normalMappedLightMaterial == null) {
                    _normalMappedLightMaterial = new Material(Shader.Find("Light2D/Internal/Normal Mapped Light"));
                    _normalMappedLightMaterial.SetTexture("_MainTex", _singleLightSourceTexture);
                }

                if(_lightCombiningMaterial == null) {
                    _lightCombiningMaterial = new Material(Shader.Find("Light2D/Internal/Light Blender"));
                    _lightCombiningMaterial.SetTexture("_MainTex", _singleLightSourceTexture);
                }

                Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);
                _lightSourcesTexture.DiscardContents();

                Color oldBackgroundColor = lightCamera.backgroundColor;
                RenderTexture oldRt = RenderTexture.active;
                Graphics.SetRenderTarget(_lightSourcesTexture);
                GL.Clear(false, true, oldBackgroundColor);
                Graphics.SetRenderTarget(oldRt);

                _lightSpritesCache.Clear();
                foreach(LightSprite lightSprite in LightSprite.AllLightSprites)
                    if(lightSprite.RendererEnabled &&
                       GeometryUtility.TestPlanesAABB(cameraPlanes, lightSprite.Renderer.bounds))
                        _lightSpritesCache.Add(lightSprite);

                Vector3 lightCamLocPos = lightCamera.transform.localPosition;
                lightCamera.targetTexture = _singleLightSourceTexture;
                lightCamera.cullingMask = 0;
                lightCamera.backgroundColor = new Color(0, 0, 0, 0);

                foreach(LightSprite lightSprite in _lightSpritesCache) {
                    // HACK: won't work for unknown reason without that line
                    lightCamera.RenderWithShader(_normalMapRenderShader, "f84j");

                    Graphics.SetRenderTarget(_singleLightSourceTexture);
                    lightSprite.DrawLightingNow(lightCamLocPos);
                    Graphics.SetRenderTarget(_lightSourcesTexture);
                    lightSprite.DrawLightNormalsNow(_normalMappedLightMaterial);
                }
                Graphics.SetRenderTarget(oldRt);

                lightCamera.cullingMask = 1 << lightSourcesLayer;
                lightCamera.Render();
                Graphics.Blit(_singleLightSourceTexture, _lightSourcesTexture, _lightCombiningMaterial);

                lightCamera.targetTexture = null;
                lightCamera.cullingMask = 0;
                lightCamera.backgroundColor = oldBackgroundColor;
            } else {
                lightCamera.targetTexture = _lightSourcesTexture;
                lightCamera.cullingMask = 1 << lightSourcesLayer;
                //LightCamera.backgroundColor = new Color(0, 0, 0, 0);
                lightCamera.Render();
                lightCamera.targetTexture = null;
                lightCamera.cullingMask = 0;
            }
        }

        private void RenderLightSourcesBlur() {
            if(blurLightSources && lightSourcesBlurMaterial != null) {
                Profiler.BeginSample("LightingSystem.OnRenderImage Bluring Light Sources");

                if(_bluredLightTexture == null) {
                    int w = _lightSourcesTexture.width == _smallLightTextureSize.x
                                ? _lightSourcesTexture.width * 2
                                : _lightSourcesTexture.width;
                    int h = _lightSourcesTexture.height == _smallLightTextureSize.y
                                ? _lightSourcesTexture.height * 2
                                : _lightSourcesTexture.height;
                    _bluredLightTexture = new RenderTexture(w, h, 0, _texFormat);
                }

                _bluredLightTexture.DiscardContents();
                _lightSourcesTexture.filterMode = FilterMode.Bilinear;
                lightSourcesBlurMaterial.mainTexture = _lightSourcesTexture;
                Graphics.Blit(null, _bluredLightTexture, lightSourcesBlurMaterial);

                if(lightTexturesFilterMode == FilterMode.Point) {
                    _lightSourcesTexture.filterMode = FilterMode.Point;
                    _lightSourcesTexture.DiscardContents();
                    Graphics.Blit(_bluredLightTexture, _lightSourcesTexture);
                }

                Profiler.EndSample();
            }
        }

        private void RenderAmbientLight() {
            if(!enableAmbientLight || ambientLightComputeMaterial == null)
                return;

            Profiler.BeginSample("LightingSystem.OnRenderImage Ambient Light");

            ConfigLightCamera(true);

            if(_ambientTexture == null)
                _ambientTexture =
                    new RenderTexture(_extendedLightTextureSize.x, _extendedLightTextureSize.y, 0, _texFormat);
            if(_prevAmbientTexture == null)
                _prevAmbientTexture =
                    new RenderTexture(_extendedLightTextureSize.x, _extendedLightTextureSize.y, 0, _texFormat);
            if(_ambientEmissionTexture == null)
                _ambientEmissionTexture =
                    new RenderTexture(_extendedLightTextureSize.x, _extendedLightTextureSize.y, 0, _texFormat);

            if(enableAmbientLight) {
                Color oldBackgroundColor = lightCamera.backgroundColor;
                lightCamera.targetTexture = _ambientEmissionTexture;
                lightCamera.cullingMask = 1 << ambientLightLayer;
                lightCamera.backgroundColor = new Color(0, 0, 0, 0);
                lightCamera.Render();
                lightCamera.targetTexture = null;
                lightCamera.cullingMask = 0;
                lightCamera.backgroundColor = oldBackgroundColor;
            }

            for(int i = 0; i < _aditionalAmbientLightCycles + 1; i++) {
                RenderTexture tmp = _prevAmbientTexture;
                _prevAmbientTexture = _ambientTexture;
                _ambientTexture = tmp;

                Vector2 texSize = new Vector2(_ambientTexture.width, _ambientTexture.height);
                Vector2 posShift = ((Vector2) (_currPos - _oldPos) / lightPixelSize).Div(texSize);
                _oldPos = _currPos;

                ambientLightComputeMaterial.SetTexture("_LightSourcesTex", _ambientEmissionTexture);
                if(_prevAmbientTexture.IsCreated())
                    ambientLightComputeMaterial.SetTexture("_MainTex", _prevAmbientTexture);
                ambientLightComputeMaterial.SetVector("_Shift", posShift);

                _ambientTexture.DiscardContents();
                Graphics.Blit(null, _ambientTexture, ambientLightComputeMaterial);

                if(blurAmbientLight && ambientLightBlurMaterial != null) {
                    Profiler.BeginSample("LightingSystem.OnRenderImage Bluring Ambient Light");

                    _prevAmbientTexture.DiscardContents();
                    ambientLightBlurMaterial.mainTexture = _ambientTexture;
                    Graphics.Blit(null, _prevAmbientTexture, ambientLightBlurMaterial);

                    RenderTexture tmpblur = _prevAmbientTexture;
                    _prevAmbientTexture = _ambientTexture;
                    _ambientTexture = tmpblur;

                    Profiler.EndSample();
                }
            }

            _aditionalAmbientLightCycles = 0;
            Profiler.EndSample();
        }

        private void RenderLightOverlay(RenderTexture src, RenderTexture dest) {
            Profiler.BeginSample("LightingSystem.OnRenderImage Light Overlay");

            ConfigLightCamera(false);

            Vector2 lightTexelSize = new Vector2(1f / _smallLightTextureSize.x, 1f / _smallLightTextureSize.y);
            float lightPixelsPerUnityMeter = LightPixelsPerUnityMeter;
            Vector2 worldOffset = Quaternion.Inverse(_camera.transform.rotation) * (lightCamera.transform.position - _camera.transform.position);
            Vector2 offset = Vector2.Scale(lightTexelSize, -worldOffset * lightPixelsPerUnityMeter);

            RenderTexture lightSourcesTex = blurLightSources && lightSourcesBlurMaterial != null && lightTexturesFilterMode != FilterMode.Point
                                                ? _bluredLightTexture
                                                : _lightSourcesTexture;
            float xDiff = _camera.aspect / lightCamera.aspect;

            if(!_camera.orthographic) {
                float gameCamHalfFov = _camera.fieldOfView * Mathf.Deg2Rad / 2f;
                float gameCamSize = Mathf.Tan(gameCamHalfFov) * lightObstaclesDistance * 2;
                _camera.orthographicSize = gameCamSize / 2f;
            }

            float scaleY = _camera.orthographicSize / lightCamera.orthographicSize;
            Vector2 scale = new Vector2(scaleY * xDiff, scaleY);

            FilterMode oldAmbientFilterMode = _ambientTexture == null ? FilterMode.Point : _ambientTexture.filterMode;
            lightOverlayMaterial.SetTexture("_AmbientLightTex", enableAmbientLight ? _ambientTexture : null);
            lightOverlayMaterial.SetTexture("_LightSourcesTex", lightSourcesTex);
            lightOverlayMaterial.SetTexture("_GameTex", src);
            lightOverlayMaterial.SetVector("_Offset", offset);
            lightOverlayMaterial.SetVector("_Scale", scale);

            if(_screenBlitTempTex == null || _screenBlitTempTex.width != src.width ||
               _screenBlitTempTex.height != src.height) {
                if(_screenBlitTempTex != null)
                    _screenBlitTempTex.Release();
                _screenBlitTempTex = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.ARGB32) {
                    filterMode = FilterMode.Point
                };
            }

            _screenBlitTempTex.DiscardContents();
            Graphics.Blit(null, _screenBlitTempTex, lightOverlayMaterial);

            if(_ambientTexture != null)
                _ambientTexture.filterMode = oldAmbientFilterMode;

            Graphics.Blit(_screenBlitTempTex, dest);

            Profiler.EndSample();
        }

        private void UpdateCamera() {
            lightPixelSize = _camera.orthographicSize * 2f / _smallLightTextureSize.y;

            float lightPixelsPerUnityMeter = LightPixelsPerUnityMeter;
            Vector3 mainPos = _camera.transform.position;
            Quaternion camRot = _camera.transform.rotation;
            Vector3 unrotMainPos = Quaternion.Inverse(camRot) * mainPos;
            Vector2 gridPos = new Vector2(
                Mathf.Round(unrotMainPos.x * lightPixelsPerUnityMeter) / lightPixelsPerUnityMeter,
                Mathf.Round(unrotMainPos.y * lightPixelsPerUnityMeter) / lightPixelsPerUnityMeter);
            Vector2 posDiff = gridPos - (Vector2) unrotMainPos;
            Vector3 pos = camRot * posDiff + mainPos;
            lightCamera.transform.position = pos;
            _currPos = pos;
        }

        public void LoopAmbientLight(int cycles) {
            _aditionalAmbientLightCycles += cycles;
        }

        private void ConfigLightCamera(bool extended) {
            if(extended) {
                lightCamera.orthographicSize =
                    _camera.orthographicSize * (_extendedLightTextureSize.y / (float) _smallLightTextureSize.y); // _extendedLightTextureSize.y/(2f*LightPixelsPerUnityMeter);
                lightCamera.fieldOfView = _camera.fieldOfView + lightCameraFovAdd;
                lightCamera.aspect = _extendedLightTextureSize.x / (float) _extendedLightTextureSize.y;
            } else {
                lightCamera.orthographicSize = _camera.orthographicSize; // _smallLightTextureSize.y / (2f * LightPixelsPerUnityMeter);
                lightCamera.fieldOfView = _camera.fieldOfView;
                lightCamera.aspect = _smallLightTextureSize.x / (float) _smallLightTextureSize.y;
            }
        }
    }
}