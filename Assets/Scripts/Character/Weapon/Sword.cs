using System.Collections;
using ExtensionMethods;
using UnityEngine;
using static AnimationParameters.Weapon;
using static UnityEngine.Physics2D;

public class Sword : WeaponAbstract {
    #region Variables

#if UNITY_EDITOR
    [Tooltip("Show debug visualizations"), SerializeField]
    private bool visualizeDebug;
#endif

    [Tooltip("Base of the blade of the sword"), SerializeField]
    private Transform swordBase;

    [Tooltip("Tip of the blade of the sword"), SerializeField]
    private Transform swordTip;

    [Tooltip("How much the weapon hurts"), SerializeField]
    private int damage = 17;

    [Tooltip("Knockback force applied by weapon"), SerializeField]
    private float knockback = 50;

    //TODO Should also affect swing speed
    [Tooltip("How much should this weapon slow down the character (and their swing), " +
             "and how much should their velocity increase the weapon's force"), SerializeField]
    private float mass = 5;


    /// <summary> previous position of the base </summary>
    private Vector2 _prevBase;

    /// <summary> previous position of the tip </summary>
    private Vector2 _prevTip;

    #endregion


    protected override void UpTap() { anim.SetTrigger(HoldUpAnim); } //TODO

    protected override void UpHold() {
        anim.SetTrigger(HoldUpAnim);
        if(!movement.grounded) StartCoroutine(_AttackDash(Direction.Up, 9, 1.2f));
    }

    protected override void DownTap() { anim.SetTrigger(movement.grounded ? TapDownAnim : TapHoldDownAirAnim); }

    protected override void DownHold() {
        if(movement.grounded)
            anim.SetTrigger(HoldDownAnim);
        else {
            anim.SetTrigger(TapHoldDownAirAnim);
            StartCoroutine(_AttackDash(Direction.Down, 15, 1.05f));
        }
    }

    protected override void BackwardTap() { anim.SetTrigger(TapBackwardAnim); }

    protected override void BackwardHold() {
        anim.SetTrigger(HoldBackwardAnim);
        if(!movement.grounded) StartCoroutine(_AttackDash(Direction.Backward, 10));
    }

    protected override void ForwardTap() { anim.SetTrigger(TapForwardAnim); }

    protected override void ForwardHold() {
        anim.SetTrigger(HoldForwardAnim);
        if(!movement.grounded) StartCoroutine(_AttackDash(Direction.Forward, 10));
    }

    public override void OnEquip(CharacterMasterAbstract newHolder) {
        base.OnEquip(newHolder);
        anim.SetBool(SwordEquipped, true);
    }

    public override void OnUnequip() {
        //TODO have to make weapon move to holster or something
        anim.SetBool(SwordEquipped, false);
    }

    protected override void BeginSwing() {
        base.BeginSwing();
        StartCoroutine(_CheckHit());
    }

    private IEnumerator _CheckHit() {
        _prevBase = swordBase.position;
        _prevTip = swordTip.position;
        while(swinging) {
            bool GetBaddyHit(out RaycastHit2D swingCheck) {
                bool HitBaddy(RaycastHit2D rHit) {
#if UNITY_EDITOR
                    if(visualizeDebug && rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                       Linecast(_prevBase, rHit.point,
                                movement.whatIsGround & ~(1 << rHit.collider.gameObject.layer))) {
                        Debug.DrawLine(_prevBase, rHit.point, Color.red);
                    }
#endif
                    return rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                           !Linecast(_prevBase, rHit.point,
                                     movement.whatIsGround & ~(1 << rHit.collider.gameObject.layer));
                }

//            print("CHECK1");
                //CHECK1: along blade
                Vector2 basePos = swordBase.position;
                Vector2 tipPos = swordTip.position;
                swingCheck = Linecast(basePos, tipPos, movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(basePos, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;

//            print("CHECK2");
                //CHECK2: along base movement
                swingCheck = Linecast(_prevBase, basePos, movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevBase, basePos);
#endif
                if(HitBaddy(swingCheck)) return true;

//            print("CHECK3");
                //CHECK3: along tip movement
                swingCheck = Linecast(_prevTip, tipPos, movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevTip, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;

//            print("CHECK4");
                //CHECK4: along lower third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f),
                                      movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f));
#endif
                if(HitBaddy(swingCheck))  return true;

//            print("CHECK5");
                //CHECK5: along upper third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f),
                                      movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f));
#endif
                if(HitBaddy(swingCheck))  return true;

//            print("CHECK6");
                //CHECK6: along first third blade
                float swordLength = Vector2.Distance(basePos, tipPos);
                Vector2 baseMid = Vector2.Lerp(_prevBase, basePos, 0.33f);
                Vector2 tipMid = Vector2.Lerp(_prevTip, tipPos, 0.33f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawRay(baseMid, (tipMid - baseMid).normalized * swordLength);
#endif
                if(HitBaddy(swingCheck))  return true;

//            print("CHECK7");
                //CHECK7: along second third blade
                baseMid = Vector2.Lerp(_prevBase, basePos, 0.66f);
                tipMid = Vector2.Lerp(_prevTip, tipPos, 0.66f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, movement.whatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawRay(baseMid, (tipMid - baseMid).normalized * swordLength);
#endif
                return HitBaddy(swingCheck);
            }

            // Check for any hits, then update the prev things
            bool baddyHit = GetBaddyHit(out RaycastHit2D swingHit);
            _prevBase = swordBase.position;
            _prevTip = swordTip.position;
            // Wait till next frame if we didn't hit anything hittable
            if(!baddyHit) {
                yield return Yields.WaitForFixedUpdate;
                continue;
            }

            IDamageable damageable = swingHit.collider.GetComponentInParent<IDamageable>();
            Vector2 point = swingHit.point;

            Vector2 force = movement.rb.velocity; //Relative Velocity
            if(swingHit.collider.attachedRigidbody) force -= swingHit.collider.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol

            // Add knockback in the direction of the swing
            Vector2 rightUp = transform.right + transform.up / 4;
            force += rightUp * knockback * (movement.facingRight ? 1 : -1);

            // Damage scaled based on relative velocity
            int damageGiven = (int) (damage * force.magnitude / knockback);
            damageable.DamageMe(point, force, damageGiven, swingHit.collider);
            yield break;
        }
    }
}