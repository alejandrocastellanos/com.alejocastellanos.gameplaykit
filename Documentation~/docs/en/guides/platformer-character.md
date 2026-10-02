# Platformer character

**GameplayKit → Create Player** gives you a character with almost every movement ability already
attached. That's great for trying things out, but for your own game you'll usually want only the
mechanics it needs. This guide builds a platformer character by hand, explains how the pieces fit
together, and shows how to tune the jump until it feels right.

## Build it step by step

1. **Create an empty GameObject** named `Player` and set its tag to **Player**. The tag isn't needed
   for movement, but the camera, the HUD, checkpoints and enemies look for it by default.
2. **Add a `CapsuleCollider2D`** and set its **Size** to about `0.8 × 1.8`. A capsule slides over
   small steps and corners better than a box. Add your `SpriteRenderer` here or on a child.
3. **Add [CharacterController2D](../components/core/CharacterController2D.md).** It adds a
   `Rigidbody2D` automatically and, on Awake, freezes its rotation and gives the collider a
   frictionless material (if it has none) so the character doesn't stick to walls. Setting the
   Rigidbody2D's **Interpolate** to *Interpolate* and **Collision Detection** to *Continuous* (what
   Create Player does) gives smoother motion at high speeds.
4. **Add the abilities** you want, in the order you want them to run — see the next sections. At
   minimum: [PlayerWalkRun](../components/movement/PlayerWalkRun.md) and
   [PlayerJump](../components/movement/PlayerJump.md).
5. **Add [CharacterCore](../components/core/CharacterCore.md) last.** On Awake it collects every
   ability on the object and its children and runs them each frame. If there is no input source, it
   adds a [KeyboardInputReader](../components/core/KeyboardInputReader.md) by itself.

That's a playable character. For health, damage and respawning, add the components described in
[Health and damage](health-and-damage.md).

!!! tip "Why CharacterCore goes last"
    In the Editor, all components already exist when the scene starts, so `CharacterCore` finds
    every ability no matter where it sits. The order still matters in two cases: abilities run in
    the order they appear in the Inspector, and when you build a character **from code**,
    `AddComponent<CharacterCore>()` runs its Awake immediately — anything added afterwards is never
    discovered. Adding it last is the habit that works in both cases.

### How abilities run

Every frame, `CharacterCore` runs four phases — `HandleInput`, `EarlyProcessAbility`,
`ProcessAbility`, `LateProcessAbility` — and each phase runs for **all** abilities before the next
one starts. Abilities never call each other; they share the
[CharacterController2D](../components/core/CharacterController2D.md) (velocity, ground state,
gravity) and the character's movement and condition states.

- While the character is dead, stunned or knocked back, those systems call `Suspend` and no
  ability runs until they `Resume`.
- If an ability throws an exception, only that ability is disabled (with an error in the Console
  naming it); the rest keep working.
- Abilities that need to cancel gravity (ladders, swimming, ledge grab, flying…) register an
  override on the controller and release it when they finish, so they never fight over
  `Rigidbody2D.gravityScale`.

Order matters for abilities that adjust what another one computed: put
[PlayerCrouch](../components/movement/PlayerCrouch.md) **after** `PlayerWalkRun` so its speed
multiplier applies to the speed that was just set.

## Combine abilities for a game feel

Some abilities are alternatives: use **either** `PlayerJump` or
[PlayerMultiJump](../components/movement/PlayerMultiJump.md), and **either**
[PlayerWallSlide](../components/movement/PlayerWallSlide.md) or
[PlayerWallCling](../components/movement/PlayerWallCling.md). Both wall variants need
[PlayerWallJump](../components/movement/PlayerWallJump.md) (it detects the wall) and add it
automatically.

=== "Classic"

    Tight, readable controls in the spirit of 8- and 16-bit platformers.

    - [PlayerWalkRun](../components/movement/PlayerWalkRun.md) — hold ++shift++ to run.
    - [PlayerJump](../components/movement/PlayerJump.md) — variable height, coyote time, jump buffer.
    - [CharacterGravityController](../components/health/CharacterGravityController.md) — falls
      faster than it rises, with a capped fall speed.
    - [PlayerCrouch](../components/movement/PlayerCrouch.md),
      [PlayerDropThrough](../components/movement/PlayerDropThrough.md),
      [PlayerClimbLadder](../components/movement/PlayerClimbLadder.md).

=== "Metroidvania"

    A growing move set; enable abilities as the player unlocks them.

    - `PlayerWalkRun`, [PlayerMultiJump](../components/movement/PlayerMultiJump.md) (start with
      **Extra Jumps** at `0` and raise it when the double jump is unlocked).
    - [PlayerDash](../components/movement/PlayerDash.md), `PlayerWallJump` + `PlayerWallSlide`.
    - [PlayerLedgeGrab](../components/movement/PlayerLedgeGrab.md) +
      [PlayerLedgeClimb](../components/movement/PlayerLedgeClimb.md),
      [PlayerCrouch](../components/movement/PlayerCrouch.md) +
      [PlayerCrawl](../components/movement/PlayerCrawl.md).
    - [PlayerGroundSlam](../components/movement/PlayerGroundSlam.md),
      [PlayerSwim](../components/movement/PlayerSwim.md), and
      [PlayerAttack](../components/combat/PlayerAttack.md) with a
      [WeaponCombo](../components/combat/WeaponCombo.md).

    To lock an ability until it's unlocked, set its `AbilityEnabled` to `false` from your
    progression script — `CharacterCore` skips disabled abilities.

