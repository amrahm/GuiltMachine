using UnityEngine;
using static WeaponScript;

[CreateAssetMenu(menuName = "ScriptableObjects/Attacks/BasicMeleeAttack")]
public class BasicMeleeAttack : WeaponAttackAbstract {
    public string animationTrigger;

    private int _animationHash;

    public override void Initialize(WeaponScript weaponScript) {
        weapon = weaponScript;
        _animationHash = Animator.StringToHash(animationTrigger);
    }

    public override void OnAttackWindup(AttackAction attackAction) {
#if UNITY_EDITOR || DEBUG
        Debug.Assert(weapon.anim != null, "weapon.anim != null");
#endif
        weapon.anim.SetTrigger(_animationHash);
    }

    public override void OnAttacking(AttackAction attackAction) {
        weapon.StartCoroutine(weapon._CheckMeleeHit(attackAction));
    }

    public override void OnRecovering(AttackAction attackAction) { }

    public override void OnFadingOut(AttackAction attackAction) { }
}