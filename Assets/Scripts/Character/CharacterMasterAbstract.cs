using UnityEngine;

public abstract class CharacterMasterAbstract : MonoBehaviour, IDamageable {
    [Tooltip("Character Stats Asset (Shared)")] [SerializeField]
    protected CharacterStats characterStats;

    [Tooltip("Movement Asset (Shared)")]
    public MovementAbstract movement;

    [Tooltip("Character Control Asset (Shared)")]
    public CharacterControlAbstract control;
    

    public abstract void DamageMe(Vector2 point, Vector2 direction, float force, int damage);
}
