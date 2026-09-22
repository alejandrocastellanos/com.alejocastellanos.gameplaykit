using System;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Vida del personaje: daño, curación, invulnerabilidad temporal y muerte.
    /// No depende de CharacterCore para poder usarse también en objetos sin abilities (barriles, enemigos simples, etc).
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterHealth : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action<float> OnDamaged;               // amount
        public event Action<float> OnHealed;                 // amount
        public event Action OnDeath;

        private float _invulnerabilityTimer;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            if (_invulnerabilityTimer <= 0f) return;

            _invulnerabilityTimer -= Time.deltaTime;
            if (_invulnerabilityTimer <= 0f)
            {
                IsInvulnerable = false;
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || IsInvulnerable || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnDamaged?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (invulnerabilityDuration > 0f)
            {
                IsInvulnerable = true;
                _invulnerabilityTimer = invulnerabilityDuration;
            }

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealed?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>Revive y restaura la vida al máximo. Útil junto a CharacterRespawn.</summary>
        public void ResetHealth()
        {
            IsDead = false;
            CurrentHealth = maxHealth;
            IsInvulnerable = false;
            _invulnerabilityTimer = 0f;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnDeath?.Invoke();
        }
    }
}
