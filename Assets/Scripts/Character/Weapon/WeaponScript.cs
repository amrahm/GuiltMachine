using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ExtensionMethods.HelperMethods;
using static MovementAbstract;
using static UnityEngine.Physics2D;

public class WeaponScript : MonoBehaviour {
    private const int AttackOverrideLayerIndex = 1;

    /// <summary> How long to wait to give them a chance to diagonal input </summary>
    private const float TimeToWaitForDiagonal = 0.01f;

    /// <summary> How long until an attack is considered held down </summary>
    private const float TapThreshold = 0.13f;

    /// <summary> How long attack buffer lasts </summary>
    private const float BufferTime = 0.3f;

    #region Variables

#if UNITY_EDITOR
    [Tooltip("Show debug visualizations"), SerializeField]
    private bool visualizeDebug;
#endif

    [Tooltip("Tip/front of the weapon"), SerializeField]
    private Transform weaponTip;

    [Tooltip("Base of the weapon"), SerializeField]
    private Transform weaponBase;

    [Tooltip("If any special parts are needed for any of the attacks, add them here. " +
             "Make sure their name matches the one in the attack.")]
    public WeaponSpecialPart[] specialParts;


    /// <summary> Previous position of the base </summary>
    private Vector2 _prevBase;

    /// <summary> Previous position of the tip </summary>
    private Vector2 _prevTip;

    [Tooltip("The list of attacks this weapon can do"), SerializeField]
    private List<AttackDefinition> attacks;

    protected internal CharacterMasterAbstract holder;
    protected internal CharacterControlAbstract ctrl;
    protected internal Animator anim;
    protected internal MovementAbstract mvmt;

    public bool Blocking { get; private set; }

    /// <summary> what was horizontal attack last frame </summary>
    private int _oldHorizInput;

    /// <summary> what was vertical attack last frame </summary>
    private int _oldVertInput;

    /// <summary> when did the current attack start </summary>
    private float _attackStart;

    /// <summary> when did the attempted buffer attack start? </summary>
    private float _bufferAttackStart;

    //TODO Should also affect swing speed
    [Tooltip("How much should this weapon slow down the character, " +
             "and how much should their velocity increase the weapon's force"), SerializeField]
    private float mass = 5;

    private CommonObjectsSingleton _animEventObjs;
    private Coroutine _fadeCoroutine;
    private Coroutine _bufferAttackCoroutine;

    private AttackAction _primaryAttack;
    private AttackAction _bufferAttack;

    private AttackCondition[] _attackConditions;
    private LayerMask _whatIsHittable;

    public enum AttackState { DeterminingType, WindingUp, Attacking, Recovering, FadingOut }

    internal enum AttackInputType {
        /// <summary> This attack only has a single action </summary>
        Single,

        /// <summary> Different action depending on whether they tap or hold </summary>
        TapHold
    }

    [Flags]
    internal enum AttackDirection { Forward, Up, Down, ForwardUp, ForwardDown, Backward, BackwardUp, BackwardDown }

    internal enum GroundedState { Both, Grounded, NotGrounded }

    #endregion

    [Serializable]
    internal class AttackCondition : IComparable {
        [Tooltip("Which directions can be pressed to activate this attack"), EnumFlags, SerializeField]
        private AttackDirection directionTriggerFlags = (AttackDirection) 1;

        internal AttackDirection[] directionTriggers;

        [Tooltip("Should this attack happen when the character is on the ground, in the air, or both"), SerializeField]
        internal GroundedState groundedState = GroundedState.Both;

        [Tooltip("What movement states can the character be in when initiating this attack"), EnumFlags, SerializeField]
        private MovementState movementStateFlags = (MovementState) (-1);

        internal MovementState[] movementStates;

        [NonSerialized] internal AttackDefinition attackDefinition;

        internal void Initialize(AttackDefinition attackDef) {
            attackDefinition = attackDef;
            directionTriggers = EnumFlagsAttribute.ReturnSelectedElements<AttackDirection>((int) directionTriggerFlags)
                .Select(x => (AttackDirection) x).ToArray();
            movementStates = EnumFlagsAttribute.ReturnSelectedElements<MovementState>((int) movementStateFlags)
                .Select(x => (MovementState) x).ToArray();
        }

