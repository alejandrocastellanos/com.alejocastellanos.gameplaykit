using GameplayKit.Combat;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>
    /// Dispara su arma equipada (WeaponHitscan o WeaponProjectile) al detectar al objetivo dentro
    /// de un cono de visión, siempre que no haya obstáculos entre medio.
    /// </summary>
    public class EnemyShootOnSight : MonoBehaviour
    {
        [Header("Detección")]
        [SerializeField] private Transform target;
        [SerializeField] private float sightRange = 10f;
        [SerializeField] private float sightAngle = 60f;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private float fireInterval = 1f;

        private WeaponHitscan _hitscan;
        private WeaponProjectile _projectile;
        private float _fireTimer;

        private void Awake()
        {
            _hitscan = GetComponent<WeaponHitscan>();
            _projectile = GetComponent<WeaponProjectile>();
        }

        private void Update()
        {
            if (target == null || !CanSeeTarget()) return;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            bool fired = _hitscan != null && _hitscan.TryFire(direction);
            if (!fired && _projectile != null) fired = _projectile.TryFire(direction);

            if (fired) _fireTimer = fireInterval;
        }

        private bool CanSeeTarget()
        {
            Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
            float distance = toTarget.magnitude;
            if (distance > sightRange) return false;

            Vector2 facing = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
            float angle = Vector2.Angle(facing, toTarget);
            if (angle > sightAngle * 0.5f) return false;

            var hit = Physics2D.Raycast(transform.position, toTarget.normalized, distance, obstacleLayers);
            return hit.collider == null;
        }
    }
}
