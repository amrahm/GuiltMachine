using Cinemachine;
using ExtensionMethods;
using UnityEngine;

public class CinemachineDampingControl : MonoBehaviour {
    [SerializeField] private Camera cam;
    private CinemachineFramingTransposer _frame;
    private float _xDamp;
    private float _yDamp;

    private void Awake() {
        if(!cam) cam = Camera.main;
        _frame = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        _xDamp = _frame.m_XDamping;
        _yDamp = _frame.m_YDamping;
    }

    private void Update() {
        Vector3 screenPos = cam.WorldToViewportPoint(_frame.TrackedPoint);
        _frame.m_XDamping = _frame.m_XDamping.SharpInDamp(Mathf.Lerp(_xDamp, 0, Mathf.Abs(screenPos.x - 0.5f) * 2));
        _frame.m_YDamping = _frame.m_YDamping.SharpInDamp(Mathf.Lerp(_yDamp, 0, Mathf.Abs(screenPos.y - 0.5f) * 2));
    }
}