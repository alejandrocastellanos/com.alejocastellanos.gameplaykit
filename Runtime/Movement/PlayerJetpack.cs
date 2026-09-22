using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Propulsión vertical sostenida con consumo y regeneración de combustible.</summary>
    public class PlayerJetpack : AbilityBase
    {
        [SerializeField] private float thrust = 14f;
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float fuelDrainPerSecond = 40f;
        [SerializeField] private float fuelRegenPerSecond = 25f;

        public float CurrentFuel { get; private set; }

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            CurrentFuel = maxFuel;
        }

        public override void ProcessAbility()
        {
            bool wantsThrust = CharacterInput.JumpHeld && CurrentFuel > 0f;

            if (wantsThrust)
            {
                CurrentFuel = Mathf.Max(0f, CurrentFuel - fuelDrainPerSecond * Time.deltaTime);
                Character.Controller.SetVerticalVelocity(thrust);
                Character.Movement.ChangeState(MovementState.Jumping);
            }
            else if (Character.Controller.IsGrounded)
            {
                CurrentFuel = Mathf.Min(maxFuel, CurrentFuel + fuelRegenPerSecond * Time.deltaTime);
            }
        }
    }
}
