using System;
using UnityEngine;

public abstract class CharacterMasterAbstract : MonoBehaviour, IDamageable {
    [Tooltip("If God Mode on, character cannot take damage.")]
    public bool godMode;

    [Tooltip("Character Stats Asset (Shared)"), SerializeField]
    protected CharacterStats characterStats;

    [SerializeField] private string characterFaction = "I THIRST FOR A FACTION";
    [NonSerialized] public int faction;

    [Tooltip("The weapon currently being held. Doesn't have to be assigned.")]
    public WeaponScript weapon;

    [Tooltip("Status indicator for this character. Doesn't have to be assigned."), SerializeField]
    protected internal StatusIndicator statusIndicator;

    [SerializeField] private GameObject bloodEffect;


    /// <summary> Reference to this gameObject's rigidbody, if it exists </summary>
    protected Rigidbody2D rb;

    /// <summary> Reference to this gameObject's CharacterPhysics, if it exists </summary>
    protected internal CharacterPhysics characterPhysics;

    /// <summary> Reference to this gameObject's Control script </summary>
    [NonSerialized] public CharacterControlAbstract control;

    /// <summary> Reference to this gameObject's Movement script, if it exists </summary>
    protected internal MovementAbstract movement;

    /// <summary> Is the character currently unhittable </summary>
    protected internal bool dodging;


    public ProtectedType CheckProtected(Vector2 point, Collider2D hitCollider) {
        if(dodging) return ProtectedType.Dodging;
        if(weapon != null && weapon.Blocking) return ProtectedType.Blocking;
        return ProtectedType.Not;
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
        movement = GetComponent<MovementAbstract>();
        characterStats = Instantiate(characterStats); //create local instance that can be modified

        statusIndicator?.SubscribeToChangeEvents(characterStats);
        characterStats.Initialize();

        faction = characterFaction.GetHashCode();

        if(weapon != null) weapon.OnEquip(this);
    }

    private void OnEnable() {
        statusIndicator?.SubscribeToChangeEvents(characterStats);
    }

    private void OnDisable() {
        statusIndicator?.UnsubscribeToChangeEvents(characterStats);
    }
}