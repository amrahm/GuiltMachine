using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {
    [SerializeField] private List<BodyPartClass> _bodyParts;
    public Dictionary<Collider2D, BodyPartClass> collToPart = new Dictionary<Collider2D, BodyPartClass>();

    private PlayerMovement _playerMovement; //Reference to PlayerMovement script

    /// <summary> Reference to Parts script, which contains all of the player's body parts </summary>
    private Parts _parts;


    private void Awake() {
        //References
        _playerMovement = GetComponent<PlayerMovement>();
        _parts = GetComponent<Parts>();

        foreach(var part in _bodyParts) part.Initialize(collToPart, _bodyParts);
    }

    private void Update() {
//        crouchAmountSmooth = Extensions.SharpInDamp(crouchAmountSmooth, crouchAmountP, 3.0f);
//        crouchAmountP -= crouchAmountP * Time.deltaTime * 2;
//        crouchAmount = crouchAmountSmooth;

        foreach(var part in collToPart.Values) {
            part.Right = part.bodyPart.transform.right;
            part.FacingRight = _playerMovement.facingRight;
        }
    }

    private void FixedUpdate() {
        RotateTo(_parts.torso, _parts.torsoTarget);
        RotateTo(_parts.head, _parts.headTarget);
        RotateTo(_parts.upperArmR, _parts.upperArmRTarget);
        RotateTo(_parts.lowerArmR, _parts.lowerArmRTarget);
        RotateTo(_parts.handR, _parts.handRTarget);
        RotateTo(_parts.upperArmL, _parts.upperArmLTarget);
        RotateTo(_parts.lowerArmL, _parts.lowerArmLTarget);
        RotateTo(_parts.handL, _parts.handLTarget);
        RotateTo(_parts.thighR, _parts.thighRTarget);
        RotateTo(_parts.shinR, _parts.shinRTarget);
        RotateTo(_parts.footR, _parts.footRTarget);
        RotateTo(_parts.thighL, _parts.thighLTarget);
        RotateTo(_parts.shinL, _parts.shinLTarget);
        RotateTo(_parts.footL, _parts.footLTarget);
        RotateTo(_parts.hips, _parts.hipsTarget);

        foreach(var part in collToPart.Values) {
            part.HitRotation();
            part.HandleTouching();
            part.PostRight = part.bodyPart.transform.right;
        }
    }

    private void RotateTo(GameObject obj, GameObject target) {
        //Reset the local positions cause sometimes they get moved
        if(!_parts.partsToLPositions.ContainsKey(obj)) {
            Debug.Log(obj.name);
        }
        obj.transform.localPosition = _parts.partsToLPositions[obj];

        //Match the animation rotation
        obj.transform.rotation = target.transform.rotation;
    }

    [Serializable]
    public class BodyPartClass {
        #region Variables

        [Tooltip("The body part to rotate/control")]
        public GameObject bodyPart;

        [Tooltip("The parent body part of this body part. Can be null.")] [SerializeField]
        private GameObject _parentPart;

        [Tooltip("A list of all of the objects that contain colliders for this body part")] [SerializeField]
        private List<GameObject> _colliderObjects = new List<GameObject>();

        [Tooltip("How resistant this part is to rotation. 1 is standard, 2 is twice as strong")] [SerializeField]
        private float _partStrength = 1;

        [Tooltip("Is this body part a leg, i.e. should it handle touching the floor differently")] [SerializeField]
        private bool _isLeg;

        /// <summary> The parent body part class of this body part. Can be null. </summary>
        private BodyPartClass _parent;

        /// <summary> How far up the length of the body part the collision was </summary>
        private float _upDown;

        /// <summary> A multiplier from 0 to 1 based on the mass of the colliding object </summary>
        private float _massMult;

        /// <summary> A vector from the base position of this body part to the point of the collision </summary>
        private Vector3 _positionVector;

        /// <summary> The normal vector of the collision </summary>
        private Vector2 _collisionNormal;

        /// <summary> How much this part is rotating beyond normal </summary>
        private float _rotAmount;

        /// <summary> How much torque is actively being added to rotate this part </summary>
        private float _torqueAmount;

        /// <summary> Should the hit rotation code run? </summary>
        private bool _shouldHitRot;

        /// <summary> Is the body part currently touching something? </summary>
        public bool IsTouching { private get; set; }

        /// <summary> The rightward direction of this bodypart after rotating </summary>
        public Vector3 PostRight { private get; set; }

        /// <summary> The rightward direction of this bodypart before rotating </summary>
        public Vector3 Right { private get; set; }

        /// <summary> Is the player facing right? </summary>
        public bool FacingRight { private get; set; }

        /// <summary> Reference to the rigidbody </summary>
        private Rigidbody2D _rb;

        /// <summary> Constant multiplier to scale up the rotations </summary>
        private const float Mult = 25f;

        private Vector3 _topVector;

        /// <summary> Adds all of the colliderObjects to a handy dictionary named collToPart.
        /// Also determines the length of this body part by looking at all of these colliders, thenceby setting _topVector </summary>
        /// <param name="collToPart">The dictionary to set up</param>
        /// <param name="bodyParts">A list of all BodyPart classes</param>
        public void Initialize(IDictionary<Collider2D, BodyPartClass> collToPart, IEnumerable<BodyPartClass> bodyParts) {
            if(_parentPart != null) _parent = bodyParts.First(part => part.bodyPart == _parentPart);
            _rb = bodyPart.GetComponentInParent<Rigidbody2D>();
            Vector3 objPos = bodyPart.transform.position;
            Vector3 farPoint = objPos;
            Collider2D farColl = bodyPart.GetComponent<Collider2D>();
            foreach(GameObject co in _colliderObjects) {
                Collider2D[] colliders = co.GetComponents<Collider2D>();
                foreach(Collider2D collider in colliders) {
                    if(Vector3.Distance(collider.bounds.center, objPos) >= Vector3.Distance(farPoint, objPos)) {
                        farPoint = collider.bounds.center;
                        farColl = collider;
                    }
                    collToPart.Add(collider, this);
                }
            }
            farPoint = Vector3.Distance(farPoint + farColl.bounds.extents, objPos) < Vector3.Distance(farPoint - farColl.bounds.extents, objPos)
                           ? bodyPart.transform.InverseTransformPoint(farPoint - farColl.bounds.extents)
                           : bodyPart.transform.InverseTransformPoint(farPoint + farColl.bounds.extents);
            _topVector = new Vector3(farPoint.x, 0);
        }

        #endregion

        /// <summary> Calculate how much rotation should be added on collision </summary>
        /// <param name="point">Point of contact</param>
        /// <param name="direction">Direction of contact</param>
        /// <param name="rVelocity">Relative Velocity of Collision</param>
        /// <param name="mass">Mass of the colliding object</param>
        public void HitCalc(Vector2 point, Vector2 direction, Vector2 rVelocity, float mass) {
            _shouldHitRot = true; //enable HitRot() to apply rotation
            IsTouching = true; //enable IsTouching() for continuous touching
            _collisionNormal = direction;
            _positionVector = point - (Vector2) bodyPart.transform.position;

            //Determines how much influence collision will have. Ranges from 0 to 1.
            _massMult = 1 / (1 + Mathf.Exp(-((mass / _partStrength - 20) / 20)));

            Vector3 toTop = bodyPart.transform.TransformPoint(_topVector) - bodyPart.transform.position;
            _upDown = Mathf.Clamp(Vector3.Dot(toTop, _positionVector) / Vector3.SqrMagnitude(toTop), -1, 1);

            if(!(Vector3.Dot(direction.normalized, rVelocity.normalized) > 0.01f))
                return; //Makes sure an object sliding away doesn't cause errant rotations
            Vector2 forceVectorPre = Vector2.Dot(rVelocity, direction) * direction * _upDown;
            Vector2 forceVector = _isLeg ? Vector2.Dot(forceVectorPre, Vector2.right) * Vector2.right : forceVectorPre;

            if(_isLeg) {
                Vector2 crouchVector = forceVectorPre - forceVector;
                //TODO: Crouch stuff
            }

            AddTorque(rVelocity, mass, forceVector);
        }

        /// <summary> Calculates the torque to add based on the collision force, and transfers the rest of the force to the parent part </summary>
        /// <param name="rVelocity">Relative Velocity of Collision</param>
        /// <param name="mass">Mass of the colliding object</param>
        /// <param name="forceVector">The vector of force from the collision</param>
        private void AddTorque(Vector2 rVelocity, float mass, Vector2 forceVector) {
            //a vector perpendicular to the force and the vector along the body part, which gives the direction to rotate.
            Vector3 cross = Vector3.Cross(_positionVector, forceVector);

            _torqueAmount += _massMult * forceVector.magnitude * Mathf.Sign(cross.z);

            //force that is parallel to limb or too close to hinge, so can't rotate it, but can be transferred to parent. 	
            Vector2 transForceVec = -0.2f * (rVelocity - forceVector) - _collisionNormal * (forceVector.magnitude - Vector3.Cross(_positionVector, forceVector).magnitude);
            _parent?.HitTransfer(bodyPart.transform.position, rVelocity, transForceVec, mass);
        }

        /// <summary> Transfers force from child to parent </summary>
        /// <param name="point">Point of contact</param>
        /// <param name="rVelocity">Relative Velocity of Collision</param>
        /// <param name="transferredForceVector">The vector of force being transferred</param>
        /// <param name="mass">Mass of the colliding object</param>
        private void HitTransfer(Vector3 point, Vector3 rVelocity, Vector3 transferredForceVector, float mass) {
            _shouldHitRot = true;
            _massMult = Mathf.Exp(-9 / (mass / _partStrength + 2f));
            _positionVector = bodyPart.transform.position - point;
            Vector3 unknownVel = Mathf.Abs(rVelocity.magnitude - _rb.velocity.magnitude) * rVelocity.normalized;
            AddTorque(unknownVel, mass, transferredForceVector);
        }

        /// <summary> Rotates the body part, dispersing the collision torque over time to return to the resting position </summary>
        public void HitRotation() {
            if(!_shouldHitRot) return;

            _rotAmount += _torqueAmount * Time.fixedDeltaTime; //Build up a rotation based on the amount of torque from the collision
            bodyPart.transform.Rotate(Vector3.forward, Mult * _rotAmount, Space.Self);

            _torqueAmount -= _torqueAmount * 3 * Time.fixedDeltaTime; //Over time, reduce the torque added from the collision
            _rotAmount = Extensions.SharpInDamp(_rotAmount, _rotAmount / 2, 0.8f); //and return the body part back to rest

            _shouldHitRot = Mathf.Abs(_rotAmount) * Mult * Mult >= 0.01f; //If the rotation is small enough, stop calling this code
        }

        /// <summary> Adjusts the rotation of this part when rotating into something that it's touching </summary>
        public void HandleTouching() {
            if(!IsTouching || !((FacingRight ? -1 : 1) * Vector3.Dot(_collisionNormal, (PostRight - Right).normalized) > 0.1f)) return;

            float torquePlus = (FacingRight ? 1 : -1) * -2 * Mult * _massMult * _positionVector.magnitude * Vector3.Angle(PostRight, Right) / 5 *
                               Mathf.Sign(Vector3.Cross(_collisionNormal, PostRight).z) * _upDown * (_isLeg ? Vector2.Dot(_collisionNormal, Vector2.right) : 1);

            _torqueAmount += torquePlus * Time.fixedDeltaTime;
            _shouldHitRot = true;
            if(_parent != null) {
                _parent._shouldHitRot = true;
            }
            IsTouching = false;
        }
    }

    /// <summary> Passes info from collision events to the BodyPartClass HitCalc method </summary>
    /// <param name="collInfo">The collision info from the collision event</param>
    private void CollisionHandler(Collision2D collInfo) {
        if(collInfo.gameObject.GetComponent<Rigidbody2D>()) {
            foreach(ContactPoint2D c in collInfo.contacts) {
                if(collToPart.ContainsKey(c.otherCollider)) {
                    BodyPartClass part = collToPart[c.otherCollider];
                    Vector2 force = float.IsNaN(c.normalImpulse) ? collInfo.relativeVelocity : c.normalImpulse / Time.fixedDeltaTime * c.normal / 1000;
                    part.HitCalc(c.point, c.normal, force, collInfo.gameObject.GetComponent<Rigidbody2D>().mass);
                }
            }
        } else {
            foreach(ContactPoint2D c in collInfo.contacts) {
                if(collToPart.ContainsKey(c.otherCollider)) {
                    BodyPartClass part = collToPart[c.otherCollider];
                    Vector2 force = float.IsNaN(c.normalImpulse) ? collInfo.relativeVelocity : c.normalImpulse / Time.fixedDeltaTime * c.normal / 1000;
                    part.IsTouching = true;
                    part.HitCalc(c.point, c.normal, force, 1000);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collInfo) {
        CollisionHandler(collInfo);
    }

    private void OnCollisionStay2D(Collision2D collInfo) {
        CollisionHandler(collInfo);
    }
}