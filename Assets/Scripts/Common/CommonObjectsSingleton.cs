using UnityEngine;

[UnitySingleton(UnitySingletonAttribute.Type.FromPrefab, false)]
public class CommonObjectsSingleton : UnitySingleton<CommonObjectsSingleton> {
    [Tooltip("The shared layer mask specifying what characters should consider to be ground.")]
    public LayerMask whatIsGroundMaster;
    
    [Tooltip("The shared layer mask specifying what can be hit by a weapon")]
    public LayerMask whatIsHittableMaster;

    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading in")]
    public AnimationEventObject attackFadeIn;
    
    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading out")]
    public AnimationEventObject attackFadeOut;

    [Tooltip("The ScriptableObject asset signifying a sword swing starting")]
    public AnimationEventObject attackAttackingStart;

    [Tooltip("The ScriptableObject asset signifying a sword swing ending")]
    public AnimationEventObject attackAttackingEnd;
}