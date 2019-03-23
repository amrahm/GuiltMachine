using System;
using System.Collections;
using ExtensionMethods;
using UnityEngine;
using static AnimationParameters.Humanoid;
using static UnityEngine.Physics2D;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(HumanoidParts))]
public class HumanoidMovement : MovementAbstract {
    /// <summary> How long after walking off a ledge should the character still be considered grounded </summary>
    private const float CoyoteTime = 0.08f;

    private const string RollStateTag = "Roll";
    private const int RollLayer = 11;
    private const float GrabAdd = 0.1f;
    private const float TimeToGrab = .15f;

    #region Variables

#if UNITY_EDITOR
    [Tooltip("Show debug visualizations"), SerializeField]
    private bool visualizeDebug;


    [Tooltip("Debug control to allow multiple jumps"), SerializeField]
    private bool allowJumpingInMidair;
#endif

    [Tooltip("The fastest the character can travel in the x axis"), SerializeField]
    private float maxSpeed = 10f;

    [Tooltip("How fast the character speeds up"), SerializeField]
    private float acceleration = 1;

    [Tooltip("A kick to make the character start moving faster"), SerializeField]
    private float kick = 1;

    [Tooltip("How much character automatically slows down while not walking but on the ground"), SerializeField]
    private float groundSlowdownMultiplier;

    [Tooltip("Sprint multiplier for when running"), SerializeField]
    private float sprintSpeed = 2;

    [Tooltip("Vertical speed of a jump"), SerializeField]
    private float jumpSpeed = 5;

    [Tooltip("Amount of time a jump can be held to jump higher"), SerializeField]
    private float jumpFuel = 100f;

    [Tooltip("Additional force added while holding jump"), SerializeField]
    private float jumpFuelForce = 30f;

    [Tooltip("How much character can steer while in midair"), SerializeField]
    private float airControl;

    [Tooltip("How much character automatically slows down while in midair"), SerializeField]
    private float airSlowdownMultiplier;

    [Tooltip("A kick to make the character start moving faster while in midair"), SerializeField]
    private float kickAir;

    [Tooltip("Offset for the back of the right foot ground check raycast"), SerializeField]
    private Vector2 groundCheckOffsetR;

    [Tooltip("Offset for the front of the right foot ground check raycast"), SerializeField]
    private Vector2 groundCheckOffsetR2 = new Vector2(0.15f, 0);

    [Tooltip("Offset for the back of the left foot ground check raycast"), SerializeField]
    private Vector2 groundCheckOffsetL;

    [Tooltip("Offset for the front of the left foot ground check raycast"), SerializeField]
    private Vector2 groundCheckOffsetL2 = new Vector2(0.15f, 0);

    [Tooltip("How long is the ground check raycast"), SerializeField]
    private float groundCheckDistance = 0.15f;

    [Tooltip("How far to check down when rolling"), SerializeField]
    private float rollingGroundCheckDistance = 1.3f;

    [Tooltip("How long is the grab check raycast"), SerializeField]
    private float grabDistance = 1;

    [Tooltip("How far along the horizontal check is the downward check"), SerializeField]
    private float grabDownDistMult = 0.7f;

    [Tooltip("How high is the highest grabbable"), SerializeField]
    private float grabTopOffset = 0.65f;

    [Tooltip("How high is the middle grabbable"), SerializeField]
    private float grabMidOffset;

    [Tooltip("How high is the lowest grabbable"), SerializeField]
    private float grabBottomOffset = -0.75f;

    [Tooltip("How wide a radius vertically needs to be clear to initiate a grab"), SerializeField]
    private float grabCheckRadiusV = 0.3f;

    [Tooltip("How far vertically beyond the grab point needs to be clear to initiate a grab"), SerializeField]
    private float grabCheckDistanceV = 1f;

    [Tooltip("How wide a radius horizontally needs to be clear to initiate a grab"), SerializeField]
    private float grabCheckRadiusH = 0.4f;

    [Tooltip("How far horizontally beyond the grab point needs to be clear to initiate a grab"), SerializeField]
    private float grabCheckDistanceH = 0.2f;

    [Header("Movement Abilities")]
    [Tooltip(" >= 1 gives the character the ability to dash-jump in midair this many times"), SerializeField]
    private int numberOfAirDashes;

    [Tooltip("How fast/far the air-dash goes"), SerializeField]
    private float airDashSpeed = 1;

