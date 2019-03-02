using System;
using System.Collections;
using ExtensionMethods;
using static ExtensionMethods.HelperMethods;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    protected const int UpperBodyLayerIndex = 1;

    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.1f;

    #region Variables

    [NonSerialized] public CharacterMasterAbstract holder;
    [NonSerialized] public CharacterControlAbstract ctrl;
    protected Animator anim;
    protected MovementAbstract mvmt;

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
        if((ctrl.attackHPress || ctrl.attackVPress) && !attacking) {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack() {
        int[] GetAttackDir(bool hPressed, bool vPressed) => new[] {
            Math.Sign(ctrl.attackHorizontal) * (mvmt.facingRight ? 1 : -1) * (hPressed ? 1 : 0),
            Math.Sign(ctrl.attackVertical * (vPressed ? 1 : 0))
        };

        int[] attackInit = GetAttackDir(ctrl.attackHPressed, ctrl.attackVPressed);
        if(attackInit[0] == 0 && attackInit[1] == 0) yield break; //rare bug

        attacking = true;
        float attackInitTime = Time.time;
        bool hWasPressed = false;
        bool vWasPressed = false;
        do {
            hWasPressed |= ctrl.attackHPressed;
            vWasPressed |= ctrl.attackVPressed;
            if(Time.time >= attackInitTime + TapThreshold) {
                AttackHold(attackInit, GetAttackDir(ctrl.attackHPressed, ctrl.attackVPressed));
                yield break;
            }
            yield return null;
        } while(ctrl.attackHPressed || ctrl.attackVPressed);

        AttackTap(attackInit, GetAttackDir(hWasPressed, vWasPressed));
    }

    protected abstract void AttackTap(int[] initDirection, int[] direction);
    protected abstract void AttackHold(int[] initDirection, int[] direction);

    protected IEnumerator _AttackDash(Vector2 direction, float speed,
                                      float acceleration = 20f, float perpVelCancelSpeed = 1.1f) {
        direction = direction.normalized;
        direction.x *= mvmt.facingRight ? 1 : -1;
        Vector2 perpVec = Vector3.Cross(direction, Vector3.forward);
        float maxVel = Mathf.Max(Mathf.Sqrt(Vector2.Dot(mvmt.rb.velocity, direction)) + speed, speed);
        bool beforeSwing = !swinging;
        while(beforeSwing) {
            if(swinging) beforeSwing = false;
            yield return null;
        }
        while(swinging) {
            mvmt.rb.gravityScale = 0;
            Vector2 vel = mvmt.rb.velocity;
            mvmt.rb.velocity = vel.SharpInDamp(direction * maxVel, 5).Projected(direction) +
                               vel.Projected(perpVec).SharpInDamp(Vector2.zero, perpVelCancelSpeed);
            maxVel += acceleration * Time.deltaTime;
            yield return null;
        }
        mvmt.rb.gravityScale = 1;
    }


    /// <summary> Call this when you pickup or switch to this weapon </summary>
    /// <param name="newHolder"> The CharacterMasterAbstract of the character that just picked up this weapon </param>
    public virtual void OnEquip(CharacterMasterAbstract newHolder) {
        holder = newHolder;
        ctrl = holder.control;
        holder.weapon = this;
        anim = holder.gameObject.GetComponent<Animator>();
        mvmt = holder.gameObject.GetComponent<MovementAbstract>();
    }

    /// <summary> Call this when you switch away from this weapon </summary>
    public abstract void OnUnequip();

    /// <summary> Call this when you drop this weapon </summary>
    public void OnDrop(CharacterMasterAbstract newHolder) {
        OnUnequip();
        holder = null;
        ctrl = null;
        anim = null;
        mvmt = null;
    }

    /// <summary>
    /// Receives events from weapon animations.
    /// To add events that are only for a special weapon, override this and call this base method in the last else block
    /// </summary>
    /// <param name="e"> The object sent from the animation </param>
    /// <param name="duration"> An optional duration that some events need </param>
    public void ReceiveAnimationEvent(AnimationEventObject e, float duration) {
        if(e == _animEventObjs.swingFadeIn) {
            if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeIn, UpperBodyLayerIndex, duration);
        } else if(e == _animEventObjs.swingStart) {
            BeginSwing();
        } else if(e == _animEventObjs.swingEnd) {
            EndSwing();
        } else if(e == _animEventObjs.swingFadeOut) {
            FadeAttackOut(duration);
        }
    }

    protected void FadeAttackOut(float duration) {
        if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeOut, UpperBodyLayerIndex, duration);
    }

    protected virtual void BeginSwing() { swinging = true; }
    protected void EndSwing() { attacking = swinging = false; }
}