=== "Precision (Celeste-like)"

    Fast, forgiving, wall-heavy movement.

    - `PlayerWalkRun`, `PlayerJump` (coyote time and buffer matter a lot here).
    - [PlayerDash8Directions](../components/movement/PlayerDash8Directions.md) — dashes in the
      direction you hold, horizontally if you hold nothing.
    - `PlayerWallJump` + [PlayerWallCling](../components/movement/PlayerWallCling.md) — hold towards
      the wall to stick to it.
    - `CharacterGravityController` with a lower **Max Fall Speed**.

    The kit's dashes recharge on a **Cooldown** timer (0.5 s by default), not on landing. If you
    want one dash per jump, gate it from your own ability.

## Tune the jump

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerJump.mp4" poster="../../assets/clips/PlayerJump.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>A tap gives a short hop; holding Jump gives the full height.</figcaption></figure>

Most of a platformer's feel lives in a handful of fields on
[PlayerJump](../components/movement/PlayerJump.md):

| Field | Default | What it does |
|---|---|---|
| **Jump Force** | `12` | Upward velocity at take-off. |
| **Jump Cut Multiplier** | `0.5` | When you release Jump while rising, vertical speed is multiplied by this. Lower = shorter taps. `1` disables variable height. |
| **Coyote Time** | `0.1` | Seconds after walking off a ledge during which you can still jump. |
| **Jump Buffer Time** | `0.12` | If Jump is pressed this long before landing, the jump fires on touchdown. |

- **Variable height.** Releasing the button early cuts the jump. A buffered jump whose button was
  already released by the time you land comes out as a short hop straight away.
- **Coyote time** makes ledges forgiving: the jump still counts for a moment after the ground
  disappears, and it is consumed by the jump so it can't give you a free double jump.
- **Jump height** with no extra gravity is roughly `Jump Force² / (2 × 9.81 × gravity scale)` —
  about 7 units for the defaults. Lower **Jump Force** for a smaller character, or raise gravity.

[PlayerMultiJump](../components/movement/PlayerMultiJump.md) has the same **Jump Force**, **Jump
Cut Multiplier** and **Jump Buffer Time**, plus **Extra Jumps** (`1`) and **Air Jump Force** (`10`)
for the jumps in the air. It has **no coyote time**: walking off a ledge and pressing Jump spends
one of the air jumps.

### Gravity shaping

[CharacterGravityController](../components/health/CharacterGravityController.md) changes gravity
while the character is airborne: ×2 when falling (**Fall Gravity Multiplier**), ×1.5 while rising
without holding Jump (**Low Jump Gravity Multiplier**), and a **Max Fall Speed** of 20. The
multipliers apply on top of the Rigidbody2D's **Gravity Scale**, which it keeps by default; set
**Base Gravity Scale** above `0` only if you want to override it from here. Falling faster than you rise is what makes a jump feel
snappy instead of floaty.

!!! warning "Two sources of jump cut"
    `PlayerJump`/`PlayerMultiJump` cut the jump on release, and `CharacterGravityController` adds
    extra gravity when Jump isn't held. Create Player uses both, so short hops are extra short. If
    taps feel too short, set **Jump Cut Multiplier** to `1` or **Low Jump Gravity Multiplier** to `1`
    so only one of them shapes the hop.

## Walls

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerWallJump.mp4" poster="../../assets/clips/PlayerWallJump.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Kicking off a wall with PlayerWallJump.</figcaption></figure>

[PlayerWallJump](../components/movement/PlayerWallJump.md) casts from the collider towards the side
the character faces. In the air, against a wall, **Jump** applies **Wall Jump Force** (`8, 12`) away
from it and ignores horizontal input for **Control Lock Time** (`0.2` s) so holding towards the wall
doesn't cancel the kick. `PlayerWallSlide` caps the fall at **Slide Speed** (`2`) while you push
towards the wall; `PlayerWallCling` stops the fall completely, and lets go on the jump frame and during
**Control Lock Time**, so wall jumping from a cling keeps the full height.

## Ground detection without setup

You don't need a "Ground" layer. `CharacterController2D` checks a small circle (**Ground Check
Radius** `0.1`) at the bottom of the character's collider every physics step:

- **Ground Layers** defaults to *Everything*. The check ignores the character's own colliders and
  all triggers, so ladders, water and pickups never count as ground.
- **Ground Check** is optional; assign a child Transform only if the bottom of your collider isn't
  where the feet are.
- Select the character in Play mode to see the probe as a gizmo: green when grounded, red in the air.

Any solid collider counts as ground, including enemies, so the player can land on them. If you
don't want that, put those objects on a layer and remove it from **Ground Layers**. Wall checks,
ledge checks and weapons follow the same rule, which is why every layer mask in the kit can stay on
*Everything*.

## Next steps

- [Input](input.md) — rebind keys or drive the character from AI.
- [Custom abilities](custom-abilities.md) — write your own `AbilityBase`.
- [Building a level](building-a-level.md) — platforms, ladders, water and hazards.
