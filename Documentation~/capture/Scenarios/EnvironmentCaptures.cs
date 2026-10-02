using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using GameplayKit.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameplayKit.DocCapture
{
    /// <summary>IInteractable propio para el clip de PlayerInteract: un cofre que se abre (cambia de color) una vez.</summary>
    public class EnvDocChest : MonoBehaviour, IInteractable
    {
        public bool Opened;

        public void Interact(GameObject interactor)
        {
            if (Opened) return;
            Opened = true;
            var visual = GetComponentInChildren<DocVisual>();
            if (visual != null) visual.SetColor(DocPalette.Good);
        }
    }

    public class EnvironmentCaptures
    {
        private static readonly Color LeverOff = DocPalette.Hex("FF9800");
        private static readonly Color PlateIdle = DocPalette.Hex("FFB300");
        private static readonly Color Wood = DocPalette.Hex("8D6E63");

        private static CharacterCore TaggedPlayer(DocStage s, Vector2 position, out ScriptedCharacterInput input, params System.Type[] abilities)
        {
            var p = s.World.Player(position, out input, abilities);
            p.gameObject.tag = "Player";
            return p;
        }

        private static void FlashMelee(DocStage s, CharacterCore p)
        {
            float facing = Mathf.Sign(p.transform.localScale.x);
            s.Flash((Vector2)p.transform.position + new Vector2(0.75f * facing, 0f), new Vector2(1.5f, 1.5f), DocPalette.Hex("FFEB3B", 0.35f), 0.15f);
        }

        // ---------------------------------------------------------------- BreakableObject

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator BreakableObject_Clip()
        {
            var s = new DocStage("BreakableObject", new Vector2(-0.5f, 2f), 8f);
            try
            {
                s.World.Ground();
                // Plantilla del loot, fuera de cámara: BreakableObject instancia una copia donde se rompe la caja.
                var coinTemplate = s.World.Block("Coin", new Vector2(60f, 60f), new Vector2(0.4f, 0.4f), true);
                coinTemplate.AddComponent<Collectible>();

                var crate = s.World.Block("Crate", new Vector2(-1.9f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                TestWorld.Set(crate, "lootDrops", new[] { coinTemplate });
                var bigCrate = s.World.Block("BigCrate", new Vector2(1.6f, 0.25f), new Vector2(1f, 1.5f)).AddComponent<BreakableObject>();
                TestWorld.Set(bigCrate, "health", 30f);

                var p = TaggedPlayer(s, new Vector2(-3f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();
                s.ShowInput(input);
                var hpField = typeof(BreakableObject).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);
                s.Caption(() => bigCrate != null ? $"Crate HP {(float)hpField.GetValue(bigCrate):0}" : "Crate HP 0");

                yield return DocStage.Settle(p);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.4f);
                input.TapAction(CharacterAction.Attack); FlashMelee(s, p);
                yield return DocStage.Seconds(0.15f);
                var loot = GameObject.Find("Coin(Clone)");
                if (loot != null) s.World.Track(loot);
                yield return DocStage.Seconds(0.45f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.25f);
                for (int i = 0; i < 3; i++)
                {
                    input.TapAction(CharacterAction.Attack); FlashMelee(s, p);
                    yield return DocStage.Seconds(0.5f);
                }
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Checkpoint

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator Checkpoint_Clip()
        {
            var s = new DocStage("Checkpoint", new Vector2(-1.5f, 2f), 8f);
            try
            {
                s.World.Ground();
                var cpGo = s.World.Block("Checkpoint", new Vector2(-3f, 1f), new Vector2(0.4f, 3f), true);
                var flag = s.Marker(new Vector2(-3f, 2.8f), DocPalette.Good, 0.6f);
                flag.SetActive(false);
                var checkpoint = cpGo.AddComponent<Checkpoint>();
                TestWorld.Set(checkpoint, "activeVisual", flag);

                var spikes = s.World.Block("Spikes", new Vector2(2f, -0.25f), new Vector2(1.2f, 0.5f), true).AddComponent<HazardZone>();
                TestWorld.Set(spikes, "damage", 100f);

                var p = TaggedPlayer(s, new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun));
                var health = p.gameObject.AddComponent<CharacterHealth>();
                var respawn = p.gameObject.AddComponent<CharacterRespawn>();
                TestWorld.Set(respawn, "respawnDelay", 0.8f);
                s.ShowInput(input);
                s.Caption(() => (checkpoint.IsActivated ? "Checkpoint ✓   " : "") + $"HP {health.CurrentHealth:0}");

                yield return DocStage.Settle(p);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                for (float t = 0f; !health.IsDead && t < 3f; t += Time.deltaTime) yield return null;
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(1.5f);   // reaparece en el checkpoint a los 0.8 s
                input.Move = Vector2.right; yield return DocStage.Seconds(0.4f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Collectible

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator Collectible_Clip()
        {
            int score = 0;
            System.Action<Collectible, int> onAny = (c, v) => score += v;
            Collectible.OnAnyCollected += onAny;
            var s = new DocStage("Collectible", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                foreach (float x in new[] { -3f, -2f, -1f, 3f, 4f })
                    s.World.Block("Coin", new Vector2(x, 0.4f), new Vector2(0.4f, 0.4f), true).AddComponent<Collectible>();
                var gem = s.World.Block("Gem", new Vector2(1.5f, 2.3f), new Vector2(0.6f, 0.6f), true).AddComponent<Collectible>();
                TestWorld.Set(gem, "value", 5);

                var p = TaggedPlayer(s, new Vector2(-5.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                s.ShowInput(input);
                s.Caption(() => $"Score {score}");

                yield return DocStage.Settle(p);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.75f);   // justo debajo de la gema
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.1f);
                input.Jump = true; yield return DocStage.Seconds(0.12f);
                input.Jump = false; yield return DocStage.Seconds(1.35f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally
            {
                Collectible.OnAnyCollected -= onAny;
                s.Dispose();
            }
        }

        // ---------------------------------------------------------------- ConveyorBelt

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator ConveyorBelt_Clip()
        {
            var s = new DocStage("ConveyorBelt", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Block("Ground", new Vector2(-7.5f, -1f), new Vector2(5f, 1f));
                s.World.Block("Ground", new Vector2(7.5f, -1f), new Vector2(5f, 1f));
                s.World.Block("Belt", new Vector2(0f, -1f), new Vector2(10f, 1f)).AddComponent<ConveyorBelt>();
                for (int i = 0; i < 5; i++)
                {
                    float x = -4f + i * 2f;
                    var chevron = s.Line(DocPalette.Hex("FFFFFF", 0.45f), 0.08f,
                        new Vector2(x - 0.2f, -0.75f), new Vector2(x + 0.15f, -1f), new Vector2(x - 0.2f, -1.25f));
                    chevron.sortingOrder = 5;
                }

                var p = s.World.Player(new Vector2(-4.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(1.2f);                                  // quieto: la cinta lo lleva
                input.Move = Vector2.left; yield return DocStage.Seconds(1.5f);       // en contra: avanza lento
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.4f);      // a favor: más rápido
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- DoorWithKey

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator DoorWithKey_Clip()
        {
            var s = new DocStage("DoorWithKey", new Vector2(0.5f, 2f), 8f);
            try
            {
                s.World.Ground();
                var doorGo = s.World.Block("Door", new Vector2(3f, 1f), new Vector2(1.5f, 3f), true);
                var door = doorGo.AddComponent<DoorWithKey>();
                var blocker = s.World.Block("DoorBlocker", new Vector2(3f, 1f), new Vector2(0.5f, 3f));
                TestWorld.Set(door, "blockingCollider", blocker.GetComponent<Collider2D>());
                s.Tint(doorGo, DocPalette.Trigger);
                s.Tint(blocker, Wood);
                // ItemPickup y DoorWithKey usan el mismo Item Id por defecto ("llave_dorada").
                s.World.Block("Key", new Vector2(-3f, 0.4f), new Vector2(0.5f, 0.5f), true).AddComponent<ItemPickup>();

                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
                var inventory = p.gameObject.AddComponent<InventoryManager>();
                s.ShowInput(input);
                s.Caption(() => (door.IsOpen ? "Door ✓   " : "") + $"Keys {(inventory.HasItem("llave_dorada") ? 1 : 0)}");

                yield return DocStage.Settle(p);
                s.StartRecording(5.4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);    // choca con la puerta cerrada
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left; yield return DocStage.Seconds(1.4f);      // recoge la llave
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.2f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.1f);     // la puerta se abre al tocarla
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Elevator

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator Elevator_Clip()
        {
            var s = new DocStage("Elevator", new Vector2(2.5f, 2f), 9f);
            try
            {
                s.World.Block("Ground", new Vector2(-4.5f, -1f), new Vector2(11f, 1f));   // x -10..1, arriba en -0.5
                s.World.Block("Ledge", new Vector2(7f, 3.5f), new Vector2(8f, 1f));       // x 3..11, arriba en 4
                var bottom = s.World.Point(new Vector2(2f, -0.75f));
                var top = s.World.Point(new Vector2(2f, 3.75f));
                s.Line(DocPalette.Hex("80CBC4", 0.35f), 0.06f, new Vector2(2f, -0.75f), new Vector2(2f, 3.75f));

                var elevatorGo = s.World.Block("Elevator", new Vector2(2f, -0.75f), new Vector2(2f, 0.5f));
                elevatorGo.SetActive(false);
                elevatorGo.AddComponent<Rigidbody2D>();
                var elevator = elevatorGo.AddComponent<Elevator>();
                TestWorld.Set(elevator, "bottomPoint", bottom.transform);
                TestWorld.Set(elevator, "topPoint", top.transform);
                elevatorGo.SetActive(true);

                var leverGo = s.World.Block("Lever", new Vector2(0.5f, 0f), new Vector2(0.3f, 1f), true);
                var lever = leverGo.AddComponent<Lever>();
                lever.OnToggled += on =>
                {
                    elevator.Activate();
                    s.Tint(leverGo, on ? DocPalette.Good : LeverOff);
                };

                var p = s.World.Player(new Vector2(-1.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerInteract));
                s.ShowInput(input);
                s.Caption(() => elevator.IsMoving ? "Moving" : "Idle");

                yield return DocStage.Settle(p);
                s.StartRecording(5.3f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);     // sube al elevador junto a la palanca
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.25f);
                input.TapInteract(); yield return DocStage.Seconds(2.5f);            // 4.5 u a 2 u/s
                input.Move = Vector2.right; yield return DocStage.Seconds(0.9f);     // baja en la cornisa
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- FallingPlatform

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator FallingPlatform_Clip()
        {
            var s = new DocStage("FallingPlatform", new Vector2(0f, 1f), 8f);
            try
            {
                s.World.Block("LedgeL", new Vector2(-5f, -0.5f), new Vector2(6f, 1f));   // x -8..-2, arriba en 0
                s.World.Block("LedgeR", new Vector2(5f, -0.5f), new Vector2(8f, 1f));    // x 1..9
                foreach (float x in new[] { -1.25f, 0.25f })
                {
                    var go = s.World.Block("Falling", new Vector2(x, -0.25f), new Vector2(1.45f, 0.5f));
                    go.AddComponent<Rigidbody2D>();
                    var falling = go.AddComponent<FallingPlatform>();
                    TestWorld.Set(falling, "destroyDelay", 1.0f);
                    TestWorld.Set(falling, "respawnDelay", 0.8f);
                }

                var p = TaggedPlayer(s, new Vector2(-5f, 1f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.0f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();   // caen, desaparecen y reaparecen en su sitio
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- HealthPickup

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator HealthPickup_Clip()
        {
            var s = new DocStage("HealthPickup", new Vector2(-1.5f, 2f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Heart", new Vector2(-4.5f, 0.2f), new Vector2(0.6f, 0.6f), true).AddComponent<HealthPickup>();
                var spikes = s.World.Block("Spikes", new Vector2(-1.5f, -0.25f), new Vector2(1f, 0.5f), true).AddComponent<HazardZone>();
                TestWorld.Set(spikes, "damage", 40f);
                s.World.Block("Heart", new Vector2(1.5f, 0.2f), new Vector2(0.6f, 0.6f), true).AddComponent<HealthPickup>();

                var p = s.World.Player(new Vector2(-7f, 0.5f), out var input, typeof(PlayerWalkRun));
                var health = p.gameObject.AddComponent<CharacterHealth>();
                health.OnDamaged += amount => s.Flash(p.transform.position, new Vector2(1f, 2f), DocPalette.Hex("FF1744", 0.45f), 0.15f);
                health.OnHealed += amount => s.Flash(p.transform.position, new Vector2(1f, 2f), DocPalette.Hex("66BB6A", 0.45f), 0.2f);
                s.ShowInput(input);
                s.Caption(() => $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}");

                yield return DocStage.Settle(p);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.62f);    // sobre el primer corazón con la vida llena
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.7f);      // no se consume
                input.Move = Vector2.right; yield return DocStage.Seconds(1.9f);     // pinchos (-40) y segundo corazón (+25)
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- ItemPickup

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator ItemPickup_Clip()
        {
            var s = new DocStage("ItemPickup", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var key = s.World.Block("Key", new Vector2(-2f, 0.4f), new Vector2(0.5f, 0.5f), true).AddComponent<ItemPickup>();
                TestWorld.Set(key, "itemId", "key");
                var gem = s.World.Block("Gem", new Vector2(1.5f, 0.4f), new Vector2(0.5f, 0.5f), true).AddComponent<ItemPickup>();
                TestWorld.Set(gem, "itemId", "gem");

                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun));
                var inventory = p.gameObject.AddComponent<InventoryManager>();
                var items = new List<string>();
                inventory.OnItemAdded += id => items.Add(id);
                s.ShowInput(input);
                s.Caption(() => "Items: " + (items.Count == 0 ? "-" : string.Join(", ", items)));

                yield return DocStage.Settle(p);
                s.StartRecording(3.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- LevelExit

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator LevelExit_Clip()
        {
            // LevelExit sin Scene Name carga la siguiente escena del build. Si la escena activa tiene una
            // siguiente válida, se activa una escena vacía creada en memoria (buildIndex -1): así Load()
            // solo registra un aviso y nunca se carga una escena durante el clip.
            Scene previous = SceneManager.GetActiveScene();
            Scene temp = default;
            int next = previous.buildIndex + 1;
            if (next > 0 && next < SceneManager.sceneCountInBuildSettings)
            {
                temp = SceneManager.CreateScene("DocCapture_LevelExit");
                SceneManager.SetActiveScene(temp);
            }

            var s = new DocStage("LevelExit", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var exitGo = s.World.Block("Exit", new Vector2(0f, 1f), new Vector2(1f, 3f), true);
                var exit = exitGo.AddComponent<LevelExit>();
                TestWorld.Set(exit, "requiredItemId", "star");
                exit.OnExitReached += () => s.Flash(exitGo.transform.position, new Vector2(1.4f, 3.4f), DocPalette.Hex("00E676", 0.5f), 0.4f);
                var star = s.World.Block("Star", new Vector2(3f, 0.4f), new Vector2(0.5f, 0.5f), true).AddComponent<ItemPickup>();
                TestWorld.Set(star, "itemId", "star");

                var p = TaggedPlayer(s, new Vector2(-4.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                var inventory = p.gameObject.AddComponent<InventoryManager>();
                s.ShowInput(input);
                s.Caption(() => $"Star {(inventory.HasItem("star") ? 1 : 0)}" + (exit.Triggered ? "   Exit ✓" : ""));

                yield return DocStage.Settle(p);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.0f);    // cruza la salida sin la estrella: nada
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.2f);
                input.Move = Vector2.left;                                          // vuelve con la estrella
                for (float t = 0f; !exit.Triggered && t < 1.5f; t += Time.deltaTime) yield return null;
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally
            {
                s.Dispose();
                if (temp.IsValid())
                {
                    if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                    SceneManager.UnloadSceneAsync(temp);
                }
            }
        }

        // ---------------------------------------------------------------- Lever

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator Lever_Clip()
        {
            var s = new DocStage("Lever", new Vector2(1f, 2f), 8f);
            try
            {
                s.World.Ground();
                var leverGo = s.World.Block("Lever", new Vector2(-1.2f, 0f), new Vector2(0.3f, 1f), true);
                var lever = leverGo.AddComponent<Lever>();
                var gate = s.World.Block("Gate", new Vector2(2f, 1f), new Vector2(0.5f, 3f));
                var gateCollider = gate.GetComponent<Collider2D>();
                lever.OnToggled += on =>
                {
                    gateCollider.enabled = !on;
                    s.Tint(leverGo, on ? DocPalette.Good : LeverOff);
                };

                var p = s.World.Player(new Vector2(-2.2f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerInteract));
                s.ShowInput(input);
                s.Caption(() => lever.IsOn ? "ON" : "OFF");

                yield return DocStage.Settle(p);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.4f);
                input.TapInteract(); yield return DocStage.Seconds(0.7f);   // ON: se abre la reja
                input.TapInteract(); yield return DocStage.Seconds(0.7f);   // OFF: se cierra
                input.TapInteract(); yield return DocStage.Seconds(0.5f);   // ON otra vez
                input.Move = Vector2.right; yield return DocStage.Seconds(1.6f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- MovingPlatform

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator MovingPlatform_Clip()
        {
            var s = new DocStage("MovingPlatform", new Vector2(0f, 1.5f), 8f);
            try
            {
                s.World.Block("LedgeL", new Vector2(-7f, -0.75f), new Vector2(4f, 1.5f));   // x -9..-5, arriba en 0
                s.World.Block("LedgeR", new Vector2(7f, -0.75f), new Vector2(4f, 1.5f));    // x 5..9
                var a = s.World.Point(new Vector2(-3.5f, -0.25f));
                var b = s.World.Point(new Vector2(3.5f, -0.25f));
                s.Line(DocPalette.Hex("90A4AE", 0.35f), 0.06f, new Vector2(-3.5f, -0.25f), new Vector2(3.5f, -0.25f));
                s.Marker(new Vector2(-3.5f, -0.25f), DocPalette.Accent, 0.25f);
                s.Marker(new Vector2(3.5f, -0.25f), DocPalette.Accent, 0.25f);

                var platform = s.World.Block("Platform", new Vector2(-3.5f, -0.25f), new Vector2(2.4f, 0.5f));
                platform.SetActive(false);
                platform.AddComponent<Rigidbody2D>();
                var mover = platform.AddComponent<MovingPlatform>();
                TestWorld.Set(mover, "waypoints", new[] { a.transform, b.transform });
                platform.SetActive(true);

                var p = s.World.Player(new Vector2(-3.5f, 1.2f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                for (float t = 0f; platform.transform.position.x < 3.3f && t < 4f; t += Time.deltaTime) yield return null;
                input.Move = Vector2.right; yield return DocStage.Seconds(0.55f);   // baja a la cornisa derecha
                input.Move = Vector2.zero;
                yield return s.WaitRecording();                                       // la plataforma vuelve sola
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- OneWayPlatform

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator OneWayPlatform_Clip()
        {
            var s = new DocStage("OneWayPlatform", new Vector2(0.5f, 2.5f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("OneWay", new Vector2(0f, 2.2f), new Vector2(4f, 0.3f)).AddComponent<OneWayPlatform>();
                var p = s.World.Player(new Vector2(-1f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(4.3f);
                yield return DocStage.Seconds(0.4f);
                input.Jump = true; yield return DocStage.Seconds(0.25f);   // atraviesa desde abajo
                input.Jump = false; yield return DocStage.Seconds(1.3f);   // y queda parado encima
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- PlayerInteract

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerInteract_Clip()
        {
            var s = new DocStage("PlayerInteract", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var leverGo = s.World.Block("Lever", new Vector2(-1f, 0f), new Vector2(0.3f, 1f), true);
                var lever = leverGo.AddComponent<Lever>();
                lever.OnToggled += on => s.Tint(leverGo, on ? DocPalette.Good : LeverOff);
                var chestGo = s.World.Block("Chest", new Vector2(3.2f, -0.1f), new Vector2(1f, 0.8f));
                var chest = chestGo.AddComponent<EnvDocChest>();
                s.Tint(chestGo, Wood);

                var p = s.World.Player(new Vector2(-4.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerInteract));
                s.Marker(p.transform.position, DocPalette.Hex("4FC3F7", 0.12f), 2.4f, p.transform);   // radio de 1.2
                s.ShowInput(input);
                s.Caption(() => $"Lever {(lever.IsOn ? "ON" : "OFF")}   Chest {(chest.Opened ? "✓" : "-")}");

                yield return DocStage.Settle(p);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.3f);
                input.TapInteract(); yield return DocStage.Seconds(0.5f);            // nada en el radio
                input.Move = Vector2.right; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.2f);
                input.TapInteract(); yield return DocStage.Seconds(0.5f);            // palanca
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.2f);
                input.TapInteract();                                                  // cofre (IInteractable propio)
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- PressurePlate

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PressurePlate_Clip()
        {
            var s = new DocStage("PressurePlate", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var plateGo = s.World.Block("Plate", new Vector2(-1f, -0.4f), new Vector2(1.4f, 0.3f), true);
                var plate = plateGo.AddComponent<PressurePlate>();
                var gate = s.World.Block("Gate", new Vector2(3f, 1f), new Vector2(0.5f, 3f));
                var gateCollider = gate.GetComponent<Collider2D>();
                plate.OnPressed += () => { gateCollider.enabled = false; s.Tint(plateGo, DocPalette.Good); };
                plate.OnReleased += () => { gateCollider.enabled = true; s.Tint(plateGo, PlateIdle); };

                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);
                s.Caption(() => plate.IsPressed ? "Pressed" : "Released");

                yield return DocStage.Settle(p);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);   // sobre la placa
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.left; yield return DocStage.Seconds(0.7f);     // se baja: se suelta
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                var crate = s.World.Block("Crate", new Vector2(-1f, 6.5f), new Vector2(0.8f, 0.8f));
                crate.AddComponent<Rigidbody2D>().freezeRotation = true;            // una caja la mantiene pulsada
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- PushableBox

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PushableBox_Clip()
        {
            var s = new DocStage("PushableBox", new Vector2(1f, 2f), 8f);
            try
            {
                s.World.Ground();
                var box = s.World.Block("Box", new Vector2(-1f, 0f), new Vector2(1f, 1f));
                box.AddComponent<Rigidbody2D>().freezeRotation = true;
                box.AddComponent<PushableBox>();
                var p = TaggedPlayer(s, new Vector2(-3.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.6f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left; yield return DocStage.Seconds(0.6f);    // al alejarse la caja se queda
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- SpringPad

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator SpringPad_Clip()
        {
            var s = new DocStage("SpringPad", new Vector2(2f, 6f), 14f);
            try
            {
                s.World.Ground();
                s.World.Block("Spring", new Vector2(-2f, -0.25f), new Vector2(1.4f, 0.5f), true).AddComponent<SpringPad>();
                s.World.Block("Ledge", new Vector2(6.5f, 7.5f), new Vector2(6f, 1f));   // x 3.5..9.5, arriba en 8
                var p = s.World.Player(new Vector2(-6f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(3.3f);   // resorte -> cornisa alta
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Teleporter

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator Teleporter_Clip()
        {
            var s = new DocStage("Teleporter", new Vector2(0.5f, 2f), 8f);
            try
            {
                s.World.Ground();
                var a = s.World.Block("PortalA", new Vector2(-2.5f, 0.5f), new Vector2(1f, 2f), true).AddComponent<Teleporter>();
                var b = s.World.Block("PortalB", new Vector2(4f, 0.5f), new Vector2(1f, 2f), true).AddComponent<Teleporter>();
                TestWorld.Set(a, "destination", b.transform);
                TestWorld.Set(b, "destination", a.transform);
                s.Line(DocPalette.Hex("7C4DFF", 0.3f), 0.06f, new Vector2(-2.5f, 1.7f), new Vector2(0.75f, 3f), new Vector2(4f, 1.7f));

                var p = TaggedPlayer(s, new Vector2(-5.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.2f);   // A -> B, sigue caminando
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.4f);
                input.Move = Vector2.left; yield return DocStage.Seconds(1.05f);   // B -> A
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- WaterZone

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WaterZone_Clip()
        {
            var s = new DocStage("WaterZone", new Vector2(0f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("Water", new Vector2(0f, 1.25f), new Vector2(10f, 3.5f), true).AddComponent<WaterZone>();   // x -5..5, y -0.5..3

                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                var heavy = s.World.Block("Crate", new Vector2(-2.5f, 6.3f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                heavy.freezeRotation = true;                     // masa 1: se hunde despacio
                var light = s.World.Block("Crate", new Vector2(2.5f, 6.3f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                light.freezeRotation = true;
                light.mass = 0.4f;                               // masa 0.4: la flotación gana y queda en la superficie
                var dry = s.World.Block("Crate", new Vector2(6.5f, 6.3f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                dry.freezeRotation = true;                       // fuera del agua, como referencia
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- WindZone2D

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WindZone2D_Clip()
        {
            var s = new DocStage("WindZone2D", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Wind", new Vector2(0f, 1.5f), new Vector2(10f, 4f), true).AddComponent<WindZone2D>();   // x -5..5, y -0.5..3.5
                var streak = DocPalette.Hex("B2EBF2", 0.5f);
                s.Line(streak, 0.05f, new Vector2(-4.2f, 2.9f), new Vector2(-2.8f, 2.9f));
                s.Line(streak, 0.05f, new Vector2(-1.0f, 2.3f), new Vector2(0.4f, 2.3f));
                s.Line(streak, 0.05f, new Vector2(2.0f, 3.0f), new Vector2(3.4f, 3.0f));
                s.Line(streak, 0.05f, new Vector2(-3.0f, 1.4f), new Vector2(-1.6f, 1.4f));
                s.Line(streak, 0.05f, new Vector2(1.2f, 1.2f), new Vector2(2.6f, 1.2f));

                s.StartRecording(4.5f);
                var outside = s.World.Block("Crate", new Vector2(-6.3f, -0.1f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                outside.freezeRotation = true;                   // fuera de la zona: no se mueve
                var inside = s.World.Block("Crate", new Vector2(-4.3f, -0.1f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                inside.freezeRotation = true;                    // dentro: el viento la arrastra
                yield return DocStage.Seconds(1.5f);
                var dropped = s.World.Block("Crate", new Vector2(-3f, 5.6f), new Vector2(0.8f, 0.8f)).AddComponent<Rigidbody2D>();
                dropped.freezeRotation = true;                   // también empuja en el aire
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
