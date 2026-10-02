---
title: Core API
description: Reference for Gameplay Kit's non-component types, including interfaces, static helpers, enums and the state machine types.
---

# Core API

These types aren't components, so they don't have component pages. They're the contracts and helpers the
components are built on, and the ones you'll use most when writing your own scripts. All of them are in the
`GameplayKit.Core` namespace unless noted otherwise.

## Interfaces

### ICharacterInput

What abilities read instead of the keyboard. `KeyboardInputReader` implements it. Implement it yourself to
drive a character from AI, a replay, a network or the Input System's `PlayerInput`.

| Member | Description |
|---|---|
| `Vector2 MoveInput { get; }` | Movement, −1..1 per axis. |
| `bool JumpPressedThisFrame { get; }` | True only on the frame jump is pressed. |
| `bool JumpHeld { get; }` | True while jump is held. |
| `bool JumpReleasedThisFrame { get; }` | True only on the frame jump is released. |
| `bool RunHeld { get; }` | True while run is held. |
| `bool CrouchHeld { get; }` | True while crouch is held. |
| `bool DashPressedThisFrame { get; }` | True only on the frame dash is pressed. |
| `bool InteractPressedThisFrame { get; }` | True only on the frame interact is pressed. |
| `bool GetActionDown(CharacterAction action)` | True only on the frame the action is pressed (like `GetKeyDown`). |
| `bool GetAction(CharacterAction action)` | True while the action is held (like `GetKey`). |
| `bool GetActionUp(CharacterAction action)` | True only on the frame the action is released (like `GetKeyUp`). |

Abilities grab the input component once, in `Initialize`, with `GetComponent<ICharacterInput>()`. Keep only
one implementation on the character: `CharacterCore` adds a `KeyboardInputReader` only when there's none.
Update your values *before* `CharacterCore.Update` reads them, for example with a negative
`[DefaultExecutionOrder]`, as the test input does.

```csharp
using GameplayKit.Core;
using UnityEngine;

/// Auto-runner: always runs right; jump with Space, gamepad A or a mouse click.
[DefaultExecutionOrder(-100)]
public class AutoRunnerInput : MonoBehaviour, ICharacterInput
{
    public Vector2 MoveInput => Vector2.right;
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public bool RunHeld => true;
    public bool CrouchHeld => false;
    public bool DashPressedThisFrame => false;
    public bool InteractPressedThisFrame => false;

    public bool GetActionDown(CharacterAction action) => false;
    public bool GetAction(CharacterAction action) => false;
    public bool GetActionUp(CharacterAction action) => false;

    private void Update()
    {
        JumpPressedThisFrame = InputCompat.JumpPressedThisFrame || InputCompat.GetKeyDown(KeyCode.Mouse0);
        JumpHeld = InputCompat.JumpHeld || InputCompat.GetKey(KeyCode.Mouse0);
        JumpReleasedThisFrame = InputCompat.JumpReleasedThisFrame || InputCompat.GetKeyUp(KeyCode.Mouse0);
    }
}
```

### IDamageable

Anything that can take damage. Weapons, hazards and enemies only know this interface. Implemented by
`CharacterHealth`, `DamageableObject` and `BreakableObject`.

| Member | Description |
|---|---|
| `void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)` | Apply `amount` damage. `hitDirection` points away from the source (`CharacterHealth` uses it for knockback). `instigator` is the attacker and may be `null`. |

Callers look for it with `GetComponentInParent<IDamageable>()` on the collider they hit, so it can sit on the
root of a character whose colliders are children.

```csharp
using GameplayKit.Core;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    public float TotalDamage { get; private set; }

    public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
    {
        TotalDamage += amount;
        Debug.Log($"{(instigator != null ? instigator.name : "Something")} hit {name} for {amount}");
    }
}
```

### IHealthSource

Anything with health. Feedback and AI read it without depending on a concrete class. Implemented by
`CharacterHealth` and `DamageableObject`.

| Member | Description |
|---|---|
| `float CurrentHealth { get; }` | Current health. |
| `float MaxHealth { get; }` | Maximum health. |
| `event Action<float> Damaged` | Raised with the damage amount. |
| `event Action<float> Healed` | Raised with the heal amount. `DamageableObject` never heals, so it never fires there. |

