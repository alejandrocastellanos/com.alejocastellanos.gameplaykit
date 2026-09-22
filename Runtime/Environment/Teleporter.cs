using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Transporta a quien lo toque desde este punto hasta un destino, opcionalmente conservando la velocidad.</summary>
    public class Teleporter : MonoBehaviour
    {
        [Header("Teletransporte")]
        [SerializeField] private Transform destination;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private float cooldown = 0.2f;
        [SerializeField] private bool preserveVelocity = true;

        private float _cooldownTimer;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_cooldownTimer > 0f || destination == null) return;
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag)) return;

            var rb = other.attachedRigidbody;
            Vector2 previousVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

            other.transform.position = destination.position;

            if (rb != null && preserveVelocity) rb.linearVelocity = previousVelocity;

            _cooldownTimer = cooldown;
        }
    }
}
