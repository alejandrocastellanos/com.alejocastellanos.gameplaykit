using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Se aleja del objetivo del AIBrain en la dirección opuesta mientras el estado esté activo.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIActionFlee : AIActionBase
    {
        [SerializeField] private float speed = 3.5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public override void PerformAction(AIBrain brain)
        {
            if (brain.Target == null || _rb == null) return;

            float direction = Mathf.Sign(transform.position.x - brain.Target.position.x);
            _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
        }

        public override void OnExitState(AIBrain brain)
        {
            if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }
}
