using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/AnimationEventObject")]
public class AnimationEventObject : ScriptableObject {
    public enum TargetType {
        Default,
        WeaponAbstract
    }

    [Tooltip("Where should the animation manager pass this event? Default if the event doesn't need to go to the animation manager.")]
    public TargetType eventType;
}