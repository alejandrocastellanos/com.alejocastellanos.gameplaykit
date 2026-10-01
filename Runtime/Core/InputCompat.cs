#if GAMEPLAYKIT_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
#define GK_NEW_INPUT
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
#if GK_NEW_INPUT
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace GameplayKit.Core
{
    /// <summary>
    /// Capa de compatibilidad de input: todo el paquete lee teclado/mouse/gamepad por aquí, nunca con
    /// UnityEngine.Input directamente. Usa el Input System (paquete com.unity.inputsystem) cuando está
    /// activo en Player Settings y cae al Input Manager clásico en caso contrario, así el kit funciona
    /// en proyectos con "Input System Package (New)", "Input Manager (Old)" o "Both".
    /// Las teclas se siguen configurando como KeyCode en el Inspector; aquí se traducen.
    /// </summary>
    public static class InputCompat
    {
        public static bool GetKey(KeyCode key)
        {
#if GK_NEW_INPUT
            var control = FindControl(key);
            return control != null && control.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if GK_NEW_INPUT
            var control = FindControl(key);
            return control != null && control.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

        public static bool GetKeyUp(KeyCode key)
        {
#if GK_NEW_INPUT
            var control = FindControl(key);
            return control != null && control.wasReleasedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyUp(key);
#else
            return false;
#endif
        }

        /// <summary>Posición del puntero en píxeles de pantalla (equivalente a Input.mousePosition).</summary>
        public static Vector2 MousePosition
        {
            get
            {
#if GK_NEW_INPUT
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mousePosition;
#else
                return Vector2.zero;
#endif
            }
        }

        /// <summary>Movimiento crudo (-1..1 por eje): WASD / flechas, o stick izquierdo del gamepad.</summary>
        public static Vector2 MoveAxis
        {
            get
            {
#if GK_NEW_INPUT
                Vector2 move = Vector2.zero;
                var kb = Keyboard.current;
                if (kb != null)
                {
                    move.x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                    move.y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                }
                var pad = Gamepad.current;
                if (move == Vector2.zero && pad != null) move = pad.leftStick.ReadValue();
                return Vector2.ClampMagnitude(move, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
                return Vector2.zero;
#endif
            }
        }

        /// <summary>Salto: Space o botón sur del gamepad (A / Cruz). En el sistema clásico usa el botón "Jump".</summary>
        public static bool JumpHeld
        {
            get
            {
#if GK_NEW_INPUT
                return GetKey(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetButton("Jump");
#else
                return false;
#endif
            }
        }

        public static bool JumpPressedThisFrame
        {
            get
            {
#if GK_NEW_INPUT
                return GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetButtonDown("Jump");
#else
                return false;
#endif
            }
        }

        public static bool JumpReleasedThisFrame
        {
            get
            {
#if GK_NEW_INPUT
                return GetKeyUp(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasReleasedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetButtonUp("Jump");
#else
                return false;
#endif
            }
        }

#if GK_NEW_INPUT
        private static readonly Dictionary<KeyCode, Key> KeyMap = new Dictionary<KeyCode, Key>();

        private static ButtonControl FindControl(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Mouse0: return Mouse.current?.leftButton;
                case KeyCode.Mouse1: return Mouse.current?.rightButton;
                case KeyCode.Mouse2: return Mouse.current?.middleButton;
                case KeyCode.Mouse3: return Mouse.current?.backButton;
                case KeyCode.Mouse4: return Mouse.current?.forwardButton;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null) return null;

            if (!KeyMap.TryGetValue(keyCode, out var key))
            {
                key = TranslateKey(keyCode);
                KeyMap[keyCode] = key;
            }
            return key == Key.None ? null : keyboard[key];
        }

        /// <summary>Traduce un KeyCode clásico al Key del Input System por nombre, con las excepciones conocidas.</summary>
        private static Key TranslateKey(KeyCode keyCode)
        {
            string name = keyCode.ToString();
            if (name.StartsWith("Alpha")) name = "Digit" + name.Substring(5);
            else if (name.StartsWith("Keypad")) name = "Numpad" + name.Substring(6);

            switch (name)
            {
                case "Return": name = "Enter"; break;
                case "LeftControl": name = "LeftCtrl"; break;
                case "RightControl": name = "RightCtrl"; break;
                case "LeftCommand": case "LeftApple": case "LeftWindows": name = "LeftMeta"; break;
                case "RightCommand": case "RightApple": case "RightWindows": name = "RightMeta"; break;
                case "BackQuote": name = "Backquote"; break;
                case "NumpadPeriod": name = "NumpadPeriod"; break;
                case "NumpadPlus": name = "NumpadPlus"; break;
            }

            return Enum.TryParse(name, out Key key) ? key : Key.None;
        }
#endif
    }
}
