using System;
using ExtensionMethods;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HumanoidParts))]
[RequireComponent(typeof(CharacterMasterAbstract))]
public class HumanoidMovement : MovementAbstract {
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

    [Tooltip("Offset for the right foot ground check raycast")]
    public Vector2 groundCheckOffsetR;

    [Tooltip("Offset for the right foot ground check raycast")]
    public Vector2 groundCheckOffsetR2;

    [Tooltip("Offset for the left foot ground check raycast")]
    public Vector2 groundCheckOffsetL;

    [Tooltip("Offset for the left foot ground check raycast")]
    public Vector2 groundCheckOffsetL2;

    [Tooltip("How long is the ground check raycast")]
    public float groundCheckDistance;

    [Tooltip("How long is the grab check raycast")]
    public float grabDistance = 1;

    [Tooltip("How far along the horizontal check is the downward check")]
    public float grabDownDistanceRatio = 0.7f;

    [Tooltip("How high is the highest grabbable")]
    public float grabTopOffset = 1.5f;

    [Tooltip("How high is the middle grabbable")]
    public float grabMidOffset;

    [Tooltip("How high is the lowest grabbable")]
    public float grabBottomOffset = -0.75f;


    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the character is crouching </summary>
    private bool _crouching;

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

    /// <summary> The land parameter in the animator </summary>
    private int _groundedAnim;

    /// <summary> The fall parameter in the animator </summary>
    private int _fallingAnim;

    /// <summary> Grab mid check vector </summary>
    private Vector2 _grabMidVec;

    /// <summary> Downward pointing grab check vector </summary>
    private Vector2 _grabDownVec;

    #endregion

    protected override void Awake() {
        //Setting up references.
        base.Awake();
        _parts = GetComponent<HumanoidParts>();

        whatIsGround = whatIsGroundMaster.whatIsGround & ~(1 << gameObject.layer); //remove current layer

        //Instance the foot physics material so that we can adjust it without affecting other users of this script
        var footMat = _parts.footR.GetComponent<Collider2D>().sharedMaterial;
        _footFrictionMat = new PhysicsMaterial2D(footMat.name + " (Instance)") {friction = footMat.friction};
        _parts.footR.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;
        _parts.footL.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;

        //Initialize animator parameters
        _speedAnim = Animator.StringToHash("Speed");
        _vSpeedAnim = Animator.StringToHash("vSpeed");
        _crouchingAnim = Animator.StringToHash("Crouching");
        _rollAnim = Animator.StringToHash("Roll");
        _jumpAnim = Animator.StringToHash("Jump");
        _groundedAnim = Animator.StringToHash("Grounded");
        _fallingAnim = Animator.StringToHash("Falling");


        //Initialize grab check vectors
        _grabMidVec = new Vector2(grabMidOffset, 0);
        _grabDownVec = new Vector2(grabDistance * grabDownDistanceRatio, grabTopOffset);
    }

    private void FixedUpdate() {
        UpdateGrounded();
        UpdateGrab();
        Move(control.moveHorizontal, control.hPressed, control.sprint);
        Jump(control.upPressed);
        Crouch(control.downPressed);
        
        if(grounded) {
            //When the feet move up relative to the hips, move the player down so that the feet stay on the ground instead of lifting into the air
            float hipsDelta = _parts.hips.transform.position.y - _parts.footR.transform.position.y;
            rb.transform.position += new Vector3(0, (hipsDelta - _lastHipsDelta) / 2);
            _lastHipsDelta = hipsDelta;
        }
    }

