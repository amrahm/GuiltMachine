using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterPhysics : MonoBehaviour {
    #region Variables

    [Tooltip("How fast to crouch from an impact")]
    public float crouchSpeed = 2;

    [Tooltip("A list of all objects that are not parts of legs that should bend when crouching, along with an amount from -1 to 1 that they should bend")]
    public GameObject[] nonLegBendParts;

    [Tooltip("The amount that the corresponding part should rotate from -1 to 1")]
    public float[] nonLegBendAmounts;


    /// <summary> A list of all the BodyPartClasses that make up this character </summary>
    public BodyPartClass[] bodyParts;

    /// <summary> A dictionary mapping colliders to their corresponding BodyPartClass </summary>
    public Dictionary<Collider2D, BodyPartClass> collToPart = new Dictionary<Collider2D, BodyPartClass>();

    /// <summary> How much crouch is being added from an impact </summary>
    private float _crouchPlus;

    /// <summary> How much should it crouch </summary>
    private float _crouchAmount;

    /// <summary> Is the player facing right? </summary>
    private bool _facingRight;


    /// <summary> Reference to Movement script, which controls the character's movement </summary>
    private MovementAbstract _movement;

    /// <summary> Reference to Parts script, which contains references to all of the character's body parts </summary>
    private PartsAbstract _parts;

    /// <summary> Reference to character's rigidbody </summary>
    private Rigidbody2D _rb;

    #endregion

    /// <summary> A modification to the amount a single body part reacts to physics </summary>
    public struct CharacterPhysicsMod {
        /// <summary> A collider on the body part to suppress the physics of </summary>
        public Collider2D collider;

        /// <summary> Amount to suppress physics, from 0 to 1. 1 will suppress entirely. </summary>
        public float dampPercent;

        /// <summary> How long to maintain this suppression </summary>
        public float duration;

        public CharacterPhysicsMod(Collider2D collider, float dampPercent, float duration) {
            this.collider = collider;
            this.dampPercent = dampPercent;
            this.duration = duration;
        }
    }

    /// <summary> Partially/completely suppress the physics system for a list of parts </summary>
    /// <param name="mods">List of PlayerPhysicsMods, which specify the part, extent, and duration of suppression</param>
    public void SuppressPhysics(List<CharacterPhysicsMod> mods) {
        foreach(var mod in mods) {
            BodyPartClass part = collToPart[mod.collider];
            part.SuppressAmount = mod.dampPercent;
        }
        if(_actuallySuppressPhysics == null)
            _actuallySuppressPhysics = StartCoroutine(ActuallySuppressPhysics());
    }

    private Coroutine _actuallySuppressPhysics;

    private IEnumerator ActuallySuppressPhysics() {
        yield return null;
    }

    private void Start() {
        _movement = GetComponent<MovementAbstract>();
        _parts = GetComponent<PartsAbstract>();
        _rb = GetComponent<Rigidbody2D>();
        foreach(var part in bodyParts) part.Initialize(this);
    }

    private void FixedUpdate() {
        foreach(var part in bodyParts) {
            part.DirPre = part.bodyPart.transform.TransformDirection(part.partDir);
#if UNITY_EDITOR
            if(part.visSettings) Debug.DrawRay(part.bodyPart.transform.position, part.DirPre);
#endif
        }

        _facingRight = _movement.facingRight;
        CrouchRotation();

        foreach(var part in bodyParts) {
            part.DirPost = part.bodyPart.transform.TransformDirection(part.partDir);
            part.HitRotation();
        }
    }

    private bool _rotated;

    private void LateUpdate() {
        _rotated = true;
        for(int i = 0; i < nonLegBendParts.Length; i++)
            nonLegBendParts[i].transform.Rotate(Vector3.forward, (_facingRight ? 1 : -1) * _crouchAmount * nonLegBendAmounts[i], Space.Self);
        foreach(var part in bodyParts) part.LateRotation();
    }

    /// <summary> Handles contracting legs and body when character hits the ground </summary>
    private void CrouchRotation() {
        if(_crouchAmount < 0.1f && _crouchPlus < 0.1f) return;

        _crouchAmount = _crouchAmount.SharpInDamp(_crouchPlus, crouchSpeed, 1, Time.fixedDeltaTime); //Quickly move towards crouchAmount

        //Bend all the bendy parts
        if(_rotated) {
            _rotated = false;
        } else
            for(int i = 0; i < nonLegBendParts.Length; i++)
                nonLegBendParts[i].transform.Rotate(Vector3.forward, (_facingRight ? 1 : -1) * _crouchAmount * nonLegBendAmounts[i], Space.Self);

        _crouchPlus = _crouchPlus.SharpInDamp(0, crouchSpeed / 4, 1, Time.fixedDeltaTime); //Over time, reduce the crouch from impact
    }

    [Serializable]
    public class BodyPartClass {
        #region Variables

        [Tooltip("The body part to rotate/control")]
        public GameObject bodyPart;

        [Tooltip("The parent body part of this body part. Can be null.")]
        public GameObject parentPart;

        [Tooltip("A list of all of the objects that contain colliders for this body part")]
        public GameObject[] colliderObjects;

        [Tooltip("How intensely this part reacts to impacts")]
        public float partWeakness = 65;

        [Tooltip("The farthest back this part should rotate. Must be less than and within 360 of upper limit, and between -360 and 720.")]
        public float lowerLimit = -180;

        [Tooltip("The farthest forward this part should rotate. Must be more than and within 360 of lower limit, and between -360 and 720.")]
        public float upperLimit = 180;

        [Tooltip("Is this body part a leg, i.e. should it handle touching the floor differently")]
        public bool isLeg;

        [Tooltip("A list of all objects that are parts of this leg that should bend when crouching, along with an amount from -1 to 1 that they should bend")]
        public GameObject[] bendParts;

        [Tooltip("The amount that the corresponding part should rotate from -1 to 1")]
        public float[] bendAmounts;

        [Tooltip("The foot of the leg")] public GameObject foot;

        [Tooltip("Is this the leg closer to the forward direction in this pair?")]
        public bool isLeadingLeg;

        [Tooltip("How far below the part is the max height that should be stepped over, and how far out to check")]
        public Vector2 maxStepHeight;

        [Tooltip("How far below the part should be checked for obstacles, and how far out to check")]
        public Vector2 footStepHeight;

        [Tooltip("How high to lift foot to step over obstacles")]
        public float stepHeightMult;

        [Tooltip("Specifies how forward a foot needs to be moving to be considered stepping for the foot step checks")]
        public float steppingThreshold;

        [Tooltip("Vector that specifies length and height of steps. Should go out as far as the step will, and just above flat ground")]
        public Vector2 stepVec;

        [Tooltip("The direction, in local space, that points from the base of this body part to the tip")]
        public Vector2 partDir = Vector2.right;

#if UNITY_EDITOR
        [Tooltip("Lets you see the setting vectors in the editor for setting them properly. White is partDir, green is stepVec.")]
        public bool visSettings;
#endif


        /// <summary> The root GameObject of this character </summary>
        private CharacterPhysics _pp;

        /// <summary> The root GameObject of this character </summary>
        private Transform _root;

        /// <summary> The parent body part class of this body part. Can be null. </summary>
        private BodyPartClass _parent;

        /// <summary> How far up the length of the body part the collision was </summary>
        private float _upDown;

        /// <summary> A vector from the base position of this body part to the point of the collision </summary>
        private Vector3 _positionVector;

        /// <summary> Value from 0 to 1 signifying how much this part ignores physics </summary>
        public float SuppressAmount { get; set; }

        /// <summary> How much this part is rotating beyond normal </summary>
        private float _rotAmount;

        /// <summary> How much torque is actively being added to rotate this part </summary>
        private float _torqueAmount;

        /// <summary> Should the hit rotation code run? </summary>
        private bool _shouldHitRot;

        /// <summary> Previous horizontal distance of foot to base </summary>
        private float _prevFootDelta;

        /// <summary> If this is a leg, how much crouch is being added from angle of floor being walked on </summary>
        private float _stepCrouchAnglePlus;

        /// <summary> If this is a leg, how much crouch is being added from height of floor being walked on </summary>
        private float _stepCrouchHeightPlus;

        /// <summary> If this is a leg, how much should it crouch </summary>
        private float _stepCrouchAmount;

        /// <summary> If this is a leg, how much foot rotation is being added to match the surface </summary>
        private float _footRotatePlus;

        /// <summary> If this is a leg, how much should the foot rotate </summary>
        private float _footRotateAmount;

        /// <summary> The rightward direction of this bodypart before rotating </summary>
        public Vector3 DirPre { get; set; }

        /// <summary> The rightward direction of this bodypart after rotating </summary>
        public Vector3 DirPost { private get; set; }

        /// <summary> Reference to the rigidbody </summary>
        private Rigidbody2D _rb;

        /// <summary> Vector from the base to the tip of this part </summary>
        private Vector3 _topVector;

        #endregion

        /// <summary> Adds all of the colliderObjects to a handy dictionary named collToPart.
        /// Also determines the length of this body part by looking at all of these colliders, thenceby setting _topVector </summary>
        /// <param name="characterPhysics">The parent CharacterPhysics class</param>
        internal void Initialize(CharacterPhysics characterPhysics) {
            _pp = characterPhysics;
            if(parentPart != null) _parent = _pp.bodyParts.First(part => part.bodyPart == parentPart);
            _rb = _pp.gameObject.GetComponent<Rigidbody2D>();
            _root = _pp.transform;
            List<GameObject> partsTemp = _pp.nonLegBendParts.ToList();
            List<float> amountsTemp = _pp.nonLegBendAmounts.ToList();
            partsTemp.AddRange(bendParts);
            amountsTemp.AddRange(bendAmounts);
            _pp.nonLegBendParts = partsTemp.ToArray();
            _pp.nonLegBendAmounts = amountsTemp.ToArray();

            Vector3 objPos = bodyPart.transform.position;
            Vector3 farPoint = objPos;
            Collider2D farColl = bodyPart.GetComponent<Collider2D>();
            foreach(GameObject co in colliderObjects) {
                Collider2D[] colliders = co.GetComponents<Collider2D>();
                foreach(Collider2D collider in colliders) {
                    if(Vector3.Distance(collider.bounds.center, objPos) >= Vector3.Distance(farPoint, objPos)) {
                        farPoint = collider.bounds.center;
                        farColl = collider;
                    }
                    _pp.collToPart.Add(collider, this);
                }
            }
            farPoint = Vector3.Distance(farPoint + farColl.bounds.extents, objPos) < Vector3.Distance(farPoint - farColl.bounds.extents, objPos)
                           ? bodyPart.transform.InverseTransformPoint(farPoint - farColl.bounds.extents)
                           : bodyPart.transform.InverseTransformPoint(farPoint + farColl.bounds.extents);
            _topVector = new Vector3(farPoint.x, 0);
            if(isLeg) _pp.StartCoroutine(CheckStep());

            if(lowerLimit < -360) {
                float old = lowerLimit;
                lowerLimit = lowerLimit % 360;
                upperLimit += lowerLimit - old;
            }
            if(upperLimit > 720) {
                float old = upperLimit;
                upperLimit = upperLimit % 360;
                lowerLimit += upperLimit - old;
            }
        }

        /// <summary> Checks which foot is moving forward and adjusts it based on where it will end up </summary>
        private IEnumerator CheckStep() {
            bool fastCheck = true;
            float fastCheckTime = 0;
            float maxWalkSlope = _pp._movement.maxWalkSlope;

            while(true) {
                if(_pp._movement.grounded) {
                    float delta = Vector2.Dot(bodyPart.transform.position - foot.transform.position, _root.right);

                    Vector2 flip = _pp._facingRight ? Vector2.one : new Vector2(-1, 1);
                    Vector2 heightStart = bodyPart.transform.position + _root.up * footStepHeight.x;
                    Vector2 heightDir = _root.right * flip * footStepHeight.y * _pp._movement.moveVec.magnitude;
                    Vector2 maxHeightStart = bodyPart.transform.position + _root.up * maxStepHeight.x;
                    Vector2 maxHeightDir = _root.right * flip * maxStepHeight.y * _pp._movement.moveVec.magnitude;
#if UNITY_EDITOR
                    if(visSettings) {
                        Debug.DrawRay(heightStart, heightDir, isLeadingLeg ? Color.cyan : new Color(0f, 1f, 0.72f));
                        Debug.DrawRay(maxHeightStart, maxHeightDir, isLeadingLeg ? new Color(0f, 0.86f, 0.86f) : new Color(0f, 0.92f, 0.66f));
                    }
#endif

                    if(delta - _prevFootDelta < -0.1f || fastCheckTime > 1f) fastCheck = false;
                    float angle = Vector2.SignedAngle(_pp._movement.groundNormal, _root.up) * (_pp._facingRight ? -1 : 1);
                    if((delta - _prevFootDelta > 0.01f || fastCheck || angle > 0 == isLeadingLeg) && Mathf.Abs(angle) < maxWalkSlope) {
                        fastCheck = true;
                        _stepCrouchAnglePlus = Mathf.Abs(angle) * (1 - (float) Math.Tanh(Mathf.Abs(angle) / maxWalkSlope * .8f));
                        _footRotatePlus = angle;
                    }

                    if(delta - _prevFootDelta > steppingThreshold) {
                        RaycastHit2D heightHit = Physics2D.Raycast(heightStart, heightDir, heightDir.magnitude, _pp._movement.whatIsGround);
                        RaycastHit2D maxHeightHit = Physics2D.Raycast(maxHeightStart, maxHeightDir, maxHeightDir.magnitude, _pp._movement.whatIsGround);
                        if(heightHit.collider != null && maxHeightHit.collider == null) {
                            fastCheck = true;

                            Vector2 topStart = new Vector2(heightHit.point.x + flip.x * 0.1f, maxHeightStart.y);
                            RaycastHit2D topHit = Physics2D.Raycast(topStart, _root.up, maxStepHeight.x, _pp._movement.whatIsGround);

                            if(topHit.collider != null)
                                _stepCrouchHeightPlus = (topHit.point - heightStart).magnitude * stepHeightMult;

#if UNITY_EDITOR
                            if(visSettings) {
                                DebugExtension.DebugPoint(topStart, Color.yellow, .2f);
                                Debug.DrawRay(topStart, _root.up * maxStepHeight.x, Color.magenta);
                                if(topHit.collider != null) DebugExtension.DebugPoint(topHit.point, Color.magenta, .2f);
                            }
#endif
                        }
                    }


                    _prevFootDelta = delta;
                } else {
                    fastCheck = false;
                }

                if(fastCheck) {
                    yield return new WaitForFixedUpdate();
                    fastCheckTime += Time.fixedDeltaTime;
                } else yield return new WaitForSeconds(0.1f);
            }
            //ReSharper disable once IteratorNeverReturns
        }

        /// <summary> Handles contracting legs and body when character hits the ground </summary>
        private void StepCrouchRotation() {
            if(_stepCrouchAmount < 0.1f && _stepCrouchAnglePlus < 0.1f && _stepCrouchHeightPlus < 0.1f) {
                _footRotateAmount = 0;
                return;
            }


            _stepCrouchAmount = _stepCrouchAmount.SharpInDamp(_stepCrouchAnglePlus + _stepCrouchHeightPlus, _pp.crouchSpeed, 1, Time.fixedDeltaTime);
            _footRotateAmount = _footRotateAmount.SharpInDamp(_footRotatePlus, _pp.crouchSpeed / 4, 1, Time.fixedDeltaTime);

            //Bend all the bendy parts
            for(int i = 0; i < bendParts.Length; i++)
                bendParts[i].transform.Rotate(Vector3.forward, (_pp._facingRight ? 1 : -1) * _stepCrouchAmount * bendAmounts[i], Space.Self);

            foot.transform.Rotate(Vector3.forward, (_pp._facingRight ? 1 : -1) * _footRotateAmount, Space.Self);

            _stepCrouchHeightPlus = _stepCrouchHeightPlus / 1.1f; //Over time, reduce the crouch
            _stepCrouchAnglePlus = _stepCrouchAnglePlus / 1.5f; //Over time, reduce the crouch
            _footRotatePlus = _footRotatePlus / 2f; //Over time, reduce the rotate
        }

        /// <summary> Calculate how much rotation should be added on collision </summary>
        /// <param name="point">Point of contact</param>
        /// <param name="collisionNormal">Direction of contact</param>
        /// <param name="impulse">Impluse of the collision</param>
        internal void HitCalc(Vector3 point, Vector2 collisionNormal, Vector2 impulse) {
            _shouldHitRot = true; //enable HitRot() to apply rotation
            _positionVector = point - bodyPart.transform.position; //A vector to the position of the hit

            Vector3 toTop = bodyPart.transform.TransformPoint(_topVector) - bodyPart.transform.position; //A vector to the top of this part
            _upDown = Mathf.Clamp(Vector3.Dot(toTop, _positionVector) / Vector3.SqrMagnitude(toTop), -1, 1); //Clamped in case of errors

            //All of the impulse in the direction of the collision normal
            Vector2 forceVectorPre = impulse * _upDown;
            //If it's a leg, only take the horizontal component
            Vector2 forceVector = isLeg ? Vector2.Dot(forceVectorPre, _root.right) * (Vector2) _root.right : forceVectorPre;

            if(isLeg) { //Add crouching using the vertical component for legs
                float verticalForce = (forceVectorPre - forceVector).y;
                if(verticalForce / _rb.mass > 0.2f) { //Min threshold so this isn't constantly active
                    _pp._crouchPlus += verticalForce;
                }
            }

            float rotCorrected = bodyPart.transform.localEulerAngles.z;
            if(lowerLimit < 0 && rotCorrected > upperLimit) rotCorrected -= 360;
            else if(upperLimit > 360 && rotCorrected < lowerLimit) rotCorrected += 360;

            if(rotCorrected > lowerLimit && rotCorrected < upperLimit) {
                //Add the magnitude of this force to torqueAmount, which will make the part rotate. The cross product gives us the proper direction.
                _torqueAmount += (_pp._facingRight ? 1 : -1) * forceVector.magnitude * Mathf.Sign(Vector3.Cross(_positionVector, forceVector).z);

                HandleTouching(collisionNormal);

                //Transfer force that was removed because of a low upDown (+ a bit more) at the hinge of this part in the direction of the impulse to the parent
                _parent?.HitCalc(bodyPart.transform.position, collisionNormal, (1.5f - _upDown) * impulse);
            } else {
//                Debug.Log($"{bodyPart.name}  :::  {bodyPart.transform.localEulerAngles.z} :: {rotCorrected}");
                _parent?.HitCalc(bodyPart.transform.position, collisionNormal, 1.5f * impulse);
            }
        }

        /// <summary> Adjusts the rotation of this part when rotating into something that it's touching </summary>
        private void HandleTouching(Vector2 collisionNormal) {
            //TODO Is massMult needed here? partStrength?
            if(Vector3.Dot(collisionNormal, (DirPre - DirPost).normalized) < -0.1f) return;

            _torqueAmount += (_pp._facingRight ? -2 : 2) * Time.fixedDeltaTime * _upDown * Vector3.Angle(DirPost, DirPre) *
                             Mathf.Sign(Vector3.Cross(collisionNormal, DirPre).z) * (isLeg ? Vector2.Dot(collisionNormal, Vector2.right) : 1);
        }

        private bool _rotated;
        private float _oldRot;
        private bool _stepCrouched;

        /// <summary> Rotates the body part, dispersing the collision torque over time to return to the resting position </summary>
        internal void HitRotation() {
            if(_stepCrouched) {
                _stepCrouched = false; //TODO is this needed?
            } else if(isLeg) StepCrouchRotation();
            if(!_shouldHitRot) return;

            _rotAmount += _torqueAmount * Time.fixedDeltaTime; //Build up a rotation based on the amount of torque from the collision


            if(_rotated) {
                _rotated = false;
                bodyPart.transform.Rotate(Vector3.forward, (_pp._facingRight ? 1 : -1) * partWeakness * 
                                                           (_rotAmount - _oldRot) / 2, Space.Self);
            } else
                bodyPart.transform.Rotate(Vector3.forward, (_pp._facingRight ? 1 : -1) * partWeakness * _rotAmount / 2,
                    Space.Self); //Rotate the part _rotAmount past where it is animated

            _torqueAmount -= _torqueAmount * 3 * Time.fixedDeltaTime; //Over time, reduce the torque added from the collision
            _rotAmount = _rotAmount.SharpInDamp(7 * _rotAmount / 8, 0.8f, 0.02f, Time.fixedDeltaTime); //and return the body part back to rest

            _shouldHitRot = Mathf.Abs(_rotAmount) * partWeakness >= 0.01f; //If the rotation is small enough, stop calling this code
        }

        /// <summary> Rotates the body part, dispersing the collision torque over time to return to the resting position </summary>
        internal void LateRotation() {
            if(isLeg) {
                StepCrouchRotation();
                _stepCrouched = true;
            }

            if(!_shouldHitRot) return;

            bodyPart.transform.Rotate(Vector3.forward, (_pp._facingRight ? 1 : -1) * partWeakness * _rotAmount / 2,
                Space.Self); //Rotate the part _rotAmount past where it is animated
            _rotated = true;
            _oldRot = _rotAmount;
        }
    }

    /// <summary> Passes info from collision events to the BodyPartClass HitCalc method </summary>
    private void CollisionHandler(Collision2D collInfo) {
        foreach(var c in collInfo.contacts) {
            if(collToPart.ContainsKey(c.otherCollider)) {
                BodyPartClass part = collToPart[c.otherCollider];
                Vector2 force = float.IsNaN(c.normalImpulse) ? collInfo.relativeVelocity : c.normalImpulse / Time.fixedDeltaTime * c.normal / 1000;
                part.HitCalc(c.point, c.normal, force);
            }
        }
    }

    public void AddForceAt(Vector2 point, Vector2 force, Collider2D hitCollider) {
        if(!hitCollider.isTrigger) _rb.AddForceAtPosition(force, point, ForceMode2D.Impulse);
        BodyPartClass part = null;
        while(part == null) {
            if(collToPart.ContainsKey(hitCollider)) part = collToPart[hitCollider];
            else hitCollider = hitCollider.transform.parent.GetComponent<Collider2D>();
        }
        part.HitCalc(point, -force, force / 7);
    }

    private void OnCollisionEnter2D(Collision2D collInfo) {
        CollisionHandler(collInfo);
    }

    private void OnCollisionStay2D(Collision2D collInfo) {
        CollisionHandler(collInfo);
    }
}