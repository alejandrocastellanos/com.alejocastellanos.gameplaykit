using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Huye del objetivo mientras esté dentro de rango — útil para criaturas neutrales o cobardes.
    /// En plataformas se mueve solo en X; con Move Vertically (juegos top-down) huye en las dos direcciones.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyFlee : MonoBehaviour
    {
        [Header("Huida")]
        [Tooltip("Vacío = el objeto con tag Player (se busca solo, útil para enemigos instanciados en runtime).")]
        [SerializeField] private Transform target;
        [SerializeField] private float fleeRange = 6f;
        [SerializeField] private float speed = 3.5f;
        [Tooltip("Top-down: huye también en Y y desactiva la gravedad del Rigidbody2D.")]
        [SerializeField] private bool moveVertically = false;

        public bool IsFleeing { get; private set; }

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
                IsFleeing = false;
                Stop();
                return;
            }

            Vector2 away = (Vector2)transform.position - (Vector2)target.position;
            IsFleeing = away.magnitude <= fleeRange;

            if (!IsFleeing)
            {
                Stop();
                return;
            }

            float direction = away.x >= 0f ? 1f : -1f;
            _rb.linearVelocity = moveVertically
                ? (away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.right) * speed
                : new Vector2(direction * speed, _rb.linearVelocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; // mira hacia donde huye
            transform.localScale = scale;
        }

        private void Stop() => _rb.linearVelocity = moveVertically ? Vector2.zero : new Vector2(0f, _rb.linearVelocity.y);
    }
}
