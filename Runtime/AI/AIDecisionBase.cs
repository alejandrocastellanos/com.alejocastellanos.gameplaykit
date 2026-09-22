using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Una condición booleana evaluada cada frame para decidir si el AIBrain cambia de estado.</summary>
    public abstract class AIDecisionBase : MonoBehaviour
    {
        public abstract bool Decide(AIBrain brain);
    }
}
