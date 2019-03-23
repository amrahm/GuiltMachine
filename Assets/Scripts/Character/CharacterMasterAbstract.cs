using System;
using JetBrains.Annotations;
using UnityEngine;

public abstract class CharacterMasterAbstract : MonoBehaviour, IDamageable {
    [Tooltip("If God Mode on, character cannot take damage.")]
    public bool godMode;

    [Tooltip("Character Stats Asset (Shared)"), SerializeField]
    protected CharacterStats characterStats;

    [Tooltip("The weapon currently being held. Doesn't have to be assigned."), CanBeNull]
    public WeaponAbstract weapon;

    [Tooltip("Status indicator for this character. Doesn't have to be assigned."), SerializeField, CanBeNull]
    protected internal StatusIndicator statusIndicator;

    [SerializeField] private GameObject bloodEffect;


    /// <summary> Reference to this gameObject's rigidbody, if it exists </summary>
    [CanBeNull] protected Rigidbody2D rb;

    /// <summary> Reference to this gameObject's CharacterPhysics, if it exists </summary>
    [CanBeNull] protected internal CharacterPhysics characterPhysics;

    /// <summary> Reference to this gameObject's Control script </summary>
    [NonSerialized] public CharacterControlAbstract control;


    public bool CheckProtected(Vector2 point, Collider2D hitCollider) {
        if(weapon != null && weapon.Blocking) {
            //TODO more complicated checking to account for blocking direction.
            return true;
        }
        return false;
    }

    public virtual void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider) {
        if(hitCollider.isTrigger) return;

        characterStats.DamagePlayer(damage);
        characterPhysics?.AddForceAt(point, force, hitCollider);

        //TODO Object pool
        if(bloodEffect) {
            var blood = Instantiate(bloodEffect, transform);
            blood.transform.position = point;
            blood.transform.right = force;
        }

        if(!godMode && characterStats.CurHealth <= 0) Die();
    }

    /// <summary> Makes the character die lol what did you expect </summary>
    protected abstract void Die();

    protected virtual void Awake() {
        //References
        rb = GetComponent<Rigidbody2D>();
        characterPhysics = GetComponent<CharacterPhysics>();
        control = GetComponent<CharacterControlAbstract>();
        characterStats = Instantiate(characterStats); //create local instance that can be modified

        statusIndicator?.SubscribeToChangeEvents(characterStats);
        characterStats.Initialize();

        if(weapon != null) weapon.OnEquip(this);
    }

    private void OnEnable() {
        statusIndicator?.SubscribeToChangeEvents(characterStats);
    }

    private void OnDisable() {
        statusIndicator?.UnsubscribeToChangeEvents(characterStats);
    }
}