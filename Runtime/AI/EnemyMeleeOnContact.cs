using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Ataca cuerpo a cuerpo al entrar en contacto físico o de trigger con el objetivo (normalmente el jugador).</summary>
    public class EnemyMeleeOnContact : MonoBehaviour
    {
        [Header("Ataque por contacto")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageCooldown = 1f;
        [SerializeField] private string targetTag = "Player";

        private float _cooldownTimer;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);
        private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D other)
        {
            if (_cooldownTimer > 0f) return;
            if (!TagFilter.PassesOptional(other, targetTag)) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null) return;

            Vector2 direction = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            damageable.ApplyDamage(damage, other.transform.position, direction, gameObject);
            _cooldownTimer = damageCooldown;
        }
    }
}
