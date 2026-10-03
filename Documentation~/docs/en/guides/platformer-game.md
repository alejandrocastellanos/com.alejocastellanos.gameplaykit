---
title: Platformer game
description: Start a platformer with Gameplay Kit — the demo, the controls and your first level in five minutes.
---

# Platformer game

A platformer is seen from the side and has gravity: the character runs, jumps, clings to walls, climbs
ladders and swims. This page is the starting point for that style: open the demo, learn the controls
and build your first level from scratch. If you haven't installed the package yet, start with
[Getting started](getting-started.md).

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/Showcase.mp4" poster="../../assets/clips/Showcase.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>A run through the demo level: platforms, an enemy, spikes, a checkpoint, a ladder, a key, water, a conveyor, a lever-driven elevator and the locked door.</figcaption></figure>

## The platformer demo

**GameplayKit → Create Demo Scene** (in the main menu bar) builds and saves a short level at
`Assets/GameplayKitDemo/GameplayKitDemo.unity` (a numbered name if one already exists), with the `Player`
and `Enemy` prefabs in `Assets/GameplayKitDemo/Prefabs/`. It asks you to save the current scene first.
Open it and press Play. Everything is made from the same builders as the other **GameplayKit** menu
items, so it's a good reference for wiring things up.

The level reads left to right (ground top at y = 0 unless noted):

| x | What's there | Components |
|---|---|---|
| −10.5 | A tall wall closes the left side. | Plain `BoxCollider2D` |
| 0 | The player starts here. Two coins sit ahead. | Player prefab, [Collectible](../components/environment/Collectible.md) |
| 6 | A thin platform you can jump through from below, with a coin on top. | [OneWayPlatform](../components/environment/OneWayPlatform.md) |
| 9 | A crate that breaks with one hit (**J**). | [BreakableObject](../components/environment/BreakableObject.md) |
| 12.5 – 19.5 | Two low walls with an enemy patrolling between them. | Enemy prefab ([EnemyPatrol](../components/ai/EnemyPatrol.md), contact damage) |
| 20 – 28 | A gap. A platform shuttles 5 units to the right and back, with spikes below. | [MovingPlatform](../components/environment/MovingPlatform.md) (ping-pong), [HazardZone](../components/environment/HazardZone.md) |
| 28.8 | A heart to recover what the spikes took. | [HealthPickup](../components/environment/HealthPickup.md) |
| 30 | The checkpoint. | [Checkpoint](../components/environment/Checkpoint.md) |
| 33.4 | A ladder up to a tower (top at y = 4) with a coin and the key at x = 42. | [LadderZone](../components/movement/LadderZone.md), [ItemPickup](../components/environment/ItemPickup.md) |
| 44 – 52 | A pool, two units deep. | [WaterZone](../components/environment/WaterZone.md) |
| 56 | A conveyor belt pushing right. | [ConveyorBelt](../components/environment/ConveyorBelt.md) |
| 60 – 63 | A lever (**E**) whose **On Toggled** event calls `Elevator.Activate`. The elevator rises 6 units. | [Lever](../components/environment/Lever.md), [Elevator](../components/environment/Elevator.md) |
| 65 – 75, y = 6 | The upper floor: a locked door at x = 70 and the goal, a collectible worth 100 points. | [DoorWithKey](../components/environment/DoorWithKey.md), `Collectible` |
| under everything | An invisible 120-unit-wide trigger at y = −14 that deals 9999 damage. | `HazardZone` |

The rest of the scene:

- **Main Camera**: orthographic size 6, with [CameraFollow](../components/camera/CameraFollow.md) on the
  player, [CameraShake](../components/camera/CameraShake.md) and [CameraBounds](../components/camera/CameraBounds.md)
  clamped to (−11, −12)–(80, 20).
- **Managers**: [ScoreManager](../components/managers/ScoreManager.md) and [PauseManager](../components/managers/PauseManager.md).
- **HUD**: a health bar, the score and a pause menu, plus an `EventSystem`.

Two things are worth noticing. Falling off the level doesn't need a `LevelManager`: the kill zone is just a
`HazardZone` strong enough to kill, and the player's `CharacterRespawn` brings it back to the last checkpoint.
And the goal is a `Collectible`, not a `LevelExit`, so the demo never loads another scene.

## Controls

| Action | Keyboard | Gamepad (Input System) |
|---|---|---|
| Move | ++a++ / ++d++ or ++arrow-left++ / ++arrow-right++ | Left stick |
| Up / down (ladders, swimming, flying) | ++w++ / ++s++ or ++arrow-up++ / ++arrow-down++ | Left stick |
| Jump | ++space++ | South button (A / Cross) |
| Run (hold) | Left ++shift++ | Left trigger |
| Crouch (hold) | Left ++ctrl++ | — |
| Dash | ++q++ | Right trigger |
| Interact | ++e++ | Select / View |
| Attack | ++j++ | West button (X / Square) |
| Special (switch weapon) | ++k++ | Right shoulder |
| Fly (toggle) | ++f++ | North button (Y / Triangle) |
| Roll | Left ++alt++ | East button (B / Circle) |
| Blink | ++c++ | Left shoulder |
| Pause | ++esc++ | Start |

A few combinations: ++s++ + ++space++ on a one-way platform drops through it, and ++s++ + ++q++ in
the air performs a ground slam (if the character has
[PlayerGroundSlam](../components/movement/PlayerGroundSlam.md)). Fly, roll and blink only work if you
add those abilities. The five actions (Attack, Special, Fly, Roll, Blink) can be rebound in
[KeyboardInputReader](../components/core/KeyboardInputReader.md).

## Your first level in five minutes

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerWalkRun.mp4" poster="../../assets/clips/PlayerWalkRun.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Walking, then holding Shift to run — the first thing you'll try after Create Player.</figcaption></figure>

1. Create a new scene and delete its default **Main Camera** (the kit's camera replaces it).
2. **GameplayKit → Create Platform**. Scale it on X (for example to 8) to get a wider floor.
3. Move the Scene view above the platform and run **GameplayKit → Create Player**.
4. **GameplayKit → Create 2D Camera**, **Create Managers** and **Create HUD**.
5. Press Play. Walk, run with ++shift++, double jump, dash with ++q++, and jump against the side of
   the platform to wall-slide and wall-jump.
6. Add an enemy with **GameplayKit → Create Enemy** on the platform. It patrols and hurts you on
   contact; hit it twice with ++j++ to destroy it (it has 20 HP and the default melee deals 10).

Nothing needed a layer, a tag or a reference: ground detection, wall checks and weapons ignore the
character's own colliders, so every mask can stay on *Everything*.

Now tweak something. Select the Player and set **Extra Jumps** on
[PlayerMultiJump](../components/movement/PlayerMultiJump.md) to `2` for a triple jump, or remove
[PlayerDash](../components/movement/PlayerDash.md) to take the dash away. Each ability is its own
component, so adding or removing a mechanic is just **Add Component** / **Remove Component**.

## Next steps

- [Platformer character](platformer-character.md) — build a character by hand, combine abilities and tune the jump.
- [Building a level](building-a-level.md) — managers, camera, environment pieces, checkpoints, doors and the HUD.
- [Combat](combat.md), [Health and damage](health-and-damage.md) and [Enemies and AI](enemies-and-ai.md) — the fights.
