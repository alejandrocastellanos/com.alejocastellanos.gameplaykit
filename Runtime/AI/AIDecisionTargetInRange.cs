using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>True si el objetivo del AIBrain está dentro de un radio determinado.</summary>
    public class AIDecisionTargetInRange : AIDecisionBase
    {
        [SerializeField] private float range = 6f;

        public override bool Decide(AIBrain brain)
        {
            if (brain.Target == null) return false;
            return Vector2.Distance(transform.position, brain.Target.position) <= range;
        }
    }
}
