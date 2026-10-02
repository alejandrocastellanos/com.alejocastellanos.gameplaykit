---
title: Testing
description: How Gameplay Kit's test suites are organised, how to run them, how to write a new test, and how the documentation clips are produced.
---

# Testing

Gameplay Kit ships with 92 automated tests: 4 editor tests that guard the component catalogue and 88
play-mode tests that exercise the mechanics in real physics. The same helpers also drive the scenarios that
record the clips in this documentation.

## How the suites are organised

| Folder | Assembly | Mode | What it checks |
|---|---|---|---|
| `Tests/Editor/` | `GameplayKit.Tests.Editor` | Edit mode | The component catalogue. |
| `Tests/Runtime/` | `GameplayKit.Tests.Runtime` | Play mode | Gameplay, plus the `TestWorld` and `ScriptedCharacterInput` helpers. |

Both assemblies use the `GameplayKit.Tests` namespace, aren't auto-referenced, and only compile when
`UNITY_INCLUDE_TESTS` is defined, which happens when the package is marked as testable (see below).

### Editor tests: the catalogue

`ComponentCatalogTests` catches mistakes that gameplay tests can't see:

| Test | Guarantees |
|---|---|
| `EveryComponent_HasItsOwnScriptFile` | Every non-abstract `MonoBehaviour` in the runtime assembly lives in a script file with its own name. Otherwise it works through `AddComponent` but shows up as *Missing Script* once saved. |
| `EveryComponent_CanBeAddedToAnEmptyGameObject` | Every component can be added to an empty GameObject without throwing, so it works straight from **Add Component**. |
| `NoComponent_SharesItsNameWithABuiltInUnityComponent` | No class name clashes with a built-in Unity component. That's why the wind zone is called `WindZone2D`. |
| `MenuBuilders_CreatePlayerAndEnemyWithoutMissingScripts` | The **GameplayKit** menu builds a tagged player with `CharacterCore` and `KeyboardInputReader`, and an enemy, with no missing scripts. |

### Play-mode tests: the mechanics

| File | Tests | Covers |
|---|---|---|
| `MovementTests.cs` | 15 | Ground detection with no setup, walk/run, jump and multi-jump, dash, wall jump/slide, glide, jetpack, crouch, ladders, swimming. |
| `GameplayTests.cs` | 16 | Health, death and respawn, checkpoints, hazards, melee/hitscan/projectile hits (never the attacker), springs, collectibles and score, keys and doors, moving platforms, conveyors, chase, patrol at walls and ledges, a basic `AIBrain`. |
| `MoreMechanicsTests.cs` | 26 | Fly, roll, blink, `PlayerAttack`, interact + lever + elevator, pressure plates, ledges, ziplines, ropes, edge dangle, 8-way dash, crawl, ground slam, path follow, fall damage, stun, knockback, one-way and falling platforms, teleporters, pushable boxes, wind, shooting enemies, contact damage. |
| `ThirdRoundTests.cs` | 31 | Wall cling, slopes, top-down, drop-through, jump buffer, persistence, charged attacks, weapon switching, aiming, flee, bounded patrol, pathfinding without a NavMesh, every AI action and decision, the spawner, lives, health pickups, damage flash and popups, camera shake and zoom, every manager, level exit, the Animator bridge. |

Test names describe the expected behaviour, for example `EnemyPatrol_DoesNotWalkOffEdges` or
`HazardZone_KeepsDamagingWhilePlayerStandsStill`. That makes the suite a handy, executable reference for how
each component is meant to be set up.

## Running the tests

1. List the package as testable in your project's `Packages/manifest.json`:

    ```json
    {
      "dependencies": {
        "com.alejocastellanos.gameplaykit": "file:/path/to/com.alejocastellanos.gameplaykit"
      },
      "testables": ["com.alejocastellanos.gameplaykit"]
    }
    ```

2. Open **Window → General → Test Runner**.
3. Run the **EditMode** tab for the catalogue tests and the **PlayMode** tab for the mechanics.

From the command line, use Unity's standard test flags:

```bash
Unity -batchmode -projectPath /path/to/project -runTests -testPlatform PlayMode -testResults results.xml
```

## Test helpers

