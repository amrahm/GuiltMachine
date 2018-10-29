using UnityEngine;

namespace Light2D.Examples {
    public class Flare : MonoBehaviour {
        private float _lifetimeElapsed;
        private Color _startColor;
        public float alphaGrowTime = 0.5f;
        public float lifetime;
        public LightSprite lightSprite;

        private void Start() {
            _startColor = lightSprite.color;
            lightSprite.color = _startColor.WithAlpha(0);
        }

        private void Update() {
            _lifetimeElapsed += Time.deltaTime;


            if(_lifetimeElapsed > lifetime) {
                _lifetimeElapsed = lifetime;
                Destroy(gameObject);
            }


            float alpha = Mathf.Lerp(0, _startColor.a, Mathf.Min(_lifetimeElapsed, alphaGrowTime) / alphaGrowTime);
            lightSprite.color = Color.Lerp(_startColor.WithAlpha(alpha), _startColor.WithAlpha(0), _lifetimeElapsed / lifetime);
        }
    }
}