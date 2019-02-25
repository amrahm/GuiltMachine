using UnityEngine;

[CreateAssetMenu(menuName="ScriptableObjects/CharacterStats")]
public class CharacterStats : ScriptableObject {
    public delegate void HealthAction(int current, int max);
    public event HealthAction HealthChanged;
    
    public delegate void GuiltAction(int current, int max);
    public event GuiltAction GuiltChanged;

    public int maxHealth = 100;

    private int _curHealth;

    public int CurHealth
    {
        get => _curHealth;
        set => _curHealth = (int)Mathf.Clamp(value, 0f, maxHealth);
    }

    public int maxGuilt = 100;

    private int _curGuilt;
    public int CurGuilt
    {
        get => _curGuilt;
        set => _curGuilt = (int)Mathf.Clamp(value, 0f, maxGuilt);
    }


    public void Initialize()
    {
        CurHealth = maxHealth;
        CurGuilt = maxGuilt/2;
        HealthChanged?.Invoke(CurHealth, maxHealth);
        GuiltChanged?.Invoke(CurGuilt, maxGuilt);
    }

    public void DamagePlayer(int damage)
    {
        CurHealth -= damage;
        HealthChanged?.Invoke(CurHealth, maxHealth);
    }

    public void HealPlayer(int healing)
    {
        CurHealth += healing;
        HealthChanged?.Invoke(CurHealth, maxHealth);
    }

    public void IncreaseGuilt(int guilt)
    {
        CurGuilt += guilt;
        GuiltChanged?.Invoke(CurGuilt, maxGuilt);
    }

    public void DecreaseGuilt(int guilt)
    {
        CurGuilt -= guilt;
        GuiltChanged?.Invoke(CurGuilt, maxGuilt);
    }
}
