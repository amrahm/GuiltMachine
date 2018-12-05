using System;
using ExtensionMethods;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HumanoidParts))]
[RequireComponent(typeof(CharacterMasterAbstract))]
public class HumanoidMovement : MovementAbstract {
    private const string RollStateTag = "Roll";
    private const float GrabAdd = 0.1f;

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

    [Tooltip("Offset for the back of the right foot ground check raycast")]
    public Vector2 groundCheckOffsetR;

    [Tooltip("Offset for the front of the right foot ground check raycast")]
    public Vector2 groundCheckOffsetR2;

    [Tooltip("Offset for the back of the left foot ground check raycast")]
    public Vector2 groundCheckOffsetL;

    [Tooltip("Offset for the front of the left foot ground check raycast")]
    public Vector2 groundCheckOffsetL2;

    [Tooltip("How long is the ground check raycast")]
    public float groundCheckDistance;

    [Tooltip("How far to check down when rolling")]
    public float rollingGroundCheckDistance = 2f;

    [Tooltip("How long is the grab check raycast")]
    public float grabDistance = 1;

    [Tooltip("How far along the horizontal check is the downward check")]
    public float grabDownDistMult = 0.7f;

    [Tooltip("How high is the highest grabbable")]
    public float grabTopOffset = 1.5f;

    [Tooltip("How high is the middle grabbable")]
    public float grabMidOffset;

    [Tooltip("How high is the lowest grabbable")]
    public float grabBottomOffset = -0.75f;

    [Tooltip("How wide a radius vertically needs to be clear to initiate a grab")]
    public float grabCheckRadiusV = 0.3f;

    [Tooltip("How wide a radius horizontally needs to be clear to initiate a grab")]
    public float grabCheckRadiusH = 0.4f;

    [Tooltip("How far horizontally beyond the radius needs to be clear to initiate a grab")]
    public float grabCheckDistanceH = 0.1f;


    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the character is currently _grabbing </summary>
    private bool _grabbing;

    /// <summary> Distance out to do downward check </summary>
    private float _grabDownDist;

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

    /// <summary> True if the character pressed jump while rolling </summary>
    private bool _wantsToRollJump;

    /// <summary> Whether or not the character is touching something </summary>
    private bool _isTouching;

    /// <summary> Reference to Parts script, which contains all of the character's body parts </summary>
    private HumanoidParts _parts;

    /// <summary> The physics material on the foot colliders </summary>
    private PhysicsMaterial2D _footFrictionMat;

    /// <summary> The speed parameter in the animator </summary>
    private int _speedAnim;

    /// <summary> The vertical speed parameter in the animator </summary>
    private int _vSpeedAnim;

    /// <summary> The crouching parameter in the animator </summary>
    private int _crouchingAnim;

    /// <summary> The roll parameter in the animator </summary>
    private int _rollAnim;

    /// <summary> The jump parameter in the animator </summary>
    private int _jumpAnim;

    /// <summary> The roll jump parameter in the animator </summary>
    private int _rollJumpAnim;

    /// <summary> The land parameter in the animator </summary>
    private int _groundedAnim;

    /// <summary> The fall parameter in the animator </summary>
    private int _fallingAnim;

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

