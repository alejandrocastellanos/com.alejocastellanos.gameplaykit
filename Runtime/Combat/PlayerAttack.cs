using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>
    /// Conecta el input con las armas: al presionar la acción de ataque usa el arma activa del personaje
    /// (o de sus hijos activos, así funciona junto a WeaponInventorySlot). Prioridad: WeaponCombo, WeaponMelee,
    /// WeaponHitscan, WeaponProjectile. Si el arma activa tiene WeaponCharge, mantener el botón carga y
    /// soltarlo ataca con el daño cargado. Las armas a distancia disparan hacia CharacterAimAndOrient si
    /// existe, o hacia donde mira el personaje. Con WeaponInventorySlot, la acción de cambio pasa a la siguiente arma.
    /// </summary>
    public class PlayerAttack : AbilityBase
    {
        [SerializeField] private CharacterAction attackAction = CharacterAction.Attack;
        [Tooltip("Acción para cambiar de arma cuando hay un WeaponInventorySlot.")]
        [SerializeField] private CharacterAction switchWeaponAction = CharacterAction.Special;

        public bool TryAttackNow() => TryAttackNow(null);

        /// <summary>Ataca con el arma activa; <paramref name="damageOverride"/> reemplaza el daño configurado (no aplica a combos).</summary>
        public bool TryAttackNow(float? damageOverride)
        {
            var combo = GetComponentInChildren<WeaponCombo>();
            if (combo != null) return combo.TryAttack();

            var melee = GetComponentInChildren<WeaponMelee>();
            if (melee != null) return damageOverride.HasValue ? melee.TryAttack(damageOverride.Value) : melee.TryAttack();

            Vector2 aim = AimDirection();
            var hitscan = GetComponentInChildren<WeaponHitscan>();
            if (hitscan != null) return damageOverride.HasValue ? hitscan.TryFire(aim, damageOverride.Value) : hitscan.TryFire(aim);

            var projectile = GetComponentInChildren<WeaponProjectile>();
            if (projectile != null) return damageOverride.HasValue ? projectile.TryFire(aim, damageOverride.Value) : projectile.TryFire(aim);

            return false;
        }

        public override void ProcessAbility()
        {
            if (Character.Condition.CurrentState == ConditionState.Dead) return;

            var inventory = GetComponent<WeaponInventorySlot>();
            if (inventory != null && CharacterInput.GetActionDown(switchWeaponAction)) inventory.EquipNext();

            var charge = GetComponentInChildren<WeaponCharge>();
            if (charge != null)
            {
                if (CharacterInput.GetActionDown(attackAction)) charge.BeginCharge();
                else if (charge.IsCharging && !CharacterInput.GetAction(attackAction)) TryAttackNow(charge.ReleaseCharge());
                return;
            }

            if (CharacterInput.GetActionDown(attackAction)) TryAttackNow();
        }

        private Vector2 AimDirection()
        {
            var aim = GetComponentInChildren<CharacterAimAndOrient>();
            return aim != null ? aim.AimDirection : Vector2.right * PhysicsQuery2D.Facing(transform);
        }
    }
}
