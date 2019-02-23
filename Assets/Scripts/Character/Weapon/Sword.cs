using System;
using System.Collections;
using UnityEngine;
using static AnimationParameters.Weapon;

public class Sword : WeaponAbstract {
    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.1f;

    #region Variables

    [Tooltip("How much the weapon hurts"), SerializeField]
    private int damage = 17;

    [Tooltip("Knockback force applied by weapon"), SerializeField]
    private float knockback = 50;

    //TODO Should also affect swing speed
    [Tooltip("How much should this weapon slow down the character (and their swing), " +
             "and how much should their velocity increase the weapon's force"), SerializeField]
    private float mass = 5;

    #endregion


    public override void OnEquip(CharacterMasterAbstract newHolder) {
        base.OnEquip(newHolder);
        anim.SetBool(SwordEquipped, true);
    }

    public override void OnUnequip() {
        //TODO have to make weapon move to holster or something
        anim.SetBool(SwordEquipped, false);
    }

    private void Update() {
        if((control.attackHPress || control.attackVPress) && !attacking) {
            StartCoroutine(Attack());
        }
    }

    enum Direction {
        Up,
        Down,
        Left,
        Right
    }

    private IEnumerator Attack() {
        print("ATTACK" + Time.time);
        int flipSign = movement.facingRight ? 1 : -1;
        attacking = true;
        Direction attackDir = control.attackHPress ?
                                  (int) Mathf.Sign(control.attackHorizontal) == flipSign ?
                                      Direction.Right :
                                      Direction.Left :
                                  (int) Mathf.Sign(control.attackVertical) == 1 ?
                                      Direction.Up :
                                      Direction.Down;

        float attackHoldTime = 0;
        switch(attackDir) {
            case Direction.Up:
                break;
            case Direction.Down:
                anim.SetTrigger(JumpStabDownAnim);
                break;
            case Direction.Left:
                break;
            case Direction.Right:
                anim.SetTrigger(JabForwardAnim);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        while(control.attackHPressed || control.attackVPressed) {
            attackHoldTime += Time.deltaTime;
            if(attackHoldTime > TapThreshold) {
                anim.SetTrigger(SwingForwardAnim);
                break;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(!swinging || hitSomething) return;
        //FIXME? Right now this means if you hit anything (even not damageable), you won't be able to hit anything else that swing
        hitSomething = true;
        // We could fix this by moving it down, but then you'll be able to swing your sword and hit people through walls
        IDamageable damageable = (other.GetComponent<IDamageable>() ??
                                  other.attachedRigidbody?.GetComponent<IDamageable>()) ??
                                 other.GetComponentInParent<IDamageable>();
        if(damageable != null) {
            Collider2D thisColl = GetComponent<Collider2D>();
            Vector2 point = other.Distance(thisColl).pointB;

            Vector2 force = thisColl.attachedRigidbody.velocity; //Relative Velocity
            if(other.attachedRigidbody) force -= other.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol

            //add knockback in the direction of the swing
            Vector2 rightUp = transform.right + transform.up / 4;
            force += rightUp * knockback * (movement.facingRight ? 1 : -1);

            //don't damage if we hit their weapon, otherwise, damage scaled based on relative velocity
            int damageGiven = other.isTrigger ? 0 : (int) (damage * force.magnitude / knockback);
//            print($"{point}, {force}, {damage}");
            damageable.DamageMe(point, force, damageGiven, other);
        }
    }
}