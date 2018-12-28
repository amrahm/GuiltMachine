using System.Collections.Generic;
using Anima2D;
using UnityEditor;
using UnityEngine;

public class PhoenixParts : PartsAbstract {
    public GameObject hips, torso, head;
    public GameObject wingR, forearmR;
    public GameObject wingL, forearmL;
    public GameObject tailMain, tailTop, tailMid, tailBottom;

    public GameObject hipsTarget, torsoTarget, headTarget;
    public GameObject wingRTarget, forearmRTarget;
    public GameObject wingLTarget, forearmLTarget;
    public GameObject tailMainTarget, tailTopTarget, tailMidTarget, tailBottomTarget;

    protected internal override void AddPartsToLists() {
        List<GameObject> partsTemp = new List<GameObject>();
        List<GameObject> targetsTemp = new List<GameObject>();

        void AddToLists(GameObject part, GameObject target) {
            partsTemp.Add(part);
            targetsTemp.Add(target);
#if UNITY_EDITOR
            if(EditorApplication.isPlaying) // Needed because this is called in edit mode from CharacterPhysics
#endif
                Destroy(target.GetComponent<Bone2D>()); // We don't actually need these anymore
        }

        AddToLists(hips, hipsTarget);
        AddToLists(torso, torsoTarget);
        AddToLists(head, headTarget);
        AddToLists(wingR, wingRTarget);
        AddToLists(forearmR, forearmRTarget);
        AddToLists(wingL, wingLTarget);
        AddToLists(forearmL, forearmLTarget);
        AddToLists(tailMain, tailMainTarget);
        AddToLists(tailTop, tailTopTarget);
        AddToLists(tailMid, tailMidTarget);
        AddToLists(tailBottom, tailBottomTarget);

        parts = partsTemp.ToArray();
        targets = targetsTemp.ToArray();
    }
}