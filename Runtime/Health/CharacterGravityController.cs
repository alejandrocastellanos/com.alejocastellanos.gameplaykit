using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Da forma a la caída: gravedad más fuerte al caer, salto corto si se suelta el botón antes,
    /// y velocidad máxima de caída. Nota: si PlayerJump ya implementa su propio "jump cut",
    /// usa una sola de las dos fuentes de control de gravedad para evitar que se pisen.
    /// </summary>
    public class CharacterGravityController : AbilityBase
    {
        [Header("Gravedad")]
        [SerializeField] private float baseGravityScale = 1f;
        [SerializeField] private float fallGravityMultiplier = 2f;
        [SerializeField] private float lowJumpGravityMultiplier = 1.5f;
        [SerializeField] private float maxFallSpeed = 20f;

        private Rigidbody2D _rb;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _rb = Character.Controller.Rigidbody;
            _rb.gravityScale = baseGravityScale;
        }

        public override void ProcessAbility()
        {
            if (_rb == null) return;

            float verticalVelocity = _rb.linearVelocity.y;

            if (verticalVelocity < 0f)
            {
                _rb.gravityScale = baseGravityScale * fallGravityMultiplier;
            }
            else if (verticalVelocity > 0f && CharacterInput != null && !CharacterInput.JumpHeld)
            {
                _rb.gravityScale = baseGravityScale * lowJumpGravityMultiplier;
            }
            else
            {
                _rb.gravityScale = baseGravityScale;
            }

            if (_rb.linearVelocity.y < -maxFallSpeed)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -maxFallSpeed);
            }
        }
    }
}
