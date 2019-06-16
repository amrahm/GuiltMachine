using System.Collections;
using ExtensionMethods;
using UnityEngine;

public class PuffchildControl : CharacterControlAbstract {
    [SerializeField, Tooltip("How far can this bad boy see?")]
    private float visionDistance;

    /// <summary> What to chase? </summary>
    private Transform _target;

    private bool _hasTarget;

    private void Start() {
        if(_target == null) {
            StartCoroutine(SearchForPlayer());
        }
    }

    private IEnumerator SearchForPlayer() {
        while(true) {
            while(!_hasTarget) {
                // Search for player every second if player is still not found
                yield return Yields.WaitForASecond;
            }

            yield return new WaitUntil(() => !_hasTarget);
        }
    }
}