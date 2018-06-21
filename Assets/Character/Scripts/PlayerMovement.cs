using System;
using ExtensionMethods;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private float _maxSpeed = 10f; //The fastest the player can travel in the x axis.
    [SerializeField] private float _acceleration = 1; //How fast the player speeds up
    [SerializeField] private float _kick = 1; //A kick to make the player start moving faster
    [SerializeField] private float _sprintSpeed = 2; //Sprint multiplier
    [SerializeField] private float _jumpForce = 400f; //Amount of force added when the player jumps.
    [SerializeField] private float _jumpFuel = 100f; //Amount of time a jump can be held to jump higher.
    [SerializeField] private float _airControl; //How much player can steer while jumping;
    [SerializeField] private LayerMask _whatIsGround; //A mask determining what is ground to the character

    public Transform groundCheck; //A position marking where to check if the player is grounded.
    public GameObject torso, head, footR, footL;
    private const float GroundedRadius = .2f; //Radius of the overlap circle to determine if grounded
    private bool _grounded; //Whether or not the player is grounded.
    private Animator _anim; //Reference to the player's animator component.
    private Rigidbody2D _rb, _torsoRb, _headRb;
    public bool facingRight = true; //For determining which way the player is currently facing.
    private bool _frictionZero; //Used to zero out friction when moving
    private float _jumpFuelLeft;
    private bool _jumpStarted;

    private void Awake() {
        //Setting up references.
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _torsoRb = torso.GetComponent<Rigidbody2D>();
        _headRb = head.GetComponent<Rigidbody2D>();
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
        float velForward = _rb.velocity.x;
        float sprintAmt = Mathf.Lerp(1, _sprintSpeed, sprint);
        Vector2 fwdVec = _rb.mass * transform.right * _acceleration * sprintAmt * move * Time.fixedDeltaTime;
        if(_grounded) {
            float walkSprint = Mathf.Abs(velForward) <= _maxSpeed + 1f ?
                                   Mathf.Abs(velForward) / _maxSpeed / 2 :
                                   Mathf.Abs(velForward) / (_maxSpeed * _sprintSpeed);
            _anim.SetFloat("Speed", walkSprint);

            if(movePressed && Mathf.Abs(velForward) < _maxSpeed * sprintAmt) {
                _rb.AddForce(fwdVec, ForceMode2D.Impulse);
                if(!_frictionZero) {
                    footL.GetComponent<Collider2D>().sharedMaterial.friction = 0;
                    footR.GetComponent<Collider2D>().sharedMaterial.friction = 0;
                    _frictionZero = true;
                }
            } else {
                if(_frictionZero) {
                    footL.GetComponent<Collider2D>().sharedMaterial.friction = 1;
                    footR.GetComponent<Collider2D>().sharedMaterial.friction = 1;
                    _frictionZero = false;
                }
                _rb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * 30;
                _headRb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * 30;
                _torsoRb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * 30;
            }

            //::Kick - If pressing walk from standstill, gives a kick so walking is more responsive.
            if(move > 0 && velForward < _maxSpeed / 3) {
                _rb.AddForce(_rb.mass * transform.right * _kick * 30);
                _headRb.AddForce(_rb.mass * transform.right * _kick * 5);
                _torsoRb.AddForce(_rb.mass * transform.right * _kick * 5);
//                Debug.Log("kickf " + Time.time); //\\\\\\\\\\\\\\\\\\\\\\\\\\
            } else if(move < 0 && velForward > -_maxSpeed / 3) {
                _rb.AddForce(_rb.mass * transform.right * -_kick * 30);
                _headRb.AddForce(_rb.mass * transform.right * -_kick * 5);
                _torsoRb.AddForce(_rb.mass * transform.right * -_kick * 5);
//                Debug.Log("kickb " + Time.time); //\\\\\\\\\\\\\\\\\\\\\\\\\\
            }

            if(move > 0 && !facingRight || move < 0 && facingRight) Flip();
        } else { //Not grounded
            fwdVec *= _airControl;
            if(movePressed && (move > 0 && velForward < _maxSpeed * sprintAmt || move < 0 && velForward > -_maxSpeed * sprintAmt)) {
                _rb.AddForce(fwdVec, ForceMode2D.Impulse);
            }
            _rb.velocity -= (Vector2) transform.right * velForward * Time.fixedDeltaTime * 3;
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
            _rb.AddForce(new Vector2(0f, _jumpForce * _rb.mass), ForceMode2D.Impulse);
            _torsoRb.AddForce(new Vector2(0f, _jumpForce * _torsoRb.mass * 0.97f));
            _headRb.AddForce(new Vector2(0f, _jumpForce * _headRb.mass * 0.9f));
        } else if(jump && _jumpFuelLeft > 0) {
            _jumpFuelLeft -= Time.fixedDeltaTime * 500;
            _rb.AddForce(new Vector2(0f, _jumpForce * _rb.mass * 3), ForceMode2D.Force);
            _rb.gravityScale = 0.0f;
            _torsoRb.gravityScale = 0.0f;
            _headRb.gravityScale = 0.0f;
        } else {
            _rb.gravityScale = 1f;
            _torsoRb.gravityScale = 1f;
            _headRb.gravityScale = 1f;
            _jumpFuelLeft = 0;
        }
        if(_grounded && !jump) {
            _jumpStarted = false;
        }
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
//        head.GetComponent<HingeJoint2D>().limits = flipLimits(head); //I don't know why this isn't needed, but ...
        torso.GetComponent<HingeJoint2D>().limits = flipLimits(torso);
    }
}