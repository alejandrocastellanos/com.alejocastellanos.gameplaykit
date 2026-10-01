using System.Collections;
using System.Collections.Generic;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Ataque cuerpo a cuerpo: activa una hitbox circular temporal frente al personaje.</summary>
    public class WeaponMelee : MonoBehaviour
    {
        [Header("Ataque cuerpo a cuerpo")]
        [SerializeField] private Transform hitboxOrigin;
        [SerializeField] private float hitboxRadius = 0.75f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private float activeDuration = 0.15f;
        [SerializeField] private float cooldown = 0.4f;

        public bool IsOnCooldown { get; private set; }
        public bool IsHitboxActive { get; private set; }

        private float _cooldownTimer;
        private readonly HashSet<IDamageable> _alreadyHit = new HashSet<IDamageable>();

        private void Update()
        {
            if (_cooldownTimer <= 0f) return;

            _cooldownTimer -= Time.deltaTime;
            IsOnCooldown = _cooldownTimer > 0f;
        }

        public bool TryAttack()
        {
            if (IsOnCooldown) return false;

            StartCoroutine(ActiveWindow());
            _cooldownTimer = cooldown;
            IsOnCooldown = true;
            return true;
        }

        /// <summary>Mantiene la hitbox activa durante activeDuration, comprobando colisiones cada frame
        /// para que un ataque no dependa de un unico chequeo instantaneo.</summary>
        private IEnumerator ActiveWindow()
        {
            IsHitboxActive = true;
            _alreadyHit.Clear();

            float elapsed = 0f;
            while (elapsed < activeDuration)
            {
                DealDamage();
                elapsed += Time.deltaTime;
                yield return null;
            }

            IsHitboxActive = false;
        }

        private void DealDamage()
        {
            Vector2 origin = hitboxOrigin != null ? (Vector2)hitboxOrigin.position : (Vector2)transform.position;
            var hits = Physics2D.OverlapCircleAll(origin, hitboxRadius, targetLayers);
            Transform owner = PhysicsQuery2D.OwnerOf(this);

            foreach (var hit in hits)
            {
                if (PhysicsQuery2D.IsPartOf(hit, owner)) continue;

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || _alreadyHit.Contains(damageable)) continue;

                _alreadyHit.Add(damageable);
                damageable.ApplyDamage(damage, origin, ((Vector2)hit.transform.position - origin).normalized, gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = hitboxOrigin != null ? hitboxOrigin.position : transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, hitboxRadius);
        }
    }
}
