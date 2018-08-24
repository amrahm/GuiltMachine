using System;
using ExtensionMethods;
using UnityEngine;

public class PlayerMovement : MovementAbstract {
    #region Variables

    public override bool FacingRight { get; set; } = true;
    public override LayerMask WhatIsGround { get; set; }
    public override bool Grounded { get; set; }
    public override Vector2 MoveVec { get; set; }
    public override float MaxWalkSlope { get; set; }
    public override float WalkSlope { get; set; }
    public override Vector2 GroundNormal { get; set; }

#if UNITY_EDITOR
    [Tooltip("Show debug visualizations, such as red line for tangent to floor and circle for setting groundCheckOffset")] [SerializeField]
    private bool _visualizeDebug;
#endif

    [Tooltip("The fastest the player can travel in the x axis")] [SerializeField]
    private float _maxSpeed = 10f;

    [Tooltip("How fast the player speeds up")] [SerializeField]
    private float _acceleration = 1;

    [Tooltip("A kick to make the player start moving faster")] [SerializeField]
    private float _kick = 1;

    [Tooltip("How much player automatically slows down while not walking but on the ground")] [SerializeField]
    private float _groundSlowdownMultiplier;

    [Tooltip("Sprint multiplier for when running")] [SerializeField]
    private float _sprintSpeed = 2;

    [Tooltip("The greatest slope that the character can walk up")] [SerializeField]
    private float _maxWalkSlope = 50;

    [Tooltip("Vertical speed of a jump")] [SerializeField]
    private float _jumpSpeed = 5;

    [Tooltip("Amount of time a jump can be held to jump higher")] [SerializeField]
    private float _jumpFuel = 100f;

    [Tooltip("Additional force added while holding jump")] [SerializeField]
    private float _jumpFuelForce = 30f;

    [Tooltip("How much player can steer while in mid-air")] [SerializeField]
    private float _airControl;

    [Tooltip("How much player automatically slows down while in mid-air")] [SerializeField]
    private float _airSlowdownMultiplier;

    [Tooltip("A kick to make the player start moving faster while in mid-air")] [SerializeField]
    private float _kickAir;

    [Tooltip("A mask determining what is ground to the character")] [SerializeField]
    private LayerMask _whatIsGround;

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


    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the player is crouching </summary>
    private bool _crouching;

    /// <summary> Position of the foot last frame when crouching </summary>
    private float _lastFootPos;

    /// <summary> Rigidbody component of the gameObject </summary>
    private Rigidbody2D _rb;

    /// <summary> Used to zero out friction when moving. True if the Player's feet have no friction. </summary>
    private bool _frictionZero;

    /// <summary> How much jump fuel is left. Starts at _jumpFuel and moves to 0 </summary>
    private float _jumpFuelLeft;

    /// <summary> True if the player is still holding jump after jumping </summary>
    private bool _jumpStarted;

    /// <summary> Whether or not the player is touching something </summary>
    private bool _isTouching;

    /// <summary> Reference to the player's animator component </summary>
    private Animator _anim;

    /// <summary> Reference to Parts script, which contains all of the player's body parts </summary>
    private PlayerParts _parts;

    /// <summary> Tthe physics material on the foot colliders </summary>
    private PhysicsMaterial2D _footFrictionMat;

    /// <summary> The speed parameter in the animator </summary>
    private int _speedAnim;

    /// <summary> The vertical speed parameter in the animator </summary>
    private int _vSpeedAnim;

    /// <summary> The crouching parameter in the animator </summary>
    private int _crouchingAnim;

    /// <summary> The roll parameter in the animator </summary>
    private int _rollAnim;

    #endregion

