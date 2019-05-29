﻿using UnityEngine;

public class SootballMovement : MovementAbstract {
    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;

    // The AI's jump speed
    public float jumpSpeed = 2;

    [Tooltip("How long is the ground check raycast")]
    public float groundCheckDistance;

    [Tooltip("Offset for the ground check raycast")]
    public Vector2 groundCheckOffset;

    private void FixedUpdate() {
        UpdateGrounded();

        if(FacingRight != control.moveHorizontal > 0) {
            Flip();
        }

        Vector2 dir = new Vector2(control.moveHorizontal, control.moveVertical);
        dir *= speed * Time.fixedDeltaTime;

        // Move the AI
        rb.AddForce(dir * rb.mass, fMode);

        // Get it to jump
        if(control.moveVertical > 0 && grounded) {
            rb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
        }
    }


    private void UpdateGrounded() {
        RaycastHit2D bodyHit =
            Physics2D.Raycast(gameObject.GetComponent<CircleCollider2D>().transform.TransformPoint(groundCheckOffset),
                              Vector2.down, groundCheckDistance, WhatIsGround);

        grounded = !(bodyHit.collider is null);

        Debug.DrawRay(gameObject.GetComponent<CircleCollider2D>().transform.TransformPoint(groundCheckOffset),
                      Vector2.down * groundCheckDistance);
    }
}