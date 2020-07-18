using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Attacks/BasicShootAttack")]
public class BasicShootAttack : AttackSubcomponentAbstract {
    public string animationTrigger;

    private int _animationHash;

    public override void Initialize(WeaponScript weaponScript) {
        weapon = weaponScript;
        _animationHash = Animator.StringToHash(animationTrigger);
    }

    public override void OnAttackWindup(WeaponScript.AttackAction attackAction) {
#if UNITY_EDITOR || DEBUG
        Debug.Assert(weapon.anim != null, "weapon.anim != null");
#endif
        weapon.anim.SetTrigger(_animationHash);
    }

    public override void OnAttacking(WeaponScript.AttackAction attackAction) {
//        Instantiate();
        RaycastHit2D shot = Physics2D.Raycast(weapon.weaponTip.transform.position, attackAction.attackDir, 1000f,
                                              weapon.whatIsHittable);
        IDamageable damageable;
        if(shot && (damageable = shot.transform.GetComponent<IDamageable>()) != null) {
            //TODO check protected
            damageable.DamageMe(shot.point, attackAction.attackDir * attackAction.attackDefinition.knockback,
                                attackAction.attackDefinition.damage, shot.collider);
            Debug.Log(shot.collider.name);
        }
        attackAction.BeginRecovering();
    }

    public override void OnRecovering(WeaponScript.AttackAction attackAction) { }
    public override void OnEnding(WeaponScript.AttackAction attackAction) { }
}