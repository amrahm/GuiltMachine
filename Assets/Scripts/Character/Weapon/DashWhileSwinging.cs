using System.Collections;
using ExtensionMethods;
using UnityEngine;
using static WeaponScript;

[CreateAssetMenu(menuName = "ScriptableObjects/Attacks/DashWhileSwinging")]
public class DashWhileSwinging : WeaponAttackAbstract {
    [Tooltip("If true, dash will stop if the character touches the ground"), SerializeField]
    private bool endWhenGrounded;

    [Tooltip("If true, the entire attack will stop too when the character touches the ground"), SerializeField,
     ConditionalHide(nameof(endWhenGrounded), true)]
    private bool alsoEndRestOfAttackWhenGrounded;

    [Tooltip("If true, dash direction is fixed, else dash is in direction of attack"), SerializeField]
    private bool hasFixedRelativeDirection;

    [Tooltip("Direction, relative to character, to dash"), SerializeField,
     ConditionalHide(nameof(hasFixedRelativeDirection), true)]
    private Vector2 fixedRelativeDirection;

    [Tooltip("Initial speed of the dash"), SerializeField]
    private float speed = 10;

    [Tooltip("How fast the dash gets faster over time"), SerializeField]
    private float acceleration = 20f;

    [Tooltip("How quickly to cancel velocity in directions other than the dash direction"), SerializeField]
    private float perpVelCancelSpeed = 1.1f;

    private Coroutine _attackDash;

    public override void Initialize(WeaponScript weaponScript) {
        weapon = weaponScript;
    }

    public override void OnAttackWindup(AttackAction attackAction) { }

    public override void OnAttacking(AttackAction attackAction) {
        _attackDash = weapon.StartCoroutine(_AttackDash(attackAction));
        if(endWhenGrounded) weapon.StartCoroutine(_GroundedEndCheck(attackAction));
    }

    public override void OnRecovering(AttackAction attackAction) { }
    public override void OnEnding(AttackAction attackAction) { }

    private IEnumerator _AttackDash(AttackAction attackAction) {
        //FIXME Not framerate independent? Seems to go crazy sometimes at low/unstable framerates
        Vector2 direction = hasFixedRelativeDirection ?
                                (Vector2) weapon.mvmt.tf.TransformDirection(fixedRelativeDirection) :
                                attackAction.attackDir.normalized;
        Vector2 perpVec = Vector3.Cross(direction, Vector3.forward);
        float maxVel = Mathf.Max(Mathf.Sqrt(Vector2.Dot(weapon.mvmt.rb.velocity, direction)) + speed, speed);
        while(attackAction.state == AttackState.Attacking) {
            weapon.mvmt.rb.gravityScale = 0;
            Vector2 vel = weapon.mvmt.rb.velocity;
            Vector2 newVel = vel.SharpInDamp(direction * maxVel, 5).Projected(direction) +
                             vel.Projected(perpVec).SharpInDamp(Vector2.zero, perpVelCancelSpeed);
            if(!float.IsNaN(newVel.x)) weapon.mvmt.rb.velocity = newVel;
            maxVel += acceleration * Time.fixedDeltaTime;
            yield return Yields.WaitForFixedUpdate;
        }
        weapon.mvmt.rb.gravityScale = 1;
    }


    private IEnumerator _GroundedEndCheck(AttackAction attackAction) {
        yield return new WaitWhile(() => attackAction.state == AttackState.WindingUp ||
                                         attackAction.state == AttackState.Attacking && !weapon.mvmt.grounded);

        if(alsoEndRestOfAttackWhenGrounded) attackAction.EndAttack();
        else {
            weapon.StopCoroutine(_attackDash);
            weapon.mvmt.rb.gravityScale = 1;
        }
    }
}