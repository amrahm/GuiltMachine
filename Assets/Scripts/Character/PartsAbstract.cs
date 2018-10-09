using System.Collections.Generic;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    public Dictionary<GameObject, Vector3> PartsToLPositions = new Dictionary<GameObject, Vector3>();
    public Dictionary<GameObject, GameObject> PartsToTargets = new Dictionary<GameObject, GameObject>();
}