        public int CompareTo(object obj) {
            if(obj is AttackCondition otherAttack) {
                if(groundedState == GroundedState.Both && otherAttack.groundedState != GroundedState.Both ||
                   movementStates.Length > otherAttack.movementStates.Length)
                    return 1;
                if(groundedState != GroundedState.Both && otherAttack.groundedState == GroundedState.Both ||
                   movementStates.Length < otherAttack.movementStates.Length)
                    return -1;
                return 0;
            }
            throw new ArgumentException($"Object is not a {nameof(AttackCondition)}");
        }
    }

    [Serializable]
    internal class AttackDefinition {
        // ReSharper disable once NotAccessedField.Global (Used in Unity inspector automatically)
        public string name = "I THIRST FOR A NAME";

        [Tooltip("Does this attack only have a single action or should it do something different if the player taps " +
                 "or the player holds the input keys"), SerializeField]
        internal AttackInputType attackInputType;

        [SerializeField]
        internal List<WeaponAttackAbstract> attackTapActions;

        [SerializeField]
        internal List<WeaponAttackAbstract> attackHoldActions;

        [Tooltip("The condition(s) that activate this attack"), SerializeField]
        internal AttackCondition[] conditions = new AttackCondition[1];

        [Tooltip("Should the character turn around and preform this attack if the input is in the opposite " +
                 "(horizontal) direction that the player is currently facing"), SerializeField]
        private bool flipIfFacingAway = true;

        [Tooltip("Should the character be prevented from turning around while performing this attack"), SerializeField]
        private bool preventFlipWhileAttacking = true;

        [Tooltip("How much the attack hurts"), SerializeField]
        internal int tapDamage = 20;

        [Tooltip("Knockback force applied by attack"), SerializeField]
        internal float tapKnockback = 50;

        [Tooltip("How much the hold attack hurts"), SerializeField]
        internal int holdDamage = 20;

        [Tooltip("Knockback force applied by the hold attack"), SerializeField]
        internal float holdKnockback = 50;

        internal void Initialize(WeaponScript weapon) {
            if(flipIfFacingAway) {
                var flipIfFacingAwayInstance = ScriptableObject.CreateInstance<FlipIfFacingAway>();
                attackTapActions.Insert(0, flipIfFacingAwayInstance);
                if(attackInputType == AttackInputType.TapHold) attackHoldActions.Insert(0, flipIfFacingAwayInstance);
            }
            if(preventFlipWhileAttacking) {
                var preventFlipWhileAttackingInstance = ScriptableObject.CreateInstance<PreventFlipWhileAttacking>();
                attackTapActions.Add(preventFlipWhileAttackingInstance);
                if(attackInputType == AttackInputType.TapHold) attackHoldActions.Add(preventFlipWhileAttackingInstance);
            }
            for(int i = 0; i < attackTapActions.Count; i++) {
                attackTapActions[i] = Instantiate(attackTapActions[i]);
                attackTapActions[i].Initialize(weapon);
            }
            for(int i = 0; i < attackHoldActions.Count; i++) {
                attackHoldActions[i] = Instantiate(attackHoldActions[i]);
                attackHoldActions[i].Initialize(weapon);
            }
            foreach(var condition in conditions) {
                condition.Initialize(this);
                if(flipIfFacingAway) {
                    List<AttackDirection> newDirections = new List<AttackDirection>();
                    foreach(var direction in condition.directionTriggers) {
                        newDirections.Add(direction);
                        var flipped = FlipAttackDirection(direction);
                        if(flipped != direction) newDirections.Add(flipped);
                    }
                    condition.directionTriggers = newDirections.ToArray();
                }
            }
        }
    }

    public class AttackAction {
        public AttackState state = AttackState.DeterminingType;
        private readonly WeaponScript _wA;

        /// <summary> Direction of the attack </summary>
        internal Vector2 attackDir;

