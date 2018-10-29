using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Light2D {
    /// <inheritdoc />
    /// <summary>
    ///     Custom sprite wich uses MeshFilter and MeshRenderer to render.
    ///     Main improvement from Unity SpriteRenderer is that you can access and modify mesh.
    ///     Also multiple CustomSprites could be merged to single mesh with MeshCombiner,
    ///     which gives much better performance for small meshes than StaticBatchingUtility.Combine.
    /// </summary>
    [ExecuteInEditMode]
    public class CustomSprite : MonoBehaviour {
        private const string GeneratedMaterialName = "Generated Material (DONT change it)";
        private const string GeneratedMeshName = "Generated Mesh (DONT change it)";
        public static Dictionary<MaterialKey, MaterialValue> materialMap = new Dictionary<MaterialKey, MaterialValue>();
        private Color _oldColor;
        private Material _oldMaterial;
        private MaterialKey _oldMaterialKey;
        private Sprite _oldSprite;

        /// <summary> Vertex color of mesh. </summary>
        public Color color = Color.white;

        // mesh data
        protected Color[] colors;

        protected bool isMeshDirty;

        /// <summary> Material to be used. </summary>
        public Material material;

        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;

        /// <summary> Sorting order of MeshRenderer. </summary>
        public int sortingOrder;

        /// <summary> Sprite from which mesh will be generated. </summary>
        public Sprite sprite;

        protected Vector4[] tangents;
        protected int[] triangles;
        protected Vector2[] uv0;
        protected Vector2[] uv1;
        protected Vector3[] vertices;

        public bool RendererEnabled { get; private set; }

        /// <summary> Is that sprite is staticaly batched? </summary>
        public bool IsPartOfStaticBatch => meshRenderer != null && meshRenderer.isPartOfStaticBatch;

        protected virtual void OnEnable() {
            colors = new Color[4];
            uv1 = new Vector2[4];
            uv0 = new Vector2[4];
            vertices = new Vector3[4];
            triangles = new[] {2, 1, 0, 1, 2, 3};
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();

            if(meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if(meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if(meshRenderer.sharedMaterials.Length > 1)
                meshRenderer.sharedMaterials = new[] {meshRenderer.sharedMaterials[0]};

#if UNITY_EDITOR
            if(material == null) material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
#else
            if(material == null) material = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
#endif

            TryReleaseMesh();
            meshFilter.sharedMesh = mesh = new Mesh();
            mesh.MarkDynamic();
            mesh.name = GeneratedMeshName;

            tangents = new Vector4[4];
            for(int i = 0; i < tangents.Length; i++)
                tangents[i] = new Vector4(1, 0, 0);

            UpdateMeshData(true);

            RendererEnabled = meshRenderer.enabled;
#if UNITY_EDITOR
            if(!Application.isPlaying) {
                DestroyImmediate(meshRenderer);
                DestroyImmediate(meshFilter);
            }
#endif
        }

        protected virtual void Start() {
            UpdateMeshData(true);
        }

        private void OnWillRenderObject() {
            UpdateMeshData();
            if(Application.isPlaying && LightingSystem.Instance.enableNormalMapping && material.shader == Shader.Find("Transparent Normal Mapped")) {
                RendererEnabled = meshRenderer.enabled;
                meshRenderer.enabled = false;
            }
        }

        private void OnRenderObject() {
            if(Application.isPlaying && LightingSystem.Instance.enableNormalMapping) meshRenderer.enabled = RendererEnabled;
        }

        /// <summary> Getting material from cache or instantiating new one. </summary>
        /// <returns>The material</returns>
        public Material GetOrCreateMaterial() {
            TryReleaseMaterial();

            if(material == null || sprite == null)
                return null;

            MaterialValue matValue;
            MaterialKey key = new MaterialKey(material, sprite.texture);

            if(!materialMap.TryGetValue(key, out matValue)) {
                Material mat = Instantiate(material);
                mat.name = GeneratedMaterialName;
                mat.mainTexture = sprite.texture;
                materialMap[key] = matValue = new MaterialValue(mat, 1);
            } else {
                matValue.usageCount++;
            }

            _oldMaterialKey = key;

            return matValue.material;
        }

        /// <summary> Getting material from cache or instantiating new one. </summary>
        /// <returns>The material</returns>
        public static Material GetOrCreateMaterial(Material baseMaterial, Texture2D texture, out MaterialKey materialKey) {
            if(baseMaterial == null || texture == null) {
                materialKey = null;
                return null;
            }

            MaterialValue matValue;
            MaterialKey key = materialKey = new MaterialKey(baseMaterial, texture);

            if(!materialMap.TryGetValue(key, out matValue)) {
                Material mat = Instantiate(baseMaterial);
                mat.name = GeneratedMaterialName;
                mat.mainTexture = texture;
                materialMap[key] = matValue = new MaterialValue(mat, 1);
            } else {
                matValue.usageCount++;
            }

            return matValue.material;
        }

        /// <summary> Deleting material from cache with reference counting. </summary>
        /// <param name="key"></param>
        public static void ReleaseMaterial(MaterialKey key) {
            MaterialValue matValue;

            if(!materialMap.TryGetValue(key, out matValue))
                return;

            matValue.usageCount--;

            if(matValue.usageCount <= 0) {
                Util.Destroy(matValue.material);
                materialMap.Remove(key);
            }
        }

        private void TryReleaseMesh() {
            if(meshFilter != null && meshFilter.sharedMesh != null &&
               meshFilter.sharedMesh.name == GeneratedMeshName && mesh == meshFilter.sharedMesh) {
                Util.Destroy(meshFilter.sharedMesh);
                meshFilter.sharedMesh = null;
            }
        }

        private void TryReleaseMaterial() {
            if(_oldMaterialKey != default(MaterialKey)) {
                ReleaseMaterial(_oldMaterialKey);
                _oldMaterialKey = default(MaterialKey);
            }
        }

        private void OnDestroy() {
            TryReleaseMesh();
            TryReleaseMaterial();
        }

        protected virtual void UpdateColor() {
            for(int i = 0; i < colors.Length; i++)
                colors[i] = color;
        }

        /// <summary>
        ///     Recreating mesh data for Sprite based on it's bounds.
        /// </summary>
        protected virtual void UpdateSprite() {
            if(sprite == null)
                return;

            Rect rect = sprite.textureRect;
            Bounds bounds = sprite.bounds;
            Texture2D tex = sprite.texture;
            Point2 textureSize = new Point2(tex.width, tex.height);

            // HACK: mipmap could cause texture padding sometimes so padded size of texture needs to be computed.
            Point2 realSize =
#if UNITY_EDITOR || UNITY_STANDALONE
                tex.mipmapCount <= 1
#else
                true
#endif
                    ? textureSize
                    : new Point2(Mathf.NextPowerOfTwo(textureSize.x), Mathf.NextPowerOfTwo(textureSize.y));

            Vector2 unitSize2 = rect.size / sprite.pixelsPerUnit / 2f;
            Vector2 offest = bounds.center;

            vertices[0] = new Vector3(-unitSize2.x + offest.x, -unitSize2.y + offest.y, 0);
            vertices[1] = new Vector3(unitSize2.x + offest.x, -unitSize2.y + offest.y, 0);
            vertices[2] = new Vector3(-unitSize2.x + offest.x, unitSize2.y + offest.y, 0);
            vertices[3] = new Vector3(unitSize2.x + offest.x, unitSize2.y + offest.y, 0);

            uv0[0] = new Vector2(rect.xMin / realSize.x, rect.yMin / realSize.y); // 0, 0
            uv0[1] = new Vector2(rect.xMax / realSize.x, rect.yMin / realSize.y); // 1, 0
            uv0[2] = new Vector2(rect.xMin / realSize.x, rect.yMax / realSize.y); // 0, 1
            uv0[3] = new Vector2(rect.xMax / realSize.x, rect.yMax / realSize.y); // 1, 1

            for(int i = 0; i < 4; i++) colors[i] = color;

            meshRenderer.sharedMaterial = GetOrCreateMaterial();
        }

        /// <summary>
        ///     Clearing and rebuilding mesh.
        /// </summary>
        protected virtual void UpdateMesh() {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv0;
            mesh.uv2 = uv1;
            mesh.colors = colors;
            mesh.tangents = tangents;
        }

        /// <summary>
        ///     Checking public fields and mesh data, then rebuilding internal state if changes found.
        /// </summary>
        /// <param name="forceUpdate">Force update even if no changes found.</param>
        protected virtual void UpdateMeshData(bool forceUpdate = false) {
            if(meshRenderer == null || meshFilter == null || IsPartOfStaticBatch)
                return;

            meshRenderer.sortingOrder = sortingOrder;

            if(color != _oldColor || forceUpdate) {
                UpdateColor();
                isMeshDirty = true;
                _oldColor = color;
            }
            if(sprite != _oldSprite || material != _oldMaterial || forceUpdate) {
                UpdateSprite();
                isMeshDirty = true;
                _oldSprite = sprite;
                _oldMaterial = material;
            }
            if(isMeshDirty) {
                UpdateMesh();
                isMeshDirty = false;
            }
        }

        /// <summary>
        ///     Used as a value to material map to support reference counting.
        /// </summary>
        public class MaterialValue {
            /// <summary>
            ///     Instantiated material from MaterialKey.Material with texture from MaterialKey.Texture.
            /// </summary>
            public Material material;

            /// <summary>
            ///     Count of CustomSprites using that material.
            /// </summary>
            public int usageCount;

            public MaterialValue(Material material, int usageCount) {
                this.material = material;
                this.usageCount = usageCount;
            }
        }

        /// <summary>
        ///     Used as a key to material map.
        /// </summary>
        public class MaterialKey : IEquatable<MaterialKey> {
            /// <summary>
            ///     Non instantiated material.
            /// </summary>
            public Material material;

            /// <summary>
            ///     Sprite's texture.
            /// </summary>
            public Texture2D texture;

            public MaterialKey(Material material, Texture2D texture) {
                this.material = material;
                this.texture = texture;
            }

            public static IEqualityComparer<MaterialKey> TextureMaterialComparer { get; } = new TextureMaterialEqualityComparer();

            public bool Equals(MaterialKey other) {
                if(ReferenceEquals(null, other)) return false;
                if(ReferenceEquals(this, other)) return true;
                return Equals(texture, other.texture) && Equals(material, other.material);
            }

            public override bool Equals(object obj) {
                if(ReferenceEquals(null, obj)) return false;
                if(ReferenceEquals(this, obj)) return true;
                if(obj.GetType() != GetType()) return false;
                return Equals((MaterialKey) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((texture != null ? texture.GetHashCode() : 0) * 397) ^ (material != null ? material.GetHashCode() : 0);
                }
            }

            public static bool operator ==(MaterialKey left, MaterialKey right) {
                return Equals(left, right);
            }

            public static bool operator !=(MaterialKey left, MaterialKey right) {
                return !Equals(left, right);
            }

            private sealed class TextureMaterialEqualityComparer : IEqualityComparer<MaterialKey> {
                public bool Equals(MaterialKey x, MaterialKey y) {
                    if(ReferenceEquals(x, y)) return true;
                    if(ReferenceEquals(x, null)) return false;
                    if(ReferenceEquals(y, null)) return false;
                    if(x.GetType() != y.GetType()) return false;
                    return Equals(x.texture, y.texture) && Equals(x.material, y.material);
                }

                public int GetHashCode(MaterialKey obj) {
                    unchecked {
                        return ((obj.texture != null ? obj.texture.GetHashCode() : 0) * 397) ^ (obj.material != null ? obj.material.GetHashCode() : 0);
                    }
                }
            }
        }
    }
}