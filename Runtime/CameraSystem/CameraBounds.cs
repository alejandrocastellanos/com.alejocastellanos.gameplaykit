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
            Vector2 min = minBounds, max = maxBounds;
            if (max.x <= min.x || max.y <= min.y)
            {
                // Sin configurar: usa los límites del LevelManager si existen; si no, no limita.
                var level = GameplayKit.Managers.LevelManager.Instance;
                if (level == null || !level.HasBounds) return;
                min = level.LevelMinBounds;
                max = level.LevelMaxBounds;
            }

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            Vector3 position = transform.position;
            // Si el nivel es más chico que la cámara en un eje, se centra en vez de oscilar.
            position.x = max.x - min.x < halfWidth * 2f ? (min.x + max.x) * 0.5f : Mathf.Clamp(position.x, min.x + halfWidth, max.x - halfWidth);
            position.y = max.y - min.y < halfHeight * 2f ? (min.y + max.y) * 0.5f : Mathf.Clamp(position.y, min.y + halfHeight, max.y - halfHeight);
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
