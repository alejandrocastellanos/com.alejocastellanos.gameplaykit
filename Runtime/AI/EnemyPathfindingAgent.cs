using UnityEngine;
using UnityEngine.AI;

namespace GameplayKit.AI
{
    /// <summary>
    /// Wrapper simple sobre NavMeshAgent para enemigos que necesitan esquivar obstáculos complejos.
    /// Usa el componente NavMeshAgent nativo de Unity sobre un NavMesh horneado desde la ventana
    /// Navigation (o generado en runtime con el paquete AI Navigation) — no requiere el paquete
    /// para funcionar con un NavMesh horneado clásico.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyPathfindingAgent : MonoBehaviour
    {
        [Header("Pathfinding")]
        [SerializeField] private Transform target;
        [SerializeField] private float repathInterval = 0.2f;

        public bool HasTarget => target != null;

        private NavMeshAgent _agent;
        private float _repathTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Update()
        {
            if (target == null) return;

            _repathTimer -= Time.deltaTime;
            if (_repathTimer > 0f) return;

            _agent.SetDestination(target.position);
            _repathTimer = repathInterval;
        }
    }
}
