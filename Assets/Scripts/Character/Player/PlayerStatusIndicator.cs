using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusIndicator : StatusIndicator {
    [SerializeField]
    private RectTransform guiltBarRect;

    [SerializeField]
    private Text guiltText;

    protected override void Start() {
        base.Start();

        if(guiltBarRect == null) {
            Debug.LogError("STATUS INDICATOR: No guilt bar object referenced!");
        }

        if(guiltText == null) {
            Debug.LogError("STATUS INDICATOR: No guilt text object referenced!");
        }
    }

    public override void SubscribeToChangeEvents(CharacterStats characterStats) {
        characterStats.HealthChanged -= SetHealth; //Prevent adding it twice
        characterStats.HealthChanged += SetHealth;
        characterStats.GuiltChanged -= SetGuilt; //Prevent adding it twice
        characterStats.GuiltChanged += SetGuilt;
    }

    public override void UnsubscribeToChangeEvents(CharacterStats characterStats) {
        characterStats.HealthChanged -= SetHealth;
        characterStats.GuiltChanged -= SetGuilt; //Prevent adding it twice
    }

    private void SetGuilt(int cur, int max) {
        float value = (float) cur / max;

        guiltBarRect.localScale = new Vector3(value, guiltBarRect.localScale.y, guiltBarRect.localScale.z);
        guiltText.text = cur + "/" + max + " Guilt";
    }
}