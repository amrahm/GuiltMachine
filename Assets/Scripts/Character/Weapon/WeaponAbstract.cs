using System;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    protected const int UpperBodyLayerIndex = 1;

    [NonSerialized] public CharacterMasterAbstract holder;
    protected Animator anim;
    protected MovementAbstract movement;

    public abstract void Attack(float horizontal, float vertical, bool hPressed, bool vPressed);
    public abstract void ReceiveAnimationEvent(AnimationEventObject e, float duration);

}