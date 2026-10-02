# Input

Ninguna habilidad del kit lee el teclado directamente. Todas leen un `ICharacterInput` — una
interfaz pequeña en el personaje — y [KeyboardInputReader](../components/core/KeyboardInputReader.md)
es solo la implementación por defecto. Eso te da dos cosas: el kit funciona con cualquiera de los
dos backends de input de Unity sin configurar nada, y puedes controlar cualquier personaje desde una
IA, un replay o tus propias acciones del Input System sin cambiar ni una sola habilidad.

## El contrato: ICharacterInput

`ICharacterInput` vive en `GameplayKit.Core`. Las habilidades lo buscan con `GetComponent` en el
personaje cuando `CharacterCore` las inicializa.

| Miembro | Significado | Lo leen (ejemplos) |
|---|---|---|
| `Vector2 MoveInput` | Movimiento, −1..1 por eje. | Caminar/correr, top-down, escaleras, nado, vuelo, la dirección del dash en 8 direcciones |
| `bool JumpPressedThisFrame` | Jump se presionó en este frame. | Saltos, wall jump, subir cornisas, atravesar plataformas |
| `bool JumpHeld` | Jump está mantenido. | Altura de salto variable, planeo, jetpack, forma de la gravedad |
| `bool JumpReleasedThisFrame` | Jump se soltó en este frame. | Recorte del salto |
| `bool RunHeld` | Correr está mantenido. | Caminar/correr, top-down |
| `bool CrouchHeld` | Agacharse está mantenido. | Agacharse (gatear lo usa a través de `PlayerCrouch`) |
| `bool DashPressedThisFrame` | Dash se presionó en este frame. | Dashes, ground slam |
| `bool InteractPressedThisFrame` | Interactuar se presionó en este frame. | Interactuar, agarrarse de cuerdas |
| `GetActionDown / GetAction / GetActionUp(CharacterAction)` | Como `GetKeyDown`, `GetKey`, `GetKeyUp` para las cinco acciones. | Atacar, cambiar de arma, volar, rodar, blink |

`CharacterAction` tiene cinco valores: `Attack`, `Special`, `Fly`, `Roll` y `Blink`. Las habilidades
que usan una acción te dejan elegir cuál en el Inspector — por ejemplo,
[PlayerAttack](../components/combat/PlayerAttack.md) tiene **Attack Action** (`Attack`) y **Switch
Weapon Action** (`Special`), [PlayerFly](../components/movement/PlayerFly.md) tiene **Toggle Action**
(`Fly`), [PlayerRollDodge](../components/movement/PlayerRollDodge.md) tiene **Roll Action** (`Roll`)
y [PlayerTeleportBlink](../components/movement/PlayerTeleportBlink.md) tiene **Blink Action**
(`Blink`).

## KeyboardInputReader

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerRollDodge.mp4" poster="../../../assets/clips/PlayerRollDodge.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>PlayerRollDodge escucha la acción Roll — Alt izquierdo o el botón este por defecto.</figcaption></figure>

Si un personaje no tiene fuente de input cuando `CharacterCore` despierta, se le agrega un
`KeyboardInputReader` automáticamente. Sus asignaciones básicas son fijas:

| Input | Teclado | Gamepad |
|---|---|---|
| Moverse | WASD o flechas | Stick izquierdo |
| Saltar | ++space++ | Botón sur (A / Cruz) |
| Correr | ++shift++ izquierdo | Gatillo izquierdo |
| Agacharse | ++ctrl++ izquierdo | — |
| Dash | ++q++ | Gatillo derecho |
| Interactuar | ++e++ | Select / View |

Las cinco acciones se configuran en **Action Bindings**. Cada entrada tiene una **Action**, una
**Key** (`KeyCode`) y un **Pad Button**:

| Acción | Tecla por defecto | Botón de gamepad por defecto |
|---|---|---|
| Attack | ++j++ | Oeste (X / Cuadrado) |
| Special | ++k++ | Bumper derecho |
| Fly | ++f++ | Norte (Y / Triángulo) |
| Roll | ++alt++ izquierdo | Este (B / Círculo) |
| Blink | ++c++ | Bumper izquierdo |

Una acción está activa si **cualquiera** de sus entradas coincide, así que puedes agregar una
segunda entrada para la misma acción — por ejemplo *Attack* en `Mouse0` para atacar también con el
botón izquierdo del mouse, además de ++j++. Los botones del mouse `Mouse0`–`Mouse4` funcionan como
teclas con los dos backends de input.

