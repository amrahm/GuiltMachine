using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName="ScriptableObjects/CharacterStats")]
public class CharacterStats : ScriptableObject {
    public int maxHealth = 100;

    private int _curHealth;
    public int CurHealth
    {
        get { return _curHealth; }
        set { _curHealth = (int)Mathf.Clamp(value, 0f, maxHealth); }
    }

    public int maxGuilt = 100;

    private int _curGuilt;
    public int CurGuilt
    {
        get { return _curGuilt; }
        set { _curGuilt = (int)Mathf.Clamp(value, 0f, maxGuilt);  }
    }


    public void Initialize(PlayerStatusIndicator playerStatusIndicator)
    {
        if(playerStatusIndicator != null) {
            playerStatusIndicator.SetHealth(CurHealth, maxHealth);//TODO Events
            playerStatusIndicator.SetGuilt(CurGuilt, maxGuilt);
        } else {
            Debug.Log("No player status indicator linked to player script.");
        }
        CurHealth = maxHealth;
        CurGuilt = maxGuilt/2;
    }

    public void DamagePlayer(int damage)
    {
        // Player is invincible
//        if (godMode)
//        {
//            return;
//        }
        CurHealth -= damage;
        if (CurHealth <= 0)
        {
//            GameMaster.KillPlayer(this);
        }

//        _playerStatusIndicator?.SetHealth(CurHealth, maxHealth); //TODO Events

    }

    public void HealPlayer(int healing)
    {
        CurHealth += healing;
//        _playerStatusIndicator?.SetHealth(CurHealth, maxHealth);
    }

    public void IncreaseGuilt(int guilt)
    {
        CurGuilt += guilt;
//        _playerStatusIndicator?.SetGuilt(CurGuilt, maxGuilt);
    }

    public void DecreaseGuilt(int guilt)
    {
        CurGuilt -= guilt;
//        _playerStatusIndicator?.SetGuilt(CurGuilt, maxGuilt);
    }
}
