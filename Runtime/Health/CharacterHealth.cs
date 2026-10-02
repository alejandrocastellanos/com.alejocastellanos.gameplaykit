using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Vida del personaje: daño, curación, invulnerabilidad temporal y muerte.
    /// No depende de CharacterCore para poder usarse también en objetos sin abilities (barriles, enemigos simples, etc).
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterHealth : MonoBehaviour, IDamageable, IHealthSource
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

        event Action<float> IHealthSource.Damaged { add => OnDamaged += value; remove => OnDamaged -= value; }
        event Action<float> IHealthSource.Healed { add => OnHealed += value; remove => OnHealed -= value; }

        private float _invulnerabilityTimer;

        private static readonly System.Collections.Generic.List<CharacterHealth> ActiveList = new System.Collections.Generic.List<CharacterHealth>();
        /// <summary>Personajes con vida activos en la escena (lo usa LevelManager para la zona de vacío).</summary>
        public static System.Collections.Generic.IReadOnlyList<CharacterHealth> Active => ActiveList;

        private void OnEnable() => ActiveList.Add(this);
        private void OnDisable() => ActiveList.Remove(this);

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

        /// <summary>Implementacion de IDamageable: permite que cualquier arma dañe a este personaje
        /// sin conocer CharacterHealth directamente. Si hay un CharacterKnockback, tambien lo dispara.</summary>
        public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
        {
            if (IsDead || IsInvulnerable) return;

            TakeDamage(amount);

            var knockback = GetComponent<GameplayKit.Health.CharacterKnockback>();
            if (knockback != null && hitDirection != Vector2.zero)
            {
                knockback.ApplyKnockback(hitDirection);
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

        /// <summary>Hace al personaje inmune al daño durante unos segundos (por ejemplo, durante un roll). No acorta una invulnerabilidad más larga ya activa.</summary>
        public void GrantInvulnerability(float seconds)
        {
            if (seconds <= 0f || IsDead) return;
            IsInvulnerable = true;
            _invulnerabilityTimer = Mathf.Max(_invulnerabilityTimer, seconds);
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
