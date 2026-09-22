using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Objeto destructible al recibir daño de cualquier fuente (arma, explosión, etc), con efecto y drop de loot opcionales.</summary>
    public class BreakableObject : MonoBehaviour, IDamageable
    {
        [Header("Objeto rompible")]
        [SerializeField] private float health = 1f;
        [SerializeField] private GameObject breakEffectPrefab;
        [SerializeField] private GameObject[] lootDrops;

        public event Action OnBroken;

        public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
        {
            if (amount <= 0f || health <= 0f) return;

            health -= amount;
            if (health <= 0f) Break();
        }

        private void Break()
        {
            if (breakEffectPrefab != null) Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

            if (lootDrops != null)
            {
                foreach (var drop in lootDrops)
                {
                    if (drop != null) Instantiate(drop, transform.position, Quaternion.identity);
                }
            }

            OnBroken?.Invoke();
            Destroy(gameObject);
        }
    }
}
