using UnityEngine;
using static ExtensionMethods.HelperMethods;
using static AnimationParameters.Weapon;

public class Sword : WeaponAbstract {
    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.3f;

    #region Variables

    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading in"),
     SerializeField]
    private AnimationEventObject swingFadeIn;

    [Tooltip("The ScriptableObject asset signifying a sword swing starting"), SerializeField]
    private AnimationEventObject swingStart;

    [Tooltip("The ScriptableObject asset signifying a sword swing ending"), SerializeField]
    private AnimationEventObject swingEnd;

    [Tooltip("How much the weapon hurts"), SerializeField]
    private int damage = 17;

    [Tooltip("Knockback force applied by weapon"), SerializeField]
    private float knockback = 50;

    //TODO Should also affect swing speed
    [Tooltip("How much should this weapon slow down the character (and their swing), " +
             "and how much should their velocity increase the weapon's force"), SerializeField]
    private float mass = 5;


    /// <summary> How long an attack key has been held </summary>
    private float _attackHoldTime;

    /// <summary> Is the sword swinging </summary>
    private bool _swinging;

    /// <summary> Did the sword already hit something this swing </summary>
    private bool _hitSomething;

    /// <summary> Was horizontal attack pressed last frame? </summary>
    private bool _hWasPressed;

    /// <summary> Was vertical attack pressed last frame? </summary>
    private bool _vWasPressed;

    #endregion


    private void Start() {
        anim = holder.gameObject.GetComponent<Animator>();
        movement = holder.gameObject.GetComponent<MovementAbstract>();
    }

    public override void Attack(float horizontal, float vertical, bool hPressed, bool vPressed) {
        //TODO can this become a coroutine?
        //TODO How about instead of doing this based on held down length, we do it based on player velocity
        //FIXME? Not sure why I gotta do this, but otherwise the animation plays twice
        anim.ResetTrigger(JabForwardAnim);
        anim.ResetTrigger(SwingForwardAnim);

        //&& (!hPressed && _hWasPressed || !vPressed && _vWasPressed)
        if(_attackHoldTime > TapThreshold)
            anim.SetTrigger(SwingForwardAnim);

        if(hPressed || vPressed) {
            if(!anim.IsInTransition(1))
                anim.SetTrigger(JabForwardAnim);
            _attackHoldTime += Time.deltaTime;
        } else {
            _attackHoldTime = 0;
        }

        _hWasPressed = hPressed;
        _vWasPressed = vPressed;
    }

    public override void ReceiveAnimationEvent(AnimationEventObject e, float duration) {
        if(e == swingFadeIn) {
            FadeAnimationLayer(this, anim, FadeType.FadeIn, UpperBodyLayerIndex, duration);
        } else if(e == swingStart) {
            _swinging = true;
        } else if(e == swingEnd) {
            _swinging = false;
            _hitSomething = false;
            FadeAnimationLayer(this, anim, FadeType.FadeOut, UpperBodyLayerIndex, duration);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(!_swinging || _hitSomething) return;
        //FIXME? Right now this means if you hit anything (even not damageable), you won't be able to hit anything else that swing
        _hitSomething = true;
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