Both helpers live in `Tests/Runtime/TestSupport.cs`.

### TestWorld

Builds minimal levels in code and destroys everything it created when disposed.

| Member | Description |
|---|---|
| `GameObject Track(GameObject go)` | Register an object to be destroyed in `Dispose`. Returns it. |
| `GameObject Block(string name, Vector2 center, Vector2 size, bool trigger = false)` | A tracked GameObject with a `BoxCollider2D`. |
| `GameObject Ground(float y = -0.5f, float width = 200f)` | A 1-unit-thick floor whose **top surface** is at `y`. |
| `CharacterCore Player(Vector2 position, out ScriptedCharacterInput input, params Type[] abilities)` | A character the way a user builds one: `CapsuleCollider2D` (0.8 × 1.8), `CharacterController2D`, `ScriptedCharacterInput`, the abilities in order, and `CharacterCore` last. |
| `GameObject Point(Vector2 position)` | An empty tracked GameObject, useful as a target or waypoint. |
| `static void Set(object target, string field, object value)` | Assign a private `[SerializeField]` (searching base classes), as the Inspector would. Throws if the field doesn't exist. |
| `void Dispose()` | Destroy every tracked object. |

### ScriptedCharacterInput

An `ICharacterInput` you drive from the test. You set the *held* state, and the "pressed/released this frame"
flags are derived automatically, just like a keyboard. It has `[DefaultExecutionOrder(-1000)]`, so abilities
see the current frame's input.

| Member | Description |
|---|---|
| `Vector2 Move` | Movement input. |
| `bool Jump`, `Run`, `Crouch` | Held buttons. Setting `Jump` to true produces one `JumpPressedThisFrame`, and false produces one `JumpReleasedThisFrame`. |
| `void TapDash()`, `void TapInteract()` | Press dash or interact for a single frame. |
| `void Hold(CharacterAction)`, `void Release(CharacterAction)` | Hold or release an action. |
| `void TapAction(CharacterAction)` | Press an action for a single frame. |

### Patterns used throughout the suite

- **Wait until grounded** before acting: loop `while (!player.Controller.IsGrounded) yield return null;`, with
  a timeout.
- **Configure before `Awake`**: for components that read their fields in `Awake`/`OnEnable` (or build lookups
  there, like `AIBrain`), create the GameObject inactive, add components, fill fields with `TestWorld.Set`,
  then `SetActive(true)`.
- **Skip physics you don't need**: tests of pure logic place objects high up (y = 50) so nothing collides.
- **Run in the background**: `SetUp` sets `Application.runInBackground = true`, so tests keep running when
  the editor loses focus.

## Writing a new test

Add a class to `Tests/Runtime` (or to your own play-mode test assembly that references `GameplayKit.Runtime`
and `GameplayKit.Tests.Runtime`). This test checks that an `AIBrain` goes back to idle when its target leaves
range:

```csharp
using System.Collections;
using System.Collections.Generic;
using GameplayKit.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    public class MyAITests
    {
        private TestWorld _world;

        [SetUp]
        public void SetUp()
        {
            Application.runInBackground = true;
            _world = new TestWorld();
        }

        [TearDown]
        public void TearDown() => _world.Dispose();

        [UnityTest]
        public IEnumerator AIBrain_GoesBackToIdle_WhenTargetLeavesRange()
        {
            var target = _world.Point(new Vector2(3f, 50f));

            // Build the enemy inactive so AIBrain.Awake sees the configured states, like a scene object would.
            var enemy = _world.Track(new GameObject("Enemy"));
            enemy.SetActive(false);
            enemy.transform.position = new Vector2(0f, 50f);
            var wait = enemy.AddComponent<AIActionWait>();
            var inRange = enemy.AddComponent<AIDecisionTargetInRange>();
            TestWorld.Set(inRange, "range", 5f);
            var brain = enemy.AddComponent<AIBrain>();
            TestWorld.Set(brain, "target", target.transform);
            TestWorld.Set(brain, "states", new List<AIState>
            {
                new AIState { name = "Idle", actions = new List<AIActionBase> { wait },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, trueTargetState = "Alert" } } },
                new AIState { name = "Alert", actions = new List<AIActionBase> { wait },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, falseTargetState = "Idle" } } },
            });
            enemy.SetActive(true);

            yield return null;
            yield return null;
            Assert.AreEqual("Alert", brain.CurrentState.name, "The target is within 5 units");

            target.transform.position = new Vector2(20f, 50f);
            yield return null;
            yield return null;
            Assert.AreEqual("Idle", brain.CurrentState.name, "The target left the range");
        }
    }
}
```