        internal int inH;
        internal int inV;

        internal AttackDefinition attackDefinition;
        private List<WeaponAttackAbstract> _attackActions;
        internal bool isHoldAttack;

        private bool _inBuffer;
        internal Coroutine waitForBuffer;

        public AttackAction(WeaponScript weaponScript, bool inBuffer) {
            _wA = weaponScript;
            _inBuffer = inBuffer;
            _wA.StartCoroutine(_InitAttack());
        }

        private void UpdateInputIfNewKeysPressed() {
            if(inH == 0 && _wA.ctrl.attackHorizontal != 0) inH = _wA.ctrl.attackHorizontal;
            if(inV == 0 && _wA.ctrl.attackVertical != 0) inV = _wA.ctrl.attackVertical;
        }

        private IEnumerator _InitAttack() {
            // First, get the attack direction
            UpdateInputIfNewKeysPressed();

            // Wait a little time to see if any other keys are pressed, since pressing two keys to get a
            // diagonal attack input probably won't happen exactly simultaneously
            float attackInitTime = Time.time;
            while(Time.time - attackInitTime < TimeToWaitForDiagonal) {
                UpdateInputIfNewKeysPressed();
                yield return null;
            }
            // Check again; might have only been updated at beginning of loop, since TimeToWaitForDiagonal is so short
            UpdateInputIfNewKeysPressed();

            //Convert the input into a vector 
            attackDir = _wA.mvmt.tf.InverseTransformDirection(inH, inV, 0);

            // Figure out which attack, if any, matches the current conditions
            attackDefinition = (from condition in _wA._attackConditions
                                where condition.directionTriggers.Contains(_wA.GetAttackDirection(inH, inV)) &&
                                      (!_wA.mvmt.grounded || condition.groundedState != GroundedState.NotGrounded) &&
                                      (_wA.mvmt.grounded || condition.groundedState != GroundedState.Grounded) &&
                                      condition.movementStates.Contains(_wA.mvmt.movementState)
                                select condition.attackDefinition).FirstOrDefault();

            // If no attack matches, just cancel this attack (do nothing, basically)
            if(attackDefinition == null) {
                EndAttack(dontFade: true);
                yield break;
            }


            // Then perform the appropriate action(s)
            switch(attackDefinition.attackInputType) {
                case AttackInputType.Single:
                    waitForBuffer = _wA.StartCoroutine(_BeginWindUpWhenNotInBuffer(attackDefinition.attackTapActions));
                    break;
                case AttackInputType.TapHold:
                    _wA.StartCoroutine(_InitTapOrHold());
                    break;
            }
        }

        private IEnumerator _BeginWindUpWhenNotInBuffer(List<WeaponAttackAbstract> attackActions) {
            if(_inBuffer) yield return new WaitWhile(() => _inBuffer);
            state = AttackState.WindingUp;
            _attackActions = attackActions;
            foreach(var attack in attackActions) attack.OnAttackWindup(this);
        }


        /// <summary> Inititates the attack as either a tap or a hold </summary>
        private IEnumerator _InitTapOrHold() {
            float attackInitTime = Time.time;
            do {
                if(Time.time - attackInitTime >= TapThreshold) {
                    waitForBuffer = _wA.StartCoroutine(_BeginWindUpWhenNotInBuffer(attackDefinition.attackHoldActions));
                    isHoldAttack = true;
                    yield break;
                }
                yield return null;
            } while(_wA.ctrl.attackHorizontal != 0 || _wA.ctrl.attackVertical != 0);

            waitForBuffer = _wA.StartCoroutine(_BeginWindUpWhenNotInBuffer(attackDefinition.attackTapActions));
        }

        internal void BeginAttacking() {
            state = AttackState.Attacking;
            foreach(var attack in _attackActions) attack.OnAttacking(this);
        }

        internal void BeginRecovering() {
            state = AttackState.Recovering;
            foreach(var attack in _attackActions) attack.OnRecovering(this);
        }

