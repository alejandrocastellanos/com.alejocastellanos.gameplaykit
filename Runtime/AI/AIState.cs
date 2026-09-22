using System;
using System.Collections.Generic;

namespace GameplayKit.AI
{
    /// <summary>Una transición evaluada cada frame mientras el estado que la contiene está activo.</summary>
    [Serializable]
    public class AITransition
    {
        public AIDecisionBase decision;
        public string trueTargetState;
        public string falseTargetState; // vacío = se queda en el estado actual
    }

    /// <summary>
    /// Un estado con nombre (ej. "Patrullando") hecho de acciones (qué hace) y transiciones
    /// (cuándo pasa a otro estado). Se arma en el Inspector arrastrando componentes.
    /// </summary>
    [Serializable]
    public class AIState
    {
        public string name;
        public List<AIActionBase> actions = new List<AIActionBase>();
        public List<AITransition> transitions = new List<AITransition>();
    }
}
