using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Sigue suavemente a un objetivo (normalmente el jugador) en 2D o 3D mediante SmoothDamp.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Seguimiento")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.2f;

        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
        }
    }
}
