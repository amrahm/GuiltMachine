using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {
    [SerializeField] private float _forceMult = 100;
    public GameObject torso, head;
    public GameObject upperArmR, lowerArmR, handR;
    public GameObject upperArmL, lowerArmL, handL;
    public GameObject thighR, shinR, footR;
    public GameObject thighL, shinL, footL;

    public GameObject torsoTarget, headTarget;
    public GameObject upperArmRTarget, lowerArmRTarget, handRTarget;
    public GameObject upperArmLTarget, lowerArmLTarget, handLTarget;
    public GameObject thighRTarget, shinRTarget, footRTarget;
    public GameObject thighLTarget, shinLTarget, footLTarget;

    public PIDController torsoPID, headPID;
    public PIDController upperArmRpid, lowerArmRpid, handRpid;
    public PIDController upperArmLpid, lowerArmLpid, handLpid;
    public PIDController thighRpid, shinRpid, footRpid;
    public PIDController thighLpid, shinLpid, footLpid;
    public float p, i, d;

    private readonly Dictionary<GameObject, float> _targetOldRotations = new Dictionary<GameObject, float>();


    private void FixedUpdate() {
        RotateTo(torso, torsoTarget, torsoPID);
        RotateTo(head, headTarget, headPID);
        RotateTo(upperArmR, upperArmRTarget, upperArmRpid);
        RotateTo(lowerArmR, lowerArmRTarget, lowerArmRpid);
        RotateTo(handR, handRTarget, handRpid);
        RotateTo(upperArmL, upperArmLTarget, upperArmLpid);
        RotateTo(lowerArmL, lowerArmLTarget, lowerArmLpid);
        RotateTo(handL, handLTarget, handLpid);
        RotateTo(thighR, thighRTarget, thighRpid);
        RotateTo(shinR, shinRTarget, shinRpid);
        RotateTo(footR, footRTarget, footRpid);
        RotateTo(thighL, thighLTarget, thighLpid);
        RotateTo(shinL, shinLTarget, shinLpid);
        RotateTo(footL, footLTarget, footLpid);
    }

    private void RotateTo(GameObject obj, GameObject target, PIDController pid) {
        if(_targetOldRotations.ContainsKey(target)) {
//            obj.GetComponent<Rigidbody2D>().MoveRotation(_targetOldRotations[target] - target.transform.eulerAngles.z);
        }
        _targetOldRotations[target] = target.transform.eulerAngles.z;


        pid.kp = p;
        pid.ki = i;
        pid.kd = d;

        var hinge = obj.GetComponent<HingeJoint2D>();
        var motor = hinge.motor;
        float dir = -Vector3.Dot(obj.transform.up, target.transform.right) / Mathf.Abs(Vector3.Dot(obj.transform.up, target.transform.right));
        float speed = pid.Update(Vector3.Angle(obj.transform.right, target.transform.right));
        speed *= Vector3.Angle(obj.transform.right, target.transform.right);
        motor.motorSpeed = _forceMult * speed * dir;
        hinge.motor = motor;
        hinge.useMotor = true;


        if(obj.name == "Head") {
            Debug.Log(_forceMult * speed * dir);
//            Debug.DrawRay(obj.transform.position, obj.transform.up);
//            Debug.DrawRay(target.transform.position, target.transform.right, Color.blue);
        }
    }
}