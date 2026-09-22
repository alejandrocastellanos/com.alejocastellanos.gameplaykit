using GameplayKit.Health;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>True si la vida (CharacterHealth) del propio enemigo cayó por debajo de un porcentaje umbral — útil para cambiar de fase.</summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class AIDecisionHealthThreshold : AIDecisionBase
    {
        [Range(0f, 1f)]
        [SerializeField] private float thresholdRatio01 = 0.5f;

        private CharacterHealth _health;

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
        }

        public override bool Decide(AIBrain brain)
        {
            if (_health == null || _health.MaxHealth <= 0f) return false;
            return _health.CurrentHealth / _health.MaxHealth <= thresholdRatio01;
        }
    }
}
