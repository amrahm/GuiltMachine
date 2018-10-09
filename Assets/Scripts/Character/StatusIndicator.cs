using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{

    [SerializeField]
    protected RectTransform healthBarRect;
    [SerializeField]
    protected Text healthText;

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
    }

    public virtual void SubscribeToChangeEvents(CharacterStats characterStats) {
            characterStats.HealthChanged -= SetHealth; //Prevent adding it twice
            characterStats.HealthChanged += SetHealth;
    }
    public virtual void UnsubscribeToChangeEvents(CharacterStats characterStats) {
        characterStats.HealthChanged -= SetHealth;
    }

    public void SetHealth(int cur, int max)
    {
        float value = (float)cur / max;

        healthBarRect.localScale = new Vector3(value, healthBarRect.localScale.y, healthBarRect.localScale.z);
        healthText.text = cur + "/" + max + " HP";
    }
}