    /// <summary> Update whether or not this character can/is grab/bing a platform </summary>
    private void UpdateGrab() {
        //TODO Should we only start calling this immediately after hitting a wall?
        //TODO that could also give us a second mid to check, mid being at the point of collision
        //TODO and would generally be more efficient
        const float grabAdd = 0.1f;
        Vector2? ledgePoint = null;
        Vector2 right = facingRight ? tf.right : -tf.right;
        RaycastHit2D grabMid = Physics2D.Raycast(tf.TransformPoint(_grabMidVec), right, grabDistance, whatIsGround);
        RaycastHit2D grabDown;
        if(grabMid.collider != null) { //First check for mid
            Vector2 grabDownOrigin = tf.TransformPoint(new Vector2(grabMid.distance + grabAdd, grabTopOffset));
            grabDown = Physics2D.Raycast(grabDownOrigin, -tf.up, grabTopOffset - grabBottomOffset, whatIsGround);
            if(Mathf.Abs(Vector2.Dot(grabDown.point - grabDownOrigin, tf.up)) > 0.1f)
                ledgePoint = grabDown.point;

        } else if((grabDown = Physics2D.Raycast(tf.TransformPoint(_grabDownVec), -tf.up,
                       grabTopOffset - grabBottomOffset, whatIsGround)).collider != null) {
            //If mid that fails, check for down
            Vector2 grabMidOrigin = tf.TransformPoint(new Vector2(0, grabTopOffset - grabDown.distance - grabAdd));
            grabMid = Physics2D.Raycast(grabMidOrigin, right, grabDistance, whatIsGround);
            if(Mathf.Abs(Vector2.Dot(grabMid.point - grabMidOrigin, tf.right)) > 0.1f)
                ledgePoint = new Vector2(grabMid.point.x + (facingRight ? grabAdd : -grabAdd), grabDown.point.y);
        }
#if UNITY_EDITOR
        if(_visualizeDebug) {
            if(ledgePoint != null) DebugExtension.DebugPoint((Vector3) ledgePoint, Color.red);
            _grabMidVec = new Vector2(0, grabMidOffset);
            _grabDownVec = new Vector2(grabDistance * grabDownDistanceRatio, grabTopOffset);
            Debug.DrawRay(tf.TransformPoint(_grabMidVec), right * grabDistance);
            Debug.DrawRay(tf.TransformPoint(_grabDownVec), -tf.up * (grabTopOffset - grabBottomOffset), Color.green);
        }
#endif
    }

    /// <summary> Update whether or not this character is touching the ground </summary>
    private void UpdateGrounded() {
        //The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        RaycastHit2D rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -tf.up, groundCheckDistance, whatIsGround);
        if(rightHit.collider == null)
            rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -tf.up, groundCheckDistance, whatIsGround);
        RaycastHit2D leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -tf.up, groundCheckDistance, whatIsGround);
        if(leftHit.collider == null)
            leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -tf.up, groundCheckDistance, whatIsGround);
        float rightAngle = Vector2.Angle(rightHit.normal, tf.up);
        float leftAngle = Vector2.Angle(leftHit.normal, tf.up);

        //pick the larger angle that is still within bounds
        bool rightGreater = rightAngle > leftAngle && rightAngle < maxWalkSlope || leftAngle > maxWalkSlope;
        groundNormal = rightGreater && rightHit.collider != null ?
                           rightHit.normal :
                           (leftHit.collider != null ? leftHit.normal : (Vector2) tf.up);
        walkSlope = rightGreater ? rightAngle : leftAngle;

        bool wasGrounded = grounded;
        grounded = (rightHit.collider != null || leftHit.collider != null) && walkSlope < maxWalkSlope;
        anim.SetBool(_groundedAnim, grounded);
        if(wasGrounded && !grounded && !_jumpStarted) {
            _falling = true;
        }
        if(_falling) {
            _fallDuration += Time.fixedDeltaTime;
            print(_fallDuration);
            if(_fallDuration > 0.15f) anim.SetBool(_fallingAnim, true); //falling without jumping
        }
        if(grounded) {
            _fallDuration = 0;
            _falling = false;
            anim.SetBool(_fallingAnim, false);
        }


#if UNITY_EDITOR
        if(_visualizeDebug) {
            Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -tf.up * groundCheckDistance);
            Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -tf.up * groundCheckDistance);
            Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -tf.up * groundCheckDistance);
            Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -tf.up * groundCheckDistance);
        }
