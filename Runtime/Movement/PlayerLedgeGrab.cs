using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Detecta un borde de plataforma (pared bloqueada a la altura del pecho, libre a la altura
    /// de la cabeza) y se cuelga: congela al personaje en el sitio hasta que se suelte o trepe.
    /// Combínalo con PlayerLedgeClimb para trepar desde el agarre.
    /// </summary>
    public class PlayerLedgeGrab : AbilityBase
    {
        [SerializeField] private Transform chestCheck;
        [SerializeField] private Transform headCheck;
        [SerializeField] private float checkDistance = 0.3f;
        [SerializeField] private LayerMask ledgeLayers;

        public bool IsGrabbingLedge { get; private set; }

        public override void ProcessAbility()
        {
            if (Character.Controller.IsGrounded) { IsGrabbingLedge = false; return; }

            if (!IsGrabbingLedge)
            {
                bool chestBlocked = Physics2D.Raycast(chestCheck.position, transform.right, checkDistance, ledgeLayers);
                bool headFree = !Physics2D.Raycast(headCheck.position, transform.right, checkDistance, ledgeLayers);

                if (chestBlocked && headFree && Character.Controller.Velocity.y < 0f)
                {
                    IsGrabbingLedge = true;
                }
            }

            if (IsGrabbingLedge)
            {
                Character.Controller.Move(Vector2.zero);
                Character.Controller.Rigidbody.gravityScale = 0f;

                if (CharacterInput.MoveInput.y < -0.5f)
                {
                    IsGrabbingLedge = false;
                    Character.Controller.Rigidbody.gravityScale = 1f;
                }
            }
            else
            {
                Character.Controller.Rigidbody.gravityScale = 1f;
            }
        }
    }
}
