using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Plataforma cinemática que recorre una lista de waypoints (loop o ping-pong) y lleva consigo al jugador que esté encima.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum LoopMode { Loop, PingPong }

        [Header("Recorrido")]
        [Tooltip("Puntos del recorrido. Pueden ser hijos de la plataforma: sus posiciones se fijan al iniciar.")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private LoopMode loopMode = LoopMode.PingPong;
        [SerializeField] private float waypointThreshold = 0.05f;

        private Rigidbody2D _rb;
        private Vector2[] _points;
        private PlatformRiders _riders;
        private int _currentIndex;
        private int _direction = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _riders = new PlatformRiders(GetComponent<Collider2D>());

            // Se copian las posiciones: si los waypoints son hijos se moverían con la plataforma y nunca llegaría.
            var points = new System.Collections.Generic.List<Vector2>();
            if (waypoints != null) foreach (var w in waypoints) if (w != null) points.Add(w.position);
            _points = points.ToArray();
        }

        private void FixedUpdate()
        {
            if (_points == null || _points.Length < 2) return;

            Vector2 target = _points[_currentIndex];
            Vector2 newPosition = Vector2.MoveTowards(_rb.position, target, speed * Time.fixedDeltaTime);
            _riders.Carry(newPosition - _rb.position);
            _rb.MovePosition(newPosition);

            if (Vector2.Distance(newPosition, target) <= waypointThreshold)
            {
                AdvanceWaypoint();
            }
        }

        private void AdvanceWaypoint()
        {
            if (loopMode == LoopMode.Loop)
            {
                _currentIndex = (_currentIndex + 1) % _points.Length;
                return;
            }

            _currentIndex += _direction;
            if (_currentIndex >= _points.Length)
            {
                _currentIndex = _points.Length - 2;
                _direction = -1;
            }
            else if (_currentIndex < 0)
            {
                _currentIndex = 1;
                _direction = 1;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionStay2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionExit2D(Collision2D collision) => _riders.OnContactEnded(collision);
    }
}
