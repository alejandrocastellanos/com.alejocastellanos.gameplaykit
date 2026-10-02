---
title: Building a level
description: Managers, camera, environment pieces, checkpoints, keys and doors, level exits and the HUD, with a tour of the demo scene.
---

# Building a level

A playable level in Gameplay Kit is a player, some solid colliders, a few environment pieces, a camera, a
handful of managers and a HUD. This guide walks through each layer, starting from the demo scene, since it
already uses most of them.

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/Showcase.mp4" poster="../../assets/clips/Showcase.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>A run through the demo level: platforms, an enemy, spikes, a checkpoint, a ladder, a key, water, a conveyor, a lever-driven elevator and the locked door.</figcaption></figure>

## A tour of the demo scene

**GameplayKit → Create Demo Scene** builds and saves a short level at
`Assets/GameplayKitDemo/GameplayKitDemo.unity` (a numbered name if one already exists). The player and enemy
prefabs go in `Assets/GameplayKitDemo/Prefabs/`. Everything is made from the same builders as the other
**GameplayKit** menu items, so it's a good reference for wiring things up. The level reads left to right
(ground top at y = 0 unless noted):

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

## Managers

Managers are plain components. Most of them are singletons with a static `Instance`, and other components
find them on their own. Add only the ones you need.

| Manager | Does | Persists between scenes |
|---|---|---|
| [GameManager](../components/managers/GameManager.md) | Holds a global `GameState` (`Playing`, `Paused`, `GameOver`) and raises `OnStateChanged`. `CharacterLives` sets it to `GameOver`, and `PauseManager` switches it between `Playing` and `Paused` (never overriding `GameOver`). | Yes |
| [LevelManager](../components/managers/LevelManager.md) | Level bounds (used by `CameraBounds` when the camera has none of its own), the active checkpoint, and the void: with **Kill Below Void** on, any `CharacterHealth` below **Void Y** (−30) takes lethal damage. | No |
| [ScoreManager](../components/managers/ScoreManager.md) | Adds every `Collectible`'s **Value** automatically (it listens to the static `Collectible.OnAnyCollected`). `AddScore`, `ResetScore`, `OnScoreChanged`. | No |
| [InventoryManager](../components/managers/InventoryManager.md) | Item and key ids. It implements `IKeyHolder`, so pickups, doors and exits can use it. **It goes on the player**, not on a managers object, and it isn't a singleton. Items are a set: each id is either held or not, with no counts. | With the player |
| [PauseManager](../components/managers/PauseManager.md) | Toggles `Time.timeScale` with **Pause Key** (Esc) or **Pause Pad Button** (Start) and raises `OnPauseChanged`, keeping the `GameManager` state in sync. It ignores the key for the first second and until it has seen it released, so a key held while loading doesn't pause the game. | No |
| [AudioManager](../components/managers/AudioManager.md) | `PlayMusic(clip)` crossfades between two music sources over **Music Fade Duration**, even while paused. `PlaySfx(clip, volume)` plays one-shots. It creates its audio sources if you don't assign them. | Yes |
| [SaveLoadSystem](../components/managers/SaveLoadSystem.md) | `Save<T>(data)` / `TryLoad<T>(out data)` store any `[Serializable]` class as JSON in `PlayerPrefs` under **Save Key**. Corrupt data is ignored with a warning. | Yes |
| [SceneTransitionManager](../components/managers/SceneTransitionManager.md) | `LoadScene(name)` or `LoadScene(buildIndex)` with a fade to **Fade Color**. It creates its own full-screen fade layer if you don't assign one, ignores requests while a transition is running and resets `Time.timeScale` to 1. | Yes |

**GameplayKit → Create Managers** creates a `Managers` object with `ScoreManager` and `PauseManager`, the
per-scene essentials.

