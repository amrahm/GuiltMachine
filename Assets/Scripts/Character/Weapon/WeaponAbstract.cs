using System.Collections;
using ExtensionMethods;
using static ExtensionMethods.HelperMethods;
using UnityEngine;

public abstract class WeaponAbstract : MonoBehaviour {
    private const int UpperBodyLayerIndex = 1;

    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.1f;

    /// <summary> How long attack buffer lasts </summary>
    private const float BufferTime = 0.3f;

    #region Variables

    private CharacterMasterAbstract _holder;
    private CharacterControlAbstract _ctrl;
    protected Animator anim;
    protected MovementAbstract mvmt;

    /// <summary> Is the weapon swinging </summary>
    protected bool swinging;

    /// <summary> Is an attack started (not necessarily swinging yet) </summary>
    private bool _attacking;

    /// <summary> what was horizontal attack last frame </summary>
    private int _oldHoriz;

    /// <summary> what was vertical attack last frame </summary>
    private int _oldVert;

    /// <summary> when did the current attack start </summary>
    private float _attackStart;

    /// <summary> when did the attempted buffer attack start? </summary>
    private float _bufferAttackStart;

    private WeaponAnimationEventObjects _animEventObjs;
    private Coroutine _fadeCoroutine;
    private Coroutine _bufferAttackCoroutine;

    #endregion

    private void Awake() {
        _animEventObjs = WeaponAnimationEventObjects.Instance;
    }

    private void Update() {
        if(_ctrl.attackHorizontal != 0 && _ctrl.attackHorizontal != _oldHoriz || // If we have new horizontal input
           _ctrl.attackVertical != 0 && _ctrl.attackVertical != _oldVert) { // or new vertical input
            // Then initiate an attack
            if(!_attacking) {
                StartCoroutine(_InitAttack(GetAttackDir()));
                _attackStart = Time.time;
            } else if(_attacking && (_oldVert == 0 && _oldHoriz == 0 || Time.time - _attackStart > TapThreshold * 2)) {
                // but if we were already attacking, and this is an entirely new attack input, or enough time has passed
                // that we know this isn't just them trying to do a diagonal, then start a buffer attack
                if(_bufferAttackCoroutine != null) StopCoroutine(_bufferAttackCoroutine);
                _bufferAttackCoroutine = StartCoroutine(_InitAttack(GetAttackDir(), true));
                _bufferAttackStart = Time.time;
            }
        }
        _oldHoriz = _ctrl.attackHorizontal;
        _oldVert = _ctrl.attackVertical;
    }

    /// <summary> Returns the current directions of attack input </summary>
    private int[] GetAttackDir() => new[] {_ctrl.attackHorizontal, _ctrl.attackVertical};

    /// <summary> Inititates the attack as either a tap or a hold </summary>
    private IEnumerator _InitAttack(int[] attackInit, bool isBufferAttack = false) {
        IEnumerator WaitTillNotAttacking() {
            yield return new WaitWhile(() => _attacking);
            if(Time.time - _bufferAttackStart < BufferTime) {
                _bufferAttackStart = 0;
                isBufferAttack = false;
            }
        }

        float attackInitTime = Time.time;
        do {
            if(Time.time >= attackInitTime + TapThreshold) {
                if(isBufferAttack) yield return StartCoroutine(WaitTillNotAttacking());
                if(isBufferAttack) yield break; // too long since buffer started
                _attacking = true;
                AttackHold(attackInit, GetAttackDir());
                yield break;
            }
            yield return null;
        } while(_ctrl.attackHorizontal != 0 || _ctrl.attackVertical != 0);

        if(isBufferAttack) yield return StartCoroutine(WaitTillNotAttacking());
        if(isBufferAttack) yield break; // too long since buffer started
        _attacking = true;
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
        _holder = newHolder;
        _ctrl = _holder.control;
        _holder.weapon = this;
        anim = _holder.gameObject.GetComponent<Animator>();
        mvmt = _holder.gameObject.GetComponent<MovementAbstract>();
    }

    /// <summary> Call this when you switch away from this weapon </summary>
    public abstract void OnUnequip();

    /// <summary> Call this when you drop this weapon </summary>
    public void OnDrop(CharacterMasterAbstract newHolder) {
        OnUnequip();
        _holder = null;
        _ctrl = null;
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