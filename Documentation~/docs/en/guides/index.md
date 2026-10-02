---
title: Guides
description: Step-by-step guides for building games with Gameplay Kit.
---

# Guides

The component pages tell you what each piece does. These guides show how the pieces fit together into a
character, a fight or a whole level. If you're new to the kit, read them roughly in this order.

## Start here

[Getting started](getting-started.md)
:   Install the package, open the demo scene from **GameplayKit → Create Demo Scene**, and build your first
    character from an empty GameObject. It also covers the default controls and the **GameplayKit** menu that
    creates ready-made players, enemies, cameras, managers and HUDs.

[Input](input.md)
:   How abilities read input through `ICharacterInput`, what `KeyboardInputReader` binds by default, how
    the kit works with the Input System, the classic Input Manager or both, and how to plug in your own input
    source (AI, replays, `PlayerInput`).

## Characters

[Platformer character](platformer-character.md)
:   A side-view character: walking and running, jumping and multi-jumping, dashes, walls, ledges, ladders,
    water and one-way platforms, and which abilities you shouldn't stack.

[Top-down character](top-down-character.md)
:   A character seen from above with `PlayerTopDownMovement`, 8-direction dash, interaction and attacks,
    without gravity.

## Fighting

[Combat](combat.md)
:   `PlayerAttack` and the weapons: melee hitboxes, hitscan, projectiles, combos, charged attacks, weapon
    switching and aiming with the mouse or the right stick.

[Health and damage](health-and-damage.md)
:   `CharacterHealth` and `DamageableObject`, invulnerability, knockback, stun, death, respawn, lives and
    game over, plus feedback like `DamageFlash` and damage numbers.

[Enemies and AI](enemies-and-ai.md)
:   Ready-made enemy behaviours versus the `AIBrain` state machine. A worked example of an enemy that
    patrols, chases you when it sees you and runs away when it's hurt. Also covers `EnemySpawner` and writing
    your own actions and decisions.

## Levels

[Building a level](building-a-level.md)
:   Managers (game, level, score, inventory, pause, audio, saves, scene transitions), the camera,
    environment pieces, checkpoints and respawn, keys and doors, the level exit and the HUD, with a tour of
    the demo scene.

## Extending the kit

[Custom abilities](custom-abilities.md)
:   How `CharacterCore` runs abilities in phases, what `AbilityBase` and `CharacterController2D` give you
    (shared gravity, control locks, suspension), and a complete new ability written step by step.

[Troubleshooting](troubleshooting.md)
:   The usual suspects: characters falling through the floor, ground not detected, abilities or input
    not responding, Missing Script, enemies walking off ledges, camera jitter, layers and tags, and tests
    that don't show up.