!!! warning "Give persistent managers their own GameObjects"
    `GameManager`, `AudioManager`, `SaveLoadSystem` and `SceneTransitionManager` call `DontDestroyOnLoad`.
    When a duplicate wakes up in the next scene, it destroys its **whole GameObject**. If you put
    `GameManager` on the same object as `ScoreManager`, the old score manager travels to the next scene, and
    the new scene's `Managers` object (with its fresh `ScoreManager`) gets destroyed. Keep each persistent
    manager on its own GameObject, and keep the per-scene ones (`LevelManager`, `ScoreManager`,
    `PauseManager`) together. That GameObject can be a child of your `Managers` object for tidiness:
    persistent managers move themselves to the scene root before calling `DontDestroyOnLoad`, which only
    works on root objects.

Because `ScoreManager` lives in the scene, the score starts at zero in every scene. To carry it over, or keep
it between sessions, save it:

```csharp
using GameplayKit.Managers;
using UnityEngine;

[System.Serializable]
public class Progress
{
    public int score;
    public string lastLevel;
}

public class ProgressSaver : MonoBehaviour
{
    public void SaveNow(string levelName)
    {
        SaveLoadSystem.Instance.Save(new Progress { score = ScoreManager.Instance.CurrentScore, lastLevel = levelName });
    }

    private void Start()
    {
        if (SaveLoadSystem.Instance != null && SaveLoadSystem.Instance.TryLoad(out Progress progress))
            ScoreManager.Instance.AddScore(progress.score);
    }
}
```

## Camera

**GameplayKit → Create 2D Camera** creates an orthographic camera (size 6) tagged `MainCamera`, with
`CameraFollow` and `CameraShake`. The four camera components stack in a fixed order:
`CameraFollow` moves the camera, `CameraBounds` (execution order 50) clamps it, and `CameraShake` (order 100)
adds its offset on top and removes it the next frame.