Used by `DamageFlash`, `DamagePopupSpawner`, `AIDecisionHealthThreshold` and `CharacterAnimatorBridge`.

```csharp
var health = GetComponentInParent<IHealthSource>();
health.Damaged += amount => Debug.Log($"-{amount} ({health.CurrentHealth}/{health.MaxHealth})");
```

### IKeyHolder

Something that carries item or key ids, normally the player. Implemented by `InventoryManager`. Used by
`ItemPickup` (adds), `DoorWithKey` (checks and optionally removes) and `LevelExit` (checks).

| Member | Description |
|---|---|
| `void AddKey(string keyId)` | Store an id. |
| `bool HasKey(string keyId)` | Whether the id is held. |
| `void RemoveKey(string keyId)` | Remove an id. |

### IInteractable

Something the player can activate with the interact button. `PlayerInteract` finds the closest one within its
radius (triggers included) and calls `Interact`. Implemented by `Lever`.

| Member | Description |
|---|---|
| `void Interact(GameObject interactor)` | Called with the interacting character's GameObject. |

```csharp
using GameplayKit.Core;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId = "llave_dorada";
    private bool _opened;

    public void Interact(GameObject interactor)
    {
        if (_opened) return;
        var holder = interactor.GetComponentInParent<IKeyHolder>();
        if (holder == null) return;
        holder.AddKey(itemId);
        _opened = true;
    }
}
```

Give the chest a `Collider2D` (a trigger is fine) so `PlayerInteract` can find it.

## Static helpers

### PhysicsQuery2D

2D physics queries that ignore the asking character. Ground, wall, ledge and sight checks start inside the
asker's collider, and raw `Physics2D` would report that collider. With these, layer masks can stay on
*Everything*.

| Member | Description |
|---|---|
| `static Transform OwnerOf(Component component)` | The physical root of a component: the Transform of its parent `Rigidbody2D`, or its own Transform when there's none. |
| `static bool IsPartOf(Collider2D collider, Transform owner)` | True when the collider is the owner itself, a child of it, or attached to the owner's Rigidbody2D. |
| `static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask mask, Transform ignore, Transform ignoreAlso = null, bool includeTriggers = false)` | The closest hit that belongs to neither `ignore` nor `ignoreAlso`. Triggers are skipped unless `includeTriggers`. Check `hit.collider != null`. |
| `static bool OverlapCircle(Vector2 point, float radius, LayerMask mask, Transform ignore, bool includeTriggers = false)` | True if any collider in the circle doesn't belong to `ignore` (solid colliders only by default). |
| `static float Facing(Transform transform)` | 1 or −1: the sign of `lossyScale.x`, which is how `PlayerWalkRun` and the enemy movers flip characters. |

```csharp
// Is there a ceiling right above the character? Ignore its own colliders.
Bounds b = GetComponent<Collider2D>().bounds;
RaycastHit2D hit = PhysicsQuery2D.Raycast(new Vector2(b.center.x, b.max.y), Vector2.up, 0.2f, ~0, transform);
bool blocked = hit.collider != null;
```

### InputCompat

The input layer. Every kit component reads keyboard, mouse and gamepad through it, never through
`UnityEngine.Input` directly. It uses the Input System when the package is installed and enabled in **Active
Input Handling** (*New* or *Both*), and the classic Input Manager otherwise. Keys are still configured as
`KeyCode` and translated.

| Member | Description |
|---|---|
| `static bool GetKey(KeyCode key)` / `GetKeyDown` / `GetKeyUp` | Like the classic `Input` methods. `Mouse0`–`Mouse4` map to mouse buttons. |
| `static Vector2 MousePosition { get; }` | Pointer position in screen pixels. |
| `static Vector2 MoveAxis { get; }` | WASD/arrows, or the left stick when no key is pressed (clamped to length 1). With the classic Input Manager: the `Horizontal`/`Vertical` axes. |
| `static Vector2 AimStick { get; }` | The gamepad's right stick. Always zero with the classic Input Manager. |
| `static bool JumpHeld` / `JumpPressedThisFrame` / `JumpReleasedThisFrame { get; }` | Space or the gamepad's south button (A/Cross). With the classic Input Manager: the `Jump` button. |
| `enum PadButton` | `None`, `South`, `East`, `West`, `North`, `LeftShoulder`, `RightShoulder`, `LeftTrigger`, `RightTrigger`, `Start`, `Select`. |
| `static bool GetPadButton(PadButton button)` / `GetPadButtonDown` / `GetPadButtonUp` | Gamepad buttons, independent of the input package. Always false with the classic Input Manager. |

