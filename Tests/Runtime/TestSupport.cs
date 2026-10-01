using System;
using System.Collections.Generic;
using System.Reflection;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Tests
{
    /// <summary>
    /// ICharacterInput controlable desde los tests: se fija el estado "mantenido" de cada botón y los
    /// eventos de un frame (PressedThisFrame / ReleasedThisFrame) se derivan solos, igual que con un teclado.
    /// Corre antes que CharacterCore para que las habilidades vean el input del frame actual.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ScriptedCharacterInput : MonoBehaviour, ICharacterInput
    {
        public Vector2 Move;
        public bool Jump;
        public bool Run;
        public bool Crouch;

        public Vector2 MoveInput => Move;
        public bool JumpHeld => Jump;
        public bool RunHeld => Run;
        public bool CrouchHeld => Crouch;
        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpReleasedThisFrame { get; private set; }
        public bool DashPressedThisFrame { get; private set; }
        public bool InteractPressedThisFrame { get; private set; }

        private bool _previousJump;
        private bool _dashQueued;
        private bool _interactQueued;

        public void TapDash() => _dashQueued = true;
        public void TapInteract() => _interactQueued = true;

        // Acciones: se "mantienen" con Hold/Release o se pulsan un frame con TapAction.
        private readonly HashSet<CharacterAction> _held = new HashSet<CharacterAction>();
        private readonly HashSet<CharacterAction> _previousHeld = new HashSet<CharacterAction>();
        private readonly HashSet<CharacterAction> _tapQueued = new HashSet<CharacterAction>();
        private readonly HashSet<CharacterAction> _down = new HashSet<CharacterAction>();
        private readonly HashSet<CharacterAction> _up = new HashSet<CharacterAction>();

        public void Hold(CharacterAction action) => _held.Add(action);
        public void Release(CharacterAction action) => _held.Remove(action);
        public void TapAction(CharacterAction action) => _tapQueued.Add(action);

        public bool GetActionDown(CharacterAction action) => _down.Contains(action);
        public bool GetAction(CharacterAction action) => _held.Contains(action) || _down.Contains(action);
        public bool GetActionUp(CharacterAction action) => _up.Contains(action);

        private void Update()
        {
            JumpPressedThisFrame = Jump && !_previousJump;
            JumpReleasedThisFrame = !Jump && _previousJump;
            _previousJump = Jump;

            DashPressedThisFrame = _dashQueued;
            _dashQueued = false;
            InteractPressedThisFrame = _interactQueued;
            _interactQueued = false;

            _down.Clear(); _up.Clear();
            foreach (var a in _held) if (!_previousHeld.Contains(a)) _down.Add(a);
            foreach (var a in _previousHeld) if (!_held.Contains(a)) _up.Add(a);
            foreach (var a in _tapQueued) _down.Add(a);
            _tapQueued.Clear();
            _previousHeld.Clear();
            foreach (var a in _held) _previousHeld.Add(a);
        }
    }

    /// <summary>Construye niveles mínimos por código y limpia todo al terminar cada test.</summary>
    public class TestWorld : IDisposable
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        public GameObject Track(GameObject go) { _created.Add(go); return go; }

        public GameObject Block(string name, Vector2 center, Vector2 size, bool trigger = false)
        {
            var go = Track(new GameObject(name));
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.isTrigger = trigger;
            return go;
        }

        public GameObject Ground(float y = -0.5f, float width = 200f) =>
            Block("Ground", new Vector2(0f, y - 0.5f), new Vector2(width, 1f));

        /// <summary>
        /// Crea un personaje como lo armaría un usuario: Collider + input + habilidades + CharacterCore al final
        /// (CharacterCore descubre las habilidades en su Awake).
        /// </summary>
        public CharacterCore Player(Vector2 position, out ScriptedCharacterInput input, params Type[] abilities)
        {
            var go = Track(new GameObject("Player"));
            go.transform.position = position;
            go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
            go.AddComponent<CharacterController2D>();
            input = go.AddComponent<ScriptedCharacterInput>();
            foreach (var ability in abilities) go.AddComponent(ability);
            return go.AddComponent<CharacterCore>();
        }

        /// <summary>Asigna un campo [SerializeField] privado, como lo haría el Inspector.</summary>
        public static void Set(object target, string field, object value)
        {
            FieldInfo info = null;
            for (var type = target.GetType(); type != null && info == null; type = type.BaseType)
                info = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (info == null) throw new ArgumentException($"No existe el campo {field} en {target.GetType().Name}");
            info.SetValue(target, value);
        }

        public GameObject Point(Vector2 position)
        {
            var go = Track(new GameObject("Point"));
            go.transform.position = position;
            return go;
        }

        public void Dispose()
        {
            foreach (var go in _created) if (go != null) UnityEngine.Object.Destroy(go);
            _created.Clear();
        }
    }
}
