using System;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    [NonSerialized] public GameObject[] parts;

    /// <summary> Initialize the parts and targets arrays </summary>
    protected abstract void AddPartsToLists();

    private void Awake() { AddPartsToLists(); }
}