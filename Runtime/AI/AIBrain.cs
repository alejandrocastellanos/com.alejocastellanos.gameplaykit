using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>
    /// Orquestador de IA: mantiene el estado activo de un enemigo y lo cambia evaluando las
    /// decisiones del estado actual. Los estados se arman en el Inspector combinando
    /// AIActionBase (qué hace) y AIDecisionBase (cuándo cambiar) — sin escribir código nuevo
    /// para cada enemigo, solo componiendo piezas ya escritas.
    /// </summary>
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] private List<AIState> states = new List<AIState>();
        [SerializeField] private string initialState;
        [Tooltip("Objetivo compartido (normalmente el jugador) que leen las AIAction/AIDecision de este enemigo.")]
        [SerializeField] private Transform target;

        private AIState _currentState;
        private readonly Dictionary<string, AIState> _statesByName = new Dictionary<string, AIState>();
        private float _stateEnteredTime;

        public AIState CurrentState => _currentState;
        public Transform Target { get => target; set => target = value; }
        public float TimeInCurrentState => Time.time - _stateEnteredTime;

        private void Awake()
        {
            foreach (AIState state in states)
            {
                _statesByName[state.name] = state;
            }
        }

        private void Start()
        {
            if (states.Count == 0) return; // sin estados configurados todavía: no hace nada (ni avisa)
            string first = string.IsNullOrEmpty(initialState) ? states[0].name : initialState;
            TransitionTo(first);
        }

        private void Update()
        {
            if (_currentState == null) return;

            foreach (AIActionBase action in _currentState.actions)
            {
                if (action != null && action.enabled) action.PerformAction(this);
            }

            foreach (AITransition transition in _currentState.transitions)
            {
                if (transition.decision == null || !transition.decision.enabled) continue;

                bool result = transition.decision.Decide(this);
                string target = result ? transition.trueTargetState : transition.falseTargetState;

                if (!string.IsNullOrEmpty(target) && target != _currentState.name)
                {
                    TransitionTo(target);
                    break;
                }
            }
        }

        public void TransitionTo(string stateName)
        {
            if (string.IsNullOrEmpty(stateName) || !_statesByName.TryGetValue(stateName, out AIState next))
            {
                Debug.LogWarning($"AIBrain: no existe el estado '{stateName}' en {name}.");
                return;
            }

            if (_currentState != null)
            {
                foreach (AIActionBase action in _currentState.actions) action?.OnExitState(this);
            }

            _currentState = next;
            _stateEnteredTime = Time.time;

            foreach (AIActionBase action in _currentState.actions) action?.OnEnterState(this);
        }
    }
}
