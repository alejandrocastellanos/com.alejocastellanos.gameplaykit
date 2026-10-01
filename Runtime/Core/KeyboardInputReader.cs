using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Implementación de referencia de ICharacterInput. Lee a través de InputCompat, así que funciona
    /// tanto con el Input System (New) como con el Input Manager clásico, sin configurar nada.
    /// Bindings: mover = WASD/flechas o stick izquierdo, saltar = Space o botón sur del gamepad,
    /// correr = Shift izquierdo (mantener), agacharse = Ctrl izquierdo (mantener), dash = Q, interactuar = E.
    /// Las acciones (atacar, especial, volar, rodar, blink) se asignan en el Inspector, con su botón de gamepad.
    /// </summary>
    public class KeyboardInputReader : MonoBehaviour, ICharacterInput
    {
        [System.Serializable]
        public struct ActionBinding
        {
            public CharacterAction action;
            public KeyCode key;
            public InputCompat.PadButton padButton;
        }

        [SerializeField] private ActionBinding[] actionBindings =
        {
            new ActionBinding { action = CharacterAction.Attack, key = KeyCode.J, padButton = InputCompat.PadButton.West },
            new ActionBinding { action = CharacterAction.Special, key = KeyCode.K, padButton = InputCompat.PadButton.RightShoulder },
            new ActionBinding { action = CharacterAction.Fly, key = KeyCode.F, padButton = InputCompat.PadButton.North },
            new ActionBinding { action = CharacterAction.Roll, key = KeyCode.LeftAlt, padButton = InputCompat.PadButton.East },
            new ActionBinding { action = CharacterAction.Blink, key = KeyCode.C, padButton = InputCompat.PadButton.LeftShoulder },
        };

        public Vector2 MoveInput { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleasedThisFrame { get; private set; }
        public bool RunHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool DashPressedThisFrame { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }

        public bool GetActionDown(CharacterAction action)
        {
            foreach (var b in actionBindings)
                if (b.action == action && (InputCompat.GetKeyDown(b.key) || InputCompat.GetPadButtonDown(b.padButton))) return true;
            return false;
        }

        public bool GetAction(CharacterAction action)
        {
            foreach (var b in actionBindings)
                if (b.action == action && (InputCompat.GetKey(b.key) || InputCompat.GetPadButton(b.padButton))) return true;
            return false;
        }

        public bool GetActionUp(CharacterAction action)
        {
            foreach (var b in actionBindings)
                if (b.action == action && (InputCompat.GetKeyUp(b.key) || InputCompat.GetPadButtonUp(b.padButton))) return true;
            return false;
        }

        private void Update()
        {
            MoveInput = InputCompat.MoveAxis;
            JumpPressedThisFrame = InputCompat.JumpPressedThisFrame;
            JumpHeld = InputCompat.JumpHeld;
            JumpReleasedThisFrame = InputCompat.JumpReleasedThisFrame;
            RunHeld = InputCompat.GetKey(KeyCode.LeftShift) || InputCompat.GetPadButton(InputCompat.PadButton.LeftTrigger);
            CrouchHeld = InputCompat.GetKey(KeyCode.LeftControl);
            DashPressedThisFrame = InputCompat.GetKeyDown(KeyCode.Q) || InputCompat.GetPadButtonDown(InputCompat.PadButton.RightTrigger);
            InteractPressedThisFrame = InputCompat.GetKeyDown(KeyCode.E) || InputCompat.GetPadButtonDown(InputCompat.PadButton.Select);
        }
    }
}
