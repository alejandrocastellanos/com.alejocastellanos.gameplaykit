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
        [SerializeField] private string ropeTag = "RopeAnchor";
        [SerializeField] private float swingForce = 6f;

        public bool IsSwinging { get; private set; }
        private Transform _anchor;
        private float _ropeLength;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(ropeTag)) return;
            _anchor = other.transform;
            _ropeLength = Vector2.Distance(transform.position, _anchor.position);
        }

        public override void ProcessAbility()
        {
            if (_anchor == null) return;

            if (!IsSwinging && CharacterInput.InteractPressedThisFrame)
            {
                IsSwinging = true;
                Character.Controller.Rigidbody.gravityScale = 0f;
            }

            if (!IsSwinging) return;

            if (CharacterInput.JumpPressedThisFrame)
            {
                IsSwinging = false;
                _anchor = null;
                Character.Controller.Rigidbody.gravityScale = 1f;
                return;
            }

            Vector2 toAnchor = (Vector2)_anchor.position - (Vector2)transform.position;
            Vector2 tangent = new Vector2(-toAnchor.y, toAnchor.x).normalized;
            Character.Controller.Move(tangent * (CharacterInput.MoveInput.x * swingForce));

            // Mantiene la distancia constante al ancla (cuerda rígida).
            Vector2 desiredPos = (Vector2)_anchor.position - toAnchor.normalized * _ropeLength;
            transform.position = desiredPos;
        }
    }
}
