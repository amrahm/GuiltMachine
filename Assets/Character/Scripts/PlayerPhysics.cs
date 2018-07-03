using System;
using System.Collections.Generic;
using ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {
    [SerializeField] private float _forceMult = 100;

    [UsedImplicitly] public BodyPartClass hitRotTorso, hitRotHead;
    [UsedImplicitly] public BodyPartClass hitRotUpperArmR, hitRotUpperArmL, hitRotLowerArmR, hitRotLowerArmL;
    [UsedImplicitly] public BodyPartClass hitRotThighR, hitRotThighL;
    public Dictionary<Collider2D, BodyPartClass> collToPart = new Dictionary<Collider2D, BodyPartClass>();

    private PlayerMovement _playerMovement; //Reference to PlayerMovement script
    private Parts _parts; //Reference to Parts script


    private void Awake() {
        //References
        _playerMovement = GetComponent<PlayerMovement>();
        _parts = GetComponent<Parts>();


        hitRotTorso = new BodyPartClass(_parts.torso, false, 1.2f, new List<GameObject> {_parts.torso}, collToPart);
        hitRotHead = new BodyPartClass(_parts.head, false, 1.2f, new List<GameObject> {_parts.head}, collToPart, hitRotTorso);
        hitRotUpperArmR = new BodyPartClass(_parts.upperArmR, false, 1f, new List<GameObject> {_parts.upperArmR}, collToPart);
        hitRotUpperArmL = new BodyPartClass(_parts.upperArmL, false, 1f, new List<GameObject> {_parts.upperArmL}, collToPart);
        hitRotLowerArmR = new BodyPartClass(_parts.lowerArmR, false, 1f, new List<GameObject> {_parts.lowerArmR, _parts.handR}, collToPart, hitRotUpperArmR);
        hitRotLowerArmL = new BodyPartClass(_parts.lowerArmL, false, 1f, new List<GameObject> {_parts.lowerArmL, _parts.handL}, collToPart, hitRotUpperArmL);
        hitRotThighR = new BodyPartClass(_parts.thighR, true, 1.2f, new List<GameObject> {_parts.thighR, _parts.shinR, _parts.footR}, collToPart);
        hitRotThighL = new BodyPartClass(_parts.thighL, true, 1.2f, new List<GameObject> {_parts.thighL, _parts.shinL, _parts.footL}, collToPart);
    }

    private void Update() {
//        crouchAmountSmooth = Extensions.SharpInDamp(crouchAmountSmooth, crouchAmountP, 3.0f);
//        crouchAmountP -= crouchAmountP * Time.deltaTime * 2;
//        crouchAmount = crouchAmountSmooth;
        foreach(var part in collToPart.Values) part.StoreForward(part.bodyPart.transform.right, _playerMovement.facingRight);
    }

    private void FixedUpdate() {
        RotateTo(_parts.torso);
        RotateTo(_parts.head);
        RotateTo(_parts.upperArmR);
        RotateTo(_parts.lowerArmR);
        RotateTo(_parts.handR);
        RotateTo(_parts.upperArmL);
        RotateTo(_parts.lowerArmL);
        RotateTo(_parts.handL);
        RotateTo(_parts.thighR);
        RotateTo(_parts.shinR);
        RotateTo(_parts.footR);
        RotateTo(_parts.thighL);
        RotateTo(_parts.shinL);
        RotateTo(_parts.footL);
        RotateTo(_parts.hips);

//        foreach(var part in collToPart.Values) {
//            part.HitRot();
//            part.IsTouching();
//            part.postRight = part.bodyPart.transform.right;
//        }
    }

    private void LateUpdate() {
        foreach(var part in collToPart.Values) {
            part.HitRot();
            part.IsTouching();
            part.postRight = part.bodyPart.transform.right;
        }
    }

    private void RotateTo(GameObject obj) {
        //Reset the local positions cause sometimes they get moved
        if(!_parts.partsToLPositions.ContainsKey(obj)) {
            Debug.Log(obj.name);
        }
        obj.transform.localPosition = _parts.partsToLPositions[obj];
    }

    [Serializable]
    public class BodyPartClass {
        #region Variables

        [UsedImplicitly] public bool testMode;
        public readonly GameObject bodyPart;
        private readonly BodyPartClass _parentPart;
        private readonly float _partStrength; //1 is standard, 2 is twice as strong
        private float _upDown;
        private float _massMult;
        private Vector3 _positionVector;
        [SerializeField] private Vector2 _tForceVector, _collisionNormal;
        [SerializeField] private float _rotAmount, _torqueAmount;

        [SerializeField] private bool _shouldHitRot;
        public bool isTouching;
        [SerializeField] private readonly bool _isLeg;
        public Vector3 postRight;
        private Vector3 _right;
        private bool _facingRight;
        private readonly Rigidbody2D _rb;
        private const float Mult = 25f;
        private Vector3 _topVector;

        public void StoreForward(Vector3 right, bool facingRight) {
            _right = right;
            _facingRight = facingRight;
        }

        public BodyPartClass(GameObject bodyPart, bool isLeg, float partStrength, IEnumerable<GameObject> colliderObjects,
            IDictionary<Collider2D, BodyPartClass> collToPart, BodyPartClass parent = null) {
            this.bodyPart = bodyPart;
            _rb = bodyPart.GetComponentInParent<Rigidbody2D>();
            _partStrength = partStrength;
            _isLeg = isLeg;
            _parentPart = parent;
            FindBounds(colliderObjects, collToPart);
        }

        private void FindBounds(IEnumerable<GameObject> colliderObjects, IDictionary<Collider2D, BodyPartClass> collToPart) {
            Vector3 objPos = bodyPart.transform.position;
            Vector3 farPoint = objPos;
            Collider2D farColl = null;
            foreach(GameObject co in colliderObjects) {
                Collider2D[] colliders = co.GetComponents<Collider2D>();
                foreach(Collider2D collider in colliders) {
                    if(Vector3.Distance(collider.bounds.center, objPos) >= Vector3.Distance(farPoint, objPos)) {
                        farPoint = collider.bounds.center;
                        farColl = collider;
                    }
                    collToPart.Add(collider, this);
                }
            }
            System.Diagnostics.Debug.Assert(farColl != null, nameof(farColl) + " != null");
            farPoint = Vector3.Distance(farPoint + farColl.bounds.extents, objPos) < Vector3.Distance(farPoint - farColl.bounds.extents, objPos)
                           ? bodyPart.transform.InverseTransformPoint(farPoint - farColl.bounds.extents)
                           : bodyPart.transform.InverseTransformPoint(farPoint + farColl.bounds.extents);
            _topVector = new Vector3(farPoint.x, 0);
        }

        #endregion

        /// <summary> Calculate how much rotation should be added on collision. </summary>
        /// <param name="point">Point of contact</param>
        /// <param name="direction">Direction of contact</param>
        /// <param name="rVelocity">Relative Velocity of Collision</param>
        /// <param name="mass">Mass of colliding object</param>
        public void HitCalc(Vector2 point, Vector2 direction, Vector2 rVelocity, float mass) {
            _shouldHitRot = true; //enable HitRot() to apply rotation
            isTouching = true; //enable IsTouching() for continuous touching
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

//            if(bodyPart.name == "ThighL") {
//                Debug.Log(forceVector);
//                Debug.DrawRay(bodyPart.transform.position, toTop);
//            }

            AddTorque(rVelocity, mass, forceVector);
        }

        private void AddTorque(Vector2 rVelocity, float mass, Vector2 forceVector) {
            //a vector perpendicular to the normal force and the bodyPart's forward vector to use during rotation. This is torque = r X F
            Vector3 cross = Vector3.Cross(_positionVector, forceVector);

            _torqueAmount += _massMult * forceVector.magnitude * Mathf.Sign(cross.z);

            //force that is parallel to limb or too close to hinge, so can't rotate it, but can be transferred to parent. 	
            _tForceVector = -0.2f * (rVelocity - forceVector) - _collisionNormal * (forceVector.magnitude - Vector3.Cross(_positionVector, forceVector).magnitude);
            _parentPart?.HitTransfer(bodyPart.transform.position, rVelocity, _tForceVector, mass);
        }

        private void HitTransfer(Vector3 point, Vector3 rVelocity, Vector3 transferredForceVector, float mass) {
            _shouldHitRot = true;
            _massMult = Mathf.Exp(-9 / (mass / _partStrength + 2f));
            _positionVector = bodyPart.transform.position - point;
            Vector3 unknownVel = Mathf.Abs(rVelocity.magnitude - _rb.velocity.magnitude) * rVelocity.normalized;
            AddTorque(unknownVel, mass, transferredForceVector);
        }

        public void HitRot() { //Then apply the rotations calculated
            if(!_shouldHitRot && !testMode) return;

            _rotAmount += _torqueAmount * Time.deltaTime;
            bodyPart.transform.Rotate(Vector3.forward, Mult * _rotAmount, Space.Self);

            if(!testMode) _rotAmount = Extensions.SharpInDamp(_rotAmount, _rotAmount / 2, 0.8f);
            _torqueAmount -= _torqueAmount * 3 * Time.deltaTime;

            _shouldHitRot = Mathf.Abs(_rotAmount) * Mult * Mult >= 0.01f;
        }

        public void IsTouching() { //adjust rotation when rotating into something
            if(!isTouching || !((_facingRight ? -1 : 1) * Vector3.Dot(_collisionNormal, (postRight - _right).normalized) > 0.1f)) return;

            float torquePlus = (_facingRight ? 1 : -1) * -2 * Mult * _massMult * _positionVector.magnitude * Vector3.Angle(postRight, _right) / 5 *
                               Mathf.Sign(Vector3.Cross(_collisionNormal, postRight).z) * _upDown * (_isLeg ? Vector2.Dot(_collisionNormal, Vector2.right) : 1);

            _torqueAmount += torquePlus * Time.deltaTime;
            _shouldHitRot = true;
            if(_parentPart != null) {
                _parentPart._shouldHitRot = true;
            }
            isTouching = false;
        }
    }

    /// <summary> Passes info from collision events to the BodyPartClass HitCalc method </summary>
    /// <param name="collInfo">The collision info from the collision event</param>
    private void CollisionHandler(Collision2D collInfo) {
        if(collInfo.gameObject.GetComponent<Rigidbody2D>()) {
            foreach(ContactPoint2D c in collInfo.contacts) {
                if(collToPart.ContainsKey(c.otherCollider)) {
                    BodyPartClass part = collToPart[c.otherCollider];
                    part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, collInfo.gameObject.GetComponent<Rigidbody2D>().mass);
                }
            }
        } else {
            foreach(ContactPoint2D c in collInfo.contacts) {
                if(collToPart.ContainsKey(c.otherCollider)) {
                    BodyPartClass part = collToPart[c.otherCollider];
                    part.isTouching = true;
                    part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, 1000);
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