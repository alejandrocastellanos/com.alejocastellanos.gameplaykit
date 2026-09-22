using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Movimiento automático a lo largo de una lista de waypoints, a velocidad constante.
    /// Útil también para reutilizar en MovingPlatform o secuencias cinemáticas.
    /// </summary>
    public class PlayerPathFollow : AbilityBase
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 3f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool autoStart = false;

        public bool IsFollowingPath { get; set; }
        private int _targetIndex;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            IsFollowingPath = autoStart;
        }

        public override void ProcessAbility()
        {
            if (!IsFollowingPath || waypoints == null || waypoints.Length == 0) return;

            Transform target = waypoints[_targetIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                _targetIndex++;
                if (_targetIndex >= waypoints.Length)
                {
                    if (loop) _targetIndex = 0;
                    else IsFollowingPath = false;
                }
            }
        }
    }
}
