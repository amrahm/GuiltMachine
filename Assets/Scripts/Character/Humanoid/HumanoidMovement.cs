using System;
using System.Collections;
using ExtensionMethods;
using UnityEngine;
using static UnityEngine.Physics2D;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HumanoidParts))]
public class HumanoidMovement : MovementAbstract {
    private const string RollStateTag = "Roll";
    private const float GrabAdd = 0.1f;
    private const float TimeToGrab = 0.15f;

    #region Variables

#if UNITY_EDITOR
    [Tooltip("Show debug visualizations")] [SerializeField]
    private bool _visualizeDebug;


    [Tooltip("Debug control to allow multiple jumps")] [SerializeField]
    private bool _allowJumpingInMidair;
#endif

    [Tooltip("The fastest the character can travel in the x axis")] [SerializeField]
    private float _maxSpeed = 10f;

    [Tooltip("How fast the character speeds up")] [SerializeField]
    private float _acceleration = 1;

    [Tooltip("A kick to make the character start moving faster")] [SerializeField]
    private float _kick = 1;

    [Tooltip("How much character automatically slows down while not walking but on the ground")] [SerializeField]
    private float _groundSlowdownMultiplier;

    [Tooltip("Sprint multiplier for when running")] [SerializeField]
    private float _sprintSpeed = 2;

    [Tooltip("Vertical speed of a jump")] [SerializeField]
    private float _jumpSpeed = 5;

    [Tooltip("Amount of time a jump can be held to jump higher")] [SerializeField]
    private float _jumpFuel = 100f;

    [Tooltip("Additional force added while holding jump")] [SerializeField]
    private float _jumpFuelForce = 30f;

    [Tooltip("How much character can steer while in mid-air")] [SerializeField]
    private float _airControl;

    [Tooltip("How much character automatically slows down while in mid-air")] [SerializeField]
    private float _airSlowdownMultiplier;

    [Tooltip("A kick to make the character start moving faster while in mid-air")] [SerializeField]
    private float _kickAir;

    [Tooltip("Offset for the back of the right foot ground check raycast")] [SerializeField]
    private Vector2 _groundCheckOffsetR;

    [Tooltip("Offset for the front of the right foot ground check raycast")] [SerializeField]
    private Vector2 _groundCheckOffsetR2 = new Vector2(0.15f, 0);

    [Tooltip("Offset for the back of the left foot ground check raycast")] [SerializeField]
    private Vector2 _groundCheckOffsetL;

    [Tooltip("Offset for the front of the left foot ground check raycast")] [SerializeField]
    private Vector2 _groundCheckOffsetL2 = new Vector2(0.15f, 0);

    [Tooltip("How long is the ground check raycast")] [SerializeField]
    private float _groundCheckDistance = 0.15f;

    [Tooltip("How far to check down when rolling")] [SerializeField]
    private float _rollingGroundCheckDistance = 1.3f;

    [Tooltip("How long is the grab check raycast")] [SerializeField]
    private float _grabDistance = 1;

    [Tooltip("How far along the horizontal check is the downward check")] [SerializeField]
    private float _grabDownDistMult = 0.7f;

    [Tooltip("How high is the highest grabbable")] [SerializeField]
    private float _grabTopOffset = 0.65f;

    [Tooltip("How high is the middle grabbable")] [SerializeField]
    private float _grabMidOffset;

    [Tooltip("How high is the lowest grabbable")] [SerializeField]
    private float _grabBottomOffset = -0.75f;

    [Tooltip("How wide a radius vertically needs to be clear to initiate a grab")] [SerializeField]
    private float _grabCheckRadiusV = 0.3f;

    [Tooltip("How far vertically beyond the grab point needs to be clear to initiate a grab")] [SerializeField]
    private float _grabCheckDistanceV = 1f;

    [Tooltip("How wide a radius horizontally needs to be clear to initiate a grab")] [SerializeField]
    private float _grabCheckRadiusH = 0.4f;

    [Tooltip("How far horizontally beyond the grab point needs to be clear to initiate a grab")] [SerializeField]
    private float _grabCheckDistanceH = 0.2f;

    [Header("Movement Abilities")]
    [Tooltip(" >= 1 gives the character the ability to dash-jump in mid-air this many times")]
    [SerializeField]
    private int _numberOfAirDashes;

    [Tooltip("How fast/far the air-dash goes")]
    [SerializeField]
    private float _airDashSpeed = 1;

    [Tooltip("The particle effect to spawn when air-dashing")] [SerializeField]
    private GameObject _airDashParticleEffect;


    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the character is currently grabbing </summary>
    private bool _grabbing;

    /// <summary> Distance out to do downward check </summary>
    private float _grabDownDist;

    /// <summary> How long since the character last left grab mode </summary>
    private float _timeSinceRelease;

    /// <summary> Whether or not the character is crouching </summary>
    private bool _crouching;

    /// <summary> Whether or not the character is currently rolling </summary>
    private bool _rolling;

