using System;
using ExtensionMethods;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    #region Variables

    [Tooltip("The fastest the player can travel in the x axis")] [SerializeField]
    private float _maxSpeed = 10f;

    [Tooltip("How fast the player speeds up")] [SerializeField]
    private float _acceleration = 1;

    [Tooltip("A kick to make the player start moving faster")] [SerializeField]
    private float _kick = 1;

    [Tooltip("Sprint multiplier for when running")] [SerializeField]
    private float _sprintSpeed = 2;

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

    [Tooltip("A position marking where to check if the player is grounded")]
    public Transform groundCheck;

    /// <summary> Radius of the overlap circle to determine if grounded </summary>
    private const float GroundedRadius = .2f;

    /// <summary> Whether or not the player is grounded </summary>
    private bool _grounded;

    /// <summary> Player's horizontal velocity </summary>
    private float _velForward;

    /// <summary> Number between 0 and 1 indicating transition between standing still and sprinting </summary>
    private float _walkSprint;

    /// <summary> Whether or not the player is crouching </summary>
    private bool _crouching;

    /// <summary> Rigidbody component of the GameObject </summary>
    private Rigidbody2D _rb, _torsoRb, _headRb, _armRRb, _armLRb;

    /// <summary> Which way the player is currently facing </summary>
    [NonSerialized] public bool facingRight = true;

    /// <summary> Used to zero out friction when moving. True if the Player's feet have no friction. </summary>
    private bool _frictionZero;

    /// <summary> How much jump fuel is left. Starts at _jumpFuel and moves to 0 </summary>
    private float _jumpFuelLeft;

    /// <summary> True if the player is still holding jump after jumping </summary>
    private bool _jumpStarted;

    /// <summary> Reference to the player's animator component </summary>
    private Animator _anim;

    /// <summary> Reference to Parts script, which contains all of the player's body parts </summary>
    private Parts _parts;

    #endregion

    private void Awake() {
        //Setting up references.
        _parts = GetComponent<Parts>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _torsoRb = _parts.torso.GetComponent<Rigidbody2D>();
        _headRb = _parts.head.GetComponent<Rigidbody2D>();
        _armRRb = _parts.upperArmR.GetComponent<Rigidbody2D>();
        _armLRb = _parts.upperArmL.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        _grounded = false;

        //The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        //This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, GroundedRadius, _whatIsGround);
        foreach(Collider2D c in colliders) {
            if(c.gameObject != gameObject) _grounded = true;
        }

        _anim.SetBool("Ground", _grounded); //Set the grounded animation
        _anim.SetFloat("vSpeed", _rb.velocity.y); //Set the vertical animation
    }

    /// <summary> Handles player walking and running </summary>
    /// <param name="move">Walking input</param>
    /// <param name="movePressed">Whether walking input is pressed</param>
    /// <param name="sprint"> Sprinting input</param>
    public void Move(float move, bool movePressed, float sprint) {
        _velForward = _rb.velocity.x;
        float sprintAmt = Mathf.Lerp(1, _sprintSpeed, sprint);
        Vector2 fwdVec = _rb.mass * transform.right * _acceleration * sprintAmt * move * Time.fixedDeltaTime;

        Action<float> kick = force => {
            //::Kick - If pressing walk from standstill, gives a kick so walking is more responsive.
            if(move > 0 && _velForward < _maxSpeed / 3) {
                _rb.AddForce(_rb.mass * transform.right * force * 30);
                _headRb.AddForce(_rb.mass * transform.right * force * 5);
                _torsoRb.AddForce(_rb.mass * transform.right * force * 5);
//                Debug.Log("kickf " + Time.time); //\\\\\\\\\\\\\\\\\\\\\\\\\\
            } else if(move < 0 && _velForward > -_maxSpeed / 3) {
                _rb.AddForce(_rb.mass * transform.right * -force * 30);
                _headRb.AddForce(_rb.mass * transform.right * -force * 5);
                _torsoRb.AddForce(_rb.mass * transform.right * -force * 5);
//                Debug.Log("kickb " + Time.time); //\\\\\\\\\\\\\\\\\\\\\\\\\\
            }
        };

        if(_grounded) {
            _walkSprint = Mathf.Abs(_velForward) <= _maxSpeed + 1f ?
                              Mathf.Abs(_velForward) / _maxSpeed / 2 :
                              Mathf.Abs(_velForward) / (_maxSpeed * _sprintSpeed);
            _anim.SetFloat("Speed", _walkSprint);

            if(movePressed && Mathf.Abs(_velForward) < _maxSpeed * sprintAmt) {
                _rb.AddForce(fwdVec, ForceMode2D.Impulse);
                if(!_frictionZero) {
                    _parts.footL.GetComponent<Collider2D>().sharedMaterial.friction = 0;
                    _parts.footR.GetComponent<Collider2D>().sharedMaterial.friction = 0;
                    _frictionZero = true;
                }
            } else {
                if(_frictionZero) {
                    _parts.footL.GetComponent<Collider2D>().sharedMaterial.friction = 1;
                    _parts.footR.GetComponent<Collider2D>().sharedMaterial.friction = 1;
                    _frictionZero = false;
                }
                _rb.velocity -= (Vector2) transform.right * _velForward * Time.fixedDeltaTime * 30;
                _headRb.velocity -= (Vector2) transform.right * _velForward * Time.fixedDeltaTime * 30;
                _torsoRb.velocity -= (Vector2) transform.right * _velForward * Time.fixedDeltaTime * 30;
            }

            kick(_kick);

            if(move > 0 && !facingRight || move < 0 && facingRight) Flip();
        } else { //Not grounded
            fwdVec *= _airControl;
            kick(_kickAir);
            if(movePressed && (move > 0 && _velForward < _maxSpeed * sprintAmt || move < 0 && _velForward > -_maxSpeed * sprintAmt)) {
                _rb.AddForce(fwdVec, ForceMode2D.Impulse);
            }
            _rb.velocity -= (Vector2) transform.right * _velForward * Time.fixedDeltaTime * _airSlowdownMultiplier;
            _anim.SetFloat("Speed", Extensions.SharpInDamp(_anim.GetFloat("Speed"), 0, 1));
        }
    }

    /// <summary> Handles player jumping </summary>
    /// <param name="jump">Is jump input pressed</param>
    public void Jump(bool jump) {
        if(_grounded && jump && _anim.GetBool("Ground") && !_jumpStarted) {
            _jumpFuelLeft = _jumpFuel;
            _jumpStarted = true;
            _anim.SetBool("Ground", false);
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
            _torsoRb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
            _headRb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
            _armRRb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
            _armLRb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
        } else if(jump && _jumpFuelLeft > 0) {
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            _rb.AddForce(new Vector2(0f, _rb.mass * _jumpFuelForce), ForceMode2D.Force);
            float grav = Mathf.Lerp(0.0f, 1, (_jumpFuel - _jumpFuelLeft) / _jumpFuel);
            _rb.gravityScale = grav;
            _torsoRb.gravityScale = grav;
            _headRb.gravityScale = grav;
            _armRRb.gravityScale = grav;
            _armLRb.gravityScale = grav;
        } else {
            _rb.gravityScale = 1f;
            _torsoRb.gravityScale = 1f;
            _headRb.gravityScale = 1f;
            _armRRb.gravityScale = 1f;
            _armLRb.gravityScale = 1f;
            _jumpFuelLeft = 0;
        }
        if(_grounded && !jump) {
            _jumpStarted = false;
        }
    }

    /// <summary> Handles player crouching </summary>
    /// <param name="crouching">Is down input pressed</param>
    public void Crouch(bool crouching) {
        bool wasStanding = !_crouching;
        _crouching = _grounded && crouching && _walkSprint < .65f;
        _anim.SetBool("Crouching", _crouching);

        bool roll = wasStanding && _crouching && _walkSprint > .1f;
        _anim.SetBool("Roll", roll);
    }

    ///<summary> Flip the player around the y axis </summary>
    private void Flip() {
        facingRight = !facingRight; //Switch the way the player is labelled as facing.

        //Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        Func<GameObject, JointAngleLimits2D> flipLimits = obj => {
            var limits = obj.GetComponent<HingeJoint2D>().limits;
            limits.max = limits.max + (!facingRight ? 90 : -90);
            limits.min = limits.min + (!facingRight ? 90 : -90);
            return limits;
        };
//        _parts.head.GetComponent<HingeJoint2D>().limits = flipLimits(_parts.head); //I don't know why this isn't needed, but ...
        _parts.torso.GetComponent<HingeJoint2D>().limits = flipLimits(_parts.torso);
    }
}