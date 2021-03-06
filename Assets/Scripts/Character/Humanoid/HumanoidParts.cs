﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D.IK;

public class HumanoidParts : PartsAbstract {
    // A container for all the human part gameobjects so they don't have to be reassigned in a bunch of different scripts
    public GameObject hips, torso, head;
    public GameObject upperArmR, lowerArmR, handR;
    public GameObject upperArmL, lowerArmL, handL;
    public GameObject thighR, shinR, footR;
    public GameObject thighL, shinL, footL;
    public GameObject weaponHolder;

    public GameObject hipsTarget, torsoTarget, headTarget;
    public GameObject upperArmRTarget, lowerArmRTarget, handRTarget;
    public GameObject upperArmLTarget, lowerArmLTarget, handLTarget;
    public GameObject thighRTarget, shinRTarget, footRTarget;
    public GameObject thighLTarget, shinLTarget, footLTarget;
    public GameObject weaponHolderTarget;

    public Solver2D armRIK, armLIK, headIK;

    protected internal override void AddPartsToLists() {
        List<GameObject> partsTemp = new List<GameObject>();
        List<GameObject> targetsTemp = new List<GameObject>();

        void AddToLists(GameObject part, GameObject target) {
            partsTemp.Add(part);
            targetsTemp.Add(target);
        }

        AddToLists(hips, hipsTarget);
        AddToLists(torso, torsoTarget);
        AddToLists(head, headTarget);
        AddToLists(upperArmR, upperArmRTarget);
        AddToLists(lowerArmR, lowerArmRTarget);
        AddToLists(handR, handRTarget);
        AddToLists(upperArmL, upperArmLTarget);
        AddToLists(lowerArmL, lowerArmLTarget);
        AddToLists(handL, handLTarget);
        AddToLists(thighR, thighRTarget);
        AddToLists(shinR, shinRTarget);
        AddToLists(footR, footRTarget);
        AddToLists(thighL, thighLTarget);
        AddToLists(shinL, shinLTarget);
        AddToLists(footL, footLTarget);
        AddToLists(weaponHolder, weaponHolderTarget);

        parts = partsTemp.ToArray();
        targets = targetsTemp.ToArray();
    }
}
