using UnityEditor;
using UnityEngine;

public class PhoenixParts : PartsAbstract {
    private void Awake() {
        AddPartsToLists();
    }
#if UNITY_EDITOR
    private void Update() {
        if(EditorApplication.isPlaying ) return;
        AddPartsToLists();
    }
#endif

    protected void AddPartsToLists() {
        //TODO
    }
}
