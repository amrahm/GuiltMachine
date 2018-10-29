using UnityEditor;
using UnityEngine;

namespace Light2D {
    public class LightingSystemCreationWindow : EditorWindow {
        private int _ambientLightLayer;
        private int _lightObstaclesLayer;
        private int _lightSourcesLayer;

        public static void CreateWindow() {
            LightingSystemCreationWindow window = GetWindow<LightingSystemCreationWindow>("Lighting system creation window");
            window.position = new Rect(200, 200, 500, 140);
        }

        private void OnGUI() {
            if(FindObjectOfType<LightingSystem>())
                GUILayout.Label("WARNING: existing lighting system is found.\nIt is recommended to remove it first, before adding new one.", EditorStyles.boldLabel);

            GUILayout.Label("Select layers you wish to use. You could modify them later in created object.");
            _lightObstaclesLayer = EditorGUILayout.LayerField("Light Obstacles", _lightObstaclesLayer);
            _lightSourcesLayer = EditorGUILayout.LayerField("Light Sources", _lightSourcesLayer);
            _ambientLightLayer = EditorGUILayout.LayerField("Ambient Light", _ambientLightLayer);

            if(GUILayout.Button("Create")) {
                Camera mainCamera = Camera.main;
                LightingSystem lighingSystem = mainCamera.GetComponent<LightingSystem>() ?? mainCamera.gameObject.AddComponent<LightingSystem>();

                GameObject prefab = Resources.Load<GameObject>("Lighting Camera");
                GameObject lightingSystemObj = Instantiate(prefab);
                lightingSystemObj.name = lightingSystemObj.name.Replace("(Clone)", "");
                lightingSystemObj.transform.parent = mainCamera.transform;
                lightingSystemObj.transform.localPosition = Vector3.zero;
                lightingSystemObj.transform.localScale = Vector3.one;
                lightingSystemObj.transform.localRotation = Quaternion.identity;

                LightingSystemPrefabConfig config = lightingSystemObj.GetComponent<LightingSystemPrefabConfig>();

                lighingSystem.lightCamera = lightingSystemObj.GetComponent<Camera>();
                lighingSystem.ambientLightComputeMaterial = config.ambientLightComputeMaterial;
                lighingSystem.lightOverlayMaterial = config.lightOverlayMaterial;
                lighingSystem.ambientLightBlurMaterial = lighingSystem.lightSourcesBlurMaterial = config.blurMaterial;

                DestroyImmediate(config);

                lighingSystem.lightCamera.depth = mainCamera.depth - 1;

                lighingSystem.lightCamera.cullingMask = 1 << _lightSourcesLayer;

                lighingSystem.lightSourcesLayer = _lightSourcesLayer;
                lighingSystem.ambientLightLayer = _ambientLightLayer;
                lighingSystem.lightObstaclesLayer = _lightObstaclesLayer;

                mainCamera.cullingMask &=
                    ~((1 << _lightSourcesLayer) | (1 << _ambientLightLayer) | (1 << _lightObstaclesLayer));

                Close();
            }
        }
    }
}