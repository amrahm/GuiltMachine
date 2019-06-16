using ExtensionMethods;
using System.Collections;
using System.Linq;
using UnityEngine;
using static WeaponScript;
using Debug = System.Diagnostics.Debug;

[CreateAssetMenu(menuName = "ScriptableObjects/Attacks/PuffchildDashAttack")]
public class PuffchildDashAttack : WeaponAttackAbstract {
    private const float ActivationEnergy = 0.3f;
    public float attackSpeed;
    public float attackLengthReducer;
    private Rigidbody2D _rb;
    private CharacterControlAbstract _ctrl;
    private float _chargeAmount;
    private Vector2 _dir;
    private PuffchildRepelCircleSpecialPart _repelCircleSpecialPart;

    /// <summary> initial gravity scale of this character </summary>
    private float _initGravity;

    public float chargeMinLength = 0.5f;
    public float chargeMaxLength = 0.8f;

    public override void Initialize(WeaponScript weaponScript) {
        weapon = weaponScript;
        _rb = weapon.holder.gameObject.GetComponent<Rigidbody2D>();
        _initGravity = _rb.gravityScale;
        _ctrl = weapon.holder.control;
        _repelCircleSpecialPart = (PuffchildRepelCircleSpecialPart)
            weapon.specialParts.First(part => part.GetType() == typeof(PuffchildRepelCircleSpecialPart));
    }

    public override void OnAttackWindup(AttackAction attackAction) {
        _dir = attackAction.attackDir.normalized;
        weapon.StartCoroutine(_ChargeAttack(attackAction));
    }

    private IEnumerator _ChargeAttack(AttackAction attackAction) {
        if(_repelCircleSpecialPart.energy < ActivationEnergy) {
            attackAction.EndAttack(dontFade: true);
            yield break;
        }

        while(_chargeAmount < chargeMinLength || //At least charge to min
              (attackAction.inH != 0 && _ctrl.attackHorizontal == attackAction.inH ||
               attackAction.inV != 0 && _ctrl.attackVertical == attackAction.inV) && _chargeAmount < chargeMaxLength) {
            _dir += 2 * chargeMaxLength * Time.fixedDeltaTime *
                    new Vector2(_ctrl.attackHorizontal, _ctrl.attackVertical);
            var right = _repelCircleSpecialPart.transform.right;
            float rotAmt = Vector2.SignedAngle(attackAction.inH >= 0 ? right : -right, _dir);
            _repelCircleSpecialPart.transform.Rotate(Vector3.forward, rotAmt * Time.fixedDeltaTime * 10);
            _rb.gravityScale = 0;
            _rb.velocity -= Time.fixedDeltaTime * 10 * _rb.velocity;
            _chargeAmount += Time.fixedDeltaTime;
            yield return Yields.WaitForFixedUpdate;
        }
        attackAction.state = AttackState.Attacking;
        attackAction.BeginAttacking();
    }

    public override void OnAttacking(AttackAction attackAction) {
        weapon.StartCoroutine(_Attack(attackAction));
    }

    private IEnumerator _Attack(AttackAction attackAction) {
        _chargeAmount = (_chargeAmount - chargeMinLength + 0.1f) * 4;
        if(!(_repelCircleSpecialPart.particleTrail is null)) {
            var emissionModule = _repelCircleSpecialPart.particleTrail.emission;
            emissionModule.rateOverDistance = 4;
        }
        while(_chargeAmount > 0 && _repelCircleSpecialPart.energy > 0) {
            _rb.gravityScale = 0;
            _rb.velocity = attackSpeed * _dir;
            _chargeAmount -= Time.fixedDeltaTime * attackLengthReducer;
            _repelCircleSpecialPart.energy -= Time.fixedDeltaTime * attackLengthReducer;
            yield return Yields.WaitForFixedUpdate;
        }
        attackAction.state = AttackState.Recovering;
        attackAction.BeginRecovering();
    }

    public override void OnRecovering(AttackAction attackAction) {
        _rb.gravityScale = _initGravity;
        _chargeAmount = 0;
        weapon.StartCoroutine(_SlowDown());
        attackAction.EndAttack(dontFade: true);
    }

    private IEnumerator _SlowDown() {
        float start = Time.time;
        while(Time.time < start + .6f) {
            _rb.velocity -= Time.fixedDeltaTime * 3 * _rb.velocity.Projected(_dir);
            yield return Yields.WaitForFixedUpdate;
        }
    }

    public override void OnFadingOut(AttackAction attackAction) {
        if(!(_repelCircleSpecialPart.particleTrail is null)) weapon.StartCoroutine(_FadePuff());
    }

    private IEnumerator _FadePuff() {
        float start = Time.time;
        Debug.Assert(!(_repelCircleSpecialPart.particleTrail is null), "_repelCircleSpecialPart.particleTrail != null");
        var emissionModule = _repelCircleSpecialPart.particleTrail.emission;
        const float fadeTime = .3f;
        while(Time.time < start + fadeTime) {
            emissionModule.rateOverDistance = 4 * (start - Time.time + fadeTime) / fadeTime;
            yield return Yields.WaitForFixedUpdate;
        }
        emissionModule.rateOverDistance = 0;
    }
}