using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Contrato de entrada que consumen las habilidades (PlayerJump, PlayerWalkRun, etc.).
    /// Al ser una interfaz, cambiar del Input Manager clásico al New Input System más
    /// adelante no obliga a tocar ninguna habilidad — solo se reemplaza la implementación.
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
    /// Implementación de referencia usando el Input Manager clásico de Unity. Suficiente
    /// para prototipar sin depender de un paquete extra; todas las habilidades dependen de
    /// ICharacterInput, así que reemplazar esto por el New Input System no las afecta.
    /// Bindings: mover = ejes Horizontal/Vertical, saltar = Space ("Jump"), correr = Shift
    /// (mantener), agacharse = Ctrl izquierdo (mantener), dash = Q, interactuar = E.
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
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            JumpPressedThisFrame = Input.GetButtonDown("Jump");
            JumpHeld = Input.GetButton("Jump");
            JumpReleasedThisFrame = Input.GetButtonUp("Jump");
            RunHeld = Input.GetKey(KeyCode.LeftShift);
            CrouchHeld = Input.GetKey(KeyCode.LeftControl);
            DashPressedThisFrame = Input.GetKeyDown(KeyCode.Q);
            InteractPressedThisFrame = Input.GetKeyDown(KeyCode.E);
        }
    }
}
