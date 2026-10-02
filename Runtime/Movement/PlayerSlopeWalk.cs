using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Pendientes: evita que el personaje resbale cuesta abajo al quedarse quieto (el controlador usa un
    /// material sin fricción) y, opcionalmente, inclina una parte visual según el ángulo del suelo.
    /// Las pendientes más empinadas que Max Slope Angle no se tratan como suelo caminable.
    /// </summary>
    public class PlayerSlopeWalk : AbilityBase
    {
        [SerializeField] private float rayLength = 0.3f;
        [SerializeField] private LayerMask groundLayers = ~0;
        [SerializeField] private float maxSlopeAngle = 45f;
        [Tooltip("Parte visual que se inclina con la pendiente (ej. el sprite). Vacío = no se inclina nada.")]
        [SerializeField] private Transform alignVisual;
        [SerializeField] private float rotationSpeed = 10f;

        /// <summary>Ángulo con signo del suelo bajo los pies (0 en plano).</summary>
        public float SlopeAngle { get; private set; }
        public bool OnWalkableSlope { get; private set; }

        private Collider2D _body;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _body = GetComponent<Collider2D>();
        }

        public override void ProcessAbility()
        {
            SlopeAngle = 0f;
            OnWalkableSlope = false;

            if (Character.Controller.IsGrounded && _body != null)
            {
                Bounds b = _body.bounds;
                Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
                RaycastHit2D hit = PhysicsQuery2D.Raycast(origin, Vector2.down, rayLength + 0.05f, groundLayers, transform);
                if (hit.collider != null)
                {
                    float angle = Vector2.SignedAngle(Vector2.up, hit.normal);
                    if (Mathf.Abs(angle) <= maxSlopeAngle)
                    {
                        SlopeAngle = angle;
                        OnWalkableSlope = Mathf.Abs(angle) > 1f;
                    }
                }
            }

            // Quieto sobre una pendiente: sin gravedad ni velocidad, para no deslizarse.
            bool idleOnSlope = OnWalkableSlope && Mathf.Abs(CharacterInput.MoveInput.x) < 0.1f && !CharacterInput.JumpPressedThisFrame
                               && Character.Controller.Velocity.y <= 0.1f;
            if (idleOnSlope)
            {
                Character.Controller.OverrideGravity(this, 0f);
                Character.Controller.Move(Vector2.zero);
            }
            else
            {
                Character.Controller.ReleaseGravity(this);
            }

            if (alignVisual != null)
            {
                Quaternion target = Quaternion.Euler(0f, 0f, SlopeAngle);
                alignVisual.rotation = Quaternion.Lerp(alignVisual.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }

        public override void ResetAbility() => Character.Controller.ReleaseGravity(this);
    }
}
