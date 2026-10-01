using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>Acciones con botón propio más allá de moverse/saltar/correr/agacharse/dash/interactuar.</summary>
    public enum CharacterAction
    {
        Attack,
        Special,
        Fly,
        Roll,
        Blink
    }

    /// <summary>
    /// Contrato de entrada que consumen las habilidades (PlayerJump, PlayerWalkRun, etc.).
    /// Al ser una interfaz, se puede reemplazar KeyboardInputReader por otra implementación
    /// (PlayerInput con acciones, IA, replays...) sin tocar ninguna habilidad.
    /// </summary>
    public interface ICharacterInput
    {
        Vector2 MoveInput { get; }
        bool JumpPressedThisFrame { get; }
        bool JumpHeld { get; }
        bool JumpReleasedThisFrame { get; }
        bool RunHeld { get; }
        bool CrouchHeld { get; }
        bool DashPressedThisFrame { get; }
        bool InteractPressedThisFrame { get; }

        /// <summary>True solo en el frame en que se presiona la acción (como GetKeyDown).</summary>
        bool GetActionDown(CharacterAction action);
        /// <summary>True mientras la acción está mantenida (como GetKey).</summary>
        bool GetAction(CharacterAction action);
        /// <summary>True solo en el frame en que se suelta la acción (como GetKeyUp).</summary>
        bool GetActionUp(CharacterAction action);
    }
}
