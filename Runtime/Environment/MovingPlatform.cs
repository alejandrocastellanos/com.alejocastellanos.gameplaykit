using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Plataforma cinemática que recorre una lista de waypoints (loop o ping-pong) y lleva consigo al jugador que esté encima.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum LoopMode { Loop, PingPong }

        [Header("Recorrido")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private LoopMode loopMode = LoopMode.PingPong;
        [SerializeField] private float waypointThreshold = 0.05f;

        private Rigidbody2D _rb;
        private int _currentIndex;
        private int _direction = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            Transform target = waypoints[_currentIndex];
            if (target == null) return;

            Vector2 newPosition = Vector2.MoveTowards(_rb.position, target.position, speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPosition);

            if (Vector2.Distance(newPosition, target.position) <= waypointThreshold)
            {
                AdvanceWaypoint();
            }
        }

        private void AdvanceWaypoint()
        {
            if (loopMode == LoopMode.Loop)
            {
                _currentIndex = (_currentIndex + 1) % waypoints.Length;
                return;
            }

            _currentIndex += _direction;
            if (_currentIndex >= waypoints.Length)
            {
                _currentIndex = waypoints.Length - 2;
                _direction = -1;
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = 1;
                _direction = 1;
            }
        }

        // Lleva al jugador consigo mientras esté parado encima (parenting temporal).
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player")) collision.transform.SetParent(transform);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player")) collision.transform.SetParent(null);
        }
    }
}
