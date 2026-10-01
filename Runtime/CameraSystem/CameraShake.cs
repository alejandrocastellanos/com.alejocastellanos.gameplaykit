using System.Collections;
using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Sacude la cámara al recibir un evento externo (daño, explosión, aterrizaje fuerte).
    /// El temblor se suma encima de lo que haga CameraFollow/CameraBounds y se retira al frame siguiente,
    /// así nunca devuelve la cámara a una posición vieja.</summary>
    [DefaultExecutionOrder(100)]
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake")]
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultMagnitude = 0.15f;

        public bool IsShaking => _remaining > 0f;

        private float _remaining;
        private float _magnitude;
        private Vector3 _appliedOffset;

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            _remaining = Mathf.Max(_remaining, duration);
            _magnitude = Mathf.Max(IsShaking ? _magnitude : 0f, magnitude);
        }

        private void Update()
        {
            // Se quita el temblor del frame anterior antes de que el seguimiento calcule la nueva posición.
            transform.position -= _appliedOffset;
            _appliedOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_remaining <= 0f) return;
            _remaining -= Time.unscaledDeltaTime;
            _appliedOffset = (Vector3)(Random.insideUnitCircle * _magnitude);
            transform.position += _appliedOffset;
        }
    }
}
