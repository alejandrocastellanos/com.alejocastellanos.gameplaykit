using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>
    /// Lleva consigo a los cuerpos dinámicos parados encima de una plataforma cinemática. Emparentar
    /// (SetParent) no funciona con Rigidbody2D dinámicos: la física sobrescribe la posición heredada.
    /// En su lugar se desplaza a cada pasajero lo mismo que se movió la plataforma en ese paso físico.
    /// </summary>
    internal class PlatformRiders
    {
        private readonly Collider2D _platformCollider;
        private readonly HashSet<Rigidbody2D> _riders = new HashSet<Rigidbody2D>();
        private readonly List<Rigidbody2D> _stale = new List<Rigidbody2D>();

        public PlatformRiders(Collider2D platformCollider)
        {
            _platformCollider = platformCollider;
        }

        public void OnContact(Collision2D collision)
        {
            Rigidbody2D rb = collision.rigidbody;
            if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) return;
            if (IsOnTop(collision.collider)) _riders.Add(rb);
            else _riders.Remove(rb);
        }

        public void OnContactEnded(Collision2D collision)
        {
            if (collision.rigidbody != null) _riders.Remove(collision.rigidbody);
        }

        public void Carry(Vector2 delta)
        {
            if (delta == Vector2.zero) return;
            foreach (var rider in _riders)
            {
                if (rider == null) { _stale.Add(rider); continue; }
                rider.position += delta;
                rider.WakeUp(); // un pasajero quieto se duerme y dejaría de recibir contactos
            }
            foreach (var rider in _stale) _riders.Remove(rider);
            _stale.Clear();
        }

        // Cuenta como pasajero quien apoya los pies sobre la cara superior (no quien choca de lado o desde abajo).
        private bool IsOnTop(Collider2D other)
        {
            if (_platformCollider == null || other == null) return false;
            return other.bounds.min.y >= _platformCollider.bounds.max.y - 0.1f;
        }
    }
}
