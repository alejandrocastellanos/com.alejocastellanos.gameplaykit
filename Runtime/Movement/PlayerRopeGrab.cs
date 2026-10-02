using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Se sujeta y balancea de un ancla como un péndulo: la gravedad lo hace oscilar, el input horizontal
    /// empuja en esa dirección y al soltarse (Salto) conserva el impulso. El ancla es cualquier trigger con
    /// RopeAnchor (o con el tag configurado).
    /// </summary>
    public class PlayerRopeGrab : AbilityBase
    {
        [Tooltip("Opcional: además de cualquier RopeAnchor, cuenta como ancla todo trigger con este tag.")]
        [SerializeField] private string ropeTag = "RopeAnchor";
        [Tooltip("Aceleración (u/s²) que aplica el input horizontal a lo largo del arco.")]
        [SerializeField] private float swingForce = 6f;
        [Tooltip("Fracción de la velocidad angular que se pierde por segundo (0 = oscila para siempre).")]
        [SerializeField] private float swingDamping = 0.25f;

        public bool IsSwinging { get; private set; }
        private Transform _anchor;
        private float _ropeLength;
        private float _angle;            // radianes; 0 = colgando justo debajo del ancla, positivo hacia la derecha
        private float _angularVelocity;  // rad/s

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<RopeAnchor>() == null && !TagFilter.Matches(other, ropeTag)) return;
            _anchor = other.transform;
            _ropeLength = Vector2.Distance(transform.position, _anchor.position);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Fuera del alcance de la cuerda ya no se puede agarrar (salvo que ya se esté columpiando).
            if (!IsSwinging && _anchor != null && other.transform == _anchor) _anchor = null;
        }

        public override void ProcessAbility()
        {
            if (_anchor == null) return;

            if (!IsSwinging && CharacterInput.InteractPressedThisFrame)
            {
                IsSwinging = true;
                Character.Controller.OverrideGravity(this, 0f);
                Vector2 offset = (Vector2)transform.position - (Vector2)_anchor.position;
                _ropeLength = Mathf.Max(0.5f, offset.magnitude);
                _angle = Mathf.Atan2(offset.x, -offset.y);
                // Conserva la velocidad con la que llegó, proyectada sobre el arco.
                _angularVelocity = Vector2.Dot(Character.Controller.Velocity, Tangent(_angle)) / _ropeLength;
            }

            if (!IsSwinging) return;

            if (CharacterInput.JumpPressedThisFrame)
            {
                IsSwinging = false;
                _anchor = null;
                Character.Controller.ReleaseGravity(this);
                return;
            }

            float dt = Time.deltaTime;
            float gravity = Mathf.Abs(Physics2D.gravity.y) * Character.Controller.DefaultGravityScale;
            // Péndulo: la gravedad tira hacia abajo del arco y el input empuja en la dirección pulsada.
            float acceleration = -gravity / _ropeLength * Mathf.Sin(_angle)
                                 + CharacterInput.MoveInput.x * swingForce / _ropeLength;
            _angularVelocity += acceleration * dt;
            _angularVelocity *= Mathf.Max(0f, 1f - swingDamping * dt);
            _angle = Mathf.Clamp(_angle + _angularVelocity * dt, -Mathf.PI * 0.95f, Mathf.PI * 0.95f);

            // Cuerda rígida: la posición sale del ángulo y la velocidad queda tangente al arco,
            // así al soltarse sale disparado con el impulso del balanceo.
            Vector2 desiredPos = (Vector2)_anchor.position + new Vector2(Mathf.Sin(_angle), -Mathf.Cos(_angle)) * _ropeLength;
            Character.Controller.Rigidbody.position = desiredPos;
            transform.position = desiredPos;
            Character.Controller.Move(Tangent(_angle) * (_angularVelocity * _ropeLength));
        }

        /// <summary>Dirección de movimiento para un ángulo creciente (hacia la derecha cuando cuelga abajo).</summary>
        private static Vector2 Tangent(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
