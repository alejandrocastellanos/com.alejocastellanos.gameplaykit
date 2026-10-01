using UnityEngine;

namespace GameplayKit.Core
{
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
    }

    /// <summary>
    /// Implementación de referencia de ICharacterInput. Lee a través de InputCompat, así que funciona
    /// tanto con el Input System (New) como con el Input Manager clásico, sin configurar nada.
    /// Bindings: mover = WASD/flechas o stick izquierdo, saltar = Space o botón sur del gamepad,
    /// correr = Shift izquierdo (mantener), agacharse = Ctrl izquierdo (mantener), dash = Q, interactuar = E.
    /// </summary>
    public class KeyboardInputReader : MonoBehaviour, ICharacterInput
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleasedThisFrame { get; private set; }
        public bool RunHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool DashPressedThisFrame { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }

        private void Update()
        {
            MoveInput = InputCompat.MoveAxis;
            JumpPressedThisFrame = InputCompat.JumpPressedThisFrame;
            JumpHeld = InputCompat.JumpHeld;
            JumpReleasedThisFrame = InputCompat.JumpReleasedThisFrame;
            RunHeld = InputCompat.GetKey(KeyCode.LeftShift);
            CrouchHeld = InputCompat.GetKey(KeyCode.LeftControl);
            DashPressedThisFrame = InputCompat.GetKeyDown(KeyCode.Q);
            InteractPressedThisFrame = InputCompat.GetKeyDown(KeyCode.E);
        }
    }
}
