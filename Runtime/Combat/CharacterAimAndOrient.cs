using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Combat
{
    /// <summary>Calcula hacia dónde apunta el personaje (mouse, stick derecho o el objetivo más cercano) y, si se asigna
    /// Part To Rotate, gira esa parte (el arma o el brazo). El cuerpo del personaje no se rota.</summary>
    public class CharacterAimAndOrient : MonoBehaviour
    {
        public enum AimMode { Mouse, Stick, ClosestTarget }

        [Header("Apuntado")]
        [SerializeField] private AimMode aimMode = AimMode.Mouse;
        [Tooltip("Parte que gira hacia el apuntado (arma, brazo). Vacío = solo se calcula AimDirection.")]
        [SerializeField] private Transform partToRotate;
        [SerializeField] private float stickDeadZone = 0.25f;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float targetSearchRadius = 10f;
        [SerializeField] private LayerMask targetLayers = ~0;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private void Reset()
        {
            aimCamera = Camera.main;
        }

        private void Update()
        {
            switch (aimMode)
            {
                case AimMode.Mouse:
                    AimAtMouse();
                    break;
                case AimMode.ClosestTarget:
                    AimAtClosestTarget();
                    break;
                case AimMode.Stick:
                    Vector2 stick = InputCompat.AimStick;
                    if (stick.magnitude > stickDeadZone) SetAimDirection(stick);
                    break;
            }

            ApplyRotation();
        }

        /// <summary>Usado externamente para el modo Stick, o para forzar la dirección de apuntado.</summary>
        public void SetAimDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f) AimDirection = direction.normalized;
        }

        private void AimAtMouse()
        {
            var cam = aimCamera != null ? aimCamera : Camera.main;
            if (cam == null) return;

            Vector3 mouseWorld = cam.ScreenToWorldPoint((Vector3)InputCompat.MousePosition);
            SetAimDirection((Vector2)mouseWorld - (Vector2)transform.position);
        }

        private void AimAtClosestTarget()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, targetSearchRadius, targetLayers);
            Transform closest = null;
            float closestDistance = float.MaxValue;

            Transform owner = PhysicsQuery2D.OwnerOf(this);
            foreach (var hit in hits)
            {
                // Solo cuenta como objetivo algo que se pueda dañar y que no sea el propio personaje.
                if (PhysicsQuery2D.IsPartOf(hit, owner) || hit.GetComponentInParent<IDamageable>() == null) continue;
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = hit.transform;
                }
            }

            if (closest != null)
            {
                SetAimDirection((Vector2)closest.position - (Vector2)transform.position);
            }
        }

        private void ApplyRotation()
        {
            if (partToRotate == null) return;
            // Ángulo en el espacio del personaje: si está volteado (escala X negativa) el arma no queda boca abajo.
            float facing = PhysicsQuery2D.Facing(transform);
            Vector2 local = new Vector2(AimDirection.x * facing, AimDirection.y);
            float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
            partToRotate.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
