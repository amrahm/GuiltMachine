using UnityEngine;

public interface IDamageable {
    /// <summary> Damage this object </summary>
    void DamageMe(Vector2 point, Vector2 direction, float force, int damage);
}