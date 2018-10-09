using UnityEngine;

public abstract class CharacterMasterAbstract : MonoBehaviour, IDamageable {
    [Tooltip("Character Stats Asset (Shared)")] [SerializeField]
    protected CharacterStats characterStats;

    [Tooltip("Character Control Asset (Shared)")]
    public CharacterControlAbstract control;
    
    [Tooltip("The weapon currently being held")]
    public WeaponAbstract weapon;

    public abstract void DamageMe(Vector2 point, Vector2 force, int damage);
}
