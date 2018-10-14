using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    [HideInInspector] public GameObject[] parts;
    [HideInInspector] public GameObject[] targets;

    /// <summary> Initialize the parts and targets arrays </summary>
    protected internal abstract void AddPartsToLists();

    private void Awake() { AddPartsToLists(); }
}