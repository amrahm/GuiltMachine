using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static ExtensionMethods.HelperMethods;
using static MovementAbstract;
using static UnityEngine.Physics2D;

/* ------ Script Overview ------
 * This script handles the framework for weapons/enemies whose bodies are weapons.
 * It allows us to specify attacks in different directions, dependent on input and movement state.
 * In the inspector, we can create a list of AttackDefinitions, which consist of WeaponAttackAbstracts,
 * as well as damage and some other stats about the attack.
 * The AttackDefinition also specifies what attack direction needs to be pressed to trigger it, as
 * well as the movement states that the player can be in (e.g. some attacks can only be triggered in the air)
 * WeaponAttackAbstracts are where the actual code for attacks live. They are scriptable objects that inherit from
 * WeaponAttackAbstract, and multiple of them can be added to a single AttackDefinition to form an attack
 * (e.g. mixing a sword swing with a dash).
 *
 * At runtime, every time an attack input is pressed, a new AttackAction is created. The AttackAction tries to
 * find an AttackDefinition that matches the input direction and the character's current movement state,
 * and then begins that attack. The AttackAction keeps track of what state the attack is in
 * (winding up, attacking, recovering, or fading out) and helps move the attack from one state to the next
 * If another AttackAction is currently happening, it will instead add it as a buffer AttackAction, and if the
 * currently active one finishes in less than BufferTime, it will move this AttackAction out of the buffer and start it.
 * ----------------------------- */

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
    private AttackDefinition[] attacks;

    protected internal CharacterMasterAbstract holder;
    protected internal CharacterControlAbstract ctrl;
    protected internal Animator anim;
    protected internal MovementAbstract mvmt;

    public bool Blocking { get; internal set; }

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

    internal AttackAction primaryAttack;
    internal AttackAction bufferAttack;

    private LayerMask _whatIsHittable;
    private LayerMask _whatIsNotHittable;

    public enum AttackState { DeterminingType, WindingUp, Attacking, Recovering, Ended }

    internal enum AttackInputType {
        /// <summary> This attack will happen on a tap (a hold too if no hold action is defined) </summary>
        Tap,

        /// <summary> If input is held, this attack will happen, else a tap will be chosen if there is one </summary>
        Hold
    }

    [Flags]
    internal enum AttackDirection { Forward, Up, Down, ForwardUp, ForwardDown, Backward, BackwardUp, BackwardDown }

    internal enum GroundedState { Both, Grounded, NotGrounded }

    #endregion


    [Serializable]
    internal class AttackDefinition : IComparable {
        // ReSharper disable once NotAccessedField.Global (Used in Unity inspector automatically)
        public string name = "I THIRST FOR A NAME";

        [Tooltip("Does this attack happen only if you hold it long enough, or is a tap sufficient"), SerializeField]
        internal AttackInputType attackInputType;

        [FormerlySerializedAs("attackScriptableObjects")] [FormerlySerializedAs("attacks")] [SerializeField]
        internal List<AttackComponentAbstract> attackComponents;

        [Tooltip("Which directions can be pressed to activate this attack"), EnumFlags, SerializeField]
        private AttackDirection directionTriggerFlags = (AttackDirection) 1;

        internal AttackDirection[] directionTriggers;

        [Tooltip("Should this attack happen when the character is on the ground, in the air, or both"), SerializeField]
        internal GroundedState groundedState = GroundedState.Both;

        [Tooltip("What movement states can the character be in when initiating this attack"), EnumFlags, SerializeField]
        private MovementState movementStateFlags = (MovementState) (-1);

        internal MovementState[] movementStates;

        [Tooltip("Should the character turn around and preform this attack if the input is in the opposite " +
                 "(horizontal) direction that the player is currently facing"), SerializeField]
        private bool flipIfFacingAway = true;

        [Tooltip("Should the character be prevented from turning around while performing this attack"), SerializeField]
        private bool preventFlipWhileAttacking = true;

        [Tooltip("How much the attack hurts"), SerializeField]
        internal int damage = 20;

        [Tooltip("Knockback force applied by attack"), SerializeField]
        internal float knockback = 50;

        internal void Initialize(WeaponScript weapon) {
            if(flipIfFacingAway) attackComponents.Insert(0, ScriptableObject.CreateInstance<FlipIfFacingAway>());
            if(preventFlipWhileAttacking) attackComponents.Add(ScriptableObject.CreateInstance<PreventFlipWhileAttacking>());
            for(int i = 0; i < attackComponents.Count; i++) {
                attackComponents[i] = Instantiate(attackComponents[i]);
                attackComponents[i].Initialize(weapon);
            }

            directionTriggers = EnumFlagsAttribute.ReturnSelectedElements<AttackDirection>((int) directionTriggerFlags)
                .Select(x => (AttackDirection) x).ToArray();
            movementStates = EnumFlagsAttribute.ReturnSelectedElements<MovementState>((int) movementStateFlags)
                .Select(x => (MovementState) x).ToArray();

            if(flipIfFacingAway) {
                List<AttackDirection> newDirections = new List<AttackDirection>();
                foreach(var direction in directionTriggers) {
                    newDirections.Add(direction);
                    var flipped = FlipAttackDirection(direction);
                    if(flipped != direction) newDirections.Add(flipped);
                }
                directionTriggers = newDirections.ToArray();
            }
        }

        public int CompareTo(object obj) {
            // This is to sort the list of AttackDefinitions so that hold attacks are picked first,
            // then ones with more specific conditions are picked first
            if(obj is AttackDefinition otherAttack) {
                if(attackInputType == AttackInputType.Tap && otherAttack.attackInputType == AttackInputType.Hold ||
                   groundedState == GroundedState.Both && otherAttack.groundedState != GroundedState.Both ||
                   movementStates.Length > otherAttack.movementStates.Length)
                    return 1;
                if(attackInputType == AttackInputType.Hold && otherAttack.attackInputType == AttackInputType.Tap ||
                   groundedState != GroundedState.Both && otherAttack.groundedState == GroundedState.Both ||
                   movementStates.Length < otherAttack.movementStates.Length)
                    return -1;
                return 0;
            }
            throw new ArgumentException($"Object is not a {nameof(AttackDefinition)}");
        }
    }

    public class AttackAction {
        public AttackState state = AttackState.Ended;
        private readonly WeaponScript _wS;

        /// <summary> Direction of the attack </summary>
        internal Vector2 attackDir;

        internal int inH;
        internal int inV;

        internal AttackDefinition attackDefinition;

        private bool _inBuffer;
        internal  Coroutine initAttack;

        private static readonly AttackDefinition NoMatches = new AttackDefinition {
            name = "NoMatches",
            attackComponents = new List<AttackComponentAbstract>()
        };

        internal AttackAction(WeaponScript weaponScript) {
            _wS = weaponScript;
        }

        internal void InitAttack(bool inBuffer = false) {
            _inBuffer = inBuffer;
            inH = inV = 0;
            initAttack = _wS.StartCoroutine(_InitAttackHelper());
        }

        private IEnumerator _InitAttackHelper() {
            void UpdateInputIfNewKeysPressed() {
                if(inH == 0 && _wS.ctrl.attackHorizontal != 0) inH = _wS.ctrl.attackHorizontal;
                if(inV == 0 && _wS.ctrl.attackVertical != 0) inV = _wS.ctrl.attackVertical;
            }

            state = AttackState.DeterminingType;

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

            // Convert the input into a vector 
            attackDir = _wS.mvmt.tf.InverseTransformDirection(inH, inV, 0);

            // Now look for a matching attack, and end early if none was found
            if(!ChooseAttack()) yield break;

            // Then, if it's a hold, make sure they hold long enough
            if(attackDefinition.attackInputType == AttackInputType.Hold) {
                attackInitTime = Time.time;
                yield return new WaitWhile(() => Time.time - attackInitTime < TapThreshold &&
                                                 (_wS.ctrl.attackHorizontal != 0 || _wS.ctrl.attackVertical != 0));
                if(Time.time - attackInitTime < TapThreshold && !ChooseAttack(true))
                    // They didn't hold for long enough, so look for a matching tap attack, and if one isn't found, break
                    yield break;
            }

            // If we get here, an attack was found, so start it if/when we're not in the buffer anymore
            if(_inBuffer) yield return new WaitWhile(() => _inBuffer);
            state = AttackState.WindingUp;
            foreach(var attack in attackDefinition.attackComponents) attack.OnAttackWindup(this);
        }

        private bool ChooseAttack(bool onlyLookForTaps = false) {
            // Figure out which attack, if any, matches the current conditions
            attackDefinition = (from attack in _wS.attacks
                                //TODO instead of GetAttackDirection with inH and inV, do it relative to rotated player
                                where (!onlyLookForTaps || attack.attackInputType == AttackInputType.Tap) &&
                                      attack.directionTriggers.Contains(_wS.GetAttackDirection(inH, inV)) &&
                                      (!_wS.mvmt.grounded || attack.groundedState != GroundedState.NotGrounded) &&
                                      (_wS.mvmt.grounded || attack.groundedState != GroundedState.Grounded) &&
                                      attack.movementStates.Contains(_wS.mvmt.movementState)
                                select attack).DefaultIfEmpty(NoMatches).First();

            // If no attack matches, just cancel this attack (do nothing, basically)
            if(ReferenceEquals(attackDefinition, NoMatches)) {
                if(ReferenceEquals(this, _wS.bufferAttack)) _wS.bufferAttack.state = AttackState.Ended;
                else EndAttack(dontFade: true); // To also make buffer attack primary
                return false;
            }
            return true;
        }

        internal void BeginAttacking() {
            state = AttackState.Attacking;
            foreach(var attack in attackDefinition.attackComponents) attack.OnAttacking(this);
        }

        internal void BeginRecovering() {
            state = AttackState.Recovering;
            foreach(var attack in attackDefinition.attackComponents) attack.OnRecovering(this);
        }

        internal void EndAttack(float duration = 0.3f, bool dontFade = false) {
#if UNITY_EDITOR || DEBUG
            Debug.Assert(!ReferenceEquals(this, _wS.bufferAttack));
#endif
            if(!ReferenceEquals(_wS.primaryAttack, this)) return;
            state = AttackState.Ended;
            if(!dontFade && _wS.anim) _wS.FadeAttackOut(duration);
            foreach(var attack in attackDefinition.attackComponents) attack.OnEnding(this);

            if(_wS.bufferAttack.state != AttackState.Ended && Time.time - _wS._bufferAttackStart < BufferTime) {
                _wS.primaryAttack = _wS.bufferAttack;
                _wS.primaryAttack._inBuffer = false;
                _wS.bufferAttack = this;
                _wS._bufferAttackStart = 0;
            }
        }
    }

    private AttackDirection GetAttackDirection(int inH, int inV) {
        switch(inH * mvmt.FlipInt) {
            case -1:
                switch(inV) {
                    case -1: return AttackDirection.BackwardDown;
                    case 0:  return AttackDirection.Backward;
                    case 1:  return AttackDirection.BackwardUp;
                    default: throw new ArgumentOutOfRangeException(nameof(inV));
                }
            case 0:
                switch(inV) {
                    case -1: return AttackDirection.Down;
                    case 0:  throw new ArgumentOutOfRangeException($"{nameof(inH)} and {nameof(inV)}");
                    case 1:  return AttackDirection.Up;
                    default: throw new ArgumentOutOfRangeException(nameof(inV));
                }
            case 1:
                switch(inV) {
                    case -1: return AttackDirection.ForwardDown;
                    case 0:  return AttackDirection.Forward;
                    case 1:  return AttackDirection.ForwardUp;
                    default: throw new ArgumentOutOfRangeException(nameof(inV));
                }
            default: throw new ArgumentOutOfRangeException(nameof(inH));
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
            default:                           throw new ArgumentOutOfRangeException(nameof(direction));
        }
    }

    private void Awake() {
        primaryAttack = new AttackAction(this);
        bufferAttack = new AttackAction(this);
    }

    private void Start() {
        // All hittable things minus this layer
        _whatIsHittable = CommonObjectsSingleton.Instance.whatIsHittableMaster & ~(1 << gameObject.layer);
        // Everything solid but not hittable
        _whatIsNotHittable = CommonObjectsSingleton.Instance.whatIsGroundMaster &
                             ~CommonObjectsSingleton.Instance.whatIsHittableMaster;
        _animEventObjs = CommonObjectsSingleton.Instance;
        foreach(AttackDefinition attack in attacks) attack.Initialize(this);
        attacks = attacks.Sort();
    }

    private void Update() {
        if(ctrl.attackHorizontal != 0 && ctrl.attackHorizontal != _oldHorizInput || // If we have new horizontal input
           ctrl.attackVertical != 0 && ctrl.attackVertical != _oldVertInput) { // or new vertical input
            if(primaryAttack.state == AttackState.Ended) {
                primaryAttack.InitAttack();
                _attackStart = Time.time;
            } else if(_oldVertInput == 0 && _oldHorizInput == 0 || Time.time - _attackStart > TapThreshold * 2) {
                // but if we were already attacking and this is an entirely new attack input, or enough time has passed
                // that we know this isn't just them trying to do a diagonal, then start a buffer attack
                if(bufferAttack.initAttack != null) StopCoroutine(bufferAttack.initAttack); // else might never stop
                bufferAttack.InitAttack(true);
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
        anim = holder.anim;
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
        if(primaryAttack.state == AttackState.Ended) return;
        if(e == _animEventObjs.attackFadeIn && primaryAttack.state == AttackState.WindingUp) {
            FadeAttackIn(duration);
        } else if(e == _animEventObjs.attackAttackingStart && primaryAttack.state == AttackState.WindingUp) {
            primaryAttack.BeginAttacking();
        } else if(e == _animEventObjs.attackAttackingEnd && primaryAttack.state == AttackState.Attacking) {
            primaryAttack.BeginRecovering();
        } else if(e == _animEventObjs.attackFadeOut && primaryAttack.state == AttackState.Recovering) {
            primaryAttack.EndAttack(duration);
        }
    }

    private void FadeAttackIn(float duration) {
        if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if(anim) _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeIn, AttackOverrideLayerIndex, duration);
    }

    private void FadeAttackOut(float duration) {
        if(_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = FadeAnimationLayer(this, anim, FadeType.FadeOut, AttackOverrideLayerIndex, duration);
    }


    private class FlipIfFacingAway : AttackComponentAbstract {
        private WeaponScript _weaponScript;
        public override void Initialize(WeaponScript weaponScript) { _weaponScript = weaponScript; }

        public override void OnAttackWindup(AttackAction attackAction) {
            if(attackAction.inH != 0 && _weaponScript.mvmt.FlipInt != attackAction.inH)
                _weaponScript.mvmt.Flip();
        }

        public override void OnAttacking(AttackAction attackAction) { }
        public override void OnRecovering(AttackAction attackAction) { }
        public override void OnEnding(AttackAction attackAction) { }
    }

    private class PreventFlipWhileAttacking : AttackComponentAbstract {
        private WeaponScript _weaponScript;
        public override void Initialize(WeaponScript weaponScript) { _weaponScript = weaponScript; }

        public override void OnAttackWindup(AttackAction attackAction) { _weaponScript.mvmt.CantFlip++; }

        public override void OnAttacking(AttackAction attackAction) { }
        public override void OnRecovering(AttackAction attackAction) { }

        public override void OnEnding(AttackAction attackAction) { _weaponScript.mvmt.CantFlip--; }
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
                       Linecast(_prevBase, rHit.point, _whatIsNotHittable & ~(1 << rHit.collider.gameObject.layer))) {
                        Debug.DrawLine(_prevBase, rHit.point, Color.red);
                    }
#endif
                    return rHit && !(rHit.collider.GetComponentInParent<IDamageable>() is null) &&
                           // Make sure nothing of a different layer is between where the sword was and where it hit
                           !Linecast(_prevBase, rHit.point,
                                     _whatIsNotHittable & ~(1 << rHit.collider.gameObject.layer));
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

            IDamageable damageable = swingHit.collider.GetComponentInParent<IDamageable>();
            Vector2 point = swingHit.point;
            Vector2 force = mvmt.rb.velocity; //Relative Velocity
            if(swingHit.collider.attachedRigidbody) force -= swingHit.collider.attachedRigidbody.velocity;
            force = mass * force; //Kinetic Energy = mv^2, but that was too much so just doing mv lol
            force += attackAction.attackDir * attackDefinition.knockback; // Add knockback in the direction of the swing


            switch(damageable.CheckProtected(point, swingHit.collider)) {
                case ProtectedType.Dodging: yield break;
                case ProtectedType.Blocking: {
                    //TODO recoil and sparks or something shit idk
                    var hTf = holder.transform;
                    Vector2 recoil = (hTf.position - swingHit.transform.position + hTf.up / 6) *
                                     attackDefinition.knockback;
                    holder.characterPhysics?.AddForceAt(point, recoil, GetComponentInParent<Collider2D>());
                    FadeAttackOut(0.30121f);
                    attackAction.BeginRecovering();
                    yield break;
                }
            }
            // Damage scaled based on relative velocity
            int damageGiven = (int) Math.Max(attackAction.attackDefinition.damage,
                                             attackAction.attackDefinition.knockback * mvmt.rb.velocity.magnitude);
            damageable.DamageMe(point, force, damageGiven, swingHit.collider);
            yield break;
        }
    }
}