using UnityEngine;

public class ShootSpell : WeaponAbstract
{
    #region Variables

    [Tooltip("How much the spell hurts")]
    [SerializeField]
    private int _damage = 17;

    [Tooltip("Knockback force applied by spell")]
    [SerializeField]
    private float _knockback = 50;

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
        
    }

    public override void Attack(float horizontal, float vertical, bool hPressed, bool vPressed)
    {
        
    }

    public override void ReceiveAnimationEvent(AnimationEventObject e, float duration)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
    }
}