    [Tooltip("The particle effect to spawn when air-dashing"), SerializeField]
    private GameObject airDashParticleEffect;


    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the character is currently grabbing </summary>
    private bool _climbing;

    /// <summary> Point to grab </summary>
    private Vector3 _grabPoint;

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

    /// <summary> This characters layer, used to reset to after a roll/dodge </summary>
    private int _initLayer;

    /// <summary> Position of the foot last frame when crouching </summary>
    private float _lastHipsDelta;

    /// <summary> Used to zero out friction when moving. True if the character's feet have no friction. </summary>
    private bool _frictionZero;

    /// <summary> How much time left till not considered grounded </summary>
    private float _coyoteTimeLeft;

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

    /// <summary> Time of first jump press, useful in detecting double taps </summary>
    private float _jumpMidAirFirstPressTime = -10f;

    /// <summary> A vector used to help smoothly rotate the character during an air dash </summary>
    private Vector3 _airDashVec;

    /// <summary> True if the character is currently air dashing </summary>
    private bool _airDashing;

    /// <summary> True if the character is on the floor while air dashing </summary>
    private bool _airDashingGrounded;

    /// <summary> How many more air dashes the character can do in midair </summary>
    private int _airDashesLeft;

    /// <summary> Whether or not the character is touching something </summary>
    private bool _isTouching;

    /// <summary> Reference to Parts script, which contains all of the character's body parts </summary>
    private HumanoidParts _parts;

    /// <summary> The physics material on the foot colliders </summary>
    private PhysicsMaterial2D _footFrictionMat;

    /// <summary> Reference to AirDashHelper Coroutine </summary>
    private IEnumerator _airDash;

    #endregion

    protected override void Awake() {
        // Setting up references.
        base.Awake();
        _parts = GetComponent<HumanoidParts>();

        // Instance the foot physics material so that we can adjust it without affecting other users of the same material
        var footMat = _parts.footR.GetComponent<Collider2D>().sharedMaterial;
        _footFrictionMat = new PhysicsMaterial2D(footMat.name + " (Instance)") {friction = footMat.friction};
        _parts.footR.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;
        _parts.footL.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;

        _grabDownDist = grabDistance * grabDownDistMult;
        _initLayer = gameObject.layer;
    }

    private void FixedUpdate() {
        UpdateGrounded();
        if(!_rolling) Move(control.moveHorizontal, control.sprint);
        Jump(control.jumpPressed);
        Crouch(control.crouchPressed);

        if(grounded && !_climbing) {
            // When the feet move up relative to the hips, move the character down so that 
            // the feet stay on the ground instead of lifting into the air
            float hipsDelta = Vector2.Dot(_parts.hips.transform.position - _parts.footR.transform.position, tf.up);
            tf.position += tf.up * (hipsDelta - _lastHipsDelta) / 2;
            _lastHipsDelta = hipsDelta;
        }

#if UNITY_EDITOR
        if(visualizeDebug) { //Visualize grab raycasts
            Debug.DrawRay(tf.TransformPoint(new Vector2(0, grabMidOffset + GrabAdd)),
                          tf.right * FlipInt * grabDistance, new Color(0.52f, 1f, 0.52f));
            Debug.DrawRay(tf.TransformPoint(new Vector2(_grabDownDist, grabTopOffset)),
                          -tf.up * (grabTopOffset - grabBottomOffset), new Color(0.38f, 0.72f, 0.38f));
            if(_climbing) DebugExtension.DebugPoint(_parts.armRIK.Target().position, Color.green);
        }
#endif
    }

    #region ClimbingStuff