    /// <summary> How long the character has been rolling </summary>
    private float _rollingTime;

    /// <summary> Which direction the character rolled </summary>
    private float _rollDir;

    /// <summary> Position of the foot last frame when crouching </summary>
    private float _lastHipsDelta;

    /// <summary> Used to zero out friction when moving. True if the character's feet have no friction. </summary>
    private bool _frictionZero;

    /// <summary> How much jump fuel is left. Starts at _jumpFuel and moves to 0 </summary>
    private float _jumpFuelLeft;

    /// <summary> How long has the character been falling? </summary>
    private float _fallDuration;

    /// <summary> True if the character is falling </summary>
    private bool _falling;

    /// <summary> True if the character is still holding jump after jumping </summary>
    private bool _jumpStarted;

    /// <summary> True if the character was holding jump on the previous frame </summary>
    private bool _wasJump;

    /// <summary> True if the character pressed jump while rolling </summary>
    private bool _wantsToRollJump;

    /// <summary> A vector used to help smoothly rotate the character during an air dash </summary>
    private Vector3 _airDashVec;

    /// <summary> True if the character is currently air dashing </summary>
    private bool _airDashing;

    /// <summary> How many more air dashes the character can do in mid-air </summary>
    private int _airDashesLeft;

    /// <summary> Whether or not the character is touching something </summary>
    private bool _isTouching;

    /// <summary> Reference to Parts script, which contains all of the character's body parts </summary>
    private HumanoidParts _parts;

    /// <summary> The physics material on the foot colliders </summary>
    private PhysicsMaterial2D _footFrictionMat;

    /// <summary> The speed float in the animator </summary>
    private int _speedAnim;

    /// <summary> The vertical speed float in the animator </summary>
    private int _vSpeedAnim;

    /// <summary> The crouching bool in the animator </summary>
    private int _crouchingAnim;

    /// <summary> The roll bool in the animator </summary>
    private int _rollAnim;

    /// <summary> The jump trigger in the animator </summary>
    private int _jumpAnim;

    /// <summary> The roll jump trigger in the animator </summary>
    private int _rollJumpAnim;

    /// <summary> The land bool in the animator </summary>
    private int _groundedAnim;

    /// <summary> The falling bool in the animator </summary>
    private int _fallingAnim;

    /// <summary> The climb trigger in the animator </summary>
    private int _climbAnim;

    /// <summary> The climb speed float in the animator </summary>
    private int _climbAnimSpeed;

    /// <summary> The air dash trigger in the animator </summary>
    private int _airDashAnim;

    #endregion

    protected override void Awake() {
        // Setting up references.
        base.Awake();
        _parts = GetComponent<HumanoidParts>();

        whatIsGround = whatIsGroundMaster.whatIsGround & ~(1 << gameObject.layer); // remove current layer

        // Instance the foot physics material so that we can adjust it without affecting other users of the same material
        var footMat = _parts.footR.GetComponent<Collider2D>().sharedMaterial;
        _footFrictionMat = new PhysicsMaterial2D(footMat.name + " (Instance)") {friction = footMat.friction};
        _parts.footR.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;
        _parts.footL.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;

        // Initialize animator parameters
        _speedAnim = Animator.StringToHash("Speed");
        _vSpeedAnim = Animator.StringToHash("vSpeed");
        _crouchingAnim = Animator.StringToHash("Crouching");
        _rollAnim = Animator.StringToHash("Roll");
        _jumpAnim = Animator.StringToHash("Jump");
        _rollJumpAnim = Animator.StringToHash("RollJump");
        _groundedAnim = Animator.StringToHash("Grounded");
        _fallingAnim = Animator.StringToHash("Falling");
        _climbAnim = Animator.StringToHash("Climb");
        _climbAnimSpeed = Animator.StringToHash("ClimbSpeed");
        _airDashAnim = Animator.StringToHash("AirDash");

        _grabDownDist = _grabDistance * _grabDownDistMult;
    }

    private void FixedUpdate() {
        UpdateGrounded();
        if(!_rolling) Move(control.moveHorizontal, control.hPressed, control.sprint);
        Jump(control.upPressed);
        Crouch(control.downPressed);

        if(grounded && !_grabbing) {
            // When the feet move up relative to the hips, move the character down so that 
            // the feet stay on the ground instead of lifting into the air
            float hipsDelta = Vector2.Dot(_parts.hips.transform.position - _parts.footR.transform.position, tf.up);
            tf.position += tf.up * (hipsDelta - _lastHipsDelta) / 2;
            _lastHipsDelta = hipsDelta;
        }

#if UNITY_EDITOR
        if(_visualizeDebug) { //Visualize grab raycasts
            Debug.DrawRay(tf.TransformPoint(new Vector2(0, _grabMidOffset + GrabAdd)),
                          (facingRight ? tf.right : -tf.right) * _grabDistance, new Color(0.52f, 1f, 0.52f));
            Debug.DrawRay(tf.TransformPoint(new Vector2(_grabDownDist, _grabTopOffset)),
                          -tf.up * (_grabTopOffset - _grabBottomOffset), new Color(0.38f, 0.72f, 0.38f));
            if(_grabbing) DebugExtension.DebugPoint(_parts.armRIK.Target().position, Color.green);
        }
#endif
    }