| Component | Notes |
|---|---|
| [CameraFollow](../components/camera/CameraFollow.md) | Follows **Target** with `SmoothDamp` (**Smooth Time** 0.2) plus **Offset** (0, 0, −10). Leave **Target** empty and it finds the object tagged `Player`, snapping to it on the first frame. `SetTarget()` switches targets at runtime. |
| [CameraBounds](../components/camera/CameraBounds.md) | Keeps the view inside **Min Bounds**/**Max Bounds**, accounting for the orthographic size and aspect ratio. If the area is smaller than the view on an axis, it centres the camera instead. Left at (0, 0)/(0, 0), it uses the `LevelManager` bounds, and if there are none it doesn't clamp. |
| [CameraShake](../components/camera/CameraShake.md) | `Shake()` uses the default duration and magnitude, and `Shake(duration, magnitude)` sets them. It runs on unscaled time. Nothing in the kit calls it for you: hook it to an event. |
| [CameraZoomBySpeed](../components/camera/CameraZoomBySpeed.md) | Zooms out from **Min Zoom** (5) to **Max Zoom** (8) as the target's speed approaches **Speed For Max Zoom** (10). With no target it uses the `Rigidbody2D` of the `Player`-tagged object. |

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/CameraShake.mp4" poster="../../assets/clips/CameraShake.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>CameraShake adds its jitter on top of the follow position, then removes it.</figcaption></figure>

A shake when the player gets hurt:

```csharp
using GameplayKit.CameraSystem;
using GameplayKit.Health;
using UnityEngine;

public class ShakeOnHit : MonoBehaviour
{
    [SerializeField] private CharacterHealth health;
    [SerializeField] private CameraShake cameraShake;

    private void OnEnable() => health.OnDamaged += HandleDamaged;
    private void OnDisable() => health.OnDamaged -= HandleDamaged;

    private void HandleDamaged(float amount) => cameraShake.Shake(0.2f, Mathf.Clamp(amount * 0.01f, 0.05f, 0.4f));
}
```

`PlayerGroundSlam.OnSlamLanded` is another good place to hook a shake.

## Environment pieces

Every environment component works on a GameObject with the right kind of collider. The "Collider" column
tells you what to give it.

| Piece | Collider | Notes |
|---|---|---|
| [OneWayPlatform](../components/environment/OneWayPlatform.md) | Solid (it adds a `BoxCollider2D` if there's none) | Sets up a `PlatformEffector2D`. Players with `PlayerDropThrough` drop through with down + jump. |
| [MovingPlatform](../components/environment/MovingPlatform.md) | Solid + `Rigidbody2D` (set to Kinematic for you) | Loop or ping-pong through **Waypoints**. Child waypoints are fine, because their positions are copied at start. It carries dynamic bodies standing on top. |
| [Elevator](../components/environment/Elevator.md) | Solid + `Rigidbody2D` | Moves between **Bottom Point** and **Top Point** when you call `Activate()` (from a lever or plate event). |
| [FallingPlatform](../components/environment/FallingPlatform.md) | Solid + `Rigidbody2D` | Falls **Fall Delay** seconds after the player lands on it, and can respawn. |
| [ConveyorBelt](../components/environment/ConveyorBelt.md) | Solid | Adds its **Speed** along **Direction** to whoever stands on it, even if they stand still. |
| [SpringPad](../components/environment/SpringPad.md) | Trigger (or solid with **Use Trigger** off) | Launches along **Launch Direction** with **Launch Force**. |
| [HazardZone](../components/environment/HazardZone.md) | Trigger | Damages any `IDamageable` on entry, then every **Damage Interval**, even if it stands still. |
| [WaterZone](../components/environment/WaterZone.md) | Trigger | Drag and buoyancy for any body. `PlayerSwim` recognises it without a tag. |
| [WindZone2D](../components/environment/WindZone2D.md) | Trigger | A constant force on the bodies inside. |
| [Teleporter](../components/environment/Teleporter.md) | Trigger | Sends whoever has **Target Tag** to **Destination**. Pairs don't bounce you back. |
| [PushableBox](../components/environment/PushableBox.md) | Solid + `Rigidbody2D` | The player pushes it by walking into it. |
| [BreakableObject](../components/environment/BreakableObject.md) | Solid | Breaks after enough damage and can drop loot. |
| [Lever](../components/environment/Lever.md) | Any (triggers count) | Toggled by `PlayerInteract` (**E**). Wire **On Turned On**, **On Turned Off** or **On Toggled** in the Inspector. |
| [PressurePlate](../components/environment/PressurePlate.md) | Trigger | Pressed by any body (or one with **Required Tag**). **On Pressed**/**On Released** events. |
| [Collectible](../components/environment/Collectible.md), [HealthPickup](../components/environment/HealthPickup.md), [ItemPickup](../components/environment/ItemPickup.md) | Trigger | Coins/score, healing (only when hurt, by default) and items/keys. |
| [Checkpoint](../components/environment/Checkpoint.md), [DoorWithKey](../components/environment/DoorWithKey.md), [LevelExit](../components/environment/LevelExit.md) | Trigger | Covered below. |

Wiring a lever to an elevator takes no code. Select the lever, add an entry to **On Toggled**, drag the
elevator in and pick `Elevator → Activate`. The first activation sends it up, and each one after that sends
it to the other end.

## Checkpoints and respawn

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/Checkpoint.mp4" poster="../../assets/clips/Checkpoint.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>After touching the checkpoint, dying brings the player back there instead of to the start.</figcaption></figure>

The player needs [CharacterHealth](../components/health/CharacterHealth.md),
[CharacterDeath](../components/health/CharacterDeath.md) and [CharacterRespawn](../components/health/CharacterRespawn.md).
*Create Player* adds all three. Here's what happens:

1. The player enters a `Checkpoint` trigger. If the collider (or its Rigidbody2D) has **Player Tag**, the
   checkpoint calls `CharacterRespawn.SetCheckpoint` and, if there's a `LevelManager`, `SetActiveCheckpoint`.
   It turns on **Active Visual** and raises `OnActivated`. A checkpoint activates **only once**, so walking
   back through an older one doesn't move your respawn point back.
2. Something kills the player: a strong `HazardZone`, an enemy, or the `LevelManager` void.
3. `CharacterDeath` sets the condition to `Dead` and suspends every ability.
4. After **Respawn Delay** (1 s), `CharacterRespawn` moves the player to the last checkpoint (or to its
   **Initial Checkpoint**, or to where it started), zeroes its velocity, restores health, revives it and
   resets every ability.

