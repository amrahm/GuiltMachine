using UnityEngine;
using System.Collections;
using ExtensionMethods;

public class CameraShake : MonoBehaviour {
    private static CameraShake _instance;

    private void Awake() {
        _instance = this;
    }

    public static void Shake(float duration, float amount) {
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.ShakeHelper(duration, amount));
    }

    private IEnumerator ShakeHelper(float duration, float amount) {
        float endTime = Time.time + duration;

        while(Time.time < endTime) {
            transform.localPosition += Random.insideUnitSphere * amount;

            amount = amount.SharpInDamp(0, 1 / duration);
            duration -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }
}