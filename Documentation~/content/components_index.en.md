---
title: Components
description: Every Gameplay Kit component, grouped by category.
---

# Components

Gameplay Kit has 113 components in nine categories. Each category matches a namespace (`GameplayKit.Movement`,
`GameplayKit.AI`, ...) and a folder under `Runtime/`. You add components from **Add Component** by searching
for their name. Nothing needs to be wired up by hand to get started. If you'd rather start from a working
setup, the **GameplayKit** menu creates ready-made players, enemies, cameras, managers and a HUD (see
[Getting started](../guides/getting-started.md)).

## How to read a component page

Every component page follows the same layout:

- **Badges and summary**: the category, the base class when it isn't `MonoBehaviour` (for example
  `AbilityBase` for character abilities) and an *Experimental* badge where it applies, followed by a
  one-line summary.
- **Clip**: a short loop recorded in game showing the component doing its job. Components with nothing
  visible to show (managers that only hold data, base classes) say so instead.
- **Overview**: what it does, how it behaves in detail, and how it works with other kit components.
- **Info table**: the namespace, base class, required components (added automatically by Unity) and the
  interfaces it implements, such as `IDamageable` or `IInteractable`.
- **Setup**: numbered steps to get it working.
- **Inspector**: every serialized field, with its Inspector label in bold, the field name below it, the
  type, the default value and what it controls. The defaults are the real values from the code, and they
  are tuned to work as-is.
- **Events**: `UnityEvent`s you can wire up in the Inspector and C# events you can subscribe to from code.
- **Scripting API**: public properties and methods worth knowing.
- **Example** and **Tips**: a realistic code snippet, and practical advice and common pitfalls.
- **See also**: closely related components.

Abilities (components derived from `AbilityBase`) only run on a character that has a `CharacterCore`. Base
classes (`AbilityBase`, `AIActionBase`, `AIDecisionBase`) are meant to be extended, not added. Their pages
show how.

## Categories

<div class="grid cards" markdown>

-   :material-cog:{ .lg .middle } __Core__

    ---

    The character foundation: `CharacterCore`, `AbilityBase`, `CharacterController2D`, the input reader and the
    Animator bridge.

    [:octicons-arrow-right-24: Core](core/index.md)

-   :material-run-fast:{ .lg .middle } __Movement__

    ---

    Thirty abilities and markers for platformers and top-down games: jumps, dashes, walls, ledges, ladders,
    ropes, ziplines, water, flight and more.

    [:octicons-arrow-right-24: Movement](movement/index.md)

-   :material-heart-pulse:{ .lg .middle } __Health & Physics__

    ---

    Health, knockback, stun, death, respawn, lives, fall damage, gravity shaping, persistence and damage flash.

    [:octicons-arrow-right-24: Health & Physics](health/index.md)

-   :material-sword:{ .lg .middle } __Combat__

    ---

    `PlayerAttack` and the weapons: melee, hitscan, projectiles, combos, charge, inventory, aiming and
    damageable props.

    [:octicons-arrow-right-24: Combat](combat/index.md)

-   :material-robot:{ .lg .middle } __AI & Enemies__

    ---

    Standalone enemy behaviours, the spawner, and the `AIBrain` state machine with its actions and decisions.

    [:octicons-arrow-right-24: AI & Enemies](ai/index.md)

-   :material-terrain:{ .lg .middle } __Environment__

    ---

    Platforms, elevators, levers, plates, doors and keys, hazards, water, wind, pickups, checkpoints and exits.

    [:octicons-arrow-right-24: Environment](environment/index.md)

-   :material-video:{ .lg .middle } __Camera__

    ---

    Follow, bounds, shake and zoom by speed for orthographic cameras.

    [:octicons-arrow-right-24: Camera](camera/index.md)

-   :material-tune:{ .lg .middle } __Managers__

    ---

    Game state, level, score, inventory, pause, audio, saves and scene transitions.

    [:octicons-arrow-right-24: Managers](managers/index.md)

-   :material-monitor-dashboard:{ .lg .middle } __UI__

    ---

    Health bar, score text, pause menu and floating damage numbers.

    [:octicons-arrow-right-24: UI](ui/index.md)

</div>
