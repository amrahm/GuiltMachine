using System;
using System.Collections;
using ExtensionMethods;
using static ExtensionMethods.HelperMethods;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    protected const int UpperBodyLayerIndex = 1;

    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.1f;

    /// <summary> How long attack buffer lasts </summary>
    private const float BufferTime = 0.5f;

    #region Variables

    [NonSerialized] public CharacterMasterAbstract holder;
    [NonSerialized] public CharacterControlAbstract ctrl;
    protected Animator anim;
    protected MovementAbstract mvmt;

    /// <summary> Is the weapon swinging </summary>
    protected bool swinging;

    /// <summary> Is an attack started (not necessarily swinging yet) </summary>
    private bool _attacking;

    /// <summary> was attacking true last frame </summary>
    private bool _wasAttacking;

    /// <summary> what was horizontal attack last frame </summary>
    private int _oldHoriz;

    /// <summary> what was vertical attack last frame </summary>
    private int _oldVert;

    /// <summary> when did the current attack start </summary>
    private float _attackStart;

    /// <summary> when did the attempted buffer attack start? </summary>
    private float _bufferAttackStart;

    /// <summary> what direction was the buffer input attack </summary>
    private int[] _bufferInit;

    private WeaponAnimationEventObjects _animEventObjs;
    private Coroutine _fadeCoroutine;

    #endregion

    private void Awake() {
        _animEventObjs = WeaponAnimationEventObjects.Instance;
    }

    private void Update() {
        if(ctrl.attackHorizontal != 0 && ctrl.attackHorizontal != _oldHoriz ||
           ctrl.attackVertical != 0 && ctrl.attackVertical != _oldVert) {
            if(_attacking && (_oldVert == 0 && _oldHoriz == 0 || Time.time - _attackStart > TapThreshold * 2)) {
                _bufferAttackStart = Time.time;
                _bufferInit = GetAttackDir();
            } else if(!_attacking) {
                StartCoroutine(_InitAttack(GetAttackDir()));
                _attackStart = Time.time;
            }
        }
        if(!_attacking && _wasAttacking && Time.time - _bufferAttackStart < BufferTime) {
            StartCoroutine(_InitAttack(_bufferInit));
            _bufferAttackStart = 0;
        }
        _wasAttacking = _attacking;
        _oldHoriz = ctrl.attackHorizontal;
        _oldVert = ctrl.attackVertical;
    }

    /// <summary> Returns the current directions of attack input </summary>
    private int[] GetAttackDir() => new[] {ctrl.attackHorizontal, ctrl.attackVertical};

    /// <summary> Inititates the attack as either a tap or a hold </summary>
    private IEnumerator _InitAttack(int[] attackInit) {
        _attacking = true;
        float attackInitTime = Time.time;
        do {
            if(Time.time >= attackInitTime + TapThreshold) {
                AttackHold(attackInit, GetAttackDir());
                yield break;
            }
            yield return null;
        } while(ctrl.attackHorizontal != 0 || ctrl.attackVertical != 0);

        AttackTap(attackInit, GetAttackDir());
    }

    protected abstract void AttackTap(int[] initDirection, int[] direction);
    protected abstract void AttackHold(int[] initDirection, int[] direction);

    protected IEnumerator _AttackDash(Vector2 direction, float speed,
                                      float acceleration = 20f, float perpVelCancelSpeed = 1.1f) {
        direction = direction.normalized;
        direction.x *= mvmt.flipInt;
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
        _attacking = false;
        mvmt.canFlip = true;
    }

    protected virtual void BeginSwing() { swinging = true; }

    protected void EndSwing() { swinging = false; }
}