using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameplayKit.AI;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    public class GameplayTests
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

        // Los campos [SerializeField] son privados; en un test se asignan como lo haría el Inspector.
        private static void Set(object target, string field, object value)
        {
            var type = target.GetType();
            FieldInfo info = null;
            for (; type != null && info == null; type = type.BaseType)
                info = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(info, $"No existe el campo {field}");
            info.SetValue(target, value);
        }

        private GameObject Point(Vector2 position) { var go = _world.Track(new GameObject("Point")); go.transform.position = position; return go; }

        // ---------------- Vida, muerte y respawn ----------------

        [UnityTest]
        public IEnumerator Health_Damage_Invulnerability_AndDeath()
        {
            var go = _world.Track(new GameObject("Target"));
            var health = go.AddComponent<CharacterHealth>();
            bool died = false;
            health.OnDeath += () => died = true;

            health.ApplyDamage(30f, Vector2.zero, Vector2.zero, null);
            Assert.AreEqual(70f, health.CurrentHealth, 0.01f);
            health.ApplyDamage(30f, Vector2.zero, Vector2.zero, null);
            Assert.AreEqual(70f, health.CurrentHealth, 0.01f, "Durante la invulnerabilidad no recibe daño");

            yield return new WaitForSeconds(0.6f);
            health.ApplyDamage(80f, Vector2.zero, Vector2.zero, null);
            Assert.AreEqual(0f, health.CurrentHealth, 0.01f);
            Assert.IsTrue(health.IsDead && died, "Al llegar a 0 muere y avisa");
        }

        [UnityTest]
        public IEnumerator Death_ThenRespawn_RestoresControl()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            var health = player.gameObject.AddComponent<CharacterHealth>();
            player.gameObject.AddComponent<CharacterDeath>();
            var respawn = player.gameObject.AddComponent<CharacterRespawn>();
            Set(respawn, "respawnDelay", 0.3f);
            yield return new WaitForSeconds(0.3f);
            Vector2 start = player.transform.position;

            player.transform.position = new Vector2(10f, 0.5f);
            health.TakeDamage(1000f);
            Assert.AreEqual(ConditionState.Dead, player.Condition.CurrentState);
            yield return new WaitForSeconds(0.6f);

            Assert.IsFalse(health.IsDead, "Debería haber reaparecido");
            Assert.AreEqual(start.x, player.transform.position.x, 0.5f, "Sin checkpoint, reaparece donde empezó");
            Assert.AreEqual(ConditionState.Normal, player.Condition.CurrentState);

            float x0 = player.transform.position.x;
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.4f);
            Assert.Greater(player.transform.position.x - x0, 0.5f, "Tras reaparecer el jugador debe poder moverse");
        }

        [UnityTest]
        public IEnumerator Checkpoint_BecomesRespawnPoint()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            player.gameObject.tag = "Player";
            var health = player.gameObject.AddComponent<CharacterHealth>();
            var respawn = player.gameObject.AddComponent<CharacterRespawn>();
            Set(respawn, "respawnDelay", 0.2f);
            var checkpoint = _world.Block("Checkpoint", new Vector2(8f, 1f), new Vector2(1f, 3f), trigger: true).AddComponent<Checkpoint>();

            yield return new WaitForSeconds(0.2f);
            player.Controller.Rigidbody.position = new Vector2(8f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(checkpoint.IsActivated, "El jugador (tag Player) debería activar el checkpoint");

            player.Controller.Rigidbody.position = new Vector2(-5f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            health.TakeDamage(1000f);
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(8f, player.transform.position.x, 0.5f, "Debe reaparecer en el checkpoint");
        }

        [UnityTest]
        public IEnumerator HazardZone_KeepsDamagingWhilePlayerStandsStill()
        {
            _world.Ground();
            _world.Block("Spikes", new Vector2(0f, 0.5f), new Vector2(3f, 2f), trigger: true).AddComponent<HazardZone>();
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            var health = player.gameObject.AddComponent<CharacterHealth>();
            Set(health, "invulnerabilityDuration", 0f);

            yield return new WaitForSeconds(2.5f);
            Assert.LessOrEqual(health.CurrentHealth, 60f,
                "Con daño cada 0.5 s, en 2.5 s quieto sobre los pinchos debería recibir al menos 4 golpes");
        }

        // ---------------- Combate ----------------

        [UnityTest]
        public IEnumerator Melee_HitsTargetOncePerSwing_NeverTheAttacker()
        {
            var attacker = _world.Player(new Vector2(0f, 50f), out _);
            attacker.Controller.Rigidbody.gravityScale = 0f;
            var attackerHealth = attacker.gameObject.AddComponent<CharacterHealth>();
            var melee = attacker.gameObject.AddComponent<WeaponMelee>();
            var target = _world.Block("Dummy", new Vector2(0.9f, 50f), new Vector2(0.5f, 1f)).AddComponent<DamageableObject>();
            yield return null;

            Assert.IsTrue(melee.TryAttack());
            Assert.IsFalse(melee.TryAttack(), "En cooldown no debería atacar de nuevo");
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(10f, target.CurrentHealth, 0.01f, "Un golpe de 10 durante toda la ventana activa, no uno por frame");
            Assert.AreEqual(attackerHealth.MaxHealth, attackerHealth.CurrentHealth, "El atacante no se golpea a sí mismo");
        }

        [UnityTest]
        public IEnumerator Hitscan_HitsFirstTarget_NotTheShooter()
        {
            var shooter = _world.Player(new Vector2(0f, 50f), out _);
            shooter.Controller.Rigidbody.gravityScale = 0f;
            var shooterHealth = shooter.gameObject.AddComponent<CharacterHealth>();
            var gun = shooter.gameObject.AddComponent<WeaponHitscan>();
            var target = _world.Block("Dummy", new Vector2(5f, 50f), new Vector2(0.5f, 2f)).AddComponent<DamageableObject>();
            yield return null;

            Assert.IsTrue(gun.TryFire(Vector2.right));
            Assert.AreEqual(5f, target.CurrentHealth, 0.01f, "El disparo debería atravesar el collider del tirador y dar al objetivo");
            Assert.AreEqual(shooterHealth.MaxHealth, shooterHealth.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator Projectile_FliesStraight_AndDamagesTarget_NotTheShooter()
        {
            // "Prefab" creado en runtime: un objeto activo y quieto lejos de todo.
            var template = _world.Track(new GameObject("ProjectileTemplate"));
            template.transform.position = new Vector2(1000f, 1000f);
            template.AddComponent<Rigidbody2D>().gravityScale = 0f;
            template.AddComponent<CircleCollider2D>().isTrigger = true;
            template.GetComponent<CircleCollider2D>().radius = 0.1f;
            var projectileTemplate = template.AddComponent<ProjectileBehaviour>();
            template.GetComponent<Rigidbody2D>().gravityScale = 1f; // como vendría un prefab por defecto

            var shooter = _world.Player(new Vector2(0f, 50f), out _);
            shooter.Controller.Rigidbody.gravityScale = 0f;
            var shooterHealth = shooter.gameObject.AddComponent<CharacterHealth>();
            var weapon = shooter.gameObject.AddComponent<WeaponProjectile>();
            Set(weapon, "projectilePrefab", projectileTemplate);
            var target = _world.Block("Dummy", new Vector2(6f, 50f), new Vector2(0.5f, 2f)).AddComponent<DamageableObject>();
            yield return null;

            Assert.IsTrue(weapon.TryFire(Vector2.right));
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(12f, target.CurrentHealth, 0.01f, "El proyectil (sin caer por gravedad) debería impactar al objetivo");
            Assert.AreEqual(shooterHealth.MaxHealth, shooterHealth.CurrentHealth, "Nace dentro del tirador pero no debe dañarlo");
        }

        // ---------------- Entorno ----------------

        [UnityTest]
        public IEnumerator SpringPad_LaunchesPlayerUp()
        {
            _world.Ground();
            _world.Block("Spring", new Vector2(0f, -0.25f), new Vector2(2f, 0.5f), trigger: true).AddComponent<SpringPad>();
            var player = _world.Player(new Vector2(0f, 3f), out _);
            float peak = -999f;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) { yield return null; peak = Mathf.Max(peak, player.transform.position.y); }
            yield return new WaitForSeconds(0.1f);
            float afterFall = player.transform.position.y;
            Assert.Greater(peak, 3.5f, "Tras caer sobre el resorte debería salir disparado por encima de donde empezó");
            Assert.IsTrue(peak > afterFall || player.Controller.Velocity.y > 0f);
        }

        [UnityTest]
        public IEnumerator Collectible_AddsScoreAndDisappears()
        {
            var managers = _world.Track(new GameObject("Managers"));
            var score = managers.AddComponent<ScoreManager>();
            _world.Ground();
            var coin = _world.Block("Coin", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), trigger: true);
            coin.AddComponent<Collectible>();
            var player = _world.Player(new Vector2(0f, 3f), out _);
            player.gameObject.tag = "Player";

            yield return new WaitForSeconds(1f);
            Assert.AreEqual(1, score.CurrentScore);
            Assert.IsTrue(coin == null, "La moneda se destruye al recogerla");
        }

        [UnityTest]
        public IEnumerator DoorWithKey_OpensOnlyWithTheKey()
        {
            _world.Ground();
            var door = _world.Block("Door", new Vector2(4f, 1f), new Vector2(1.5f, 3f), trigger: true);
            var blocker = _world.Block("DoorBlocker", new Vector2(4f, 1f), new Vector2(0.5f, 3f)).GetComponent<Collider2D>();
            var doorComp = door.AddComponent<DoorWithKey>();
            Set(doorComp, "blockingCollider", blocker);
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            var inventory = player.gameObject.AddComponent<InventoryManager>();
            yield return new WaitForSeconds(0.2f);

            player.Controller.Rigidbody.position = new Vector2(3.2f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(doorComp.IsOpen, "Sin la llave no abre");

            player.Controller.Rigidbody.position = new Vector2(0f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            inventory.AddItem("llave_dorada");
            player.Controller.Rigidbody.position = new Vector2(3.2f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(doorComp.IsOpen, "Con la llave abre");
            Assert.IsFalse(blocker.enabled, "Deja de bloquear el paso");
            Assert.IsFalse(inventory.HasItem("llave_dorada"), "La llave se consume por defecto");
        }

        [UnityTest]
        public IEnumerator MovingPlatform_ReachesChildWaypoints_AndCarriesPlayer()
        {
            var platform = _world.Block("Platform", new Vector2(0f, 0f), new Vector2(3f, 0.5f));
            platform.AddComponent<Rigidbody2D>();
            var a = new GameObject("A"); a.transform.SetParent(platform.transform); a.transform.localPosition = Vector3.zero;
            var b = new GameObject("B"); b.transform.SetParent(platform.transform); b.transform.localPosition = new Vector3(6f, 0f, 0f);
            var mover = platform.AddComponent<MovingPlatform>();
            Set(mover, "waypoints", new[] { b.transform, a.transform });
            Set(mover, "speed", 3f);
            // Awake ya corrió al agregarlo sin waypoints: se fuerza a releerlos como al cargar la escena.
            platform.SetActive(false);
            var awake = typeof(MovingPlatform).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awake.Invoke(mover, null);
            platform.SetActive(true);

            var player = _world.Player(new Vector2(0f, 1.2f), out _);
            yield return new WaitForSeconds(1.2f);
            Assert.Greater(platform.transform.position.x, 2.5f, "Con waypoints hijos la plataforma igual debe avanzar hacia el destino");
            Assert.AreEqual(platform.transform.position.x, player.transform.position.x, 0.6f, "El jugador parado encima viaja con la plataforma");
        }

        [UnityTest]
        public IEnumerator ConveyorBelt_KeepsMovingAPlayerThatStandsStill()
        {
            _world.Block("Belt", new Vector2(0f, -0.5f), new Vector2(40f, 1f)).AddComponent<ConveyorBelt>();
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            yield return new WaitForSeconds(0.3f);
            float x0 = player.transform.position.x;
            yield return new WaitForSeconds(2f);
            Assert.Greater(player.transform.position.x - x0, 4f, "A 3 u/s durante 2 s debería haberlo arrastrado ~6 unidades, aunque esté quieto");
        }

        // ---------------- IA ----------------

        [UnityTest]
        public IEnumerator EnemyChase_MovesTowardTarget()
        {
            _world.Ground();
            var target = Point(new Vector2(6f, 0.5f));
            var enemy = _world.Block("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var chase = enemy.AddComponent<EnemyChase>();
            Set(chase, "target", target.transform);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(chase.IsChasing);
            Assert.Greater(enemy.transform.position.x, 1.5f);
        }

        [UnityTest]
        public IEnumerator EnemyPatrol_TurnsAtWall_WithoutConfiguration()
        {
            _world.Ground();
            _world.Block("Wall", new Vector2(3f, 1f), new Vector2(1f, 3f));
            var enemy = _world.Block("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            enemy.AddComponent<EnemyPatrol>();
            yield return new WaitForSeconds(2.5f);
            Assert.Less(enemy.transform.localScale.x, 0f, "Debería haberse dado vuelta al llegar a la pared");
            Assert.Less(enemy.GetComponent<Rigidbody2D>().linearVelocity.x, 0f);
        }

        [UnityTest]
        public IEnumerator EnemyPatrol_DoesNotWalkOffEdges()
        {
            _world.Block("Ledge", new Vector2(0f, -0.5f), new Vector2(6f, 1f));
            var enemy = _world.Block("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            enemy.AddComponent<EnemyPatrol>();
            yield return new WaitForSeconds(5f);
            Assert.Greater(enemy.transform.position.y, -0.5f, "Debería seguir sobre la plataforma");
            Assert.Less(Mathf.Abs(enemy.transform.position.x), 3f);
        }

        [UnityTest]
        public IEnumerator AIBrain_SwitchesFromIdleToChaseWhenTargetIsClose()
        {
            _world.Ground();
            var target = Point(new Vector2(20f, 0.5f));
            var enemy = _world.Block("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1f));
            enemy.SetActive(false);
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var wait = enemy.AddComponent<AIActionWait>();
            var move = enemy.AddComponent<AIActionMoveToTarget>();
            var inRange = enemy.AddComponent<AIDecisionTargetInRange>();
            Set(inRange, "range", 5f);
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "target", target.transform);
            Set(brain, "states", new List<AIState>
            {
                new AIState { name = "Idle", actions = new List<AIActionBase> { wait },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, trueTargetState = "Chase" } } },
                new AIState { name = "Chase", actions = new List<AIActionBase> { move },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, falseTargetState = "Idle" } } },
            });
            enemy.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("Idle", brain.CurrentState.name);
            target.transform.position = new Vector2(4f, 0.5f);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual("Chase", brain.CurrentState.name);
            Assert.Greater(enemy.transform.position.x, 0.5f, "En Chase debería moverse hacia el objetivo");
        }
    }
}
