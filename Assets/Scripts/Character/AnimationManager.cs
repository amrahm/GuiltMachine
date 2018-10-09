using JetBrains.Annotations;
using UnityEngine;
using static AnimationEventObject;

[RequireComponent(typeof(CharacterMasterAbstract))]
public class AnimationManager : MonoBehaviour {
    private CharacterMasterAbstract _characterMaster;

    private void Start() {
        _characterMaster = GetComponent<CharacterMasterAbstract>();
    }

    /// <summary> Used by the animation system to receive events embedded at specific times in animations</summary>
    [UsedImplicitly]
    public void HandleEventWithDuration(AnimationEvent e) {
        AnimationEventObject eObject = e.objectReferenceParameter as AnimationEventObject;
        HandleEvent(eObject, e.floatParameter);
    }

    /// <summary> Used by the animation system to receive events embedded at specific times in animations</summary>
    [UsedImplicitly]
    public void HandleEvent(AnimationEventObject eObject) {
        HandleEvent(eObject, 0);
    }

    private void HandleEvent(AnimationEventObject eObject, float duration) {
        switch(eObject.eventType) {
            case TargetType.WeaponAbstract:
                _characterMaster.weapon?.ReceiveAnimationEvent(eObject, duration);
                break;
            default:
                Debug.LogError($"{nameof(AnimationManager)} doesn't know what to do with event type ${eObject.eventType}");
                break;
        }
    }
}