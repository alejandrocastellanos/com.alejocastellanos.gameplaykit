using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Algo que un enemigo HACE mientras está en un estado (patrullar, disparar, esperar...).</summary>
    public abstract class AIActionBase : MonoBehaviour
    {
        public abstract void PerformAction(AIBrain brain);

        /// <summary>Se llama una vez al entrar al estado que contiene esta acción.</summary>
        public virtual void OnEnterState(AIBrain brain) { }

        /// <summary>Se llama una vez al salir del estado que contiene esta acción.</summary>
        public virtual void OnExitState(AIBrain brain) { }
    }
}