        internal void EndAttack(float duration = 0.3f, bool dontFade = false) {
            state = AttackState.FadingOut;
            if(!dontFade) {
                _wA.FadeAttackOut(duration);
            }
            foreach(var attack in _attackActions) attack.OnFadingOut(this);
            if(ReferenceEquals(_wA._bufferAttack, this)) {
                _wA._bufferAttack = null;
            } else if(_wA._bufferAttack != null && Time.time - _wA._bufferAttackStart < BufferTime) {
                _wA._primaryAttack = _wA._bufferAttack;
                _wA._primaryAttack._inBuffer = false;
                _wA._bufferAttack = null;
                _wA._bufferAttackStart = 0;
            } else _wA._primaryAttack = null;
        }
    }

    private AttackDirection GetAttackDirection(int inH, int inV) {
        switch(inH * mvmt.FlipInt) {
            case -1:
                switch(inV) {
                    case -1: return AttackDirection.BackwardDown;
                    case 0:  return AttackDirection.Backward;
                    case 1:  return AttackDirection.BackwardUp;
                    default: throw new ArgumentOutOfRangeException($"Invalid value for {nameof(inV)}");
                }
            case 0:
                switch(inV) {
                    case -1: return AttackDirection.Down;
                    case 0:  throw new ArgumentOutOfRangeException($"No input direction??");
                    case 1:  return AttackDirection.Up;
                    default: throw new ArgumentOutOfRangeException($"Invalid value for {nameof(inV)}");
                }
            case 1:
                switch(inV) {
                    case -1: return AttackDirection.ForwardDown;
                    case 0:  return AttackDirection.Forward;
                    case 1:  return AttackDirection.ForwardUp;
                    default: throw new ArgumentOutOfRangeException($"Invalid value for {nameof(inV)}");
                }
            default: throw new ArgumentOutOfRangeException($"Invalid value for {nameof(inH)}");
        }
    }

