using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Inflige daño según la distancia caída al aterrizar, por encima de un umbral mínimo.</summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class CharacterFallDamage : AbilityBase
    {
        [Header("Daño por caída")]
        [SerializeField] private float minFallDistance = 5f;
        [SerializeField] private float damagePerUnit = 4f;
        [SerializeField] private float maxDamage = 60f;

        private CharacterHealth _health;
        private float _fallStartHeight;
        private bool _wasGrounded;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _health = GetComponent<CharacterHealth>();
        }

        public override void ProcessAbility()
        {
            bool isGrounded = Character.Controller.IsGrounded;

            if (!isGrounded && _wasGrounded)
            {
                _fallStartHeight = transform.position.y;
            }
            else if (isGrounded && !_wasGrounded)
            {
                float fallDistance = _fallStartHeight - transform.position.y;
                if (fallDistance > minFallDistance)
                {
                    float damage = Mathf.Min(maxDamage, (fallDistance - minFallDistance) * damagePerUnit);
                    _health.TakeDamage(damage);
                }
            }

            _wasGrounded = isGrounded;
        }
    }
}
