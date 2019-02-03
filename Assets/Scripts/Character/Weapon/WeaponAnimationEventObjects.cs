using UnityEngine;

[UnitySingleton(UnitySingletonAttribute.Type.FromPrefab, false)]
public class WeaponAnimationEventObjects : UnitySingleton<WeaponAnimationEventObjects> {
    [Tooltip("The ScriptableObject asset signifying when the sword swing animation should start fading in")]
    public AnimationEventObject swingFadeIn;

    [Tooltip("The ScriptableObject asset signifying a sword swing starting")]
    public AnimationEventObject swingStart;

    [Tooltip("The ScriptableObject asset signifying a sword swing ending")]
    public AnimationEventObject swingEnd;
}