    private static AttackDirection FlipAttackDirection(AttackDirection direction) {
        switch(direction) {
            case AttackDirection.Forward:      return AttackDirection.Backward;
            case AttackDirection.Up:           return AttackDirection.Up;
            case AttackDirection.Down:         return AttackDirection.Down;
            case AttackDirection.ForwardUp:    return AttackDirection.BackwardUp;
            case AttackDirection.ForwardDown:  return AttackDirection.BackwardDown;
            case AttackDirection.Backward:     return AttackDirection.Forward;
            case AttackDirection.BackwardUp:   return AttackDirection.ForwardUp;
            case AttackDirection.BackwardDown: return AttackDirection.ForwardDown;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    private void Start() {
        _whatIsHittable = CommonObjectsSingleton.Instance.whatIsHittableMaster.layerMask & ~(1 << gameObject.layer);
        _animEventObjs = CommonObjectsSingleton.Instance;
        List<AttackCondition> attackConditionsTemp = new List<AttackCondition>();
        foreach(AttackDefinition attack in attacks) {
            attack.Initialize(this);
            attackConditionsTemp.AddRange(attack.conditions);
        }
        attackConditionsTemp.Sort();
        _attackConditions = attackConditionsTemp.ToArray();
    }

    private void Update() {
        if(ctrl.attackHorizontal != 0 && ctrl.attackHorizontal != _oldHorizInput || // If we have new horizontal input
           ctrl.attackVertical != 0 && ctrl.attackVertical != _oldVertInput) { // or new vertical input
            if(_primaryAttack == null) {
                _primaryAttack = new AttackAction(this, false);
                _attackStart = Time.time;
            } else if(_oldVertInput == 0 && _oldHorizInput == 0 || Time.time - _attackStart > TapThreshold * 2) {
                // but if we were already attacking and this is an entirely new attack input, or enough time has passed
                // that we know this isn't just them trying to do a diagonal, then start a buffer attack
                if(_bufferAttack?.waitForBuffer != null) StopCoroutine(_bufferAttack.waitForBuffer); // else never stops
                _bufferAttack = new AttackAction(this, true);
                _bufferAttackStart = Time.time;
            }
        }
        _oldHorizInput = ctrl.attackHorizontal;
        _oldVertInput = ctrl.attackVertical;
    }

    /// <summary> Call this when you pickup or switch to this weapon </summary>
    /// <param name="newHolder"> The CharacterMasterAbstract of the character that just picked up this weapon </param>
    public void OnEquip(CharacterMasterAbstract newHolder) {
        holder = newHolder;
        ctrl = holder.control;
        holder.weapon = this;
        anim = holder.gameObject.GetComponent<Animator>();
        mvmt = holder.gameObject.GetComponent<MovementAbstract>();
    }

    /// <summary> Call this when you switch away from this weapon </summary>
    public void OnUnequip() { }

    /// <summary> Call this when you drop this weapon </summary>
    public void OnDrop(CharacterMasterAbstract newHolder) {
        OnUnequip();
        holder = null;
        ctrl = null;
        anim = null;
        mvmt = null;
    }

    /// <summary> Receives events from weapon animations. </summary>
    /// <param name="e"> The object sent from the animation </param>
    /// <param name="duration"> An optional duration that some events need </param>
    public void ReceiveAnimationEvent(AnimationEventObject e, float duration) {
        if(_primaryAttack == null) return;
        if(e == _animEventObjs.attackFadeIn && _primaryAttack.state == AttackState.WindingUp) {
            FadeAttackIn(duration);
        } else if(e == _animEventObjs.attackAttackingStart && _primaryAttack.state == AttackState.WindingUp) {
            _primaryAttack.BeginAttacking();
        } else if(e == _animEventObjs.attackAttackingEnd && _primaryAttack.state == AttackState.Attacking) {
            _primaryAttack.BeginRecovering();
        } else if(e == _animEventObjs.attackFadeOut && _primaryAttack.state == AttackState.Recovering) {
            _primaryAttack.EndAttack(duration);
        }
    }

    private void FadeAttackIn(float duration) {
        if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeIn, AttackOverrideLayerIndex, duration);
    }

    private void FadeAttackOut(float duration) {
        if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeOut, AttackOverrideLayerIndex, duration);
    }


    
    private class FlipIfFacingAway : WeaponAttackAbstract {
        private WeaponScript _weaponScript;
        public override void Initialize(WeaponScript weaponScript) { _weaponScript = weaponScript; }

        public override void OnAttackWindup(AttackAction attackAction) {
            if(attackAction.inH != 0 && _weaponScript.mvmt.FlipInt != attackAction.inH)
                _weaponScript.mvmt.Flip();
        }

        public override void OnAttacking(AttackAction attackAction) { }
        public override void OnRecovering(AttackAction attackAction) { }
        public override void OnFadingOut(AttackAction attackAction) { }
    }

    private class PreventFlipWhileAttacking : WeaponAttackAbstract {
        private WeaponScript _weaponScript;
        public override void Initialize(WeaponScript weaponScript) { _weaponScript = weaponScript; }
        public override void OnAttackWindup(AttackAction attackAction) { _weaponScript.mvmt.cantFlip++; }
        public override void OnAttacking(AttackAction attackAction) { }
        public override void OnRecovering(AttackAction attackAction) { }
        public override void OnFadingOut(AttackAction attackAction) { _weaponScript.mvmt.cantFlip--; }
    }

