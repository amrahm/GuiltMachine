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
}