    private void Awake() {
        //Setting up references.
        _parts = GetComponent<PlayerParts>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        //Instance the foot physics material so that we can adjust it without affecting other users of this script
        var footMat = _parts.footR.GetComponent<Collider2D>().sharedMaterial;
        _footFrictionMat = new PhysicsMaterial2D(footMat.name + " (Instance)") {friction = footMat.friction};
        _parts.footR.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;
        _parts.footL.GetComponent<Collider2D>().sharedMaterial = _footFrictionMat;

        _speedAnim = Animator.StringToHash("Speed");
        _vSpeedAnim = Animator.StringToHash("vSpeed");
        _crouchingAnim = Animator.StringToHash("Crouching");
        _rollAnim = Animator.StringToHash("Roll");

        WhatIsGround = _whatIsGround;
        MaxWalkSlope = _maxWalkSlope;
    }

    private void FixedUpdate() {
        //The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        RaycastHit2D rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -transform.up, groundCheckDistance, _whatIsGround);
        if(rightHit.collider == null)
            rightHit = Physics2D.Raycast(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -transform.up, groundCheckDistance, _whatIsGround);
        RaycastHit2D leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -transform.up, groundCheckDistance, _whatIsGround);
        if(leftHit.collider == null)
            leftHit = Physics2D.Raycast(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -transform.up, groundCheckDistance, _whatIsGround);
        float rightAngle = Vector2.Angle(rightHit.normal, transform.up);
        float leftAngle = Vector2.Angle(leftHit.normal, transform.up);

        bool rightGreater = rightAngle > leftAngle && rightAngle < _maxWalkSlope || leftAngle > _maxWalkSlope; //pick the larger angle that is still within bounds
        GroundNormal = rightGreater ? rightHit.normal : leftHit.normal;
        WalkSlope = rightGreater ? rightAngle : leftAngle;

        Grounded = (rightHit.collider != null || leftHit.collider != null) && WalkSlope < _maxWalkSlope;
#if UNITY_EDITOR
        if(_visualizeDebug) {
            Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR), -transform.up * groundCheckDistance);
            Debug.DrawRay(_parts.footR.transform.TransformPoint(groundCheckOffsetR2), -transform.up * groundCheckDistance);
            Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL), -transform.up * groundCheckDistance);
            Debug.DrawRay(_parts.footL.transform.TransformPoint(groundCheckOffsetL2), -transform.up * groundCheckDistance);
        }
