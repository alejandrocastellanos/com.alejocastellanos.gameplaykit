# Top-down character

For games seen from above — action RPGs, twin-stick shooters, dungeon crawlers — the character
moves freely in two axes and gravity must not pull it down the screen.
[PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md) does exactly that and
replaces `PlayerWalkRun` and the jump abilities. Everything else in the kit (health, weapons,
interaction, camera) works the same as in a platformer.

## The quick way

**GameplayKit → Create Top-Down Player** builds a `Player (Top-Down)` object, tagged `Player`, with:

- a `CircleCollider2D` (radius `0.4`), a `Rigidbody2D` with interpolation and
  [CharacterController2D](../components/core/CharacterController2D.md);
- [KeyboardInputReader](../components/core/KeyboardInputReader.md),
  [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md),
  [PlayerDash8Directions](../components/movement/PlayerDash8Directions.md),
  [PlayerInteract](../components/environment/PlayerInteract.md),
  [PlayerAttack](../components/combat/PlayerAttack.md) and
  [WeaponMelee](../components/combat/WeaponMelee.md);
- [CharacterHealth](../components/health/CharacterHealth.md),
  [CharacterKnockback](../components/health/CharacterKnockback.md),
  [CharacterDeath](../components/health/CharacterDeath.md),
  [CharacterRespawn](../components/health/CharacterRespawn.md),
  [InventoryManager](../components/managers/InventoryManager.md),
  [DamageFlash](../components/health/DamageFlash.md),
  [CharacterAnimatorBridge](../components/core/CharacterAnimatorBridge.md) and, last,
  [CharacterCore](../components/core/CharacterCore.md).

To build it by hand, add the same components in that order. A circle (or a short capsule) is the
usual collider for top-down: it slides along walls and around corners without snagging.

## Movement and the gravity override

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerTopDownMovement.mp4" poster="../../assets/clips/PlayerTopDownMovement.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Free movement in eight directions with smooth acceleration.</figcaption></figure>

When it initializes, `PlayerTopDownMovement` registers a gravity override of `0` on the
`CharacterController2D`, and registers it again whenever the character is reset (for example on
respawn). You don't need to touch the Rigidbody2D's **Gravity Scale**.

| Field | Default | What it does |
|---|---|---|
| **Walk Speed** | `4` | Speed with the stick or WASD / arrows. |
| **Run Speed** | `6.5` | Speed while holding ++shift++ (or the left trigger). |
| **Acceleration** | `0.05` | Seconds to reach the target speed (smoothed). `0` = instant start and stop. |
| **Flip With Direction** | on | Mirrors the character on X when moving left or right. |

- Input is clamped to length 1, so moving diagonally isn't faster than moving straight.
- While the character is stunned, in knockback or dead, `CharacterCore` suspends its abilities, so
  `PlayerTopDownMovement` doesn't run: the `Rigidbody2D` keeps its last velocity and, with no gravity
  or ground friction, the character keeps sliding until abilities resume (respawning resets the
  velocity to zero). If you want it to stop, zero its velocity from your own script, for example on
  `OnCharacterDeath` from [CharacterDeath](../components/health/CharacterDeath.md).
- `LastMoveDirection` remembers the last direction you moved in (it starts as `Vector2.down`), which
  is handy for idle animations and facing.

!!! warning "Keep platformer abilities off a top-down character"
    Gravity overrides stack and the most recent one wins. For example,
    [PlayerSwim](../components/movement/PlayerSwim.md) registers a gravity of `0.2` while in water,
    which would make a top-down character drift down the screen, and
    [PlayerClimbLadder](../components/movement/PlayerClimbLadder.md) writes its own vertical
    speed. Don't combine `PlayerTopDownMovement` with `PlayerWalkRun`, the jump abilities,
    `CharacterGravityController` or the wall, ladder and swim abilities.

## 8-direction dash

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerDash8Directions.mp4" poster="../../assets/clips/PlayerDash8Directions.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Dashing in the direction of the input, including diagonals.</figcaption></figure>

[PlayerDash8Directions](../components/movement/PlayerDash8Directions.md) dashes along the current
movement input when you press ++q++ (or the right trigger): **Dash Speed** `16` for **Dash
Duration** `0.15` s, then a **Cooldown** of `0.5` s. With no input it dashes horizontally towards
the side the character faces — not along `LastMoveDirection`.

