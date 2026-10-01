using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>
    /// Volumen de agua: aplica arrastre y flotabilidad a cualquier Rigidbody2D que entre, y dispara eventos.
    /// Si el objeto tiene PlayerSwim (categoría Movimiento), esa habilidad ya reacciona por su cuenta
    /// al tag "Water"; este componente cubre además objetos sin PlayerSwim (cajas, enemigos, proyectiles).
    /// </summary>
    public class WaterZone : MonoBehaviour
    {
        [Header("Zona de agua")]
        [SerializeField] private float linearDragInWater = 3f;
        [SerializeField] private float buoyancyForce = 8f;

        public event Action<GameObject> OnEnterWater;
        public event Action<GameObject> OnExitWater;

        private readonly Dictionary<Rigidbody2D, float> _originalDrag = new Dictionary<Rigidbody2D, float>();

        private readonly TriggerOccupants<Rigidbody2D> _bodies = new TriggerOccupants<Rigidbody2D>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            _bodies.Enter(rb);
            if (rb != null && !_originalDrag.ContainsKey(rb))
            {
                _originalDrag[rb] = rb.linearDamping;
                rb.linearDamping = linearDragInWater;
            }

            OnEnterWater?.Invoke(other.gameObject);
        }

        private void FixedUpdate()
        {
            // Desde FixedUpdate y no OnTriggerStay2D: un cuerpo dormido deja de recibir Stay y se hundiría.
            foreach (var rb in _bodies.Snapshot()) rb.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            _bodies.Exit(rb);
            if (rb != null && _originalDrag.TryGetValue(rb, out float original))
            {
                rb.linearDamping = original;
                _originalDrag.Remove(rb);
            }

            OnExitWater?.Invoke(other.gameObject);
        }
    }
}