    #region GrabLedgeStuff

    /// <summary> Handle if this character is grabbing a platform </summary>
    private IEnumerator GrabHandle(Vector3 grabPoint) {
        float timeBeenGrabbing = 0;
        Vector3 handRStart = _parts.armRIK.Target().position;
        Vector3 handLStart = _parts.armLIK.Target().position;
        while(_grabbing) {
            Vector3 right = facingRight ? tf.right : -tf.right;

            const float grabMaxVelocityThreshold = 3f;
            float dT = Time.fixedDeltaTime;

            // Move the head to a little above halfway between the two points
            // Move the head further down if the character can't reach their IK target, which should let them reach it
            float handIKErr = Vector2.Distance(_parts.armRIK.Target().position, _parts.handRTarget.transform.position);
            Vector3 handIKMidPoint = (_parts.armRIK.Target().position + _parts.armLIK.Target().position) / 2;
            Vector3 headIKPos = handIKMidPoint + tf.up * (1 - handIKErr);
            _parts.headIK.Target().position = _parts.headIK.Target().position.SharpInDamp(headIKPos, 5, dT);

            timeBeenGrabbing += dT;
            if(timeBeenGrabbing < TimeToGrab) {
                // Move the hands from the start to the grab point, and fade in the head IK
                _parts.armRIK.Target().position = Vector3.Lerp(handRStart, grabPoint, timeBeenGrabbing / TimeToGrab);
                _parts.armLIK.Target().position = Vector3.Lerp(handLStart, grabPoint + right * GrabAdd,
                                                               timeBeenGrabbing / TimeToGrab);
                _parts.headIK.weight = Mathf.Lerp(0, 1, timeBeenGrabbing / TimeToGrab);
                rb.velocity = rb.velocity.SharpInDamp(Vector2.zero, 1, dT); // and slow down the velocity some
            } else if(Mathf.Abs(rb.velocity.y) < grabMaxVelocityThreshold) { // If the velocity is slow enough:
                //FIXME remove velocity threshold? Not sure the physics engine is stable enough for this to not cause problems.
                // Make sure the IKs are all at their final positions
                _parts.armRIK.Target().position = grabPoint;
                _parts.armLIK.Target().position = grabPoint + right * GrabAdd;
                _parts.headIK.weight = 1;

                // Raycast check from either foot (whichever hits) to the wall
                //TODO how to deal with thin floating ledges, like this one:    ————————
                Vector3 shift = -tf.up * GrabAdd;
                var sideCheck = Raycast(_parts.footR.transform.position + shift, right, _grabDistance, whatIsGround);
#if UNITY_EDITOR
                if(_visualizeDebug && sideCheck)
                    Debug.DrawRay(_parts.footR.transform.position, right * _grabDistance, Color.cyan);
#endif
                if(!sideCheck) {
                    sideCheck = Raycast(_parts.footL.transform.position + shift, right, _grabDistance, whatIsGround);
#if UNITY_EDITOR
                    if(_visualizeDebug)
                        Debug.DrawRay(_parts.footL.transform.position, right * _grabDistance, Color.cyan);
#endif
                }

                // Add a force to keep the character pressed against the wall
                float wallDist = sideCheck ? Vector2.Distance(_parts.footR.transform.position, sideCheck.point) : 0.5f;
                Vector2 keepAgainstWall = right * .01f * Mathf.Pow(wallDist * 4, 2);
                // And a force to move them up or down based on input
                float hIn = control.moveHorizontal * (facingRight ? 1 : -1);
                Vector2 climbControl = (Vector2) tf.up * 3 * dT * Mathf.Min(hIn + control.moveVertical, 1f);
                rb.MovePosition(rb.position + keepAgainstWall + climbControl);

                anim.SetFloat(_climbAnimSpeed, hIn + control.moveVertical);

                // If neither foot is on the wall, or the feet have moved past the grab point, release
                bool aboveLedge = !sideCheck || Vector2.Dot(sideCheck.point, right) > Vector2.Dot(grabPoint, right);
                if(aboveLedge || hIn < -0.2f) GrabRelease(); // also release if character moves away from wall
            } else {
                // If the velocity was too fast, just release
                GrabRelease();
            }
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary> Update whether or not this character can grab a platform </summary>
    private void GrabSetPoint(Vector2 hitNormal, Vector2 hitPoint) {
        if(_grabbing) return;
        //Ensure character isn't grabbing and is in mid-air or about to be, and is trying to move into the hit platform
        if(_grabbing || grounded && !_jumpStarted ||
           Vector2.Dot(control.moveHorizontal * hitNormal, tf.right) > -0.1f) return;

        Vector2 right = facingRight ? tf.right : -tf.right;
        Vector2 midPoint = tf.TransformPoint(new Vector2(0, tf.InverseTransformPoint(hitPoint).y + GrabAdd));
        RaycastHit2D grabMid = Raycast(midPoint, right, _grabDistance, whatIsGround);
#if UNITY_EDITOR
        if(_visualizeDebug) Debug.DrawRay(midPoint, right * _grabDistance, Color.magenta);
#endif

        // First check for mid, first at the point of contact, and if that's null, then the normal mid check
        if(grabMid || (grabMid = Raycast(tf.TransformPoint(new Vector2(0, _grabMidOffset + GrabAdd)), right,
                                         _grabDistance, whatIsGround))) {
            Vector2 downOrigin = tf.TransformPoint(new Vector2(grabMid.distance + GrabAdd, _grabTopOffset));
            RaycastHit2D down = Raycast(downOrigin, -tf.up, _grabTopOffset - _grabBottomOffset, whatIsGround);
            if(down && GrabCheckIfClear(down.point, right)) GrabBegin(down.point);
            return;
        }

        // If mid fails, check for down.
        Vector2 grabDownOrigin = tf.TransformPoint(new Vector2(_grabDownDist, _grabTopOffset));
        RaycastHit2D grabDown = Raycast(grabDownOrigin, -tf.up, _grabTopOffset - _grabBottomOffset, whatIsGround);
        if(grabDown) {
            Vector2 grabMidOrigin = tf.TransformPoint(new Vector2(0, _grabTopOffset - grabDown.distance - GrabAdd));
            grabMid = Raycast(grabMidOrigin, right, _grabDistance, whatIsGround);
            bool pointNotInGround = Vector2.Distance(grabMid.point, grabMidOrigin) > 0.1f;
            if(pointNotInGround && grabMid && GrabCheckIfClear(midPoint, right))
                GrabBegin(grabMid.point + right * GrabAdd + (Vector2) tf.up * GrabAdd);
        }
    }

    /// <summary> Used to start grabbing </summary>
    private void GrabBegin(Vector2 point) {
        if(Time.time - _timeSinceRelease < 0.5f) return;
        _grabbing = true;
        _parts.handR.GetComponent<Collider2D>().isTrigger = true;
        _parts.handL.GetComponent<Collider2D>().isTrigger = true;
        _parts.lowerArmR.GetComponent<Collider2D>().isTrigger = true;
        _parts.lowerArmL.GetComponent<Collider2D>().isTrigger = true;
        _parts.upperArmR.GetComponent<Collider2D>().isTrigger = true;
        _parts.upperArmL.GetComponent<Collider2D>().isTrigger = true;
        _parts.armRIK.gameObject.SetActive(true);
        _parts.armLIK.gameObject.SetActive(true);
        _parts.headIK.gameObject.SetActive(true);
        _parts.armRIK.weight = _parts.armLIK.weight = 1;
        _parts.headIK.weight = 0;
        _parts.armRIK.Target().position = _parts.handR.transform.position;
        _parts.armLIK.Target().position = _parts.handL.transform.position;
        anim.SetBool(_climbAnim, true);
        rb.gravityScale = 0f;
        StartCoroutine(GrabHandle(point));
    }

    /// <summary> Used to set end of grab </summary>
    private void GrabRelease() {
        _timeSinceRelease = Time.time;
        _grabbing = false;
        _parts.handR.GetComponent<Collider2D>().isTrigger = false;
        _parts.handL.GetComponent<Collider2D>().isTrigger = false;
        _parts.lowerArmR.GetComponent<Collider2D>().isTrigger = false;
        _parts.lowerArmL.GetComponent<Collider2D>().isTrigger = false;
        _parts.upperArmR.GetComponent<Collider2D>().isTrigger = false;
        _parts.upperArmL.GetComponent<Collider2D>().isTrigger = false;
        anim.SetBool(_climbAnim, false);
        rb.gravityScale = 1f;

        StartCoroutine(GrabIKReleaseHelper());

        //TODO check if this is a pull up release or a cancel release
        StartCoroutine(GrabPullUp());
    }

    /// <summary> Used to finish the grab by moving the character to on top of the ledge </summary>
    private IEnumerator GrabPullUp() {
        float timeBeenReleased = 0;
        _parts.footR.GetComponent<Collider2D>().isTrigger = true;
        _parts.footL.GetComponent<Collider2D>().isTrigger = true;

        while(timeBeenReleased < TimeToGrab) {
            //FIXME is this frame rate independent?
            rb.AddForce((facingRight ? tf.right : -tf.right) * 15 * rb.mass);
            timeBeenReleased = Time.time - _timeSinceRelease;

            yield return new WaitForFixedUpdate();
        }

        _parts.footR.GetComponent<Collider2D>().isTrigger = false;
        _parts.footL.GetComponent<Collider2D>().isTrigger = false;
    }

    /// <summary> Used to gradually release from IK pose </summary>
    private IEnumerator GrabIKReleaseHelper() {
        float timeBeenReleased = 0;
        while(timeBeenReleased < TimeToGrab) {
            timeBeenReleased = Time.time - _timeSinceRelease;
            float ikWeight = Mathf.Lerp(1, 0, timeBeenReleased / TimeToGrab);
            _parts.armRIK.weight = _parts.armLIK.weight = _parts.headIK.weight = ikWeight;
            yield return null;
        }
        _parts.armRIK.gameObject.SetActive(false);
        _parts.armLIK.gameObject.SetActive(false);
        _parts.headIK.gameObject.SetActive(false);
    }

    /// <summary> Checks if its clear to grab by circlecasting up and to the side </summary>
    private bool GrabCheckIfClear(Vector2 grabPoint, Vector2 right) {
        // Convert the grab point to local coordinates so we can use its x and y without worrying about rotated gravity
        Vector2 grabPointLocal = tf.InverseTransformPoint(grabPoint);

        // Check up with a circle cast that starts at the character's mid-height, offset from the wall horizontally
        Vector2 upCheckStart = tf.TransformPoint(new Vector2(grabPointLocal.x - GrabAdd * 2 - _grabCheckRadiusV, 0));
        float upCheckDist = grabPointLocal.y + _grabCheckDistanceV;
        RaycastHit2D upCheck = CircleCast(upCheckStart, _grabCheckRadiusV, tf.up, upCheckDist, whatIsGround);

        // Check sideways starting from the character's x position, offset above the top of the ledge
        Vector2 sideCheckStart = tf.TransformPoint(new Vector2(0, grabPointLocal.y + GrabAdd + _grabCheckRadiusH));
        float sideCheckDist = grabPointLocal.x + _grabCheckDistanceH;
        RaycastHit2D sideCheck = Raycast(sideCheckStart, right, sideCheckDist, whatIsGround);
        // And do two more sideways checks so that the three raycasts span grabCheckRadiusH distance
        // Which should ensure that at least that distance is clear above the ledge
        Vector2 upRadH = (Vector2) tf.up * _grabCheckRadiusH;
        if(!sideCheck)
            sideCheck = Raycast(sideCheckStart + upRadH, right, sideCheckDist, whatIsGround);
        if(!sideCheck)
            sideCheck = Raycast(sideCheckStart - upRadH, right, sideCheckDist, whatIsGround);

        //TODO if upCheck clear and side check not, we might still be able to pull up with a (different?) animation,
        //TODO or we can hang on the ledge, based on how far until horizontal check hits
#if UNITY_EDITOR
        if(_visualizeDebug) {
            Debug.DrawRay(upCheckStart, tf.up * upCheckDist, Color.red);
            Debug.DrawRay(sideCheckStart, right * sideCheckDist, Color.red);
            Debug.DrawRay(sideCheckStart + upRadH, right * sideCheckDist, Color.red);
            Debug.DrawRay(sideCheckStart - upRadH, right * sideCheckDist, Color.red);
        }
#endif
        return !upCheck && !sideCheck;
    }

    #endregion

    /// <summary> Update whether or not this character is touching the ground </summary>
    private void UpdateGrounded() {
        bool wasGrounded;
        if(!_rolling) {
            /* This checks both feet to see if they are touching the ground, and if they are, it checks the angle of the ground they are on.
             * For each foot, a raycast checks the back of the foot, and if that hits nothing, then the front of the foot is checked.
             * Then, for any feet that are touching the ground, we check the angle of the ground, and use the larger angle, as long as it's
             * still less than the maxWalkSlope. This is so that uneven surfaces will still be walked up quickly.
             * walkSlope is set to the chosen angle, and is used in the Move(...) method. */
            RaycastHit2D rightHit = Raycast(_parts.footR.transform.TransformPoint(_groundCheckOffsetR), -tf.up,
                                            _groundCheckDistance, whatIsGround);
            if(!rightHit)
                rightHit = Raycast(_parts.footR.transform.TransformPoint(_groundCheckOffsetR2), -tf.up,
                                   _groundCheckDistance, whatIsGround);
            RaycastHit2D leftHit = Raycast(_parts.footL.transform.TransformPoint(_groundCheckOffsetL), -tf.up,
                                           _groundCheckDistance, whatIsGround);
            if(!leftHit)
                leftHit = Raycast(_parts.footL.transform.TransformPoint(_groundCheckOffsetL2), -tf.up,
                                  _groundCheckDistance, whatIsGround);
            float rightAngle = Vector2.Angle(rightHit.normal, tf.up);
            float leftAngle = Vector2.Angle(leftHit.normal, tf.up);

            // Pick the larger angle that is still within bounds
            bool rightGreater = rightAngle > leftAngle && rightAngle < maxWalkSlope || leftAngle > maxWalkSlope;
            groundNormal = rightGreater && rightHit ? rightHit.normal : (leftHit ? leftHit.normal : (Vector2) tf.up);
            walkSlope = rightGreater ? rightAngle : leftAngle;

            wasGrounded = grounded;
            grounded = (rightHit || leftHit) && walkSlope < maxWalkSlope;
        } else { // Rolling
            // If the character is rolling, their feet will leave the ground, but we still want to consider them as touching
            // the ground, so we instead do a raycast out from the center, and set grounded and walkSlope using this.
            RaycastHit2D rollHit = Raycast(tf.position, -tf.up, _rollingGroundCheckDistance, whatIsGround);
            groundNormal = rollHit.collider != null ? rollHit.normal : (Vector2) tf.up;
            walkSlope = Vector2.Angle(rollHit.normal, tf.up);
            wasGrounded = grounded;
            grounded = rollHit && walkSlope < maxWalkSlope;
        }
        // Set this so the animator can play or transition to/from the appropriate animations
        anim.SetBool(_groundedAnim, grounded);
        // If the character was grounded and isn't anymore, but they didn't jump, then they must have walked off a ledge or something
        if(wasGrounded && !grounded && !_jumpStarted) _falling = true;
        if(_falling && !(_rolling || _grabbing)) {
            // If they are falling for long enough (but not still rolling), set the fall animation
            _fallDuration += Time.fixedDeltaTime;
            if(_fallDuration > 0.15f) anim.SetBool(_fallingAnim, true); // falling without jumping
        }
        if(grounded) {
            // Reset all of those things when they aren't falling
            _fallDuration = 0;
            _falling = false;
            anim.SetBool(_fallingAnim, false);
        }


#if UNITY_EDITOR
        if(_visualizeDebug) {
            if(!_rolling) {
                Debug.DrawRay(_parts.footR.transform.TransformPoint(_groundCheckOffsetR),
                              -tf.up * _groundCheckDistance);
                Debug.DrawRay(_parts.footR.transform.TransformPoint(_groundCheckOffsetR2),
                              -tf.up * _groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(_groundCheckOffsetL),
                              -tf.up * _groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(_groundCheckOffsetL2),
                              -tf.up * _groundCheckDistance);
            } else {
                Debug.DrawRay(tf.position, -tf.up * _rollingGroundCheckDistance);
            }
        }
#endif
    }

    /// <summary> Handles character walking and running </summary>
    /// <param name="moveIn">Walking input</param>
    /// <param name="movePressed">Whether walking input is pressed</param>
    /// <param name="sprint"> Whether sprint input is pressed </param>
    private void Move(float moveIn, bool movePressed, bool sprint) {
        //Calculate the tangent to the ground so that the player can walk smoothly up and down slopes
        Vector2 tangent = grounded ? Vector3.Cross(groundNormal, Vector3.forward) : tf.right;
        float velForward = Vector2.Dot(rb.velocity, tf.right); // Use this so that running down slopes is faster
        float velTangent = Vector2.Dot(rb.velocity, tangent);  // And use this so that the animation looks right
#if UNITY_EDITOR
        if(_visualizeDebug) Debug.DrawRay(_parts.footL.transform.position, tangent, Color.blue);
#endif
        float slopeReducer = Mathf.Lerp(1, .7f, walkSlope / maxWalkSlope); // reduce speed as slopes increase
        moveVec = slopeReducer * rb.mass * tangent * _acceleration * moveIn * Time.fixedDeltaTime;

        void AddKick(float force) { // If walking from standstill, gives a kick so walking is more responsive
            Vector2 kickAdd = rb.mass * tangent * force * 30 * slopeReducer;
            if(moveIn > 0 && velForward < _maxSpeed / 3) rb.AddForce(kickAdd);
            else if(moveIn < 0 && velForward > -_maxSpeed / 3) rb.AddForce(-kickAdd);
        }

        // checks if intention is not same direction as motion
        bool moveDirIsNotVelDir = Math.Sign(moveIn) != Math.Sign(velForward);

        if(grounded) {
            float sprintAmt = sprint ? _sprintSpeed : 1;
            moveVec *= sprintAmt; // We do this here because we don't want them to be able to start sprinting in mid-air

            // Set animation params
            _walkSprint = Mathf.Abs(velTangent) <= _maxSpeed + 1f ?
                              Mathf.Abs(velTangent) / _maxSpeed / 2 :
                              Mathf.Abs(velTangent) / (_maxSpeed * _sprintSpeed);
            // avg it with player intention
            _walkSprint = (_walkSprint + Mathf.Abs(moveIn / 2 * _sprintSpeed * slopeReducer)) / 2;
            // avg it for smoothing
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(_walkSprint, 2f, Time.fixedDeltaTime));

            //Check that the character wants to walk, but isn't walking too fast
            if(movePressed && (Mathf.Abs(velForward) < _maxSpeed * sprintAmt || moveDirIsNotVelDir)) {
                rb.AddForce(moveVec, ForceMode2D.Impulse);
                if(!_frictionZero) {
                    // zero out friction so that movement is smooth
                    _footFrictionMat.friction = 0;
                    _frictionZero = true;
                }
            } else {
                if(_frictionZero) {
                    // reset friction to normal
                    _footFrictionMat.friction = 1;
                    _frictionZero = false;
                }
                // and slow the character down so that movement isn't all slidey
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * _groundSlowdownMultiplier;
            }

            AddKick(_kick);

            if((moveIn > 0 && !facingRight || moveIn < 0 && facingRight) && !_grabbing) Flip();
        } else { // Not grounded
            // Here we switch to air contol mode.
            // First, make sure the character isn't trying to move into a wall or something, since otherwise they'll stick to it
            if(!_isTouching) {
                // If they aren't let them control their movement some
                moveVec *= _airControl;
                AddKick(_kickAir);
                if(movePressed && (moveIn > 0 && velForward < _maxSpeed || moveIn < 0 && velForward > -_maxSpeed)) {
                    rb.AddForce(moveVec, ForceMode2D.Impulse);
                }
            }
            // If they aren't holding move in the direction of motion, slow them down a little
            if(!movePressed || moveDirIsNotVelDir)
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * _airSlowdownMultiplier;
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(0)); //TODO not used while in midair yet
        }
    }

