using System.Collections.Generic;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Zona de daño por contacto (picos, lava, ácido) — aplica daño repetido a cualquier IDamageable dentro, a un intervalo fijo.</summary>
    public class HazardZone : MonoBehaviour
    {
        [Header("Zona de peligro")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageInterval = 0.5f;

        private readonly Dictionary<IDamageable, float> _timers = new Dictionary<IDamageable, float>();

        private void OnTriggerStay2D(Collider2D other)
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null) return;

            _timers.TryGetValue(damageable, out float timer);

            if (timer <= 0f)
            {
                Vector2 direction = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
                damageable.ApplyDamage(damage, other.transform.position, direction, gameObject);
                timer = damageInterval;
            }
            else
            {
                timer -= Time.deltaTime;
            }

            _timers[damageable] = timer;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null) _timers.Remove(damageable);
        }
    }
}
