using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Trepa desde un agarre de borde (PlayerLedgeGrab) hasta quedar de pie arriba.</summary>
    [RequireComponent(typeof(PlayerLedgeGrab))]
    public class PlayerLedgeClimb : AbilityBase
    {
        [SerializeField] private Vector2 climbOffset = new Vector2(0.5f, 1f);
        [SerializeField] private float climbDuration = 0.25f;

        private PlayerLedgeGrab _ledgeGrab;
        private bool _isClimbing;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _ledgeGrab = GetComponent<PlayerLedgeGrab>();
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
            float direction = Mathf.Sign(transform.localScale.x);
            Vector3 target = start + new Vector3(climbOffset.x * direction, climbOffset.y, 0f);

            float elapsed = 0f;
            while (elapsed < climbDuration)
            {
                transform.position = Vector3.Lerp(start, target, elapsed / climbDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = target;
            Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
            _isClimbing = false;
        }
    }
}
