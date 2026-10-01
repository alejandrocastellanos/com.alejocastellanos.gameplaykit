using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Sigue suavemente a un objetivo (normalmente el jugador) en 2D o 3D mediante SmoothDamp.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Seguimiento")]
        [Tooltip("Si se deja vacío, sigue al objeto con tag Player.")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.2f;

        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player == null) return;
                target = player.transform;
                transform.position = target.position + offset; // primer encuadre sin barrido desde el origen
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
        }
    }
}