Tips:

- Name tests `Subject_ExpectedBehaviour`, and write assertion messages that explain what *should* happen.
- Assert behaviour (positions, health, states), not implementation details.
- Use `WaitForSeconds` for physics-driven checks, and leave margins: physics isn't frame-exact.
- Keep each test self-contained. Everything goes through `TestWorld` so `TearDown` cleans up.

## How the documentation clips are made

Every clip on this site is recorded by an explicit play-mode test. Nothing is captured by hand, so clips can
be regenerated whenever a component changes.

### The capture framework

The capture sources live in `Documentation~/capture/`:

| File | Contents |
|---|---|
| `DocStage.cs` | `DocStage` (a `TestWorld` plus a recording camera) and its helpers: `DocPalette` (colours), `DocPainter`/`DocVisual` (draw every `Collider2D` as a coloured shape by role, so scenarios only build physics), `DocFollow` (camera follow), `DocOverlay` (pressed keys and a live caption) and `DocRecorder` (frame writer). |
| `GameplayKit.DocCapture.asmdef` | A play-mode test assembly referencing `GameplayKit.Runtime`, `GameplayKit.Tests.Runtime`, `UnityEngine.UI` and NUnit. |
| `Scenarios/*.cs` | One `[UnityTest, Explicit, Category("DocCapture")]` method per clip, named `<Component>_Clip`. |

A `DocStage` sets `Time.captureFramerate` to 30 fps and renders a 16:9 orthographic camera into a 960×540
`RenderTexture`. Once `StartRecording(seconds)` is called, `DocRecorder` reads the texture at the end of each
frame and writes numbered PNGs (`f_0000.png`, `f_0001.png`, ...) to `<project>/DocCaptures/frames/<Id>/`, where
`Id` is the component's class name (or `Showcase`). A scenario looks just like a gameplay test:

```csharp
[UnityTest, Explicit, Category("DocCapture")]
public IEnumerator PlayerDash_Clip()
{
    var s = new DocStage("PlayerDash", new Vector2(2f, 2.5f), 9f);
    try
    {
        s.World.Ground();
        var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerDash));
        s.ShowInput(input);
        yield return DocStage.Settle(p);
        s.StartRecording(4f);
        yield return DocStage.Seconds(0.4f);
        input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
        input.TapDash();            yield return DocStage.Seconds(0.8f);
        input.Move = Vector2.zero;
        yield return s.WaitRecording();
    }
    finally { s.Dispose(); }
}
```

### Recording the clips

1. Unity ignores folders whose name ends in `~`, so `Documentation~/capture/` is never compiled inside the
   package. **Copy the `capture` folder into the `Assets/` folder of a Unity project** that uses the package.
2. Make sure the package is in `testables` (the capture assembly references `GameplayKit.Tests.Runtime`).
3. In the Test Runner's **PlayMode** tab, select the `DocCapture` tests you want and run them. They're
   `Explicit`, so they never run as part of a normal test run.
4. Encode each frame folder with ffmpeg into an H.264 MP4 and a poster JPG named after the component, for
   example:

    ```bash
    ffmpeg -framerate 30 -i DocCaptures/frames/PlayerDash/f_%04d.png \
           -c:v libx264 -pix_fmt yuv420p -movflags +faststart docs/assets/clips/PlayerDash.mp4
    ffmpeg -i DocCaptures/frames/PlayerDash/f_0060.png docs/assets/clips/PlayerDash.jpg
    ```

5. Put the files in `docs/assets/clips/`. The site generator embeds `<Component>.mp4` on the component's page
   and uses `<Component>.jpg` for its catalogue card whenever they exist.

Clips are shared by both languages, so the on-screen captions stay short and language-neutral ("HP 3/5",
"Score 30").
