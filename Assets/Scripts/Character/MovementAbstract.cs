using System;
using UnityEngine;

public abstract class MovementAbstract : MonoBehaviour {
    [Tooltip("The shared WhatIsGround asset specifying what characters should consider to be ground. Ignore for enemies that only fly.")]
    public WhatIsGround whatIsGroundMaster;

    [Tooltip("The greatest slope that the character can walk up. Ignore for enemies that only fly.")]
    public float maxWalkSlope = 50;


    /// <summary> A mask determining what is ground to the character </summary>
    [NonSerialized] public LayerMask whatIsGround;

    /// <summary> Which way the character is currently facing </summary>
    [NonSerialized] public bool facingRight = true;

    /// <summary> Is the player currently on the ground </summary>
    [NonSerialized] public bool grounded;

    /// <summary> The direction that the character wants to move </summary>
    [NonSerialized] public Vector2 moveVec;

    /// <summary> The slope the character is walking on </summary>
    [NonSerialized] public float walkSlope;

    /// <summary> The normal of the ground the character is walking on </summary>
    [NonSerialized] public Vector2 groundNormal;

    /// <summary> Transform component of the gameObject </summary
    protected Transform tf;

    /// <summary> Rigidbody component of the gameObject </summary>
    protected Rigidbody2D rb;

    /// <summary> Reference to Control script, which gives input to this script </summary>
    protected CharacterControlAbstract control;

    /// <summary> Reference to Master script, which holds the status indicator and weapons </summary>
    protected CharacterMasterAbstract master;

    /// <summary> Reference to the player's animator component </summary>
    protected Animator anim;

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
}