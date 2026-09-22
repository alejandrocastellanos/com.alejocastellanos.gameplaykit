using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>True si hay línea de visión directa (sin obstáculos) hacia el objetivo del AIBrain.</summary>
    public class AIDecisionLineOfSight : AIDecisionBase
    {
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private LayerMask obstacleLayers;

        public override bool Decide(AIBrain brain)
        {
            if (brain.Target == null) return false;

            Vector2 toTarget = (Vector2)brain.Target.position - (Vector2)transform.position;
            if (toTarget.magnitude > maxDistance) return false;

            var hit = Physics2D.Raycast(transform.position, toTarget.normalized, toTarget.magnitude, obstacleLayers);
            return hit.collider == null;
        }
    }
}
