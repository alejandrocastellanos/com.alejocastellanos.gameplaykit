using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Huye del objetivo mientras esté dentro de rango — útil para criaturas neutrales o cobardes.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyFlee : MonoBehaviour
    {
        [Header("Huida")]
        [SerializeField] private Transform target;
        [SerializeField] private float fleeRange = 6f;
        [SerializeField] private float speed = 3.5f;

        public bool IsFleeing { get; private set; }

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (target == null)
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
        }
    }
}
