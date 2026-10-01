using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Trepa desde un agarre de borde (PlayerLedgeGrab) hasta quedar de pie sobre la cornisa al presionar saltar.</summary>
    [RequireComponent(typeof(PlayerLedgeGrab))]
    public class PlayerLedgeClimb : AbilityBase
    {
        [Tooltip("Cuánto se adentra sobre la cornisa más allá del borde, además del ancho del personaje.")]
        [SerializeField] private float forwardMargin = 0.1f;
        [SerializeField] private float climbDuration = 0.25f;

        public bool IsClimbing => _isClimbing;

        private PlayerLedgeGrab _ledgeGrab;
        private Collider2D _body;
        private bool _isClimbing;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _ledgeGrab = GetComponent<PlayerLedgeGrab>();
            _body = GetComponent<Collider2D>();
        }

        public override void ProcessAbility()
        {
            if (_isClimbing || !_ledgeGrab.IsGrabbingLedge) return;
            if (!CharacterInput.JumpPressedThisFrame) return;

            StartCoroutine(ClimbRoutine());
        }

        private IEnumerator ClimbRoutine()
        {
            _isClimbing = true;
            Vector3 start = transform.position;

            // Destino calculado desde la esquina real del borde: pies apoyados sobre la cornisa.
            float halfWidth = _body != null ? _body.bounds.extents.x : 0.5f;
            float pivotToFeet = _body != null ? transform.position.y - _body.bounds.min.y : 0.5f;
            Vector2 ledge = _ledgeGrab.LedgePoint;
            Vector3 target = new Vector3(
                ledge.x + _ledgeGrab.LedgeDirection * (halfWidth + forwardMargin),
                ledge.y + pivotToFeet + 0.02f,
                start.z);

            // Primero sube y luego avanza, para no atravesar la esquina en diagonal.
            Vector3 above = new Vector3(start.x, target.y, start.z);
            float elapsed = 0f;
            while (elapsed < climbDuration)
            {
                float t = elapsed / climbDuration;
                Vector3 p = t < 0.5f ? Vector3.Lerp(start, above, t * 2f) : Vector3.Lerp(above, target, (t - 0.5f) * 2f);
                Character.Controller.Rigidbody.position = p;
                transform.position = p;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Character.Controller.Rigidbody.position = target;
            transform.position = target;
            Character.Controller.Move(Vector2.zero);
            _ledgeGrab.Release();
            _isClimbing = false;
        }
    }
}
