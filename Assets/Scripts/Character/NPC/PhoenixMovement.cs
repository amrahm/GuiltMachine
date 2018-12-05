using UnityEngine;

public class PhoenixMovement : MovementAbstract {
    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;

    protected override void Awake() {
        //Setting up references.
        base.Awake();
        facingRight = false; //sprite was drawn facing the other way lol
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
}