using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Arrastrarse por espacios bajos: se mueve solo mientras se mantiene agachado, a velocidad fija.</summary>
    [RequireComponent(typeof(PlayerCrouch))]
    public class PlayerCrawl : AbilityBase
    {
        [SerializeField] private float crawlSpeed = 1.5f;

        private PlayerCrouch _crouch;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _crouch = GetComponent<PlayerCrouch>();
        }

        public override void ProcessAbility()
        {
            if (!_crouch.IsCrouching) return;

            float horizontal = CharacterInput.MoveInput.x;
            Vector2 v = Character.Controller.Velocity;
            v.x = horizontal * crawlSpeed;
            Character.Controller.Move(v);
        }
    }
}
