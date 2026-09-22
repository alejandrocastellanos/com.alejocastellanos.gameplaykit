using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Movimiento dentro de zonas de agua: gravedad reducida y control en las 4 direcciones.
    /// Se activa por trigger con tag "Water" (ver WaterZone en Environment).
    /// </summary>
    public class PlayerSwim : AbilityBase
    {
        [SerializeField] private string waterTag = "Water";
        [SerializeField] private float swimSpeed = 3.5f;
        [SerializeField] private float waterGravityScale = 0.2f;

        public bool IsInWater { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(waterTag))
            {
                IsInWater = true;
                Character.Controller.Rigidbody.gravityScale = waterGravityScale;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(waterTag))
            {
                IsInWater = false;
                Character.Controller.Rigidbody.gravityScale = 1f;
            }
        }

        public override void ProcessAbility()
        {
            if (!IsInWater) return;

            Character.Controller.Move(CharacterInput.MoveInput * swimSpeed);
            Character.Movement.ChangeState(MovementState.Swimming);
        }
    }
}
