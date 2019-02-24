﻿using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PhoenixMaster : CharacterMasterAbstract {
    [FormerlySerializedAs("_deathExplosionPrefab")] [Tooltip("Explosion to play when phoenix dies")] [SerializeField]
    private Transform deathExplosionPrefab;

    /// <summary> prevents multiple damaging collisions to player </summary>
    private bool _exploded;
    /// <summary> helps reset _exploded </summary>
    private float _timeExploded;

    public override void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider) {
        if(!godMode) characterStats.DamagePlayer(damage);
        if(characterStats.CurHealth <= 0) StartCoroutine(Die());
        characterPhysics?.AddForceAt(point, force, hitCollider);
    }

    /// <summary> Makes the phoenix die lol what did you expect </summary>
    private IEnumerator Die() {
        Transform deathParticle = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
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
//        print("Destroyed " + gameObject.name + " from the scene.");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if(damageable != null) {
            if(Time.time - _timeExploded > 0.5f) _exploded = false;
            
            if(!_exploded) {
                Vector2 point = collision.GetContact(0).point;
                var relativeVelocity = collision.GetContact(0).relativeVelocity;
                damageable.DamageMe(point, -collision.GetContact(0).normal * collision.GetContact(0).normalImpulse, 34, collision.collider);

                // Enemy self-destructs and causes damage to player
                // Comment out line below if you do not want enemy to self-destruct
//                StartCoroutine(Die());

                _exploded = true;
                _timeExploded = Time.time;
            }
        }
    }
}