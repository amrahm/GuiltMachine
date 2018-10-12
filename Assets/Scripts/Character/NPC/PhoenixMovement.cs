using UnityEngine;

public class PhoenixMovement : MovementAbstract {
    /// <summary> Rigidbody component of the gameObject </summary>
    private Rigidbody2D _rb;

    /// <summary> Reference to Control script, which gives input to this script </summary>
    private CharacterControlAbstract _control;

    // The AI's speed per second (not framerate dependent)
    public float speed = 500f;
    public ForceMode2D fMode = ForceMode2D.Force;

	void Start () {
	    _rb = GetComponent<Rigidbody2D>();
	    _control = GetComponent<CharacterControlAbstract>();
		
	}
	
	void FixedUpdate () {
	    Vector2 dir = new Vector2(_control.moveHorizontal, _control.moveVertical);
	    dir *= speed * Time.fixedDeltaTime;

	    // Move the AI
	    _rb.AddForce(dir * _rb.mass, fMode);
		
	}
}
