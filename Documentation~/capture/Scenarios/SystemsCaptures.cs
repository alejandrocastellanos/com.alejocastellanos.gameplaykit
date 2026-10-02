using System.Collections;
using System.Collections.Generic;
using GameplayKit.CameraSystem;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using GameplayKit.Tests;
using GameplayKit.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GameplayKit.DocCapture
{
    /// <summary>Clips de Core, cámara, managers y UI.</summary>
    public class SystemsCaptures
    {
        // =====================================================================
        // Core
        // =====================================================================

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterController2D_Clip()
        {
            var s = new DocStage("CharacterController2D", new Vector2(-1f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("Ledge", new Vector2(-5f, 0.5f), new Vector2(8f, 2f)); // x -9..-1, top y = 1.5
                var p = s.World.Player(new Vector2(-6.5f, 2.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                s.ShowInput(input);
                yield return DocStage.Settle(p);

                // Punto de chequeo de suelo (base del collider), verde en el suelo y rojo en el aire.
                var probe = s.Marker((Vector2)p.transform.position + new Vector2(0f, -0.9f), DocPalette.Good, 0.25f, p.transform)
                    .GetComponent<SpriteRenderer>();
                probe.sortingOrder = 12;
                s.Caption(() =>
                {
                    bool grounded = p.Controller.IsGrounded;
                    probe.color = grounded ? DocPalette.Good : DocPalette.Enemy;
                    return grounded ? "IsGrounded: true" : "IsGrounded: false";
                });

                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 2.5f, 3f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.4f);
                input.Jump = true; yield return DocStage.Seconds(0.25f);
                input.Jump = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterCore_Clip()
        {
            var s = new DocStage("CharacterCore", new Vector2(0f, 3f), 10f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-7f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerDash));
                s.ShowInput(input);
                s.Title("PlayerWalkRun + PlayerJump + PlayerDash");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);   // walk
                input.Run = true; yield return DocStage.Seconds(0.4f);             // run
                input.Jump = true; yield return DocStage.Seconds(0.3f);            // jump
                input.Jump = false;
                input.TapDash(); yield return DocStage.Seconds(0.5f);              // dash in the air
                input.Run = false; input.Move = Vector2.zero;
                yield return Until(() => p.Controller.IsGrounded, 1.5f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // =====================================================================
        // Camera (the capture camera itself carries the component)
        // =====================================================================

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CameraFollow_Clip()
        {
            var s = new DocStage("CameraFollow", new Vector2(-6f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                Pillars(s, -15f, 30f, 3f);
                var p = s.World.Player(new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                s.ShowInput(input);

                var follow = s.Camera.gameObject.AddComponent<CameraFollow>();
                TestWorld.Set(follow, "offset", new Vector3(0f, 2f, -10f));
                follow.SetTarget(p.transform);
                yield return DocStage.Settle(p);

                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Run = true; yield return DocStage.Seconds(0.5f);
                input.Jump = true; yield return DocStage.Seconds(0.3f);
                input.Jump = false; yield return DocStage.Seconds(0.9f);
                input.Run = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CameraBounds_Clip()
        {
            var s = new DocStage("CameraBounds", new Vector2(0f, 3f), 9f);
            try
            {
                s.World.Ground(-0.5f, 22f);                                            // x -11..11
                s.World.Block("WallL", new Vector2(-10.5f, 4f), new Vector2(1f, 10f)); // inner face x = -10
                s.World.Block("WallR", new Vector2(10.5f, 4f), new Vector2(1f, 10f));  // inner face x = 10
                Pillars(s, -8f, 8f, 2f);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                var cam = s.Camera.gameObject;
                var follow = cam.AddComponent<CameraFollow>();
                TestWorld.Set(follow, "offset", new Vector3(0f, 2f, -10f));
                follow.SetTarget(p.transform);
                var bounds = cam.AddComponent<CameraBounds>();
                TestWorld.Set(bounds, "minBounds", new Vector2(-11f, -1.5f));
                TestWorld.Set(bounds, "maxBounds", new Vector2(11f, 9f));
                s.Caption(() => $"Camera X {s.Camera.transform.position.x:0.0}");
                yield return DocStage.Settle(p);

                s.StartRecording(5.8f);
                yield return DocStage.Seconds(0.3f);
                input.Run = true; input.Move = Vector2.left;
                yield return Until(() => p.transform.position.x <= -9.4f, 2.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.4f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 9.4f, 4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.zero; input.Run = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CameraShake_Clip()
        {
            var s = new DocStage("CameraShake", new Vector2(0f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-3f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                s.ShowInput(input);
                var shake = s.Camera.gameObject.AddComponent<CameraShake>();
                string label = "";
                float labelUntil = -1f;
                s.Caption(() => Time.time < labelUntil ? label : "");
                yield return DocStage.Settle(p);

                s.StartRecording(4.2f);
                yield return DocStage.Seconds(0.4f);

                // Una roca cae y al tocar el suelo: sacudida fuerte con parámetros propios.
                var rock = s.World.Track(new GameObject("Boulder"));
                rock.transform.position = new Vector3(2.5f, 7.8f, 0f);
                rock.AddComponent<CircleCollider2D>().radius = 0.7f;
                var rockBody = rock.AddComponent<Rigidbody2D>();
                rockBody.gravityScale = 2f;
                rockBody.mass = 5f;
                yield return Until(() => rockBody.linearVelocity.y < -1f, 1f);
                yield return Until(() => rockBody.linearVelocity.y > -0.2f, 2f);
                shake.Shake(0.4f, 0.35f);
                label = "Shake(0.4, 0.35)"; labelUntil = Time.time + 0.8f;
                s.Flash(new Vector2(2.5f, -0.3f), new Vector2(2.4f, 0.4f), DocPalette.Hex("FFD54F", 0.5f), 0.15f);
                yield return DocStage.Seconds(1.0f);

                // Un salto y al aterrizar: sacudida suave con los valores por defecto.
                input.Jump = true; yield return DocStage.Seconds(0.25f);
                input.Jump = false;
                yield return Until(() => !p.Controller.IsGrounded, 0.5f);
                yield return Until(() => p.Controller.IsGrounded, 2f);
                shake.Shake();
                label = "Shake()"; labelUntil = Time.time + 0.8f;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CameraZoomBySpeed_Clip()
        {
            var s = new DocStage("CameraZoomBySpeed", new Vector2(-6f, 2f), 10f); // orthographicSize 5 = Min Zoom
            try
            {
                s.World.Block("Ground", new Vector2(0f, -5.5f), new Vector2(200f, 10f)); // grueso: rellena al alejar el zoom
                Pillars(s, -16f, 34f, 3f);
                var p = s.World.Player(new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerDash));
                s.ShowInput(input);

                var cam = s.Camera;
                var follow = cam.gameObject.AddComponent<CameraFollow>();
                TestWorld.Set(follow, "offset", new Vector3(0f, 2f, -10f));
                follow.SetTarget(p.transform);
                var body = p.Controller.Rigidbody;
                var zoom = cam.gameObject.AddComponent<CameraZoomBySpeed>();
                TestWorld.Set(zoom, "target", body);
                s.Caption(() => $"Speed {body.linearVelocity.magnitude:0.0}   Size {cam.orthographicSize:0.0}");
                yield return DocStage.Settle(p);

                s.StartRecording(5f);
                yield return DocStage.Seconds(0.4f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.2f);
                input.Run = true; yield return DocStage.Seconds(1.0f);
                input.TapDash(); yield return DocStage.Seconds(0.6f);
                input.Run = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // =====================================================================
        // Managers
        // =====================================================================

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator ScoreManager_Clip()
        {
            var s = new DocStage("ScoreManager", new Vector2(2f, 2.5f), 9f);
            try
            {
                var score = s.World.Track(new GameObject("Managers")).AddComponent<ScoreManager>();
                s.World.Ground();
                Coin(s, new Vector2(-2.5f, 0.2f), 1);
                Coin(s, new Vector2(-1.5f, 0.2f), 1);
                Coin(s, new Vector2(-0.5f, 0.2f), 1);
                Coin(s, new Vector2(2.2f, 2.3f), 5);
                Coin(s, new Vector2(7.3f, 0.2f), 1);
                Coin(s, new Vector2(8.1f, 0.2f), 1);
                var p = s.World.Player(new Vector2(-4.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                p.gameObject.tag = "Player";
                s.ShowInput(input);
                s.Caption(() => $"Score {score.CurrentScore}");
                yield return DocStage.Settle(p);

                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 1f, 2.5f);
                input.Jump = true; yield return DocStage.Seconds(0.12f);
                input.Jump = false;
                yield return Until(() => p.transform.position.x >= 8.6f, 3f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PauseManager_Clip()
        {
            var s = new DocStage("PauseManager", new Vector2(0f, 2.5f), 9f);
            try
            {
                var pause = CreatePauseManager(s);
                s.World.Ground();
                Ball(s, new Vector2(4.6f, 5f));
                Ball(s, new Vector2(6.6f, 3.2f));
                var p = s.World.Player(new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.Caption(() => pause.IsPaused ? "Paused" : "Playing");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.1f);
                pause.SetPaused(true);              // lo mismo que pulsar Esc
                yield return DocStage.Frames(30);   // timeScale = 0: se esperan cuadros, no segundos
                pause.SetPaused(false);
                yield return DocStage.Seconds(1.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator LevelManager_Clip()
        {
            var s = new DocStage("LevelManager", new Vector2(-1.5f, 0.8f), 10f);
            try
            {
                var level = s.World.Track(new GameObject("LevelManager")).AddComponent<LevelManager>();
                TestWorld.Set(level, "voidY", -3.5f);
                s.World.Block("GroundL", new Vector2(-6f, -1f), new Vector2(10f, 1f)); // x -11..-1, top y = -0.5
                s.World.Block("GroundR", new Vector2(8f, -1f), new Vector2(10f, 1f));  // x 3..13 (pit in between)
                s.World.Block("Checkpoint", new Vector2(-4f, 0.5f), new Vector2(1f, 2f), true).AddComponent<Checkpoint>();
                s.Line(DocPalette.Hex("FF1744", 0.8f), 0.06f, new Vector2(-14f, -3.5f), new Vector2(12f, -3.5f));
                s.Title("Void Y = -3.5");

                var go = s.PlayerShell(new Vector2(-7f, 0.5f), out var input);
                go.tag = "Player";
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<CharacterDeath>();
                var respawn = go.AddComponent<CharacterRespawn>();
                TestWorld.Set(respawn, "respawnDelay", 0.6f);
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => health.IsDead ? "Dead" : $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}");
                yield return DocStage.Settle(p);

                s.StartRecording(4.8f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;                                   // pasa por el checkpoint
                yield return Until(() => p.transform.position.x >= 0.8f, 3f);
                input.Move = Vector2.zero;                                    // cae recto al foso
                yield return Until(() => health.IsDead, 2f);                  // bajo Void Y: muere
                yield return Until(() => !health.IsDead, 2f);                 // reaparece en el checkpoint
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator InventoryManager_Clip()
        {
            var s = new DocStage("InventoryManager", new Vector2(-2f, 2f), 9f);
            try
            {
                s.World.Ground();
                var keyGo = s.World.Track(new GameObject("Key"));
                keyGo.transform.position = new Vector2(-7f, 0.2f);
                var keyCollider = keyGo.AddComponent<CircleCollider2D>();
                keyCollider.radius = 0.3f;
                keyCollider.isTrigger = true;
                TestWorld.Set(keyGo.AddComponent<ItemPickup>(), "itemId", "gold_key");

                var doorGo = s.World.Block("Door", new Vector2(0f, 1f), new Vector2(1.5f, 3f), true);
                var blocker = s.World.Block("DoorBlocker", new Vector2(0f, 1f), new Vector2(0.5f, 3f));
                var door = doorGo.AddComponent<DoorWithKey>();
                TestWorld.Set(door, "requiredItemId", "gold_key");
                TestWorld.Set(door, "blockingCollider", blocker.GetComponent<Collider2D>());
                s.Tint(doorGo, DocPalette.Trigger);
                s.Tint(blocker, DocPalette.Hex("8D6E63"));

                var go = s.PlayerShell(new Vector2(-3f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var inventory = go.AddComponent<InventoryManager>();
                var p = s.Activate(go);
                s.ShowInput(input);
                var items = new List<string>();
                inventory.OnItemAdded += id => items.Add(id);
                inventory.OnItemRemoved += id => items.Remove(id);
                s.Caption(() => items.Count == 0 ? "Items: -" : "Items: " + string.Join(", ", items));
                yield return DocStage.Settle(p);

                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.9f);   // sin llave: la puerta bloquea
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.25f);
                input.Run = true; input.Move = Vector2.left;                      // a buscar la llave
                yield return Until(() => inventory.HasItem("gold_key"), 2f);
                yield return DocStage.Seconds(0.1f);
                input.Move = Vector2.right;                                       // con llave: abre y la consume
                yield return Until(() => p.transform.position.x >= 3f, 3f);
                input.Move = Vector2.zero; input.Run = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator GameManager_Clip()
        {
            var s = new DocStage("GameManager", new Vector2(-1f, 2.5f), 9f);
            try
            {
                var gm = s.World.Track(new GameObject("GameManager")).AddComponent<GameManager>();
                s.World.Ground();
                s.World.Block("Spikes", new Vector2(1.5f, -0.25f), new Vector2(2f, 0.5f), true).AddComponent<HazardZone>();

                var go = s.PlayerShell(new Vector2(-4f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                TestWorld.Set(health, "maxHealth", 20f);
                TestWorld.Set(health, "invulnerabilityDuration", 0.2f);
                var lives = go.AddComponent<CharacterLives>();
                TestWorld.Set(lives, "startingLives", 2);
                go.AddComponent<CharacterDeath>();
                var respawn = go.AddComponent<CharacterRespawn>();
                TestWorld.Set(respawn, "respawnDelay", 0.5f);
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"Lives {lives.Lives}   {gm.CurrentState}");
                yield return DocStage.Settle(p);

                s.StartRecording(5.8f);
                yield return DocStage.Seconds(0.3f);
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    input.Move = Vector2.right;
                    yield return Until(() => p.transform.position.x >= 1.3f, 2.5f);
                    input.Move = Vector2.zero;
                    yield return Until(() => health.IsDead, 2f);
                    if (attempt == 0)
                    {
                        yield return Until(() => !health.IsDead, 2f); // reaparece con una vida menos
                        yield return DocStage.Seconds(0.3f);
                    }
                }
                yield return s.WaitRecording();                       // sin vidas: GameOver
            }
            finally { s.Dispose(); }
        }

        // =====================================================================
        // UI
        // =====================================================================

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator UIHealthBar_Clip()
        {
            var s = new DocStage("UIHealthBar", new Vector2(0f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("Spikes", new Vector2(-1f, -0.25f), new Vector2(2f, 0.5f), true).AddComponent<HazardZone>();
                Heart(s, new Vector2(3f, 0.3f));

                var go = s.PlayerShell(new Vector2(-5f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}");

                // Barra con Slider (como la arma el HUD del kit, pero en un Canvas que la cámara de captura ve).
                var canvas = MakeCanvas(s, "HUD");
                var bar = Rect("HealthBar", canvas.transform, new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(320f, 28f));
                bar.gameObject.SetActive(false);
                AddImage(bar.gameObject, new Color(0f, 0f, 0f, 0.6f));
                var area = Rect("Fill Area", bar, Vector2.zero, Vector2.zero, Vector2.zero);
                Stretch(area, 4f);
                var fill = Rect("Fill", area, Vector2.zero, Vector2.zero, Vector2.zero);
                Stretch(fill, 0f);
                AddImage(fill.gameObject, DocPalette.Good);
                var slider = bar.gameObject.AddComponent<Slider>();
                slider.transition = Selectable.Transition.None;
                slider.interactable = false;
                slider.fillRect = fill;
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 1f;
                var healthBar = bar.gameObject.AddComponent<UIHealthBar>();
                TestWorld.Set(healthBar, "health", health);
                TestWorld.Set(healthBar, "slider", slider);
                bar.gameObject.SetActive(true);
                yield return DocStage.Settle(p);

                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.9f);  // a los pinchos
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.5f);   // la barra baja
                input.Move = Vector2.right;                                      // al corazón: la barra sube
                yield return Until(() => p.transform.position.x >= 4f, 2.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator UIDamagePopup_Clip()
        {
            var s = new DocStage("UIDamagePopup", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Spikes", new Vector2(-3f, -0.25f), new Vector2(1.5f, 0.5f), true).AddComponent<HazardZone>();
                Heart(s, new Vector2(0f, 0.3f));
                var strong = s.World.Block("Spikes", new Vector2(3f, -0.25f), new Vector2(1.5f, 0.5f), true).AddComponent<HazardZone>();
                TestWorld.Set(strong, "damage", 25f);

                var go = s.PlayerShell(new Vector2(-6f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<DamageFlash>();
                go.AddComponent<DamagePopupSpawner>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}");
                yield return DocStage.Settle(p);

                s.StartRecording(4.2f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 5.5f, 3.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator DamagePopupSpawner_Clip()
        {
            var s = new DocStage("DamagePopupSpawner", new Vector2(-1f, 2f), 8f);
            try
            {
                s.World.Ground();
                var dummy = s.World.Block("Dummy", new Vector2(1f, 0.5f), new Vector2(1f, 2f)); // x 0.5..1.5
                dummy.SetActive(false);
                var target = dummy.AddComponent<DamageableObject>();
                TestWorld.Set(target, "maxHealth", 100f);
                TestWorld.Set(target, "destroyOnDeath", false);
                dummy.AddComponent<DamagePopupSpawner>();
                dummy.SetActive(true);

                var go = s.PlayerShell(new Vector2(-4f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                go.AddComponent<PlayerAttack>();
                var weapon = go.AddComponent<WeaponMelee>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"HP {target.CurrentHealth:0}/{target.MaxHealth:0}");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.1f);   // hasta el muñeco
                input.Move = Vector2.zero;
                for (int i = 0; i < 3; i++)
                {
                    if (i == 2) TestWorld.Set(weapon, "damage", 25f);             // el último golpe pega más fuerte
                    input.TapAction(CharacterAction.Attack);
                    s.Flash((Vector2)p.transform.position + new Vector2(0.75f, 0f), new Vector2(1.5f, 1.5f),
                        DocPalette.Hex("FFD54F", 0.35f), 0.15f);
                    yield return DocStage.Seconds(0.55f);
                }
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator UIScoreText_Clip()
        {
            var s = new DocStage("UIScoreText", new Vector2(0.5f, 2.5f), 9f);
            try
            {
                s.World.Track(new GameObject("Managers")).AddComponent<ScoreManager>();
                s.World.Ground();
                for (int i = 0; i < 7; i++) Coin(s, new Vector2(-2f + i, 0.2f), 1);
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun));
                p.gameObject.tag = "Player";
                s.ShowInput(input);

                var canvas = MakeCanvas(s, "HUD");
                var label = Rect("Score", canvas.transform, new Vector2(0f, 1f), new Vector2(24f, -16f), new Vector2(500f, 60f));
                label.gameObject.SetActive(false);
                var text = AddText(label.gameObject, "", 40, TextAnchor.UpperLeft);
                text.fontStyle = FontStyle.Bold;
                var scoreText = label.gameObject.AddComponent<UIScoreText>();
                TestWorld.Set(scoreText, "format", "Coins: {0}");
                label.gameObject.SetActive(true);
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 5.5f, 3.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator UIPauseMenu_Clip()
        {
            var s = new DocStage("UIPauseMenu", new Vector2(0f, 2.5f), 9f);
            try
            {
                var pause = CreatePauseManager(s);
                s.World.Ground();
                Ball(s, new Vector2(4.6f, 5f));
                Ball(s, new Vector2(6.6f, 3.2f));
                var p = s.World.Player(new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun));

                var canvas = MakeCanvas(s, "HUD");
                var panel = Rect("PausePanel", canvas.transform, Vector2.zero, Vector2.zero, Vector2.zero);
                Stretch(panel, 0f);
                AddImage(panel.gameObject, new Color(0f, 0f, 0f, 0.6f));
                var title = Rect("Title", panel, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(600f, 90f));
                AddText(title.gameObject, "Paused", 64, TextAnchor.MiddleCenter);
                var resume = MenuButton(panel, "Resume", new Vector2(0f, -10f));
                MenuButton(panel, "Restart", new Vector2(0f, -80f));
                panel.gameObject.SetActive(false);
                var menu = canvas.gameObject.AddComponent<UIPauseMenu>();
                TestWorld.Set(menu, "menuRoot", panel.gameObject);
                yield return DocStage.Settle(p);

                s.StartRecording(3.8f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                pause.SetPaused(true);                         // como Esc: UIPauseMenu muestra el panel
                yield return DocStage.Frames(24);
                var normal = resume.color;
                resume.color = DocPalette.Accent;              // "clic" en Reanudar
                yield return DocStage.Frames(8);
                menu.OnResumeButtonPressed();                  // oculta el panel y reanuda
                resume.color = normal;
                yield return DocStage.Seconds(1.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static IEnumerator Until(System.Func<bool> condition, float timeout)
        {
            for (float t = 0f; !condition() && t < timeout; t += Time.deltaTime) yield return null;
        }

        /// <summary>Columnas translúcidas (triggers sin efecto) para que se note el movimiento de la cámara.</summary>
        private static void Pillars(DocStage s, float fromX, float toX, float step)
        {
            int i = 0;
            for (float x = fromX; x <= toX; x += step, i++)
            {
                float h = i % 2 == 0 ? 3f : 4.5f;
                s.World.Block("Pillar", new Vector2(x, -0.5f + h * 0.5f), new Vector2(0.5f, h), true);
            }
        }

        private static void Coin(DocStage s, Vector2 position, int value)
        {
            var go = s.World.Track(new GameObject(value > 1 ? "Gem" : "Coin"));
            go.transform.position = position;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = value > 1 ? 0.35f : 0.25f;
            col.isTrigger = true;
            var collectible = go.AddComponent<Collectible>();
            if (value != 1) TestWorld.Set(collectible, "value", value);
            if (value > 1) s.Tint(go, DocPalette.Hex("E040FB"));
        }

        private static void Heart(DocStage s, Vector2 position)
        {
            var go = s.World.Track(new GameObject("Heart"));
            go.transform.position = position;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;
            go.AddComponent<HealthPickup>();
        }

        private static void Ball(DocStage s, Vector2 position)
        {
            var go = s.World.Track(new GameObject("Ball"));
            go.transform.position = position;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.sharedMaterial = new PhysicsMaterial2D("DocBouncy") { bounciness = 0.9f, friction = 0f };
            go.AddComponent<Rigidbody2D>();
        }

        private static PauseManager CreatePauseManager(DocStage s)
        {
            var go = s.World.Track(new GameObject("PauseManager"));
            go.SetActive(false);
            var pause = go.AddComponent<PauseManager>();
            // Sin tecla ni botón: solo se pausa desde el escenario (el teclado real no interviene).
            TestWorld.Set(pause, "pauseKey", KeyCode.None);
            TestWorld.Set(pause, "pausePadButton", InputCompat.PadButton.None);
            go.SetActive(true);
            return pause;
        }

        /// <summary>Canvas Screen Space - Camera sobre la cámara de captura (los Overlay no se graban).</summary>
        private static Canvas MakeCanvas(DocStage s, string name)
        {
            var go = s.World.Track(new GameObject(name, typeof(RectTransform)));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = s.Camera;
            canvas.planeDistance = 1f;
            canvas.sortingOrder = 50;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(DocStage.Width, DocStage.Height);
            return canvas;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static void Stretch(RectTransform rt, float padding)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        private static Image AddImage(GameObject go, Color color)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static UnityEngine.UI.Text AddText(GameObject go, string content, int size, TextAnchor alignment)
        {
            var text = go.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return text;
        }

        private static Image MenuButton(RectTransform panel, string label, Vector2 position)
        {
            var rt = Rect(label, panel, new Vector2(0.5f, 0.5f), position, new Vector2(240f, 52f));
            var image = AddImage(rt.gameObject, DocPalette.Hex("37474F"));
            var textRt = Rect("Label", rt, Vector2.zero, Vector2.zero, Vector2.zero);
            Stretch(textRt, 0f);
            AddText(textRt.gameObject, label, 30, TextAnchor.MiddleCenter);
            return image;
        }
    }
}
