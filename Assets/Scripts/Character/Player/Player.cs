using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [Tooltip("If God Mode on, player cannot take damage.")]
    public bool godMode = false;

    [System.Serializable]
	public class PlayerStats
    {
        public int maxHealth = 100;

        private int _curHealth;
        public int curHealth
        {
            get { return _curHealth; }
            set { _curHealth = (int)Mathf.Clamp(value, 0f, maxHealth); }
        }

        public int maxGuilt = 100;

        private int _curGuilt;
        public int curGuilt
        {
            get { return _curGuilt; }
            set { _curGuilt = (int)Mathf.Clamp(value, 0f, maxGuilt);  }
        }

        public void Init()
        {
            curHealth = maxHealth;
            curGuilt = maxGuilt/2;
        }
    }

    public PlayerStats playerStats = new PlayerStats();

    [SerializeField]
    private PlayerStatusIndicator playerStatusIndicator;

    void Start()
    {
        playerStats.Init();

        if (playerStatusIndicator != null)
        {
            playerStatusIndicator.SetHealth(playerStats.curHealth, playerStats.maxHealth);
            playerStatusIndicator.SetGuilt(playerStats.curGuilt, playerStats.maxGuilt);
        }
        else
        {
            Debug.Log("No player status indicator linked to player script.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            DamagePlayer(5);
        }
        if (Input.GetKeyDown("m"))
        {
            HealPlayer(7);
        }
        if (Input.GetKeyDown("v"))
        {
            IncreaseGuilt(5);
        }
        if (Input.GetKeyDown("b"))
        {
            DecreaseGuilt(7);
        }
    }

    public void DamagePlayer(int damage)
    {
        // Player is invincible
        if (godMode)
        {
            return;
        }
        playerStats.curHealth -= damage;
        if (playerStats.curHealth <= 0)
        {
            GameMaster.KillPlayer(this);
        }

        playerStatusIndicator.SetHealth(playerStats.curHealth, playerStats.maxHealth);

    }

    public void HealPlayer(int healing)
    {
        playerStats.curHealth += healing;
        playerStatusIndicator.SetHealth(playerStats.curHealth, playerStats.maxHealth);
    }

    public void IncreaseGuilt(int guilt)
    {
        playerStats.curGuilt += guilt;
        playerStatusIndicator.SetGuilt(playerStats.curGuilt, playerStats.maxGuilt);
    }

    public void DecreaseGuilt(int guilt)
    {
        playerStats.curGuilt -= guilt;
        playerStatusIndicator.SetGuilt(playerStats.curGuilt, playerStats.maxGuilt);
    }
}