#endif

        _anim.SetFloat(_vSpeedAnim, _rb.velocity.y); //Set the vertical animation for moving up/down through the air

        if(Grounded) {
            //When the feet move up relative to the hips, move the player down so that the feet stay on the ground instead of lifting into the air
            _rb.transform.position += new Vector3(0, (_parts.hips.transform.position.y - _parts.footR.transform.position.y - _lastFootPos) / 2);
            _lastFootPos = _parts.hips.transform.position.y - _parts.footR.transform.position.y;
        }
    }

    /// <summary> Handles player walking and running </summary>
    /// <param name="move">Walking input</param>
    /// <param name="movePressed">Whether walking input is pressed</param>
    /// <param name="sprint"> Sprinting input</param>
    public void Move(float move, bool movePressed, float sprint) {
        float sprintAmt = Mathf.Lerp(1, _sprintSpeed, sprint);
        Vector2 tangent = Grounded ? Vector3.Cross(GroundNormal, Vector3.forward) : transform.right;
        float velForward = _rb.velocity.x;
        float velTangent = Vector2.Dot(_rb.velocity, tangent);
#if UNITY_EDITOR
        if(_visualizeDebug) Debug.DrawRay(_parts.footL.transform.position, tangent, Color.red);
#endif
        MoveVec = _rb.mass * tangent * _acceleration * sprintAmt * move * Time.fixedDeltaTime;
        float slopeReducer = Mathf.Lerp(1, .6f, WalkSlope / _maxWalkSlope);
        MoveVec *= slopeReducer; //reduce speed as slopes increase

        Action<float> kick = force => { //If pressing walk from standstill, gives a kick so walking is more responsive
            if(move > 0 && velForward < _maxSpeed / 3) _rb.AddForce(_rb.mass * tangent * force * 30 * slopeReducer);
            else if(move < 0 && velForward > -_maxSpeed / 3) _rb.AddForce(_rb.mass * tangent * -force * 30 * slopeReducer);
        };

        if(Grounded) {
            _walkSprint = Mathf.Abs(velTangent) <= _maxSpeed + 1f ? Mathf.Abs(velTangent) / _maxSpeed / 2 : Mathf.Abs(velTangent) / (_maxSpeed * _sprintSpeed);
            _walkSprint = (_walkSprint + Mathf.Abs(move / 2 * _sprintSpeed * slopeReducer)) / 2; //avg it with intention
            _anim.SetFloat(_speedAnim, _anim.GetFloat(_speedAnim).SharpInDamp(_walkSprint, 2f, 1f, Time.fixedDeltaTime)); //avg it out for smoothing
//            _anim.SetFloat("Speed", Mathf.Abs(move / 2 * _sprintSpeed));

            if(movePressed && Mathf.Abs(velForward) < _maxSpeed * sprintAmt) {
                _rb.AddForce(MoveVec, ForceMode2D.Impulse);
                if(!_frictionZero) {
                    _footFrictionMat.friction = 0;
                    _frictionZero = true;
                }
            } else {
                if(_frictionZero) {
                    _footFrictionMat.friction = 1;
                    _frictionZero = false;
                }
                _rb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * _groundSlowdownMultiplier;
            }

            kick(_kick);

            if(move > 0 && !FacingRight || move < 0 && FacingRight) Flip();
        } else { //Not grounded
            //Make sure the player isn't trying to move into a wall or something, since otherwise they'll stick to it
            if(!_isTouching) {
                MoveVec *= _airControl;
                kick(_kickAir);
                if(movePressed && (move > 0 && velForward < _maxSpeed * sprintAmt || move < 0 && velForward > -_maxSpeed * sprintAmt)) {
                    _rb.AddForce(MoveVec, ForceMode2D.Impulse);
                }
            }
            _rb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * _airSlowdownMultiplier;
            _anim.SetFloat(_speedAnim, _anim.GetFloat(_speedAnim).SharpInDamp(0, 1));
        }
    }

    /// <summary> Handles player jumping </summary>
    /// <param name="jump">Is jump input pressed</param>
    public void Jump(bool jump) {
        if(Grounded && jump && !_jumpStarted) {
            _jumpFuelLeft = _jumpFuel;
            _jumpStarted = true;
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
        } else if(jump && _jumpFuelLeft > 0) {
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            _rb.AddForce(new Vector2(0f, _rb.mass * _jumpFuelForce), ForceMode2D.Force);
            float grav = Mathf.Lerp(0.0f, 1, (_jumpFuel - _jumpFuelLeft) / _jumpFuel);
            _rb.gravityScale = grav;
        } else {
            _rb.gravityScale = 1f;
            _jumpFuelLeft = 0;
        }
        if(Grounded && !jump) {
            _jumpStarted = false;
        }
    }

    /// <summary> Handles player crouching </summary>
    /// <param name="crouching">Is down input pressed</param>
    public void Crouch(bool crouching) {
        bool wasStanding = !_crouching;
        _crouching = Grounded && crouching && _walkSprint < .65f;
        _anim.SetBool(_crouchingAnim, _crouching);

        bool roll = wasStanding && _crouching && _walkSprint > .01f;
        _anim.SetBool(_rollAnim, roll);
    }


    ///<summary> Flip the player around the y axis </summary>
    private void Flip() {
        FacingRight = !FacingRight; //Switch the way the player is labelled as facing.

        //Multiply the player's x local scale by -1.
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionEnter2D(Collision2D collInfo) {
        _isTouching = true;
    }

    // ReSharper disable once UnusedParameter.Local
    private void OnCollisionExit2D(Collision2D collInfo) {
        _isTouching = false;
    }
}