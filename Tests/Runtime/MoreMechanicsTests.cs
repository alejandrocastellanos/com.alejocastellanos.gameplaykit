using System.Collections;
using GameplayKit.AI;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    /// <summary>Segunda ronda: mecánicas con acciones de input, interacción, entorno avanzado e IA a distancia.</summary>
    public class MoreMechanicsTests
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

        private static void Set(object target, string field, object value) => TestWorld.Set(target, field, value);

        private static IEnumerator Settle(CharacterCore player)
        {
            for (float t = 0f; !player.Controller.IsGrounded && t < 2f; t += Time.deltaTime) yield return null;
            yield return new WaitForSeconds(0.2f);
        }

        // ---------------- Acciones de input ----------------

        [UnityTest]
        public IEnumerator Fly_TogglesWithAction_IgnoresGravity_AndTurnsOff()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerFly));
            var fly = player.GetComponent<PlayerFly>();
            yield return Settle(player);

            input.TapAction(CharacterAction.Fly);
            yield return null; yield return null;
            Assert.IsTrue(fly.IsFlying);
            float y0 = player.transform.position.y;
            input.Move = Vector2.up;
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(player.transform.position.y - y0, 1.5f, "Volando debería subir con el input");

            input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.3f);
            float hover = player.transform.position.y;
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(hover, player.transform.position.y, 0.1f, "Sin input se queda flotando");

            input.TapAction(CharacterAction.Fly);
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(fly.IsFlying);
            Assert.Less(player.transform.position.y, hover - 0.3f, "Al desactivarlo vuelve a caer");
        }

        [UnityTest]
        public IEnumerator Fly_And_Ladder_StillWorkWithGravityController()
        {
            // CharacterGravityController reescribía la gravedad cada frame y anulaba volar/escaleras.
            _world.Ground();
            var ladder = _world.Block("Ladder", new Vector2(4f, 4f), new Vector2(1f, 10f), trigger: true);
            ladder.AddComponent<LadderZone>();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input,
                typeof(CharacterGravityController), typeof(PlayerFly), typeof(PlayerClimbLadder));
            yield return Settle(player);

            input.TapAction(CharacterAction.Fly);
            yield return new WaitForSeconds(0.5f);
            float hover = player.transform.position.y;
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(hover, player.transform.position.y, 0.1f, "Volando no debe caer aunque haya CharacterGravityController");
            input.TapAction(CharacterAction.Fly);
            yield return Settle(player);

            player.Controller.Rigidbody.position = new Vector2(4f, 0.5f);
            input.Move = Vector2.up;
            yield return new WaitForSeconds(0.3f);
            input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.1f);
            float onLadder = player.transform.position.y;
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(onLadder, player.transform.position.y, 0.1f, "Quieto en la escalera no debe resbalar");
        }

        [UnityTest]
        public IEnumerator Roll_MovesForward_AndIgnoresDamage()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerRollDodge));
            var health = player.gameObject.AddComponent<CharacterHealth>();
            yield return Settle(player);

            float x0 = player.transform.position.x;
            input.TapAction(CharacterAction.Roll);
            yield return null; yield return null;
            health.ApplyDamage(50f, Vector2.zero, Vector2.zero, null);
            Assert.AreEqual(health.MaxHealth, health.CurrentHealth, "Durante la rodada no recibe daño");
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(player.transform.position.x - x0, 1.5f, "La rodada avanza aunque no haya input de movimiento");
        }

        [UnityTest]
        public IEnumerator Blink_TeleportsForward_AndStopsBeforeWalls()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerTeleportBlink));
            yield return Settle(player);

            input.TapAction(CharacterAction.Blink);
            yield return null; yield return null;
            Assert.AreEqual(4f, player.transform.position.x, 0.2f, "Sin obstáculos avanza blinkDistance (4)");

            _world.Block("Wall", new Vector2(6.5f, 1f), new Vector2(1f, 3f));
            yield return new WaitForSeconds(1.1f); // cooldown
            input.TapAction(CharacterAction.Blink);
            yield return null; yield return null;
            var body = player.GetComponent<Collider2D>();
            Assert.LessOrEqual(body.bounds.max.x, 6.01f, "No debe quedar metido en la pared");
            Assert.Greater(player.transform.position.x, 5f, "Pero sí avanza hasta pegarse a ella");
        }

        [UnityTest]
        public IEnumerator PlayerAttack_MeleeBreaksObjectInFront_EvenFacingLeft()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerAttack));
            player.gameObject.AddComponent<WeaponMelee>();
            var crate = _world.Block("Crate", new Vector2(-1.1f, 0f), new Vector2(0.8f, 1f));
            crate.AddComponent<BreakableObject>();
            yield return Settle(player);

            input.Move = Vector2.left; yield return null; yield return null; input.Move = Vector2.zero;
            yield return new WaitForSeconds(0.1f);
            input.TapAction(CharacterAction.Attack);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(crate == null, "Mirando a la izquierda, el golpe debe salir hacia la izquierda y romper la caja");
        }

        [UnityTest]
        public IEnumerator PlayerAttack_HitscanShootsWhereThePlayerFaces()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
            player.gameObject.AddComponent<WeaponHitscan>();
            var target = _world.Block("Dummy", new Vector2(6f, 0.5f), new Vector2(0.5f, 2f)).AddComponent<DamageableObject>();
            yield return Settle(player);
            input.TapAction(CharacterAction.Attack);
            yield return null; yield return null;
            Assert.Less(target.CurrentHealth, target.MaxHealth);
        }

        [UnityTest]
        public IEnumerator Interact_LeverActivatesElevatorThroughUnityEvent()
        {
            _world.Ground();
            var elevatorGo = _world.Block("Elevator", new Vector2(5f, -0.25f), new Vector2(2f, 0.5f));
            elevatorGo.AddComponent<Rigidbody2D>();
            var bottom = _world.Point(new Vector2(5f, -0.25f));
            var top = _world.Point(new Vector2(5f, 4f));
            elevatorGo.SetActive(false);
            var elevator = elevatorGo.AddComponent<Elevator>();
            Set(elevator, "bottomPoint", bottom.transform);
            Set(elevator, "topPoint", top.transform);
            elevatorGo.SetActive(true);

            var leverGo = _world.Block("Lever", new Vector2(0.8f, 0.5f), new Vector2(0.5f, 1f), trigger: true);
            var lever = leverGo.AddComponent<Lever>();
            var onTurnedOn = (UnityEngine.Events.UnityEvent)typeof(Lever)
                .GetField("onTurnedOn", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lever);
            onTurnedOn.AddListener(elevator.Activate);

            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerInteract));
            yield return Settle(player);
            input.TapInteract();
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(lever.IsOn, "Interactuar cerca de la palanca la acciona");
            Assert.Greater(elevatorGo.transform.position.y, 1f, "El UnityEvent de la palanca activa el elevador");
        }

        [UnityTest]
        public IEnumerator PressurePlate_PressedByAFallingBox()
        {
            _world.Ground();
            var plate = _world.Block("Plate", new Vector2(0f, -0.4f), new Vector2(1.5f, 0.3f), trigger: true).AddComponent<PressurePlate>();
            bool fired = false;
            plate.OnPressed += () => fired = true;
            var box = _world.Block("Box", new Vector2(0f, 3f), new Vector2(0.8f, 0.8f));
            box.AddComponent<Rigidbody2D>();
            yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(plate.IsPressed && fired);
        }

        // ---------------- Movimiento avanzado ----------------

        [UnityTest]
        public IEnumerator LedgeGrab_HangsOnTheEdge_AndClimbOnTop()
        {
            _world.Ground(-10f);
            _world.Block("Wall", new Vector2(1f, -2f), new Vector2(1f, 8f)); // cara superior en y = 2
            var player = _world.Player(new Vector2(0f, 4f), out var input, typeof(PlayerLedgeGrab), typeof(PlayerLedgeClimb));
            var grab = player.GetComponent<PlayerLedgeGrab>();

            for (float t = 0f; !grab.IsGrabbingLedge && t < 2f; t += Time.deltaTime) yield return null;
            Assert.IsTrue(grab.IsGrabbingLedge, "Cayendo pegado a la pared debería colgarse del borde");
            float hangY = player.transform.position.y;
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(hangY, player.transform.position.y, 0.05f, "Colgado no debería caer");

            input.Jump = true;
            yield return new WaitForSeconds(0.6f);
            input.Jump = false;
            yield return new WaitForSeconds(0.3f);
            var body = player.GetComponent<Collider2D>();
            Assert.GreaterOrEqual(body.bounds.min.y, 1.9f, "Tras trepar los pies quedan sobre la cornisa");
            Assert.Greater(player.transform.position.x, 0.5f, "Y sobre la plataforma, no en el aire junto a ella");
            Assert.IsFalse(grab.IsGrabbingLedge);
            Assert.IsTrue(player.Controller.IsGrounded);
        }

        [UnityTest]
        public IEnumerator Zipline_RidesFromStartToEnd()
        {
            _world.Ground(-20f);
            var zipGo = _world.Block("Zipline", new Vector2(0f, 3f), new Vector2(1f, 1f), trigger: true);
            var path = zipGo.AddComponent<ZiplinePath>();
            path.Start = _world.Point(new Vector2(0f, 3f)).transform;
            path.End = _world.Point(new Vector2(8f, 1f)).transform;
            var player = _world.Player(new Vector2(0f, 4.5f), out _, typeof(PlayerZipline));
            var zip = player.GetComponent<PlayerZipline>();
            for (float t = 0f; !zip.IsRiding && t < 1f; t += Time.deltaTime) yield return null;
            Assert.IsTrue(zip.IsRiding, "Al tocar la tirolesa empieza a deslizarse (sin tags)");
            yield return new WaitForSeconds(1.3f);
            Assert.Greater(player.transform.position.x, 6f, "Llega al otro extremo");
        }

        [UnityTest]
        public IEnumerator RopeGrab_SwingsAtFixedLength_AndJumpReleases()
        {
            _world.Ground(-20f);
            var anchorGo = _world.Track(new GameObject("Anchor"));
            anchorGo.transform.position = new Vector2(0f, 5f);
            var circle = anchorGo.AddComponent<CircleCollider2D>(); circle.isTrigger = true; circle.radius = 3f;
            anchorGo.AddComponent<RopeAnchor>();
            var player = _world.Player(new Vector2(2f, 3f), out var input, typeof(PlayerRopeGrab));
            var rope = player.GetComponent<PlayerRopeGrab>();
            player.Controller.Rigidbody.gravityScale = 0f; // que no se salga del alcance antes de agarrarse
            yield return new WaitForSeconds(0.1f);

            input.TapInteract();
            yield return null; yield return null;
            Assert.IsTrue(rope.IsSwinging);
            float length = Vector2.Distance(player.transform.position, anchorGo.transform.position);
            input.Move = Vector2.left;
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(length, Vector2.Distance(player.transform.position, anchorGo.transform.position), 0.2f, "La cuerda mantiene su largo");
            input.Move = Vector2.zero;
            input.Jump = true; yield return null; yield return null;
            Assert.IsFalse(rope.IsSwinging, "Saltar suelta la cuerda");
        }

        [UnityTest]
        public IEnumerator EdgeDangle_DetectsEdgeOnlyAtTheBorder()
        {
            _world.Block("Ledge", new Vector2(0f, -1f), new Vector2(4f, 1f)); // de x = -2 a x = 2
            var player = _world.Player(new Vector2(0f, 0.5f), out _, typeof(PlayerEdgeDangle));
            var dangle = player.GetComponent<PlayerEdgeDangle>();
            yield return Settle(player);
            Assert.IsFalse(dangle.IsAtEdge, "En el centro no está al borde");
            player.Controller.Rigidbody.position = new Vector2(1.8f, player.transform.position.y);
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(dangle.IsAtEdge, "Con el frente sobre el vacío está al borde");
        }

        [UnityTest]
        public IEnumerator Dash8_DashesDiagonally()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerDash8Directions));
            yield return Settle(player);
            Vector2 p0 = player.transform.position;
            input.Move = new Vector2(1f, 1f);
            input.TapDash();
            yield return new WaitForSeconds(0.15f);
            Vector2 d = (Vector2)player.transform.position - p0;
            Assert.Greater(d.x, 0.8f); Assert.Greater(d.y, 0.8f);
        }

        [UnityTest]
        public IEnumerator Crawl_MovesSlowlyWhileCrouched()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerCrouch), typeof(PlayerCrawl));
            yield return Settle(player);
            float x0 = player.transform.position.x;
            input.Crouch = true; input.Move = Vector2.right;
            yield return new WaitForSeconds(1f);
            float d = player.transform.position.x - x0;
            Assert.Greater(d, 0.3f, "Agachado avanza");
            Assert.Less(d, 1.6f, "Pero lento");
        }

        [UnityTest]
        public IEnumerator GroundSlam_FallsFast_AndReportsLanding()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 8f), out var input, typeof(PlayerGroundSlam));
            bool landed = false;
            player.GetComponent<PlayerGroundSlam>().OnSlamLanded += () => landed = true;
            yield return new WaitForSeconds(0.1f);
            input.Move = Vector2.down; input.TapDash();
            yield return new WaitForSeconds(0.15f);
            Assert.Less(player.Controller.Velocity.y, -15f);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(landed);
        }

        [UnityTest]
        public IEnumerator PathFollow_TravelsAlongWaypoints()
        {
            _world.Ground(-20f);
            var player = _world.Track(new GameObject("Player"));
            player.transform.position = Vector2.zero;
            player.AddComponent<CapsuleCollider2D>();
            player.AddComponent<CharacterController2D>();
            player.AddComponent<ScriptedCharacterInput>();
            var follow = player.AddComponent<PlayerPathFollow>();
            Set(follow, "waypoints", new[] { _world.Point(new Vector2(3f, 2f)).transform, _world.Point(new Vector2(6f, 0f)).transform });
            Set(follow, "loop", false);
            Set(follow, "autoStart", true);
            Set(follow, "speed", 6f);
            player.AddComponent<CharacterCore>();
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(6f, player.transform.position.x, 0.4f, "Debería terminar en el último waypoint");
            Assert.IsFalse(follow.IsFollowingPath);
        }

        // ---------------- Vida ----------------

        [UnityTest]
        public IEnumerator FallDamage_HurtsAfterABigFall_NotOnSpawn()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 1.5f), out _, typeof(CharacterFallDamage));
            var health = player.GetComponent<CharacterHealth>();
            yield return Settle(player);
            Assert.AreEqual(health.MaxHealth, health.CurrentHealth, "Una caída corta al aparecer no hace daño");

            player.Controller.Rigidbody.position = new Vector2(0f, 12f);
            yield return new WaitForSeconds(2f);
            Assert.Less(health.CurrentHealth, health.MaxHealth, "Caer ~11 unidades (mínimo 5) debería doler");
        }

        [UnityTest]
        public IEnumerator Stun_FreezesControl_ThenRestoresIt()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            var stun = player.gameObject.AddComponent<CharacterStun>();
            yield return Settle(player);
            stun.Stun(0.5f);
            input.Move = Vector2.right;
            float x0 = player.transform.position.x;
            yield return new WaitForSeconds(0.3f);
            Assert.Less(player.transform.position.x - x0, 0.3f, "Aturdido no responde al input");
            yield return new WaitForSeconds(0.6f);
            float x1 = player.transform.position.x;
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(player.transform.position.x - x1, 0.5f, "Al pasar el aturdimiento recupera el control");
            Assert.AreEqual(ConditionState.Normal, player.Condition.CurrentState);
        }

        [UnityTest]
        public IEnumerator Knockback_PushesAwayFromTheHit()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out _, typeof(PlayerWalkRun));
            var health = player.gameObject.AddComponent<CharacterHealth>();
            player.gameObject.AddComponent<CharacterKnockback>();
            yield return Settle(player);
            float x0 = player.transform.position.x;
            health.ApplyDamage(10f, Vector2.zero, new Vector2(1f, 0.5f), null);
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(player.transform.position.x - x0, 0.5f, "PlayerWalkRun no debería anular el empujón");
        }

        // ---------------- Entorno ----------------

        [UnityTest]
        public IEnumerator OneWayPlatform_JumpThroughFromBelow_LandOnTop()
        {
            _world.Ground();
            var platform = _world.Block("OneWay", new Vector2(0f, 3f), new Vector2(4f, 0.3f));
            platform.AddComponent<OneWayPlatform>();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerJump));
            yield return Settle(player);
            input.Jump = true; // mantenido: soltarlo antes recorta el salto (jump cut) y no llegaría
            yield return new WaitForSeconds(0.6f);
            input.Jump = false;
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(player.transform.position.y, 3f, "Debería atravesar desde abajo y quedar parado encima");
            Assert.IsTrue(player.Controller.IsGrounded);
        }

        [UnityTest]
        public IEnumerator FallingPlatform_FallsAfterBeingSteppedOn_ThenRespawns()
        {
            _world.Ground(-30f);
            var platform = _world.Block("Falling", new Vector2(0f, 0f), new Vector2(3f, 0.5f));
            platform.AddComponent<Rigidbody2D>();
            var falling = platform.AddComponent<FallingPlatform>();
            Set(falling, "fallDelay", 0.2f);
            Set(falling, "destroyDelay", 0.6f);
            Set(falling, "respawnDelay", 0.4f);
            var player = _world.Player(new Vector2(0f, 1.5f), out _);
            player.gameObject.tag = "Player";

            yield return new WaitForSeconds(0.9f);
            Assert.Less(platform.transform.position.y, -0.3f, "Tras pisarla debería caer");
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(platform.activeSelf, "Debería reaparecer tras respawnDelay");
            Assert.AreEqual(0f, platform.transform.position.y, 0.05f, "En su posición original");
        }

        [UnityTest]
        public IEnumerator Teleporters_PairDoesNotBounceThePlayerBack()
        {
            _world.Ground();
            var a = _world.Block("PortalA", new Vector2(0f, 0.5f), new Vector2(1f, 2f), trigger: true).AddComponent<Teleporter>();
            var b = _world.Block("PortalB", new Vector2(10f, 0.5f), new Vector2(1f, 2f), trigger: true).AddComponent<Teleporter>();
            Set(a, "destination", b.transform);
            Set(b, "destination", a.transform);
            var player = _world.Player(new Vector2(-3f, 0.5f), out _);
            player.gameObject.tag = "Player";
            yield return Settle(player);
            player.Controller.Rigidbody.position = new Vector2(0f, 0.5f);
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(10f, player.transform.position.x, 0.5f, "Llega al portal B y se queda ahí");
        }

        [UnityTest]
        public IEnumerator PushableBox_MovesWhenThePlayerWalksIntoIt()
        {
            _world.Ground();
            var box = _world.Block("Box", new Vector2(1.5f, 0f), new Vector2(1f, 1f));
            box.AddComponent<Rigidbody2D>().freezeRotation = true;
            box.AddComponent<PushableBox>();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            player.gameObject.tag = "Player";
            yield return Settle(player);
            input.Move = Vector2.right;
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(box.transform.position.x, 2.2f);
        }

        [UnityTest]
        public IEnumerator WindZone_PushesBodiesInside()
        {
            _world.Ground();
            _world.Block("Wind", new Vector2(0f, 1f), new Vector2(20f, 4f), trigger: true).AddComponent<WindZone2D>();
            var crate = _world.Block("Crate", new Vector2(0f, 0f), new Vector2(0.8f, 0.8f));
            var rb = crate.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            crate.GetComponent<Collider2D>().sharedMaterial = new PhysicsMaterial2D { friction = 0f };
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(crate.transform.position.x, 1f);
        }

        // ---------------- IA ----------------

        [UnityTest]
        public IEnumerator EnemyShootOnSight_ShootsVisibleTarget_NotThroughWalls()
        {
            _world.Ground();
            var target = _world.Block("Target", new Vector2(6f, 0.5f), new Vector2(0.6f, 1.5f)).AddComponent<DamageableObject>();
            var enemy = _world.Block("Enemy", new Vector2(0f, 0.5f), new Vector2(0.8f, 1.5f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            enemy.AddComponent<WeaponHitscan>();
            var shooter = enemy.AddComponent<EnemyShootOnSight>();
            Set(shooter, "target", target.transform);
            yield return new WaitForSeconds(0.5f);
            Assert.Less(target.CurrentHealth, target.MaxHealth, "Con línea de visión debería disparar");

            var wall = _world.Block("Wall", new Vector2(3f, 1f), new Vector2(0.5f, 4f));
            float hp = target.CurrentHealth;
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(hp, target.CurrentHealth, 0.01f, "Con una pared en medio no debería disparar");
        }

        [UnityTest]
        public IEnumerator EnemyMeleeOnContact_HurtsThePlayerOnTouch()
        {
            _world.Ground();
            var enemy = _world.Block("Enemy", new Vector2(2f, 0f), new Vector2(0.8f, 1f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            enemy.AddComponent<EnemyMeleeOnContact>();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            player.gameObject.tag = "Player";
            var health = player.gameObject.AddComponent<CharacterHealth>();
            yield return Settle(player);
            input.Move = Vector2.right;
            yield return new WaitForSeconds(1f);
            Assert.Less(health.CurrentHealth, health.MaxHealth);
        }
    }
}
