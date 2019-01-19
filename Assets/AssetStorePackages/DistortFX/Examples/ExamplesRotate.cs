using UnityEngine;

public class ExamplesRotate : MonoBehaviour {

  public Transform Target;
  public float Speed = 10;
	
	void Update () {
    if (Target) {
      transform.RotateAround(Target.position, Vector3.up, Speed * Time.deltaTime);
    }
	}
}
