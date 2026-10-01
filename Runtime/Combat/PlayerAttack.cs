using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>
    /// Conecta el input con las armas: al presionar la acción de ataque usa el arma activa del personaje
    /// (o de sus hijos activos, así funciona junto a WeaponInventorySlot). Prioridad: WeaponCombo, WeaponMelee,
    /// WeaponHitscan, WeaponProjectile. Las armas a distancia disparan hacia CharacterAimAndOrient si existe,
    /// o hacia donde mira el personaje.
    /// </summary>
    public class PlayerAttack : AbilityBase
    {
        [SerializeField] private CharacterAction attackAction = CharacterAction.Attack;

        public bool TryAttackNow()
        {
            var combo = GetComponentInChildren<WeaponCombo>();
            if (combo != null) return combo.TryAttack();

            var melee = GetComponentInChildren<WeaponMelee>();
            if (melee != null) return melee.TryAttack();

            Vector2 aim = AimDirection();
            var hitscan = GetComponentInChildren<WeaponHitscan>();
            if (hitscan != null) return hitscan.TryFire(aim);

            var projectile = GetComponentInChildren<WeaponProjectile>();
            if (projectile != null) return projectile.TryFire(aim);

            return false;
        }

        public override void ProcessAbility()
        {
            if (Character.Condition.CurrentState == ConditionState.Dead) return;
            if (CharacterInput.GetActionDown(attackAction)) TryAttackNow();
        }

        private Vector2 AimDirection()
        {
            var aim = GetComponent<CharacterAimAndOrient>();
            return aim != null ? aim.AimDirection : Vector2.right * PhysicsQuery2D.Facing(transform);
        }
    }
}
