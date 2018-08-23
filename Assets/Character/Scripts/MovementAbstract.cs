using UnityEngine;

public abstract class MovementAbstract : MonoBehaviour {
    /// <summary> Which way the character is currently facing </summary>
    public abstract bool FacingRight { get; set; }

    /// <summary> The direction that the character wants to move </summary>
    public abstract Vector2 MoveVec { get; set; }

}
