using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Ataque a distancia instantáneo por raycast (rayo láser, francotirador).</summary>
    public class WeaponHitscan : MonoBehaviour
    {
        [Header("Disparo instantáneo (raycast)")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float range = 20f;
        [SerializeField] private float damage = 15f;
        [SerializeField] private LayerMask hittableLayers = ~0;
        [SerializeField] private float cooldown = 0.2f;

        /// <summary>(origen, punto final) — util para dibujar una linea/trail del disparo.</summary>
        public event Action<Vector2, Vector2> OnShotFired;

        public bool IsOnCooldown { get; private set; }

        private float _cooldownTimer;

        private void Update()
        {
            if (_cooldownTimer <= 0f) return;

            _cooldownTimer -= Time.deltaTime;
            IsOnCooldown = _cooldownTimer > 0f;
        }

        public bool TryFire(Vector2 direction) => TryFire(direction, damage);

        public bool TryFire(Vector2 direction, float damageAmount)
        {
            if (IsOnCooldown) return false;

            Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            var hit = PhysicsQuery2D.Raycast(origin, direction.normalized, range, hittableLayers, PhysicsQuery2D.OwnerOf(this));
            Vector2 endPoint = hit.collider != null ? hit.point : origin + direction.normalized * range;

            if (hit.collider != null)
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.ApplyDamage(damageAmount, hit.point, direction.normalized, gameObject);
            }

            OnShotFired?.Invoke(origin, endPoint);
            _cooldownTimer = cooldown;
            IsOnCooldown = true;
            return true;
        }
    }
}
