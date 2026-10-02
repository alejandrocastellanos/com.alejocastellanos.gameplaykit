using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Recorre una lista de puntos moviéndose en horizontal (enemigos de plataformas). Los puntos pueden ser
    /// hijos del enemigo: sus posiciones se fijan al iniciar. Solo se compara la distancia en X, así un punto un poco
    /// más alto o más bajo que el suelo también se alcanza.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIActionPatrolPoints : AIActionBase
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float waypointThreshold = 0.2f;
        [SerializeField] private bool loop = true;

        public int CurrentIndex => _currentIndex;
        public bool Finished { get; private set; }

        private Rigidbody2D _rb;
        private float[] _pointsX;
        private int _currentIndex;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            var xs = new System.Collections.Generic.List<float>();
            if (waypoints != null) foreach (var w in waypoints) if (w != null) xs.Add(w.position.x);
            _pointsX = xs.ToArray();
        }

        public override void PerformAction(AIBrain brain)
        {
            if (_pointsX == null || _pointsX.Length == 0 || _rb == null || Finished)
            {
                if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float dx = _pointsX[_currentIndex] - transform.position.x;
            if (Mathf.Abs(dx) <= waypointThreshold)
            {
                AdvanceWaypoint();
                return;
            }

            float direction = Mathf.Sign(dx);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }

        private void AdvanceWaypoint()
        {
            _currentIndex++;
            if (_currentIndex < _pointsX.Length) return;
            if (loop) _currentIndex = 0;
            else { _currentIndex = _pointsX.Length - 1; Finished = true; }
        }

        public override void OnExitState(AIBrain brain)
        {
            if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }
}
