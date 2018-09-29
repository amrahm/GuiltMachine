using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusIndicator : MonoBehaviour
{

    [SerializeField]
    private RectTransform healthBarRect;
    [SerializeField]
    private Text healthText;
    [SerializeField]
    private RectTransform guiltBarRect;
    [SerializeField]
    private Text guiltText;

    void Start()
    {
        if (healthBarRect == null)
        {
            Debug.LogError("STATUS INDICATOR: No health bar object referenced!");
        }

        if (healthText == null)
        {
            Debug.LogError("STATUS INDICATOR: No health text object referenced!");
        }
        if (guiltBarRect == null)
        {
            Debug.LogError("STATUS INDICATOR: No guilt bar object referenced!");
        }

        if (guiltText == null)
        {
            Debug.LogError("STATUS INDICATOR: No guilt text object referenced!");
        }

    }

    public void SetHealth(int _cur, int _max)
    {
        float _value = (float)_cur / _max;

        healthBarRect.localScale = new Vector3(_value, healthBarRect.localScale.y, healthBarRect.localScale.z);
        healthText.text = _cur + "/" + _max + " HP";
    }

    public void SetGuilt(int _cur, int _max)
    {
        float _value = (float)_cur / _max;

        guiltBarRect.localScale = new Vector3(_value, guiltBarRect.localScale.y, guiltBarRect.localScale.z);
        guiltText.text = _cur + "/" + _max + " Guilt";
    }
}
