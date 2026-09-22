using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Se mueve en línea recta hacia el objetivo del AIBrain mientras el estado esté activo.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIActionMoveToTarget : AIActionBase
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private float stoppingDistance = 0.3f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public override void PerformAction(AIBrain brain)
        {
            if (brain.Target == null || _rb == null) return;

            float distance = Vector2.Distance(transform.position, brain.Target.position);
            if (distance <= stoppingDistance)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float direction = Mathf.Sign(brain.Target.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
        }

        public override void OnExitState(AIBrain brain)
        {
            if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }
}
