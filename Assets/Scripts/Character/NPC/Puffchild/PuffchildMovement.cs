using ExtensionMethods;
using System;
using UnityEngine;

public class PuffchildMovement : MovementAbstract {
    #region Variables

    [Tooltip("The fastest the character can travel in the x axis"), SerializeField]
    private float maxSpeed = 10f;

    [Tooltip("How fast the character speeds up"), SerializeField]
    private float acceleration = 1;

    [Tooltip("A kick to make the character start moving faster"), SerializeField]
    private float kick = 1;

    [Tooltip("How much character automatically slows down while not walking but on the ground"), SerializeField]
    private float groundSlowdownMultiplier;

    [Tooltip("How much character can steer while in midair"), SerializeField]
    private float airControl;

    [Tooltip("How much character automatically slows down while in midair"), SerializeField]
    private float airSlowdownMultiplier;

    [Tooltip("The max speed the character can reach in midair under its own power"), SerializeField]
    private float airMaxSpeed = 3f;

    [Tooltip("A kick to make the character start moving faster while in midair"), SerializeField]
    private float kickAir;

    [Tooltip("Vertical speed of a jump"), SerializeField]
    private float jumpSpeed = 5;

    [Tooltip("Amount of time a jump can be held to jump higher"), SerializeField]
    private float jumpFuel = 100f;

    [Tooltip("Additional force added while holding jump"), SerializeField]
    private float jumpFuelForce = 30f;

    /// <summary> Whether or not the character is touching something </summary>
    private bool _isTouching;

    /// <summary> Speed at which character should rotate </summary>
    private float _rotationSpeed;

    /// <summary> How much jump fuel is left. Starts at _jumpFuel and moves to 0 </summary>
    private float _jumpFuelLeft;

    /// <summary> True if the character is still holding jump after jumping </summary>
    private bool _jumpStarted;

    /// <summary> initial gravity scale of this character </summary>
    private float _initGravity;

    /// <summary> Is the gravity currently zero </summary>
    private bool _gravityZeroed;

    private PuffchildSquishRepel _squishRepelScript;
    private GameObject _sprite;

    #endregion

    protected override void Awake() {
        base.Awake();
        _initGravity = rb.gravityScale;
        _squishRepelScript = GetComponentInChildren<PuffchildSquishRepel>();
        _sprite = _squishRepelScript.gameObject;
    }

    private void FixedUpdate() {
        grounded = _squishRepelScript.touching && Vector2.Dot(_squishRepelScript.groundedNormal, Vector2.up) > -.2;
        if(grounded) groundNormal = _squishRepelScript.groundedNormal;
        UpdateCoyoteGrounded();

        if(!disableMovement) {
            Move(control.moveHorizontal, control.moveVertical);
            Jump(control.jumpPressed, control.moveVertical);
        }
    }

