using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Recorre una lista ordenada de waypoints mientras el estado esté activo (con loop opcional).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIActionPatrolPoints : AIActionBase
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float waypointThreshold = 0.2f;
        [SerializeField] private bool loop = true;

        private Rigidbody2D _rb;
        private int _currentIndex;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public override void PerformAction(AIBrain brain)
        {
            if (waypoints == null || waypoints.Length == 0 || _rb == null) return;

            Transform target = waypoints[_currentIndex];
            if (target == null) return;

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= waypointThreshold)
            {
                AdvanceWaypoint();
                return;
            }

            float direction = Mathf.Sign(target.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
        }

        private void AdvanceWaypoint()
        {
            _currentIndex++;
            if (_currentIndex >= waypoints.Length)
            {
                _currentIndex = loop ? 0 : waypoints.Length - 1;
            }
        }

        public override void OnExitState(AIBrain brain)
        {
            if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }
}
