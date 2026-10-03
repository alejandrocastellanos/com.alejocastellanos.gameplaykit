---
title: Guides
description: Step-by-step guides for building platformer and top-down games with Gameplay Kit.
---

# Guides

The component pages tell you what each piece does. These guides show how the pieces fit together into a
game. Start with **Getting started**, follow the path for your game style (platformer or top-down), and
then reach for the shared guides when you need them.

## Start here

[Getting started](getting-started.md)
:   Install the package and meet the kit's two menus: **GameplayKit** in the main menu bar (with the demo
    scenes) and **GameObject → GameplayKit** / right-click in the Hierarchy. Each item says whether it's for
    platformers, top-down games or both.

[Input](input.md)
:   How abilities read input through `ICharacterInput`, what `KeyboardInputReader` binds by default, how
    the kit works with the Input System, the classic Input Manager or both, and how to plug in your own input
    source (AI, replays, `PlayerInput`).

## Platformer games

Side view with gravity: running, jumping, walls, ladders and water.

[Platformer game](platformer-game.md)
:   The starting point: the platformer demo walked through left to right, the platformer controls and your
    first level in five minutes.

[Platformer character](platformer-character.md)
:   Build the character by hand: walking and running, jumping and multi-jumping, dashes, walls, ledges,
    ladders, water and one-way platforms, which abilities you shouldn't stack, and how to tune the jump.

## Top-down games

Seen from above, without gravity: 8-direction movement, mouse aiming and shooting.

[Top-down game](top-down-game.md)
:   The starting point: the demo dungeon with its room-by-room map, the top-down controls and your first
    room in five minutes.

[Top-down character](top-down-character.md)
:   `PlayerTopDownMovement` and the gravity override, the 8-direction dash, four-way attacks, shooting
    towards the mouse with two weapons, the camera, and enemies that move on both axes.

## For both styles

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

[Building a level](building-a-level.md)
:   Managers (game, level, score, inventory, pause, audio, saves, scene transitions), the camera,
    environment pieces, checkpoints and respawn, keys and doors, the level exit and the HUD.

[Custom abilities](custom-abilities.md)
:   How `CharacterCore` runs abilities in phases, what `AbilityBase` and `CharacterController2D` give you
    (shared gravity, control locks, suspension), and a complete new ability written step by step.

[Troubleshooting](troubleshooting.md)
:   The usual suspects: characters falling through the floor, ground not detected, abilities or input
    not responding, Missing Script, enemies walking off ledges, camera jitter, layers and tags, and tests
    that don't show up.
