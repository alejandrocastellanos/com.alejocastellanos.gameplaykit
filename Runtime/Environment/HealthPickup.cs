using System;
using GameplayKit.Health;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Cura a quien lo toque (si tiene CharacterHealth) y desaparece. Por defecto no se gasta si la vida ya está llena.</summary>
    public class HealthPickup : MonoBehaviour
    {
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private bool onlyIfHurt = true;
        [SerializeField] private GameObject pickupEffectPrefab;

        public event Action<GameObject> OnPickedUp;

        // Quien está encima se recuerda: si llegó con la vida llena y luego lo hieren, lo recoge sin tener que salir y volver.
        private readonly TriggerOccupants<CharacterHealth> _inside = new TriggerOccupants<CharacterHealth>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponentInParent<CharacterHealth>();
            _inside.Enter(health);
            TryHeal(health);
        }

        private void OnTriggerExit2D(Collider2D other) => _inside.Exit(other.GetComponentInParent<CharacterHealth>());

        private void Update()
        {
            foreach (var health in _inside.Snapshot())
                if (TryHeal(health)) return;
        }

        private bool TryHeal(CharacterHealth health)
        {
            if (this == null || health == null || health.IsDead) return false;
            if (onlyIfHurt && health.CurrentHealth >= health.MaxHealth) return false;

            health.Heal(healAmount);
            if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            OnPickedUp?.Invoke(health.gameObject);
            Destroy(gameObject);
            enabled = false;
            return true;
        }
    }
}
