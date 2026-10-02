using System.Collections;
using GameplayKit.CameraSystem;
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
    /// <summary>Clips de documentación de movimiento (grupo A). PlayerWalkRun y PlayerWallJump están en SampleCaptures.</summary>
    public class MovementACaptures
    {
        private static readonly Color GhostColor = DocPalette.Hex("4FC3F7", 0.35f);

        // ------------------------------------------------------------------ helpers

        /// <summary>Personaje cenital: círculo de radio 0.4 (como el menú Create Top-Down Player), sin gravedad.</summary>
        private static CharacterCore TopDownPlayer(DocStage s, Vector2 position, out ScriptedCharacterInput input, params System.Type[] abilities)
        {
            var go = s.World.Track(new GameObject("Player"));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            go.AddComponent<CharacterController2D>();
            input = go.AddComponent<ScriptedCharacterInput>();
            foreach (var ability in abilities) go.AddComponent(ability);
            var core = s.Activate(go);
            s.Trail(go, true);
            return core;
        }

        /// <summary>Habitación vista desde arriba: interior de 12 × 6.6 centrado en el origen.</summary>
        private static void TopDownRoom(DocStage s)
        {
            s.World.Block("WallTop", new Vector2(0f, 3.55f), new Vector2(13f, 0.5f));
            s.World.Block("WallBottom", new Vector2(0f, -3.55f), new Vector2(13f, 0.5f));
            s.World.Block("WallLeft", new Vector2(-6.25f, 0f), new Vector2(0.5f, 7.6f));
            s.World.Block("WallRight", new Vector2(6.25f, 0f), new Vector2(0.5f, 7.6f));
        }

        private static void Ghost(DocStage s, Transform t) =>
            s.Flash(t.position, new Vector2(0.8f, 1.8f), GhostColor, 0.4f);

        // ------------------------------------------------------------------ clips

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerJump_Clip()
        {
            var s = new DocStage("PlayerJump", new Vector2(-1f, 4.2f), 11f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerJump));
                string label = "";
                s.ShowInput(input);
                s.Caption(() => label);
                yield return DocStage.Settle(p);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);

                // Toque corto: el salto se recorta al soltar.
                label = "Tap";
                input.Move = Vector2.right; input.Jump = true;
                yield return DocStage.Seconds(0.07f);
                input.Jump = false;
                yield return DocStage.Seconds(1.25f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(0.35f);

                // Mantenido hasta pasar el punto más alto: salto completo.
                label = "Hold";
                input.Move = Vector2.right; input.Jump = true;
                yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(0.8f);
                input.Jump = false;
                yield return DocStage.Seconds(1.3f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerMultiJump_Clip()
        {
            var s = new DocStage("PlayerMultiJump", new Vector2(0f, 4f), 11f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-3f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerMultiJump));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);

                input.Move = Vector2.right; input.Jump = true;           // salto desde el suelo (corto)
                yield return DocStage.Seconds(0.07f);
                input.Jump = false;
                yield return DocStage.Seconds(0.45f);
                input.Jump = true;                                        // salto en el aire
                yield return DocStage.Seconds(0.45f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(0.65f);
                input.Jump = false;
                yield return DocStage.Seconds(0.3f);
                input.Jump = true;                                        // tercer intento: sin efecto
                yield return DocStage.Seconds(0.1f);
                input.Jump = false;
                yield return DocStage.Seconds(1.0f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerCrouch_Clip()
        {
            var s = new DocStage("PlayerCrouch", new Vector2(0f, 1.6f), 6.5f);
            try
            {
                s.World.Ground();
                // Saliente bajo: su base queda a 1.25 sobre el suelo (de pie mide 1.8, agachado 0.9).
                s.World.Block("Ceiling", new Vector2(0f, 1.5f), new Vector2(2f, 1.5f));
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerCrouch));
                var crouch = p.GetComponent<PlayerCrouch>();
                s.ShowInput(input);
                s.Caption(() => $"{(crouch.IsCrouching ? "Crouch" : "Stand")}  {Mathf.Abs(p.Controller.Velocity.x):0.0} u/s");
                yield return DocStage.Settle(p);
                s.StartRecording(4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);
                input.Crouch = true; yield return DocStage.Seconds(1.9f);
                input.Crouch = false; yield return DocStage.Seconds(0.55f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerCrawl_Clip()
        {
            var s = new DocStage("PlayerCrawl", new Vector2(0f, 1.6f), 6.5f);
            try
            {
                s.World.Ground();
                s.World.Block("Ceiling", new Vector2(0f, 1.5f), new Vector2(2.4f, 1.5f));
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerCrouch), typeof(PlayerCrawl));
                s.ShowInput(input);
                s.Caption(() => $"{Mathf.Abs(p.Controller.Velocity.x):0.0} u/s");
                yield return DocStage.Settle(p);
                s.StartRecording(4.7f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.75f);
                input.Crouch = true; yield return DocStage.Seconds(2.55f);
                input.Crouch = false; yield return DocStage.Seconds(0.55f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerDash_Clip()
        {
            var s = new DocStage("PlayerDash", new Vector2(0f, 2f), 7f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-4f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerDash));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(3.4f);
                yield return DocStage.Seconds(0.3f);

                // Dash en el suelo hacia la derecha.
                input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
                input.TapDash(); yield return DocStage.Seconds(0.6f);

                // Saltito y dash aéreo hacia la izquierda: avanza en línea recta.
                input.Move = Vector2.left; input.Jump = true;
                yield return DocStage.Seconds(0.07f);
                input.Jump = false;
                yield return DocStage.Seconds(0.35f);
                input.TapDash(); yield return DocStage.Seconds(0.2f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(0.9f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerDash8Directions_Clip()
        {
            var s = new DocStage("PlayerDash8Directions", Vector2.zero, 9f);
            try
            {
                TopDownRoom(s);
                TopDownPlayer(s, new Vector2(-3f, -1.5f), out var input, typeof(PlayerTopDownMovement), typeof(PlayerDash8Directions));
                s.ShowInput(input);
                yield return DocStage.Seconds(0.3f);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);

                var directions = new[]
                {
                    new Vector2(1f, 1f), Vector2.right, new Vector2(1f, -1f),
                    Vector2.up, Vector2.left, new Vector2(-1f, -1f)
                };
                foreach (var dir in directions)
                {
                    input.Move = dir; input.TapDash();
                    yield return DocStage.Seconds(0.17f);
                    input.Move = Vector2.zero;
                    yield return DocStage.Seconds(0.45f);
                }
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerGroundSlam_Clip()
        {
            var s = new DocStage("PlayerGroundSlam", new Vector2(0f, 4f), 11f);
            try
            {
                s.World.Ground();
                var shake = s.Camera.gameObject.AddComponent<CameraShake>();
                var p = s.World.Player(new Vector2(-3f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerGroundSlam));
                p.GetComponent<PlayerGroundSlam>().OnSlamLanded += () =>
                {
                    s.Flash(new Vector2(p.transform.position.x, -0.3f), new Vector2(3f, 0.4f), DocPalette.Accent, 0.25f);
                    shake.Shake(0.25f, 0.2f);
                };
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(3.8f);
                yield return DocStage.Seconds(0.3f);

                // Salto alto hacia la derecha y golpe en picada.
                input.Move = Vector2.right; input.Jump = true;
                yield return DocStage.Seconds(0.75f);
                input.Jump = false;
                yield return DocStage.Seconds(0.05f);
                input.Move = Vector2.down; input.TapDash();
                yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(0.4f);

                // Segundo golpe desde un saltito.
                input.Move = Vector2.left; input.Jump = true;
                yield return DocStage.Seconds(0.07f);
                input.Jump = false;
                yield return DocStage.Seconds(0.33f);
                input.Move = Vector2.down; input.TapDash();
                yield return DocStage.Seconds(0.6f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerWallSlide_Clip()
        {
            var s = new DocStage("PlayerWallSlide", new Vector2(0f, 4f), 10f);
            try
            {
                s.World.Ground();
                s.World.Block("Wall", new Vector2(2.5f, 4.5f), new Vector2(1f, 11f));
                var p = s.World.Player(new Vector2(-1.5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerWallSlide));
                s.ShowInput(input);
                s.Caption(() => $"Vy {p.Controller.Velocity.y:0.0}");
                yield return DocStage.Settle(p);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.3f);
                input.Jump = true; input.Move = Vector2.right;   // llega a la pared subiendo
                yield return DocStage.Seconds(1.3f);              // pasa el punto más alto: no hay recorte
                input.Jump = false;
                yield return DocStage.Seconds(1.2f);              // se desliza empujando hacia la pared
                input.Move = Vector2.zero;                        // suelta: cae a velocidad normal
                yield return DocStage.Seconds(1.1f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerWallCling_Clip()
        {
            var s = new DocStage("PlayerWallCling", new Vector2(0f, 4f), 10f);
            try
            {
                s.World.Ground();
                s.World.Block("Wall", new Vector2(2.5f, 4.5f), new Vector2(1f, 11f));
                var p = s.World.Player(new Vector2(-1.5f, 0.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerWallCling));
                s.ShowInput(input);
                s.Caption(() => $"Vy {p.Controller.Velocity.y:0.0}");
                yield return DocStage.Settle(p);
                s.StartRecording(4.8f);
                yield return DocStage.Seconds(0.3f);
                input.Jump = true; input.Move = Vector2.right;   // se agarra al tocar la pared (~0.78 s)
                yield return DocStage.Seconds(0.9f);
                input.Jump = false;
                yield return DocStage.Seconds(0.9f);
                input.Move = Vector2.zero;                        // suelta un momento: cae
                yield return DocStage.Seconds(0.35f);
                input.Move = Vector2.right;                       // se vuelve a agarrar
                yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.zero;                        // suelta: cae hasta el suelo
                yield return DocStage.Seconds(1.2f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerSlopeWalk_Clip()
        {
            var s = new DocStage("PlayerSlopeWalk", new Vector2(0.8f, 1.8f), 8f);
            try
            {
                s.World.Ground();
                // Rampa de 25°: la superficie sale del suelo en x = -1 y termina en (5.85, 2.69).
                var ramp = s.World.Block("Ramp", new Vector2(2.436f, 0.551f), new Vector2(8f, 1f));
                ramp.transform.rotation = Quaternion.Euler(0f, 0f, 25f);
                var p = s.World.Player(new Vector2(-4.5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerSlopeWalk));
                var slope = p.GetComponent<PlayerSlopeWalk>();
                s.ShowInput(input);
                s.Caption(() => $"Slope {slope.SlopeAngle:0}°");
                yield return DocStage.Settle(p);
                s.StartRecording(5.4f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.9f);   // sube la rampa
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.3f);    // quieto: no resbala
                input.Move = Vector2.left; yield return DocStage.Seconds(1.4f);    // baja
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerDropThrough_Clip()
        {
            var s = new DocStage("PlayerDropThrough", new Vector2(0f, 2.3f), 7.5f);
            try
            {
                s.World.Ground();
                s.World.Block("OneWay", new Vector2(0f, 2.5f), new Vector2(4f, 0.3f)).AddComponent<OneWayPlatform>();
                var p = s.World.Player(new Vector2(0f, 4.5f), out var input,
                    typeof(PlayerWalkRun), typeof(PlayerJump), typeof(PlayerDropThrough));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(3.4f);
                yield return DocStage.Seconds(0.3f);

                // Abajo + salto: atraviesa la plataforma.
                input.Move = Vector2.down; input.Jump = true;
                yield return DocStage.Seconds(0.07f);
                input.Jump = false; input.Move = Vector2.zero;
                yield return DocStage.Seconds(1.1f);

                // Salto desde abajo: la atraviesa subiendo y aterriza encima.
                input.Jump = true;
                yield return DocStage.Seconds(0.25f);
                input.Jump = false;
                yield return DocStage.Seconds(1.2f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerTopDownMovement_Clip()
        {
            var s = new DocStage("PlayerTopDownMovement", Vector2.zero, 9f);
            try
            {
                TopDownRoom(s);
                TopDownPlayer(s, new Vector2(-4f, -1.5f), out var input, typeof(PlayerTopDownMovement));
                s.ShowInput(input);
                yield return DocStage.Seconds(0.3f);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);
                input.Move = new Vector2(1f, 1f); yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.up; input.Run = true; yield return DocStage.Seconds(0.6f);   // corre hasta la pared de arriba
                input.Move = Vector2.left; yield return DocStage.Seconds(0.8f);
                input.Run = false; input.Move = Vector2.down; yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerRollDodge_Clip()
        {
            var s = new DocStage("PlayerRollDodge", new Vector2(0f, 2f), 7f);
            try
            {
                s.World.Ground();
                s.World.Block("Spikes", new Vector2(0f, -0.25f), new Vector2(1.5f, 0.5f), true).AddComponent<HazardZone>();

                var go = s.PlayerShell(new Vector2(-4f, 0.5f), out var input);
                go.AddComponent<PlayerWalkRun>();
                var roll = go.AddComponent<PlayerRollDodge>();
                var health = go.AddComponent<CharacterHealth>();
                s.PaintNow(go);                    // antes de DamageFlash, que cachea los SpriteRenderers en Awake
                go.AddComponent<DamageFlash>();
                var p = s.Activate(go);
                s.Trail(go, true);
                s.ShowInput(input);
                s.Caption(() => $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}" + (roll.IsRolling ? "  Roll" : ""));
                yield return DocStage.Settle(p);
                s.StartRecording(3.8f);
                yield return DocStage.Seconds(0.3f);

                input.Move = Vector2.right; yield return DocStage.Seconds(1.75f);   // camina sobre los picos: recibe daño
                input.Move = Vector2.left; yield return DocStage.Seconds(0.4f);     // se da vuelta junto a los picos
                input.TapAction(CharacterAction.Roll);                              // rueda a través de ellos sin daño
                yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator PlayerTeleportBlink_Clip()
        {
            var s = new DocStage("PlayerTeleportBlink", new Vector2(0f, 1.8f), 7f);
            try
            {
                s.World.Ground();
                s.World.Block("Wall", new Vector2(4f, 1f), new Vector2(1f, 3f));   // cara izquierda en x = 3.5
                var p = s.World.Player(new Vector2(-5f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerTeleportBlink));
                s.ShowInput(input);
                yield return DocStage.Settle(p);
                s.StartRecording(3.7f);
                yield return DocStage.Seconds(0.3f);

                input.Move = Vector2.right; yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.zero;
                Ghost(s, p.transform); input.TapAction(CharacterAction.Blink);   // 4 unidades en terreno libre
                yield return DocStage.Seconds(1.1f);                              // cooldown (1 s)
                Ghost(s, p.transform); input.TapAction(CharacterAction.Blink);   // se detiene antes de la pared
                yield return DocStage.Seconds(0.55f);
                input.Move = Vector2.left; yield return DocStage.Seconds(0.1f);  // se da vuelta
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.45f);
                Ghost(s, p.transform); input.TapAction(CharacterAction.Blink);   // de vuelta a la izquierda
                yield return DocStage.Seconds(0.7f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
