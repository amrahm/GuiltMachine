using System;
using UnityEngine;

public abstract class MovementAbstract : ScriptableObject {
    [Tooltip("A mask determining what is ground to the character")]
    public LayerMask whatIsGround;

    [Tooltip("The greatest slope that the character can walk up")]
    public float maxWalkSlope = 50;

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

    /// <summary> Set up references for the movement script </summary>
    /// <param name="parts">Reference to the parts script, if needed. Must be the correct subclass of PartsAbstract</param>
    /// <param name="anim">Reference to the character's animator</param>
    /// <param name="rb">Reference to the character's rigidbody</param>
    /// <param name="tf">Reference to the character's transform</param>
    public abstract void Initialize(PartsAbstract parts, Animator anim, Rigidbody2D rb, Transform tf);

    /// <summary> Move this character with its horizontal controls </summary>
    public abstract void MoveHorizontal(float horizontal, bool hPressed, float sprint);

    /// <summary> Move this character with its vertical controls </summary>
    public abstract void MoveVertical(float vertical, bool upPressed, bool downPressed);

    /// <summary> Update whether or not this character is touching the ground </summary>
    public abstract void UpdateGrounded();
}