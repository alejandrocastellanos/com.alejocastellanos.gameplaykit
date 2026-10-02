using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.UI
{
    /// <summary>Muestra un número flotante cada vez que este objeto recibe daño o se cura. Funciona con CharacterHealth
    /// o DamageableObject (cualquier IHealthSource en el objeto o sus padres).</summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        [Tooltip("Prefab propio opcional. Vacío = un texto simple creado por código.")]
        [SerializeField] private UIDamagePopup popupPrefab;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.2f);
        [SerializeField] private bool showHeals = true;

        private IHealthSource _health;

        private void OnEnable()
        {
            _health = GetComponentInParent<IHealthSource>();
            if (_health == null) return;
            _health.Damaged += HandleDamaged;
            _health.Healed += HandleHealed;
        }

        private void OnDisable()
        {
            if (_health == null) return;
            _health.Damaged -= HandleDamaged;
            _health.Healed -= HandleHealed;
        }

        private void HandleDamaged(float amount) => UIDamagePopup.Spawn(transform.position + (Vector3)offset, amount, false, popupPrefab);

        private void HandleHealed(float amount)
        {
            if (showHeals) UIDamagePopup.Spawn(transform.position + (Vector3)offset, amount, true, popupPrefab);
        }
    }
}
