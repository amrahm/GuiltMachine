using System.Collections;
using ExtensionMethods;
using UnityEngine;
using static AnimationParameters.Weapon;
using static UnityEngine.Physics2D;

public class Sword : WeaponScript {
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


    /// <summary> Previous position of the base </summary>
    private Vector2 _prevBase;

    /// <summary> Previous position of the tip </summary>
    private Vector2 _prevTip;

    /// <summary> Direction of the attack </summary>
    private Vector2 _attackDir;

    #endregion

    private Vector2 Direction(int[] dir) => mvmt.tf.InverseTransformDirection(dir[0], dir[1], 0);
    
    protected override AttackType RequestAttackType(int[] attackDirection) {
        return attackDirection[1] == 1 ? AttackType.Immediate : AttackType.TapHold;
    }

    // ReSharper disable ConvertIfStatementToSwitchStatement
    protected override void AttackTap(int[] direction) {
        _attackDir = Direction(direction);
        if(direction[1] == 0) {
            bool attackIsBackwards = mvmt.FlipInt != direction[0];
            if(mvmt.grounded) {
                anim.SetTrigger(TapForwardAnim);
                if(attackIsBackwards) mvmt.Flip();
            } else {
                anim.SetTrigger(attackIsBackwards ? TapBackwardAnim : TapForwardAnim);
            }
        } else if(direction[1] == 1) anim.SetTrigger(HoldUpAnim); //TODO
        else if(direction[1] == -1) anim.SetTrigger(mvmt.grounded ? TapDownAnim : TapHoldDownAirAnim);
    }

    protected override void AttackHold(int[] direction) {
        _attackDir = Direction(direction);
        if(direction[1] == 0) { // must be horizontal
            bool attackIsBackwards = mvmt.FlipInt != direction[0];
            if(mvmt.grounded) {
                anim.SetTrigger(HoldForwardAnim);
                if(attackIsBackwards) mvmt.Flip();
            } else {
                anim.SetTrigger(attackIsBackwards ? HoldBackwardAnim : HoldForwardAnim);
                StartCoroutine(_AttackDash(_attackDir, 10));
            }
        } else if(direction[1] == 1) {
            anim.SetTrigger(HoldUpAnim);
            if(!mvmt.grounded) StartCoroutine(_AttackDash(_attackDir, 9, perpVelCancelSpeed: 2f));
        } else if(direction[1] == -1) {
            if(mvmt.grounded)
                anim.SetTrigger(HoldDownAnim);
            else {
                anim.SetTrigger(TapHoldDownAirAnim);
                StartCoroutine(_DownDashEndCheck());
                StartCoroutine(_AttackDash(-mvmt.tf.up, 15, perpVelCancelSpeed: 1.05f));
            }
        }
    }
    // ReSharper restore ConvertIfStatementToSwitchStatement

    private IEnumerator _DownDashEndCheck() {
        bool beforeSwing = !swinging;
        while(beforeSwing || swinging && !mvmt.grounded) {
            if(swinging) beforeSwing = false;
            yield return null;
        }
        FadeAttackOut(0.30123f);
        EndSwing();
    }


    public override void OnEquip(CharacterMasterAbstract newHolder) {
        base.OnEquip(newHolder);
        anim.SetBool(SwordEquippedAnim, true);
    }

    public override void OnUnequip() {
        //TODO have to make weapon move to holster or something
        anim.SetBool(SwordEquippedAnim, false);
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
                       Linecast(_prevBase, rHit.point, mvmt.WhatIsGround & ~(1 << rHit.collider.gameObject.layer))) {
                        Debug.DrawLine(_prevBase, rHit.point, Color.red);
                    }
#endif
                    return rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                           // Make sure nothing of a different layer is between where the sword was and where it hit
                           !Linecast(_prevBase, rHit.point, mvmt.WhatIsGround & ~(1 << rHit.collider.gameObject.layer));
                }

//               print("CHECK1");
                //CHECK1: along blade
                Vector2 basePos = swordBase.position;
                Vector2 tipPos = swordTip.position;
                swingCheck = Linecast(basePos, tipPos, mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(basePos, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;


//               print("CHECK2");
                //CHECK2: along tip movement
                swingCheck = Linecast(_prevTip, tipPos, mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevTip, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;

//                print("CHECK3");
                //CHECK3: along base movement
                swingCheck = Linecast(_prevBase, basePos, mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevBase, basePos);
#endif
                if(HitBaddy(swingCheck)) return true;

//               print("CHECK4");
                //CHECK4: along lower third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f),
                                      mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f));
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK5");
                //CHECK5: along upper third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f),
                                      mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f));
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK6");
                //CHECK6: along first third blade
                float swordLength = Vector2.Distance(basePos, tipPos);
                Vector2 baseMid = Vector2.Lerp(_prevBase, basePos, 0.33f);
                Vector2 tipMid = Vector2.Lerp(_prevTip, tipPos, 0.33f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, mvmt.WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawRay(baseMid, (tipMid - baseMid).normalized * swordLength);
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK7");
                //CHECK7: along second third blade
                baseMid = Vector2.Lerp(_prevBase, basePos, 0.66f);
                tipMid = Vector2.Lerp(_prevTip, tipPos, 0.66f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, mvmt.WhatIsGround);
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
            Vector2 force = mvmt.rb.velocity; //Relative Velocity
            if(swingHit.collider.attachedRigidbody) force -= swingHit.collider.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol
            force += _attackDir * knockback; // Add knockback in the direction of the swing


            if(damageable.CheckProtected(point, swingHit.collider)) {
                //TODO recoil and sparks or something shit idk
                var hTf = holder.transform;
                Vector2 recoil = (hTf.position - swingHit.transform.position + hTf.up / 6) * knockback;
                holder.characterPhysics?.AddForceAt(point, recoil, GetComponentInParent<Collider2D>());
                FadeAttackOut(0.30121f);
                EndSwing();
                yield break;
            }
            // Damage scaled based on relative velocity
            int damageGiven = (int) (damage * force.magnitude / knockback);
            damageable.DamageMe(point, force, damageGiven, swingHit.collider);
            yield break;
        }
    }
}