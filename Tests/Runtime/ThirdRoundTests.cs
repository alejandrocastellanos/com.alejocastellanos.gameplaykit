using System.Collections;
using System.Collections.Generic;
using GameplayKit.AI;
using GameplayKit.CameraSystem;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using GameplayKit.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    /// <summary>Tercera ronda: los componentes que solo tenían el chequeo de "se agrega sin errores" y las piezas nuevas.</summary>
    public class ThirdRoundTests
    {
        private TestWorld _world;

        [SetUp]
        public void SetUp()
        {
            Application.runInBackground = true;
            Time.timeScale = 1f;
            _world = new TestWorld();
        }

        [TearDown]
        public void TearDown() => _world.Dispose();

        private static void Set(object target, string field, object value) => TestWorld.Set(target, field, value);

        private static IEnumerator Settle(CharacterCore player)
        {
            for (float t = 0f; !player.Controller.IsGrounded && t < 2f; t += Time.deltaTime) yield return null;
            yield return new WaitForSeconds(0.2f);
        }

        private GameObject Body(string name, Vector2 pos, Vector2 size)
        {
            var go = _world.Block(name, pos, size);
            go.AddComponent<Rigidbody2D>().freezeRotation = true;
            return go;
        }

        /// <summary>Crea un objeto con componentes con el GameObject inactivo (para asignar campos antes de su Awake).</summary>
        private GameObject Inactive(string name, Vector2 pos)
        {
            var go = _world.Track(new GameObject(name));
            go.transform.position = pos;
            go.SetActive(false);
            return go;
        }

        private static AIState State(string name, AIActionBase action, AIDecisionBase decision = null, string onTrue = null)
        {
            var s = new AIState { name = name, actions = new List<AIActionBase>() };
            if (action != null) s.actions.Add(action);
            if (decision != null) s.transitions.Add(new AITransition { decision = decision, trueTargetState = onTrue });
            return s;
        }

        // ---------------- Movimiento ----------------

        [UnityTest]
        public IEnumerator WallCling_HoldsOnlyWhilePushingTowardTheWall()
        {
            _world.Ground(-20f);
            _world.Block("Wall", new Vector2(1f, 0f), new Vector2(1f, 30f));
            var player = _world.Player(new Vector2(0f, 5f), out var input, typeof(PlayerWallJump), typeof(PlayerWallCling));
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.6f);
            float y = player.transform.position.y;
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(y, player.transform.position.y, 0.15f, "Empujando contra la pared se queda agarrado");

            input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.4f);
            Assert.Less(player.transform.position.y, y - 0.3f, "Sin empujar, cae");
        }

        [UnityTest]
        public IEnumerator SlopeWalk_StandsStillOnASlope()
        {
            var slope = _world.Block("Slope", new Vector2(0f, -1f), new Vector2(20f, 1f));
            slope.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
            var player = _world.Player(new Vector2(0f, 2f), out _, typeof(PlayerSlopeWalk));
            yield return Settle(player);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(player.GetComponent<PlayerSlopeWalk>().OnWalkableSlope);
            float x = player.transform.position.x;
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(x, player.transform.position.x, 0.1f, "Quieto en una pendiente de 30° no debe resbalar");
        }

        [UnityTest]
        public IEnumerator TopDown_MovesInAnyDirectionWithoutGravity()
        {
            var player = _world.Player(new Vector2(0f, 0f), out var input, typeof(PlayerTopDownMovement));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0f, player.transform.position.y, 0.05f, "Sin gravedad: no cae");
            input.Move = new Vector2(1f, 1f);
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(player.transform.position.x, 1f);
            Assert.Greater(player.transform.position.y, 1f);
            input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.3f);
            Assert.Less(player.Controller.Velocity.magnitude, 0.2f, "Al soltar se detiene");
        }

        [UnityTest]
        public IEnumerator DropThrough_FallsThroughOneWayPlatform_InsteadOfJumping()
        {
            _world.Ground(-5f);
            _world.Block("OneWay", new Vector2(0f, -0.15f), new Vector2(4f, 0.3f)).AddComponent<OneWayPlatform>();
            var player = _world.Player(new Vector2(0f, 1f), out var input, typeof(PlayerJump), typeof(PlayerDropThrough));
            yield return Settle(player);
            input.Move = Vector2.down;
            input.Jump = true;
            yield return null; yield return null;
            input.Jump = false; input.Move = Vector2.zero;
            Assert.LessOrEqual(player.Controller.Velocity.y, 0.1f, "Abajo + saltar no debe saltar");
            yield return new WaitForSeconds(1f);
            Assert.Less(player.transform.position.y, -2f, "Debería haber atravesado la plataforma");
        }

        [UnityTest]
        public IEnumerator JumpBuffer_JumpPressedJustBeforeLandingStillJumps()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 3f), out var input, typeof(PlayerJump));
            // Presionar justo antes de tocar el suelo.
            while (player.transform.position.y > 0.75f) yield return null; // ~0.35 sobre el punto de apoyo
            input.Jump = true;
            float peak = 0f;
            for (float t = 0f; t < 0.8f; t += Time.deltaTime) { peak = Mathf.Max(peak, player.Controller.Velocity.y); yield return null; }
            input.Jump = false;
            Assert.Greater(peak, 8f, "El salto presionado un instante antes de aterrizar debe salir al aterrizar");
        }

        [UnityTest]
        public IEnumerator Persistence_DetachesAndSurvivesSceneLoads_DuplicatesAreRemoved()
        {
            var parent = _world.Track(new GameObject("Parent"));
            var a = new GameObject("PersistentA"); a.transform.SetParent(parent.transform);
            a.AddComponent<CharacterPersistence>();
            yield return null;
            Assert.IsNull(a.transform.parent, "Se suelta del padre para poder persistir");
            Assert.AreEqual("DontDestroyOnLoad", a.scene.name);
            var b = new GameObject("PersistentB");
            b.AddComponent<CharacterPersistence>();
            yield return null;
            Assert.IsTrue(b == null, "Un segundo con el mismo id se destruye");
            Object.Destroy(a);
            yield return null;
        }

        // ---------------- Combate ----------------

        [UnityTest]
        public IEnumerator WeaponCharge_HoldingLongerHitsHarder()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
            player.gameObject.AddComponent<WeaponMelee>();
            player.gameObject.AddComponent<WeaponCharge>();
            var dummy = _world.Block("DummyA", new Vector2(1f, 0.5f), new Vector2(0.5f, 1f));
            dummy.SetActive(false); // la vida máxima se asigna antes de su Awake
            var quick = dummy.AddComponent<DamageableObject>();
            Set(quick, "maxHealth", 100f);
            dummy.SetActive(true);
            yield return Settle(player);

            input.Hold(CharacterAction.Attack); yield return null; yield return null;
            input.Release(CharacterAction.Attack);
            yield return new WaitForSeconds(0.5f);
            float quickDamage = 100f - quick.CurrentHealth;
            Assert.Greater(quickDamage, 0f, "Soltar enseguida igual ataca");

            input.Hold(CharacterAction.Attack);
            yield return new WaitForSeconds(1.6f);
            input.Release(CharacterAction.Attack);
            yield return new WaitForSeconds(0.4f);
            float chargedDamage = 100f - quick.CurrentHealth - quickDamage;
            Assert.Greater(chargedDamage, quickDamage * 3f, "Cargado al máximo pega bastante más");
        }

        [UnityTest]
        public IEnumerator WeaponInventorySlot_SwitchActionCyclesWeapons()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
            var sword = new GameObject("Sword"); sword.transform.SetParent(player.transform, false); sword.AddComponent<WeaponMelee>();
            var gun = new GameObject("Gun"); gun.transform.SetParent(player.transform, false); gun.AddComponent<WeaponHitscan>();
            var slot = player.gameObject.AddComponent<WeaponInventorySlot>();
            Set(slot, "weapons", new[] { sword, gun });
            var target = _world.Block("Dummy", new Vector2(6f, 0.5f), new Vector2(0.5f, 2f)).AddComponent<DamageableObject>();
            yield return Settle(player);

            Assert.IsTrue(sword.activeSelf && !gun.activeSelf, "Empieza con la primera arma");
            input.TapAction(CharacterAction.Special);
            yield return null; yield return null;
            Assert.IsTrue(gun.activeSelf && !sword.activeSelf, "La acción de cambio equipa la siguiente");
            input.TapAction(CharacterAction.Attack);
            yield return null; yield return null;
            Assert.Less(target.CurrentHealth, target.MaxHealth, "Ataca con el arma equipada (la pistola, a distancia)");
        }

        [UnityTest]
        public IEnumerator AimAndOrient_ClosestTarget_RotatesOnlyTheWeapon()
        {
            var player = _world.Track(new GameObject("Shooter"));
            var arm = new GameObject("Arm"); arm.transform.SetParent(player.transform, false);
            player.AddComponent<BoxCollider2D>();
            var aim = player.AddComponent<CharacterAimAndOrient>();
            Set(aim, "aimMode", CharacterAimAndOrient.AimMode.ClosestTarget);
            Set(aim, "partToRotate", arm.transform);
            _world.Block("Enemy", new Vector2(0f, 5f), new Vector2(0.5f, 0.5f)).AddComponent<DamageableObject>();
            _world.Block("Wall", new Vector2(2f, 0f), new Vector2(0.5f, 0.5f)); // no dañable: se ignora
            yield return null; yield return null;
            Assert.AreEqual(0f, Vector2.Angle(aim.AimDirection, Vector2.up), 1f, "Apunta al objetivo dañable más cercano");
            Assert.AreEqual(90f, arm.transform.eulerAngles.z, 1f, "Gira el brazo");
            Assert.AreEqual(0f, player.transform.eulerAngles.z, 0.01f, "El cuerpo no se rota");
        }

        // ---------------- IA ----------------

        [UnityTest]
        public IEnumerator EnemyFlee_RunsAwayAndFacesAway()
        {
            _world.Ground();
            var threat = _world.Point(new Vector2(-2f, 0.5f));
            var enemy = Body("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1f));
            Set(enemy.AddComponent<EnemyFlee>(), "target", threat.transform);
            yield return new WaitForSeconds(0.8f);
            Assert.Greater(enemy.transform.position.x, 1f);
            Assert.Greater(enemy.transform.localScale.x, 0f);
            threat.transform.position = new Vector2(enemy.transform.position.x + 2f, 0.5f);
            yield return new WaitForSeconds(0.3f);
            Assert.Less(enemy.transform.localScale.x, 0f, "Al cambiar de lado la amenaza, se da vuelta");
        }

        [UnityTest]
        public IEnumerator EnemyPatrolWithinBounds_WorksWithChildBounds()
        {
            _world.Ground();
            var enemy = Inactive("Enemy", new Vector2(0f, 0.5f));
            enemy.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 1f);
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var left = new GameObject("L"); left.transform.SetParent(enemy.transform, false); left.transform.localPosition = new Vector3(-2f, 0f, 0f);
            var right = new GameObject("R"); right.transform.SetParent(enemy.transform, false); right.transform.localPosition = new Vector3(2f, 0f, 0f);
            var patrol = enemy.AddComponent<EnemyPatrolWithinBounds>();
            Set(patrol, "leftBound", left.transform);
            Set(patrol, "rightBound", right.transform);
            enemy.SetActive(true);
            float maxX = -99f, minX = 99f;
            for (float t = 0f; t < 4f; t += Time.deltaTime) { maxX = Mathf.Max(maxX, enemy.transform.position.x); minX = Mathf.Min(minX, enemy.transform.position.x); yield return null; }
            Assert.Less(maxX, 2.6f, "Se da vuelta en el límite derecho aunque sea hijo suyo");
            Assert.Less(minX, -1.5f, "Y vuelve hasta el izquierdo");
        }

        [UnityTest]
        public IEnumerator PathfindingAgent_WithoutNavMesh_DoesNothingAndDoesNotFail()
        {
            var target = _world.Point(new Vector2(5f, 0f));
            var go = _world.Track(new GameObject("Agent"));
            var agent = go.AddComponent<EnemyPathfindingAgent>();
            agent.SetTarget(target.transform);
            yield return new WaitForSeconds(0.3f);
            var nav = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
            Assert.IsFalse(nav.updateUpAxis, "Configurado para 2D");
            Assert.AreEqual(0f, go.transform.position.x, 0.01f);
        }

        [UnityTest]
        public IEnumerator AIBrain_FleeAction_MovesAwayFromTarget()
        {
            _world.Ground();
            var target = _world.Point(new Vector2(-3f, 0.5f));
            var enemy = Inactive("Enemy", new Vector2(0f, 0.5f));
            enemy.AddComponent<BoxCollider2D>(); enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var flee = enemy.AddComponent<AIActionFlee>();
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "target", target.transform);
            Set(brain, "states", new List<AIState> { State("Flee", flee) });
            enemy.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            Assert.Greater(enemy.transform.position.x, 1f);
        }

        [UnityTest]
        public IEnumerator AIBrain_PatrolPoints_ReachesRaisedPointsAndStopsAtTheEnd()
        {
            _world.Ground();
            var enemy = Inactive("Enemy", new Vector2(0f, 0.5f));
            enemy.AddComponent<BoxCollider2D>(); enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var patrol = enemy.AddComponent<AIActionPatrolPoints>();
            Set(patrol, "waypoints", new[] { _world.Point(new Vector2(3f, 2f)).transform, _world.Point(new Vector2(-1f, 3f)).transform });
            Set(patrol, "loop", false);
            Set(patrol, "speed", 4f);
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "states", new List<AIState> { State("Patrol", patrol) });
            enemy.SetActive(true);
            yield return new WaitForSeconds(3f);
            Assert.IsTrue(patrol.Finished, "Alcanza puntos más altos que el suelo comparando solo en X");
            Assert.AreEqual(-1f, enemy.transform.position.x, 0.4f, "Y se detiene en el último");
            Assert.Less(Mathf.Abs(enemy.GetComponent<Rigidbody2D>().linearVelocity.x), 0.1f);
        }

        [UnityTest]
        public IEnumerator AIBrain_ShootAction_DamagesTheTarget()
        {
            var targetGo = _world.Block("Target", new Vector2(5f, 50f), new Vector2(0.5f, 2f));
            var target = targetGo.AddComponent<DamageableObject>();
            var enemy = Inactive("Enemy", new Vector2(0f, 50f));
            enemy.AddComponent<WeaponHitscan>();
            var shoot = enemy.AddComponent<AIActionShoot>();
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "target", targetGo.transform);
            Set(brain, "states", new List<AIState> { State("Shoot", shoot) });
            enemy.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            Assert.Less(target.CurrentHealth, target.MaxHealth);
        }

        [UnityTest]
        public IEnumerator AIBrain_HealthThreshold_WorksWithDamageableObject()
        {
            var enemy = Inactive("Enemy", new Vector2(0f, 50f));
            var hp = enemy.AddComponent<DamageableObject>();
            Set(hp, "maxHealth", 100f);
            var wait = enemy.AddComponent<AIActionWait>();
            var low = enemy.AddComponent<AIDecisionHealthThreshold>();
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "states", new List<AIState> { State("Fight", wait, low, "Retreat"), State("Retreat", wait) });
            enemy.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual("Fight", brain.CurrentState.name);
            hp.ApplyDamage(60f, Vector2.zero, Vector2.zero, null);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual("Retreat", brain.CurrentState.name, "Bajo 50 % de vida cambia de estado");
        }

        [UnityTest]
        public IEnumerator AIBrain_TimeInState_TransitionsAfterTheDelay()
        {
            var enemy = Inactive("Enemy", new Vector2(0f, 50f));
            var wait = enemy.AddComponent<AIActionWait>();
            var timer = enemy.AddComponent<AIDecisionTimeInState>();
            Set(timer, "seconds", 0.5f);
            var brain = enemy.AddComponent<AIBrain>();
            Set(brain, "states", new List<AIState> { State("Idle", wait, timer, "Alert"), State("Alert", wait) });
            enemy.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual("Idle", brain.CurrentState.name);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("Alert", brain.CurrentState.name);
        }

        [UnityTest]
        public IEnumerator EnemySpawner_RespectsMaxAlive_AndAssignsTarget()
        {
            var player = _world.Track(new GameObject("ThePlayer") { tag = "Player" });
            var template = Inactive("EnemyTemplate", new Vector2(1000f, 1000f));
            template.AddComponent<AIBrain>();
            var spawnerGo = _world.Track(new GameObject("Spawner"));
            spawnerGo.transform.position = new Vector2(0f, 50f);
            var spawner = spawnerGo.AddComponent<EnemySpawner>();
            Set(spawner, "prefab", template);
            Set(spawner, "interval", 0.1f);
            Set(spawner, "maxAlive", 2);
            // El prefab de prueba está inactivo: las copias también; se activan al aparecer.
            var spawned = new List<GameObject>();
            spawner.OnSpawned += go => { go.SetActive(true); spawned.Add(go); _world.Track(go); };
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(2, spawner.AliveCount, "Nunca más de Max Alive a la vez");
            Assert.AreEqual(player.transform, spawned[0].GetComponent<AIBrain>().Target, "Apuntan al jugador");
            Object.Destroy(spawned[0]);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(3, spawner.SpawnedCount, "Al morir uno, genera otro");
        }

        // ---------------- Vida y feedback ----------------

        [UnityTest]
        public IEnumerator Lives_RespawnUntilOutOfLives_ThenGameOver()
        {
            var managers = _world.Track(new GameObject("GM"));
            var gm = managers.AddComponent<GameManager>();
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            var health = player.gameObject.AddComponent<CharacterHealth>();
            var lives = player.gameObject.AddComponent<CharacterLives>();
            Set(lives, "startingLives", 2);
            lives.ResetLives();
            Set(player.gameObject.AddComponent<CharacterRespawn>(), "respawnDelay", 0.1f);
            bool over = false;
            lives.OnGameOver += () => over = true;
            yield return Settle(player);

            health.TakeDamage(1000f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(health.IsDead, "Con vidas restantes reaparece");
            Assert.AreEqual(1, lives.Lives);

            yield return new WaitForSeconds(0.6f); // fin de la invulnerabilidad tras reaparecer
            health.TakeDamage(1000f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(health.IsDead && over, "Sin vidas: game over y no reaparece");
            Assert.AreEqual(GameManager.GameState.GameOver, gm.CurrentState);
            Object.Destroy(managers);
        }

        [UnityTest]
        public IEnumerator HealthPickup_HealsOnlyWhenHurt()
        {
            _world.Ground();
            var pickup = _world.Block("Heart", new Vector2(0f, 0.5f), new Vector2(1f, 1f), trigger: true);
            pickup.AddComponent<HealthPickup>();
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            var health = player.gameObject.AddComponent<CharacterHealth>();
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(pickup != null, "Con la vida llena no se consume");
            health.TakeDamage(40f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(pickup == null, "Herido, lo recoge");
            Assert.AreEqual(85f, health.CurrentHealth, 0.01f);
        }

        [UnityTest]
        public IEnumerator DamageFlash_TintsThenRestoresColors()
        {
            var go = _world.Track(new GameObject("Target"));
            var sr = new GameObject("Visual").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(go.transform, false);
            sr.color = Color.blue;
            var health = go.AddComponent<CharacterHealth>();
            go.AddComponent<DamageFlash>();
            yield return null;
            health.ApplyDamage(10f, Vector2.zero, Vector2.zero, null);
            yield return null;
            Assert.AreNotEqual(Color.blue, sr.color, "Al recibir daño cambia de color");
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(Color.blue, sr.color, "Al terminar vuelve exactamente a su color");
        }

        [UnityTest]
        public IEnumerator DamagePopups_AppearOnDamage_AndCleanUp()
        {
            var target = _world.Block("Dummy", new Vector2(0f, 50f), new Vector2(1f, 1f));
            var hp = target.AddComponent<DamageableObject>();
            Set(hp, "maxHealth", 100f);
            target.AddComponent<DamagePopupSpawner>();
            yield return null;
            hp.ApplyDamage(7f, Vector2.zero, Vector2.zero, null);
            yield return null;
            var popup = Object.FindAnyObjectByType<UIDamagePopup>();
            Assert.IsNotNull(popup, "Aparece un número flotante");
            Assert.AreEqual("-7", popup.GetComponent<UnityEngine.UI.Text>().text);
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(Object.FindAnyObjectByType<UIDamagePopup>() == null, "Se destruye al terminar");
        }

        // ---------------- Cámara ----------------

        [UnityTest]
        public IEnumerator CameraShake_ShakesAroundTheFollowPosition_NotTheStart()
        {
            var target = _world.Point(new Vector2(0f, 0f));
            var camGo = _world.Track(new GameObject("Cam"));
            camGo.AddComponent<Camera>().orthographic = true;
            var follow = camGo.AddComponent<CameraFollow>();
            follow.SetTarget(target.transform);
            Set(follow, "smoothTime", 0.01f);
            var shake = camGo.AddComponent<CameraShake>();
            target.transform.position = new Vector2(20f, 0f);
            yield return new WaitForSeconds(0.5f);
            shake.Shake(0.3f, 0.5f);
            bool moved = false;
            // Se mide al final del frame: el temblor se aplica en LateUpdate y se retira en el Update siguiente.
            for (float t = 0f; t < 0.25f; t += Time.deltaTime) { yield return new WaitForEndOfFrame(); if (Mathf.Abs(camGo.transform.position.y) > 0.05f) moved = true; }
            Assert.IsTrue(moved, "La cámara tiembla");
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(20f, camGo.transform.position.x, 0.1f, "Al terminar sigue donde está el objetivo, no vuelve al inicio");
            Assert.AreEqual(0f, camGo.transform.position.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator CameraZoomBySpeed_ZoomsOutWhenTheTargetIsFast()
        {
            var body = Body("Runner", new Vector2(0f, 50f), new Vector2(1f, 1f));
            var rb = body.GetComponent<Rigidbody2D>(); rb.gravityScale = 0f;
            var camGo = _world.Track(new GameObject("Cam"));
            var cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5f;
            Set(camGo.AddComponent<CameraZoomBySpeed>(), "target", rb);
            rb.linearVelocity = new Vector2(10f, 0f);
            yield return new WaitForSeconds(1f);
            Assert.Greater(cam.orthographicSize, 6.5f);
        }

        // ---------------- Managers ----------------

        [UnityTest]
        public IEnumerator LevelManager_KillsBelowVoid_TracksCheckpoint_FeedsCameraBounds()
        {
            var levelGo = Inactive("Level", Vector2.zero);
            var level = levelGo.AddComponent<LevelManager>();
            Set(level, "voidY", -10f);
            Set(level, "levelMinBounds", new Vector2(-10f, -10f));
            Set(level, "levelMaxBounds", new Vector2(10f, 10f));
            levelGo.SetActive(true);

            var player = _world.Player(new Vector2(0f, -5f), out _);
            player.gameObject.tag = "Player";
            var health = player.gameObject.AddComponent<CharacterHealth>();
            var checkpoint = _world.Block("CP", new Vector2(0f, -5f), new Vector2(2f, 2f), trigger: true).AddComponent<Checkpoint>();
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(checkpoint.transform, level.ActiveCheckpoint, "El checkpoint avisa al LevelManager");

            var camGo = _world.Track(new GameObject("Cam"));
            var cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 3f; cam.aspect = 1f;
            camGo.AddComponent<CameraBounds>();
            camGo.transform.position = new Vector3(50f, 0f, -10f);
            yield return null; yield return null;
            Assert.AreEqual(7f, camGo.transform.position.x, 0.01f, "CameraBounds sin configurar usa los límites del LevelManager");

            yield return new WaitForSeconds(1.5f);
            Assert.IsTrue(health.IsDead, "Al caer por debajo de Void Y muere");
        }

        [UnityTest]
        public IEnumerator GameManager_ChangesStateAndNotifies()
        {
            var go = _world.Track(new GameObject("GM"));
            var gm = go.AddComponent<GameManager>();
            GameManager.GameState? notified = null;
            gm.OnStateChanged += s => notified = s;
            gm.SetState(GameManager.GameState.Paused);
            Assert.AreEqual(GameManager.GameState.Paused, gm.CurrentState);
            Assert.AreEqual(GameManager.GameState.Paused, notified);
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioManager_WorksWithoutAssignedSources()
        {
            var go = _world.Track(new GameObject("Audio"));
            var audio = go.AddComponent<AudioManager>();
            var clip = AudioClip.Create("tone", 44100, 1, 44100, false);
            audio.PlayMusic(clip);
            audio.PlaySfx(clip);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(3, go.GetComponentsInChildren<AudioSource>().Length, "Crea sus fuentes de música A/B y efectos");
            Assert.AreEqual(clip, audio.CurrentMusic);
            Object.Destroy(go);
            yield return null;
        }

        [System.Serializable] private class SaveData { public int level; public float hp; }

        [UnityTest]
        public IEnumerator SaveLoad_RoundTrip_AndCorruptDataIsIgnored()
        {
            var go = _world.Track(new GameObject("Save"));
            var save = go.AddComponent<SaveLoadSystem>();
            Set(save, "saveKey", "GameplayKit.Tests.Save");
            save.Save(new SaveData { level = 3, hp = 42.5f });
            Assert.IsTrue(save.TryLoad(out SaveData loaded));
            Assert.AreEqual(3, loaded.level);
            Assert.AreEqual(42.5f, loaded.hp);

            PlayerPrefs.SetString("GameplayKit.Tests.Save", "{ esto no es json");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no se pudo leer"));
            Assert.IsFalse(save.TryLoad(out SaveData _));
            save.ClearSave();
            Assert.IsFalse(save.HasSave);
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneTransition_CreatesItsOwnFadeLayer_AndPersists()
        {
            var go = _world.Track(new GameObject("Transitions"));
            go.AddComponent<SceneTransitionManager>();
            yield return null;
            var group = go.GetComponentInChildren<CanvasGroup>();
            Assert.IsNotNull(group, "Sin CanvasGroup asignado crea su capa de fundido");
            Assert.AreEqual(0f, group.alpha);
            Assert.AreEqual("DontDestroyOnLoad", go.scene.name);
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LevelExit_RequiresTheItem_ThenTriggers()
        {
            _world.Ground();
            var exit = _world.Block("Exit", new Vector2(4f, 1f), new Vector2(1f, 3f), trigger: true).AddComponent<LevelExit>();
            Set(exit, "requiredItemId", "estrella");
            var player = _world.Player(new Vector2(0f, 0.5f), out _);
            player.gameObject.tag = "Player";
            var inventory = player.gameObject.AddComponent<InventoryManager>();
            yield return Settle(player);
            player.Controller.Rigidbody.position = new Vector2(4f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(exit.Triggered, "Sin el ítem no se activa");
            player.Controller.Rigidbody.position = new Vector2(0f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            inventory.AddItem("estrella");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no hay escena siguiente"));
            player.Controller.Rigidbody.position = new Vector2(4f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(exit.Triggered);
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator AnimatorBridge_WritesOnlyExistingParameters()
        {
            var controller = new UnityEditor.Animations.AnimatorController();
            controller.AddLayer("Base");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            var animator = player.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            player.gameObject.AddComponent<CharacterAnimatorBridge>();
            yield return Settle(player);
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(animator.GetFloat("Speed"), 1f);
            Assert.IsTrue(animator.GetBool("Grounded"));
            // Sin parámetros VelocityY/MovementState/Dead/Hit en el controller: no debe haber warnings ni errores.
            LogAssert.NoUnexpectedReceived();
        }
#endif
    }
}
