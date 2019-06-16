using UnityEngine;

public class PuffchildSquish : MonoBehaviour {
    public float checkDist = 0.5f;
    public GameObject repelFieldGameObject;
    private Vector3 _startPos;
    private LayerMask _whatIsGround;

    private void Start() {
        _startPos = transform.localPosition;
        _whatIsGround = CommonObjectsSingleton.Instance.whatIsGroundMaster.layerMask & ~(1 << gameObject.layer);
    }

    private void Update() {
        Vector3 pos = transform.parent.TransformPoint(_startPos);
        float actualCheckDist = ((Vector2) repelFieldGameObject.transform.localScale).magnitude * checkDist;
        RaycastHit2D hitCheck = Physics2D.Raycast(pos, transform.right, actualCheckDist, _whatIsGround);
        transform.position =
            pos + Vector3.Project(-(actualCheckDist - hitCheck.distance) * transform.right, hitCheck.normal);
    }
}