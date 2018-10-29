using UnityEngine;

namespace Light2D {
    /// <inheritdoc />
    /// <summary>
    /// Some configuration for LightingSystem. Containd in lighting system prefab, destroyed after ininial setup.
    /// </summary>
    public class LightingSystemPrefabConfig : MonoBehaviour {
        public Material ambientLightComputeMaterial;
        public Material lightOverlayMaterial;
        public Material blurMaterial;
    }
}