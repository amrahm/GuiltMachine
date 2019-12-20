﻿using UnityEngine;

namespace Light2D {
    /// <inheritdoc />
    /// <summary>
    ///     Automatically updating mesh, material and main texture of light obstacle.
    ///     Class is copying all data used for rendering from parent.
    /// </summary>
    public class LightObstacleMesh : MonoBehaviour {
        private CustomSprite.MaterialKey _materialKey;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Color _oldAddColor;
        private Material _oldMaterial;
        private Color32 _oldMulColor;
        private Mesh _oldParentMesh;
        private MeshFilter _parentMeshFilter;
        private MeshRenderer _parentMeshRenderer;
        public Color additiveColor;
        public Material material;
        public Color32 multiplicativeColor;

        private void Awake() {
            _parentMeshRenderer = transform.parent.GetComponent<MeshRenderer>();
            _parentMeshFilter = transform.parent.GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if(_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            if(_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        private void Update() {
            Refresh();
        }

        private void Refresh() {
            //TODO optimize this?
            if(_parentMeshFilter == null || _parentMeshFilter == null || _meshRenderer == null || _meshFilter == null ||
               _parentMeshFilter.sharedMesh == null || _parentMeshRenderer.sharedMaterial == null) {
                if(_meshRenderer != null)
                    _meshRenderer.enabled = false;
                return;
            }

            bool dirty = false;
            if(_parentMeshFilter.mesh != _oldParentMesh) {
                if(_meshFilter.mesh != null)
                    Destroy(_meshFilter.mesh);
                _meshFilter.mesh = Instantiate(_parentMeshFilter.sharedMesh);
                _meshFilter.mesh.MarkDynamic();

                if(_meshFilter.mesh.tangents == null) {
                    Vector4[] tangents = new Vector4[_meshFilter.mesh.vertexCount];
                    for(int i = 0; i < tangents.Length; i++)
                        tangents[i] = new Vector4(1, 0);
                    _meshFilter.mesh.tangents = tangents;
                }

                _oldParentMesh = _parentMeshFilter.sharedMesh;
                dirty = true;
            }

            if(_oldMaterial != _parentMeshRenderer.sharedMaterial ||
               _oldMaterial != null && _parentMeshRenderer.sharedMaterial != null &&
               _oldMaterial.mainTexture != _parentMeshRenderer.sharedMaterial.mainTexture) {
                if(_meshRenderer.sharedMaterial != null && _materialKey != null) CustomSprite.ReleaseMaterial(_materialKey);
                Material baseMat = material == null ? _parentMeshRenderer.sharedMaterial : material;
                Texture2D tex = _parentMeshRenderer.sharedMaterial.mainTexture as Texture2D;
                _meshRenderer.sharedMaterial = CustomSprite.GetOrCreateMaterial(baseMat, tex, out _materialKey);
                _oldMaterial = _parentMeshRenderer.sharedMaterial;
            }

            if(!multiplicativeColor.Equals(_oldMulColor) || additiveColor != _oldAddColor || dirty) {
                Color32[] colors = _meshFilter.mesh.colors32;
                if(colors == null || colors.Length != _meshFilter.mesh.vertexCount)
                    colors = new Color32[_meshFilter.mesh.vertexCount];

                for(int i = 0; i < colors.Length; i++)
                    colors[i] = multiplicativeColor;
                _meshFilter.mesh.colors32 = colors;

                Vector2 uv1 = new Vector2(
                    Util.DecodeFloatRgba((Vector4) additiveColor),
                    Util.DecodeFloatRgba(new Vector4(additiveColor.a, 0, 0)));
                Vector2[] uv1Arr = _meshFilter.mesh.uv2;
                if(uv1Arr == null || uv1Arr.Length != colors.Length)
                    uv1Arr = new Vector2[colors.Length];
                for(int i = 0; i < uv1Arr.Length; i++) uv1Arr[i] = uv1;
                _meshFilter.mesh.uv2 = uv1Arr;

                _oldMulColor = multiplicativeColor;
                _oldAddColor = additiveColor;
            }
        }
    }
}