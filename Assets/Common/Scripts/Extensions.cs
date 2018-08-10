using System.Reflection;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace ExtensionMethods {
    public static class Extensions {
        public static float SharpOutDamp(float current, float target, float smoothTime, float deltaTime) {
            float diff = Mathf.Abs(current - target);
            float v = 9 / (diff * smoothTime) * deltaTime;
            v = v * v;
            return Mathf.Lerp(current, target, v);
        }

        #region Overloads

        public static float SharpOutDamp(float current, float target, float smoothTime) {
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        public static float SharpOutDampAngle(float current, float target, float smoothTime, float deltaTime) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpOutDamp(current, target, smoothTime, deltaTime);
        }

        public static float SharpOutDampAngle(float current, float target, float smoothTime) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        public static Vector3 SharpOutDamp(Vector3 current, Vector3 target, float smoothTime, float deltaTime) {
            float diff = Vector3.Distance(current, target);
            float v = 9 / (diff * smoothTime) * deltaTime;
            v = v * v;
            return Vector3.Lerp(current, target, v);
        }

        public static Vector3 SharpOutDamp(Vector3 current, Vector3 target, float smoothTime) {
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        #endregion

        /// <summary> Interpolate from current to target quickly when they are far apart, and slower when they are close </summary>
        /// <param name="current">Current float to interpolate from </param>
        /// <param name="target">Target value</param>
        /// <param name="speed">How quickly to move between them</param>
        /// <param name="factor">Scales what values are considered "far apart", from 0 to 1 probably</param>
        /// <param name="deltaTime">Time between frames, e.g. Time.deltaTime</param>
        /// <returns>Float between current and target</returns>
        public static float SharpInDamp(float current, float target, float speed, float factor, float deltaTime) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Mathf.Lerp(current, target, v);
        }

        #region Overloads
        public static float SharpInDamp(float current, float target, float speed, float factor) {
            return SharpInDamp(current, target, speed, factor, Time.deltaTime);
        }

        public static float SharpInDamp(float current, float target, float speed) {
            return SharpInDamp(current, target, speed, 1, Time.deltaTime);
        }

        public static float SharpInDampAngle(float current, float target, float speed, float factor, float deltaTime) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpInDamp(current, target, speed, factor, deltaTime);
        }

        public static float SharpInDampAngle(float current, float target, float speed, float factor) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpInDamp(current, target, speed, factor, Time.deltaTime);
        }

        public static float SharpInDampAngle(float current, float target, float speed) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpInDamp(current, target, speed, 1, Time.deltaTime);
        }

        public static Vector3 SharpInDamp(Vector3 current, Vector3 target, float speed, float factor, float deltaTime) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Vector3.Lerp(current, target, v);
        }

        public static Vector3 SharpInDamp(Vector3 current, Vector3 target, float speed, float factor) {
            return SharpInDamp(current, target, speed, factor, Time.deltaTime);
        }

        public static Vector3 SharpInDamp(Vector3 current, Vector3 target, float speed) {
            return SharpInDamp(current, target, speed, 1, Time.deltaTime);
        }

        public static Quaternion SharpInDamp(Quaternion current, Quaternion target, float speed, float factor, float deltaTime) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Quaternion.Slerp(current, target, v);
        }

        public static Quaternion SharpInDamp(Quaternion current, Quaternion target, float speed, float factor) {
            return SharpInDamp(current, target, speed, factor, Time.deltaTime);
        }

        public static Quaternion SharpInDamp(Quaternion current, Quaternion target, float speed) {
            return SharpInDamp(current, target, speed, 1, Time.deltaTime);
        }

        #endregion

        public static string GetTooltip(FieldInfo field, bool inherit) {
            TooltipAttribute[] attributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];

            string ret = "";
            if(attributes != null && attributes.Length > 0)
                ret = attributes[0].tooltip;

            return ret;
        }
    }
}