using System.Collections.Generic;
using UnityEngine;

public class Parts : MonoBehaviour {
    //A container for all the human part gameobjects so they don't have to be reassigned in a bunch of different scripts
    public GameObject hips, torso, head;
    public GameObject upperArmR, lowerArmR, handR;
    public GameObject upperArmL, lowerArmL, handL;
    public GameObject thighR, shinR, footR;
    public GameObject thighL, shinL, footL;

    public GameObject hipsTarget, torsoTarget, headTarget;
    public GameObject upperArmRTarget, lowerArmRTarget, handRTarget;
    public GameObject upperArmLTarget, lowerArmLTarget, handLTarget;
    public GameObject thighRTarget, shinRTarget, footRTarget;
    public GameObject thighLTarget, shinLTarget, footLTarget;

    public Dictionary<GameObject, Vector3> partsToLPositions = new Dictionary<GameObject, Vector3>();

    private void Awake() {
        partsToLPositions.Add(hips, hips.transform.localPosition);
        partsToLPositions.Add(torso, torso.transform.localPosition);
        partsToLPositions.Add(head, head.transform.localPosition);
        partsToLPositions.Add(upperArmR, upperArmR.transform.localPosition);
        partsToLPositions.Add(lowerArmR, lowerArmR.transform.localPosition);
        partsToLPositions.Add(handR, handR.transform.localPosition);
        partsToLPositions.Add(upperArmL, upperArmL.transform.localPosition);
        partsToLPositions.Add(lowerArmL, lowerArmL.transform.localPosition);
        partsToLPositions.Add(handL, handL.transform.localPosition);
        partsToLPositions.Add(thighR, thighR.transform.localPosition);
        partsToLPositions.Add(shinR, shinR.transform.localPosition);
        partsToLPositions.Add(footR, footR.transform.localPosition);
        partsToLPositions.Add(thighL, thighL.transform.localPosition);
        partsToLPositions.Add(shinL, shinL.transform.localPosition);
        partsToLPositions.Add(footL, footL.transform.localPosition);
    }
}
