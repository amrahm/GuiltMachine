using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour {
    public bool receiveShadow = true;

    private void Start() {
        GetComponent<Renderer>().receiveShadows = receiveShadow;
        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
}