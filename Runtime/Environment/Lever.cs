using System;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Interruptor accionado manualmente por el jugador (llamar Interact() desde tu detección de interacción); alterna su estado y dispara un evento.</summary>
    public class Lever : MonoBehaviour
    {
        public bool IsOn { get; private set; }
        public event Action<bool> OnToggled;

        public void Interact()
        {
            IsOn = !IsOn;
            OnToggled?.Invoke(IsOn);
        }
    }
}
