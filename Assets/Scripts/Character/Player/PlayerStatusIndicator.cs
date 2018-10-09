using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusIndicator : StatusIndicator
{
    [SerializeField]
    private RectTransform _guiltBarRect;
    [SerializeField]
    private Text _guiltText;

    private void Start()
    {
        if (healthBarRect == null)
        {
            Debug.LogError("STATUS INDICATOR: No health bar object referenced!");
        }

        if (healthText == null)
        {
            Debug.LogError("STATUS INDICATOR: No health text object referenced!");
        }
        if (_guiltBarRect == null)
        {
            Debug.LogError("STATUS INDICATOR: No guilt bar object referenced!");
        }

        if (_guiltText == null)
        {
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

    private void SetGuilt(int cur, int max)
    {
        float value = (float)cur / max;

        _guiltBarRect.localScale = new Vector3(value, _guiltBarRect.localScale.y, _guiltBarRect.localScale.z);
        _guiltText.text = cur + "/" + max + " Guilt";
    }
}