    /// <summary> Handles character walking and running </summary>
    /// <param name="moveIn">Walking input</param>
    /// <param name="upDown">Weather puffchild should move up or down in ambiguous situations</param>
    private void Move(float moveIn, float upDown) {
        // Calculate the tangent to the ground so that the player can walk smoothly up and down slopes
        Vector2 tangent = grounded ? Vector3.Cross(groundNormal, Vector3.forward) : tf.right;
        // And flip it if the player is on a wall and wants to go down instead of up (or vice versa)
        // If no upDown intention is signalled, default to up
        float tangentUpness = moveIn * Vector2.Dot(tangent, tf.up);
        bool upDownPressed = Mathf.Abs(upDown) > .2f;
        int flipRotation = 1;
        bool gravityZeroedThisFrame = false;
        if(!upDownPressed && tangentUpness > .9) {
            tangent = tf.right / 4;
            gravityZeroedThisFrame = _gravityZeroed = true;
            rb.gravityScale = 0;
            rb.velocity -= Time.deltaTime * 6 * rb.velocity.Projected(tf.up);
        } else if(tangentUpness * (upDownPressed ? Mathf.Sign(upDown) : 1) < -.8f) {
            tangent *= -1;
            flipRotation = -1;
        }

        float velTangent = Vector2.Dot(rb.velocity, tangent);
        if(upDownPressed && tangentUpness > .9 && velTangent * moveIn < 0) {
            rb.gravityScale = 0;
            gravityZeroedThisFrame = _gravityZeroed = true;
            rb.velocity -= Time.deltaTime * 6 * rb.velocity.Projected(tf.up);
        }

        if(!gravityZeroedThisFrame && _gravityZeroed) {
            rb.gravityScale = _initGravity;
            _gravityZeroed = false;
        }

        float slopeReducer = Mathf.Lerp(1, .7f, walkSlope / maxWalkSlope); // reduce speed as slopes increase
        moveVec = slopeReducer * rb.mass * acceleration * moveIn * Time.fixedDeltaTime * tangent;

        _rotationSpeed = grounded ? -velTangent * flipRotation : _rotationSpeed.SharpInDamp(0, .2f);
        _sprite.transform.Rotate(Vector3.forward, _rotationSpeed);

        void AddKick(float force) { // If walking from standstill, gives a kick so walking is more responsive
            Vector2 kickAdd = rb.mass * force * 30 * slopeReducer * tangent;
            if(moveIn > 0 && velTangent < maxSpeed / 3) rb.AddForce(kickAdd);
            else if(moveIn < 0 && velTangent > -maxSpeed / 3) rb.AddForce(-kickAdd);
        }

        // checks if intention is not same direction as motion
        bool moveDirIsNotVelDir = Math.Sign(moveIn) != Math.Sign(velTangent);

        if(grounded) {
            // Check that the character wants to walk, but isn't walking too fast
            if(Mathf.Abs(moveIn) > 0.1f && (Mathf.Abs(velTangent) < maxSpeed || moveDirIsNotVelDir)) {
                rb.AddForce(moveVec, ForceMode2D.Impulse);
            } else {
                // slow the character down so that movement isn't all slidey
                rb.velocity -= velTangent * Time.fixedDeltaTime * groundSlowdownMultiplier * (Vector2) tf.right;
            }
            AddKick(kick);
        } else { // Not grounded
//            
            // Here we switch to air contol mode.
            moveVec *= airControl;
            AddKick(kickAir);
            if(moveIn > 0 && velTangent < airMaxSpeed || moveIn < 0 && velTangent > -airMaxSpeed)
                rb.AddForce(moveVec, ForceMode2D.Impulse);

            // If they aren't holding move in the direction of motion, slow them down a little
            if(Mathf.Abs(moveIn) < 0.1f || moveDirIsNotVelDir)
                rb.velocity -= velTangent * Time.fixedDeltaTime * airSlowdownMultiplier * (Vector2) tf.right;
        }
    }

    /// <summary> Handles character jumping </summary>
    /// <param name="jump">Is jump input pressed?</param>
    /// <param name="upDown">What direction to jump if on wall</param>
    private void Jump(bool jump, float upDown) {
        Vector2 JumpDir() {
            float wallSteepness = 1 - Vector2.Dot(groundNormal, tf.up);
            return wallSteepness * (1 + Mathf.Clamp(upDown, -1, .2f)) * .85f * (Vector2) tf.up +
                   groundNormal * (1 - wallSteepness * Mathf.Clamp(upDown, -.25f, .65f));
        }

        if((grounded || coyoteGrounded) && jump && !_jumpStarted) {
            // Initialize the jump
            _jumpFuelLeft = jumpFuel;
            _jumpStarted = true; //This ensures we don't repeat this step a bunch of times per jump
            rb.velocity = rb.velocity.Projected(tf.right) + JumpDir() * jumpSpeed;
        } else {
            if(jump && _jumpFuelLeft > 0) {
                // Make the character rise higher the longer they hold jump
                _jumpFuelLeft -= Time.fixedDeltaTime * 500;
                rb.AddForce(rb.mass * jumpFuelForce * JumpDir(), ForceMode2D.Force);
            } else {
                _jumpFuelLeft = 0;
            }
        }

        // If landed, allow jumping again
        if(grounded && !jump) _jumpStarted = false;
    }
}