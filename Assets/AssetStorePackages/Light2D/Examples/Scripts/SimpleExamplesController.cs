using UnityEngine;

namespace Light2D.Examples {
    public class SimpleExamplesController : MonoBehaviour {
        private int _currColorIndex;
        private int _currExampleIndex;
        public LightSprite[] coloredLights = new LightSprite[0];
        public GameObject[] examples = new GameObject[0];
        public Color[] lightColors = {Color.white};

        private void Start() {
            UpdateExample();
            UpdateColors();
        }

        private void Update() {
            if(Input.GetMouseButtonUp(0)) {
                _currExampleIndex++;
                if(_currExampleIndex >= examples.Length)
                    _currExampleIndex = 0;

                UpdateExample();
            }

            if(Input.GetMouseButtonUp(1)) {
                _currColorIndex++;
                if(_currColorIndex >= lightColors.Length)
                    _currColorIndex = 0;

                UpdateColors();
            }
        }

        private void UpdateColors() {
            Color color = lightColors.Length == 0 ? Color.white : lightColors[_currColorIndex];
            foreach(LightSprite sprite in coloredLights)
                sprite.color = color.WithAlpha(sprite.color.a);
        }

        private void UpdateExample() {
            for(int i = 0; i < examples.Length; i++)
                examples[i].SetActive(i == _currExampleIndex);
            LightingSystem.Instance.LoopAmbientLight(20);
        }
    }
}