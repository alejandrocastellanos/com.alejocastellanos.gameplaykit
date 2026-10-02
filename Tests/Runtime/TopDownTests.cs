using System.Collections;
using GameplayKit.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    /// <summary>Comportamientos de enemigos en modo top-down (sin gravedad, movimiento en 2 ejes).</summary>
    public class TopDownTests
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

        private GameObject Enemy<T>(Vector2 position) where T : Component
        {
            var go = _world.Track(new GameObject("Enemy"));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            go.AddComponent<Rigidbody2D>().freezeRotation = true;
            var behaviour = go.AddComponent<T>();
            TestWorld.Set(behaviour, "moveVertically", true);
            go.SetActive(true);
            return go;
        }

        [UnityTest]
        public IEnumerator EnemyChase_MoveVertically_ChasesOnBothAxesWithoutFalling()
        {
            var player = _world.Block("Player", new Vector2(0f, 5f), new Vector2(0.8f, 0.8f));
            player.tag = "Player";
            var enemy = Enemy<EnemyChase>(Vector2.zero);
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0f, enemy.GetComponent<Rigidbody2D>().gravityScale, "En top-down no usa gravedad");
            Assert.Greater(enemy.transform.position.y, 1.5f, "Persigue hacia arriba");
            Assert.Less(Mathf.Abs(enemy.transform.position.x), 0.3f, "Sin desviarse en X");
        }

        [UnityTest]
        public IEnumerator EnemyFlee_MoveVertically_RunsAwayOnBothAxes()
        {
            var player = _world.Block("Player", new Vector2(0f, -1f), new Vector2(0.8f, 0.8f));
            player.tag = "Player";
            var enemy = Enemy<EnemyFlee>(Vector2.zero);
            yield return new WaitForSeconds(0.6f);
            Assert.Greater(enemy.transform.position.y, 1f, "Huye hacia arriba, lejos del jugador");
        }
    }
}
