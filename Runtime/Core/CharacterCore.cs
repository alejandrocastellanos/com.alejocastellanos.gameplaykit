using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Orquestador central del personaje. Se agrega una sola vez por personaje; descubre
    /// automáticamente todas las AbilityBase del GameObject (y sus hijos) y corre su ciclo
    /// de vida cada frame. Agregar o quitar una habilidad es solo Add/Remove Component —
    /// CharacterCore no necesita saber de antemano qué habilidades existen.
    /// </summary>
    public class CharacterCore : MonoBehaviour
    {
        public CharacterStateMachine<MovementState> Movement { get; private set; }
        public CharacterStateMachine<ConditionState> Condition { get; private set; }
        public CharacterController2D Controller { get; private set; }

        private readonly List<AbilityBase> _abilities = new List<AbilityBase>();

        private void Awake()
        {
            Movement = new CharacterStateMachine<MovementState>(MovementState.Idle);
            Condition = new CharacterStateMachine<ConditionState>(ConditionState.Normal);
            Controller = GetComponent<CharacterController2D>();

            GetComponentsInChildren(true, _abilities);
            foreach (AbilityBase ability in _abilities)
            {
                ability.Initialize(this);
            }
        }

        private void Update()
        {
            if (Condition.CurrentState == ConditionState.Paused) return;

            foreach (AbilityBase ability in _abilities)
            {
                if (!ability.AbilityEnabled) continue;
                ability.HandleInput();
                ability.EarlyProcessAbility();
                ability.ProcessAbility();
            }

            foreach (AbilityBase ability in _abilities)
            {
                if (ability.AbilityEnabled) ability.LateProcessAbility();
            }
        }

        public void ResetCharacter()
        {
            Movement.ChangeState(MovementState.Idle);
            Condition.ChangeState(ConditionState.Normal);
            foreach (AbilityBase ability in _abilities) ability.ResetAbility();
        }
    }
}
