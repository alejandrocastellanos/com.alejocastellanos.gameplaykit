using System;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Ataque cargado: ponlo junto al arma (WeaponMelee, WeaponHitscan o WeaponProjectile). Con PlayerAttack, mantener
    /// el botón carga y al soltarlo el arma ataca con un daño entre Min y Max según el tiempo cargado.</summary>
    public class WeaponCharge : MonoBehaviour
    {
        [Header("Carga")]
        [SerializeField] private float maxChargeTime = 1.5f;
        [SerializeField] private float minDamage = 5f;
        [SerializeField] private float maxDamage = 30f;

        public bool IsCharging { get; private set; }
        public float ChargeRatio01 => maxChargeTime > 0f ? Mathf.Clamp01(_chargeTimer / maxChargeTime) : 1f;

        /// <summary>Se dispara al soltar, con el daño final ya calculado.</summary>
        public event Action<float> OnReleased;

        private float _chargeTimer;

        public void BeginCharge()
        {
            IsCharging = true;
            _chargeTimer = 0f;
        }

        private void Update()
        {
            if (!IsCharging) return;
            _chargeTimer = Mathf.Min(maxChargeTime, _chargeTimer + Time.deltaTime);
        }

        public float ReleaseCharge()
        {
            IsCharging = false;
            float damage = Mathf.Lerp(minDamage, maxDamage, ChargeRatio01);
            OnReleased?.Invoke(damage);
            _chargeTimer = 0f;
            return damage;
        }
    }
}
