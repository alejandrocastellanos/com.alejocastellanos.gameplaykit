using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Se desprende y cae tras un breve retraso al ser pisada; puede desaparecer y reaparecer en su posición original.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingPlatform : MonoBehaviour
    {
        [Header("Caída")]
        [SerializeField] private float fallDelay = 0.5f;
        [SerializeField] private float destroyDelay = 3f;
        [SerializeField] private bool respawnAfterDestroy = true;
        [SerializeField] private float respawnDelay = 4f;
        [SerializeField] private string triggerTag = "Player";

        private Rigidbody2D _rb;
        private Vector3 _initialPosition;
        private bool _isFalling;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _initialPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isFalling) return;
            if (!string.IsNullOrEmpty(triggerTag) && !collision.transform.CompareTag(triggerTag)) return;

            _isFalling = true;
            Invoke(nameof(StartFalling), fallDelay);
        }

        private void StartFalling()
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;

            if (destroyDelay >= 0f) Invoke(nameof(HandleDestroy), destroyDelay);
        }

        private void HandleDestroy()
        {
            gameObject.SetActive(false);

            if (respawnAfterDestroy) Invoke(nameof(Respawn), respawnDelay);
        }

        private void Respawn()
        {
            transform.position = _initialPosition;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _isFalling = false;
            gameObject.SetActive(true);
        }
    }
}
