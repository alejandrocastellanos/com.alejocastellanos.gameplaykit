---
title: Gameplay Kit
description: Modular 2D gameplay for Unity 6, assembled from Add Component.
hide:
  - navigation
  - toc
---

<div class="gk-hero" markdown>

<div markdown>

# Gameplay Kit

Modular 2D gameplay for Unity 6: movement, combat, health, enemy AI, interactive levels, camera, managers
and UI. Every piece is a component you drop on a GameObject. It works with its default values, so you don't
have to set up layers, tags or references first.

[Get started](guides/getting-started.md){ .md-button .md-button--primary }
[Browse components](components/index.md){ .md-button }

</div>

<div markdown="0"><video src="assets/clips/Showcase.mp4" poster="assets/clips/Showcase.jpg" autoplay loop muted playsinline preload="metadata"></video></div>

</div>

## What's inside

113 components in nine categories. Each one has its own page with a clip, setup steps and the full Inspector
reference.

<div class="grid cards" markdown>

-   :material-cog:{ .lg .middle } __Core__

    ---

    `CharacterCore`, `AbilityBase`, `CharacterController2D`, the keyboard/gamepad input reader and the
    Animator bridge. This is what every character is built on.

    [:octicons-arrow-right-24: Core](components/core/index.md)

-   :material-run-fast:{ .lg .middle } __Movement__

    ---

    Walk/run, jump with coyote time and jump buffer, multi-jump, dashes, walls, ledges, ladders, ropes,
    ziplines, swimming, flying, gliding, jetpack, slopes and top-down movement.

    [:octicons-arrow-right-24: Movement](components/movement/index.md)

-   :material-heart-pulse:{ .lg .middle } __Health & Physics__

    ---

    Health with invulnerability, knockback, stun, death, respawn, lives with game over, fall damage, gravity
    shaping, persistence between scenes and damage flash.

    [:octicons-arrow-right-24: Health & Physics](components/health/index.md)

-   :material-sword:{ .lg .middle } __Combat__

    ---

    Melee, hitscan and projectile weapons, combos, charged attacks, a weapon inventory, mouse/stick aiming and
    damageable props, all driven by `PlayerAttack`.

    [:octicons-arrow-right-24: Combat](components/combat/index.md)

-   :material-robot:{ .lg .middle } __AI & Enemies__

    ---

    Ready-made behaviours (patrol, chase, flee, shoot on sight, contact damage), an `EnemySpawner` and the
    `AIBrain` state machine you assemble in the Inspector.

    [:octicons-arrow-right-24: AI & Enemies](components/ai/index.md)

-   :material-terrain:{ .lg .middle } __Environment__

    ---

    Moving, falling and one-way platforms, elevators, levers, pressure plates, keys and doors, hazards, water,
    wind, pickups, checkpoints and level exits.

    [:octicons-arrow-right-24: Environment](components/environment/index.md)

-   :material-video:{ .lg .middle } __Camera__

    ---

    Smooth follow, level bounds, screen shake and zoom by speed for orthographic 2D cameras.

    [:octicons-arrow-right-24: Camera](components/camera/index.md)

-   :material-tune:{ .lg .middle } __Managers__

    ---

    Game state, level (void and checkpoints), score, inventory, pause, audio with crossfade, save/load and
    scene transitions with a fade.

    [:octicons-arrow-right-24: Managers](components/managers/index.md)

-   :material-monitor-dashboard:{ .lg .middle } __UI__

    ---

    Health bar, score text, pause menu and floating damage numbers that hook themselves up to the managers.

    [:octicons-arrow-right-24: UI](components/ui/index.md)

</div>

## Install

Gameplay Kit needs **Unity 6000.0** or newer. Add it to your project's `Packages/manifest.json`. As a local
package:

```json
"com.alejocastellanos.gameplaykit": "file:/path/to/com.alejocastellanos.gameplaykit"
```

Or from git:

```json
"com.alejocastellanos.gameplaykit": "https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0"
```

The Input System package is optional: the kit uses it when it is installed and active, and falls back to the
classic Input Manager otherwise. Then open **GameplayKit → Create Demo Scene** and press Play.

## How it works

A character is just a GameObject with a 2D collider, a `CharacterController2D` and the abilities you want.
`CharacterCore` finds every ability on the object and its children and runs them each frame, so adding or
removing a mechanic is just Add Component or Remove Component.

```text
Player  (tag: Player)
├─ CapsuleCollider2D
├─ Rigidbody2D             ← added automatically by CharacterController2D
├─ CharacterController2D   ← ground detection, shared gravity, velocity helpers
├─ PlayerWalkRun           ┐
├─ PlayerJump              │ abilities (AbilityBase)
├─ PlayerDash              ┘
└─ CharacterCore           ← finds the abilities and runs them every frame
```

If nothing on the character provides input, `CharacterCore` adds a `KeyboardInputReader` on its own. The
default controls are A/D or the arrows to move, Space to jump, Shift to run, Ctrl to crouch, Q to dash, J to
attack, K for the special action, E to interact and Esc to pause. On a gamepad: left stick, A/Cross to jump,
X/Square to attack.

Categories talk to each other through small interfaces (`IDamageable`, `IHealthSource`, `IKeyHolder`,
`IInteractable`). A weapon can hurt anything that takes damage, and a door can check any inventory, without
knowing the concrete classes. See [Architecture](reference/architecture.md) for the full picture.

## Where to go next

- [Getting started](guides/getting-started.md): install the package, open the demo and build your first character.
- [Platformer character](guides/platformer-character.md) and [Top-down character](guides/top-down-character.md): pick the abilities for your game.
- [Combat](guides/combat.md), [Health and damage](guides/health-and-damage.md) and [Enemies and AI](guides/enemies-and-ai.md): fights from both sides.
- [Building a level](guides/building-a-level.md): managers, camera, checkpoints, doors and the HUD.
- [Custom abilities](guides/custom-abilities.md): write your own mechanic on top of `AbilityBase`.
- [Troubleshooting](guides/troubleshooting.md): answers to the most common setup problems.
