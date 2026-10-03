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
        [Tooltip("Desactivado: vuela en línea recta (bala, flecha). Activado: cae con la gravedad (granada).")]
        [SerializeField] private bool affectedByGravity = false;

        private Rigidbody2D _rb;
        private float _damage;
        private GameObject _instigator;
        private int _bounceCount;
        private Transform _owner;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>Configura y lanza el proyectil. Llamado por el arma justo después de Instantiate.</summary>
        public void Launch(Vector2 direction, float speed, float damage, GameObject instigator)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            _damage = damage;
            _instigator = instigator;
            _owner = instigator != null ? PhysicsQuery2D.OwnerOf(instigator.transform) : null;
            if (!affectedByGravity) _rb.gravityScale = 0f;
            _rb.linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Nace dentro de quien dispara: sin esto se dañaría a sí mismo al salir.
            if (PhysicsQuery2D.IsPartOf(other, _owner)) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(_damage, transform.position, _rb.linearVelocity.normalized, _instigator);
                Destroy(gameObject);
                return;
            }

            // Zonas trigger (agua, viento, checkpoints...) no detienen el proyectil.
            if (other.isTrigger) return;
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
