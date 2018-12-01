using UnityEngine;

public class PhoenixMovement : MovementAbstract {
    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;
    public Transform statusIndicator;

    protected override void Awake() {
        //Setting up references.
        base.Awake();
        facingRight = false; //sprite was drawn facing the other way lol
    }

    private void Start()
    {
        if (statusIndicator == null)
        {
            statusIndicator = GetComponentInChildren<StatusIndicator>().gameObject.transform;
        }
    }

    private void FixedUpdate() {
        if(facingRight != control.moveHorizontal > 0) {
            Flip();
        }

        Vector2 dir = new Vector2(control.moveHorizontal, control.moveVertical);
        dir *= speed * Time.fixedDeltaTime;

        // Move the AI
        rb.AddForce(dir * rb.mass, fMode);
    }

    ///<summary> Flip the player around the y axis </summary>
    private void Flip() {
        facingRight = !facingRight; //Switch the way the player is labelled as facing.
        //Multiply the player's x local scale by -1.
        tf.localScale = new Vector3(-tf.localScale.x, tf.localScale.y, tf.localScale.z);
        // Fix the status indicator to always face the proper direction regardless of phoenix orientation
        statusIndicator.localScale = new Vector3(-statusIndicator.localScale.x, statusIndicator.localScale.y, statusIndicator.localScale.z);
    }
}