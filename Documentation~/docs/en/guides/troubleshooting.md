---
title: Troubleshooting
description: Answers to the most common Gameplay Kit setup problems.
---

# Troubleshooting

Most problems come down to a missing component, a collider of the wrong kind, or a project setting. Start with
the Console: kit messages are prefixed with `[GameplayKit]` (some messages are still in Spanish).

## The character falls through the floor

- **The floor needs a 2D collider.** `BoxCollider2D`, `TilemapCollider2D`, `CompositeCollider2D`,
  `EdgeCollider2D`... A 3D `BoxCollider` does nothing in 2D physics.
- **The floor must not be a trigger.** Triggers never stop bodies, and they never count as ground.
- **Check the Layer Collision Matrix** (**Project Settings → Physics 2D**). If you moved the player or the
  ground to custom layers, those layers have to collide.
- **Fast falls through thin platforms**: set the player's `Rigidbody2D` **Collision Detection** to
  *Continuous*. **GameplayKit → Create Player** already does.
- The `Rigidbody2D` must be **Dynamic** and **Simulated**. `CharacterController2D` adds one automatically if
  it's missing.

## The character doesn't detect the ground (can't jump)

`CharacterController2D.IsGrounded` comes from a small circle (**Ground Check Radius** 0.1) at the bottom
centre of the character's collider, checked every physics step against **Ground Layers** (*Everything* by
default). Select the character in Play mode: the gizmo is green when grounded and red when not.

- **The collider must be on the same GameObject as `CharacterController2D`.** It looks for the collider with
  `GetComponent`, not in children. If your collider lives on a child, assign a **Ground Check** transform at
  the feet instead. Without either, it logs a warning saying it needs a Collider2D or a Ground Check.
- If you assigned **Ground Check**, make sure it sits at the feet, not at the pivot in the middle of the body.
- If you narrowed **Ground Layers**, every walkable surface (platforms, one-way platforms, moving platforms)
  must be on one of those layers.
- Trigger colliders never count as ground. Colliders belonging to the character itself (including children
  under its Rigidbody2D) are ignored, so you don't need a "Player" layer.

## Abilities don't do anything

- **Is there a `CharacterCore`?** Abilities are plain components: nothing calls them without a core on the
  same GameObject (or a parent of the object they're on).
- **Did you build the character from code?** The core discovers abilities once, in its `Awake`. Add it last,
  or create the GameObject inactive, add everything and activate it at the end.
- **Look for `[GameplayKit] ... se desactivó` in the Console.** An ability that throws an exception is
  disabled (`AbilityEnabled = false`) so it can't break the others. Fix the cause (usually a missing reference
  in the Inspector) and re-enter Play mode.
- **The character might be suspended.** While dead, stunned or knocked back, `CharacterCore` runs no
  abilities at all. Check `CharacterCore.AbilitiesSuspended` and `Condition.CurrentState`.
- **Is the ability ticked?** `CharacterCore` skips abilities whose component is unticked in the Inspector, as
  well as those with `AbilityEnabled = false`. Ticking an ability again doesn't revive one the core disabled
  after an exception: its `AbilityEnabled` stays false until you re-enter Play mode.
- **Two abilities fight over the same value.** Within a phase, abilities run in Inspector order, and the
  later one wins. For example, `PlayerCrouch` must come after `PlayerWalkRun`. Don't combine abilities meant
  as alternatives: `PlayerJump` *or* `PlayerMultiJump`, `PlayerWallSlide` *or* `PlayerWallCling`,
  `PlayerTopDownMovement` *instead of* `PlayerWalkRun` + jumps.
- **Is the right input source used?** `CharacterCore` only adds `KeyboardInputReader` when no component
  implements `ICharacterInput`. If you added your own input component, the keyboard reader isn't there, and
  your component has to provide everything.

## Input doesn't work

The kit reads all input through `InputCompat`, which picks a backend from **Project Settings → Player → Other
Settings → Active Input Handling**:

| Active Input Handling | What the kit uses |
|---|---|
| *Input System Package (New)* | The Input System. The package must be installed, and the kit detects it automatically (version 1.0.0 or newer). |
| *Input Manager (Old)* | The classic Input Manager: the `Horizontal`/`Vertical` axes and the `Jump` button, plus `KeyCode`s. Gamepad buttons bound as `PadButton` and the right stick (aiming) **don't work** in this mode. |
| *Both* | The Input System. |

Other things to check:

- **The Game view needs focus** for keyboard input.
- **Gamepad attack, run, dash and pause need the Input System.** Crouch has no gamepad binding by default.
- **Pause doesn't react right after pressing Play.** `PauseManager` ignores its key for the first second and
  until it has seen the key released. That's intentional.
- **Errors from `StandaloneInputModule`** with the new Input System: your `EventSystem` uses the old module.
  Replace it with *Input System UI Input Module*. **GameplayKit → Create HUD** picks the right one
  automatically.
- **Action keys** (Attack J, Special K, Fly F, Roll Left Alt, Blink C) and their gamepad buttons are set in
  `KeyboardInputReader` → **Action Bindings**. See [Input](input.md).

