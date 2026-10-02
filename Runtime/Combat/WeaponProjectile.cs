using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Dispara proyectiles instanciados (bala, flecha, bola de fuego) desde un punto de disparo.</summary>
    public class WeaponProjectile : MonoBehaviour
    {
        [Header("Disparo de proyectil")]
        [SerializeField] private ProjectileBehaviour projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float damage = 8f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float cooldown = 0.3f;

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
            if (IsOnCooldown || projectilePrefab == null) return false;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            var projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            projectile.Launch(direction.normalized, projectileSpeed, damageAmount, gameObject);

            _cooldownTimer = cooldown;
            IsOnCooldown = true;
            return true;
        }
    }
}