    /// <summary> Handle if this character is climbing a platform </summary>
    private IEnumerator _ClimbHandle() {
        float timeBeenClimbing = 0;
        Vector3 handRStart = _parts.armRIK.Target().position;
        Vector3 handLStart = _parts.armLIK.Target().position;
        while(_climbing) {
            Vector3 right = tf.right * FlipInt;
            float dT = Time.fixedDeltaTime;

            // Move the head to a little above halfway between the two points
            // Move the head further down if the character can't reach their IK target, which should let them reach it
            float handIKErr = Vector2.Distance(_parts.armRIK.Target().position, _parts.handRTarget.transform.position);
            Vector3 handIKMidPoint = (_parts.armRIK.Target().position + _parts.armLIK.Target().position) / 2;
            Vector3 headIKPos = handIKMidPoint + tf.up * (1 - handIKErr);
            _parts.headIK.Target().position = _parts.headIK.Target().position.SharpInDamp(headIKPos, 5, dT);

            timeBeenClimbing += dT;
            if(timeBeenClimbing < TimeToGrab) {
                // Move the hands from the start to the grab point, and fade in the head IK
                _parts.armRIK.Target().position = Vector3.Lerp(handRStart, _grabPoint, timeBeenClimbing / TimeToGrab);
                _parts.armLIK.Target().position = Vector3.Lerp(handLStart, _grabPoint + right * GrabAdd,
                                                               timeBeenClimbing / TimeToGrab);
                _parts.headIK.weight = Mathf.Lerp(0, 1, timeBeenClimbing / TimeToGrab);
                rb.velocity = rb.velocity.SharpInDamp(Vector2.zero, 1, dT); // and slow down the velocity some
            } else {
                // Make sure the IKs are all at their final positions
                _parts.armRIK.Target().position = _grabPoint;
                _parts.armLIK.Target().position = _grabPoint + right * GrabAdd;
                _parts.headIK.weight = 1;

                // Raycast check from either foot (whichever hits) to the wall
                Vector3 shift = -tf.up * GrabAdd;
                var sideCheck = Raycast(_parts.footR.transform.position + shift, right, grabDistance, WhatIsGround);
#if UNITY_EDITOR
                if(visualizeDebug && sideCheck)
                    Debug.DrawRay(_parts.footR.transform.position, right * grabDistance, Color.cyan);
#endif
                bool sideCheckPastGrabPoint = Vector2.Dot(sideCheck.point, right) > Vector2.Dot(_grabPoint, right);
                anim.SetBool(ClimbIsAgainstWallAnim, sideCheck && !sideCheckPastGrabPoint);

                // Add a force to keep the character pressed against the wall
                float sideDistance = Vector2.Dot(right, _parts.footL.transform.position - _grabPoint);
                float wallDist = sideCheck && !sideCheckPastGrabPoint ?
                                     Vector2.Distance(_parts.footR.transform.position, sideCheck.point) :
                                     (sideDistance < -GrabAdd * 3.5f ? 0.5f : 0);
                Vector2 keepAgainstWall = right * Mathf.Pow(wallDist * 4, 2) * dT;

                // And a force to move them up or down based on input
                float horizontalInput = control.moveHorizontal * FlipInt;
                float climbInput = Mathf.Min(horizontalInput + control.moveVertical, 1f);
                Vector2 climbControl = (Vector2) tf.up * 3 * climbInput * dT;
                anim.SetFloat(ClimbAnimSpeed, climbInput);

                rb.MovePosition(rb.position + keepAgainstWall + climbControl);


                // Vertical distance of back foot to the grab point
                float downDistance = Vector2.Dot(tf.up, _parts.footR.transform.position - _grabPoint);
                anim.SetBool(ClimbStepOverAnim, downDistance > -GrabAdd * 3);

                bool aboveLedge = downDistance > 0; // Release if the character has climbed high enough
                // or moves too far down
                bool tooFarToHoldOn = Vector2.Dot(tf.up, tf.position - _grabPoint) < -grabTopOffset - GrabAdd;
                bool movingAway = horizontalInput < -0.2f; // also release if character moves away from wall
                if(aboveLedge || tooFarToHoldOn || movingAway)
                    ClimbRelease(aboveLedge);
            }

            yield return Yields.WaitForFixedUpdate;
        }
    }

