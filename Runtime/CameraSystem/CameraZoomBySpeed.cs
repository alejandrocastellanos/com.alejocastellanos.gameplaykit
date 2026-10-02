using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Ajusta el zoom (tamaño ortográfico) según la velocidad de un Rigidbody2D objetivo — más rápido, más zoom out.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraZoomBySpeed : MonoBehaviour
    {
        [Header("Zoom por velocidad")]
        [SerializeField] private Rigidbody2D target;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 8f;
        [SerializeField] private float speedForMaxZoom = 10f;
        [SerializeField] private float zoomSmoothTime = 0.3f;

        private Camera _camera;
        private float _zoomVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) target = player.GetComponentInParent<Rigidbody2D>();
            }
            if (target == null || !_camera.orthographic) return;

            float speedRatio = Mathf.Clamp01(target.linearVelocity.magnitude / speedForMaxZoom);
            float desiredZoom = Mathf.Lerp(minZoom, maxZoom, speedRatio);

            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, desiredZoom, ref _zoomVelocity, zoomSmoothTime);
        }
    }
}
