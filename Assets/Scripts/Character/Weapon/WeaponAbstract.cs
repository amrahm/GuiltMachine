using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    protected const int UpperBodyLayerIndex = 1;

    [NonSerialized] public CharacterMasterAbstract holder;
    protected Animator anim;

    public abstract void Attack(float horizontal, float vertical, bool hPressed, bool vPressed);
    public abstract void ReceiveAnimationEvent(AnimationEventObject e, float duration);

    /// <summary> Fades an animation layer in or out over the specified duration </summary>
    protected IEnumerator FadeLayer(bool fadeIn, int layerIndex, float duration) {
        float start = anim.GetLayerWeight(layerIndex);
        float end = fadeIn ? 1 : 0.01f; //Don't fade out all the way or the fadeIn event won't be able to be called
        float startTime = Time.time;
        float weight = 0;
        while(weight < 0.99f) {
            weight = (Time.time - startTime) / duration;
            anim.SetLayerWeight(layerIndex, Mathf.Lerp(start, end, weight));
            yield return null;
        }
        // ReSharper disable once IteratorNeverReturns
    }
}