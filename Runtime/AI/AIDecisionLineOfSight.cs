using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>True si hay línea de visión directa (sin obstáculos) hacia el objetivo del AIBrain.</summary>
    public class AIDecisionLineOfSight : AIDecisionBase
    {
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private LayerMask obstacleLayers = ~0;

        public override bool Decide(AIBrain brain)
        {
            if (brain.Target == null) return false;

            Vector2 toTarget = (Vector2)brain.Target.position - (Vector2)transform.position;
            if (toTarget.magnitude > maxDistance) return false;

            var hit = PhysicsQuery2D.Raycast(transform.position, toTarget.normalized, toTarget.magnitude, obstacleLayers,
                PhysicsQuery2D.OwnerOf(this), PhysicsQuery2D.OwnerOf(brain.Target));
            return hit.collider == null;
        }
    }
}
