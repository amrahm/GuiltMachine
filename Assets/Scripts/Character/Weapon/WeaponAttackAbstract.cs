using UnityEngine;
using static WeaponScript;

public abstract class WeaponAttackAbstract : ScriptableObject {
    protected WeaponScript weapon;

    public abstract void Initialize(WeaponScript weaponScript);
    public abstract void OnAttackWindup(AttackAction attackAction);
    public abstract void OnAttacking(AttackAction attackAction);
    public abstract void OnRecovering(AttackAction attackAction);
    public abstract void OnFadingOut(AttackAction attackAction);
}