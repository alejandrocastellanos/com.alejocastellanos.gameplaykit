using System;
using System.Collections;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Movement;
using GameplayKit.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.DocCapture
{
    /// <summary>Llama a Tick en cada LateUpdate (líneas de cuerda, tintes y rastros que siguen al personaje).</summary>
    public class MovementBTicker : MonoBehaviour
    {
        public Action Tick;
        private void LateUpdate() => Tick?.Invoke();
    }

    public class MovementBCaptures
    {
        private static readonly Color Rope = DocPalette.Hex("FFE082");

        private static void OnEachFrame(DocStage s, Action tick)
        {
            var go = s.World.Track(new GameObject("~ticker"));
            go.AddComponent<DocNoPaint>();
            go.AddComponent<MovementBTicker>().Tick = tick;
        }

        private static IEnumerator Until(Func<bool> condition, float timeout)
        {
            for (float t = 0f; !condition() && t < timeout; t += Time.deltaTime) yield return null;
        }

        // ---------------------------------------------------------------- Ledges

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerLedgeGrab_Clip()
        {
            var s = new DocStage("PlayerLedgeGrab", new Vector2(1.5f, 2.8f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Platform", new Vector2(3.5f, 2f), new Vector2(5f, 5f)); // x 1..6, top 4.5
                var p = s.World.Player(new Vector2(-2.5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerLedgeGrab));
                TestWorld.Set(p.GetComponent<PlayerJump>(), "jumpForce", 9f);
                var grab = p.GetComponent<PlayerLedgeGrab>();
                s.ShowInput(input);
                s.Caption(() => grab.IsGrabbingLedge ? "Hanging" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; input.Jump = true;
                yield return Until(() => grab.IsGrabbingLedge, 2.5f);
                input.Jump = false;
                yield return DocStage.Seconds(1.0f);
                input.Move = new Vector2(-1f, -1f); yield return DocStage.Seconds(0.15f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerLedgeClimb_Clip()
        {
            var s = new DocStage("PlayerLedgeClimb", new Vector2(1.5f, 2.8f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Platform", new Vector2(3.5f, 2f), new Vector2(5f, 5f)); // x 1..6, top 4.5
                var p = s.World.Player(new Vector2(-2.5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerLedgeGrab), typeof(PlayerLedgeClimb));
                TestWorld.Set(p.GetComponent<PlayerJump>(), "jumpForce", 9f);
                var grab = p.GetComponent<PlayerLedgeGrab>();
                var climb = p.GetComponent<PlayerLedgeClimb>();
                s.ShowInput(input);
                s.Caption(() => climb.IsClimbing ? "Climbing" : grab.IsGrabbingLedge ? "Hanging" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; input.Jump = true;
                yield return Until(() => grab.IsGrabbingLedge, 2.5f);
                input.Jump = false;
                yield return DocStage.Seconds(0.6f);
                input.Jump = true; yield return DocStage.Seconds(0.15f);
                input.Jump = false;
                yield return Until(() => !grab.IsGrabbingLedge, 1f);
                yield return DocStage.Seconds(0.8f); // Move sigue a la derecha: camina sobre la plataforma
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerEdgeDangle_Clip()
        {
            var s = new DocStage("PlayerEdgeDangle", new Vector2(0f, 1.5f), 7f);
            try
            {
                s.World.Block("Ledge", new Vector2(-2f, -1f), new Vector2(8f, 1f)); // x -6..2, top -0.5
                var p = s.World.Player(new Vector2(-3.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerEdgeDangle));
                var dangle = p.GetComponent<PlayerEdgeDangle>();
                s.ShowInput(input);
                s.Caption(() => dangle.IsAtEdge ? "IsAtEdge true" : "IsAtEdge false");
                OnEachFrame(s, () =>
                {
                    if (p == null) return;
                    s.Tint(p.gameObject, dangle.IsAtEdge ? DocPalette.Accent : DocPalette.Player);
                });
                yield return DocStage.Settle(p);
                s.StartRecording(4.2f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= 1.75f, 2.5f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.2f);
                input.Move = Vector2.left; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Ladders

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerClimbLadder_Clip()
        {
            var s = new DocStage("PlayerClimbLadder", new Vector2(0.5f, 2.3f), 8f);
            try
            {
                s.World.Ground();
                s.World.Block("Platform", new Vector2(3.5f, 1.5f), new Vector2(5f, 4f)); // x 1..6, top 3.5
                var ladder = s.World.Block("Ladder", new Vector2(0f, 2f), new Vector2(1f, 5f), true); // top 4.5
                ladder.AddComponent<LadderZone>();
                var p = s.World.Player(new Vector2(-3.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerClimbLadder));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= -0.05f, 2f);
                input.Move = Vector2.up;
                yield return Until(() => p.transform.position.y >= 4.5f, 2.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator LadderZone_Clip()
        {
            var s = new DocStage("LadderZone", new Vector2(0f, 2.3f), 8f);
            try
            {
                s.World.Ground();
                var ladder = s.World.Block("Ladder", new Vector2(0f, 2.5f), new Vector2(1f, 6f), true);
                ladder.AddComponent<LadderZone>();
                var p = s.World.Player(new Vector2(-2.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerClimbLadder));
                var climb = p.GetComponent<PlayerClimbLadder>();
                s.ShowInput(input);
                s.Caption(() => climb.IsOnLadder ? "IsOnLadder true" : "IsOnLadder false");
                yield return DocStage.Settle(p);
                s.StartRecording(5.2f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f); // la atraviesa sin engancharse
                input.Move = Vector2.left;
                yield return Until(() => p.transform.position.x <= 0.05f, 1.5f);
                input.Move = Vector2.up; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.6f); // quieto, sin resbalar
                input.Move = Vector2.down;
                yield return Until(() => !climb.IsOnLadder && p.Controller.IsGrounded, 2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Rope

        private static GameObject RopeAnchorAt(DocStage s, Vector2 position, float radius, out CircleCollider2D circle)
        {
            var anchor = s.World.Track(new GameObject("Anchor"));
            anchor.transform.position = position;
            circle = anchor.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = radius;
            anchor.AddComponent<RopeAnchor>();
            s.Tint(anchor, DocPalette.Hex("FFE082", 0.12f));
            s.Marker(position, Rope, 0.3f);
            return anchor;
        }

        private static void RopeLine(DocStage s, Vector2 anchor, CharacterCore p, PlayerRopeGrab rope)
        {
            var line = s.Line(Rope, 0.07f, anchor, anchor);
            line.enabled = false;
            OnEachFrame(s, () =>
            {
                if (p == null || line == null) return;
                line.enabled = rope.IsSwinging;
                line.SetPosition(1, p.transform.position);
            });
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerRopeGrab_Clip()
        {
            var s = new DocStage("PlayerRopeGrab", new Vector2(0f, 3f), 8f);
            try
            {
                s.World.Ground();
                var anchorPos = new Vector2(0f, 5.5f);
                RopeAnchorAt(s, anchorPos, 2.5f, out _);
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerRopeGrab));
                TestWorld.Set(p.GetComponent<PlayerJump>(), "jumpForce", 9f);
                var rope = p.GetComponent<PlayerRopeGrab>();
                RopeLine(s, anchorPos, p, rope);
                // Sin ShowInput: con la implementación actual, derecha gira en sentido horario (hacia la izquierda
                // bajo el ancla) y el overlay confundiría.
                s.Caption(() => rope.IsSwinging ? "Swinging" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(5.5f);
                yield return DocStage.Seconds(0.3f);
                input.Jump = true;
                for (float t = 0f; !rope.IsSwinging && t < 1.5f; t += Time.deltaTime) { input.TapInteract(); yield return null; }
                input.Jump = false;
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.left;  yield return DocStage.Seconds(1.2f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero;  yield return DocStage.Seconds(0.3f);
                input.Jump = true; yield return DocStage.Seconds(0.2f);
                input.Jump = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator RopeAnchor_Clip()
        {
            var s = new DocStage("RopeAnchor", new Vector2(0f, 3f), 8f);
            try
            {
                s.World.Ground();
                var anchorPos = new Vector2(0f, 5.5f);
                RopeAnchorAt(s, anchorPos, 2.5f, out var circle);
                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerRopeGrab));
                TestWorld.Set(p.GetComponent<PlayerJump>(), "jumpForce", 9f);
                var rope = p.GetComponent<PlayerRopeGrab>();
                var body = p.GetComponent<Collider2D>();
                RopeLine(s, anchorPos, p, rope);
                s.ShowInput(input);
                s.Caption(() => rope.IsSwinging ? "Grabbed" : body.Distance(circle).isOverlapped ? "In range" : "Out of range");
                yield return DocStage.Settle(p);
                s.StartRecording(4.8f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => p.transform.position.x >= -0.05f, 1.5f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.3f);
                input.TapInteract(); yield return DocStage.Seconds(0.5f); // fuera de alcance: no pasa nada
                input.Jump = true;
                for (float t = 0f; !rope.IsSwinging && t < 1.5f; t += Time.deltaTime) { input.TapInteract(); yield return null; }
                input.Jump = false;
                yield return DocStage.Seconds(0.8f);
                input.Jump = true; yield return DocStage.Seconds(0.2f);
                input.Jump = false;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Zipline

        private static ZiplinePath Zipline(DocStage s, Vector2 trigger, Vector2 start, Vector2 end)
        {
            var zipGo = s.World.Block("Zipline", trigger, new Vector2(0.6f, 1.6f), true);
            var path = zipGo.AddComponent<ZiplinePath>();
            path.Start = s.World.Point(start).transform;
            path.End = s.World.Point(end).transform;
            var cable = Vector2.up * 0.95f; // el cable va justo sobre la cabeza del personaje colgado
            s.Line(Rope, 0.06f, start + cable, end + cable);
            s.Marker(start + cable, Rope, 0.25f);
            s.Marker(end + cable, Rope, 0.25f);
            return path;
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerZipline_Clip()
        {
            var s = new DocStage("PlayerZipline", new Vector2(-1.5f, 2.2f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("HighPlatform", new Vector2(-6.5f, 1.25f), new Vector2(5f, 3.5f)); // x -9..-4, top 3
                Zipline(s, new Vector2(-4.3f, 4.2f), new Vector2(-4.3f, 4.2f), new Vector2(5f, 1.2f));
                var p = s.World.Player(new Vector2(-7.5f, 4f), out var input, typeof(PlayerWalkRun), typeof(PlayerZipline));
                var zip = p.GetComponent<PlayerZipline>();
                s.ShowInput(input);
                s.Caption(() => zip.IsRiding ? "Riding" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => zip.IsRiding, 1.5f);
                input.Move = Vector2.zero;
                yield return Until(() => !zip.IsRiding, 2.5f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator ZiplinePath_Clip()
        {
            var s = new DocStage("ZiplinePath", new Vector2(0f, 2.5f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("HighPlatform", new Vector2(5.5f, 1.5f), new Vector2(4f, 4f)); // x 3.5..7.5, top 3.5
                Zipline(s, new Vector2(-4.5f, 0.6f), new Vector2(-4.5f, 0.7f), new Vector2(4.2f, 5.2f)); // cuesta arriba
                var p = s.World.Player(new Vector2(-7f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerZipline));
                var zip = p.GetComponent<PlayerZipline>();
                s.ShowInput(input);
                s.Caption(() => zip.IsRiding ? "Riding" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => zip.IsRiding, 1.5f);
                input.Move = Vector2.zero;
                yield return Until(() => !zip.IsRiding, 2.5f);
                yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Air

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerGlide_Clip()
        {
            var s = new DocStage("PlayerGlide", new Vector2(-1f, 1.6f), 7f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-6.5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerGlide));
                TestWorld.Set(p.GetComponent<PlayerJump>(), "jumpForce", 8f);
                TestWorld.Set(p.GetComponent<PlayerWalkRun>(), "walkSpeed", 2.5f);
                var glide = p.GetComponent<PlayerGlide>();
                s.ShowInput(input);
                s.Caption(() => glide.IsGliding ? "Gliding" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(5.8f);
                int frame = 0;
                OnEachFrame(s, () =>
                {
                    if (p == null || p.Controller.IsGrounded || ++frame % 2 != 0) return;
                    s.Marker(p.transform.position, glide.IsGliding ? DocPalette.Accent : DocPalette.Hex("FFFFFF", 0.5f), 0.12f);
                });
                yield return DocStage.Seconds(0.3f);

                // 1) Salto normal: suelta el botón en la cima y cae rápido.
                input.Move = Vector2.right; input.Jump = true;
                yield return DocStage.Seconds(0.3f);
                yield return Until(() => p.Controller.Velocity.y < 0f, 1.5f);
                input.Jump = false;
                yield return Until(() => p.Controller.IsGrounded, 1.5f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.4f);

                // 2) Mismo salto manteniendo el botón: planea.
                input.Move = Vector2.right; input.Jump = true;
                yield return DocStage.Seconds(0.3f);
                yield return Until(() => p.Controller.IsGrounded, 3f);
                input.Jump = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerJetpack_Clip()
        {
            var s = new DocStage("PlayerJetpack", new Vector2(0f, 2.3f), 7.5f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerJetpack));
                var jet = p.GetComponent<PlayerJetpack>();
                // Valores más suaves que los de fábrica para que el vuelo quepa en cuadro y el tanque se vacíe en el clip.
                TestWorld.Set(jet, "thrust", 4f);
                TestWorld.Set(jet, "fuelDrainPerSecond", 125f);
                s.ShowInput(input);
                s.Caption(() => $"Fuel {jet.CurrentFuel:0}%");
                yield return DocStage.Settle(p);
                s.StartRecording(5.8f);
                yield return DocStage.Seconds(0.3f);
                input.Jump = true;
                yield return Until(() => jet.CurrentFuel <= 0f, 1.5f);
                yield return DocStage.Seconds(0.2f); // sin combustible, mantener no hace nada
                input.Jump = false;
                yield return Until(() => p.Controller.IsGrounded, 2f);
                yield return DocStage.Seconds(1.2f); // recarga en el suelo
                input.Jump = true;
                yield return Until(() => jet.CurrentFuel <= 0f, 1f);
                input.Jump = false;
                yield return DocStage.Seconds(0.1f);
                yield return Until(() => p.Controller.IsGrounded, 2f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerFly_Clip()
        {
            var s = new DocStage("PlayerFly", new Vector2(-1f, 2f), 7f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerFly));
                var fly = p.GetComponent<PlayerFly>();
                s.ShowInput(input);
                s.Caption(() => fly.IsFlying ? "Fly ON" : "Fly OFF");
                yield return DocStage.Settle(p);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.TapAction(CharacterAction.Fly); yield return DocStage.Seconds(0.25f);
                input.Move = Vector2.up; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);
                input.Move = new Vector2(0.7071f, -0.7071f); yield return DocStage.Seconds(0.4f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.5f); // flota
                input.TapAction(CharacterAction.Fly);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Water

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerSwim_Clip()
        {
            var s = new DocStage("PlayerSwim", new Vector2(0f, -1.5f), 8f);
            try
            {
                s.World.Block("BankL", new Vector2(-5f, -2f), new Vector2(6f, 4f));      // x -8..-2, top 0
                s.World.Block("PoolFloor", new Vector2(2f, -4.5f), new Vector2(16f, 1f)); // top -4
                s.World.Block("BankR", new Vector2(8f, -2f), new Vector2(4f, 4f));       // x 6..10, top 0
                var water = s.World.Block("Water", new Vector2(2f, -2.2f), new Vector2(8f, 3.6f), true); // x -2..6, y -4..-0.4
                water.AddComponent<WaterZone>();
                var p = s.World.Player(new Vector2(-5.5f, 1f), out var input, typeof(PlayerWalkRun), typeof(PlayerSwim));
                var swim = p.GetComponent<PlayerSwim>();
                s.ShowInput(input);
                s.Caption(() => swim.IsInWater ? "In water" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right;
                yield return Until(() => swim.IsInWater, 2f);
                yield return DocStage.Seconds(0.2f);
                input.Move = new Vector2(0.6f, -0.8f); yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.up;
                yield return Until(() => !swim.IsInWater, 1.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------------------------------------------------------- Path

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerPathFollow_Clip()
        {
            var s = new DocStage("PlayerPathFollow", new Vector2(0f, 2f), 7.5f);
            try
            {
                s.World.Ground();
                s.World.Block("Wall", new Vector2(0f, 0.5f), new Vector2(1f, 2f)); // y -0.5..1.5
                var a = new Vector2(-3f, 2.8f);
                var b = new Vector2(2.5f, 2.8f);
                var c = new Vector2(4.5f, 0.8f);
                var go = s.PlayerShell(new Vector2(-5f, 0.5f), out _);
                var follow = go.AddComponent<PlayerPathFollow>();
                TestWorld.Set(follow, "waypoints", new[] { s.World.Point(a).transform, s.World.Point(b).transform, s.World.Point(c).transform });
                TestWorld.Set(follow, "loop", false);
                var p = s.Activate(go);
                s.Line(DocPalette.Hex("FFD54F", 0.5f), 0.05f, new Vector2(-5f, 0.4f), a, b, c);
                s.Marker(a, DocPalette.Accent, 0.25f);
                s.Marker(b, DocPalette.Accent, 0.25f);
                s.Marker(c, DocPalette.Accent, 0.25f);
                s.Caption(() => follow.IsFollowingPath ? "Following" : "");
                yield return DocStage.Settle(p);
                s.StartRecording(5.2f);
                yield return DocStage.Seconds(0.4f);
                follow.IsFollowingPath = true;
                yield return Until(() => !follow.IsFollowingPath, 4.5f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
