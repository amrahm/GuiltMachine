using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {
    public List<BodyPartClass> bodyParts;
    public Dictionary<Collider2D, BodyPartClass> collToPart = new Dictionary<Collider2D, BodyPartClass>();

    [Tooltip("Reference to Parts script, which contains all of the player's body parts")]
    public PartsAbstract parts;


    private void Awake() {
        foreach(var part in bodyParts) part.Initialize(collToPart, bodyParts);
    }

    private void FixedUpdate() {
        foreach(var part in bodyParts) {
            part.FacingRight = transform.localScale.x > 0;
            part.DirPre = part.bodyPart.transform.TransformDirection(part.partDir.normalized);
            if(part.visPartDir) Debug.DrawRay(part.bodyPart.transform.position, part.DirPre);
        }

        foreach(var part in parts.PartsToTargets.Keys) RotateTo(part, parts.PartsToTargets[part]);

        foreach(var part in bodyParts) {
            part.DirPost = part.bodyPart.transform.TransformDirection(part.partDir.normalized);
            part.HitRotation();
        }
    }

    /// <summary> Rotates the non-animated skeleton to the animated skeleton </summary>
    /// <param name="obj">part from the non-animated skeleton</param>
    /// <param name="target">part from the animated skeleton</param>
    private void RotateTo(GameObject obj, GameObject target) {
#if DEBUG || UNITY_EDITOR
        //Log any errors, since this shouldn't happen
        if(!parts.PartsToLPositions.ContainsKey(obj))
            Debug.LogError($"Trying to rotate {obj.name}, but it wasn't found in{nameof(PlayerParts)}.{nameof(PlayerParts.PartsToLPositions)}");
#endif

        //Reset the local positions cause sometimes they get moved
        obj.transform.localPosition = parts.PartsToLPositions[obj];

        //Match the animation rotation
        obj.transform.rotation = target.transform.rotation;
    }

    [Serializable]
    public class BodyPartClass {
        #region Variables

        [Tooltip("The body part to rotate/control")]
        public GameObject bodyPart;

        [Tooltip("The parent body part of this body part. Can be null.")]
        public GameObject parentPart;

        [Tooltip("A list of all of the objects that contain colliders for this body part")]
        public List<GameObject> colliderObjects = new List<GameObject>();

        [Tooltip("How intensely this part reacts to impacts")]
        public float partWeakness = 65;

        [Tooltip("Is this body part a leg, i.e. should it handle touching the floor differently")]
        public bool isLeg;

        [Tooltip("How fast the leg should crouch from an impact")]
        public float crouchSpeed = 2;

        [Tooltip("A list of all objects that should bend left when crouching, along with an amount from 0 to 1 that they should bend")]
        public List<GameObject> bendLeft = new List<GameObject>();

        [Tooltip("The amount that the corresponding part should rotate from 0 to 1")]
        public List<float> bendLeftAmounts = new List<float>();

        [Tooltip("A list of all objects that should bend right when crouching, along with amount they should bend")]
        public List<GameObject> bendRight = new List<GameObject>();

        [Tooltip("The amount that the corresponding part should rotate to crouch")]
        public List<float> bendRightAmounts = new List<float>();

        [Tooltip("The direction, in local space, that points from the base of this body part to the tip")]
        public Vector2 partDir = Vector2.right;

        [Tooltip("Lets you see the part direction in-game for setting it properly")]
        public bool visPartDir;


        /// <summary> The parent body part class of this body part. Can be null. </summary>
        private BodyPartClass _parent;

        /// <summary> How far up the length of the body part the collision was </summary>
        private float _upDown;

        /// <summary> A vector from the base position of this body part to the point of the collision </summary>
        private Vector3 _positionVector;

        /// <summary> The normal vector of the collision </summary>
        private Vector2 _collisionNormal;

        /// <summary> How much this part is rotating beyond normal </summary>
        private float _rotAmount;

        /// <summary> How much torque is actively being added to rotate this part </summary>
        private float _torqueAmount;

        /// <summary> A list of all other legs that should crouch with this one") </summary>
        private IEnumerable<BodyPartClass> _linkedLegs;

        /// <summary> If this is a leg, how much crouch is being added from an impact </summary>
        private float _crouchPlus;

        /// <summary> If this is a leg, how much should it crouch </summary>
        private float _crouchAmount;

        /// <summary> Should the hit rotation code run? </summary>
        private bool _shouldHitRot;

        /// <summary> The rightward direction of this bodypart after rotating </summary>
        public Vector3 DirPre { get; set; }

        /// <summary> The rightward direction of this bodypart after rotating </summary>
        public Vector3 DirPost { private get; set; }

        /// <summary> Is the player facing right? </summary>
        public bool FacingRight { private get; set; }

        /// <summary> Reference to the rigidbody </summary>
        private Rigidbody2D _rb;

        /// <summary> Vector from the base to the tip of this part </summary>
        private Vector3 _topVector;

        /// <summary> Adds all of the colliderObjects to a handy dictionary named collToPart.
        /// Also determines the length of this body part by looking at all of these colliders, thenceby setting _topVector </summary>
        /// <param name="collToPart">The dictionary to set up</param>
        /// <param name="bodyParts">A list of all BodyPart classes</param>
        public void Initialize(IDictionary<Collider2D, BodyPartClass> collToPart, List<BodyPartClass> bodyParts) {
            if(parentPart != null) _parent = bodyParts.First(part => part.bodyPart == parentPart);
            if(isLeg) _linkedLegs = bodyParts.Where(part => part.isLeg);
            _rb = bodyPart.GetComponentInParent<Rigidbody2D>();
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
        /// <param name="impulse">Impluse of the collision</param>
        public void HitCalc(Vector3 point, Vector2 direction, Vector2 impulse) {
            _shouldHitRot = true; //enable HitRot() to apply rotation
            _collisionNormal = direction;
            _positionVector = point - bodyPart.transform.position; //A vector to the position of the hit

            Vector3 toTop = bodyPart.transform.TransformPoint(_topVector) - bodyPart.transform.position; //A vector to the top of this part
            _upDown = Mathf.Clamp(Vector3.Dot(toTop, _positionVector) / Vector3.SqrMagnitude(toTop), -1, 1); //Clamped in case of errors

            //All of the impulse in the direction of the collision normal
            Vector2 forceVectorPre = impulse * _upDown;
            //If it's a leg, only take the horizontal component
            Vector2 forceVector = isLeg ? Vector2.Dot(forceVectorPre, Vector2.right) * Vector2.right : forceVectorPre;

            if(isLeg) { //Add crouching using the vertical component for legs
                float verticalForce = (forceVectorPre - forceVector).y;
                if(verticalForce / _rb.mass > 0.2f) { //Min threshold so this isn't constantly active
                    foreach(var leg in _linkedLegs) leg._crouchPlus += verticalForce;
                }
            }

            //Add the magnitude of this force to torqueAmount, which will make the part rotate. The cross product gives us the proper direction.
            _torqueAmount += (FacingRight ? 1 : -1) * forceVector.magnitude * Mathf.Sign(Vector3.Cross(_positionVector, forceVector).z);

            HandleTouching();

            //Transfer force that was removed because of a low upDown (+ a bit more) at the hinge of this part in the direction of the impulse to the parent
            _parent?.HitCalc(bodyPart.transform.position, direction, (1.5f - _upDown) * impulse);
        }

        /// <summary> Rotates the body part, dispersing the collision torque over time to return to the resting position </summary>
        public void HitRotation() {
            if(isLeg) CrouchRotation();
            if(!_shouldHitRot) return;

            _rotAmount += _torqueAmount * Time.fixedDeltaTime; //Build up a rotation based on the amount of torque from the collision
            bodyPart.transform.Rotate(Vector3.forward, (FacingRight ? 1 : -1) * partWeakness * _rotAmount / 2, Space.Self); //Rotate the part _rotAmount past where it is animated

            _torqueAmount -= _torqueAmount * 3 * Time.fixedDeltaTime; //Over time, reduce the torque added from the collision
            _rotAmount = Extensions.SharpInDamp(_rotAmount, 7 * _rotAmount / 8, 0.8f, 0.02f, Time.fixedDeltaTime); //and return the body part back to rest

            _shouldHitRot = Mathf.Abs(_rotAmount) * partWeakness >= 0.01f; //If the rotation is small enough, stop calling this code
        }

        /// <summary> Handles Contracting multi-part legs when they hit the ground </summary>
        private void CrouchRotation() {
            if(_crouchAmount < 0.1f && _crouchPlus < 0.1f) return;

            _crouchAmount = Extensions.SharpInDamp(_crouchAmount, _crouchPlus, crouchSpeed, 1, Time.fixedDeltaTime); //Quickly move towards crouchAmount

            //Bend all the bendy parts
            for(int i = 0; i < bendRight.Count; i++)
                bendRight[i].transform.Rotate(Vector3.forward, (FacingRight ? -1 : 1) * _crouchAmount * bendRightAmounts[i], Space.Self);
            for(int i = 0; i < bendLeft.Count; i++)
                bendLeft[i].transform.Rotate(Vector3.forward, (FacingRight ? 1 : -1) * _crouchAmount * bendLeftAmounts[i], Space.Self);

            _crouchPlus = Extensions.SharpInDamp(_crouchPlus, 0, crouchSpeed / 4, 1, Time.fixedDeltaTime); //Over time, reduce the crouch from impact
        }

        /// <summary> Adjusts the rotation of this part when rotating into something that it's touching </summary>
        private void HandleTouching() {
            //TODO Is massMult needed here? partStrength?
            if(-1 * Vector3.Dot(_collisionNormal, (DirPost - DirPre).normalized) < -0.1f) return;

            _torqueAmount += (FacingRight ? -2 : 2) * Time.fixedDeltaTime * _upDown * Vector3.Angle(DirPost, DirPre) *
                             Mathf.Sign(Vector3.Cross(_collisionNormal, DirPre).z) * (isLeg ? Vector2.Dot(_collisionNormal, Vector2.right) : 1);
            _shouldHitRot = true;
        }
    }

    /// <summary> Passes info from collision events to the BodyPartClass HitCalc method </summary>
    /// <param name="collInfo">The collision info from the collision event</param>
    private void CollisionHandler(Collision2D collInfo) {
        foreach(ContactPoint2D c in collInfo.contacts) {
            if(collToPart.ContainsKey(c.otherCollider)) {
                BodyPartClass part = collToPart[c.otherCollider];
                Vector2 force = float.IsNaN(c.normalImpulse) ? collInfo.relativeVelocity : c.normalImpulse / Time.fixedDeltaTime * c.normal / 1000;
                part.HitCalc(c.point, c.normal, force);
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