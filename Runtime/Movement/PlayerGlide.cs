using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Cae lento mientras se mantiene el input de salto en el aire (planeador/vela).</summary>
    public class PlayerGlide : AbilityBase
    {
        [SerializeField] private float glideFallSpeed = 2f;

        public bool IsGliding { get; private set; }

        public override void ProcessAbility()
        {
            IsGliding = !Character.Controller.IsGrounded &&
                        CharacterInput.JumpHeld &&
                        Character.Controller.Velocity.y < 0f;

            if (!IsGliding) return;

            Character.Controller.SetVerticalVelocity(Mathf.Max(Character.Controller.Velocity.y, -glideFallSpeed));
        }
    }
}
