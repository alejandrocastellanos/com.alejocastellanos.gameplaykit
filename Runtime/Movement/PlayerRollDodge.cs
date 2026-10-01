using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Rodada de esquive: impulso corto con una ventana de invulnerabilidad (IsInvulnerable).</summary>
    public class PlayerRollDodge : AbilityBase
    {
        [SerializeField] private float rollSpeed = 10f;
        [SerializeField] private float rollDuration = 0.3f;
        [SerializeField] private float cooldown = 0.6f;
        [SerializeField] private KeyCode rollKey = KeyCode.LeftAlt;

        public bool IsInvulnerable { get; private set; }
        private bool _isRolling;
        private float _cooldownTimer;

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;

            if (!_isRolling && InputCompat.GetKeyDown(rollKey) && _cooldownTimer <= 0f && Character.Controller.IsGrounded)
            {
                StartCoroutine(RollRoutine());
            }
        }

        private IEnumerator RollRoutine()
        {
            _isRolling = true;
            IsInvulnerable = true;
            _cooldownTimer = cooldown;
            float direction = Mathf.Sign(transform.localScale.x);

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