## Interact and attack

[PlayerInteract](../components/environment/PlayerInteract.md) works the same in both views: press
++e++ and the closest `IInteractable` within **Interact Radius** (`1.2`) is activated — levers, or
any script of yours that implements the interface. Triggers count, so interactables don't need a
solid collider.

[PlayerAttack](../components/combat/PlayerAttack.md) with [WeaponMelee](../components/combat/WeaponMelee.md)
attacks on ++j++. By default the melee hitbox is placed at **Hitbox Offset** `(0.75, 0)` mirrored by
the facing, so it only reaches **left or right**. For four- or eight-way melee, give the weapon a
**Hitbox Origin** child and move it with the last movement direction:

```csharp
using GameplayKit.Movement;
using UnityEngine;

// Put this on the player. Assign the same child Transform as WeaponMelee's Hitbox Origin.
public class HitboxFollowsMovement : MonoBehaviour
{
    [SerializeField] private PlayerTopDownMovement movement;
    [SerializeField] private Transform hitboxOrigin;
    [SerializeField] private float distance = 0.75f;

    private void LateUpdate()
    {
        Vector2 direction = movement.LastMoveDirection;
        // The character is mirrored with a negative scale X, so undo it for the local position.
        float facing = Mathf.Sign(transform.lossyScale.x);
        hitboxOrigin.localPosition = new Vector2(direction.x * facing, direction.y) * distance;
    }
}
```

For ranged combat, use [WeaponProjectile](../components/combat/WeaponProjectile.md) or
[WeaponHitscan](../components/combat/WeaponHitscan.md) together with
[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md): in **Mouse** mode the shots
go towards the cursor, in **Stick** mode towards the right stick, and in **Closest Target** mode
towards the nearest damageable object. That's a twin-stick shooter with no code. See
[Combat](combat.md).

## Camera setup

**GameplayKit → Create 2D Camera** creates an orthographic camera (size `6`) with
[CameraFollow](../components/camera/CameraFollow.md) and
[CameraShake](../components/camera/CameraShake.md). With its **Target** empty, `CameraFollow` finds
the object tagged `Player` — the top-down player is tagged — and snaps to it on the first frame,
then follows with **Smooth Time** `0.2` and **Offset** `(0, 0, -10)`.

- To keep the camera inside the map, add [CameraBounds](../components/camera/CameraBounds.md) and set
  **Min Bounds** / **Max Bounds** to the corners of your level. If you leave them empty, it uses the
  bounds of a [LevelManager](../components/managers/LevelManager.md) in the scene, if there is one.
  If the level is smaller than the view on an axis, the camera is centred on that axis.
- Top-down views usually look best with a slightly larger **Size** on the Camera (more of the map
  visible) and a smaller **Smooth Time** so fast dashes don't leave the player near the screen edge.
- [CameraZoomBySpeed](../components/camera/CameraZoomBySpeed.md) zooms out as the player moves
  faster. It takes over the camera's **Size**, between its **Min Zoom** (`5`) and **Max Zoom** (`8`).

## Enemies and the demo dungeon

Tick **Move Vertically** on [EnemyChase](../components/ai/EnemyChase.md) or
[EnemyFlee](../components/ai/EnemyFlee.md) and they move on both axes with no gravity. A static
[EnemyShootOnSight](../components/ai/EnemyShootOnSight.md) with **Sight Angle** 360 works as a turret that
fires in any direction, and [EnemyPatrolWithinBounds](../components/ai/EnemyPatrolWithinBounds.md) patrols a
corridor if its `Rigidbody2D` has Gravity Scale 0.

**GameplayKit → Create Top-Down Demo Scene** builds all of this into a three-room dungeon
(`Assets/GameplayKitDemo/GameplayKitTopDownDemo.unity`) so you can see how the pieces are wired: open it
and press Play. Controls: ++w++ ++a++ ++s++ ++d++ or arrows to move, ++shift++ to run, ++q++ to dash,
++j++ to attack, ++e++ to interact and ++esc++ to pause.

## Next steps

- [Input](input.md) — gamepad support and custom input sources.
- [Combat](combat.md) — weapons, aiming and damage.
- [Enemies and AI](enemies-and-ai.md) — enemies that chase and shoot.
