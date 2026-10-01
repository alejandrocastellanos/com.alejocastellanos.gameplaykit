using System.Collections;
using GameplayKit.Core;
using GameplayKit.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    public class MovementTests
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

        private static IEnumerator Settle(CharacterCore player, float timeout = 2f)
        {
            float t = 0f;
            while (!player.Controller.IsGrounded && t < timeout) { t += Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.2f);
        }

        [UnityTest]
        public IEnumerator Controller_DetectsGround_WithoutConfiguration()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 2f), out _);
            Assert.IsFalse(player.Controller.IsGrounded, "Recién creado en el aire no debería estar en el suelo");
            yield return Settle(player);
            Assert.IsTrue(player.Controller.IsGrounded, "Con valores por defecto debería detectar el suelo al aterrizar");
        }

        [UnityTest]
        public IEnumerator WalkRun_MovesInInputDirection_AndRunIsFaster()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            yield return Settle(player);

            float x0 = player.transform.position.x;
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.5f);
            float walked = player.transform.position.x - x0;

            float x1 = player.transform.position.x;
            input.Run = true;
            yield return new WaitForSeconds(0.5f);
            float ran = player.transform.position.x - x1;

            Assert.Greater(walked, 1f, "Caminando medio segundo debería avanzar");
            Assert.Greater(ran, walked * 1.3f, "Corriendo debería avanzar más que caminando");

            input.Run = false;
            input.Move = Vector2.left;
            yield return new WaitForSeconds(0.1f);
            Assert.Less(player.transform.localScale.x, 0f, "Al moverse a la izquierda debería voltearse");
        }

        [UnityTest]
        public IEnumerator Jump_RisesAndLandsAgain()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerJump));
            yield return Settle(player);

            float groundY = player.transform.position.y;
            float peak = groundY;
            input.Jump = true;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                peak = Mathf.Max(peak, player.transform.position.y);
                yield return null;
            }
            input.Jump = false;
            Assert.Greater(peak - groundY, 1.5f, "El salto debería elevar al personaje");

            yield return Settle(player, 3f);
            Assert.IsTrue(player.Controller.IsGrounded, "Debería volver a aterrizar");
        }

        [UnityTest]
        public IEnumerator Jump_CannotJumpAgainInTheAir()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerJump));
            yield return Settle(player);

            input.Jump = true; yield return null; yield return null;
            input.Jump = false;
            yield return new WaitForSeconds(0.45f); // cerca del punto más alto, ya cayendo
            float vyBefore = player.Controller.Velocity.y;
            input.Jump = true; yield return null; yield return null;
            Assert.LessOrEqual(player.Controller.Velocity.y, vyBefore + 0.5f, "PlayerJump solo no debe permitir saltar en el aire");
        }

        [UnityTest]
        public IEnumerator MultiJump_AllowsOneAirJump()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerMultiJump));
            yield return Settle(player);

            input.Jump = true; yield return null; yield return null;
            input.Jump = false;
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(player.Controller.IsGrounded);
            input.Jump = true; yield return null; yield return null;
            Assert.Greater(player.Controller.Velocity.y, 5f, "El salto en el aire debería impulsar hacia arriba");
            input.Jump = false;
            yield return new WaitForSeconds(0.5f);
            input.Jump = true; yield return null; yield return null;
            Assert.Less(player.Controller.Velocity.y, 5f, "Con extraJumps = 1 no debería haber un tercer salto");
        }

        [UnityTest]
        public IEnumerator Dash_CoversMoreDistanceThanWalking()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerDash));
            yield return Settle(player);

            float x0 = player.transform.position.x;
            input.TapDash();
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(player.transform.position.x - x0, 1.5f, "El dash debería desplazar hacia donde mira");
        }

        [UnityTest]
        public IEnumerator WallJump_DetectsWallInFront_AndPushesAway()
        {
            _world.Ground();
            _world.Block("Wall", new Vector2(1f, 3f), new Vector2(1f, 8f));
            var player = _world.Player(new Vector2(0f, 3f), out var input, typeof(PlayerWallJump));
            var wallJump = player.GetComponent<PlayerWallJump>();
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(wallJump.IsTouchingWall, "Mirando hacia una pared pegada debería detectarla");
            input.Jump = true; yield return null; yield return null;
            Assert.Less(player.Controller.Velocity.x, -1f, "El wall jump debería alejarlo de la pared");
            Assert.Greater(player.Controller.Velocity.y, 1f, "El wall jump debería impulsar hacia arriba");
        }

        [UnityTest]
        public IEnumerator WallJump_DoesNotDetectWallBehind()
        {
            _world.Ground();
            _world.Block("Wall", new Vector2(-1f, 3f), new Vector2(1f, 8f));
            var player = _world.Player(new Vector2(0f, 3f), out _, typeof(PlayerWallJump));
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(player.GetComponent<PlayerWallJump>().IsTouchingWall, "Una pared a la espalda no cuenta");
        }

        [UnityTest]
        public IEnumerator WallJump_WithWalkRun_KeepsHorizontalKick()
        {
            _world.Ground();
            _world.Block("Wall", new Vector2(1f, 3f), new Vector2(1f, 8f));
            var player = _world.Player(new Vector2(0f, 3f), out var input, typeof(PlayerWalkRun), typeof(PlayerWallJump));
            input.Move = Vector2.right; // empujando contra la pared, como haría un jugador
            yield return new WaitForSeconds(0.1f);
            float x0 = player.transform.position.x;
            input.Jump = true;
            yield return new WaitForSeconds(0.15f);
            Assert.Less(player.transform.position.x, x0 - 0.3f,
                "Con PlayerWalkRun presente, el impulso horizontal del wall jump no debería anularse en el siguiente frame");
        }

        [UnityTest]
        public IEnumerator WallSlide_LimitsFallSpeed()
        {
            _world.Ground(-20f);
            _world.Block("Wall", new Vector2(1f, 0f), new Vector2(1f, 30f));
            var player = _world.Player(new Vector2(0f, 5f), out var input, typeof(PlayerWallJump), typeof(PlayerWallSlide));
            input.Move = Vector2.right;
            yield return new WaitForSeconds(1f);
            Assert.GreaterOrEqual(player.Controller.Velocity.y, -2.5f, "Deslizando por la pared la caída debería estar limitada");
        }

        [UnityTest]
        public IEnumerator Glide_LimitsFallSpeedWhileHoldingJump()
        {
            _world.Ground(-50f);
            var player = _world.Player(new Vector2(0f, 10f), out var input, typeof(PlayerGlide));
            input.Jump = true;
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(player.GetComponent<PlayerGlide>().IsGliding);
            Assert.GreaterOrEqual(player.Controller.Velocity.y, -2.5f);
        }

        [UnityTest]
        public IEnumerator Jetpack_AscendsAndConsumesFuel()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerJetpack));
            var jetpack = player.GetComponent<PlayerJetpack>();
            yield return Settle(player);
            float y0 = player.transform.position.y;
            float fuel0 = jetpack.CurrentFuel;
            input.Jump = true;
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(player.transform.position.y - y0, 2f, "El jetpack debería elevar");
            Assert.Less(jetpack.CurrentFuel, fuel0, "Debería gastar combustible");
        }

        [UnityTest]
        public IEnumerator Crouch_ShrinksColliderAndKeepsFeetPlanted()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerCrouch));
            var body = player.GetComponent<CapsuleCollider2D>();
            yield return Settle(player);
            float standingHeight = body.size.y;
            float feet = body.bounds.min.y;

            input.Crouch = true;
            yield return new WaitForSeconds(0.2f);
            Assert.Less(body.size.y, standingHeight * 0.8f, "Agachado el collider debería ser más bajo");
            Assert.AreEqual(feet, body.bounds.min.y, 0.1f, "Los pies no deberían despegarse del suelo al agacharse");

            input.Crouch = false;
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(standingHeight, body.size.y, 0.001f, "Al levantarse vuelve exactamente a su tamaño original");
        }

        [UnityTest]
        public IEnumerator Ladder_ClimbsUpWithoutCustomTags()
        {
            _world.Ground();
            var ladder = _world.Block("Ladder", new Vector2(0f, 4f), new Vector2(1f, 10f), trigger: true);
            ladder.AddComponent<LadderZone>();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerClimbLadder));
            yield return Settle(player);
            float y0 = player.transform.position.y;
            input.Move = Vector2.up;
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(player.GetComponent<PlayerClimbLadder>().IsOnLadder);
            Assert.Greater(player.transform.position.y - y0, 1f, "Debería subir por la escalera");
        }

        [UnityTest]
        public IEnumerator Swim_InsideWaterZone()
        {
            _world.Ground(-20f);
            var water = _world.Block("Water", new Vector2(0f, -5f), new Vector2(20f, 20f), trigger: true);
            water.AddComponent<GameplayKit.Environment.WaterZone>();
            var player = _world.Player(new Vector2(0f, 0f), out var input, typeof(PlayerSwim));
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(player.GetComponent<PlayerSwim>().IsInWater, "Dentro de una WaterZone debería nadar, sin necesitar el tag Water");
            float x0 = player.transform.position.x;
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(player.transform.position.x - x0, 0.8f);
        }
    }
}