    internal IEnumerator _CheckMeleeHit(AttackAction attackAction) {
        AttackDefinition attackDefinition = attackAction.attackDefinition;
        _prevBase = weaponBase.position;
        _prevTip = weaponTip.position;
        while(attackAction.state == AttackState.Attacking) {
            bool GetBaddyHit(out RaycastHit2D swingCheck) {
                bool HitBaddy(RaycastHit2D rHit) {
#if UNITY_EDITOR
                    if(visualizeDebug && rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                       Linecast(_prevBase, rHit.point, _whatIsHittable & ~(1 << rHit.collider.gameObject.layer))) {
                        Debug.DrawLine(_prevBase, rHit.point, Color.red);
                    }
#endif
                    return rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                           // Make sure nothing of a different layer is between where the sword was and where it hit
                           !Linecast(_prevBase, rHit.point, _whatIsHittable & ~(1 << rHit.collider.gameObject.layer));
                }

//               print("CHECK1");
                //CHECK1: along blade
                Vector2 basePos = weaponBase.position;
                Vector2 tipPos = weaponTip.position;
                swingCheck = Linecast(basePos, tipPos, _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(basePos, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;


//               print("CHECK2");
                //CHECK2: along tip movement
                swingCheck = Linecast(_prevTip, tipPos, _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevTip, tipPos);
#endif
                if(HitBaddy(swingCheck)) return true;

//                print("CHECK3");
                //CHECK3: along base movement
                swingCheck = Linecast(_prevBase, basePos, _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawLine(_prevBase, basePos);
#endif
                if(HitBaddy(swingCheck)) return true;

//               print("CHECK4");
                //CHECK4: along lower third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f),
                                      _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.33f), Vector2.Lerp(basePos, tipPos, 0.33f));
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK5");
                //CHECK5: along upper third movement
                swingCheck = Linecast(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f),
                                      _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug)
                    Debug.DrawLine(Vector2.Lerp(_prevBase, _prevTip, 0.66f), Vector2.Lerp(basePos, tipPos, 0.66f));
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK6");
                //CHECK6: along first third blade
                float swordLength = Vector2.Distance(basePos, tipPos);
                Vector2 baseMid = Vector2.Lerp(_prevBase, basePos, 0.33f);
                Vector2 tipMid = Vector2.Lerp(_prevTip, tipPos, 0.33f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawRay(baseMid, (tipMid - baseMid).normalized * swordLength);
#endif
                if(HitBaddy(swingCheck))  return true;

//               print("CHECK7");
                //CHECK7: along second third blade
                baseMid = Vector2.Lerp(_prevBase, basePos, 0.66f);
                tipMid = Vector2.Lerp(_prevTip, tipPos, 0.66f);
                swingCheck = Raycast(baseMid, tipMid - baseMid, swordLength, _whatIsHittable);
#if UNITY_EDITOR
                if(visualizeDebug) Debug.DrawRay(baseMid, (tipMid - baseMid).normalized * swordLength);
#endif
                return HitBaddy(swingCheck);
            }

            // Check for any hits, then update the prev things
            bool baddyHit = GetBaddyHit(out RaycastHit2D swingHit);
            _prevBase = weaponBase.position;
            _prevTip = weaponTip.position;
            // Wait till next frame if we didn't hit anything hittable
            if(!baddyHit) {
                yield return Yields.WaitForFixedUpdate;
                continue;
            }

            float knockback = attackAction.isHoldAttack ?
                                  attackDefinition.holdKnockback :
                                  attackDefinition.tapKnockback;
            int damage = attackAction.isHoldAttack ? attackDefinition.holdDamage : attackDefinition.tapDamage;

            IDamageable damageable = swingHit.collider.GetComponentInParent<IDamageable>();
            Vector2 point = swingHit.point;
            Vector2 force = mvmt.rb.velocity; //Relative Velocity
            if(swingHit.collider.attachedRigidbody) force -= swingHit.collider.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol
            force += attackAction.attackDir * knockback; // Add knockback in the direction of the swing


            switch(damageable.CheckProtected(point, swingHit.collider)) {
                case ProtectedType.Dodging: yield break;
                case ProtectedType.Blocking: {
                    //TODO recoil and sparks or something shit idk
                    var hTf = holder.transform;
                    Vector2 recoil = (hTf.position - swingHit.transform.position + hTf.up / 6) * knockback;
                    holder.characterPhysics?.AddForceAt(point, recoil, GetComponentInParent<Collider2D>());
                    FadeAttackOut(0.30121f);
                    attackAction.BeginRecovering();
                    yield break;
                }
            }
            // Damage scaled based on relative velocity
            int damageGiven = (int) Math.Max(damage, damage * mvmt.rb.velocity.magnitude);
            damageable.DamageMe(point, force, damageGiven, swingHit.collider);
            yield break;
        }
    }
}