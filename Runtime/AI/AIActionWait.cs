namespace GameplayKit.AI
{
    /// <summary>No hace nada por sí sola — se combina con una transición por tiempo (AIDecisionTimeInState) para esperar N segundos antes de continuar.</summary>
    public class AIActionWait : AIActionBase
    {
        public override void PerformAction(AIBrain brain) { }
    }
}
