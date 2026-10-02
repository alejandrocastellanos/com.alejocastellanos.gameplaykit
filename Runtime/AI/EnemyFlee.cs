using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Huye del objetivo mientras esté dentro de rango — útil para criaturas neutrales o cobardes.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyFlee : MonoBehaviour
    {
        [Header("Huida")]
        [Tooltip("Vacío = el objeto con tag Player (se busca solo, útil para enemigos instanciados en runtime).")]
        [SerializeField] private Transform target;
        [SerializeField] private float fleeRange = 6f;
        [SerializeField] private float speed = 3.5f;

        public bool IsFleeing { get; private set; }

        private Rigidbody2D _rb;
        private float _nextTargetSearch;

        /// <summary>Objetivo actual; asígnalo por código o déjalo vacío para usar el jugador.</summary>
        public Transform Target { get => target; set => target = value; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!PlayerLocator.Resolve(ref target, ref _nextTargetSearch))
            {
                IsFleeing = false;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float distance = Vector2.Distance(transform.position, target.position);
            IsFleeing = distance <= fleeRange;

            if (!IsFleeing)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float direction = Mathf.Sign(transform.position.x - target.position.x);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; // mira hacia donde huye
            transform.localScale = scale;
        }
    }
}
