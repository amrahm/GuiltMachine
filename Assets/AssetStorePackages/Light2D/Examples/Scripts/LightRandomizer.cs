using UnityEngine;

namespace Light2D.Examples {
    [RequireComponent(typeof(LightSprite))]
    public class LightRandomizer : MonoBehaviour {
        public float maxLightAlpha = 0.8f;
        public float maxRadius = 35;
        public float minLightAlpha = 0.3f;
        public float minRadius = 5;

        private void Start() {
            LightSprite rend = GetComponent<LightSprite>();

            rend.transform.localScale = Vector3.one * Random.Range(minRadius, maxRadius);
            Vector4 c = new Vector4(Random.value, Random.value, Random.value, Random.Range(minLightAlpha, maxLightAlpha));
            float maxc = Mathf.Max(c.x, c.y, c.z);
            rend.color = new Color(c.x / maxc, c.y / maxc, c.z / maxc, c.w);
        }
    }
}