﻿#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Light2D {
    public static class Light2DMenu {
        [MenuItem("GameObject/Light2D/Lighting System", false, 6)]
        public static void CreateLightingSystem() {
            LightingSystemCreationWindow.CreateWindow();
        }

        [MenuItem("GameObject/Light2D/Light Obstacle", false, 6)]
        public static void CreateLightObstacle() {
            List<Renderer> baseObjects = Selection.gameObjects.Select(o => o.GetComponent<Renderer>()).Where(r => r != null).ToList();
            if(baseObjects.Count == 0)
                Debug.LogError("Can't create light obstacle from selected object. You need to select any object with renderer attached to it to create light obstacle.");

            foreach(Renderer gameObj in baseObjects) {
                string name = gameObj.name + " Light Obstacle";

                Transform child = gameObj.transform.Find(name);
                GameObject obstacleObj = child == null ? new GameObject(name) : child.gameObject;

                foreach(LightObstacleSprite obstacleSprite in obstacleObj.GetComponents<LightObstacleSprite>())
                    Util.Destroy(obstacleSprite);

                obstacleObj.transform.parent = gameObj.transform;
                obstacleObj.transform.localPosition = Vector3.zero;
                obstacleObj.transform.localRotation = Quaternion.identity;
                obstacleObj.transform.localScale = Vector3.one;

                obstacleObj.AddComponent<LightObstacleSprite>();
            }
        }

        [MenuItem("GameObject/Light2D/Light Source", false, 6)]
        public static void CreateLightSource() {
            GameObject obj = new GameObject("Light");
            if(LightingSystem.Instance != null)
                obj.layer = LightingSystem.Instance.lightSourcesLayer;
            LightSprite light = obj.AddComponent<LightSprite>();
            light.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/AssetStorePackages/Light2D/Materials/Light60Points.mat");
            light.sprite = Resources.Load<Sprite>("DefaultLight");
            light.color = new Color(1, 1, 1, 0.5f);
            Selection.activeObject = obj;
        }
    }
}

#endif