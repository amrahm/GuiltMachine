using System;
using System.Collections;
using static ExtensionMethods.HelperMethods;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    protected const int UpperBodyLayerIndex = 1;

    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.1f;

    #region Variables

    [NonSerialized] public CharacterMasterAbstract holder;
    [NonSerialized] public CharacterControlAbstract control;
    protected Animator anim;
    protected MovementAbstract movement;

    /// <summary> Is an attack started (not necessarily swinging yet) </summary>
    protected bool attacking;

    /// <summary> Is the weapon swinging </summary>
    protected bool swinging;

    private WeaponAnimationEventObjects _animEventObjs;
    private Coroutine _fadeCoroutine;

    #endregion

    private void Awake() {
        _animEventObjs = WeaponAnimationEventObjects.Instance;
    }

    private void Update() {
        if((control.attackHPress || control.attackVPress) && !attacking) {
            StartCoroutine(Attack());
        }
    }

    private enum Direction {
        Forward,
        Backward,
        Up,
        Down
    }

    private IEnumerator Attack() {
        attacking = true;
        Direction attackDir = control.attackHPress ?
                                  Math.Sign(control.attackHorizontal) == (movement.facingRight ? 1 : -1) ?
                                      Direction.Forward :
                                      Direction.Backward :
                                  Math.Sign(control.attackVertical) == 1 ?
                                      Direction.Up :
                                      Direction.Down;

        float attackHoldTime = 0;
        while(control.attackHPressed || control.attackVPressed) {
            attackHoldTime += Time.deltaTime;
            if(attackHoldTime >= TapThreshold) {
                switch(attackDir) {
                    case Direction.Forward:
                        ForwardHold();
                        break;
                    case Direction.Backward:
                        BackwardHold();
                        break;
                    case Direction.Up:
                        UpHold();
                        break;
                    case Direction.Down:
                        DownHold();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            }
            yield return null;
        }

        if(attackHoldTime < TapThreshold)
            switch(attackDir) {
                case Direction.Forward:
                    ForwardTap();
                    break;
                case Direction.Backward:
                    BackwardTap();
                    break;
                case Direction.Up:
                    UpTap();
                    break;
                case Direction.Down:
                    DownTap();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }

    protected abstract void UpTap();
    protected abstract void UpHold();
    protected abstract void DownTap();
    protected abstract void DownHold();
    protected abstract void BackwardTap();
    protected abstract void BackwardHold();
    protected abstract void ForwardTap();
    protected abstract void ForwardHold();


    /// <summary> Call this when you pickup or switch to this weapon </summary>
    /// <param name="newHolder"> The CharacterMasterAbstract of the character that just picked up this weapon </param>
    public virtual void OnEquip(CharacterMasterAbstract newHolder) {
        holder = newHolder;
        control = holder.control;
        holder.weapon = this;
        anim = holder.gameObject.GetComponent<Animator>();
        movement = holder.gameObject.GetComponent<MovementAbstract>();
    }

    /// <summary> Call this when you switch away from this weapon </summary>
    public abstract void OnUnequip();

    /// <summary> Call this when you drop this weapon </summary>
    public void OnDrop(CharacterMasterAbstract newHolder) {
        OnUnequip();
        holder = null;
        control = null;
        anim = null;
        movement = null;
    }

    /// <summary>
    /// Receives events from weapon animations.
    /// To add events that are only for a special weapon, override this and call this base method in the last else block
    /// </summary>
    /// <param name="e"> The object sent from the animation </param>
    /// <param name="duration"> An optional duration that some events need </param>
    public virtual void ReceiveAnimationEvent(AnimationEventObject e, float duration) {
        if(e == _animEventObjs.swingFadeIn) {
            if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeIn, UpperBodyLayerIndex, duration);
        } else if(e == _animEventObjs.swingStart) { BeginSwing(); } else if(e == _animEventObjs.swingEnd) {
            EndSwing();
        } else if(e == _animEventObjs.swingFadeOut) {
            if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeOut, UpperBodyLayerIndex, duration);
        }
    }

    protected virtual void BeginSwing() { swinging = true; }

    protected void EndSwing() {
        attacking = false;
        swinging = false;
    }
}