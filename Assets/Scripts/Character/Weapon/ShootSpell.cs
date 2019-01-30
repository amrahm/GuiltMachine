using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static ExtensionMethods.HelperMethods;

public class ShootSpell : WeaponAbstract {
    #region Variables

    [FormerlySerializedAs("_swingFadeIn")]
    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading in")]
    [SerializeField]
    private AnimationEventObject swingFadeIn;

    [FormerlySerializedAs("_swingStart")]
    [Tooltip("The ScriptableObject asset signifying a sword swing starting")]
    [SerializeField]
    private AnimationEventObject swingStart;

    [FormerlySerializedAs("_swingEnd")]
    [Tooltip("The ScriptableObject asset signifying a sword swing ending")]
    [SerializeField]
    private AnimationEventObject swingEnd;

    [FormerlySerializedAs("_damage")]
    [Tooltip("How much the weapon hurts")]
    [SerializeField]
    private int damage = 17;

    [FormerlySerializedAs("_knockback")]
    [Tooltip("Knockback force applied by weapon")]
    [SerializeField]
    private float knockback = 50;

    [FormerlySerializedAs("_mass")]
    [Tooltip("How much should this weapon slow down the character, and how much should their velocity increase the weapon's force")]
    [SerializeField]
    private float mass = 5;

    [FormerlySerializedAs("_tapThreshold")]
    [Tooltip("How long until an attack is considered held down")]
    [SerializeField]
    private float tapThreshold;


    /// <summary> How long an attack key has been held </summary>
    private float _attackHoldTime;

    /// <summary> Is the sword swinging </summary>
    private bool _swinging;

    /// <summary> Did the sword already hit something this swing </summary>
    private bool _hitSomething;

    /// <summary> The speed parameter in the animator </summary>
    private int _jabArmRightAnim;

    /// <summary> The vertical speed parameter in the animator </summary>
    private int _swingArmRightAnim;

    /// <summary> Was horizontal attack pressed last frame? </summary>
    private bool _hWasPressed;

    /// <summary> Was vertical attack pressed last frame? </summary>
    private bool _vWasPressed;

    #endregion


    private void Start()
    {
        anim = holder.gameObject.GetComponent<Animator>();
        movement = holder.gameObject.GetComponent<MovementAbstract>();

        _jabArmRightAnim = Animator.StringToHash("JabArmRight");
        _swingArmRightAnim = Animator.StringToHash("SwingArmRight");
    }

    public override void Attack(float horizontal, float vertical, bool hPressed, bool vPressed)
    {
        //TODO How about instead of doing this based on held down length, we do it based on player velocity
        anim.ResetTrigger(_jabArmRightAnim); //FIXME? Not sure why I gotta do this, but otherwise the animation plays twice
        anim.ResetTrigger(_swingArmRightAnim);

        if (_attackHoldTime < tapThreshold && (!hPressed && _hWasPressed || !vPressed && _vWasPressed)) anim.SetTrigger(_jabArmRightAnim);

        if (hPressed || vPressed)
        {
            if (!anim.IsInTransition(1))
                anim.SetTrigger(_swingArmRightAnim);
            _attackHoldTime += Time.deltaTime;
        }
        else
        {
            _attackHoldTime = 0;
        }

        _hWasPressed = hPressed;
        _vWasPressed = vPressed;
    }

    public override void ReceiveAnimationEvent(AnimationEventObject e, float duration)
    {
        if (e == swingFadeIn)
        {
            FadeAnimationLayer(this, anim, FadeType.FadeIn , UpperBodyLayerIndex, duration);
        }
        else if (e == swingStart)
        {
            _swinging = true;
        }
        else if (e == swingEnd)
        {
            _swinging = false;
            _hitSomething = false;
            FadeAnimationLayer(this, anim, FadeType.FadeOut , UpperBodyLayerIndex, duration);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_swinging || _hitSomething) return;
        _hitSomething = true; //FIXME? Right now this means if you hit anything (even not damageable), you won't be able to hit anything else that swing
                              // We could fix this by moving it down, but then you'll be able to swing your sword and hit people through walls
        IDamageable damageable = (other.GetComponent<IDamageable>() ??
                                  other.attachedRigidbody?.GetComponent<IDamageable>()) ??
                                 other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Collider2D thisColl = GetComponent<Collider2D>();
            Vector2 point = other.Distance(thisColl).pointB;

            Vector2 force = thisColl.attachedRigidbody.velocity; //Relative Velocity
            if (other.attachedRigidbody) force -= other.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol

            //add knockback in the direction of the swing
            Vector2 rightUp = transform.right + transform.up / 4;
            force += rightUp * knockback * (movement.facingRight ? 1 : -1);

            //don't damage if we hit their weapon, otherwise, damage scaled based on relative velocity
            int damageGiven = other.isTrigger ? 0 : (int)(damage * force.magnitude / knockback);
            //            print($"{point}, {force}, {damage}");
            damageable.DamageMe(point, force, damageGiven, other);
        }
    }
}