using System.Collections.Generic;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    public abstract Dictionary<GameObject, Vector3> PartsToLPositions { get; }
    public abstract Dictionary<GameObject, GameObject> PartsToTargets { get; }
}