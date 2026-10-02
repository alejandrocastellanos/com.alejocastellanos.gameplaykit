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
    /// <remarks>EXPERIMENTAL: usa el NavMesh de Unity, que se hornea en 3D. En un juego 2D necesita un NavMesh
    /// sobre el plano XY (por ejemplo con el paquete comunitario NavMeshPlus). Sin NavMesh no hace nada.</remarks>
    [AddComponentMenu("GameplayKit/Experimental/Enemy Pathfinding Agent (NavMesh)")]
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
            // Configuración para 2D (plano XY): sin esto el agente rota el sprite y lo saca del plano.
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Update()
        {
            if (target == null || !_agent.isOnNavMesh) return;

            _repathTimer -= Time.deltaTime;
            if (_repathTimer > 0f) return;

            _agent.SetDestination(target.position);
            _repathTimer = repathInterval;
        }
    }
}
