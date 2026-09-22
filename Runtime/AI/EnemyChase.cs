using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Persigue a un objetivo (normalmente el jugador) mientras esté dentro de rango de detección.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyChase : MonoBehaviour
    {
        [Header("Persecución")]
        [SerializeField] private Transform target;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;

        public bool IsChasing { get; private set; }

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (target == null)
            {
                IsChasing = false;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float distance = Vector2.Distance(transform.position, target.position);
            IsChasing = distance <= detectionRange;

            if (!IsChasing || distance <= stoppingDistance)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float direction = Mathf.Sign(target.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction >= 0 ? 1f : -1f);
            transform.localScale = scale;
        }
    }
}