    /// <summary> Handles character jumping </summary>
    /// <param name="jump">Is jump input pressed?</param>
    /// <param name="rollJump">Is this a jump from a roll?</param>
    private void Jump(bool jump, bool rollJump = false) {
#if UNITY_EDITOR
        if(_allowJumpingInMidair) grounded = true;
#endif
        if(_rolling && grounded && jump) {
            // If rolling and press jump, set this so that character can jump at a good point in the roll
            _wantsToRollJump = true;
        } else if((grounded || rollJump) && jump && !_jumpStarted) {
            // Initialize the jump
            _jumpFuelLeft = _jumpFuel;
            _jumpStarted = true; //This ensures we don't repeat this step a bunch of times per jump
            rb.velocity = Vector3.Project(rb.velocity, tf.right) + tf.up * _jumpSpeed;
            anim.SetTrigger(rollJump ? _rollJumpAnim : _jumpAnim);
        } else if(jump && _jumpFuelLeft > 0) {
            // Make the character rise higher the longer they hold jump
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            rb.AddForce(tf.up * rb.mass * _jumpFuelForce, ForceMode2D.Force);
        } else {
            _jumpFuelLeft = 0;
        }

        // If the character tries to jump in mid air, they do an air dash (if they have that ability)
        //TODO Maybe change this to double tap jump?
        if(jump && !_wasJump && !grounded && _numberOfAirDashes > 0) {
            if(_airDash != null) StopCoroutine(_airDash);
            _airDash = AirDashHelper();
            StartCoroutine(_airDash);
        }


        // If landed, allow jumping again
        if(grounded && !jump) {
            _airDashesLeft = _numberOfAirDashes;
            _jumpStarted = false;
        }

        // Set the vertical animation for moving up/down through the air
        if(!grounded) anim.SetFloat(_vSpeedAnim, rb.velocity.y / 8);

        if(_jumpStarted && !jump) _wasJump = true;

        _wasJump = jump;
    }

