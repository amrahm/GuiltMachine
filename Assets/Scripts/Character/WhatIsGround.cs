using UnityEngine;

/// <inheritdoc />
/// <summary> 
/// A shared layermask specifying what characters should consider to be ground.
/// Characters should automatically update their instance of this object to remove their own layer.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/WhatIsGround")]
public class WhatIsGround : ScriptableObject {
    [Tooltip("A mask determining what is ground to characters")]
    public LayerMask whatIsGround;
}
