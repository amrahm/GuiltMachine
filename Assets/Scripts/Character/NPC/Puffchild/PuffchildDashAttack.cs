using ExtensionMethods;
using System.Collections;
using System.Linq;
using UnityEngine;
using static WeaponScript;

[CreateAssetMenu(menuName = "ScriptableObjects/Attacks/PuffchildDashAttack")]
public class PuffchildDashAttack : WeaponAttackAbstract {
    private const float ActivationEnergy = 0.3f;
    private const int TrailRateOverDistance = 6;

    public float attackSpeed;
    public float attackLengthReducer;
    public float chargeMinLength = 0.5f;
    public float chargeMaxLength = 0.8f;

    private Rigidbody2D _rb;
    private CharacterControlAbstract _ctrl;
    private float _chargeAmount;
    private Vector2 _dir;
    private PuffchildRepelCircleSpecialPart _repelCirclePart;
    private Coroutine _fadeOutCoroutine;

    /// <summary> initial gravity scale of this character </summary>
    private float _initGravity;

    private bool _isparticleTrailNotNull;

    private void Start() { _isparticleTrailNotNull = _repelCirclePart.particleTrail != null; }

    public override void Initialize(WeaponScript weaponScript) {
        weapon = weaponScript;
        _rb = weapon.holder.gameObject.GetComponent<Rigidbody2D>();
        _initGravity = _rb.gravityScale;
        _ctrl = weapon.holder.control;
        _repelCirclePart = (PuffchildRepelCircleSpecialPart)
            weapon.specialParts.First(part => part.GetType() == typeof(PuffchildRepelCircleSpecialPart));
    }

    public override void OnAttackWindup(AttackAction attackAction) {
        if(_fadeOutCoroutine != null) weapon.StopCoroutine(_fadeOutCoroutine);
        _dir = attackAction.attackDir.normalized;
        weapon.StartCoroutine(_ChargeAttack(attackAction));
    }

    private IEnumerator _ChargeAttack(AttackAction attackAction) {
        if(_repelCirclePart.energy < ActivationEnergy) {
            attackAction.EndAttack(dontFade: true);
            yield break;
        }

        while(_chargeAmount < chargeMinLength || //At least charge to min
              (attackAction.inH != 0 && _ctrl.attackHorizontal == attackAction.inH ||
               attackAction.inV != 0 && _ctrl.attackVertical == attackAction.inV) && _chargeAmount < chargeMaxLength) {
            _dir += 2 * chargeMaxLength * Time.fixedDeltaTime *
                    new Vector2(_ctrl.attackHorizontal, _ctrl.attackVertical);
            var right = _repelCirclePart.transform.right;
            float rotAmt = Vector2.SignedAngle(attackAction.inH >= 0 ? right : -right, _dir);
            _repelCirclePart.transform.Rotate(Vector3.forward, rotAmt * Time.fixedDeltaTime * 10);
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
        float chargeDistance = (_chargeAmount - chargeMinLength + 0.1f) * 2.5f;
        if(_isparticleTrailNotNull) {
            var emissionModule = _repelCirclePart.particleTrail.emission;
            emissionModule.rateOverDistance = TrailRateOverDistance;
        }
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        while(chargeDistance > 0 && _repelCirclePart.energy > 0 && !_repelCirclePart.touchingHittable) {
            _rb.gravityScale = 0;
            _rb.velocity = attackSpeed * _dir;
            chargeDistance -= Time.fixedDeltaTime * attackLengthReducer;
            _repelCirclePart.energy -= Time.fixedDeltaTime * attackLengthReducer;

            yield return Yields.WaitForFixedUpdate;
        }
        attackAction.state = AttackState.Recovering;
        attackAction.BeginRecovering();
    }

    public override void OnRecovering(AttackAction attackAction) {
        weapon.StartCoroutine(_SlowDown(attackAction));
    }

    private IEnumerator _SlowDown(AttackAction attackAction) {
        bool exploded = false;
        _rb.gravityScale = _initGravity;
        if(_repelCirclePart.touchingHittable) {
            if(_isparticleTrailNotNull) {
                var emissionModule = _repelCirclePart.particleTrail.emission;
                emissionModule.rateOverDistance = 0;
            }
            _repelCirclePart.StartCoroutine(_repelCirclePart._Explode(
                                                (int) (_chargeAmount * attackAction.attackDefinition.damage),
                                                _chargeAmount * attackAction.attackDefinition.knockback, _ctrl,
                                                weapon.holder.movement));
            exploded = true;
            weapon.Blocking = true;
        } else {
            attackAction.EndAttack();
        }

        _chargeAmount = 0;
        float start = Time.time;
        while(Time.time < start + .6f) {
            _rb.velocity -= Time.fixedDeltaTime * 3 * _rb.velocity.Projected(_dir);
            yield return Yields.WaitForFixedUpdate;
        }
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        if(exploded) {
            yield return new WaitWhile(() => _repelCirclePart.exploded);
            weapon.Blocking = false;
            attackAction.EndAttack(dontFade: true);
        }
    }

    public override void OnFadingOut(AttackAction attackAction) {
        if(_isparticleTrailNotNull) _fadeOutCoroutine = weapon.StartCoroutine(_FadePuff());
    }

    private IEnumerator _FadePuff() {
        float start = Time.time;
        var emissionModule = _repelCirclePart.particleTrail.emission;
        const float fadeTime = .3f;
        while(Time.time < start + fadeTime) {
            emissionModule.rateOverDistance = TrailRateOverDistance * (start - Time.time + fadeTime) / fadeTime;
            yield return Yields.WaitForFixedUpdate;
        }
        emissionModule.rateOverDistance = 0;
    }
}