using UnityEngine;
using static PlayerPhysics;

public class PlayerPhysicsArmHelper : MonoBehaviour {
    public PlayerPhysics playerPhysics;
    public bool leftArm;

    private void OnCollisionEnter2D(Collision2D collInfo) {
        if(collInfo.gameObject.GetComponent<Rigidbody2D>()) {
            foreach(ContactPoint2D c in collInfo.contacts) {
//                Debug.Log(c.otherCollider);

                BodyPartClass part = leftArm ? playerPhysics.hitRotArmL : playerPhysics.hitRotArmR;
                part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, collInfo.gameObject.GetComponent<Rigidbody2D>().mass);
            }
        } else {
            foreach(ContactPoint2D c in collInfo.contacts) {
                BodyPartClass part = leftArm ? playerPhysics.hitRotArmL : playerPhysics.hitRotArmR;
                part.isTouching = true;
                part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, 1000);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collInfo) {
        if(collInfo.gameObject.GetComponent<Rigidbody2D>()) {
            foreach(ContactPoint2D c in collInfo.contacts) {
                BodyPartClass part = leftArm ? playerPhysics.hitRotArmL : playerPhysics.hitRotArmR;
                part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, collInfo.gameObject.GetComponent<Rigidbody2D>().mass);
            }
        } else {
            foreach(ContactPoint2D c in collInfo.contacts) {
                BodyPartClass part = leftArm ? playerPhysics.hitRotArmL : playerPhysics.hitRotArmR;
                part.isTouching = true;
                part.HitCalc(c.point, c.normal, collInfo.relativeVelocity, 1000);
            }
        }
    }
}