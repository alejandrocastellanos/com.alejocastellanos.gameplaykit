using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>
    /// Encadena una secuencia de golpes (por ejemplo varios WeaponMelee) si el input llega dentro
    /// de la ventana de combo; si se agota el tiempo, la secuencia vuelve al primer golpe.
    /// </summary>
    public class WeaponCombo : MonoBehaviour
    {
        [Header("Combo")]
        [SerializeField] private WeaponMelee[] comboHits;
        [SerializeField] private float comboWindow = 0.6f;

        public int CurrentComboIndex { get; private set; }

        private float _windowTimer;

        private void Update()
        {
            if (_windowTimer <= 0f) return;

            _windowTimer -= Time.deltaTime;
            if (_windowTimer <= 0f) CurrentComboIndex = 0;
        }

        public bool TryAttack()
        {
            if (comboHits == null || comboHits.Length == 0) return false;

            var hit = comboHits[CurrentComboIndex];
            if (hit == null || !hit.TryAttack()) return false;

            CurrentComboIndex = (CurrentComboIndex + 1) % comboHits.Length;
            _windowTimer = comboWindow;
            return true;
        }
    }
}
