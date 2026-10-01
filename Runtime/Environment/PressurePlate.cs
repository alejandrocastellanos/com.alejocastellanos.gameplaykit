using System;
using GameplayKit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayKit.Environment
{
    /// <summary>Se activa por peso (cualquier Rigidbody2D encima, o con un tag específico) y dispara eventos — para abrir puertas o activar mecanismos.</summary>
    public class PressurePlate : MonoBehaviour
    {
        [Header("Placa de presión")]
        [SerializeField] private string requiredTag = "";
        [SerializeField] private UnityEvent onPressed = new UnityEvent();
        [SerializeField] private UnityEvent onReleased = new UnityEvent();

        public bool IsPressed { get; private set; }
        public event Action OnPressed;
        public event Action OnReleased;

        private int _contactCount;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValid(other)) return;

            _contactCount++;
            if (_contactCount == 1)
            {
                IsPressed = true;
                onPressed.Invoke();
                OnPressed?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsValid(other)) return;

            _contactCount = Mathf.Max(0, _contactCount - 1);
            if (_contactCount == 0)
            {
                IsPressed = false;
                onReleased.Invoke();
                OnReleased?.Invoke();
            }
        }

        private bool IsValid(Collider2D other)
        {
            return TagFilter.PassesOptional(other, requiredTag);
        }
    }
}
