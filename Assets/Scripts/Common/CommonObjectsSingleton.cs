using UnityEngine;

[UnitySingleton(UnitySingletonAttribute.Type.FromPrefab, false)]
public class CommonObjectsSingleton : UnitySingleton<CommonObjectsSingleton> {
    [Tooltip("The shared WhatIsGround asset specifying what characters should consider to be ground.")]
    public LayerMaskScriptableObject whatIsGroundMaster;
    
    [Tooltip("The shared WhatIsGround asset specifying what can be hit by a weapon")]
    public LayerMaskScriptableObject whatIsHittableMaster;

    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading in")]
    public AnimationEventObject attackFadeIn;
    
    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading out")]
    public AnimationEventObject attackFadeOut;

    [Tooltip("The ScriptableObject asset signifying a sword swing starting")]
    public AnimationEventObject attackAttackingStart;

    [Tooltip("The ScriptableObject asset signifying a sword swing ending")]
    public AnimationEventObject attackAttackingEnd;
}