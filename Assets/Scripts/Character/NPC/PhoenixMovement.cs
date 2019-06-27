using UnityEngine;

public class PhoenixMovement : MovementAbstract {
    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;

    private void FixedUpdate() {
        if(FacingRight != control.moveHorizontal > 0) {
            Flip();
        }

        Vector2 dir = new Vector2(control.moveHorizontal, control.moveVertical);
        dir *= speed * Time.fixedDeltaTime;

        // Move the AI
        rb.AddForce(dir * rb.mass, fMode);
    }
}