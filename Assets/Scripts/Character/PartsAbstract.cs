using System;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    [NonSerialized] public GameObject[] parts;
    [NonSerialized] public GameObject[] targets;
}