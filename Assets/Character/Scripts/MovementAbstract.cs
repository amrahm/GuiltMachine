using UnityEngine;

public abstract class MovementAbstract : MonoBehaviour {
    /// <summary> Which way the character is currently facing </summary>
    public abstract bool FacingRight { get; set; }

    /// <summary> Layermask specifying what to check as ground </summary>
    public abstract LayerMask WhatIsGround { get; set; }

    /// <summary> Is the player currently on the ground </summary>
    public abstract bool Grounded { get; set; }
    
    /// <summary> The direction that the character wants to move </summary>
    public abstract Vector2 MoveVec { get; set; }
    
    /// <summary> The max slope the character can walk up </summary>
    public abstract float MaxWalkSlope { get; set; }

    /// <summary> The slope the character is walking on </summary>
    public abstract float WalkSlope { get; set; }

    /// <summary> The normal of the ground the character is walking on </summary>
    public abstract Vector2 GroundNormal { get; set; }

}
