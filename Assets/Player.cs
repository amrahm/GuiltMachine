using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [System.Serializable]
	public class PlayerStats
    {
        public float health = 100f;
        public float maxHealth = 100f;
    }

    public PlayerStats playerStats = new PlayerStats();

    void DamagePlayer(float damage)
    {
        playerStats.health -= damage;
        if (playerStats.health <= 0)
        {
            GameMaster.KillPlayer(this);
        }
    }

    void HealPlayer(float healing)
    {
        playerStats.health = Mathf.Min(playerStats.maxHealth, playerStats.health + healing);
    }
}
