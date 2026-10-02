using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Persigue a un objetivo (normalmente el jugador) mientras esté dentro de rango de detección.
    /// En plataformas se mueve solo en X; con Move Vertically (juegos top-down) persigue en las dos direcciones.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyChase : MonoBehaviour
    {
        [Header("Persecución")]
        [Tooltip("Vacío = el objeto con tag Player (se busca solo, útil para enemigos instanciados en runtime).")]
        [SerializeField] private Transform target;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [Tooltip("Top-down: persigue también en Y y desactiva la gravedad del Rigidbody2D.")]
        [SerializeField] private bool moveVertically = false;

        public bool IsChasing { get; private set; }

        private Rigidbody2D _rb;
        private float _nextTargetSearch;

        /// <summary>Objetivo actual; asígnalo por código o déjalo vacío para usar el jugador.</summary>
        public Transform Target { get => target; set => target = value; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (moveVertically) _rb.gravityScale = 0f;
        }

        private void Update()
        {
            if (!PlayerLocator.Resolve(ref target, ref _nextTargetSearch))
            {
                IsChasing = false;
                Stop();
                return;
            }

            Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
            float distance = toTarget.magnitude;
            IsChasing = distance <= detectionRange;

            if (!IsChasing || distance <= stoppingDistance)
            {
                Stop();
                return;
            }

            float direction = Mathf.Sign(toTarget.x);
            _rb.linearVelocity = moveVertically
                ? toTarget.normalized * speed
                : new Vector2(direction * speed, _rb.linearVelocity.y);

            if (Mathf.Abs(toTarget.x) > 0.05f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (direction >= 0 ? 1f : -1f);
                transform.localScale = scale;
            }
        }

        private void Stop() => _rb.linearVelocity = moveVertically ? Vector2.zero : new Vector2(0f, _rb.linearVelocity.y);
    }
}