#endif
    }

    /// <summary> Handles player walking and running </summary>
    /// <param name="move">Walking input</param>
    /// <param name="movePressed">Whether walking input is pressed</param>
    /// <param name="sprint"> Whether sprint input is pressed </param>
    private void Move(float move, bool movePressed, bool sprint) {
        Vector2 tangent = grounded ? Vector3.Cross(groundNormal, Vector3.forward) : tf.right;
        float velForward = rb.velocity.x;
        float velTangent = Vector2.Dot(rb.velocity, tangent);
#if UNITY_EDITOR
        if(_visualizeDebug) Debug.DrawRay(_parts.footL.transform.position, tangent, Color.blue);
#endif
        float slopeReducer = Mathf.Lerp(1, .7f, walkSlope / maxWalkSlope); //reduce speed as slopes increase
        moveVec = slopeReducer * rb.mass * tangent * _acceleration * move * Time.fixedDeltaTime;

        Action<float> kick = force => { //If pressing walk from standstill, gives a kick so walking is more responsive
            if(move > 0 && velForward < _maxSpeed / 3) rb.AddForce(rb.mass * tangent * force * 30 * slopeReducer);
            else if(move < 0 && velForward > -_maxSpeed / 3) rb.AddForce(rb.mass * tangent * -force * 30 * slopeReducer);
        };

        if(grounded) {
            float sprintAmt = sprint ? _sprintSpeed : 1;
            moveVec *= sprintAmt;

            //Set animation params
            _walkSprint = Mathf.Abs(velTangent) <= _maxSpeed + 1f ? Mathf.Abs(velTangent) / _maxSpeed / 2 : Mathf.Abs(velTangent) / (_maxSpeed * _sprintSpeed);
            _walkSprint = (_walkSprint + Mathf.Abs(move / 2 * _sprintSpeed * slopeReducer)) / 2; //avg it with intention
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(_walkSprint, 2f, 1f, Time.fixedDeltaTime)); //avg it out for smoothing
//            _anim.SetFloat("Speed", Mathf.Abs(move / 2 * _sprintSpeed));

            if(movePressed && Mathf.Abs(velForward) < _maxSpeed * sprintAmt) {
                rb.AddForce(moveVec, ForceMode2D.Impulse);
                if(!_frictionZero) {
                    _footFrictionMat.friction = 0;
                    _frictionZero = true;
                }
            } else {
                if(_frictionZero) {
                    _footFrictionMat.friction = 1;
                    _frictionZero = false;
                }
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * _groundSlowdownMultiplier;
            }

            kick(_kick);

            if(move > 0 && !facingRight || move < 0 && facingRight) Flip();
        } else { //Not grounded
            //Make sure the player isn't trying to move into a wall or something, since otherwise they'll stick to it
            if(!_isTouching) {
                moveVec *= _airControl;
                kick(_kickAir);
                if(movePressed && (move > 0 && velForward < _maxSpeed || move < 0 && velForward > -_maxSpeed)) {
                    rb.AddForce(moveVec, ForceMode2D.Impulse);
                }
            }
            if(!movePressed || Math.Abs(Mathf.Sign(move) - Mathf.Sign(velForward)) > 0.5f )
                rb.velocity -= (Vector2) tf.right * velForward * Time.fixedDeltaTime * _airSlowdownMultiplier;
            anim.SetFloat(_speedAnim, anim.GetFloat(_speedAnim).SharpInDamp(0, 1));
        }
    }

    /// <summary> Handles player jumping </summary>
    /// <param name="jump">Is jump input pressed</param>
    private void Jump(bool jump) {
#if UNITY_EDITOR
        if(_allowJumpingInMidair) grounded = true;
#endif
        if(grounded && jump && !_jumpStarted) {
            _jumpFuelLeft = _jumpFuel;
            _jumpStarted = true;
            rb.velocity = new Vector2(rb.velocity.x, _jumpSpeed);
            anim.SetTrigger(_jumpAnim);
        } else if(jump && _jumpFuelLeft > 0) {
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            rb.AddForce(new Vector2(0f, rb.mass * _jumpFuelForce), ForceMode2D.Force);
            float grav = Mathf.Lerp(0.0f, 1, (_jumpFuel - _jumpFuelLeft) / _jumpFuel);
            rb.gravityScale = grav;
        } else {
            rb.gravityScale = 1f;
            _jumpFuelLeft = 0;
        }

        if(grounded && !jump) _jumpStarted = false;
        if(!grounded) anim.SetFloat(_vSpeedAnim, rb.velocity.y / 8); //Set the vertical animation for moving up/down through the air
    }

    /// <summary> Handles player crouching </summary>
    /// <param name="crouching">Is down input pressed</param>
    private void Crouch(bool crouching) {
        bool wasStanding = !_crouching;
        _crouching = grounded && crouching && _walkSprint < .65f;
        anim.SetBool(_crouchingAnim, _crouching);

        bool roll = wasStanding && _crouching && _walkSprint > .01f;
        anim.SetBool(_rollAnim, roll);
    }


    ///<summary> Flip the player around the y axis </summary>
    private void Flip() {
        facingRight = !facingRight; //Switch the way the player is labelled as facing.

        //Multiply the player's x local scale by -1.
        tf.localScale = new Vector3(-tf.localScale.x, tf.localScale.y, tf.localScale.z);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionEnter2D(Collision2D collInfo) {
        _isTouching = true;
//        print(collInfo.collider + "    :::   " + Time.time);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionExit2D(Collision2D collInfo) {
        _isTouching = false;
    }
}