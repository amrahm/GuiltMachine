using UnityEngine;

public abstract class MovementAbstract : MonoBehaviour {
    #region Variables

    [Header("Movement Setup")]
    [Tooltip("The shared WhatIsGround asset specifying what characters should consider to be ground." +
             " Ignore for enemies that only fly.")]
    public WhatIsGround whatIsGroundMaster;

    [Tooltip("The greatest slope that the character can walk up. Ignore for enemies that only fly.")]
    public float maxWalkSlope = 50;


    /// <summary> A mask determining what is ground to the character </summary>
    protected internal LayerMask whatIsGround;

    /// <summary> Which way the character is currently facing </summary>
    protected internal bool facingRight = true;

    /// <summary> 1 if character is facing right, else -1 </summary>
    protected internal int flipInt = 1;

    /// <summary> Is the character character allowed to turn around? </summary>
    protected internal bool canFlip = true;

    /// <summary> Is the character currently on the ground? </summary>
    protected internal bool grounded;

    /// <summary> Did the character just leave the ground, but can still jump to account for reaction time? </summary>
    protected internal bool coyoteGrounded;

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

    #endregion

    protected virtual void Awake() {
        //Setting up references.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        control = GetComponent<CharacterControlAbstract>();
        master = GetComponent<CharacterMasterAbstract>();
        tf = transform;

        if(whatIsGroundMaster != null)
            whatIsGround = whatIsGroundMaster.whatIsGround & ~(1 << gameObject.layer); //remove current layer
    }


    ///<summary> Flip the character around the y axis </summary>
    protected internal void Flip() {
        if(!canFlip) return;
        facingRight = !facingRight; // Switch the way the character is labelled as facing.
        flipInt *= -1;
        //Multiply the character's x local scale by -1.
        tf.localScale = new Vector3(-tf.localScale.x, tf.localScale.y, tf.localScale.z);
        // Fix the status indicator to always face the proper direction regardless of phoenix orientation
        Transform statusTf = master.statusIndicator?.gameObject.transform;
        if(statusTf != null)
            statusTf.localScale = new Vector3(-statusTf.localScale.x, statusTf.localScale.y, statusTf.localScale.z);
    }
}