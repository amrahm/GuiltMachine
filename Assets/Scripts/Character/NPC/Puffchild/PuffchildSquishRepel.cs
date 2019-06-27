using System;
using UnityEngine;

public class PuffchildSquishRepel : MonoBehaviour {
    [Tooltip("How much this should repel when compressed"), SerializeField] private float repelForce = 200;
    [Tooltip("How much this should stick to surfaces to prevent bounce"), SerializeField] private int stickiness = 4;
    private Collider2D _repelTrigger;
    [NonSerialized] public bool touching;
    [NonSerialized] public Vector2 groundedNormal;
    private Rigidbody2D _rb;

    void Start() {
        _rb = GetComponentInParent<Rigidbody2D>();
        _repelTrigger = GetComponent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D other) {
        touching = true;
        ColliderDistance2D dist = _repelTrigger.Distance(other);
        _rb.AddForce((dist.distance * repelForce - Vector2.Dot(dist.normal, _rb.velocity) * stickiness) * dist.normal);
        groundedNormal = -dist.normal;
    }

    private void OnTriggerExit2D(Collider2D other) { touching = false; }
}