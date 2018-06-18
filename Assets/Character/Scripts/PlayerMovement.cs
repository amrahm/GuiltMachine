using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private float _maxSpeed = 10f; // The fastest the player can travel in the x axis.
    [SerializeField] private float _jumpForce = 400f; // Amount of force added when the player jumps.
    [SerializeField] private bool _airControl; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask _whatIsGround; // A mask determining what is ground to the character

    public Transform groundCheck; // A position marking where to check if the player is grounded.
    private const float GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool _grounded; // Whether or not the player is grounded.
    private Animator _anim; // Reference to the player's animator component.
    private Rigidbody2D _rb;
    public bool facingRight = true; // For determining which way the player is currently facing.

    private void Awake() {
        // Setting up references.
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        _grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, GroundedRadius, _whatIsGround);
        foreach(Collider2D c in colliders) {
            if(c.gameObject != gameObject)
                _grounded = true;
        }

        _anim.SetBool("Ground", _grounded);

        //Set the vertical animation
        _anim.SetFloat("vSpeed", _rb.velocity.y);
    }

    public void Move(float move, bool jump) {
        //only control the player if grounded or airControl is turned on
        if(_grounded || _airControl) {
            // The Speed animator parameter is set to the absolute value of the horizontal input.
            _anim.SetFloat("Speed", Mathf.Abs(move));

            // Move the character
            _rb.velocity = new Vector2(move * _maxSpeed, _rb.velocity.y);

            // If the input is moving the player right and the player is facing left...
            if(move > 0 && !facingRight) {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if(move < 0 && facingRight) {
                // ... flip the player.
                Flip();
            }
        }

        // If the player should jump...
        if(_grounded && jump && _anim.GetBool("Ground")) {
            // Add a vertical force to the player.
            _grounded = false;
            _anim.SetBool("Ground", false);
            _rb.AddForce(new Vector2(0f, _jumpForce * _rb.mass));
        }
    }

    /// <summary> Flip the player around the y axis </summary>
    private void Flip() {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}