using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour {
    public int damage = 17;
    [Tooltip("Knockback force applied by weapon.")]
    public float knockback = 50;
    [Tooltip("Delay in seconds until the weapon can deal damage again.")]
    public float attackDelay = 0.2f;
    [Tooltip("Forcemode: Force or Impulse")]
    public ForceMode2D forceMode = ForceMode2D.Impulse;

    // Tracks enemies that have been attacked to prevent multiple attacks
    // on same enemy within one weapon swing
    private HashSet<GameObject> enemies = new HashSet<GameObject>();
    
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO remove stuff below
        if (other.gameObject.CompareTag("Enemy") && !enemies.Contains(other.gameObject))
        {

            enemies.Add(other.gameObject);
            StartCoroutine(HitEnemy(other));
        }

        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        if(damageable != null) {
            Vector2 point = other.Distance(GetComponent<Collider2D>()).pointB;
            var relativeVelocity = GetComponent<Rigidbody2D>().velocity;
            if(other.attachedRigidbody) relativeVelocity -= other.attachedRigidbody.velocity;

            damageable.DamageMe(point, relativeVelocity, knockback, damage);
        }
        //TODO Pass information to holder, which I guess maybe implements some interface? Maybe just pass "other" and have holder do all this math
    }   

    IEnumerator HitEnemy(Collider2D other)
    {
        // Handle damaging the enemy
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        enemy.DamageEnemy(damage);

        // Handle applying knockback to the enemy
        bool facingRight = transform.localScale.x > 0;
        Rigidbody2D enemyRigidbody = other.attachedRigidbody;
        Vector2 forceToApply = facingRight ? Vector2.right : Vector2.left;
        enemyRigidbody.AddForce(forceToApply * knockback, forceMode);
        yield return new WaitForSeconds(attackDelay);
        // Clear the enemies set to allow enemies to be attacked again
        enemies.Remove(other.gameObject);
    }
}
