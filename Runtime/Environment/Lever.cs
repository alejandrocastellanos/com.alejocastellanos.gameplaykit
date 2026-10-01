using System;
using GameplayKit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayKit.Environment
{
    /// <summary>Interruptor que el jugador acciona con PlayerInteract (o llamando Interact()); alterna su estado.
    /// Los UnityEvents permiten conectar en el Inspector qué pasa al encender/apagar (ej. Elevator.Activate).</summary>
    public class Lever : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool startsOn = false;
        [SerializeField] private UnityEvent onTurnedOn = new UnityEvent();
        [SerializeField] private UnityEvent onTurnedOff = new UnityEvent();
        [Tooltip("Se dispara en cada cambio, encendiendo o apagando.")]
        [SerializeField] private UnityEvent onToggled = new UnityEvent();

        public bool IsOn { get; private set; }
        public event Action<bool> OnToggled;

        private void Awake()
        {
            IsOn = startsOn;
        }

        public void Interact()
        {
            IsOn = !IsOn;
            (IsOn ? onTurnedOn : onTurnedOff).Invoke();
            onToggled.Invoke();
            OnToggled?.Invoke(IsOn);
        }

        void IInteractable.Interact(GameObject interactor) => Interact();
    }
}
