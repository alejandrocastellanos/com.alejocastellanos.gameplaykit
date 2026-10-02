using System;

namespace GameplayKit.Core
{
    /// <summary>Cualquier cosa con vida (CharacterHealth, DamageableObject...). Lo usan la IA, las barras de vida,
    /// los números de daño y el parpadeo al recibir golpes sin depender de una clase concreta.</summary>
    public interface IHealthSource
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        event Action<float> Damaged;
        event Action<float> Healed;
    }
}
