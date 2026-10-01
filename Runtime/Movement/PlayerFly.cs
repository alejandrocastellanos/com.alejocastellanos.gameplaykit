using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Vuelo libre en las 4 direcciones, activable/desactivable con una acción de input. Ignora la gravedad mientras está activo.</summary>
    public class PlayerFly : AbilityBase
    {
        [SerializeField] private float flySpeed = 6f;
        [Tooltip("Acción que activa/desactiva el vuelo (su tecla se asigna en el lector de input).")]
        [SerializeField] private CharacterAction toggleAction = CharacterAction.Fly;

        public bool IsFlying { get; private set; }

        public override void ProcessAbility()
        {
            if (CharacterInput.GetActionDown(toggleAction)) SetFlying(!IsFlying);
            if (!IsFlying) return;

            Character.Controller.Move(CharacterInput.MoveInput * flySpeed);
        }

        public void SetFlying(bool flying)
        {
            IsFlying = flying;
            if (flying) Character.Controller.OverrideGravity(this, 0f); else Character.Controller.ReleaseGravity(this);
            if (flying) Character.Controller.Move(Vector2.zero);
        }

        public override void ResetAbility()
        {
            if (IsFlying) SetFlying(false);
        }
    }
}
