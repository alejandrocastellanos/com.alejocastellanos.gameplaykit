using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Movimiento automático a lo largo de una lista de waypoints, a velocidad constante (sin gravedad mientras dura).
    /// Útil para secuencias cinemáticas o tramos guiados. Activa con IsFollowingPath o autoStart.
    /// </summary>
    public class PlayerPathFollow : AbilityBase
    {
        [Tooltip("Puntos del recorrido. Pueden ser hijos del personaje: sus posiciones se fijan al iniciar.")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 3f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool autoStart = false;

        private bool _isFollowing;
        public bool IsFollowingPath
        {
            get => _isFollowing;
            set
            {
                if (_isFollowing == value) return;
                _isFollowing = value;
                if (Character != null)
                {
                    if (value) Character.Controller.OverrideGravity(this, 0f); else Character.Controller.ReleaseGravity(this);
                    if (!value) Character.Controller.Move(Vector2.zero);
                }
            }
        }

        private Vector2[] _points;
        private int _targetIndex;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            var points = new System.Collections.Generic.List<Vector2>();
            if (waypoints != null) foreach (var w in waypoints) if (w != null) points.Add(w.position);
            _points = points.ToArray();
            IsFollowingPath = autoStart;
        }

        public override void ProcessAbility()
        {
            if (!IsFollowingPath || _points == null || _points.Length == 0) return;

            Vector2 position = Character.Controller.Rigidbody.position;
            Vector2 target = _points[_targetIndex];
            Vector2 toTarget = target - position;
            float step = speed * Time.deltaTime;

            if (toTarget.magnitude <= step)
            {
                Character.Controller.Rigidbody.position = target;
                _targetIndex++;
                if (_targetIndex >= _points.Length)
                {
                    _targetIndex = 0;
                    if (!loop) { IsFollowingPath = false; return; }
                }
            }
            Character.Controller.Move(toTarget.normalized * speed);
        }
    }
}