## "Missing Script" on a kit or custom component

Unity can only save a `MonoBehaviour` in scenes and prefabs when it lives in a file with **exactly the same
name** as the class. A class in a differently named file still works with `AddComponent` from code, but shows
up as *Missing Script* once saved. Every kit component follows this rule: the editor test
`EveryComponent_HasItsOwnScriptFile` checks it. Apply the same rule to your own abilities, actions and
decisions.

Other causes: the package was removed or its path in `Packages/manifest.json` changed (all kit components
go missing at once), or a script has compile errors (Unity can't load *any* script until they're fixed).

## Enemies walk off ledges

Only [EnemyPatrol](../components/ai/EnemyPatrol.md) checks for ledges: it casts **Edge Check Distance** (0.5)
down from just past its front corner, and turns around when there's no ground. None of the others do:
`EnemyPatrolWithinBounds`, `EnemyChase`, `EnemyFlee`, `AIActionPatrolPoints`, `AIActionMoveToTarget` and
`AIActionFlee` all walk wherever their target or bounds are. To keep them on a platform:

- Put the bounds or waypoints inside the platform.
- Block the edges with walls, or put a lethal [HazardZone](../components/environment/HazardZone.md) below.
- Use `EnemyPatrol` where ledge-awareness matters, or write a custom decision that casts down in front of the
  enemy (see [Enemies and AI](enemies-and-ai.md#writing-your-own-actions-and-decisions)).

If `EnemyPatrol` keeps turning on the spot, its edge or wall ray is hitting something unexpected. Check
**Ground Layers**/**Obstacle Layers**, and make sure the enemy's own collider isn't a trigger and that the
enemy isn't floating above the floor.

## The camera jitters

- Set the player's `Rigidbody2D` **Interpolate** to *Interpolate*. Physics runs at a fixed rate and the camera
  every frame, so without interpolation the player appears to stutter. *Create Player* already sets it.
- Don't parent the camera to the player when it has `CameraFollow`. `CameraFollow` already follows it smoothly
  in `LateUpdate`.
- Leave the execution order alone: `CameraFollow` moves, `CameraBounds` (order 50) clamps and `CameraShake`
  (order 100) adds its offset last. Moving the camera from another `LateUpdate` script can fight with them.
- If the camera slides back and forth at a level edge, the bounds area is smaller than the view on that
  axis. `CameraBounds` then centres the camera, so check that **Min Bounds**/**Max Bounds** are what you
  meant.

## Layers and tags

The kit is designed to work on a fresh project with no layers or tags added:

- **Layer masks default to *Everything*.** Ground, wall and sight checks go through `PhysicsQuery2D`, which
  ignores the asking character's own colliders, so you don't need a "Player" layer to keep a character from
  detecting itself.
- **The player must be tagged `Player`** (a built-in tag). These all look for it: `Checkpoint`, `Collectible`,
  `LevelExit`, `FallingPlatform`, `Teleporter`, `PushableBox`, `EnemyMeleeOnContact`, and the auto-targeting in
  `CameraFollow`, `CameraZoomBySpeed`, `UIHealthBar`, `EnemyChase`, `EnemyFlee`, `EnemyShootOnSight` and
  `EnemySpawner`. *Create Player* sets the tag.
- **Optional tags don't need to exist.** Ladders, water and rope anchors are recognised by marker components
  (`LadderZone`, `WaterZone`, `RopeAnchor`). The tags `Ladder`, `Water` and `RopeAnchor` are an optional
  alternative. Tag comparisons go through `TagFilter`, which doesn't throw when a tag isn't defined in the
  Tag Manager.

## Other common questions

**The player slides down slopes when standing still.** The controller gives the collider a frictionless
material (so the character doesn't stick to walls). Add [PlayerSlopeWalk](../components/movement/PlayerSlopeWalk.md).

**The character sticks to walls.** You probably assigned a physics material with friction to the collider or
Rigidbody2D. The kit only adds its frictionless material when neither has one.

**Restart in the pause menu does nothing in a build.** The scene must be in the build list (**File → Build
Profiles**). In the editor, `UIPauseMenu` can reload it even when it isn't.

**`LevelExit` logs "no hay escena siguiente".** It has no **Scene Name** and the current scene is the last one
in the build list (or isn't in it). Set **Scene Name** or add the next scene to the list.

**Managers or the score vanish after loading a scene.** A persistent manager (`GameManager`, `AudioManager`,
`SaveLoadSystem`, `SceneTransitionManager`) shares a GameObject with other components, and its duplicate
destroyed the whole object. See [Building a level](building-a-level.md#managers).

## Tests don't show up in the Test Runner

Package tests are only compiled when the package is listed as testable. Add this to your project's
`Packages/manifest.json`, next to `dependencies`:

```json
"testables": ["com.alejocastellanos.gameplaykit"]
```

Then open **Window → General → Test Runner**. The catalogue tests are under **EditMode**, the gameplay tests
under **PlayMode**. See [Testing](../reference/testing.md).
