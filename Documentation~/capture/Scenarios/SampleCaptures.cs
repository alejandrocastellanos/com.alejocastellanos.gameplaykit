using System.Collections;
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
    public class SampleCaptures
    {
        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerWalkRun()
        {
            var s = new DocStage("PlayerWalkRun", new Vector2(3f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Run = true; yield return DocStage.Seconds(0.9f);
                input.Run = false; input.Move = Vector2.left; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerWallJump()
        {
            var s = new DocStage("PlayerWallJump", new Vector2(0f, 4f), 10f);
            try
            {
                s.World.Ground();
                s.World.Block("WallL", new Vector2(-2.6f, 5f), new Vector2(1f, 12f));
                s.World.Block("WallR", new Vector2(2.6f, 5f), new Vector2(1f, 12f));
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerWallJump));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Jump = true; input.Move = Vector2.right; yield return DocStage.Seconds(0.35f);
                input.Jump = false;
                for (int i = 0; i < 4; i++)
                {
                    yield return DocStage.Seconds(0.12f);
                    input.Jump = true; yield return DocStage.Seconds(0.15f); input.Jump = false;
                    input.Move = i % 2 == 0 ? Vector2.left : Vector2.right;
                    yield return DocStage.Seconds(0.2f);
                }
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator HazardZone()
        {
            var s = new DocStage("HazardZone", new Vector2(1f, 2.5f), 8f);
            try
            {
                s.World.Ground();
                var spikes = s.World.Block("Spikes", new Vector2(1.5f, -0.25f), new Vector2(2f, 0.5f), true);
                spikes.AddComponent<GameplayKit.Environment.HazardZone>();
                var go = new GameObject("Player");
                go.SetActive(false);
                s.World.Track(go);
                go.transform.position = new Vector2(-3f, 0.5f);
                go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
                go.AddComponent<CharacterController2D>();
                var input = go.AddComponent<ScriptedCharacterInput>();
                go.AddComponent<PlayerWalkRun>();
                var health = go.AddComponent<CharacterHealth>();
                s.PaintNow(go);
                go.AddComponent<DamageFlash>();
                go.AddComponent<CharacterCore>();
                go.SetActive(true);
                s.ShowInput(input);
                s.Caption(() => $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}");
                var p = go.GetComponent<CharacterCore>();
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.6f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