    /// <summary> Update whether or not this character can grab a platform </summary>
    private void ClimbSetGrabPoint(Vector2 hitNormal, Vector2 hitPoint) {
        if(_climbing) return;
        //Ensure character isn't grabbing and is in midair or about to be, and is trying to move into the hit platform
        if(_climbing || grounded && !_jumpStarted ||
           Vector2.Dot(control.moveHorizontal * hitNormal, tf.right) > -0.1f) return;

        Vector2 right = tf.right * FlipInt;
        Vector2 midPoint = tf.TransformPoint(new Vector2(0, tf.InverseTransformPoint(hitPoint).y + GrabAdd));
        RaycastHit2D grabMid = Raycast(midPoint, right, grabDistance, WhatIsGround);
#if UNITY_EDITOR
        if(visualizeDebug) Debug.DrawRay(midPoint, right * grabDistance, Color.magenta);
#endif

        // First check for mid, first at the point of contact, and if that's null, then the normal mid check
        if(grabMid || (grabMid = Raycast(tf.TransformPoint(new Vector2(0, grabMidOffset + GrabAdd)), right,
                                         grabDistance, WhatIsGround))) {
            Vector2 downOrigin = tf.TransformPoint(new Vector2(grabMid.distance + GrabAdd, grabTopOffset));
            RaycastHit2D down = Raycast(downOrigin, -tf.up, grabTopOffset - grabBottomOffset, WhatIsGround);
            if(down && ClimbCheckIfClear(down.point, right)) ClimbBegin(down.point);
            return;
        }

        // If mid fails, check for down.
        RaycastHit2D grabDown = Raycast(tf.TransformPoint(new Vector2(_grabDownDist, grabTopOffset)),
                                        -tf.up, grabTopOffset - grabBottomOffset, WhatIsGround);
        if(grabDown) {
            Vector2 grabMidOrigin = tf.TransformPoint(new Vector2(0, grabTopOffset - grabDown.distance - GrabAdd));
            grabMid = Raycast(grabMidOrigin, right, grabDistance, WhatIsGround);
            bool pointNotInGround = Vector2.Distance(grabMid.point, grabMidOrigin) > 0.1f;
            if(pointNotInGround && grabMid && ClimbCheckIfClear(midPoint, right))
                ClimbBegin(grabMid.point + right * GrabAdd + (Vector2) tf.up * GrabAdd);
        }
    }

    /// <summary> Used to start grabbing </summary>
    private void ClimbBegin(Vector2 point) {
        if(Time.time - _timeSinceRelease < 0.5f) return;
        _climbing = true;
        _airDashing = false;
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
        anim.SetBool(ClimbAnim, true);
        rb.gravityScale = 0f;
        _grabPoint = point;
        StartCoroutine(_ClimbHandle());
    }

    /// <summary> Used to set end of grab </summary>
    private void ClimbRelease(bool pullUp) {
        _timeSinceRelease = Time.time;
        _climbing = false;
        _parts.handR.GetComponent<Collider2D>().isTrigger = false;
        _parts.handL.GetComponent<Collider2D>().isTrigger = false;
        _parts.lowerArmR.GetComponent<Collider2D>().isTrigger = false;
        _parts.lowerArmL.GetComponent<Collider2D>().isTrigger = false;
        _parts.upperArmR.GetComponent<Collider2D>().isTrigger = false;
        _parts.upperArmL.GetComponent<Collider2D>().isTrigger = false;
        anim.SetBool(ClimbAnim, false);

        StartCoroutine(_ClimbIKRelease());
        if(pullUp) StartCoroutine(_ClimbPullUp());
        else rb.gravityScale = 1f; // We do this in GrabPullUp if pullUp
    }

    /// <summary> Used to finish the grab by moving the character to on top of the ledge </summary>
    private IEnumerator _ClimbPullUp() {
        _parts.footR.GetComponent<Collider2D>().isTrigger = true;
        _parts.footL.GetComponent<Collider2D>().isTrigger = true;
        Vector3 right = tf.right * FlipInt;
        rb.gravityScale = .5f;
        float hDist = 1;
        const float maxDistance = .7f;
        do {
            tf.Translate((Vector2) (right * (hDist + 0.1f) + _grabPoint - _parts.footR.transform.position) * 4 *
                         Time.fixedDeltaTime);
            if(Vector3.Dot(tf.up, _grabPoint - _parts.footR.transform.position) > 0)
                tf.Translate((Vector2) tf.up * Time.fixedDeltaTime);

            yield return Yields.WaitForFixedUpdate;

            hDist = Vector3.Dot(right, _grabPoint - tf.position);
        } while(hDist > -GrabAdd && Time.time - _timeSinceRelease < TimeToGrab * 2 &&
                Vector3.Distance(_grabPoint, _parts.footR.transform.position) < maxDistance);
        rb.gravityScale = 1f;

        _parts.footR.GetComponent<Collider2D>().isTrigger = false;
        _parts.footL.GetComponent<Collider2D>().isTrigger = false;
    }