If neither backend is available, every method returns false or zero.

```csharp
if (InputCompat.GetKeyDown(KeyCode.Tab) || InputCompat.GetPadButtonDown(InputCompat.PadButton.North))
    ToggleMap();
```

### TagFilter

Tolerant tag comparison. `CompareTag` throws when a tag isn't defined in the Tag Manager. These methods don't,
and they also accept the tag of a collider's attached `Rigidbody2D`, so a collider on a child of the player
counts as the player.

| Member | Description |
|---|---|
| `static bool Matches(Component component, string tag)` | True only if `tag` isn't empty and the object (or its collider's Rigidbody2D) has it. |
| `static bool PassesOptional(Component component, string tag)` | An optional filter: an empty tag accepts any object. |

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (!TagFilter.Matches(other, "Player")) return;
    // ...
}
```

### PlayerLocator

Finds the player for components whose target is empty. `EnemyChase`, `EnemyFlee` and `EnemyShootOnSight` use it
so prefabs and enemies spawned at runtime don't need a scene reference. It looks for the GameObject tagged
`Player` with `GameObject.FindWithTag`, but at most once every half second, so calling it every frame from many
enemies stays cheap.

| Member | Description |
|---|---|
| `const string PlayerTag` | `"Player"`, the tag it looks for. |
| `static bool Resolve(ref Transform target, ref float nextSearchTime)` | Returns true if `target` already has a value. If it's empty and `nextSearchTime` has passed, searches for the tagged player, fills `target` when found and schedules the next search 0.5 s later. Keep one `nextSearchTime` field per component. |

```csharp
using GameplayKit.Core;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform target; // empty = the object tagged Player
    private float _nextSearch;

    private void Update()
    {
        if (!PlayerLocator.Resolve(ref target, ref _nextSearch)) return;
        float side = Mathf.Sign(target.position.x - transform.position.x);
        transform.localScale = new Vector3(side, 1f, 1f);
    }
}
```

### TriggerOccupants&lt;T&gt; and PlatformRiders

These two live in `GameplayKit.Environment` and are **internal**: they're implementation helpers for the kit's
own zones and platforms, and aren't callable from your assemblies. They're documented here because they
explain behaviour you can observe.

- **`TriggerOccupants<T>`** tracks who is inside a trigger using Enter/Exit, because `OnTriggerStay2D` stops
  when a body falls asleep (a player standing still). It counts colliders per occupant, so a character with
  several colliders doesn't leave early, and its snapshot drops destroyed objects. Members: `Enter(T)`,
  `Exit(T)` (true when the occupant has fully left) and `Snapshot()`. Used by `HazardZone`, `HealthPickup`,
  `WaterZone` and `WindZone2D`.
- **`PlatformRiders`** carries dynamic bodies standing on a kinematic platform. Parenting doesn't work for
  dynamic Rigidbody2D, so it moves each rider by the platform's displacement every physics step and wakes it
  up. A body counts as a rider when its collider's bottom is at or above the platform's top (with 0.1 units
  of tolerance). Members: `OnContact(Collision2D)`, `OnContactEnded(Collision2D)` and `Carry(Vector2 delta)`.
  Used by `MovingPlatform`, `Elevator` and `ConveyorBelt`.

To get the same behaviour in your own scripts, copy the pattern: track occupants in Enter/Exit and act from
`Update`/`FixedUpdate`.

## Enums

### CharacterAction

Buttons beyond move/jump/run/crouch/dash/interact. `KeyboardInputReader` binds each one to a key and a gamepad
button in **Action Bindings**.

| Value | Default key | Default pad button | Used by |
|---|---|---|---|
| `Attack` | J | West (X/Square) | `PlayerAttack` |
| `Special` | K | Right Shoulder | `PlayerAttack` (switch weapon with `WeaponInventorySlot`) |
| `Fly` | F | North | `PlayerFly` |
| `Roll` | Left Alt | East | `PlayerRollDodge` |
| `Blink` | C | Left Shoulder | `PlayerTeleportBlink` |

### MovementState

What the character is doing. It's held in `CharacterCore.Movement`, and any ability can read or change it.
`CharacterAnimatorBridge` writes it as an integer to the **MovementState** Animator parameter, in this order:

`Idle` (0), `Walking` (1), `Running` (2), `Jumping` (3), `Falling` (4), `Dashing` (5), `WallSliding` (6),
`Crouching` (7), `Swimming` (8).

### ConditionState

The character's general condition, independent of movement. It's held in `CharacterCore.Condition`.

| Value | Set by | Effect |
|---|---|---|
| `Normal` | Start, `CharacterRespawn` (via `ResetCharacter`), the end of a stun | — |
| `Stunned` | `CharacterStun` | Abilities suspended. `PlayerWalkRun` also ignores input in this state. |
| `Dead` | `CharacterDeath` | Abilities suspended until revived. `CharacterAnimatorBridge` sets **Dead**. |
| `Paused` | Your code | `CharacterCore` skips all abilities while paused. Nothing in the kit sets it: `PauseManager` uses `Time.timeScale` instead. |

## CharacterStateMachine&lt;TState&gt;

A small generic state machine (`where TState : Enum`). `CharacterCore` keeps one for `MovementState`
(`Movement`) and one for `ConditionState` (`Condition`).

| Member | Description |
|---|---|
| `CharacterStateMachine(TState initialState)` | Starts with `CurrentState` and `PreviousState` set to `initialState`. |
| `TState CurrentState { get; }` | The current state. |
| `TState PreviousState { get; }` | The state before the last change. |
| `event Action<TState, TState> OnStateChanged` | Raised with `(previous, current)` on every change. |
| `void ChangeState(TState newState)` | Switches state. Changing to the current state does nothing and raises no event. |

```csharp
using GameplayKit.Core;
using UnityEngine;

