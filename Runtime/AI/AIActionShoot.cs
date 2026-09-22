using GameplayKit.Combat;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Dispara el arma equipada (WeaponHitscan o WeaponProjectile) hacia el objetivo del AIBrain a intervalos regulares.</summary>
    public class AIActionShoot : AIActionBase
    {
        [SerializeField] private float fireInterval = 1f;

        private WeaponHitscan _hitscan;
        private WeaponProjectile _projectile;
        private float _fireTimer;

        private void Awake()
        {
            _hitscan = GetComponent<WeaponHitscan>();
            _projectile = GetComponent<WeaponProjectile>();
        }

        public override void OnEnterState(AIBrain brain)
        {
            _fireTimer = 0f;
        }

        public override void PerformAction(AIBrain brain)
        {
            if (brain.Target == null) return;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            Vector2 direction = ((Vector2)brain.Target.position - (Vector2)transform.position).normalized;
            bool fired = _hitscan != null && _hitscan.TryFire(direction);
            if (!fired && _projectile != null) fired = _projectile.TryFire(direction);

            if (fired) _fireTimer = fireInterval;
        }
    }
}
