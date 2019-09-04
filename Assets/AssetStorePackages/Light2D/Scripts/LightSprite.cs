using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Light2D {
    /// <summary>
    ///     Used to draw lights. Puts LightOrigin world position to UV1.
    ///     Supports Point and Line light types.
    /// </summary>
    [ExecuteInEditMode]
    public class LightSprite : CustomSprite {
        public enum LightShape {
            Point,
            Line
        }

        public static readonly List<LightSprite> AllLightSprites = new List<LightSprite>();
        private Matrix4x4 _modelMatrix;
        private Vector3 _oldLightOrigin;
        private LightShape _oldLightShape;
        public Vector3 lightOrigin = new Vector3(0, 0, 1);
        public LightShape shape = LightShape.Point;
        private static readonly int LightPos = Shader.PropertyToID("_LightPos");

        public MeshRenderer Renderer => meshRenderer;

        protected override void OnEnable() {
            base.OnEnable();
            AllLightSprites.Add(this);
        }

        private void OnDisable() {
            AllLightSprites.Remove(this);
        }

        /// <summary>
        ///     Update UV1 which is used for raytracking in shader. UV1 is set to world position of LightOrigin.
        /// </summary>
        private void UpdatePosition() {
            if(sprite == null || !Application.isPlaying)
                return;

            Matrix4x4 mat = _modelMatrix;
            Vector2 size = sprite.bounds.size;

            if(shape == LightShape.Point) {
                // LightOrigin needs to be send in world position instead of local because 
                // Unity non uniform scaling is breaking model matrix in shader.
                Vector3 pos = mat.MultiplyPoint(((Vector2) lightOrigin).Mul(size));
                if(!LightingSystem.Instance.xzPlane) {
                    for(int i = 0; i < uv1.Length; i++)
                        uv1[i] = pos;
                } else {
                    Vector2 p = new Vector2(pos.x, pos.z);
                    for(int i = 0; i < uv1.Length; i++)
                        uv1[i] = p;
                }
            } else if(shape == LightShape.Line) {
                Vector3 lpos = mat.MultiplyPoint(new Vector2(-0.5f, lightOrigin.y).Mul(size));
                Vector3 rpos = mat.MultiplyPoint(new Vector2(0.5f, lightOrigin.y).Mul(size));
                if(!LightingSystem.Instance.xzPlane) {
                    uv1[0] = lpos;
                    uv1[1] = rpos;
                    uv1[2] = lpos;
                    uv1[3] = rpos;
                } else {
                    Vector2 lp = new Vector2(lpos.x, lpos.z);
                    Vector2 rp = new Vector2(rpos.x, rpos.z);
                    uv1[0] = lp;
                    uv1[1] = rp;
                    uv1[2] = lp;
                    uv1[3] = rp;
                }
            }
        }

        protected override void UpdateMeshData(bool forceUpdate = false) {
            if(IsPartOfStaticBatch)
                return;

            Matrix4x4 objMat = transform.localToWorldMatrix;
            if(!objMat.FastEquals(_modelMatrix) ||
               _oldLightOrigin != lightOrigin || _oldLightShape != shape || forceUpdate) {
                _modelMatrix = objMat;
                _oldLightOrigin = lightOrigin;
                _oldLightShape = shape;
                UpdatePosition();
                isMeshDirty = true;
            }

            base.UpdateMeshData(forceUpdate);
        }

        private void OnDrawGizmosSelected() {
            if(sprite == null)
                return;

            Vector3 size = sprite.bounds.size;
            if(shape == LightShape.Point) {
                Vector3 center = transform.TransformPoint(lightOrigin);
                Gizmos.DrawLine(
                    center + transform.TransformDirection(new Vector2(-0.1f, 0)),
                    center + transform.TransformDirection(new Vector2(0.1f, 0)));
                Gizmos.DrawLine(
                    center + transform.TransformDirection(new Vector2(0, -0.1f)),
                    center + transform.TransformDirection(new Vector2(0, 0.1f)));
            } else if(shape == LightShape.Line && sprite != null) {
                Vector3 lpos = transform.TransformPoint(new Vector3(-0.5f, lightOrigin.y).Mul(size));
                Vector3 rpos = transform.TransformPoint(new Vector3(0.5f, lightOrigin.y).Mul(size));
                Gizmos.DrawLine(lpos, rpos);
            }
        }

        public void DrawLightingNow(Vector2 lightCamLocalPos) {
            Material sharedMat = meshRenderer.sharedMaterial;

            if(!sharedMat.SetPass(0))
                return;

            Vector3 v1 = _modelMatrix.MultiplyPoint(vertices[0]) - (Vector3) lightCamLocalPos;
            Vector3 v2 = _modelMatrix.MultiplyPoint(vertices[2]) - (Vector3) lightCamLocalPos;
            Vector3 v3 = _modelMatrix.MultiplyPoint(vertices[3]) - (Vector3) lightCamLocalPos;
            Vector3 v4 = _modelMatrix.MultiplyPoint(vertices[1]) - (Vector3) lightCamLocalPos;

            GL.Begin(GL.QUADS);

            GL.Color(color);

            GL.MultiTexCoord(0, uv0[0]);
            GL.MultiTexCoord(1, uv1[0] - lightCamLocalPos);
            GL.Vertex(v1);

            GL.MultiTexCoord(0, uv0[2]);
            GL.MultiTexCoord(1, uv1[2] - lightCamLocalPos);
            GL.Vertex(v2);

            GL.MultiTexCoord(0, uv0[3]);
            GL.MultiTexCoord(1, uv1[3] - lightCamLocalPos);
            GL.Vertex(v3);

            GL.MultiTexCoord(0, uv0[1]);
            GL.MultiTexCoord(1, uv1[1] - lightCamLocalPos);
            GL.Vertex(v4);

            GL.End();
        }

        public void DrawLightNormalsNow(Material mat) {
            Vector2 size = sprite.bounds.size;
            Vector2 center = _modelMatrix.MultiplyPoint3x4(((Vector2) lightOrigin).Mul(size));
            Vector4 lightPos = new Vector4(center.x, center.y, lightOrigin.z);

            mat.SetVector(LightPos, lightPos);

            if(!mat.SetPass(0))
                return;

            Vector3 v1 = _modelMatrix.MultiplyPoint3x4(vertices[0]);
            Vector3 v2 = _modelMatrix.MultiplyPoint3x4(vertices[2]);
            Vector3 v3 = _modelMatrix.MultiplyPoint3x4(vertices[3]);
            Vector3 v4 = _modelMatrix.MultiplyPoint3x4(vertices[1]);

            GL.Begin(GL.QUADS);
            GL.Vertex(v1);
            GL.Vertex(v2);
            GL.Vertex(v3);
            GL.Vertex(v4);
            GL.End();
        }
    }
}