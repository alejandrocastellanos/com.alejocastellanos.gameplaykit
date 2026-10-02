# Input

No ability in the kit reads the keyboard directly. They all read an `ICharacterInput` — a small
interface on the character — and [KeyboardInputReader](../components/core/KeyboardInputReader.md)
is just the default implementation. That gives you two things: the kit works with either Unity
input backend without configuration, and you can drive any character from AI, a replay or your
own Input System actions without changing a single ability.

## The contract: ICharacterInput

`ICharacterInput` lives in `GameplayKit.Core`. Abilities find it with `GetComponent` on the
character when `CharacterCore` initializes them.

| Member | Meaning | Read by (examples) |
|---|---|---|
| `Vector2 MoveInput` | Movement, −1..1 per axis. | Walk/run, top-down, ladders, swimming, flying, aiming the 8-way dash |
| `bool JumpPressedThisFrame` | Jump went down this frame. | Jumps, wall jump, ledge climb, drop-through |
| `bool JumpHeld` | Jump is held. | Variable jump height, glide, jetpack, gravity shaping |
| `bool JumpReleasedThisFrame` | Jump went up this frame. | Jump cut |
| `bool RunHeld` | Run is held. | Walk/run, top-down |
| `bool CrouchHeld` | Crouch is held. | Crouch (crawling uses it through `PlayerCrouch`) |
| `bool DashPressedThisFrame` | Dash went down this frame. | Dashes, ground slam |
| `bool InteractPressedThisFrame` | Interact went down this frame. | Interact, rope grab |
| `GetActionDown / GetAction / GetActionUp(CharacterAction)` | Like `GetKeyDown`, `GetKey`, `GetKeyUp` for the five actions. | Attack, weapon switch, fly, roll, blink |

`CharacterAction` has five values: `Attack`, `Special`, `Fly`, `Roll` and `Blink`. Abilities that
use an action let you choose which one in the Inspector — for example
[PlayerAttack](../components/combat/PlayerAttack.md) has **Attack Action** (`Attack`) and **Switch
Weapon Action** (`Special`), [PlayerFly](../components/movement/PlayerFly.md) has **Toggle Action**
(`Fly`), [PlayerRollDodge](../components/movement/PlayerRollDodge.md) has **Roll Action** (`Roll`)
and [PlayerTeleportBlink](../components/movement/PlayerTeleportBlink.md) has **Blink Action**
(`Blink`).

## KeyboardInputReader

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerRollDodge.mp4" poster="../../assets/clips/PlayerRollDodge.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>PlayerRollDodge listens to the Roll action — Left Alt or the East button by default.</figcaption></figure>

If a character has no input source when `CharacterCore` wakes up, it adds a `KeyboardInputReader`
automatically. Its basic bindings are fixed:

| Input | Keyboard | Gamepad |
|---|---|---|
| Move | WASD or arrows | Left stick |
| Jump | ++space++ | South (A / Cross) |
| Run | Left ++shift++ | Left trigger |
| Crouch | Left ++ctrl++ | — |
| Dash | ++q++ | Right trigger |
| Interact | ++e++ | Select / View |

The five actions are configurable in **Action Bindings**. Each entry has an **Action**, a **Key**
(`KeyCode`) and a **Pad Button**:

| Action | Default key | Default pad button |
|---|---|---|
| Attack | ++j++ | West (X / Square) |
| Special | ++k++ | Right shoulder |
| Fly | ++f++ | North (Y / Triangle) |
| Roll | Left ++alt++ | East (B / Circle) |
| Blink | ++c++ | Left shoulder |

An action is active if **any** of its entries matches, so you can add a second entry for the same
action — for example *Attack* on `Mouse0` to attack with the left mouse button as well as ++j++.
Mouse buttons `Mouse0`–`Mouse4` work as keys with both input backends.

## InputCompat: new Input System or classic

Everything goes through `InputCompat`, a static class that picks the backend at compile time:

- If the **Input System** package is installed and **Active Input Handling** (Player Settings) is
  *Input System Package (New)* or *Both*, it uses the Input System. Keys are still set as `KeyCode`
  in the Inspector and translated by name.
- Otherwise, if the classic **Input Manager** is enabled, it uses `UnityEngine.Input`.

| | Input System | Classic Input Manager |
|---|---|---|
| Keyboard keys and mouse buttons | Yes | Yes |
| Movement | WASD, arrows, then left stick | **Horizontal** / **Vertical** axes |
| Jump | ++space++ or South button | **Jump** button |
| Other gamepad buttons (`PadButton`) | Yes | Never pressed |
| Right stick (`AimStick`) | Yes | Always zero |

So if you need full gamepad support — attacks, dash, run, aiming with the right stick — install
the Input System. You can use `InputCompat` from your own scripts to stay backend-agnostic too:

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

!!! note "What doesn't go through ICharacterInput"
    [CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) reads the mouse and the
    right stick through `InputCompat` directly, and
    [PauseManager](../components/managers/PauseManager.md) reads its own **Pause Key** and **Pause
    Pad Button**. To aim from code, set the aim component to **Stick** mode and call
    `SetAimDirection` — without stick input it keeps the direction you set.

## Write your own input source

Implement `ICharacterInput` on a `MonoBehaviour`, put it on the character, and every ability uses it.
Rules to keep in mind:

1. **Same GameObject as `CharacterCore`**, not a child.
2. **It must exist before `CharacterCore` wakes up.** Abilities cache the input source when they
   initialize. In the Editor that's automatic; from code, add your input component before
   `CharacterCore` (or create the object inactive and activate it at the end).
3. **Remove `KeyboardInputReader`** if the character has one (Create Player adds it). With two input
   sources it is unclear which one the abilities pick.
4. **"ThisFrame" values must be true for exactly one frame.** Track the previous held state and
   compare, as in the examples below.

=== "AI follower"

    A companion that walks towards a target, runs when far away, jumps when the target is higher
    and attacks when close. It drives the same abilities the player uses.

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

                // Hold Jump for 0.3 s, at most once per second, while the target is above us.
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

=== "Input System actions"

    Reads actions from an Input Actions asset (or from a `PlayerInput`'s actions), so players can
    rebind everything with the Input System's own tools. Requires the Input System package.

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
                default: return null; // Fly, Roll and Blink are not mapped in this example
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

    The `?.` operators are used on `InputAction`, a plain C# class, so they are safe here. Make the
    **Move** action a *Value / Vector2* action (for example a 2D Vector composite plus the left
    stick) and the rest *Button* actions.

**Replays** follow the same pattern: record the values of a real input source every frame (move,
the held buttons, the actions), then play them back from an `ICharacterInput` that reads the
recording and derives the "ThisFrame" values by comparing with the previous frame, exactly like the
AI example does.

!!! tip "One frame of latency"
    An input source updates in its own `Update`, which may run before or after `CharacterCore`'s.
    Either way each press is seen exactly once. If you want the abilities to always see it in the
    same frame, add `[DefaultExecutionOrder(-50)]` to your input class.

## Next steps

- [Custom abilities](custom-abilities.md) — read `CharacterInput` from your own ability.
- [Combat](combat.md) — what Attack and Special do with weapons.
