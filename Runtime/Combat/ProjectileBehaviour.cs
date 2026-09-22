using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Vuelo, rebote opcional y aplicación de daño al impactar. Instanciado por WeaponProjectile.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Proyectil")]
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int maxBounces = 0;
        [SerializeField] private LayerMask collidableLayers = ~0;

        private Rigidbody2D _rb;
        private float _damage;
        private GameObject _instigator;
        private int _bounceCount;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>Configura y lanza el proyectil. Llamado por el arma justo después de Instantiate.</summary>
        public void Launch(Vector2 direction, float speed, float damage, GameObject instigator)
        {
            _damage = damage;
            _instigator = instigator;
            _rb.linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_instigator != null && other.gameObject == _instigator) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(_damage, transform.position, _rb.linearVelocity.normalized, _instigator);
                Destroy(gameObject);
                return;
            }

            if (((1 << other.gameObject.layer) & collidableLayers) == 0) return;

            if (_bounceCount < maxBounces)
            {
                _bounceCount++;
                Vector2 normal = ((Vector2)transform.position - other.ClosestPoint(transform.position)).normalized;
                _rb.linearVelocity = Vector2.Reflect(_rb.linearVelocity, normal);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
