using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Verdadero cuando la vida del enemigo baja de un porcentaje. Funciona con CharacterHealth o
    /// DamageableObject (cualquier IHealthSource en el objeto o sus padres).</summary>
    public class AIDecisionHealthThreshold : AIDecisionBase
    {
        [Range(0f, 1f)]
        [SerializeField] private float thresholdRatio01 = 0.5f;

        private IHealthSource _health;

        private void Awake()
        {
            _health = GetComponentInParent<IHealthSource>();
        }

        public override bool Decide(AIBrain brain)
        {
            if (_health == null) _health = GetComponentInParent<IHealthSource>();
            if (_health == null || _health.MaxHealth <= 0f) return false;
            return _health.CurrentHealth / _health.MaxHealth <= thresholdRatio01;
        }
    }
}
