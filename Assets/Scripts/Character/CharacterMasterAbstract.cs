using JetBrains.Annotations;
using UnityEngine;

public abstract class CharacterMasterAbstract : MonoBehaviour, IDamageable {
    [Tooltip("If God Mode on, character cannot take damage.")]
    public bool godMode;

    [Tooltip("Character Stats Asset (Shared)")] [SerializeField]
    protected CharacterStats characterStats;

    [Tooltip("Character Control Asset (Shared)")]
    public CharacterControlAbstract control;
    
    [Tooltip("The weapon currently being held")]
    public WeaponAbstract weapon;

    [Tooltip("Status indicator for this character. Doesn't have to be assigned.")]
    [SerializeField] [CanBeNull] protected StatusIndicator statusIndicator;


    public abstract void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider);

    protected virtual void Awake() {
        //References
        characterStats = Instantiate(characterStats); //create local instance that can be modified
        control = Instantiate(control); //create local instance that can be modified

        statusIndicator?.SubscribeToChangeEvents(characterStats);
        characterStats.Initialize();

        if(weapon != null) weapon.holder = this;
    }

    private void OnEnable() {
        statusIndicator?.SubscribeToChangeEvents(characterStats);
    }

    private void OnDisable() {
        statusIndicator?.UnsubscribeToChangeEvents(characterStats);
    }
}
