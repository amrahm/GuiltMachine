using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidParts : PartsAbstract {
    //A container for all the human part gameobjects so they don't have to be reassigned in a bunch of different scripts
    public GameObject hips, torso, head;
    public GameObject upperArmR, lowerArmR, handR;
    public GameObject upperArmL, lowerArmL, handL;
    public GameObject thighR, shinR, footR;
    public GameObject thighL, shinL, footL;

    protected override void AddPartsToLists() {
        List<GameObject> partsTemp = new List<GameObject> {hips, torso, head, upperArmR, lowerArmR, handR, upperArmL, 
            lowerArmL, handL, thighR, shinR, footR, thighL, shinL, footL};


        parts = partsTemp.ToArray();
    }
}