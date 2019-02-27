using UnityEngine;
using System.Collections;
using Cinemachine;
using ExtensionMethods;

public class CameraShake : MonoBehaviour {
    private static CameraShake _instance;
    private CinemachineBasicMultiChannelPerlin _noise;

    private void Awake() {
        _instance = this;
        _noise = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public static void Shake(float duration, float amount) {
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.ShakeHelper(duration, amount));
    }

    private IEnumerator ShakeHelper(float duration, float amount) {
        float endTime = Time.time + duration;
        _noise.m_FrequencyGain = amount * 20;
        _noise.m_AmplitudeGain = amount * 20;
        while(Time.time < endTime) {
            _noise.m_AmplitudeGain = _noise.m_AmplitudeGain.SharpInDamp(0, 1 / duration);
            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}