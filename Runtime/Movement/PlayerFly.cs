using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Vuelo libre en las 4 direcciones, activable/desactivable. Ignora la gravedad mientras está activo.</summary>
    public class PlayerFly : AbilityBase
    {
        [SerializeField] private float flySpeed = 6f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;

        public bool IsFlying { get; private set; }

        public override void ProcessAbility()
        {
            if (InputCompat.GetKeyDown(toggleKey))
            {
                IsFlying = !IsFlying;
                Character.Controller.Rigidbody.gravityScale = IsFlying ? 0f : Character.Controller.DefaultGravityScale;
            }

            if (!IsFlying) return;

            Character.Controller.Move(CharacterInput.MoveInput * flySpeed);
        }
    }
}