    private IEnumerator _airDash;

    /// <summary> Handles rotating the character during their air dash </summary>
    private IEnumerator AirDashHelper() {
        // Initialize _airDashVector unless it was already being used from another air dash
        // It's initialized to the right direction of the hip under the assumption that the hip bone is pointed upwards
        if(!_airDashing) _airDashVec = _parts.hipsTarget.transform.right;

        // Magnitude of the characters horizontal/vertical input, used to normalize so e.g. diagonal dashes aren't
        // more powerful the cardinal direction dashes, and to make sure that the dash is in *some* direction
        float norm = Mathf.Sqrt(Mathf.Pow(control.moveHorizontal, 2) + Mathf.Pow(control.moveVertical, 2));
        // Only start dash if directional input is given and there are air dashes left, else just end any previous dash
        if(norm > 0.001f && _airDashesLeft > 0) {
            _airDashing = true;
            anim.SetBool(_airDashAnim, true);
            CameraShake.Shake(1, 0.25f);
            Instantiate(_airDashParticleEffect, tf);
            _airDashesLeft--;

            Vector2 controlVec = (tf.right * control.moveHorizontal + tf.up * control.moveVertical) / norm;
            Vector2 dashVelocity = controlVec * _jumpSpeed * _airDashSpeed;

            // This makes sure dashes only increase speed in the direction of the dash, never decrease
            float velDot = Vector2.Dot(rb.velocity, controlVec);
            if(velDot > 0) {
                float dashMag = dashVelocity.magnitude;
                // This should reduce the dash speed if the character is already moving fast in that direction.
                // At ~4x the dash speed, dashing won't add any more speed to the character (dashReducer ~= 1 at
                // that point). This is the hyperbolic tangent function btw.
                float dashReducer = (Mathf.Exp(1 / dashMag * velDot) - 1) / (Mathf.Exp(1 / dashMag * velDot) + 1);
                dashVelocity += (velDot - dashReducer * dashMag) * controlVec;
            }

            rb.velocity = dashVelocity;

            // Start gravity at 0 so movement starts purely in dash direction. This also helps up dashes be as strong
            // as down dashes. Gravity is restored to normal in the while loop below
            rb.gravityScale = 0;
            const float squareVelocityThreshold = 9;
            RaycastHit2D groundCheck = Raycast(tf.position, -tf.up, _rollingGroundCheckDistance, whatIsGround);
            while(!groundCheck || rb.velocity.sqrMagnitude > squareVelocityThreshold) {
                if(rb.gravityScale < 1) rb.gravityScale += Time.deltaTime * 3;
                else rb.gravityScale = 1;

                Vector2 epsilon = new Vector2(0.1f, 0.0001f); // This prevents any instant flips
                // We use a separate vector (_airDashVector) instead of damping the value of the transform directly
                // because the actual transform gets reset every frame by the animation.
                _airDashVec = _airDashVec.SharpInDamp(rb.velocity.normalized * (facingRight ? 1 : -1) + epsilon, 2f);
                _parts.hipsTarget.transform.right = _airDashVec;

                // WaitForEndOfFrame to make sure all the normal animation stuff has already happened
                yield return new WaitForEndOfFrame();
                groundCheck = Raycast(tf.position, -tf.up, _rollingGroundCheckDistance, whatIsGround);
            }
            rb.gravityScale = 1;
        } else if(_airDashing) {
            // If the character was air dashing but just did another air dash with no directional input, the first air
            // dash is canceled when we stop the coroutine, then we need to wait till a new frame for the while loop
            // below to work (otherwise the hips will still be rotated to _airDashVec and the while loop will skip)
            rb.gravityScale = 1;
            yield return new WaitForEndOfFrame();
        }

        // At this point, the air dash is over, so we stop the animation
        // And rotate the character back to where they would otherwise be
        anim.SetBool(_airDashAnim, false);
        bool wasFacingRight = facingRight; //This is used in case the character flips, so we can flip _airDashVector too
        while(Vector3.Angle(_airDashVec, _parts.hipsTarget.transform.right) > 5f) {
            if(wasFacingRight != facingRight)
                _airDashVec = Vector3.Reflect(_airDashVec, _parts.hipsTarget.transform.right);
            _airDashVec = _airDashVec.SharpInDamp(_parts.hipsTarget.transform.right, 1f);
            _parts.hipsTarget.transform.right = _airDashVec;
            wasFacingRight = facingRight;
            yield return new WaitForEndOfFrame();
        }
        _airDashing = false;
    }

