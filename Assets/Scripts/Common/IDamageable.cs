using UnityEngine;

public interface IDamageable {
    /// <summary> Check if this object is currently protected at this point (e.g. blocking) </summary>
    /// <param name="point"> The point hit</param>
    /// <param name="hitCollider"> The collider that was hit </param>
    bool CheckProtected(Vector2 point, Collider2D hitCollider);

    /// <summary> Damage this object </summary>
    /// <param name="point"> The point hit</param>
    /// <param name="force"> The direction and magntude of the incoming hit</param>
    /// <param name="damage"> How much damage the incoming hit should do </param>
    /// <param name="hitCollider"> The collider that was hit </param>
    void DamageMe(Vector2 point, Vector2 force, int damage, Collider2D hitCollider);
}