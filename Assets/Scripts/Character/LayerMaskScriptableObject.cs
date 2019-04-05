using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/LayerMaskScriptableObject")]
public class LayerMaskScriptableObject : ScriptableObject {
    [FormerlySerializedAs("whatIsGround")] public LayerMask layerMask;
}