    /// <summary> Handles character crouching </summary>
    /// <param name="crouch">Is down input pressed</param>
    private void Crouch(bool crouch) {
        bool wasStanding = !_crouching; // Used to check if we should actually just roll
        _crouching = crouch && grounded && _walkSprint < .65f; // Crouch, unless sprinting

        // If the character initiates a roll beyond a certain speed, make them roll instead
        if(!_rolling && wasStanding && _crouching && _walkSprint > .1f)
            Roll(true);
        else {
            // otherwise, set the animator
            anim.SetBool(_crouchingAnim, _crouching);
            // and deal with rolling if that's still happening
            Roll(false);
        }
    }

    /// <summary> Handles character rolling </summary>
    /// <param name="rollStart"> Should we start rolling? </param>
    private void Roll(bool rollStart) {
        if(rollStart) {
            // Start the roll and set the direction
            _rolling = true;
            anim.SetBool(_rollAnim, true);
            _rollDir = Mathf.Sign(control.moveHorizontal);
        } else if(_rolling) {
            // Continue the roll and check if it should end, or if the character should RollJump
            _rollingTime += Time.fixedDeltaTime;
            Move(_rollDir, true, false);
            if(!grounded) RollJump();
            if(!anim.IsPlaying(RollStateTag) && _rollingTime > 0.5f) RollEnd();
        }
    }

    /// <summary> Used to end roll </summary>
    private void RollEnd() {
        _rolling = false;
        anim.SetBool(_rollAnim, false);
        _rollingTime = 0;
    }

    /// <summary> Used to jump once good point has been reached in a roll </summary>
    private void RollJump() {
        if(!_wantsToRollJump) return;
        _wantsToRollJump = false;
        RollEnd();
        Jump(true, true);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionEnter2D(Collision2D collInfo) {
        _isTouching = true;
        if(!_grabbing) GrabSetPoint(collInfo.GetContact(0).normal, collInfo.GetContact(0).point);
//        print(collInfo.collider + "    :::   " + Time.time);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionExit2D(Collision2D collInfo) {
        _isTouching = false;
    }
}