using System;
using System.Collections.Generic;

namespace GameplayKit.Core
{
    /// <summary>Estado de movimiento del personaje. Cada habilidad lo lee y lo cambia según corresponda.</summary>
    public enum MovementState
    {
        Idle, Walking, Running, Jumping, Falling, Dashing, WallSliding, Crouching, Swimming
    }

    /// <summary>Condición general del personaje, independiente de qué está haciendo.</summary>
    public enum ConditionState
    {
        Normal, Stunned, Dead, Paused
    }

    /// <summary>
    /// Máquina de estados genérica y liviana. CharacterCore mantiene una instancia para
    /// MovementState y otra para ConditionState; cualquier habilidad puede leerlas o cambiarlas.
    /// </summary>
    [Serializable]
    public class CharacterStateMachine<TState> where TState : Enum
    {
        public TState CurrentState { get; private set; }
        public TState PreviousState { get; private set; }

        public event Action<TState, TState> OnStateChanged;

        public CharacterStateMachine(TState initialState)
        {
            CurrentState = initialState;
            PreviousState = initialState;
        }

        public void ChangeState(TState newState)
        {
            if (EqualityComparer<TState>.Default.Equals(CurrentState, newState)) return;
            PreviousState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(PreviousState, CurrentState);
        }
    }
}
