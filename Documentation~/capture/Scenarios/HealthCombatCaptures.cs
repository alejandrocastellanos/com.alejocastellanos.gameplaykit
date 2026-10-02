using System.Collections;
using GameplayKit.AI;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Movement;
using GameplayKit.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.DocCapture
{
    /// <summary>Clips de documentación de las categorías Health y Combat.</summary>
    public class HealthCombatCaptures
    {
        private static readonly Color HitColor = DocPalette.Hex("FFFFFF", 0.35f);

        // ------------------------------------------------------------------ helpers

        /// <summary>Corre <paramref name="each"/> cada frame durante <paramref name="seconds"/> de juego.</summary>
        private static IEnumerator During(float seconds, System.Action each)
        {
            float t = 0f;
            while (t < seconds)
            {
                each();
                yield return null;
                t += Time.deltaTime;
            }
        }

        /// <summary>Muestra el área de un golpe cuerpo a cuerpo (mismo cálculo que WeaponMelee sin Hitbox Origin).</summary>
        private static void SwingFlash(DocStage s, Transform attacker, float radius = 0.75f, float offset = 0.75f, Color? color = null)
        {
            float facing = attacker.lossyScale.x < 0f ? -1f : 1f;
            s.Flash((Vector2)attacker.position + new Vector2(offset * facing, 0f), Vector2.one * radius * 2f, color ?? HitColor, 0.15f);
        }

        /// <summary>Dibuja un instante el rayo de un WeaponHitscan (se engancha a su evento OnShotFired).</summary>
        private static void ShowShots(DocStage s, WeaponHitscan gun)
        {
            gun.OnShotFired += (from, to) =>
            {
                var lr = s.Line(DocPalette.Accent, 0.07f, from, to);
                lr.sortingOrder = 18;
                UnityEngine.Object.Destroy(lr.gameObject, 0.12f);
            };
        }

        /// <summary>Objeto con DamageableObject + DamageFlash (pintado antes de que DamageFlash cachee sus sprites).</summary>
        private static DamageableObject Dummy(DocStage s, string name, Vector2 center, Vector2 size, float maxHealth, bool destroyOnDeath = true)
        {
            var go = s.World.Block(name, center, size);
            go.SetActive(false);
            var dummy = go.AddComponent<DamageableObject>();
            TestWorld.Set(dummy, "maxHealth", maxHealth);
            TestWorld.Set(dummy, "destroyOnDeath", destroyOnDeath);
            s.PaintNow(go);
            go.AddComponent<DamageFlash>();
            go.SetActive(true);
            return dummy;
        }

        private static GameObject Spikes(DocStage s, Vector2 center, float width)
        {
            var spikes = s.World.Block("Spikes", center, new Vector2(width, 0.5f), true);
            spikes.AddComponent<HazardZone>();
            return spikes;
        }

        /// <summary>Gira al personaje (PlayerWalkRun) unos frames hacia la dirección indicada y lo deja quieto.</summary>
        private static IEnumerator Face(ScriptedCharacterInput input, float direction)
        {
            input.Move = new Vector2(direction, 0f);
            yield return null; yield return null;
            input.Move = Vector2.zero;
        }

        private static string Hp(float current, float max) => $"HP {current:0}/{max:0}";

        // ------------------------------------------------------------------ Health

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterHealth_Clip()
        {
            var s = new DocStage("CharacterHealth", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                Spikes(s, new Vector2(-1f, -0.25f), 2f);
                s.World.Block("Heart", new Vector2(3.5f, 0.3f), new Vector2(0.6f, 0.6f), true).AddComponent<HealthPickup>();

                var go = s.PlayerShell(new Vector2(-4f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => Hp(health.CurrentHealth, health.MaxHealth));
                yield return DocStage.Settle(p);

                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.65f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.3f);   // quieto sobre los pinchos
                input.Move = Vector2.right; yield return DocStage.Seconds(1.3f);  // sale y recoge el corazón
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterDeath_Clip()
        {
            var s = new DocStage("CharacterDeath", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                Spikes(s, new Vector2(0.5f, -0.25f), 3f);

                var go = s.PlayerShell(new Vector2(-3f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                TestWorld.Set(health, "maxHealth", 30f);
                TestWorld.Set(health, "invulnerabilityDuration", 0.4f); // un golpe de los pinchos cada 0.5 s
                var death = go.AddComponent<CharacterDeath>();
                TestWorld.Set(death, "disableDelay", 1.5f);
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"{Hp(health.CurrentHealth, health.MaxHealth)}  {p.Condition.CurrentState}");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.3f);   // muere a los 3 golpes
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);  // ya no responde
                input.Move = Vector2.zero;
                yield return s.WaitRecording();                                     // a los 1.5 s se desactiva
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterFallDamage_Clip()
        {
            var s = new DocStage("CharacterFallDamage", new Vector2(-0.5f, 4.75f), 13f);
            try
            {
                s.World.Ground();
                s.World.Block("Ledge", new Vector2(-9f, 9.5f), new Vector2(6f, 1f));  // arriba en y = 10
                s.World.Block("Step", new Vector2(-4f, 7.5f), new Vector2(4f, 1f));   // escalón en y = 8 (caída corta)

                var go = s.PlayerShell(new Vector2(-9.5f, 10.9f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<CharacterFallDamage>();
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => Hp(health.CurrentHealth, health.MaxHealth));
                yield return DocStage.Settle(p);

                s.StartRecording(4.8f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(3.4f);  // cae 2 (sin daño) y luego 8.5 (daño)
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterGravityController_Clip()
        {
            var s = new DocStage("CharacterGravityController", new Vector2(0f, 4f), 10.5f);
            try
            {
                s.World.Ground();
                var plain = s.World.Player(new Vector2(-2f, 0.5f), out var inputA, typeof(PlayerJump));
                var shaped = s.World.Player(new Vector2(2f, 0.5f), out var inputB, typeof(PlayerJump), typeof(CharacterGravityController));
                s.Tint(shaped.gameObject, DocPalette.Good);
                s.ShowInput(inputA);
                s.Caption(() => "Green = GravityController");
                yield return DocStage.Settle(plain);
                yield return DocStage.Settle(shaped);

                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                inputA.Jump = inputB.Jump = true; yield return DocStage.Seconds(2.6f);  // salto completo mantenido
                inputA.Jump = inputB.Jump = false; yield return DocStage.Seconds(0.1f);
                inputA.Jump = inputB.Jump = true; yield return DocStage.Seconds(0.12f); // toque corto
                inputA.Jump = inputB.Jump = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterKnockback_Clip()
        {
            var s = new DocStage("CharacterKnockback", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var enemy = s.World.Block("Enemy", new Vector2(1.5f, -0.05f), new Vector2(0.9f, 0.9f));
                enemy.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                enemy.transform.localScale = new Vector3(-1f, 1f, 1f); // mira hacia el jugador
                enemy.AddComponent<EnemyMeleeOnContact>();

                var go = s.PlayerShell(new Vector2(-2f, 0.5f), out var input);
                go.tag = "Player";
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                go.AddComponent<CharacterKnockback>();
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => Hp(health.CurrentHealth, health.MaxHealth));
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);  // choca y sale despedido
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.9f);   // segundo golpe
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterLives_Clip()
        {
            var s = new DocStage("CharacterLives", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                Spikes(s, new Vector2(0.6f, -0.25f), 1.2f);
                s.World.Block("Wall", new Vector2(1.7f, 1.5f), new Vector2(1f, 4f)); // frena al personaje muerto

                var go = s.PlayerShell(new Vector2(-2f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                TestWorld.Set(health, "maxHealth", 10f);
                go.AddComponent<CharacterDeath>();
                var respawn = go.AddComponent<CharacterRespawn>();
                TestWorld.Set(respawn, "respawnDelay", 0.6f);
                var lives = go.AddComponent<CharacterLives>();
                TestWorld.Set(lives, "startingLives", 3);
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.ShowInput(input);
                s.Caption(() => $"Lives {lives.Lives}  {Hp(health.CurrentHealth, health.MaxHealth)}" + (lives.IsGameOver ? "  GAME OVER" : ""));
                yield return DocStage.Settle(p);

                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;                       // camina a los pinchos una y otra vez
                float t = 0f;
                while (!lives.IsGameOver && t < 3.5f) { t += Time.deltaTime; yield return null; }
                yield return DocStage.Seconds(0.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterRespawn_Clip()
        {
            var s = new DocStage("CharacterRespawn", new Vector2(0.5f, 0.5f), 9f);
            try
            {
                // Suelo con un foso de pinchos entre x = 2 y x = 4.
                s.World.Block("Ground", new Vector2(-4.5f, -2.5f), new Vector2(13f, 4f));
                s.World.Block("Ground", new Vector2(8f, -2.5f), new Vector2(8f, 4f));
                s.World.Block("Ground", new Vector2(3f, -3.75f), new Vector2(2f, 1.5f));
                Spikes(s, new Vector2(3f, -2.75f), 2f);
                s.World.Block("Checkpoint", new Vector2(-2f, 0.75f), new Vector2(0.8f, 2.5f), true).AddComponent<Checkpoint>();

                var go = s.PlayerShell(new Vector2(-5.5f, 0.5f), out var input);
                go.tag = "Player";
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                TestWorld.Set(health, "maxHealth", 10f);
                go.AddComponent<CharacterDeath>();
                var respawn = go.AddComponent<CharacterRespawn>();
                TestWorld.Set(respawn, "respawnDelay", 0.8f);
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                respawn.OnRespawn += () => s.Flash(go.transform.position, new Vector2(1.4f, 2.4f), DocPalette.Hex("66BB6A", 0.45f), 0.3f);
                s.ShowInput(input);
                s.Caption(() => $"{Hp(health.CurrentHealth, health.MaxHealth)}  {p.Condition.CurrentState}");
                yield return DocStage.Settle(p);

                s.StartRecording(5.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(2.3f);   // pasa el checkpoint y cae al foso
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.5f);    // muere y reaparece en el checkpoint
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterStun_Clip()
        {
            var s = new DocStage("CharacterStun", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun));
                var stun = p.gameObject.AddComponent<CharacterStun>();
                var star = s.Marker((Vector2)p.transform.position + new Vector2(0f, 1.3f), DocPalette.Accent, 0.35f, p.transform);
                star.SetActive(false);
                s.ShowInput(input);
                s.Caption(() => p.Condition.CurrentState.ToString());
                yield return DocStage.Settle(p);

                s.StartRecording(3.8f);
                System.Action sync = () => star.SetActive(stun.IsStunned);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.15f);
                stun.Stun();                                                        // 1 s por defecto
                s.Flash(p.transform.position, new Vector2(1.2f, 2.2f), DocPalette.Hex("FFD54F", 0.5f), 0.15f);
                yield return During(0.2f, sync);
                input.Move = Vector2.right;                                         // intenta moverse: no puede
                yield return During(1.6f, sync);                                    // al terminar el aturdimiento camina
                input.Move = Vector2.zero;
                yield return During(0.6f, sync);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator DamageFlash_Clip()
        {
            var s = new DocStage("DamageFlash", new Vector2(0f, 1.5f), 6.5f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();

                // Enemigo con CharacterHealth: se tiñe y parpadea mientras es invulnerable.
                var enemy = s.World.Track(new GameObject("Enemy"));
                enemy.SetActive(false);
                enemy.transform.position = new Vector2(1.2f, 0.4f);
                enemy.transform.localScale = new Vector3(-1f, 1f, 1f);
                enemy.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 1.8f);
                var enemyHealth = enemy.AddComponent<CharacterHealth>();
                s.PaintNow(enemy);
                enemy.AddComponent<DamageFlash>();
                enemy.SetActive(true);

                // Caja con DamageableObject: solo el tinte (no tiene invulnerabilidad).
                var crate = Dummy(s, "Crate", new Vector2(-1.2f, 0f), new Vector2(0.9f, 1f), 100f, false);

                s.ShowInput(input);
                s.Caption(() => $"Enemy {enemyHealth.CurrentHealth:0}  Crate {crate.CurrentHealth:0}");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.9f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.9f);
                yield return Face(input, -1f);
                yield return DocStage.Seconds(0.1f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.8f);
                yield return Face(input, 1f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ------------------------------------------------------------------ Combat

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator DamageableObject_Clip()
        {
            var s = new DocStage("DamageableObject", new Vector2(-0.5f, 1.5f), 6.5f);
            try
            {
                s.World.Ground();
                var crate = Dummy(s, "Crate", new Vector2(0.5f, 0.1f), new Vector2(0.9f, 1.2f), 30f);
                var p = s.World.Player(new Vector2(-3f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();
                crate.OnDestroyedByDamage += () => s.Flash(new Vector2(0.5f, 0.1f), new Vector2(1.3f, 1.6f), DocPalette.Hex("CE93D8", 0.5f), 0.25f);
                s.ShowInput(input);
                s.Caption(() => crate != null ? Hp(crate.CurrentHealth, crate.MaxHealth) : "Destroyed");
                yield return DocStage.Settle(p);

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.62f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                for (int i = 0; i < 3; i++)
                {
                    input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                    yield return DocStage.Seconds(0.55f);
                }
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerAttack_Clip()
        {
            var s = new DocStage("PlayerAttack", new Vector2(1.2f, 1.5f), 7f);
            try
            {
                s.World.Ground();
                s.World.Block("Crate", new Vector2(1.1f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                s.World.Block("Crate", new Vector2(-1.1f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                s.World.Block("Crate", new Vector2(3.6f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();
                s.ShowInput(input);
                yield return DocStage.Settle(p);

                s.StartRecording(3.6f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);   // derecha
                yield return DocStage.Seconds(0.5f);
                yield return Face(input, -1f);
                yield return DocStage.Seconds(0.15f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);   // izquierda
                yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.15f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);   // la caja más lejana
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponMelee_Clip()
        {
            var s = new DocStage("WeaponMelee", new Vector2(0f, 1.5f), 6.5f);
            try
            {
                s.World.Ground();
                s.World.Block("Crate", new Vector2(-1.1f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                var dummy = Dummy(s, "Dummy", new Vector2(1.15f, 0.3f), new Vector2(0.6f, 1.6f), 30f, false);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();
                s.ShowInput(input);
                s.Caption(() => Hp(dummy.CurrentHealth, dummy.MaxHealth));
                yield return DocStage.Settle(p);

                s.StartRecording(3.6f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.6f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.6f);
                yield return Face(input, -1f);                                          // el golpe sale hacia donde mira
                yield return DocStage.Seconds(0.15f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return DocStage.Seconds(0.8f);
                yield return Face(input, 1f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponCombo_Clip()
        {
            var s = new DocStage("WeaponCombo", new Vector2(0.5f, 1.5f), 6.5f);
            try
            {
                s.World.Ground();
                var dummy = Dummy(s, "Dummy", new Vector2(1.3f, 0.3f), new Vector2(0.6f, 1.6f), 100f, false);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
                var go = p.gameObject;
                var hit1 = go.AddComponent<WeaponMelee>();
                var hit2 = go.AddComponent<WeaponMelee>();
                var hit3 = go.AddComponent<WeaponMelee>();
                TestWorld.Set(hit1, "damage", 5f);
                TestWorld.Set(hit2, "damage", 8f);
                TestWorld.Set(hit3, "damage", 15f);
                TestWorld.Set(hit3, "hitboxRadius", 1f);
                TestWorld.Set(hit3, "hitboxOffset", new Vector2(1f, 0f));
                var combo = go.AddComponent<WeaponCombo>();
                TestWorld.Set(combo, "comboHits", new[] { hit1, hit2, hit3 });
                var hits = new[] { hit1, hit2, hit3 };

                s.ShowInput(input);
                s.Caption(() =>
                {
                    string active = "-";
                    for (int i = 0; i < hits.Length; i++) if (hits[i].IsHitboxActive) active = (i + 1).ToString();
                    return $"Hit {active}  {Hp(dummy.CurrentHealth, dummy.MaxHealth)}";
                });
                yield return DocStage.Settle(p);

                System.Action[] flashes =
                {
                    () => SwingFlash(s, p.transform),
                    () => SwingFlash(s, p.transform),
                    () => SwingFlash(s, p.transform, 1f, 1f, DocPalette.Hex("FFD54F", 0.45f)),
                };

                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                for (int i = 0; i < 3; i++)                                // 1-2-3 dentro de la ventana
                {
                    input.TapAction(CharacterAction.Attack); flashes[i]();
                    yield return DocStage.Seconds(0.25f);
                }
                yield return DocStage.Seconds(0.75f);
                for (int i = 0; i < 2; i++)                                // 1-2 ...
                {
                    input.TapAction(CharacterAction.Attack); flashes[i]();
                    yield return DocStage.Seconds(0.25f);
                }
                yield return DocStage.Seconds(0.65f);                       // ... se agota la ventana
                input.TapAction(CharacterAction.Attack); flashes[0]();      // vuelve al golpe 1
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponCharge_Clip()
        {
            var s = new DocStage("WeaponCharge", new Vector2(0.3f, 1.5f), 6.5f);
            try
            {
                s.World.Ground();
                var dummy = Dummy(s, "Dummy", new Vector2(1.15f, 0.3f), new Vector2(0.6f, 1.6f), 100f, false);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
                p.gameObject.AddComponent<WeaponMelee>();
                var charge = p.gameObject.AddComponent<WeaponCharge>();
                float lastHit = 0f;
                charge.OnReleased += damage => lastHit = damage;
                s.ShowInput(input);
                s.Caption(() => $"Charge {charge.ChargeRatio01 * 100f:0}%  Hit {lastHit:0}  {Hp(dummy.CurrentHealth, dummy.MaxHealth)}");
                yield return DocStage.Settle(p);

                // Barra de carga sobre la cabeza.
                Vector2 barStart = (Vector2)p.transform.position + new Vector2(-0.6f, 1.4f);
                s.Line(DocPalette.Hex("FFFFFF", 0.2f), 0.14f, barStart, barStart + new Vector2(1.2f, 0f)).sortingOrder = 11;
                var bar = s.Line(DocPalette.Accent, 0.14f, barStart, barStart);
                bar.sortingOrder = 12;
                System.Action sync = () => bar.SetPosition(1, (Vector3)(barStart + new Vector2(1.2f * charge.ChargeRatio01, 0f)));

                s.StartRecording(4.2f);
                yield return During(0.3f, sync);
                input.Hold(CharacterAction.Attack); yield return null; yield return null;   // toque rápido
                input.Release(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return During(0.8f, sync);
                input.Hold(CharacterAction.Attack);                                       // carga completa
                yield return During(1.6f, sync);
                input.Release(CharacterAction.Attack); SwingFlash(s, p.transform, 0.75f, 0.75f, DocPalette.Hex("FFD54F", 0.5f));
                yield return During(1.2f, sync);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponHitscan_Clip()
        {
            var s = new DocStage("WeaponHitscan", new Vector2(2.5f, 1.5f), 7f);
            try
            {
                s.World.Ground();
                var a = Dummy(s, "Dummy", new Vector2(3f, 0.5f), new Vector2(0.6f, 2f), 45f);
                var b = Dummy(s, "Dummy", new Vector2(6f, 0.5f), new Vector2(0.6f, 2f), 45f);
                var p = s.World.Player(new Vector2(-2f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                var gun = p.gameObject.AddComponent<WeaponHitscan>();
                ShowShots(s, gun);
                s.ShowInput(input);
                s.Caption(() => $"A {(a != null ? a.CurrentHealth : 0f):0}/45  B {(b != null ? b.CurrentHealth : 0f):0}/45");
                yield return DocStage.Settle(p);

                s.StartRecording(3.6f);
                yield return DocStage.Seconds(0.3f);
                for (int i = 0; i < 5; i++)                         // 3 disparos rompen A; los siguientes llegan a B
                {
                    input.TapAction(CharacterAction.Attack);
                    yield return DocStage.Seconds(0.45f);
                }
                yield return Face(input, -1f);                      // hacia el vacío: el rayo llega a Range
                yield return DocStage.Seconds(0.15f);
                input.TapAction(CharacterAction.Attack);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponProjectile_Clip()
        {
            var s = new DocStage("WeaponProjectile", new Vector2(2.5f, 1.5f), 8f);
            try
            {
                s.World.Ground();
                // "Prefab" en runtime: activo pero bajo un padre inactivo, así sus copias nacen activas (Awake corre).
                var templates = s.World.Track(new GameObject("Templates"));
                templates.SetActive(false);
                var bullet = new GameObject("Bullet");
                bullet.transform.SetParent(templates.transform, false);
                bullet.AddComponent<Rigidbody2D>().gravityScale = 0f;
                var bulletCol = bullet.AddComponent<CircleCollider2D>();
                bulletCol.isTrigger = true;
                bulletCol.radius = 0.12f;
                var bulletTemplate = bullet.AddComponent<ProjectileBehaviour>();

                var dummy = Dummy(s, "Dummy", new Vector2(5f, 0.5f), new Vector2(0.6f, 2f), 24f);
                s.World.Block("Wall", new Vector2(8f, 1.5f), new Vector2(0.6f, 5f));
                var p = s.World.Player(new Vector2(-2f, 0.5f), out var input, typeof(PlayerAttack));
                var weapon = p.gameObject.AddComponent<WeaponProjectile>();
                TestWorld.Set(weapon, "projectilePrefab", bulletTemplate);
                s.ShowInput(input);
                s.Caption(() => dummy != null ? Hp(dummy.CurrentHealth, dummy.MaxHealth) : "Destroyed");
                yield return DocStage.Settle(p);

                s.StartRecording(3.4f);
                yield return DocStage.Seconds(0.3f);
                for (int i = 0; i < 4; i++)                       // 3 balas rompen el muñeco; la 4.ª choca con la pared
                {
                    input.TapAction(CharacterAction.Attack);
                    yield return DocStage.Seconds(0.45f);
                }
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator ProjectileBehaviour_Clip()
        {
            var s = new DocStage("ProjectileBehaviour", new Vector2(0f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("WallL", new Vector2(-5f, 4f), new Vector2(0.6f, 3f));
                s.World.Block("WallR", new Vector2(5f, 4f), new Vector2(0.6f, 3f));
                var lamp = s.World.Block("Turret", new Vector2(0f, 4f), new Vector2(0.6f, 0.6f));
                var mortar = s.World.Block("Turret", new Vector2(-6.5f, 0.25f), new Vector2(0.8f, 1.5f));
                var dummy = Dummy(s, "Dummy", new Vector2(0.5f, 0.3f), new Vector2(0.8f, 1.6f), 20f);

                var templates = s.World.Track(new GameObject("Templates"));
                templates.SetActive(false);

                var bolt = new GameObject("Bolt");                 // recto, rebota 2 veces
                bolt.transform.SetParent(templates.transform, false);
                bolt.AddComponent<Rigidbody2D>();
                var boltCol = bolt.AddComponent<CircleCollider2D>();
                boltCol.isTrigger = true;
                boltCol.radius = 0.15f;
                var boltTemplate = bolt.AddComponent<ProjectileBehaviour>();
                TestWorld.Set(boltTemplate, "maxBounces", 2);

                var grenade = new GameObject("Grenade");           // cae con la gravedad
                grenade.transform.SetParent(templates.transform, false);
                grenade.AddComponent<Rigidbody2D>();
                var grenadeCol = grenade.AddComponent<CircleCollider2D>();
                grenadeCol.isTrigger = true;
                grenadeCol.radius = 0.2f;
                var grenadeTemplate = grenade.AddComponent<ProjectileBehaviour>();
                TestWorld.Set(grenadeTemplate, "affectedByGravity", true);

                s.Caption(() => dummy != null ? Hp(dummy.CurrentHealth, dummy.MaxHealth) : "Destroyed");
                yield return DocStage.Seconds(0.2f);

                s.StartRecording(4.2f);
                yield return DocStage.Seconds(0.3f);
                var b = UnityEngine.Object.Instantiate(boltTemplate, lamp.transform.position, Quaternion.identity);
                b.Launch(Vector2.right, 7f, 5f, lamp);
                yield return DocStage.Seconds(0.5f);
                var g = UnityEngine.Object.Instantiate(grenadeTemplate, mortar.transform.position, Quaternion.identity);
                g.Launch(new Vector2(1f, 1.2f), 8f, 10f, mortar);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator WeaponInventorySlot_Clip()
        {
            var s = new DocStage("WeaponInventorySlot", new Vector2(2.5f, 1.5f), 7f);
            try
            {
                s.World.Ground();
                s.World.Block("Crate", new Vector2(1.1f, 0f), new Vector2(0.8f, 1f)).AddComponent<BreakableObject>();
                var dummy = Dummy(s, "Dummy", new Vector2(6f, 0.5f), new Vector2(0.6f, 2f), 20f);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
                var sword = new GameObject("Sword"); sword.transform.SetParent(p.transform, false); sword.AddComponent<WeaponMelee>();
                var gun = new GameObject("Gun"); gun.transform.SetParent(p.transform, false);
                var gunWeapon = gun.AddComponent<WeaponHitscan>();
                ShowShots(s, gunWeapon);
                var slot = p.gameObject.AddComponent<WeaponInventorySlot>();
                TestWorld.Set(slot, "weapons", new[] { sword, gun });

                // Las armas no tienen collider: un trazo visible bajo cada una (se oculta con el arma).
                Vector2 pos = p.transform.position;
                var blade = s.Line(DocPalette.Hex("ECEFF1"), 0.1f, pos + new Vector2(0.3f, 0.3f), pos + new Vector2(0.85f, 0.95f));
                blade.sortingOrder = 12;
                blade.transform.SetParent(sword.transform, true);
                var barrel = s.Line(DocPalette.Hex("FFB300"), 0.18f, pos + new Vector2(0.25f, 0.05f), pos + new Vector2(0.8f, 0.05f));
                barrel.sortingOrder = 12;
                barrel.transform.SetParent(gun.transform, true);

                s.ShowInput(input);
                s.Caption(() => (slot.EquippedIndex == 0 ? "Sword" : slot.EquippedIndex == 1 ? "Gun" : "-")
                    + "  Dummy " + (dummy != null ? dummy.CurrentHealth.ToString("0") : "0"));
                yield return DocStage.Settle(p);

                s.StartRecording(3.6f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);   // espada: rompe la caja
                yield return DocStage.Seconds(0.6f);
                input.TapAction(CharacterAction.Special);                              // cambia a la pistola
                yield return DocStage.Seconds(0.4f);
                input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.5f);
                input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.6f);
                input.TapAction(CharacterAction.Special);                              // vuelve a la espada
                yield return DocStage.Seconds(0.4f);
                input.TapAction(CharacterAction.Attack); SwingFlash(s, p.transform);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator CharacterAimAndOrient_Clip()
        {
            var s = new DocStage("CharacterAimAndOrient", new Vector2(0.5f, 2f), 9f);
            try
            {
                s.World.Ground();
                Dummy(s, "Target", new Vector2(-4f, 3.5f), new Vector2(0.8f, 0.8f), 1000f, false);
                Dummy(s, "Target", new Vector2(4.5f, 2.5f), new Vector2(0.8f, 0.8f), 1000f, false);

                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
                var arm = new GameObject("Arm");
                arm.transform.SetParent(p.transform, false);
                var aim = p.gameObject.AddComponent<CharacterAimAndOrient>();
                TestWorld.Set(aim, "aimMode", CharacterAimAndOrient.AimMode.ClosestTarget);
                TestWorld.Set(aim, "partToRotate", arm.transform);
                var gun = p.gameObject.AddComponent<WeaponHitscan>();
                ShowShots(s, gun);

                // El brazo es invisible: una línea local que gira con él.
                var armLine = s.Line(DocPalette.Accent, 0.14f, Vector2.zero, Vector2.zero);
                armLine.transform.SetParent(arm.transform, false);
                armLine.useWorldSpace = false;
                armLine.SetPosition(0, new Vector3(0.25f, 0f, 0f));
                armLine.SetPosition(1, new Vector3(0.95f, 0f, 0f));
                armLine.sortingOrder = 12;

                s.ShowInput(input);
                yield return DocStage.Settle(p);

                s.StartRecording(4.2f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack);                       // apunta al objetivo más cercano (arriba)
                yield return DocStage.Seconds(0.5f);
                input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.6f); // el más cercano pasa a ser el de la derecha
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.5f);
                input.TapAction(CharacterAction.Attack);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
