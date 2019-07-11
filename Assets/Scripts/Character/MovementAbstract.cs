using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public abstract class MovementAbstract : MonoBehaviour {
    /// <summary> How long after walking off a ledge should the character still be considered grounded </summary>
    private const float CoyoteTime = 0.08f;

    #region Variables

    [Header("Movement Setup")]
    [Tooltip("The greatest slope that the character can walk up. Ignore for enemies that only fly.")]
    public float maxWalkSlope = 50;

    public const int RollLayer = 11;


    /// <summary> A mask determining what is ground to the character </summary>
    protected internal LayerMask WhatIsGround { get; private set; }

    /// <summary> Which way the character is currently facing </summary>
    protected internal bool FacingRight { get; private set; } = true;

    /// <summary> 1 if character is facing right, else -1 </summary>
    protected internal int FlipInt { get; private set; } = 1;

    /// <summary> Is the character character allowed to turn around? No if greater than 0 </summary>
    protected internal int CantFlip {
        get => _cantFlip;
        set {
#if UNITY_EDITOR
            // This can help see who's causing problems
            var mth = new StackTrace().GetFrame(1).GetMethod();
            string cls = mth.ReflectedType?.Name;
            if(value > _cantFlip)
                cantFlippers.Add($"{DateTime.Now.TimeOfDay} ::: {cls}  :  {mth.Name}");
            else
                cantFlippers.Remove(cantFlippers.Last(str => cls != null && str.Contains(cls)));
#endif
            _cantFlip = value;
        }
    }

    private int _cantFlip;

#if UNITY_EDITOR
    // ReSharper disable once CollectionNeverQueried.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    protected internal List<string> cantFlippers = new List<string>();
#endif

    /// <summary> Should the character not be allowed to move itself (e.g. if weapon is moving character)? </summary>
    protected internal bool disableMovement;

    /// <summary> Is the character currently on the ground? </summary>
    protected internal bool grounded;

    /// <summary> Did the character just leave the ground, but can still jump to account for reaction time? </summary>
    protected internal bool coyoteGrounded;

    /// <summary> How much time left till not considered grounded </summary>
    private float _coyoteTimeLeft;

    /// <summary> Possible weird states that the character can be in, or default </summary>
    protected internal MovementState movementState;

    /// <summary> The direction that the character wants to move </summary>
    protected internal Vector2 moveVec;

    /// <summary> The slope the character is walking on </summary>
    protected internal float walkSlope;

    /// <summary> The normal of the ground the character is walking on </summary>
    protected internal Vector2 groundNormal;

    /// <summary> Transform component of the gameObject </summary
    protected internal Transform tf;

    /// <summary> Rigidbody component of the gameObject </summary>
    protected internal Rigidbody2D rb;

    /// <summary> Reference to Control script, which gives input to this script </summary>
    protected CharacterControlAbstract control;

    /// <summary> Reference to Master script, which holds the status indicator and weapons </summary>
    protected CharacterMasterAbstract master;

    /// <summary> Reference to the character's animator component </summary>
    protected Animator anim;


    [Flags]
    public enum MovementState { Default, Crouching, Rolling, Diving, Sliding, Climbing }

    #endregion

    protected virtual void Awake() {
        //Setting up references.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        control = GetComponent<CharacterControlAbstract>();
        master = GetComponent<CharacterMasterAbstract>();
        tf = transform;

        // Common WhatIsGround, but remove current layer
        WhatIsGround = CommonObjectsSingleton.Instance.whatIsGroundMaster & ~(1 << gameObject.layer);
    }


    ///<summary> Flip the character around the y axis </summary>
    protected internal void Flip() {
        if(CantFlip > 0) return;
#if UNITY_EDITOR || DEBUG
        if(CantFlip < 0) {
            Debug.LogError($"{nameof(CantFlip)} < 0, and it should never be");
            CantFlip = 0;
        }
#endif
        FacingRight = !FacingRight; // Switch the way the character is labelled as facing.
        FlipInt *= -1;
        //Multiply the character's x local scale by -1.
        var localScale = tf.localScale;
        localScale.x *= -1;
        tf.localScale = localScale;
        // Fix the status indicator to always face the proper direction regardless of phoenix orientation
        Transform statusTf = master.statusIndicator?.gameObject.transform;
        if(statusTf != null) {
            var scale = statusTf.localScale;
            scale.x *= -1;
            statusTf.localScale = scale;
        }
    }


    /// <summary> Updates coyoteGrounded, which gives the character a little extra time to do a jump after walking off
    /// an edge. This helps account for human reaction times and such </summary>
    protected internal void UpdateCoyoteGrounded() {
        if(grounded) _coyoteTimeLeft = CoyoteTime;
        else if(!grounded && _coyoteTimeLeft > 0) {
            _coyoteTimeLeft -= Time.fixedDeltaTime;
            coyoteGrounded = true;
        } else {
            coyoteGrounded = false;
        }
    }
}