    /// <summary> Used to gradually release from IK pose </summary>
    private IEnumerator _ClimbIKRelease() {
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
    private bool ClimbCheckIfClear(Vector2 grabPoint, Vector2 right) {
        // Convert the grab point to local coordinates so we can use its x and y without worrying about rotated gravity
        Vector2 grabPointLocal = tf.InverseTransformPoint(grabPoint);

        // Check up with a circle cast that starts at the character's mid-height, offset from the wall horizontally
        Vector2 upCheckStart = tf.TransformPoint(new Vector2(grabPointLocal.x - GrabAdd * 2 - grabCheckRadiusV, 0));
        float upCheckDist = grabPointLocal.y + grabCheckDistanceV;
        RaycastHit2D upCheck = CircleCast(upCheckStart, grabCheckRadiusV, tf.up, upCheckDist, WhatIsGround);

        // Check sideways starting from the character's x position, offset above the top of the ledge
        Vector2 sideCheckStart = tf.TransformPoint(new Vector2(0, grabPointLocal.y + GrabAdd + grabCheckRadiusH));
        float sideCheckDist = grabPointLocal.x + grabCheckDistanceH;
        RaycastHit2D sideCheck = Raycast(sideCheckStart, right, sideCheckDist, WhatIsGround);
        // And do two more sideways checks so that the three raycasts span grabCheckRadiusH distance
        // Which should ensure that at least that distance is clear above the ledge
        Vector2 upRadH = (Vector2) tf.up * grabCheckRadiusH;
        if(!sideCheck)
            sideCheck = Raycast(sideCheckStart + upRadH, right, sideCheckDist, WhatIsGround);
        if(!sideCheck)
            sideCheck = Raycast(sideCheckStart - upRadH, right, sideCheckDist, WhatIsGround);

#if UNITY_EDITOR
        if(visualizeDebug) {
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
        bool wasGrounded = grounded;
        Vector2 up = tf.up; // cache up cast to Vector2

        RaycastHit2D groundCheckHit;
        if(_rolling) {
            // If the character is rolling, their feet will leave the ground, but we still want to consider them as touching
            // the ground, so we instead do a raycast out from the center, and set grounded and walkSlope using this.
            groundCheckHit = Raycast(tf.position, -up, rollingGroundCheckDistance, WhatIsGround);
        } else if(_airDashing) {
            // Same as rolling, but they go head first when air dashing
            groundCheckHit = Raycast(_parts.head.transform.position, -up, rollingGroundCheckDistance / 2,
                                     WhatIsGround);
        } else {
            /* This checks both feet to see if they are touching the ground, and if they are, it checks the angle of the ground they are on.
             * For each foot, a raycast checks the back of the foot, and if that hits nothing, then the front of the foot is checked.
             * Then, for any feet that are touching the ground, we check the angle of the ground, and use the larger angle, as long as it's
             * still less than the maxWalkSlope. This is so that uneven surfaces will still be walked up quickly.
             * walkSlope is set to the chosen angle, and is used in the Move(...) method. */
            RaycastHit2D rightHit = Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -up,
                                            groundCheckDistance, WhatIsGround);
            if(!rightHit)
                rightHit = Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -up,
                                   groundCheckDistance, WhatIsGround);
            RaycastHit2D leftHit = Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -up,
                                           groundCheckDistance, WhatIsGround);
            if(!leftHit)
                leftHit = Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -up,
                                  groundCheckDistance, WhatIsGround);
            float rightAngle = Vector2.Angle(rightHit.normal, up);
            float leftAngle = Vector2.Angle(leftHit.normal, up);

            // Pick the larger angle that is still within bounds
            bool rightGreater = rightAngle >= leftAngle && rightAngle < maxWalkSlope || leftAngle >= maxWalkSlope;
            // Now choose the right one if it was greater and actually hit. If leftHit didn't hit, rightGreater = true
            groundCheckHit = rightGreater && rightHit ? rightHit : leftHit;
        }
        groundNormal = groundCheckHit ? groundCheckHit.normal : up;
        walkSlope = Vector2.Angle(groundCheckHit.normal, up);
        if(_airDashing)
            _airDashingGrounded = groundCheckHit && walkSlope < maxWalkSlope;
        else
            grounded = groundCheckHit && walkSlope < maxWalkSlope;

        // This bit of code gives the character a little extra time to do a jump after walking off an edge
        // This helps account for human reaction times and such
        if(grounded) _coyoteTimeLeft = CoyoteTime;
        else if(!grounded && _coyoteTimeLeft > 0) {
            _coyoteTimeLeft -= Time.fixedDeltaTime;
            coyoteGrounded = true;
        } else {
            coyoteGrounded = false;
        }

        // Set this so the animator can play or transition to/from the appropriate animations
        anim.SetBool(GroundedAnim, grounded || coyoteGrounded);
        // If the character was grounded and isn't anymore, but they didn't jump, then they must have walked off a ledge or something
        if(wasGrounded && !grounded && !_jumpStarted) _falling = true;
        if(_falling && !(_rolling || _climbing)) {
            // If they are falling for long enough (but not still rolling), set the fall animation
            _fallDuration += Time.fixedDeltaTime;
            if(_fallDuration > 0.15f) anim.SetBool(FallingAnim, true); // falling without jumping
        }
        if(grounded) {
            // Reset all of those things when they aren't falling
            _fallDuration = 0;
            _falling = false;
            anim.SetBool(FallingAnim, false);
        }


