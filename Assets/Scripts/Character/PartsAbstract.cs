using System;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    [NonSerialized] public GameObject[] parts;
    [NonSerialized] public GameObject[] targets;

    /// <summary> Initialize the parts and targets arrays </summary>
    protected abstract void AddPartsToLists();

    private void Awake() { AddPartsToLists(); }
}