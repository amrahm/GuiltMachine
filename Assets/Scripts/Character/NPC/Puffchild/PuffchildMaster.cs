using System.Collections;
using UnityEngine;

public class PuffchildMaster : CharacterMasterAbstract {
    [Tooltip("Explosion to play when phoenix dies")] [SerializeField]
    private Transform deathExplosionPrefab;

    protected override void Die() { StartCoroutine(_DieHelper()); }

    private IEnumerator _DieHelper() {
        Transform deathParticle = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
        // Destroy particle system after 1 second
        Destroy(deathParticle.gameObject, 1f);

        // TODO: Shake the camera (need to implement CameraShake script)
        CameraShake.Shake(0.5f, 0.5f);

        GetComponent<Rigidbody2D>().isKinematic = true;
        foreach(Collider2D child in GetComponentsInChildren<Collider2D>()) child.isTrigger = true;
        transform.localScale = Vector3.zero;

        // Audio to play on death
        AudioSource deathSound = GetComponent<AudioSource>();
        deathSound.Play();
        yield return new WaitForSeconds(deathSound.clip.length);

        Destroy(gameObject);
    }
}