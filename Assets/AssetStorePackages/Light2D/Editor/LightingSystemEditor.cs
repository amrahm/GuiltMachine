#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Light2D {
    [CustomEditor(typeof(LightingSystem))]
    public class LightingSystemEditor : Editor {
        private SerializedProperty _lightPixelSize;
        private SerializedProperty _lightCameraSizeAdd;
        private SerializedProperty _lightCameraFovAdd;
        private SerializedProperty _enableAmbientLight;
        private SerializedProperty _blurLightSources;
        private SerializedProperty _blurAmbientLight;
        private SerializedProperty _hdr;
        private SerializedProperty _lightObstaclesAntialiasing;
        private SerializedProperty _ambientLightComputeMaterial;
        private SerializedProperty _lightOverlayMaterial;
        private SerializedProperty _lightSourcesBlurMaterial;
        private SerializedProperty _ambientLightBlurMaterial;
        private SerializedProperty _lightCamera;
        private SerializedProperty _lightSourcesLayer;
        private SerializedProperty _ambientLightLayer;
        private SerializedProperty _lightObstaclesLayer;
        private SerializedProperty _lightObstaclesReplacementShaderLayer;
        private SerializedProperty _lightObstaclesDistance;
        private SerializedProperty _lightTexturesFilterMode;
        private SerializedProperty _enableNormalMapping;

        private void OnEnable() {
            _lightPixelSize = serializedObject.FindProperty(nameof(LightingSystem.lightPixelSize));
            _lightCameraSizeAdd = serializedObject.FindProperty(nameof(LightingSystem.lightCameraSizeAdd));
            _lightCameraFovAdd = serializedObject.FindProperty(nameof(LightingSystem.lightCameraFovAdd));
            _enableAmbientLight = serializedObject.FindProperty(nameof(LightingSystem.enableAmbientLight));
            _blurLightSources = serializedObject.FindProperty(nameof(LightingSystem.blurLightSources));
            _blurAmbientLight = serializedObject.FindProperty(nameof(LightingSystem.blurAmbientLight));
            _hdr = serializedObject.FindProperty(nameof(LightingSystem.hdr));
            _lightObstaclesAntialiasing = serializedObject.FindProperty(nameof(LightingSystem.lightObstaclesAntialiasing));
            _ambientLightComputeMaterial = serializedObject.FindProperty(nameof(LightingSystem.ambientLightComputeMaterial));
            _lightOverlayMaterial = serializedObject.FindProperty(nameof(LightingSystem.lightOverlayMaterial));
            _lightSourcesBlurMaterial = serializedObject.FindProperty(nameof(LightingSystem.lightSourcesBlurMaterial));
            _ambientLightBlurMaterial = serializedObject.FindProperty(nameof(LightingSystem.ambientLightBlurMaterial));
            _lightCamera = serializedObject.FindProperty(nameof(LightingSystem.lightCamera));
            _lightSourcesLayer = serializedObject.FindProperty(nameof(LightingSystem.lightSourcesLayer));
            _ambientLightLayer = serializedObject.FindProperty(nameof(LightingSystem.ambientLightLayer));
            _lightObstaclesLayer = serializedObject.FindProperty(nameof(LightingSystem.lightObstaclesLayer));
            _lightObstaclesDistance = serializedObject.FindProperty(nameof(LightingSystem.lightObstaclesDistance));
            _lightTexturesFilterMode = serializedObject.FindProperty(nameof(LightingSystem.lightTexturesFilterMode));
            _enableNormalMapping = serializedObject.FindProperty(nameof(LightingSystem.enableNormalMapping));
            _lightObstaclesReplacementShaderLayer =
                serializedObject.FindProperty(nameof(LightingSystem.lightObstaclesReplacementShaderLayer));
        }

        public override void OnInspectorGUI() {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            if(Application.isPlaying)
                GUI.enabled = false;

            LightingSystem lightingSystem = (LightingSystem) target;
            Camera cam = lightingSystem.GetComponent<Camera>();
            bool isMobileTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ||
                                  EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;

            if(cam == null) EditorGUILayout.LabelField("WARNING: No attached camera found.");

            EditorGUILayout.PropertyField(_lightPixelSize, new GUIContent("Light Pixel Size"));

            if(cam != null) {
                float size;
                if(cam.orthographic) {
                    size = (cam.orthographicSize + _lightCameraSizeAdd.floatValue) * 2f;
                } else {
                    float halfFov = (cam.fieldOfView + _lightCameraFovAdd.floatValue) * Mathf.Deg2Rad / 2f;
                    size = Mathf.Tan(halfFov) * _lightObstaclesDistance.floatValue * 2;
                }
                if(!Application.isPlaying) {
                    int lightTextureHeight = Mathf.RoundToInt(size / _lightPixelSize.floatValue);
                    int oldSize = lightTextureHeight;
                    lightTextureHeight = EditorGUILayout.IntField("Light Texture Height", lightTextureHeight);
                    if(lightTextureHeight % 2 != 0)
                        lightTextureHeight++;
                    if(lightTextureHeight < 16) {
                        if(lightTextureHeight < 8)
                            lightTextureHeight = 8;
                        EditorGUILayout.LabelField("WARNING: Light Texture Height is too small.");
                        EditorGUILayout.LabelField(" 50-200 (mobile) and 200-1000 (pc) is recommended.");
                    }
                    if(lightTextureHeight > (isMobileTarget ? 200 : 1000)) {
                        if(lightTextureHeight > 2048)
                            lightTextureHeight = 2048;
                        EditorGUILayout.LabelField("WARNING: Light Texture Height is too big.");
                        EditorGUILayout.LabelField(" 50-200 (mobile) and 200-1000 (pc) is recommended.");
                    }
                    if(oldSize != lightTextureHeight) _lightPixelSize.floatValue = size / lightTextureHeight;
                }
            }

            if(cam == null || cam.orthographic) {
                EditorGUILayout.PropertyField(_lightCameraSizeAdd, new GUIContent("Light Camera Size Add"));
            } else {
                EditorGUILayout.PropertyField(_lightCameraFovAdd, new GUIContent("Light Camera Fov Add"));
                EditorGUILayout.PropertyField(_lightObstaclesDistance, new GUIContent("Camera To Light Obstacles Distance"));
            }

            EditorGUILayout.PropertyField(_hdr, new GUIContent("64 Bit Color"));
            EditorGUILayout.PropertyField(_lightObstaclesAntialiasing, new GUIContent("Light Obstacles Antialiasing"));
            EditorGUILayout.PropertyField(_enableNormalMapping, new GUIContent("Normal Mapping"));
            if(_enableNormalMapping.boolValue && isMobileTarget)
                EditorGUILayout.LabelField("WARNING: Normal mapping is not supported on mobiles.");
            _lightTexturesFilterMode.enumValueIndex = (int) (FilterMode) EditorGUILayout.EnumPopup(
                "Texture Filtering", (FilterMode) _lightTexturesFilterMode.enumValueIndex);

            EditorGUILayout.PropertyField(_blurLightSources, new GUIContent("Blur Light Sources"));
            if(_blurLightSources.boolValue && _enableNormalMapping.boolValue) {
                EditorGUILayout.LabelField("    Blurring light sources with normal mapping enabled\n");
                EditorGUILayout.LabelField("     could significantly reduce lighting quality.");
            }

            bool normalGuiEnableState = GUI.enabled;
            if(!_blurLightSources.boolValue)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(_lightSourcesBlurMaterial, new GUIContent("   Light Sources Blur Material"));
            GUI.enabled = normalGuiEnableState;

            EditorGUILayout.PropertyField(_enableAmbientLight, new GUIContent("Enable Ambient Light"));
            if(!_enableAmbientLight.boolValue)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(_blurAmbientLight, new GUIContent("   Blur Ambient Light"));
            bool oldEnabled = GUI.enabled;
            if(!_blurAmbientLight.boolValue)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(_ambientLightBlurMaterial, new GUIContent("   Ambient Light Blur Material"));
            GUI.enabled = oldEnabled;
            EditorGUILayout.PropertyField(_ambientLightComputeMaterial, new GUIContent("   Ambient Light Compute Material"));
            GUI.enabled = normalGuiEnableState;

            EditorGUILayout.PropertyField(_lightOverlayMaterial, new GUIContent("Light Overlay Material"));
            EditorGUILayout.PropertyField(_lightCamera, new GUIContent("Lighting Camera"));
            _lightSourcesLayer.intValue =
                EditorGUILayout.LayerField(new GUIContent("Light Sources Layer"), _lightSourcesLayer.intValue);
            _lightObstaclesLayer.intValue =
                EditorGUILayout.LayerField(new GUIContent("Light Obstacles Layer"), _lightObstaclesLayer.intValue);
            _ambientLightLayer.intValue =
                EditorGUILayout.LayerField(new GUIContent("Ambient Light Layer"), _ambientLightLayer.intValue);
            EditorGUILayout.PropertyField(_lightObstaclesReplacementShaderLayer);

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif