using System.Collections.Generic;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Zona de daño por contacto (picos, lava, ácido) — aplica daño repetido a cualquier IDamageable dentro, a un intervalo fijo.
    /// El primer golpe llega al entrar; los siguientes cada damageInterval segundos mientras siga dentro, aunque esté quieto.</summary>
    public class HazardZone : MonoBehaviour
    {
        [Header("Zona de peligro")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageInterval = 0.5f;

        private readonly TriggerOccupants<IDamageable> _occupants = new TriggerOccupants<IDamageable>();
        private readonly Dictionary<IDamageable, float> _timers = new Dictionary<IDamageable, float>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null) return;
            _occupants.Enter(damageable);
            if (!_timers.ContainsKey(damageable)) _timers[damageable] = 0f;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && _occupants.Exit(damageable)) _timers.Remove(damageable);
        }

        private void Update()
        {
            foreach (var damageable in _occupants.Snapshot())
            {
                _timers.TryGetValue(damageable, out float timer);
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    Vector2 position = damageable is Component c ? (Vector2)c.transform.position : (Vector2)transform.position;
                    Vector2 direction = (position - (Vector2)transform.position).normalized;
                    damageable.ApplyDamage(damage, position, direction, gameObject);
                    timer = damageInterval;
                }
                _timers[damageable] = timer;
            }
        }
    }
}
