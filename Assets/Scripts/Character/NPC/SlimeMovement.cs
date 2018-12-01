using UnityEngine;

public class SlimeMovement : MovementAbstract
{
    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;

    // The AI's jump speed
    public float _jumpSpeed = 2;

    [Tooltip("How long is the ground check raycast")]
    public float groundCheckDistance;

    [Tooltip("Offset for the ground check raycast")]
    public Vector2 groundCheckOffset;

    protected override void Awake()
    {
        //Setting up references.
        base.Awake();

        whatIsGround = whatIsGroundMaster.whatIsGround & ~(1 << gameObject.layer); //remove current layer
    }

    private void FixedUpdate()
    {
        UpdateGrounded();

        if (facingRight != control.moveHorizontal > 0)
        {
            Flip();
        }

        Vector2 dir = new Vector2(control.moveHorizontal, control.moveVertical);
        dir *= speed * Time.fixedDeltaTime;

        // Move the AI
        rb.AddForce(dir * rb.mass, fMode);

        // Get it to jump
        if (control.moveVertical > 0 && grounded)
        {
            rb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
        }
    }

    ///<summary> Flip the player around the y axis </summary>
    private void Flip()
    {
        facingRight = !facingRight; //Switch the way the player is labelled as facing.

        //Multiply the player's x local scale by -1.
        tf.localScale = new Vector3(-tf.localScale.x, tf.localScale.y, tf.localScale.z);
    }

    private void UpdateGrounded()
    {
        RaycastHit2D bodyHit = Physics2D.Raycast(gameObject.GetComponent<CircleCollider2D>().transform.TransformPoint(groundCheckOffset), Vector2.down, groundCheckDistance, whatIsGround);

        //The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        float angle = Vector2.Angle(bodyHit.normal, tf.up);

        grounded = (bodyHit.collider != null);

        Debug.DrawRay(gameObject.GetComponent<CircleCollider2D>().transform.TransformPoint(groundCheckOffset), Vector2.down * groundCheckDistance);
    }
}