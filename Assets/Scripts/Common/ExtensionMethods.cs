using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.U2D.IK;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace ExtensionMethods {
    public static class DampingExtensions {
        /// <summary> Interpolate from current to target slower when they are far apart, and quickly when they are close </summary>
        /// <param name="current">Current float to interpolate from </param>
        /// <param name="target">Target value</param>
        /// <param name="smoothTime"> How smooth (slow) to make interpolation </param>
        /// <param name="deltaTime">Time between frames, e.g. Time.deltaTime</param>
        /// <returns></returns>
        public static float SharpOutDamp(this float current, float target, float smoothTime, float deltaTime) {
            float v = 9 / (Mathf.Abs(current - target) * smoothTime) * deltaTime;
            v = v * v;
            return Mathf.Lerp(current, target, v);
        }

        #region Overloads

        public static float SharpOutDamp(this float current, float target, float smoothTime = 1) {
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        public static float SharpOutDampAngle(this float current, float target, float smoothTime, float deltaTime) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpOutDamp(current, target, smoothTime, deltaTime);
        }

        public static float SharpOutDampAngle(this float current, float target, float smoothTime = 1) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        public static Vector3 SharpOutDamp(this Vector3 current, Vector3 target, float smoothTime, float deltaTime) {
            float v = 9 / (Vector3.Distance(current, target) * smoothTime) * deltaTime;
            v = v * v;
            return Vector3.Lerp(current, target, v);
        }

        public static Vector3 SharpOutDamp(this Vector3 current, Vector3 target, float smoothTime = 1) {
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        public static Vector2 SharpOutDamp(this Vector2 current, Vector2 target, float smoothTime, float deltaTime) {
            float v = 9 / (Vector2.Distance(current, target) * smoothTime) * deltaTime;
            v = v * v;
            return Vector2.Lerp(current, target, v);
        }

        public static Vector2 SharpOutDamp(this Vector2 current, Vector2 target, float smoothTime = 1) {
            return SharpOutDamp(current, target, smoothTime, Time.deltaTime);
        }

        #endregion

        /// <summary> Interpolate from current to target quickly when they are far apart, and slower when they are close </summary>
        /// <param name="current">Current float to interpolate from </param>
        /// <param name="target">Target value</param>
        /// <param name="speed">How quickly to move between them</param>
        /// <param name="deltaTime">Time between frames, e.g. Time.deltaTime</param>
        /// <param name="factor">Scales what values are considered "far apart". Can't be 0 </param>
        /// <returns>Float between current and target</returns>
        public static float SharpInDamp(this float current, float target, float speed, float deltaTime,
                                        float factor = 1) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Mathf.Lerp(current, target, v);
        }

        #region Overloads

        public static float SharpInDamp(this float current, float target, float speed = 1) {
            return SharpInDamp(current, target, speed, Time.deltaTime);
        }

        public static float SharpInDampAngle(this float current, float target, float speed, float deltaTime,
                                             float factor = 1) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpInDamp(current, target, speed, deltaTime, factor);
        }

        public static float SharpInDampAngle(this float current, float target, float speed) {
            target = current + Mathf.DeltaAngle(current, target);
            return SharpInDamp(current, target, speed, Time.deltaTime);
        }

        public static Vector3 SharpInDamp(this Vector3 current, Vector3 target, float speed, float deltaTime,
                                          float factor = 1) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Vector3.Lerp(current, target, v);
        }

        public static Vector3 SharpInDamp(this Vector3 current, Vector3 target, float speed) {
            return SharpInDamp(current, target, speed, Time.deltaTime);
        }

        public static Vector2 SharpInDamp(this Vector2 current, Vector2 target, float speed, float deltaTime,
                                          float factor = 1) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Vector2.Lerp(current, target, v);
        }

        public static Vector2 SharpInDamp(this Vector2 current, Vector2 target, float speed) {
            return SharpInDamp(current, target, speed, Time.deltaTime);
        }

        public static Quaternion SharpInDamp(this Quaternion current, Quaternion target, float speed, float deltaTime,
                                             float factor = 1) {
            float s = speed * deltaTime;
            float v = Mathf.Pow(2, -10 * s) * Mathf.Sin((s - factor / 4) * (2 * Mathf.PI) / factor) + 1;
            return Quaternion.Slerp(current, target, v);
        }

        public static Quaternion SharpInDamp(this Quaternion current, Quaternion target, float speed) {
            return SharpInDamp(current, target, speed, Time.deltaTime);
        }

        #endregion
    }

    public static class Vector2Extensions {
        public static Vector2 Rotate(this Vector2 v, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            v.x = cos * v.x - sin * v.y;
            v.y = sin * v.x + cos * v.y;
            return v;
        }
    }

    public static class HelperMethods {
        public static string GetTooltip(FieldInfo field, bool inherit) {
            TooltipAttribute[] attributes =
                field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];

            string ret = "";
            if(attributes != null && attributes.Length > 0)
                ret = attributes[0].tooltip;

            return ret;
        }


        /// <summary> Fades an animation layer in or out over the specified duration </summary>
        /// <param name="caller"> Reference to script calling this function. Should always be "this" </param>
        /// <param name="animator"> Animator to use </param>
        /// <param name="fadeType"> Select whether fading in, out, or both </param>
        /// <param name="layerIndex"> index of animation layer to fade </param>
        /// <param name="duration"> How long the fade should take </param>
        /// <param name="smooth"> Smooth if true else linear </param>
        public static void FadeAnimationLayer(MonoBehaviour caller, Animator animator, FadeType fadeType,
                                              int layerIndex, float duration, bool smooth = false) {
            caller.StartCoroutine(
                FadeAnimationLayerHelper(caller, animator, fadeType, layerIndex, duration, 1, smooth));
        }

        /// <summary> Fades an animation layer in or out over the specified duration </summary>
        /// <param name="caller"> Reference to script calling this function. Should always be "this" </param>
        /// <param name="animator"> Animator to use </param>
        /// <param name="fadeType"> Select whether fading in, out, or both </param>
        /// <param name="layerIndex"> index of animation layer to fade </param>
        /// <param name="duration"> How long the fade should take </param>
        /// <param name="extent"> How far to fade in, if fading in </param>
        /// <param name="smooth"> Smooth if true else linear </param>
        public static void FadeAnimationLayer(MonoBehaviour caller, Animator animator, FadeType fadeType,
                                              int layerIndex, float duration, float extent, bool smooth = false) {
            caller.StartCoroutine(
                FadeAnimationLayerHelper(caller, animator, fadeType, layerIndex, duration, extent, smooth));
        }

        private static IEnumerator FadeAnimationLayerHelper(MonoBehaviour caller, Animator animator, FadeType fadeType,
                                                            int layerIndex, float duration, float extent, bool smooth) {
            float start = animator.GetLayerWeight(layerIndex);
            float end = fadeType != FadeType.FadeOut ?
                            extent :
                            0.01f; //Don't fade out all the way or the fadeIn event won't be able to be called
            float startTime = Time.time;
            float weight = 0;
            float actualDuration = fadeType == FadeType.FadeInOut ? duration / 2 : duration;
            while(weight < 0.99f) {
                weight = smooth ?
                             (fadeType == FadeType.FadeOut ?
                                  weight.SharpOutDamp(1, 1 / duration) :
                                  weight.SharpInDamp(1, duration)) :
                             (Time.time - startTime) / actualDuration;
//                Debug.Log(weight);
                animator.SetLayerWeight(layerIndex, Mathf.Lerp(start, end, weight));
                yield return null;
            }

            if(fadeType == FadeType.FadeInOut) {
                caller.StartCoroutine(FadeAnimationLayerHelper(caller, animator, FadeType.FadeOut, layerIndex,
                                                               duration / 2, extent, smooth));
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public enum FadeType {
            FadeIn,
            FadeOut,
            FadeInOut
        }
    }

    public static class ArrayExtensions {
        public static void Init<T>(this T[] array, T defaultVaue) {
            if(array == null) return;
            for(int i = 0; i < array.Length; i++) array[i] = defaultVaue;
        }
    }

    public static class ColorExtensions {
        public static Color WithAlpha(this Color color, float value) {
            color.a = value;
            return color;
        }

        public static Color32 WithAlpha(this Color32 color, byte value) {
            color.a = value;
            return color;
        }
    }

    public static class AnimatorExtensions {
        public static bool IsPlaying(this Animator animator) {
            return animator.GetCurrentAnimatorStateInfo(0).length >
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public static bool IsPlaying(this Animator animator, string stateTag) {
            return animator.IsPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsTag(stateTag);
        }
    }

    public static class Solver2DExtensions {
        public static Transform Target(this Solver2D solver, int chainIndex = 0) {
            return solver.GetChain(chainIndex).target;
        }
    }
}