## InputCompat: Input System nuevo o clásico

Todo pasa por `InputCompat`, una clase estática que elige el backend al compilar:

- Si el paquete **Input System** está instalado y **Active Input Handling** (Player Settings) está en
  *Input System Package (New)* o *Both*, usa el Input System. Las teclas se siguen configurando como
  `KeyCode` en el Inspector y se traducen por nombre.
- Si no, y el **Input Manager** clásico está activo, usa `UnityEngine.Input`.

| | Input System | Input Manager clásico |
|---|---|---|
| Teclas y botones del mouse | Sí | Sí |
| Movimiento | WASD, flechas y, si no, el stick izquierdo | Ejes **Horizontal** / **Vertical** |
| Saltar | ++space++ o botón sur | Botón **Jump** |
| Otros botones del gamepad (`PadButton`) | Sí | Nunca se presionan |
| Stick derecho (`AimStick`) | Sí | Siempre cero |

Así que, si necesitas soporte completo de gamepad — ataques, dash, correr, apuntar con el stick
derecho —, instala el Input System. También puedes usar `InputCompat` desde tus propios scripts para
no depender del backend:

```csharp
using GameplayKit.Core;
using UnityEngine;

public class ScreenshotKey : MonoBehaviour
{
    private void Update()
    {
        if (InputCompat.GetKeyDown(KeyCode.F12) || InputCompat.GetPadButtonDown(InputCompat.PadButton.Select))
            ScreenCapture.CaptureScreenshot("screenshot.png");
    }
}
```

!!! note "Lo que no pasa por ICharacterInput"
    [CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) lee el mouse y el stick
    derecho directamente a través de `InputCompat`, y
    [PauseManager](../components/managers/PauseManager.md) lee su propia **Pause Key** y su **Pause
    Pad Button**. Para apuntar desde código, pon el componente de apuntado en modo **Stick** y llama
    a `SetAimDirection` — sin input del stick, mantiene la dirección que fijaste.

## Escribe tu propia fuente de input

Implementa `ICharacterInput` en un `MonoBehaviour`, ponlo en el personaje y todas las habilidades lo
usan. Reglas a tener en cuenta:

1. **En el mismo GameObject que `CharacterCore`**, no en un hijo.
2. **Tiene que existir antes de que `CharacterCore` despierte.** Las habilidades guardan la fuente
   de input cuando se inicializan. En el Editor eso es automático; desde código, agrega tu
   componente de input antes que `CharacterCore` (o crea el objeto desactivado y actívalo al final).
3. **Quita `KeyboardInputReader`** si el personaje tiene uno (Create Player lo agrega). Con dos
   fuentes de input no está claro cuál toman las habilidades.
4. **Los valores "ThisFrame" tienen que ser true durante exactamente un frame.** Guarda el estado
   mantenido del frame anterior y compáralo, como en los ejemplos de abajo.

=== "Acompañante con IA"

    Un compañero que camina hacia un objetivo, corre cuando está lejos, salta cuando el objetivo está
    más arriba y ataca cuando está cerca. Controla las mismas habilidades que usa el jugador.

    ```csharp
    using GameplayKit.Core;
    using UnityEngine;

    public class FollowTargetInput : MonoBehaviour, ICharacterInput
    {
        [SerializeField] private Transform target;
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float runDistance = 6f;
        [SerializeField] private float jumpHeight = 1f;
        [SerializeField] private float attackDistance = 1.2f;

        private bool _jumpHeld, _previousJump, _attackHeld, _previousAttack;
        private float _jumpHoldUntil, _nextJumpTime;

        public Vector2 MoveInput { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpHeld => _jumpHeld;
        public bool JumpReleasedThisFrame { get; private set; }
        public bool RunHeld { get; private set; }
        public bool CrouchHeld => false;
        public bool DashPressedThisFrame => false;
        public bool InteractPressedThisFrame => false;

        public bool GetActionDown(CharacterAction action) =>
            action == CharacterAction.Attack && _attackHeld && !_previousAttack;
        public bool GetAction(CharacterAction action) =>
            action == CharacterAction.Attack && _attackHeld;
        public bool GetActionUp(CharacterAction action) =>
            action == CharacterAction.Attack && !_attackHeld && _previousAttack;

        private void Update()
        {
            _previousJump = _jumpHeld;
            _previousAttack = _attackHeld;
            MoveInput = Vector2.zero;
            RunHeld = false;
            _attackHeld = false;

            if (target != null)
            {
                Vector2 toTarget = target.position - transform.position;
                float distanceX = Mathf.Abs(toTarget.x);
                if (distanceX > stopDistance) MoveInput = new Vector2(Mathf.Sign(toTarget.x), 0f);
                RunHeld = distanceX > runDistance;
                _attackHeld = toTarget.magnitude < attackDistance;

                // Mantiene Jump 0,3 s, como mucho una vez por segundo, mientras el objetivo está más arriba.
                if (toTarget.y > jumpHeight && Time.time >= _nextJumpTime)
                {
                    _jumpHoldUntil = Time.time + 0.3f;
                    _nextJumpTime = Time.time + 1f;
                }
            }

            _jumpHeld = Time.time < _jumpHoldUntil;
            JumpPressedThisFrame = _jumpHeld && !_previousJump;
            JumpReleasedThisFrame = !_jumpHeld && _previousJump;
        }
    }
    ```

