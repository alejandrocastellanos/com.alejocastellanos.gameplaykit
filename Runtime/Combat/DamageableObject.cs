using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>
    /// Vida simple para objetos que no son personajes (barriles, cajas rompibles, torretas, props destructibles).
    /// Recibe daño de cualquier arma a través de IDamageable y dispara eventos de feedback/loot.
    /// </summary>
    public class DamageableObject : MonoBehaviour, IDamageable, IHealthSource
    {
        [Header("Vida del objeto")]
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private bool destroyOnDeath = true;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }

        public event Action<float, GameObject> OnDamaged; // (cantidad, instigador)

        private Action<float> _damaged;
        private Action<float> _healed; // un objeto dañable no se cura, pero cumple el contrato
        event Action<float> IHealthSource.Damaged { add => _damaged += value; remove => _damaged -= value; }
        event Action<float> IHealthSource.Healed { add => _healed += value; remove => _healed -= value; }
        public event Action OnDestroyedByDamage;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
        {
            if (amount <= 0f || CurrentHealth <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnDamaged?.Invoke(amount, instigator);
            _damaged?.Invoke(amount);

            if (CurrentHealth <= 0f)
            {
                OnDestroyedByDamage?.Invoke();
                if (destroyOnDeath) Destroy(gameObject);
            }
        }
    }
}
