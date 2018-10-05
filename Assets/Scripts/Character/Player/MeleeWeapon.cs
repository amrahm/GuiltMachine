using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour {
    public int damage = 17;
    [Tooltip("Knockback force applied by weapon.")]
    public float knockback = 500;
    [Tooltip("Delay in seconds until the weapon can deal damage again.")]
    public float attackDelay = 0.2f;
    [Tooltip("Forcemode: Force or Impulse")]
    public ForceMode2D forceMode = ForceMode2D.Force;

    // Tracks enemies that have been attacked to prevent multiple attacks
    // on same enemy within one weapon swing
    private HashSet<GameObject> enemies = new HashSet<GameObject>();
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !enemies.Contains(collision.gameObject))
        {

            enemies.Add(collision.gameObject);
            StartCoroutine(HitEnemy(collision));
        }
    }   

    IEnumerator HitEnemy(Collider2D collision)
    {
        // Handle damaging the enemy
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        enemy.DamageEnemy(damage);

        // Handle applying knockback to the enemy
        bool facingRight = this.transform.lossyScale.x > 0;
        Rigidbody2D enemyRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        Vector2 forceToApply = facingRight ? Vector2.right : Vector2.left;
        enemyRigidbody.AddForce(forceToApply * knockback, forceMode);
        yield return new WaitForSeconds(attackDelay);
        // Clear the enemies set to allow enemies to be attacked again
        enemies.Clear();
    }
}
