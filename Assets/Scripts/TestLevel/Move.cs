using UnityEngine;

public class Move : MonoBehaviour
{
    // for different types of movements set these
    public bool vertical = false;
    public float speed = .01f;
    public float offset = 4f;

    private bool positive = true;
    private float startingPos;
    private Rigidbody2D _rb;

    private Vector3 pos {
        get => transform.position;
        set {
            _rb = GetComponent<Rigidbody2D>();
            if(_rb != null) _rb.MovePosition(value);
            else transform.position = value;
        }
    }

    void Start() { startingPos = vertical ? pos.y : pos.x; }

    void FixedUpdate()
    {
        if (vertical)
        {
            if (positive)
            {
                if (pos.y >= startingPos + offset) {
                    positive = false;
                }
                pos = new Vector3(pos.x,pos.y+speed,pos.z);
            }
            else
            {
                if (pos.y <= startingPos - offset)
                {
                    positive = true;
                }
                pos = new Vector3(pos.x, pos.y - speed, pos.z);
            }
        }
        else
        {
            if (positive)
            {
                if (pos.x >= startingPos + offset)
                {
                    positive = false;
                }
                pos = new Vector3(pos.x+speed, pos.y, pos.z);
            }
            else
            {
                if (pos.x <= startingPos - offset)
                {
                    positive = true;
                }
                pos = new Vector3(pos.x-speed, pos.y , pos.z);
            }
        }
    }
}
