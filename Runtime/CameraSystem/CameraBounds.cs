using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Limita el recorrido de la cámara a un rectángulo (los bordes del nivel), respetando el tamaño ortográfico.</summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(50)] // después de CameraFollow, para que el límite gane
    public class CameraBounds : MonoBehaviour
    {
        [Header("Límites del nivel")]
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (!_camera.orthographic) return;
            if (maxBounds.x <= minBounds.x || maxBounds.y <= minBounds.y) return; // sin configurar: no limita

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            position.y = Mathf.Clamp(position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            transform.position = position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
