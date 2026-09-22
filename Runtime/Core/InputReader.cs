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
        bool DashPressedThisFrame { get; }
    }

    /// <summary>
    /// Implementación de referencia usando el Input Manager clásico de Unity. Suficiente
    /// para prototipar sin depender de un paquete extra; todas las habilidades dependen de
    /// ICharacterInput, así que reemplazar esto por el New Input System no las afecta.
    /// </summary>
    public class KeyboardInputReader : MonoBehaviour, ICharacterInput
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleasedThisFrame { get; private set; }
        public bool DashPressedThisFrame { get; private set; }

        private void Update()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            JumpPressedThisFrame = Input.GetButtonDown("Jump");
            JumpHeld = Input.GetButton("Jump");
            JumpReleasedThisFrame = Input.GetButtonUp("Jump");
            DashPressedThisFrame = Input.GetKeyDown(KeyCode.LeftShift);
        }
    }
}
