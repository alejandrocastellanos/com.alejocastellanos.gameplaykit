using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Dash vertical hacia abajo. Dispara OnSlamLanded al tocar el suelo (para camera shake, daño en área, etc.).</summary>
    public class PlayerGroundSlam : AbilityBase
    {
        [SerializeField] private float slamSpeed = 20f;
        [SerializeField] private float minAirTimeToTrigger = 0.05f;

        public event System.Action OnSlamLanded;

        private bool _isSlamming;

        public override void ProcessAbility()
        {
            if (!_isSlamming && !Character.Controller.IsGrounded &&
                CharacterInput.MoveInput.y < -0.5f && CharacterInput.DashPressedThisFrame)
            {
                StartCoroutine(SlamRoutine());
            }
        }

        private IEnumerator SlamRoutine()
        {
            _isSlamming = true;
            yield return new WaitForSeconds(minAirTimeToTrigger);

            while (_isSlamming && !Character.Controller.IsGrounded)
            {
                Character.Controller.SetVerticalVelocity(-slamSpeed);
                yield return null;
            }

            _isSlamming = false;
            OnSlamLanded?.Invoke();
        }
    }
}
