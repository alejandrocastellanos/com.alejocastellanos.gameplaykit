using System.Collections;
using GameplayKit.AI;
using GameplayKit.Core;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    /// <summary>Regresiones de los bugs encontrados al documentar el kit.</summary>
    public class BugFixTests
    {
        private TestWorld _world;

        [SetUp]
        public void SetUp()
        {
            Application.runInBackground = true;
            _world = new TestWorld();
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
            Time.timeScale = 1f;
        }

        private static IEnumerator Settle(CharacterCore player, float timeout = 2f)
        {
            float t = 0f;
            while (!player.Controller.IsGrounded && t < timeout) { t += Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(0.2f);
        }

        [UnityTest]
        public IEnumerator CharacterCore_SkipsAbilitiesUncheckedInTheInspector()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
            yield return Settle(player);
            player.GetComponent<PlayerWalkRun>().enabled = false;
            float x0 = player.transform.position.x;
            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.4f);
            Assert.Less(Mathf.Abs(player.transform.position.x - x0), 0.05f, "Una habilidad desmarcada no debería moverlo");

            player.GetComponent<PlayerWalkRun>().enabled = true;
            yield return new WaitForSeconds(0.4f);
            Assert.Greater(player.transform.position.x - x0, 0.5f, "Al volver a marcarla, camina");
        }

        [UnityTest]
        public IEnumerator RopeGrab_RightPushesRight_AndSwingsLikeAPendulum()
        {
            _world.Ground(-20f);
            var anchorGo = _world.Track(new GameObject("Anchor"));
            anchorGo.transform.position = new Vector2(0f, 5f);
            var circle = anchorGo.AddComponent<CircleCollider2D>(); circle.isTrigger = true; circle.radius = 4f;
            anchorGo.AddComponent<RopeAnchor>();
            var player = _world.Player(new Vector2(0f, 2f), out var input, typeof(PlayerRopeGrab));
            var rope = player.GetComponent<PlayerRopeGrab>();
            yield return new WaitForSeconds(0.1f); // que el trigger del ancla registre al personaje
            input.TapInteract();
            yield return null; yield return null;
            Assert.IsTrue(rope.IsSwinging);
            float ropeLength = Vector2.Distance(player.transform.position, anchorGo.transform.position);

            input.Move = Vector2.right;
            yield return new WaitForSeconds(0.6f);
            Assert.Greater(player.transform.position.x, 0.5f, "Pulsar derecha balancea hacia la derecha");

            // Sin input, la gravedad lo devuelve más allá del centro (péndulo).
            input.Move = Vector2.zero;
            float minX = player.transform.position.x;
            for (float t = 0f; t < 2f; t += Time.deltaTime) { minX = Mathf.Min(minX, player.transform.position.x); yield return null; }
            Assert.Less(minX, -0.3f, "Suelto, oscila hacia el otro lado");
            Assert.AreEqual(ropeLength, Vector2.Distance(player.transform.position, anchorGo.transform.position), 0.25f, "La cuerda mantiene su largo");
        }

        [UnityTest]
        public IEnumerator WallCling_DoesNotEatTheWallJumpHeight()
        {
            _world.Ground();
            _world.Block("Wall", new Vector2(1.5f, 5f), new Vector2(1f, 12f));
            var player = _world.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerWallJump), typeof(PlayerWallCling));
            yield return Settle(player);
            input.Move = Vector2.right;
            input.Jump = true; yield return new WaitForSeconds(0.25f); input.Jump = false;
            yield return new WaitForSeconds(0.25f);
            Assert.IsTrue(player.GetComponent<PlayerWallJump>().IsTouchingWall, "Debería estar aferrado a la pared");

            float y0 = player.transform.position.y;
            input.Jump = true; // sigue empujando hacia la pared
            float maxY = y0;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, player.transform.position.y); yield return null; }
            Assert.Greater(maxY - y0, 1.5f, "El wall jump conserva su altura aunque se empuje hacia la pared");
        }

        [UnityTest]
        public IEnumerator EnemyChase_FindsThePlayerByTag_WhenItHasNoTarget()
        {
            _world.Ground();
            var player = _world.Block("Player", new Vector2(6f, 0f), new Vector2(0.8f, 1f));
            player.tag = "Player";
            var enemy = _world.Block("Enemy", new Vector2(0f, 0f), new Vector2(0.8f, 1f));
            enemy.AddComponent<Rigidbody2D>().freezeRotation = true;
            var chase = enemy.AddComponent<EnemyChase>();
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual(player.transform, chase.Target, "Sin objetivo asignado busca al objeto con tag Player");
            Assert.Greater(enemy.transform.position.x, 0.5f, "Y lo persigue");
        }

        [UnityTest]
        public IEnumerator PauseManager_SetsTheGameManagerPausedState()
        {
            var game = _world.Track(new GameObject("GameManager")).AddComponent<GameManager>();
            var pause = _world.Track(new GameObject("PauseManager")).AddComponent<PauseManager>();
            yield return null;
            pause.SetPaused(true);
            Assert.AreEqual(GameManager.GameState.Paused, game.CurrentState);
            pause.SetPaused(false);
            Assert.AreEqual(GameManager.GameState.Playing, game.CurrentState);

            game.SetState(GameManager.GameState.GameOver);
            pause.SetPaused(true);
            Assert.AreEqual(GameManager.GameState.GameOver, game.CurrentState, "Pausar no pisa un GameOver");
            pause.SetPaused(false);
        }

        [UnityTest]
        public IEnumerator Knockback_PushesTheSameForWeakAndStrongHits()
        {
            _world.Ground();
            var player = _world.Player(new Vector2(0f, 0.5f), out _, typeof(PlayerWalkRun));
            var health = player.gameObject.AddComponent<CharacterHealth>();
            player.gameObject.AddComponent<CharacterKnockback>();
            yield return Settle(player);
            float x0 = player.transform.position.x;
            health.ApplyDamage(1f, Vector2.zero, Vector2.right, null);
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(player.transform.position.x - x0, 1f, "Un golpe de 1 de daño también empuja");
        }

        [UnityTest]
        public IEnumerator GravityController_KeepsTheRigidbodyGravityScale()
        {
            _world.Ground();
            var go = _world.Track(new GameObject("Player"));
            go.SetActive(false);
            go.transform.position = new Vector2(0f, 0.5f);
            go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
            go.AddComponent<Rigidbody2D>().gravityScale = 2.5f;
            go.AddComponent<CharacterController2D>();
            go.AddComponent<ScriptedCharacterInput>();
            go.AddComponent<CharacterGravityController>();
            var core = go.AddComponent<CharacterCore>();
            go.SetActive(true);
            yield return Settle(core);
            Assert.AreEqual(2.5f, core.Controller.DefaultGravityScale, 0.001f, "Con Base Gravity Scale en 0 respeta el Rigidbody2D");
        }
    }
}
