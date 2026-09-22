using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Contrato común para cualquier cosa que pueda recibir daño: personajes (CharacterHealth),
    /// objetos del entorno (DamageableObject) o enemigos. Las armas solo conocen esta interfaz,
    /// nunca el tipo concreto que hay detrás.
    /// </summary>
    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator);
    }
}