        _grabDownDist = grabDistance * grabDownDistMult;
    }

    private void FixedUpdate() {
        UpdateGrounded();
        UpdateGrab();
        if(!_rolling) Move(control.moveHorizontal, control.hPressed, control.sprint);
        Jump(control.upPressed);
        Crouch(control.downPressed);

        if(grounded) {
            // When the feet move up relative to the hips, move the character down so that 
            // the feet stay on the ground instead of lifting into the air
            float hipsDelta = Vector2.Dot(_parts.hips.transform.position - _parts.footR.transform.position, tf.up);
            rb.transform.position += tf.up * (hipsDelta - _lastHipsDelta) / 2;
            _lastHipsDelta = hipsDelta;
        }
    }

    /// <summary> Update whether or not this character can/is grab/bing a platform </summary>
    private void UpdateGrab(ContactPoint2D? hitContact = null) {
        // TODO Should we only start calling this immediately after hitting a wall?
        // TODO that could also give us a second mid to check, mid being at the point of collision
        // TODO and would generally be more efficient
        Vector2 right = facingRight ? tf.right : -tf.right;
        Vector2? hitPoint = hitContact?.point;
        Vector2? hitVel = hitContact?.relativeVelocity.magnitude * hitContact?.normal;
        bool hitInCorrectDirection = (!grounded || _jumpStarted) && hitContact.HasValue &&
                                     Vector2.Dot(hitVel.Value, right) < (facingRight ? 0.3f : -0.3f);
#if UNITY_EDITOR
        if(_visualizeDebug) {
            Vector2 midPoint = tf.TransformPoint(new Vector2(0, grabMidOffset));
            Debug.DrawRay(midPoint, right * grabDistance);
            Debug.DrawRay(tf.TransformPoint(new Vector2(_grabDownDist, grabTopOffset)),
                          -tf.up * (grabTopOffset - grabBottomOffset), Color.gray);
            if(!(_grabbing || (!grounded || _jumpStarted) && hitContact.HasValue &&
                 Vector2.Dot(hitVel.Value, right) > -0.3f))
                return; // If we only went this far for debug viz, quit here
        }
#endif
        if(!(_grabbing || hitInCorrectDirection)) return;

        // Find Grab Point
        if(!_grabbing) {
            float midHeight = hitPoint.HasValue ? tf.InverseTransformPoint(hitPoint.Value).y : grabMidOffset;
            Vector2 midPoint = tf.TransformPoint(new Vector2(0, midHeight + GrabAdd));
            RaycastHit2D grabMid = Physics2D.Raycast(midPoint, right, grabDistance, whatIsGround);
            RaycastHit2D grabDown;
            // ReSharper disable AssignmentInConditionalExpression
            if(grabMid) { // First check for mid
                Vector2 grabDownOrigin = tf.TransformPoint(new Vector2(grabMid.distance + GrabAdd, grabTopOffset));
                grabDown = Physics2D.Raycast(grabDownOrigin, -tf.up, grabTopOffset - grabBottomOffset, whatIsGround);
                bool grabIsClear = CheckIfGrabIsClear(midPoint, grabMid, grabDown, right);
                if(grabIsClear) BeginGrab(grabDown.point, right);
            } else if(grabDown = Physics2D.Raycast(tf.TransformPoint(new Vector2(_grabDownDist, grabTopOffset)),
                                                   tf.up, grabTopOffset - grabBottomOffset, whatIsGround)) {
                // If mid fails, check for down. We do assignment here to avoid doing it if mid hits something
                Vector2 grabMidOrigin = tf.TransformPoint(new Vector2(0, grabTopOffset - grabDown.distance - GrabAdd));
                grabMid = Physics2D.Raycast(grabMidOrigin, right, grabDistance, whatIsGround);
                bool grabIsClear = CheckIfGrabIsClear(midPoint, grabMid, grabDown, right);
                bool pointNotInGround = Vector2.Distance(grabMid.point, grabMidOrigin) > 0.1f;
                if(grabIsClear && pointNotInGround)
                    BeginGrab(grabMid.point + right * GrabAdd + (Vector2) tf.up * GrabAdd, right);
            }
            // ReSharper restore AssignmentInConditionalExpression
        }

        // Now that we might be grabbing, do stuff with grab point
        if(_grabbing) {
            _parts.armRIK.weight += Time.fixedDeltaTime;
            _parts.armLIK.weight += Time.fixedDeltaTime;
//            _parts.upperArmLTarget.GetComponent<LimbIK>().solver.IKPositionWeight.SharpOutDamp(1, 1, Time.fixedDeltaTime);
        }
    }

    private void BeginGrab(Vector2 point, Vector2 right) {
        _grabbing = true;
        _parts.handR.GetComponent<Collider2D>().isTrigger = true;
        _parts.handL.GetComponent<Collider2D>().isTrigger = true;
        _parts.lowerArmR.GetComponent<Collider2D>().isTrigger = true;
        _parts.lowerArmL.GetComponent<Collider2D>().isTrigger = true;
        _parts.upperArmR.GetComponent<Collider2D>().isTrigger = true;
        _parts.upperArmL.GetComponent<Collider2D>().isTrigger = true;
        _parts.armRIK.gameObject.SetActive(true);
        _parts.armLIK.gameObject.SetActive(true);
        _parts.armRIK.weight = 0;
        _parts.armLIK.weight = 0;
        _parts.armRIK.GetChain(0).target.parent = null;
        _parts.armLIK.GetChain(0).target.parent = null;
        _parts.armRIK.GetChain(0).target.position = point;
        _parts.armLIK.GetChain(0).target.position = point + right * GrabAdd;
    }

    /// <summary> Checks if its clear to grab by circlecasting up and to the side </summary>
    private bool CheckIfGrabIsClear(Vector2 upCheckStart, RaycastHit2D grabMid, RaycastHit2D grabDown, Vector2 right) {
        upCheckStart += right * (grabMid.distance - grabCheckRadiusV);
        float upCheckDist = grabTopOffset - grabDown.distance + grabCheckRadiusV;
        RaycastHit2D upCheck = Physics2D.CircleCast(upCheckStart, grabCheckRadiusV, tf.up, upCheckDist, whatIsGround);
        Vector2 sideCheckStart = upCheckStart + right * GrabAdd +
                                 (Vector2) tf.up * (grabTopOffset - grabDown.distance + GrabAdd + grabCheckRadiusH);
        Vector2 sideCheckSize = new Vector2(GrabAdd, grabCheckRadiusH);
        RaycastHit2D sideCheck = Physics2D.CapsuleCast(sideCheckStart, sideCheckSize, CapsuleDirection2D.Vertical, 0f,
                                                       right, grabCheckDistanceH, whatIsGround);
        //TODO if upCheck clear and side check not, we might still be able to pull up with a (different?) animation, or we can hang on the ledge
#if UNITY_EDITOR
        if(_visualizeDebug) {
            Debug.DrawRay(upCheckStart, tf.up * upCheckDist, Color.red);
            Debug.DrawRay(sideCheckStart, right * GrabAdd, Color.red);
        }
#endif
        return !upCheck && !sideCheck;
    }

    /// <summary> Update whether or not this character is touching the ground </summary>
    private void UpdateGrounded() {
        bool wasGrounded;
        if(!_rolling) {
            /* This checks both feet to see if they are touching the ground, and if they are, it checks the angle of the ground they are on.
             * For each foot, a raycast checks the back of the foot, and if that hits nothing, then the front of the foot is checked.
             * Then, for any feet that are touching the ground, we check the angle of the ground, and use the larger angle, as long as it's
             * still less than the maxWalkSlope. This is so that uneven surfaces will still be walked up quickly.
             * walkSlope is set to the chosen angle, and is used in the Move(...) method. */
            RaycastHit2D rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -tf.up,
                                                      groundCheckDistance, whatIsGround);
            if(!rightHit)
                rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -tf.up,
                                             groundCheckDistance, whatIsGround);
            RaycastHit2D leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -tf.up,
                                                     groundCheckDistance, whatIsGround);
            if(!leftHit)
                leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -tf.up,
                                            groundCheckDistance, whatIsGround);
            float rightAngle = Vector2.Angle(rightHit.normal, tf.up);
            float leftAngle = Vector2.Angle(leftHit.normal, tf.up);

            // Pick the larger angle that is still within bounds
            bool rightGreater = rightAngle > leftAngle && rightAngle < maxWalkSlope || leftAngle > maxWalkSlope;
            groundNormal = rightGreater && rightHit ? rightHit.normal :
                               (leftHit ? leftHit.normal : (Vector2) tf.up);
            walkSlope = rightGreater ? rightAngle : leftAngle;

            wasGrounded = grounded;
            grounded = (rightHit || leftHit) && walkSlope < maxWalkSlope;
        } else { // Rolling
            // If the character is rolling, their feet will leave the ground, but we still want to consider them as touching
            // the ground, so we instead do a raycast out from the center, and set grounded and walkSlope using this.
            RaycastHit2D rollHit = Physics2D.Raycast(tf.position, -tf.up, rollingGroundCheckDistance, whatIsGround);
            groundNormal = rollHit ? rollHit.normal : (Vector2) tf.up;
            walkSlope = Vector2.Angle(rollHit.normal, tf.up);
            wasGrounded = grounded;
            grounded = rollHit && walkSlope < maxWalkSlope;
        }
        // Set this so the animator can play or transition to/from the appropriate animations
        anim.SetBool(_groundedAnim, grounded);
        // If the character was grounded and isn't anymore, but they didn't jump, then they must have walked off a ledge or something
        if(wasGrounded && !grounded && !_jumpStarted) _falling = true;
        if(_falling && !_rolling) {
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
                Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -tf.up * groundCheckDistance);
                Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -tf.up * groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -tf.up * groundCheckDistance);
                Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -tf.up * groundCheckDistance);
            } else {
                Debug.DrawRay(tf.position, -tf.up * rollingGroundCheckDistance);
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
        float velTangent = Vector2.Dot(rb.velocity, tangent); // And use this so that the animation looks right
#if UNITY_EDITOR
        if(_visualizeDebug) Debug.DrawRay(_parts.footL.transform.position, tangent, Color.blue);
#endif
        float slopeReducer = Mathf.Lerp(1, .7f, walkSlope / maxWalkSlope); // reduce speed as slopes increase
        moveVec = slopeReducer * rb.mass * tangent * _acceleration * moveIn * Time.fixedDeltaTime;

        Action<float> addKick = force => { // If walking from standstill, gives a kick so walking is more responsive
            //TODO C# 7 use local function
            Vector2 kickAdd = rb.mass * tangent * force * 30 * slopeReducer;
            if(moveIn > 0 && velForward < _maxSpeed / 3) rb.AddForce(kickAdd);
            else if(moveIn < 0 && velForward > -_maxSpeed / 3) rb.AddForce(-kickAdd);
        };

        // checks if intention is not same direction as motion
        bool moveDirIsNotVelDir = Math.Sign(moveIn) != Math.Sign(velForward);

        if(grounded) {
            float sprintAmt = sprint ? _sprintSpeed : 1;
            moveVec *= sprintAmt; // We do this here because we don't want them to be able to start sprinting in mid-air

            // Set animation params
            _walkSprint = Mathf.Abs(velTangent) <= _maxSpeed + 1f ? Mathf.Abs(velTangent) / _maxSpeed / 2
                              : Mathf.Abs(velTangent) / (_maxSpeed * _sprintSpeed);
            // avg it with player intention
            _walkSprint = (_walkSprint + Mathf.Abs(moveIn / 2 * _sprintSpeed * slopeReducer)) / 2;
            // avg it for smoothing
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(_walkSprint, 2f, 1f, Time.fixedDeltaTime));

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

            addKick(_kick);

            if(moveIn > 0 && !facingRight || moveIn < 0 && facingRight) Flip();
        } else { // Not grounded
            // Here we switch to air contol mode.
            // First, make sure the character isn't trying to move into a wall or something, since otherwise they'll stick to it
            if(!_isTouching) {
                // If they aren't let them control their movement some
                moveVec *= _airControl;
                addKick(_kickAir);
                if(movePressed && (moveIn > 0 && velForward < _maxSpeed || moveIn < 0 && velForward > -_maxSpeed)) {
                    rb.AddForce(moveVec, ForceMode2D.Impulse);
                }
            }
            // If they aren't holding move in the direction of motion, slow them down a little
            if(!movePressed || moveDirIsNotVelDir)
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * _airSlowdownMultiplier;
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(0, 1)); //TODO not used while in midair yet
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
            rb.velocity += (Vector2) tf.up * _jumpSpeed;
            anim.SetTrigger(rollJump ? _rollJumpAnim : _jumpAnim);
        } else if(jump && _jumpFuelLeft > 0) {
            // Make the character rise higher the longer they hold jump
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            rb.AddForce(tf.up * rb.mass * _jumpFuelForce, ForceMode2D.Force);
            float grav = Mathf.Lerp(0.0f, 1, (_jumpFuel - _jumpFuelLeft) / _jumpFuel);
            rb.gravityScale = grav;
        } else {
            //But as soon as they release jump, let gravity take over
            rb.gravityScale = 1f;
            _jumpFuelLeft = 0;
        }

        // If landed, allow jumping again
        if(grounded && !jump) _jumpStarted = false;

        // Set the vertical animation for moving up/down through the air
        if(!grounded) anim.SetFloat(_vSpeedAnim, rb.velocity.y / 8);
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
//        if(!_grabbing) UpdateGrab(collInfo.GetContact(0));
//        print(collInfo.collider + "    :::   " + Time.time);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionExit2D(Collision2D collInfo) {
        _isTouching = false;
    }
}