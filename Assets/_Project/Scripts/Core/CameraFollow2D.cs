using UnityEngine;

namespace Project.Core
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(0.01f, 0.5f)] private float smoothTime = 0.12f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
