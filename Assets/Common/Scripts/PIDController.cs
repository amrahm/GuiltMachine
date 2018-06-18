using System;
using UnityEngine;

[Serializable]
public class PIDController {
    private float _integral;

    private float _lastError;

    [Tooltip("Derivative constant (fights oscillation)")]
    public float kd = 1f;

    [Tooltip("Integral constant (counters cumulated error)")]
    public float ki = 0.05f;

    [Tooltip("Proportional constant (counters current error)")]
    public float kp = 0.2f;

    [Tooltip("Current control value")] public float value;

    /// <summary> Update our value, based on the given error.  We assume here that the last update was Time.deltaTime seconds ago. </summary>
    /// <param name="error"> Difference between current and desired outcome. </param>
    /// <returns> Updated control value. </returns>
    public float Update(float error) {
        return Update(error, Time.deltaTime);
    }

    /// <summary> Update our value, based on the given error, which was last updated dt seconds ago. </summary>
    /// <param name="error"> Difference between current and desired outcome. </param>
    /// <param name="dt"> Time step. </param>
    /// <returns> Updated control value. </returns>
    public float Update(float error, float dt) {
        float derivative = (error - _lastError) / dt;
        _integral += error * dt;
        _lastError = error;

        value = Mathf.Clamp01(kp * error + ki * _integral + kd * derivative);
        return value;
    }
}