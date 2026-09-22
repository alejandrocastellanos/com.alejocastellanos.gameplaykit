using System.Collections;
using UnityEngine;

namespace GameplayKit.CameraSystem
{
    /// <summary>Sacude la cámara al recibir un evento externo (daño, explosión, aterrizaje fuerte).</summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake")]
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultMagnitude = 0.15f;

        private Vector3 _originalLocalPosition;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            _originalLocalPosition = transform.localPosition;
        }

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Vector2 offset = Random.insideUnitCircle * magnitude;
                transform.localPosition = _originalLocalPosition + (Vector3)offset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalLocalPosition;
            _shakeRoutine = null;
        }
    }
}