=== "Acciones del Input System"

    Lee acciones de un asset de Input Actions (o de las acciones de un `PlayerInput`), así los
    jugadores pueden reasignar todo con las herramientas del propio Input System. Requiere el paquete
    Input System.

    ```csharp
    using GameplayKit.Core;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputActionsReader : MonoBehaviour, ICharacterInput
    {
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private InputActionReference run;
        [SerializeField] private InputActionReference crouch;
        [SerializeField] private InputActionReference dash;
        [SerializeField] private InputActionReference interact;
        [SerializeField] private InputActionReference attack;
        [SerializeField] private InputActionReference special;

        public Vector2 MoveInput => Action(move)?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool JumpPressedThisFrame => Down(jump);
        public bool JumpHeld => Held(jump);
        public bool JumpReleasedThisFrame => Up(jump);
        public bool RunHeld => Held(run);
        public bool CrouchHeld => Held(crouch);
        public bool DashPressedThisFrame => Down(dash);
        public bool InteractPressedThisFrame => Down(interact);

        public bool GetActionDown(CharacterAction action) => Down(For(action));
        public bool GetAction(CharacterAction action) => Held(For(action));
        public bool GetActionUp(CharacterAction action) => Up(For(action));

        private InputActionReference For(CharacterAction action)
        {
            switch (action)
            {
                case CharacterAction.Attack: return attack;
                case CharacterAction.Special: return special;
                default: return null; // Fly, Roll y Blink no se asignan en este ejemplo
            }
        }

        private static InputAction Action(InputActionReference reference) =>
            reference != null ? reference.action : null;
        private static bool Down(InputActionReference r) => Action(r)?.WasPressedThisFrame() ?? false;
        private static bool Held(InputActionReference r) => Action(r)?.IsPressed() ?? false;
        private static bool Up(InputActionReference r) => Action(r)?.WasReleasedThisFrame() ?? false;

        private void OnEnable()
        {
            foreach (var reference in new[] { move, jump, run, crouch, dash, interact, attack, special })
                Action(reference)?.Enable();
        }
    }
    ```

    Los operadores `?.` se usan sobre `InputAction`, una clase C# común, así que aquí son seguros.
    Haz que la acción **Move** sea de tipo *Value / Vector2* (por ejemplo, un composite 2D Vector más
    el stick izquierdo) y el resto acciones de tipo *Button*.

**Los replays** siguen el mismo patrón: graba cada frame los valores de una fuente de input real
(movimiento, botones mantenidos, acciones) y luego reprodúcelos desde un `ICharacterInput` que lea la
grabación y calcule los valores "ThisFrame" comparando con el frame anterior, exactamente como hace
el ejemplo de la IA.

!!! tip "Un frame de latencia"
    Una fuente de input se actualiza en su propio `Update`, que puede correr antes o después del de
    `CharacterCore`. En cualquier caso, cada pulsación se ve exactamente una vez. Si quieres que las
    habilidades siempre la vean en el mismo frame, agrega `[DefaultExecutionOrder(-50)]` a tu clase
    de input.

## Siguientes pasos

- [Habilidades propias](custom-abilities.md) — lee `CharacterInput` desde tu propia habilidad.
- [Combate](combat.md) — qué hacen Attack y Special con las armas.
