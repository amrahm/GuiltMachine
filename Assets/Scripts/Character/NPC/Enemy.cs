using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [System.Serializable]
    public class EnemyStats
    {
        public int MaxHealth = 100;
        public int Health = 100;

        public float HealthPercentage()
        {
            float result = Health < 0 ? 0 : (float)Health / MaxHealth;
            return result;
        }
    }

    public EnemyStats stats = new EnemyStats();

    public void DamageEnemy(int damage)
    {
        stats.Health -= damage;
        if (stats.Health <= 0)
        {
            GameMaster.KillEnemy(this);
        }
    }

    public void HealEnemy(int healing)
    {
        stats.Health = (int)Mathf.Min(stats.MaxHealth, stats.Health + healing);
    }
}
