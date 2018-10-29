using UnityEngine;

namespace Light2D.Examples {
    public class CameraFollower : MonoBehaviour {
        private Camera _camera;
        private Transform _cameraTransform;
        private Vector2 _smoothVelocity;
        public float camBordersMul = 0.8f;
        public float cameraPositionLerp = 0.02f;
        public Rigidbody2D followed;
        public float instantJumpDistance = 50;
        public float minAccountedSpeed = 10;
        public float velocityMul = 1;
        public float velocitySmoothnessLerp = 0.9f;

        private void OnEnable() {
            _camera = Camera.main;
            _cameraTransform = _camera.transform;
            _cameraTransform.position = _cameraTransform.position.WithXy(followed.position);
        }

        private void Start() {
            _cameraTransform.position = _cameraTransform.position.WithXy(followed.position);
        }

        private void Update() {
            if(followed != null) {
                Vector3 camPos = _cameraTransform.position;
                Vector2 followedPos = followed.position;

                Vector2 vel = followed.velocity.sqrMagnitude > minAccountedSpeed * minAccountedSpeed
                                  ? followed.velocity
                                  : Vector2.zero;
                _smoothVelocity = Vector2.Lerp(vel, _smoothVelocity, velocitySmoothnessLerp);

                Vector2 camTargetPos = followedPos + _smoothVelocity * velocityMul;
                float camHalfWidth = _camera.orthographicSize * _camera.aspect * camBordersMul;
                float camHalfHeight = _camera.orthographicSize * camBordersMul;
                Vector2 followedDir = followedPos - camTargetPos;

                if(followedDir.x > camHalfWidth)
                    camTargetPos.x = followedPos.x - camHalfWidth;
                if(followedDir.x < -camHalfWidth)
                    camTargetPos.x = followedPos.x + camHalfWidth;
                if(followedDir.y > camHalfHeight)
                    camTargetPos.y = followedPos.y - camHalfHeight;
                if(followedDir.y < -camHalfHeight)
                    camTargetPos.y = followedPos.y + camHalfHeight;

                Vector2 pos = (followedPos - (Vector2) camPos).sqrMagnitude < instantJumpDistance * instantJumpDistance
                                  ? Vector2.Lerp(camPos, camTargetPos, cameraPositionLerp * Time.deltaTime)
                                  : followedPos;

                _cameraTransform.position = camPos.WithXy(pos);
            }
        }
    }
}