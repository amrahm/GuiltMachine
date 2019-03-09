using UnityEngine;

public class ShootSpell : WeaponAbstract {
    #region Variables

    [Tooltip("How much the weapon hurts"), SerializeField]
    private int damage = 17;

    [Tooltip("Knockback force applied by weapon"), SerializeField]
    private float knockback = 50;

    [Tooltip("How much should this weapon slow down the character, " +
             "and how much should their velocity increase the weapon's force"), SerializeField]
    private float mass = 5;

    [Tooltip("How long until an attack is considered held down"), SerializeField]
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

    protected override void AttackTap(int[] initDirection, int[] direction) { throw new System.NotImplementedException(); }
    protected override void AttackHold(int[] initDirection, int[] direction) { throw new System.NotImplementedException(); }
    public override void OnUnequip() { throw new System.NotImplementedException(); }
}