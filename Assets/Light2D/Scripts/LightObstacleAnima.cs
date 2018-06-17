using System;
using Anima2D;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Light2D {
    /// <summary> Sprite with dual color support. Grabs sprite from GameSpriteRenderer field. </summary>
    [ExecuteInEditMode]
    public class LightObstacleAnima : CustomSprite {
        private SpriteMeshInstance _oldGameSpriteRenderer;
        private Color _oldSecondaryColor;
        private SpriteMesh _oldUnitySprite;

        /// <summary> Color is packed in mesh UV1. </summary>
        public Color additiveColor;

        /// <summary> Renderer from which sprite will be used. </summary>
        public SpriteMeshInstance gameSpriteRenderer;

        protected override void OnEnable() {
#if UNITY_EDITOR
            if(material == null) material = (Material) AssetDatabase.LoadAssetAtPath("Assets/Light2D/Materials/DualColor.mat", typeof(Material));
#endif

            base.OnEnable();

            if(gameSpriteRenderer == null && transform.parent != null)
                gameSpriteRenderer = transform.parent.gameObject.GetComponent<SpriteMeshInstance>();

            gameObject.layer = LightingSystem.Instance.lightObstaclesLayer;

            UpdateMeshData(true);
        }

        private void UpdateSecondaryColor() {
            Vector2 uv1L = new Vector2(
                Util.DecodeFloatRGBA((Vector4) additiveColor),
                Util.DecodeFloatRGBA(new Vector4(additiveColor.a, 0, 0)));
            for(int i = 0; i < uv1.Length; i++) uv1[i] = uv1L;
        }

        protected override void UpdateMeshData(bool forceUpdate = false) {
            if(meshRenderer == null || meshFilter == null || IsPartOfStaticBatch) return;

            if(gameSpriteRenderer != null && (gameSpriteRenderer != _oldGameSpriteRenderer || forceUpdate ||
                                              _oldUnitySprite != null && _oldUnitySprite.sprite != null && _oldUnitySprite.sprite != sprite)) {
                _oldGameSpriteRenderer = gameSpriteRenderer;

                _oldUnitySprite = gameSpriteRenderer.spriteMesh;
                if(_oldUnitySprite != null) sprite = _oldUnitySprite.sprite;

                material.EnableKeyword("NORMAL_TEXCOORD");
            }

            if(_oldSecondaryColor != additiveColor || forceUpdate) {
                UpdateSecondaryColor();
                isMeshDirty = true;
                _oldSecondaryColor = additiveColor;
            }

            base.UpdateMeshData(forceUpdate);
        }
    }
}