#if UNITY_EDITOR
        if(visualizeDebug) {
            if(_rolling) {
                Debug.DrawRay(tf.position, -up * rollingGroundCheckDistance);
            } else if(_rolling) {
                Debug.DrawRay(tf.position, -up * rollingGroundCheckDistance);
            } else {
                Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR),
                              -up * groundCheckDistance);
                Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR2),
                              -up * groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL),
                              -up * groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL2),
                              -up * groundCheckDistance);
            }
        }
#endif
    }

    /// <summary> Handles character walking and running </summary>
    /// <param name="moveIn">Walking input</param>
    /// <param name="sprint"> Whether sprint input is pressed </param>
    private void Move(float moveIn, bool sprint) {
        //Calculate the tangent to the ground so that the player can walk smoothly up and down slopes
        Vector2 tangent = grounded ? Vector3.Cross(groundNormal, Vector3.forward) : tf.right;
        float velForward = Vector2.Dot(rb.velocity, tf.right); // Use this so that running down slopes is faster
        float velTangent = Vector2.Dot(rb.velocity, tangent);  // And use this so that the animation looks right
#if UNITY_EDITOR
        if(visualizeDebug) Debug.DrawRay(_parts.footL.transform.position, tangent, Color.blue);
#endif
        float slopeReducer = Mathf.Lerp(1, .7f, walkSlope / maxWalkSlope); // reduce speed as slopes increase
        moveVec = slopeReducer * rb.mass * tangent * acceleration * moveIn * Time.fixedDeltaTime;

        void AddKick(float force) { // If walking from standstill, gives a kick so walking is more responsive
            Vector2 kickAdd = rb.mass * tangent * force * 30 * slopeReducer;
            if(moveIn > 0 && velForward < maxSpeed / 3) rb.AddForce(kickAdd);
            else if(moveIn < 0 && velForward > -maxSpeed / 3) rb.AddForce(-kickAdd);
        }

        // checks if intention is not same direction as motion
        bool moveDirIsNotVelDir = Math.Sign(moveIn) != Math.Sign(velForward);

        if(grounded) {
            float sprintAmt = sprint ? sprintSpeed : 1;
            moveVec *= sprintAmt; // We do this here because we don't want them to be able to start sprinting in midair

            // Check that the character wants to walk, but isn't walking too fast
            if(Mathf.Abs(moveIn) > 0.1f && (Mathf.Abs(velForward) < maxSpeed * sprintAmt || moveDirIsNotVelDir)) {
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
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * groundSlowdownMultiplier;
            }
            AddKick(kick);

            // Flip the player if necessary
            bool shouldFlip = (moveIn > 0 && !FacingRight || moveIn < 0 && FacingRight) && !_climbing;
            if(shouldFlip) Flip();

            // Set animation params
            _walkSprint = Mathf.Abs(velTangent) <= maxSpeed + 1f ?
                              Mathf.Abs(velTangent) / maxSpeed / 2 :
                              Mathf.Abs(velTangent) / (maxSpeed * sprintSpeed);
            // avg it with player intention
            _walkSprint = (_walkSprint + Mathf.Abs(moveIn / 2 * sprintSpeed * slopeReducer)) / 2;
            if(cantFlip > 0 && shouldFlip) _walkSprint *= -1;
            // avg it for smoothing
            anim.SetFloat(SpeedAnim, anim.GetFloat(SpeedAnim).SharpInDamp(_walkSprint, 2f, Time.fixedDeltaTime));
        } else { // Not grounded
            // Here we switch to air contol mode.
            // First, make sure the character isn't trying to move into a wall or something, since otherwise they'll stick to it
            if(!_isTouching) {
                // If they aren't let them control their movement some
                moveVec *= airControl;
                AddKick(kickAir);
                if(Mathf.Abs(moveIn - velTangent / maxSpeed) > 0 &&
                   (moveIn > 0 && velForward < maxSpeed || moveIn < 0 && velForward > -maxSpeed)) {
                    rb.AddForce(moveVec, ForceMode2D.Impulse);
                }
            }
            // If they aren't holding move in the direction of motion, slow them down a little
            if(Mathf.Abs(moveIn) < 0.1f || moveDirIsNotVelDir)
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * airSlowdownMultiplier;
            anim.SetFloat(SpeedAnim, anim.GetFloat(SpeedAnim).SharpInDamp(0)); //TODO not used while in midair yet
        }
    }

    /// <summary> Handles character jumping </summary>
    /// <param name="jump">Is jump input pressed?</param>
    /// <param name="rollJump">Is this a jump from a roll?</param>
    private void Jump(bool jump, bool rollJump = false) {
#if UNITY_EDITOR
        if(allowJumpingInMidair) grounded = true;
#endif
        if(_rolling && (grounded || coyoteGrounded) && jump) {
            // If rolling and press jump, set this so that character can jump at a good point in the roll
            _wantsToRollJump = true;
        } else if((grounded || coyoteGrounded || rollJump) && jump && !_jumpStarted) {
            // Initialize the jump
            _jumpFuelLeft = jumpFuel;
            _jumpStarted = true; //This ensures we don't repeat this step a bunch of times per jump
            rb.velocity = Vector3.Project(rb.velocity, tf.right) + tf.up * jumpSpeed;
            anim.SetTrigger(rollJump ? RollJumpAnim : JumpAnim);
        } else if(jump && _jumpFuelLeft > 0) {
            // Make the character rise higher the longer they hold jump
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            rb.AddForce(tf.up * rb.mass * jumpFuelForce, ForceMode2D.Force);
        } else {
            _jumpFuelLeft = 0;
        }

        // Check for double tapping jump in midair
        const float doubleTapTime = 0.3f;
        bool doubleTappedInMidAir = false;
        if(jump && !_wasJump && !grounded) {
            if(Time.time - _jumpMidAirFirstPressTime > doubleTapTime) {
                _jumpMidAirFirstPressTime = Time.time;
            } else if(Time.time - _jumpMidAirFirstPressTime <= doubleTapTime) {
                doubleTappedInMidAir = true;
                _jumpMidAirFirstPressTime -= doubleTapTime; //ensure that they need to double tap again to get back here
            }
        }

        // If the character double taps jump in midair, they do an air dash (if they have that ability)
        if(doubleTappedInMidAir && numberOfAirDashes > 0) {
            if(_airDash != null) StopCoroutine(_airDash);
            _airDash = _AirDash();
            StartCoroutine(_airDash);
        }


        // If landed, allow jumping again
        if(grounded && !jump) {
            _airDashesLeft = numberOfAirDashes;
            _jumpStarted = false;
        }

        // Set the vertical animation for moving up/down through the air
        if(!grounded) anim.SetFloat(VSpeedAnim, rb.velocity.y / 8);

        if(_jumpStarted && !jump) _wasJump = true;

        _wasJump = jump;
    }

    /// <summary> Handles launching and rotating the character during their air dash </summary>
    private IEnumerator _AirDash() {
        // Initialize _airDashVector unless it was already being used from another air dash
        // It's initialized to the right direction of the hip under the assumption that the hip bone is pointed upwards
        if(!_airDashing) _airDashVec = _parts.hipsTarget.transform.right;

        // Magnitude of the characters horizontal/vertical input, used to normalize so e.g. diagonal dashes aren't
        // more powerful the cardinal direction dashes, and to make sure that the dash is in *some* direction
        float norm = Mathf.Sqrt(Mathf.Pow(control.moveHorizontal, 2) + Mathf.Pow(control.moveVertical, 2));
        // Only start dash if directional input is given and there are air dashes left, else just end any previous dash
        if(norm > 0.001f && _airDashesLeft > 0) {
            _airDashing = true;
            anim.SetBool(AirDashAnim, true);
            CameraShake.Shake(1, 0.25f);
            Instantiate(airDashParticleEffect, tf);
            _airDashesLeft--;

            Vector2 controlVec = (tf.right * control.moveHorizontal + tf.up * control.moveVertical) / norm;
            Vector2 dashVelocity = controlVec * jumpSpeed * airDashSpeed;

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


            // Start a dive with gravity at 0 so movement starts purely in dash direction.
            // This also helps up dashes be as strong as down dashes.
            rb.gravityScale = 0;
            const float squareVelocityThreshold = 9;
            while(_airDashing && (!_airDashingGrounded || rb.velocity.sqrMagnitude > squareVelocityThreshold)) {
                if(rb.gravityScale < 1) rb.gravityScale += Time.deltaTime * 3;
                else rb.gravityScale = 1;

                Vector2 epsilon = new Vector2(0.1f, 0.0001f); // This prevents any instant flips
                // We use a separate vector (_airDashVector) instead of damping the value of the transform directly
                // because the actual transform gets reset every frame by the animation.
                _airDashVec = _airDashVec.SharpInDamp(rb.velocity.normalized * FlipInt + epsilon, 2f);
                _parts.hipsTarget.transform.right = _airDashVec;

                // WaitForEndOfFrame to make sure all the normal animation stuff has already happened
                yield return Yields.WaitForEndOfFrame;
            }
            rb.gravityScale = 1;
        } else if(_airDashing) {
            // If the character was air dashing but just did another air dash with no directional input, the first air
            // dash is canceled when we stop the coroutine, then we need to wait till a new frame for the while loop
            // below to work (otherwise the hips will still be rotated to _airDashVec and the while loop will skip)
            rb.gravityScale = 1;
            yield return Yields.WaitForEndOfFrame;
        }

        // At this point, the air dash is over, so we stop the animation
        // And rotate the character back to where they would otherwise be
        anim.SetBool(AirDashAnim, false);
        bool wasFacingRight = FacingRight; //This is used in case the character flips, so we can flip _airDashVector too
        while(Vector3.Angle(_airDashVec, _parts.hipsTarget.transform.right) > 5f) {
            if(wasFacingRight != FacingRight)
                _airDashVec = Vector3.Reflect(_airDashVec, _parts.hipsTarget.transform.right);
            _airDashVec = _airDashVec.SharpInDamp(_parts.hipsTarget.transform.right, 1f);
            _parts.hipsTarget.transform.right = _airDashVec;
            wasFacingRight = FacingRight;
            yield return Yields.WaitForEndOfFrame;
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
            RollStart();
        else {
            // otherwise, set the animator
            anim.SetBool(CrouchingAnim, _crouching);
            // and deal with rolling if that's still happening
            Roll();
        }
//        if(!_crouching && !_rolling && crouch && !grounded)
            //TODO Dive
    }

    /// <summary> Start the roll and set the direction </summary>
    private void RollStart() {
        _rolling = true;
        anim.SetBool(RollAnim, true);
        _rollDir = Mathf.Sign(control.moveHorizontal);
        foreach(var part in _parts.parts) part.layer = RollLayer;
        cantFlip++;
        print("ROLL INC " + cantFlip);
    }

    /// <summary> Handles character rolling </summary>
    private void Roll() {
        if(_rolling) {
            // Continue the roll and check if it should end, or if the character should RollJump
            _rollingTime += Time.fixedDeltaTime;
            Move(_rollDir, false);
            if(!grounded) RollJump();
            if(!anim.IsPlaying(RollStateTag) && _rollingTime > 0.5f) RollEnd();
        }
    }

    /// <summary> Used to end roll </summary>
    private void RollEnd() {
        if(!_rolling) return;
        _rolling = false;
        anim.SetBool(RollAnim, false);
        _rollingTime = 0;
        foreach(var part in _parts.parts) part.layer = _initLayer;
        cantFlip--;
        print("ROLL DEC " + cantFlip);
    }

    /// <summary> Used to jump once good point has been reached in a roll </summary>
    private void RollJump() {
        if(!_wantsToRollJump) return;
        _wantsToRollJump = false;
        RollEnd();
        Jump(true, true);
    }

    private void OnCollisionEnter2D(Collision2D collInfo) {
        _isTouching = true;
        if(!_climbing) ClimbSetGrabPoint(collInfo.GetContact(0).normal, collInfo.GetContact(0).point);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionExit2D(Collision2D collInfo) {
        _isTouching = false;
    }
}