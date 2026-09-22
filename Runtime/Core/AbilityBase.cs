using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Clase base de la que heredan todas las habilidades de movimiento/combate
    /// (PlayerJump, PlayerDash, PlayerWallJump, etc.). CharacterCore descubre todas las
    /// habilidades presentes en el GameObject y llama su ciclo cada frame, en el orden
    /// Early -> Process -> Late, para que cada una reaccione sin depender del orden de
    /// ejecución por defecto de Unity.
    /// </summary>
    public abstract class AbilityBase : MonoBehaviour
    {
        protected CharacterCore Character { get; private set; }
        public bool AbilityEnabled { get; set; } = true;

        public virtual void Initialize(CharacterCore character)
        {
            Character = character;
        }

        /// <summary>Lee input crudo. Se llama antes que ProcessAbility.</summary>
        public virtual void HandleInput() { }

        /// <summary>Lógica temprana (ej. detectar condiciones antes de que otras habilidades actúen).</summary>
        public virtual void EarlyProcessAbility() { }

        /// <summary>Lógica principal de la habilidad.</summary>
        public virtual void ProcessAbility() { }

        /// <summary>Se llama al final del frame (ej. actualizar parámetros del Animator).</summary>
        public virtual void LateProcessAbility() { }

        /// <summary>Se llama cuando el personaje respawnea o se reinicia el nivel.</summary>
        public virtual void ResetAbility() { }
    }
}
