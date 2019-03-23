using System.Linq;
using UnityEngine;

public abstract class PartsAbstract : MonoBehaviour {
    /// <summary> An array of all the _real_ parts of this character </summary>
    [HideInInspector] public GameObject[] parts;

    /// <summary> An array of all the target parts of this character (the fake, animated parts) </summary>
    [HideInInspector] public GameObject[] targets;

    /// <summary> Initialize the parts and targets arrays </summary>
    protected internal abstract void AddPartsToLists();

    private void Awake() { AddPartsToLists(); }
}