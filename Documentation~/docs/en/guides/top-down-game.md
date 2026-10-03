---
title: Top-down game
description: Start a top-down game with Gameplay Kit — the demo dungeon, the controls and your first room in five minutes.
---

# Top-down game

A top-down game is seen from above and has no gravity: the character moves freely in eight directions,
aims with the mouse or the stick, and fights enemies that chase it around the map — action RPGs,
twin-stick shooters, dungeon crawlers. This page is the starting point for that style: open the demo,
learn the controls and build your first room from scratch. If you haven't installed the package yet,
start with [Getting started](getting-started.md).

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/TopDownShooter.mp4" poster="../../assets/clips/TopDownShooter.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Aiming in 360° and shooting chasers that move on both axes while a turret fires back; at the end, switching to the sword with K.</figcaption></figure>

## The demo dungeon

**GameplayKit → Create Top-Down Demo Scene** (in the main menu bar) builds and saves a three-room
dungeon at `Assets/GameplayKitDemo/GameplayKitTopDownDemo.unity`, with the `TopDownPlayer`,
`TopDownChaser`, `TopDownTurret`, `TopDownBullet` and `TopDownPlayerBullet` prefabs in
`Assets/GameplayKitDemo/Prefabs/`. It asks you to save the current scene first and never touches the
platformer demo. Open it and press Play.

<figure markdown>
  ![Map of the top-down demo dungeon](../../assets/images/topdown-demo-map.jpg)
  <figcaption>The whole demo dungeon: the start room (left), the spike corridor, the arena (right) and the treasure room behind the locked door (top).</figcaption>
</figure>

| Area | What's there | Components |
|---|---|---|
| Start room (left) | The player starts here. Four crates that break under shots or sword hits, coins, a heart and a teleporter (bottom left) to the treasure room. | [BreakableObject](../components/environment/BreakableObject.md), [Collectible](../components/environment/Collectible.md), [HealthPickup](../components/environment/HealthPickup.md), [Teleporter](../components/environment/Teleporter.md) |
| Corridor | A strip of spikes across its whole width and, at the end, the checkpoint. | [HazardZone](../components/environment/HazardZone.md), [Checkpoint](../components/environment/Checkpoint.md) |
| Arena (right) | Two enemies that chase you, a turret that fires in any direction, a patroller going back and forth along the bottom, two pillars for cover, coins and the key (bottom right). | [EnemyChase](../components/ai/EnemyChase.md), [EnemyShootOnSight](../components/ai/EnemyShootOnSight.md), [EnemyPatrolWithinBounds](../components/ai/EnemyPatrolWithinBounds.md), [ItemPickup](../components/environment/ItemPickup.md) |
| Door (top of the arena) | Opens if you carry the key. | [DoorWithKey](../components/environment/DoorWithKey.md) |
| Treasure room (top) | A lever (++e++) whose event turns off a gate; behind it, the goal (a collectible worth 100 points) and two coins. A teleporter takes you back to the start room. | [Lever](../components/environment/Lever.md), `Collectible`, `Teleporter` |

The rest of the scene:

- **TopDownPlayer**: the **Create Top-Down Player** character with two weapons in a
  [WeaponInventorySlot](../components/combat/WeaponInventorySlot.md) — a `Gun` child with
  [WeaponProjectile](../components/combat/WeaponProjectile.md) and a `Sword` child with
  [WeaponMelee](../components/combat/WeaponMelee.md) — and
  [CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) in **Mouse** mode, which turns
  the gun towards the cursor. How to build it is in
  [Top-down character](top-down-character.md#shooting-towards-the-mouse).
- **Main Camera**: orthographic size 7, with [CameraFollow](../components/camera/CameraFollow.md),
  [CameraShake](../components/camera/CameraShake.md) and [CameraBounds](../components/camera/CameraBounds.md)
  clamped to the dungeon, from (−11, −9) to (44, 20).
- **Managers** and **HUD**, the same as in the platformer demo.

As in the platformer demo, the goal is a `Collectible` rather than a `LevelExit`, so the demo never
loads another scene.

## Controls

| Action | Keyboard and mouse | Gamepad (Input System) |
|---|---|---|
| Move (8 directions) | ++w++ ++a++ ++s++ ++d++ or arrows | Left stick |
| Run (hold) | Left ++shift++ | Left trigger |
| 8-direction dash | ++q++ | Right trigger |
| Aim | Mouse (with **CharacterAimAndOrient** in **Mouse** mode) | Right stick (**Stick** mode) |
| Attack / shoot | ++j++ — left click too in the demo | West button (X / Square) |
| Switch weapon | ++k++ | Right shoulder |
| Interact | ++e++ | Select / View |
| Pause | ++esc++ | Start |

Jump, crouch and the other platformer keys do nothing on a top-down character. Left click isn't bound by
default: the demo adds it as an extra **Attack** binding (`Mouse0`) on its
[KeyboardInputReader](../components/core/KeyboardInputReader.md), and you can do the same.

## Your first room in five minutes

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerTopDownMovement.mp4" poster="../../assets/clips/PlayerTopDownMovement.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Free movement in eight directions with smooth acceleration.</figcaption></figure>

1. Create a new scene and delete its default **Main Camera** (the kit's camera replaces it).
2. **GameplayKit → Create Top-Down Player**.
3. **GameplayKit → Create 2D Camera**, **Create Managers** and **Create HUD**.
4. With the player near (0, 0), close a room with four **GameplayKit → Create Platform**: two with
   **Scale** X `5` above and below the player (for example at y = 5 and y = −5), and two with
   **Rotation** Z `90` and **Scale** X `3` on the sides (x = 10 and x = −10). In a top-down game, a
   platform is just a wall.
5. Press Play. Move in eight directions with ++w++ ++a++ ++s++ ++d++, run with ++shift++ and dash in
   any direction with ++q++.
6. Add an enemy with **GameplayKit → Create Top-Down Enemy** inside the room, about 5 units from the
   player. It chases you when you're within 7 units and hurts you on contact. Hit it three times with
   ++j++ to destroy it (it has 30 HP and the default melee deals 10). The hit goes left or right,
   depending on which way the character faces.

Nothing needed a layer, a tag or a reference, and you didn't touch **Gravity Scale**:
[PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md) cancels the character's gravity
on its own, and the top-down enemy already comes with gravity `0`.

To shoot towards the mouse like the demo does, follow
[Shooting towards the mouse](top-down-character.md#shooting-towards-the-mouse), or drag the demo's
`TopDownPlayer` prefab into the scene instead of step 2.

## Next steps

- [Top-down character](top-down-character.md) — movement, dash, four-way attacks, shooting towards the mouse and the camera.
- [Enemies and AI](enemies-and-ai.md) — chasers, turrets, patrollers and state machines.
- [Building a level](building-a-level.md) — managers, environment pieces, checkpoints, doors and the HUD.
- [Combat](combat.md) and [Health and damage](health-and-damage.md) — weapons, aiming, death and respawn.
