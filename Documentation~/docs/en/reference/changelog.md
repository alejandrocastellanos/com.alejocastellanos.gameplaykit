---
title: Changelog
description: Notable changes to the Gameplay Kit package.
---

# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package uses
[Semantic Versioning](https://semver.org/).

## 1.1.0 — 2026-10-02

### Added

- Top-down demo scene (**GameplayKit → Create Top-Down Demo Scene**, also in the Demo sample) and a **Move Vertically** option on `EnemyChase` and `EnemyFlee` for top-down enemies.
- Documentation site (MkDocs Material, English and Spanish) in `Documentation~/`, published to GitHub Pages by `.github/workflows/docs.yml`, with an animated clip for every component.
- `PlayerLocator` helper and public `Target` properties on `EnemyChase`, `EnemyFlee` and `EnemyShootOnSight`.

### Fixed

- `CharacterCore` now skips abilities whose component is unticked in the Inspector (not only `AbilityEnabled = false`), and `AIBrain` skips disabled action and decision components.
- `PlayerRopeGrab` is now a real pendulum: gravity makes it swing, horizontal input pushes in the pressed direction, new **Swing Damping** field (0.25), and momentum is kept when grabbing and when letting go with Jump.
- `PlayerWallCling` no longer cancels the wall jump's height: it doesn't cling on the jump frame or during the wall jump's **Control Lock Time**.
- `EnemyChase`, `EnemyFlee` and `EnemyShootOnSight` find the `Player`-tagged object when **Target** is empty (new `PlayerLocator` helper) and expose a public `Target` property; `EnemySpawner` now assigns its target to them too, not only to `AIBrain`.
- `PauseManager.SetPaused` keeps `GameManager` in sync (`Playing` ↔ `Paused`, never over `GameOver`), and `GameManager`, `AudioManager` and `SaveLoadSystem` detach from their parent before `DontDestroyOnLoad`.
- `CharacterKnockback` no longer pushes with the damage amount: new **Force** (8 u/s, independent of damage and mass) and **Upward Lift** (0.35) fields, plus an `ApplyKnockback(direction, speed)` overload for an explicit speed.
- `CharacterGravityController` **Base Gravity Scale** now defaults to 0, which keeps the `Rigidbody2D` **Gravity Scale**; only a value above 0 replaces it.

## 1.0.0 — 2026-10-02

First public release.

### Added

- **Core:** `CharacterCore` + `AbilityBase` (independent abilities, isolated when they fail, with
  `Suspend`/`Resume`), `CharacterController2D` (zero-config ground detection, shared gravity, horizontal
  control lock), `CharacterStateMachine`, `ICharacterInput` with `KeyboardInputReader` (new Input System,
  classic Input Manager and gamepad), `CharacterAnimatorBridge`, and the contracts `IDamageable`,
  `IHealthSource`, `IKeyHolder` and `IInteractable`.
- **Movement:** walk/run, jump with coyote time and jump buffer, multi-jump, crouch, crawl, wall
  jump/slide/cling, dash and 8-direction dash, ground slam, ledges (grab, climb, dangle), ladders, ropes,
  ziplines, glide, jetpack, fly, swim, slopes, roll, blink, path following, dropping through one-way
  platforms, and top-down movement.
- **Health & physics:** health with invulnerability, configurable gravity, fall damage, stun, knockback,
  death, respawn, lives with game over, persistence between scenes and `DamageFlash`.
- **Combat:** melee, projectiles, hitscan, combos, charged attacks, a weapon inventory, aiming with the mouse,
  the stick or at the closest target, damageable objects, and `PlayerAttack` connecting input to weapons.
- **AI:** ready-to-use behaviours (patrol, bounded patrol, chase, flee, shoot on sight, contact damage),
  `EnemySpawner`, and the `AIBrain` state machine with actions and decisions assembled in the Inspector.
- **Environment:** moving, one-way and falling platforms, elevator, spring, pressure plate, lever, door with
  key, breakable objects, pushable box, checkpoint, teleporter, conveyor belt, hazard zones, wind and water,
  collectibles, item and health pickups, and a level exit.
- **Camera:** follow, bounds (its own or the `LevelManager`'s), shake and zoom by speed.
- **Managers:** game, level (death below the void, checkpoints), score, inventory, pause, audio with
  crossfade, save/load, and scene transitions with an automatic fade.
- **UI:** health bar, damage numbers with `DamagePopupSpawner`, pause menu and score text.
- **Editor:** a *GameplayKit* menu that creates a Player (platformer and top-down), an Enemy, a platform, a 2D
  camera, managers, a HUD and a playable demo scene.
- **Sample** "Demo 2D", importable from the Package Manager.
- **Tests:** 88 play-mode tests covering the mechanics and 4 editor tests covering the component catalogue.

### Experimental

- `EnemyPathfindingAgent` requires an external 2D NavMesh (for example NavMeshPlus). Without a NavMesh it
  does nothing.
