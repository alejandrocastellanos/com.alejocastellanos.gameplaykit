# Getting started

Gameplay Kit is a Unity package of small, independent 2D gameplay components. You build characters,
enemies and levels by stacking components from **Add Component**, and everything works with its
default values: no layers, tags or references to set up before pressing Play.

This page covers what both game styles share: installing the package and using its menus. Then pick
your path:

<div class="grid cards" markdown>

-   **Platformer game**

    ---

    Side view with gravity: running, jumping, walls, ladders, water. The demo is a complete
    platformer level.

    [:octicons-arrow-right-24: Start a platformer](platformer-game.md)

-   **Top-down game**

    ---

    Seen from above, without gravity: 8-direction movement and dash, mouse aiming and shooting. The
    demo is a three-room dungeon.

    [:octicons-arrow-right-24: Start a top-down game](top-down-game.md)

</div>

## Requirements

- **Unity 6** (6000.0 or newer).
- 2D Physics and uGUI. They are declared as package dependencies, so Unity enables them for you.
- The **Input System** package is optional. The kit works with *Input System Package (New)*,
  *Input Manager (Old)* or *Both* — see [Input](input.md).

## Install the package

=== "Git URL"

    Open **Window → Package Manager**, click **+ → Add package from git URL…** and paste the
    repository URL with the version tag:

    ```text
    https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0
    ```

    Or add it directly to your project's `Packages/manifest.json`:

    ```json
    "com.alejocastellanos.gameplaykit": "https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0"
    ```

    The `#v1.1.0` suffix pins the release, so your project won't change when new commits land.

=== "Local folder"

    Clone or download the package to disk, then use **+ → Add package from disk…** in the Package
    Manager and pick its `package.json`, or reference the folder in `Packages/manifest.json`:

    ```json
    "com.alejocastellanos.gameplaykit": "file:/path/to/com.alejocastellanos.gameplaykit"
    ```

    This is the best option if you want to modify the package while using it.

## The GameplayKit menus

The kit adds two menus that build ready-to-use objects. Each object is created at the centre of the
Scene view, selected, and can be undone with ++ctrl+z++ (++cmd+z++ on Mac).

!!! info "These are two different menus"
    - **GameplayKit** is a menu of its own in Unity's **main menu bar**, at the same level as *File*,
      *Edit*, *Assets* or *GameObject*. It's the only one with the **demo scenes**.
    - **GameObject → GameplayKit** (the same submenu shows up when you **right-click in the
      Hierarchy**) has the individual objects, without the demo scenes. If you right-click an object,
      characters, enemies and platforms are created as its children.

### Main menu bar → GameplayKit

| Menu item | Style | What it creates |
|---|---|---|
| **Create Player** | Platformer | A complete platformer character tagged `Player`: capsule collider, [CharacterController2D](../components/core/CharacterController2D.md), walk/run, double jump, dash, wall jump and wall slide, crouch, ladders, swimming, drop-through, slopes, interact, melee attack, health, knockback, death, respawn, inventory, damage flash, animator bridge and [CharacterCore](../components/core/CharacterCore.md). |
| **Create Top-Down Player** | Top-down | A character seen from above, tagged `Player`: circle collider, [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md), 8-direction dash, interact, melee attack, health, knockback, death and respawn. |
| **Create Enemy** | Platformer | A red box with gravity that patrols (turning at walls and ledges), hurts the player on contact and is destroyed after enough damage (20 HP), with a damage flash and damage numbers. |
| **Create Top-Down Enemy** | Top-down | An enemy without gravity that chases the player on both axes when it's within 7 units ([EnemyChase](../components/ai/EnemyChase.md) with **Move Vertically**), hurts on contact and has 30 HP. |
| **Create Platform** | Both | A 4 × 0.5 solid block with a `BoxCollider2D`: a floor in a platformer, a wall in a top-down game. |
| **Create 2D Camera** | Both | An orthographic `Main Camera` (size 6) with [CameraFollow](../components/camera/CameraFollow.md) and [CameraShake](../components/camera/CameraShake.md). It follows whatever is tagged `Player`. |
| **Create Managers** | Both | A `Managers` object with [ScoreManager](../components/managers/ScoreManager.md) and [PauseManager](../components/managers/PauseManager.md). |
| **Create HUD** | Both | A canvas with a health bar, a score text and a pause menu (resume / restart), plus an `EventSystem` if the scene has none. |
| **Create Demo Scene** | Platformer | The platformer demo level. See [Platformer game](platformer-game.md#the-platformer-demo). |
| **Create Top-Down Demo Scene** | Top-down | The top-down demo dungeon. See [Top-down game](top-down-game.md#the-demo-dungeon). |

Both demo scenes ask you to save the current scene first, are saved in `Assets/GameplayKitDemo/` and
never overwrite each other. They also ship as a sample: **Package Manager → Gameplay Kit → Samples →
Demo 2D → Import**.

### Right-click in the Hierarchy → GameplayKit

| Item | Same as |
|---|---|
| **Player** | **Create Player** |
| **Top-Down Player** | **Create Top-Down Player** |
| **Enemy** | **Create Enemy** |
| **Top-Down Enemy** | **Create Top-Down Enemy** |
| **Platform** | **Create Platform** |
| **2D Camera**, **Managers**, **HUD** | The items with the same name. They are always created at the scene root, and only once even with several objects selected. |

!!! tip "HUD texts"
    The HUD is created with Spanish placeholder texts ("Puntos: 0", "Pausa", "Reanudar",
    "Reiniciar"). Change the score label with the **Format** field of
    [UIScoreText](../components/ui/UIScoreText.md) and edit the button labels directly in the
    hierarchy.

## Controls

Both styles read the keyboard and gamepad through [KeyboardInputReader](../components/core/KeyboardInputReader.md),
but each uses different keys (a top-down character doesn't jump, and it aims with the mouse). The tables
live on each path: [platformer controls](platformer-game.md#controls) and
[top-down controls](top-down-game.md#controls).

!!! warning "Gamepad buttons need the Input System"
    With only the classic Input Manager, the kit reads movement from the **Horizontal** /
    **Vertical** axes and jump from the **Jump** button of the Input Manager; the other gamepad
    buttons are not read. Details in [Input](input.md).

## Next steps

- [Platformer game](platformer-game.md) — the demo, the controls and your first platformer level.
- [Top-down game](top-down-game.md) — the demo dungeon, the controls and your first top-down room.
- [Input](input.md) — rebind keys or drive a character with your own input source.
