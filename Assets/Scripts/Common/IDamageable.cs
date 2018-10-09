using UnityEngine;

public interface IDamageable {
    /// <summary> Damage this object </summary>
    void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider);
}