Add [CharacterLives](../components/health/CharacterLives.md) and each death costs a life. When the last one is
gone, the player stays dead, `OnGameOver` fires and `GameManager` (if present) switches to `GameOver`:

```csharp
using GameplayKit.Health;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private CharacterLives lives;
    [SerializeField] private GameObject panel;

    private void OnEnable() => lives.OnGameOver += Show;
    private void OnDisable() => lives.OnGameOver -= Show;

    private void Show() => panel.SetActive(true);
}
```

## Keys and doors

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/DoorWithKey.mp4" poster="../../assets/clips/DoorWithKey.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>The door stays shut until the player picks up the key, then opens on contact.</figcaption></figure>

1. Make sure the player has an `IKeyHolder`, which in practice means `InventoryManager` (*Create Player*
   adds it).
2. Place a key: a small trigger with [ItemPickup](../components/environment/ItemPickup.md). On touch it adds
   **Item Id** to the holder and destroys itself.
3. Build the door: a GameObject with a **trigger** collider a bit larger than the doorway and
   [DoorWithKey](../components/environment/DoorWithKey.md). Add a child with a **solid** collider and the
   door's sprite, then assign that child to both **Blocking Collider** and **Visual When Closed**.
4. When a holder with **Required Item Id** enters the trigger, the door disables the blocking collider, hides
   the visual and raises `OnOpened`. With **Consume Item On Open**, the key is removed.

`ItemPickup` and `DoorWithKey` share the same default id (`llave_dorada`), so a key and a door work
together out of the box. Change both ids when a level has several locks. `DoorWithKey.Open()` is public, so
a lever or pressure plate event can open a door without a key.

## Leaving the level

[LevelExit](../components/environment/LevelExit.md) is a trigger that loads another scene when the player
(by **Player Tag**) touches it:

- **Scene Name** set: that scene is loaded. Empty: the next scene in the build list. If there isn't one,
  you get a warning and nothing happens.
- **Required Item Id** set: only a player holding that item can use it.
- With a `SceneTransitionManager` in the scene, the load fades. Without one, it loads immediately.

Scenes must be in the build list (**File → Build Profiles**) to load. To keep the player across levels, add
[CharacterPersistence](../components/health/CharacterPersistence.md) to it and remove the player object from
the later scenes, or keep it and let the duplicate destroy itself. Either way, the camera and the health bar
find the `Player`-tagged object again on their own.

## The HUD

**GameplayKit → Create HUD** builds a Screen Space Overlay canvas (scaled for 1920×1080) with:

- **HealthBar**: a [UIHealthBar](../components/ui/UIHealthBar.md) driving a filled `Image`. Leave **Health**
  empty and it finds the `Player`-tagged character, and finds it again if that character is replaced.
- **Score**: a [UIScoreText](../components/ui/UIScoreText.md). Its default **Format** is `Puntos: {0}`. Change
  it to `Score: {0}` (or anything with `{0}`) for an English HUD.
- **PausePanel**: shown and hidden by a [UIPauseMenu](../components/ui/UIPauseMenu.md) on the canvas, which
  listens to `PauseManager`. Its two buttons call `OnResumeButtonPressed` and `OnRestartButtonPressed`.
  Restart reloads the active scene. In the editor this works even if the scene isn't in the build list, but
  in a build it must be. There's also `OnQuitButtonPressed`, which loads **Main Menu Scene Name**.
- An `EventSystem`, using the Input System's UI module when that package is installed and
  `StandaloneInputModule` otherwise.

The labels in the generated HUD ("Pausa", "Reanudar", "Reiniciar") are plain `Text` components, so you can
rename them freely. For damage numbers over characters and enemies, add
[DamagePopupSpawner](../components/ui/DamagePopupSpawner.md) to them. It needs no canvas.

## See also

- [Enemies and AI](enemies-and-ai.md): populate the level.
- [Health and damage](health-and-damage.md): everything that happens between a hit and a respawn.
- [Troubleshooting](troubleshooting.md): when a pickup, door or camera doesn't react.
