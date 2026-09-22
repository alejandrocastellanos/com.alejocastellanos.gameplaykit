namespace GameplayKit.AI
{
    /// <summary>True si el AIBrain lleva al menos X segundos en el estado actual — útil para temporizar patrullas o esperas.</summary>
    public class AIDecisionTimeInState : AIDecisionBase
    {
        [UnityEngine.SerializeField] private float seconds = 2f;

        public override bool Decide(AIBrain brain)
        {
            return brain.TimeInCurrentState >= seconds;
        }
    }
}
