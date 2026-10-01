using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Se sujeta y balancea de un ancla (péndulo simple). El ancla es cualquier Transform con
    /// tag "RopeAnchor" en un collider trigger.
    /// </summary>
    public class PlayerRopeGrab : AbilityBase
    {
        [Tooltip("Opcional: además de cualquier RopeAnchor, cuenta como ancla todo trigger con este tag.")]
        [SerializeField] private string ropeTag = "RopeAnchor";
        [SerializeField] private float swingForce = 6f;

        public bool IsSwinging { get; private set; }
        private Transform _anchor;
        private float _ropeLength;

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
            }

            if (!IsSwinging) return;

            if (CharacterInput.JumpPressedThisFrame)
            {
                IsSwinging = false;
                _anchor = null;
                Character.Controller.ReleaseGravity(this);
                return;
            }

            Vector2 toAnchor = (Vector2)_anchor.position - (Vector2)transform.position;
            Vector2 tangent = new Vector2(-toAnchor.y, toAnchor.x).normalized;
            Character.Controller.Move(tangent * (CharacterInput.MoveInput.x * swingForce));

            // Mantiene la distancia constante al ancla (cuerda rígida).
            Vector2 desiredPos = (Vector2)_anchor.position - toAnchor.normalized * _ropeLength;
            Character.Controller.Rigidbody.position = desiredPos;
            transform.position = desiredPos;
        }
    }
}