/// Shows a "dizzy" icon while the character is stunned.
public class StunIcon : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    private CharacterCore _core;

    private void Start()
    {
        _core = GetComponent<CharacterCore>();
        _core.Condition.OnStateChanged += HandleChanged;
    }

    private void OnDestroy()
    {
        if (_core != null) _core.Condition.OnStateChanged -= HandleChanged;
    }

    private void HandleChanged(ConditionState previous, ConditionState current) =>
        icon.SetActive(current == ConditionState.Stunned);
}
```

Subscribe in `Start` or later: the state machines are created in `CharacterCore.Awake`. Several abilities set
`Movement` every frame (for example `PlayerWalkRun` writes `Idle`/`Walking`/`Running` even in mid-air while
`PlayerJump` writes `Falling`), so `Movement.OnStateChanged` can fire often. Prefer reading the controller
(`IsGrounded`, `Velocity`) for things like landing effects.

## AIState and AITransition

Serializable classes in `GameplayKit.AI` that make up an `AIBrain`'s **States** list. You normally edit them in
the Inspector. See [Enemies and AI](../guides/enemies-and-ai.md#aibrain-states-actions-and-decisions).

**`AIState`**

| Field | Description |
|---|---|
| `string name` | The state's name. Transitions and `AIBrain.TransitionTo` refer to it (case-sensitive). |
| `List<AIActionBase> actions` | Actions whose `PerformAction` runs every frame while the state is active, in order. Disabled action components are skipped. |
| `List<AITransition> transitions` | Checked in order after the actions. The first one that leads to a different state wins. |

**`AITransition`**

| Field | Description |
|---|---|
| `AIDecisionBase decision` | Evaluated every frame. Transitions with no decision, or whose decision component is disabled, are skipped. |
| `string trueTargetState` | State to go to when the decision returns true. Empty means stay. |
| `string falseTargetState` | State to go to when it returns false. Empty means stay. |

`AIBrain.states` is a private serialized field, so a brain's states are set in the Inspector, not from code.
The kit's tests fill it by reflection with `TestWorld.Set(brain, "states", ...)`.
