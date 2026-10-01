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
    [RequireComponent(typeof(CharacterController2D))]
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

            // Sin una fuente de input las habilidades fallarían al leer CharacterInput; si nadie
            // aportó una (PlayerInput propio, IA, etc.), se usa el lector de teclado/gamepad por defecto.
            if (GetComponent<ICharacterInput>() == null) gameObject.AddComponent<KeyboardInputReader>();

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
                try
                {
                    ability.HandleInput();
                    ability.EarlyProcessAbility();
                    ability.ProcessAbility();
                }
                catch (System.Exception e) { DisableFaultyAbility(ability, e); }
            }

            foreach (AbilityBase ability in _abilities)
            {
                if (!ability.AbilityEnabled) continue;
                try { ability.LateProcessAbility(); }
                catch (System.Exception e) { DisableFaultyAbility(ability, e); }
            }
        }

        /// <summary>
        /// Una habilidad mal configurada no debe tumbar a las demás: sin esto, una excepción corta el
        /// foreach y todas las habilidades siguientes dejan de ejecutarse, además de repetir el error
        /// cada frame. Se reporta una sola vez, con contexto, y se desactiva solo esa habilidad.
        /// </summary>
        private void DisableFaultyAbility(AbilityBase ability, System.Exception e)
        {
            ability.AbilityEnabled = false;
            Debug.LogError($"[GameplayKit] {ability.GetType().Name} en '{name}' lanzó una excepción y se desactivó. Revisa su configuración en el Inspector.", ability);
            Debug.LogException(e, ability);
        }

        public void ResetCharacter()
        {
            Movement.ChangeState(MovementState.Idle);
            Condition.ChangeState(ConditionState.Normal);
            foreach (AbilityBase ability in _abilities) ability.ResetAbility();
        }
    }
}
