using System;
using System.Collections.Generic;
using Anima2D;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PhoenixParts : PartsAbstract {
    public GameObject hips, torso, head;
    public GameObject wingR, forearmR;
    public GameObject wingL, forearmL;
    public GameObject tailMain, tailTop, tailMid, tailBottom;

    public GameObject hipsTarget, torsoTarget, headTarget;
    public GameObject wingRTarget, forearmRTarget;
    public GameObject wingLTarget, forearmLTarget;
    public GameObject tailMainTarget, tailTopTarget, tailMidTarget, tailBottomTarget;
    private void Awake() {
        AddPartsToLists();
    }
#if UNITY_EDITOR
    private void Update() {
        if(EditorApplication.isPlaying ) return;
        AddPartsToLists();
    }
#endif

    private void AddPartsToLists() {
        List<GameObject> partsTemp = new List<GameObject>();
        List<GameObject> targetsTemp = new List<GameObject>();

        Action<GameObject, GameObject> addToLists = (part, target) => {
            partsTemp.Add(part);
            targetsTemp.Add(target);
#if UNITY_EDITOR
            if(EditorApplication.isPlaying)
#endif
                Destroy(target.GetComponent<Bone2D>()); //We don't actually need these anymore
        };

        addToLists(hips, hipsTarget);
        addToLists(torso, torsoTarget);
        addToLists(head, headTarget);
        addToLists(wingR, wingRTarget);
        addToLists(forearmR, forearmRTarget);
        addToLists(wingL, wingLTarget);
        addToLists(forearmL, forearmLTarget);
        addToLists(tailMain, tailMainTarget);
        addToLists(tailTop, tailTopTarget);
        addToLists(tailMid, tailMidTarget);
        addToLists(tailBottom, tailBottomTarget);

        parts = partsTemp.ToArray();
        targets = targetsTemp.ToArray();
    }
}
