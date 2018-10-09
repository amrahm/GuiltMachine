using UnityEngine;

public class HumanoidParts : PartsAbstract {
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

    private void Awake() {
        PartsToLPositions.Add(hips, hips.transform.localPosition);
        PartsToLPositions.Add(torso, torso.transform.localPosition);
        PartsToLPositions.Add(head, head.transform.localPosition);
        PartsToLPositions.Add(upperArmR, upperArmR.transform.localPosition);
        PartsToLPositions.Add(lowerArmR, lowerArmR.transform.localPosition);
        PartsToLPositions.Add(handR, handR.transform.localPosition);
        PartsToLPositions.Add(upperArmL, upperArmL.transform.localPosition);
        PartsToLPositions.Add(lowerArmL, lowerArmL.transform.localPosition);
        PartsToLPositions.Add(handL, handL.transform.localPosition);
        PartsToLPositions.Add(thighR, thighR.transform.localPosition);
        PartsToLPositions.Add(shinR, shinR.transform.localPosition);
        PartsToLPositions.Add(footR, footR.transform.localPosition);
        PartsToLPositions.Add(thighL, thighL.transform.localPosition);
        PartsToLPositions.Add(shinL, shinL.transform.localPosition);
        PartsToLPositions.Add(footL, footL.transform.localPosition);

        PartsToTargets.Add(hips, hipsTarget);
        PartsToTargets.Add(torso, torsoTarget);
        PartsToTargets.Add(head, headTarget);
        PartsToTargets.Add(upperArmR, upperArmRTarget);
        PartsToTargets.Add(lowerArmR, lowerArmRTarget);
        PartsToTargets.Add(handR, handRTarget);
        PartsToTargets.Add(upperArmL, upperArmLTarget);
        PartsToTargets.Add(lowerArmL, lowerArmLTarget);
        PartsToTargets.Add(handL, handLTarget);
        PartsToTargets.Add(thighR, thighRTarget);
        PartsToTargets.Add(shinR, shinRTarget);
        PartsToTargets.Add(footR, footRTarget);
        PartsToTargets.Add(thighL, thighLTarget);
        PartsToTargets.Add(shinL, shinLTarget);
        PartsToTargets.Add(footL, footLTarget);
    }
}
