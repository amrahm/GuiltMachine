using System.Collections;
using UnityEngine;

public class PhoenixMaster : CharacterMasterAbstract {
    [Tooltip("Explosion to play when phoenix dies")] [SerializeField]
    private Transform _deathExplosionPrefab;

    private bool _exploded;

    public override void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider) {
        if (!godMode) characterStats.DamagePlayer(damage);
        if(!hitCollider.isTrigger) rb?.AddForceAtPosition(force, point, ForceMode2D.Impulse);
        if (characterStats.CurHealth <= 0) StartCoroutine(Die());
        characterPhysics?.AddForceAt(point, force, hitCollider);
    }

    /// <summary> Makes the phoenix die lol what did you expect </summary>
    private IEnumerator Die() {
        Transform deathParticle = Instantiate(_deathExplosionPrefab, transform.position, Quaternion.identity);
        // Destroy particle system after 1 second
        Destroy(deathParticle.gameObject, 1f);

        // TODO: Shake the camera (need to implement CameraShake script)
        //camShake.Shake(camShakeAmt, camShakeLength);

        GetComponent<Rigidbody2D>().isKinematic = true;
        transform.localScale = Vector3.zero;

        // Audio to play on death
        AudioSource deathSound = GetComponent<AudioSource>();
        deathSound.Play();
        yield return new WaitForSeconds(deathSound.clip.length);

        Destroy(gameObject);
        Debug.Log("Destroyed " + gameObject.name + " from the scene.");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // exploded flag prevents multiple damaging collisions to player
            if (!_exploded)
            {
                Vector2 point = collision.GetContact(0).point;
                var relativeVelocity = collision.GetContact(0).relativeVelocity;
                damageable.DamageMe(point, -relativeVelocity*15, 34, collision.collider);
                
                // Enemy self-destructs and causes damage to player
                // Comment out line below if you do not want enemy to self-destruct
                StartCoroutine(Die());

                _exploded = true;
            }
        }
    }
}