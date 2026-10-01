using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Rodada de esquive: impulso corto con una ventana de invulnerabilidad. Si el personaje tiene
    /// CharacterHealth, durante la rodada no recibe daño.</summary>
    public class PlayerRollDodge : AbilityBase
    {
        [SerializeField] private float rollSpeed = 10f;
        [SerializeField] private float rollDuration = 0.3f;
        [SerializeField] private float cooldown = 0.6f;
        [SerializeField] private CharacterAction rollAction = CharacterAction.Roll;

        public bool IsInvulnerable { get; private set; }
        public bool IsRolling => _isRolling;

        private bool _isRolling;
        private float _cooldownTimer;
        private GameplayKit.Health.CharacterHealth _health;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _health = GetComponent<GameplayKit.Health.CharacterHealth>();
        }

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;

            if (!_isRolling && CharacterInput.GetActionDown(rollAction) && _cooldownTimer <= 0f && Character.Controller.IsGrounded)
            {
                StartCoroutine(RollRoutine());
            }
        }

        private IEnumerator RollRoutine()
        {
            _isRolling = true;
            IsInvulnerable = true;
            _cooldownTimer = cooldown;
            if (_health == null) _health = GetComponent<GameplayKit.Health.CharacterHealth>(); // por si se agregó después
            if (_health != null) _health.GrantInvulnerability(rollDuration);
            Character.Controller.LockHorizontalControl(rollDuration);
            float direction = PhysicsQuery2D.Facing(transform);

            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                Character.Controller.Move(new Vector2(direction * rollSpeed, Character.Controller.Velocity.y));
                elapsed += Time.deltaTime;
                yield return null;
            }

            IsInvulnerable = false;
            _isRolling = false;
        }
    }
}
