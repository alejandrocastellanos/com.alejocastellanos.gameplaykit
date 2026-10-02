using System.Collections;
using System.Collections.Generic;
using GameplayKit.AI;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Health;
using GameplayKit.Movement;
using GameplayKit.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.DocCapture
{
    /// <summary>Calls an action every LateUpdate (updates range rings, sight lines... while a clip records).</summary>
    public class AICapturesTicker : MonoBehaviour
    {
        public System.Action Tick;

        private void LateUpdate()
        {
            if (Tick != null) Tick();
        }
    }

    public class AICaptures
    {
        // ---------------- helpers ----------------

        private static AIState State(string name, AIActionBase action, params AITransition[] transitions)
        {
            var state = new AIState { name = name, actions = new List<AIActionBase>(), transitions = new List<AITransition>() };
            if (action != null) state.actions.Add(action);
            state.transitions.AddRange(transitions);
            return state;
        }

        private static AITransition When(AIDecisionBase decision, string onTrue = null, string onFalse = null) =>
            new AITransition { decision = decision, trueTargetState = onTrue, falseTargetState = onFalse };

        private static string StateOf(AIBrain brain) =>
            brain != null && brain.CurrentState != null ? brain.CurrentState.name : "";

        /// <summary>Inactive enemy with a 0.8x1 box (resting on the default ground when y = 0) and an optional dynamic body.</summary>
        private static GameObject EnemyShell(DocStage s, string name, Vector2 position, bool body = true)
        {
            var go = s.World.Track(new GameObject(name));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 1f);
            if (body) go.AddComponent<Rigidbody2D>().freezeRotation = true;
            return go;
        }

        private static void Face(Transform t, float direction)
        {
            Vector3 scale = t.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction < 0f ? -1f : 1f);
            t.localScale = scale;
        }

        /// <summary>Circle (range) drawn as a closed line that follows the parent.</summary>
        private static LineRenderer Ring(DocStage s, Transform parent, float radius, Color color, float width = 0.05f)
        {
            const int n = 56;
            var points = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                points[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            var lr = s.Line(color, width, points);
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.transform.SetParent(parent, false);
            return lr;
        }

        /// <summary>Vision cone (two edges) that follows the parent and flips with its scale.</summary>
        private static LineRenderer Cone(DocStage s, Transform parent, float range, float angle, Color color)
        {
            float half = angle * 0.5f * Mathf.Deg2Rad;
            var upper = new Vector2(Mathf.Cos(half), Mathf.Sin(half)) * range;
            var lower = new Vector2(Mathf.Cos(half), -Mathf.Sin(half)) * range;
            var lr = s.Line(color, 0.04f, upper, Vector2.zero, lower);
            lr.useWorldSpace = false;
            lr.transform.SetParent(parent, false);
            return lr;
        }

        private static void ShotLine(DocStage s, Vector2 from, Vector2 to)
        {
            var lr = s.Line(DocPalette.Projectile, 0.08f, from, to);
            lr.sortingOrder = 18;
            UnityEngine.Object.Destroy(lr.gameObject, 0.12f);
        }

        /// <summary>Hitscan shots are invisible: draw a short-lived line for each one.</summary>
        private static void ShowShots(DocStage s, WeaponHitscan gun)
        {
            gun.OnShotFired += (from, to) => ShotLine(s, from, to);
        }

        private static void Every(DocStage s, System.Action tick)
        {
            var go = s.World.Track(new GameObject("~tick"));
            go.AddComponent<DocNoPaint>();
            go.AddComponent<AICapturesTicker>().Tick = tick;
        }

        private static IEnumerator Until(System.Func<bool> done, float timeout)
        {
            for (float t = 0f; t < timeout && !done(); t += Time.deltaTime) yield return null;
        }

        private static IEnumerator During(float seconds, System.Action<float> tick)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                tick(t / seconds);
                yield return null;
            }
            tick(1f);
        }

        private static readonly Color RangeColor = DocPalette.Hex("FFFFFF", 0.22f);
        private static readonly Color RangeActive = DocPalette.Hex("EF5350", 0.6f);
        private static readonly Color PointColor = DocPalette.Hex("FFD54F", 0.9f);

        // ---------------- AIBrain ----------------

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIBrain_Clip()
        {
            var s = new DocStage("AIBrain", new Vector2(0f, 2.5f), 10f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(6f, 0.5f), out var input, typeof(PlayerWalkRun));
                var hp = p.gameObject.AddComponent<CharacterHealth>();
                Face(p.transform, -1f);
                s.ShowInput(input);

                var a = s.World.Point(new Vector2(-6f, 0f));
                var b = s.World.Point(new Vector2(-2.5f, 0f));
                s.Marker(new Vector2(-6f, -0.2f), PointColor, 0.25f);
                s.Marker(new Vector2(-2.5f, -0.2f), PointColor, 0.25f);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-4f, 0f));
                var patrol = enemy.AddComponent<AIActionPatrolPoints>();
                TestWorld.Set(patrol, "waypoints", new[] { b.transform, a.transform });
                var chase = enemy.AddComponent<AIActionMoveToTarget>();
                TestWorld.Set(chase, "stoppingDistance", 2.5f);
                var gun = enemy.AddComponent<WeaponHitscan>();
                TestWorld.Set(gun, "damage", 10f);
                var shoot = enemy.AddComponent<AIActionShoot>();
                TestWorld.Set(shoot, "fireInterval", 0.6f);
                var far = enemy.AddComponent<AIDecisionTargetInRange>();
                TestWorld.Set(far, "range", 4.5f);
                var near = enemy.AddComponent<AIDecisionTargetInRange>();
                TestWorld.Set(near, "range", 2.6f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Patrol", patrol, When(far, onTrue: "Chase")),
                    State("Chase", chase, When(far, onFalse: "Patrol"), When(near, onTrue: "Attack")),
                    State("Attack", shoot, When(near, onFalse: "Chase")),
                });
                ShowShots(s, gun);
                Ring(s, enemy.transform, 4.5f, RangeColor);
                Ring(s, enemy.transform, 2.6f, DocPalette.Hex("EF5350", 0.45f));
                s.Caption(() => $"{StateOf(brain)}   HP {hp.CurrentHealth:0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(6.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left;
                yield return Until(() => StateOf(brain) == "Chase", 3f);
                input.Move = Vector2.zero;
                yield return Until(() => StateOf(brain) == "Attack", 2f);
                yield return DocStage.Seconds(1.4f);
                input.Run = true; input.Move = Vector2.right;
                yield return Until(() => StateOf(brain) == "Patrol" || p.transform.position.x > 7f, 2f);
                input.Run = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------- Actions ----------------

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIActionPatrolPoints_Clip()
        {
            var s = new DocStage("AIActionPatrolPoints", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var pa = new Vector2(-5f, 0f);
                var pb = new Vector2(-1f, 0.9f);
                var pc = new Vector2(4f, 0.3f);
                var a = s.World.Point(pa);
                var b = s.World.Point(pb);
                var c = s.World.Point(pc);
                s.Line(DocPalette.Hex("FFD54F", 0.25f), 0.04f, pa, pb, pc);
                s.Marker(pa, PointColor, 0.3f);
                s.Marker(pb, PointColor, 0.3f);
                s.Marker(pc, PointColor, 0.3f);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-5f, 0f));
                var patrol = enemy.AddComponent<AIActionPatrolPoints>();
                TestWorld.Set(patrol, "waypoints", new[] { a.transform, b.transform, c.transform, b.transform });
                TestWorld.Set(patrol, "speed", 3f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "states", new List<AIState> { State("Patrol", patrol) });
                s.Caption(() => $"Waypoint {patrol.CurrentIndex}");

                enemy.SetActive(true);
                yield return DocStage.Frames(2);
                s.StartRecording(6.1f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIActionMoveToTarget_Clip()
        {
            var s = new DocStage("AIActionMoveToTarget", new Vector2(1f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerWalkRun));
                Face(p.transform, -1f);
                s.ShowInput(input);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-5.5f, 0f));
                var move = enemy.AddComponent<AIActionMoveToTarget>();
                TestWorld.Set(move, "stoppingDistance", 1.2f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState> { State("Chase", move) });
                s.Caption(() => $"Distance {Vector2.Distance(enemy.transform.position, p.transform.position):0.0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(4.8f);
                yield return DocStage.Seconds(1.9f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIActionFlee_Clip()
        {
            var s = new DocStage("AIActionFlee", new Vector2(1f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-5.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-1.5f, 0f));
                var wait = enemy.AddComponent<AIActionWait>();
                var flee = enemy.AddComponent<AIActionFlee>();
                var inRange = enemy.AddComponent<AIDecisionTargetInRange>();
                TestWorld.Set(inRange, "range", 3.5f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Idle", wait, When(inRange, onTrue: "Flee")),
                    State("Flee", flee, When(inRange, onFalse: "Idle")),
                });
                Ring(s, enemy.transform, 3.5f, RangeColor);
                s.Caption(() => StateOf(brain));

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.3f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIActionShoot_Clip()
        {
            var s = new DocStage("AIActionShoot", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(6.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                var hp = p.gameObject.AddComponent<CharacterHealth>();
                Face(p.transform, -1f);
                s.ShowInput(input);

                var turret = EnemyShell(s, "EnemyTurret", new Vector2(-5.5f, 0f), body: false);
                var gun = turret.AddComponent<WeaponHitscan>();
                var wait = turret.AddComponent<AIActionWait>();
                var shoot = turret.AddComponent<AIActionShoot>();
                TestWorld.Set(shoot, "fireInterval", 0.6f);
                var inRange = turret.AddComponent<AIDecisionTargetInRange>();
                TestWorld.Set(inRange, "range", 6f);
                var brain = turret.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Idle", wait, When(inRange, onTrue: "Shoot")),
                    State("Shoot", shoot, When(inRange, onFalse: "Idle")),
                });
                ShowShots(s, gun);
                Ring(s, turret.transform, 6f, RangeColor);
                s.Caption(() => $"{StateOf(brain)}   HP {hp.CurrentHealth:0}");

                yield return DocStage.Settle(p);
                turret.SetActive(true);
                s.StartRecording(5.7f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left; yield return DocStage.Seconds(1.7f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(1.8f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.2f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIActionWait_Clip()
        {
            var s = new DocStage("AIActionWait", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var a = s.World.Point(new Vector2(-4.5f, 0f));
                var b = s.World.Point(new Vector2(4.5f, 0f));
                s.Marker(new Vector2(-4.5f, -0.2f), PointColor, 0.25f);
                s.Marker(new Vector2(4.5f, -0.2f), PointColor, 0.25f);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-4f, 0f));
                var patrol = enemy.AddComponent<AIActionPatrolPoints>();
                TestWorld.Set(patrol, "waypoints", new[] { b.transform, a.transform });
                TestWorld.Set(patrol, "speed", 2.5f);
                var wait = enemy.AddComponent<AIActionWait>();
                var walkTime = enemy.AddComponent<AIDecisionTimeInState>();
                TestWorld.Set(walkTime, "seconds", 1.4f);
                var waitTime = enemy.AddComponent<AIDecisionTimeInState>();
                TestWorld.Set(waitTime, "seconds", 0.9f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Walk", patrol, When(walkTime, onTrue: "Wait")),
                    State("Wait", wait, When(waitTime, onTrue: "Walk")),
                });
                s.Caption(() => $"{StateOf(brain)}  {brain.TimeInCurrentState:0.0}s");

                enemy.SetActive(true);
                yield return DocStage.Frames(2);
                s.StartRecording(7f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------- Decisions ----------------

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIDecisionTargetInRange_Clip()
        {
            var s = new DocStage("AIDecisionTargetInRange", new Vector2(1f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(5f, 0.5f), out var input, typeof(PlayerWalkRun));
                Face(p.transform, -1f);
                s.ShowInput(input);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-3f, 0f));
                var wait = enemy.AddComponent<AIActionWait>();
                var move = enemy.AddComponent<AIActionMoveToTarget>();
                TestWorld.Set(move, "speed", 2.5f);
                TestWorld.Set(move, "stoppingDistance", 1.3f);
                var inRange = enemy.AddComponent<AIDecisionTargetInRange>();
                TestWorld.Set(inRange, "range", 3.5f);
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Idle", wait, When(inRange, onTrue: "Chase")),
                    State("Chase", move, When(inRange, onFalse: "Idle")),
                });
                var ring = Ring(s, enemy.transform, 3.5f, RangeColor, 0.06f);
                Every(s, () =>
                {
                    if (enemy == null || !enemy.activeInHierarchy) return;
                    ring.startColor = ring.endColor = inRange.Decide(brain) ? RangeActive : RangeColor;
                });
                s.Caption(() => $"{StateOf(brain)}   Dist {Vector2.Distance(enemy.transform.position, p.transform.position):0.0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left;
                yield return Until(() => StateOf(brain) == "Chase", 3f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(1.0f);
                input.Run = true; input.Move = Vector2.right;
                yield return Until(() => StateOf(brain) == "Idle" || p.transform.position.x > 7.5f, 2f);
                input.Run = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIDecisionLineOfSight_Clip()
        {
            var s = new DocStage("AIDecisionLineOfSight", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var closed = new Vector2(0f, 0.75f);
                var open = new Vector2(0f, 3.6f);
                var door = s.World.Block("Door", closed, new Vector2(0.6f, 2.5f));
                var p = s.World.Player(new Vector2(4f, 0.5f), out _);
                Face(p.transform, -1f);

                var turret = EnemyShell(s, "EnemyTurret", new Vector2(-5f, 0f), body: false);
                var gun = turret.AddComponent<WeaponHitscan>();
                var wait = turret.AddComponent<AIActionWait>();
                var shoot = turret.AddComponent<AIActionShoot>();
                TestWorld.Set(shoot, "fireInterval", 0.5f);
                var los = turret.AddComponent<AIDecisionLineOfSight>();
                TestWorld.Set(los, "maxDistance", 12f);
                var brain = turret.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Idle", wait, When(los, onTrue: "Shoot")),
                    State("Shoot", shoot, When(los, onFalse: "Idle")),
                });
                ShowShots(s, gun);

                var sight = s.Line(RangeColor, 0.05f, new Vector2(-5f, 0f), (Vector2)p.transform.position);
                Every(s, () =>
                {
                    if (p == null) return;
                    sight.SetPosition(1, p.transform.position);
                    sight.startColor = sight.endColor = los.Decide(brain) ? DocPalette.Hex("66BB6A", 0.8f) : RangeColor;
                });
                s.Caption(() => StateOf(brain));

                yield return DocStage.Settle(p);
                turret.SetActive(true);
                s.StartRecording(4.4f);
                yield return DocStage.Seconds(0.3f);
                yield return During(0.4f, k => { door.transform.position = Vector2.Lerp(closed, open, k); Physics2D.SyncTransforms(); });
                yield return DocStage.Seconds(1.7f);
                yield return During(0.4f, k => { door.transform.position = Vector2.Lerp(open, closed, k); Physics2D.SyncTransforms(); });
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIDecisionHealthThreshold_Clip()
        {
            var s = new DocStage("AIDecisionHealthThreshold", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                s.World.Block("Wall", new Vector2(-7.5f, 1.5f), new Vector2(1f, 4f));
                var p = s.World.Player(new Vector2(3f, 0.5f), out var input, typeof(PlayerAttack));
                var playerGun = p.gameObject.AddComponent<WeaponHitscan>();
                Face(p.transform, -1f);
                s.ShowInput(input);
                ShowShots(s, playerGun);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-4f, 0f));
                var hp = enemy.AddComponent<CharacterHealth>();
                TestWorld.Set(hp, "maxHealth", 60f);
                var move = enemy.AddComponent<AIActionMoveToTarget>();
                TestWorld.Set(move, "stoppingDistance", 3f);
                var flee = enemy.AddComponent<AIActionFlee>();
                var low = enemy.AddComponent<AIDecisionHealthThreshold>();
                var brain = enemy.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Fight", move, When(low, onTrue: "Flee")),
                    State("Flee", flee),
                });
                hp.OnDamaged += amount => s.Flash(enemy.transform.position, new Vector2(1f, 1.2f), DocPalette.Hex("FFFFFF", 0.6f), 0.1f);
                s.Caption(() => $"{StateOf(brain)}   HP {hp.CurrentHealth:0}/{hp.MaxHealth:0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.9f);
                input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.8f);
                input.TapAction(CharacterAction.Attack);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator AIDecisionTimeInState_Clip()
        {
            var s = new DocStage("AIDecisionTimeInState", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(4f, 0.5f), out _);
                Face(p.transform, -1f);

                var turret = EnemyShell(s, "EnemyTurret", new Vector2(-5.5f, 0f), body: false);
                var gun = turret.AddComponent<WeaponHitscan>();
                var shoot = turret.AddComponent<AIActionShoot>();
                TestWorld.Set(shoot, "fireInterval", 0.25f);
                var wait = turret.AddComponent<AIActionWait>();
                var burstTime = turret.AddComponent<AIDecisionTimeInState>();
                TestWorld.Set(burstTime, "seconds", 1.2f);
                var reloadTime = turret.AddComponent<AIDecisionTimeInState>();
                TestWorld.Set(reloadTime, "seconds", 1.5f);
                var brain = turret.AddComponent<AIBrain>();
                TestWorld.Set(brain, "target", p.transform);
                TestWorld.Set(brain, "initialState", "Reload");
                TestWorld.Set(brain, "states", new List<AIState>
                {
                    State("Burst", shoot, When(burstTime, onTrue: "Reload")),
                    State("Reload", wait, When(reloadTime, onTrue: "Burst")),
                });
                ShowShots(s, gun);
                s.Caption(() => $"{StateOf(brain)}  {brain.TimeInCurrentState:0.0}s");

                yield return DocStage.Settle(p);
                turret.SetActive(true);
                yield return DocStage.Frames(2);
                s.StartRecording(5.6f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        // ---------------- Stand-alone enemy behaviours ----------------

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyChase_Clip()
        {
            var s = new DocStage("EnemyChase", new Vector2(1f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(5f, 0.5f), out var input, typeof(PlayerWalkRun));
                Face(p.transform, -1f);
                s.ShowInput(input);

                var enemy = EnemyShell(s, "Enemy", new Vector2(-4f, 0f));
                var chase = enemy.AddComponent<EnemyChase>();
                TestWorld.Set(chase, "target", p.transform);
                TestWorld.Set(chase, "detectionRange", 4.5f);
                TestWorld.Set(chase, "stoppingDistance", 1.2f);
                Ring(s, enemy.transform, 4.5f, RangeColor);
                s.Caption(() => chase.IsChasing ? "Chase" : "Idle");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(4.6f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.left;
                yield return Until(() => chase.IsChasing, 3f);
                input.Move = Vector2.zero;
                yield return DocStage.Seconds(1.2f);
                input.Run = true; input.Move = Vector2.right;
                yield return DocStage.Seconds(0.9f);
                input.Run = false; input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyFlee_Clip()
        {
            var s = new DocStage("EnemyFlee", new Vector2(1f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-5.5f, 0.5f), out var input, typeof(PlayerWalkRun));
                s.ShowInput(input);

                var critter = EnemyShell(s, "Enemy", new Vector2(-1.5f, 0f));
                Face(critter.transform, -1f); // starts looking at the player
                var flee = critter.AddComponent<EnemyFlee>();
                TestWorld.Set(flee, "target", p.transform);
                TestWorld.Set(flee, "fleeRange", 3.5f);
                Ring(s, critter.transform, 3.5f, RangeColor);
                s.Caption(() => flee.IsFleeing ? "Flee" : "Idle");

                yield return DocStage.Settle(p);
                critter.SetActive(true);
                s.StartRecording(4.5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.3f);
                input.Move = Vector2.zero; yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.8f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyMeleeOnContact_Clip()
        {
            var s = new DocStage("EnemyMeleeOnContact", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(-3f, 0.5f), out var input, typeof(PlayerWalkRun));
                p.gameObject.tag = "Player";
                var hp = p.gameObject.AddComponent<CharacterHealth>();
                hp.OnDamaged += amount => s.Flash(p.transform.position, new Vector2(1.1f, 2f), DocPalette.Hex("EF5350", 0.55f), 0.12f);
                s.ShowInput(input);

                var enemy = EnemyShell(s, "Enemy", new Vector2(1f, 0f));
                enemy.GetComponent<Rigidbody2D>().mass = 50f; // heavy: the player bumps into it instead of pushing it away
                enemy.AddComponent<EnemyMeleeOnContact>();
                Face(enemy.transform, -1f);
                s.Caption(() => $"HP {hp.CurrentHealth:0}/{hp.MaxHealth:0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(5f);
                yield return DocStage.Seconds(0.3f);
                input.Move = Vector2.right; yield return DocStage.Seconds(1.0f);
                input.Move = Vector2.left;  yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.left;  yield return DocStage.Seconds(0.5f);
                input.Move = Vector2.right; yield return DocStage.Seconds(0.7f);
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyPatrol_Clip()
        {
            var s = new DocStage("EnemyPatrol", new Vector2(0f, 1.5f), 8f);
            try
            {
                // Platform from x = -4 to x = 3 with a wall on its left end and a drop on its right end.
                s.World.Block("Ledge", new Vector2(-0.5f, -1f), new Vector2(7f, 1f));
                s.World.Block("Wall", new Vector2(-3.5f, 0.5f), new Vector2(1f, 2f));
                var enemy = EnemyShell(s, "Enemy", new Vector2(0f, 0f));
                enemy.AddComponent<EnemyPatrol>();

                enemy.SetActive(true);
                yield return DocStage.Frames(1);
                s.StartRecording(5.6f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyPatrolWithinBounds_Clip()
        {
            var s = new DocStage("EnemyPatrolWithinBounds", new Vector2(0f, 2f), 8f);
            try
            {
                s.World.Ground();
                var left = s.World.Point(new Vector2(-3f, 0f));
                var right = s.World.Point(new Vector2(3f, 0f));
                foreach (float x in new[] { -3f, 3f })
                {
                    s.Line(DocPalette.Hex("FFD54F", 0.45f), 0.04f, new Vector2(x, -0.5f), new Vector2(x, 2f));
                    s.Marker(new Vector2(x, 2f), PointColor, 0.25f);
                }

                var enemy = EnemyShell(s, "Enemy", new Vector2(0f, 0f));
                var patrol = enemy.AddComponent<EnemyPatrolWithinBounds>();
                TestWorld.Set(patrol, "leftBound", left.transform);
                TestWorld.Set(patrol, "rightBound", right.transform);

                enemy.SetActive(true);
                yield return DocStage.Frames(1);
                s.StartRecording(6f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemyShootOnSight_Clip()
        {
            var s = new DocStage("EnemyShootOnSight", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(4f, 0.5f), out _);
                var hp = p.gameObject.AddComponent<CharacterHealth>();
                Face(p.transform, -1f);

                var left = s.World.Point(new Vector2(-5.5f, 0f));
                var right = s.World.Point(new Vector2(-2.5f, 0f));
                var enemy = EnemyShell(s, "Enemy", new Vector2(-4f, 0f));
                var gun = enemy.AddComponent<WeaponHitscan>();
                var shooter = enemy.AddComponent<EnemyShootOnSight>();
                TestWorld.Set(shooter, "target", p.transform);
                var patrol = enemy.AddComponent<EnemyPatrolWithinBounds>();
                TestWorld.Set(patrol, "leftBound", left.transform);
                TestWorld.Set(patrol, "rightBound", right.transform);
                ShowShots(s, gun);
                Cone(s, enemy.transform, 10f, 60f, DocPalette.Hex("FFD54F", 0.4f));
                s.Caption(() => $"HP {hp.CurrentHealth:0}");

                yield return DocStage.Settle(p);
                enemy.SetActive(true);
                s.StartRecording(6f);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator EnemySpawner_Clip()
        {
            var s = new DocStage("EnemySpawner", new Vector2(0f, 2f), 9f);
            try
            {
                s.World.Ground();
                var p = s.World.Player(new Vector2(0f, 0.5f), out var input, typeof(PlayerAttack));
                var playerGun = p.gameObject.AddComponent<WeaponHitscan>();
                s.ShowInput(input);
                ShowShots(s, playerGun);

                // Runtime "prefab": inactive, far away. Its copies are activated in OnSpawned.
                var template = EnemyShell(s, "EnemyTemplate", new Vector2(1000f, 1000f));
                var move = template.AddComponent<AIActionMoveToTarget>();
                TestWorld.Set(move, "speed", 2.5f);
                TestWorld.Set(move, "stoppingDistance", 1.3f);
                var health = template.AddComponent<DamageableObject>();
                TestWorld.Set(health, "maxHealth", 10f);
                var templateBrain = template.AddComponent<AIBrain>();
                TestWorld.Set(templateBrain, "states", new List<AIState> { State("Chase", move) });

                var pointL = s.World.Point(new Vector2(-6.5f, 2f));
                var pointR = s.World.Point(new Vector2(6.5f, 2f));
                s.Marker(new Vector2(-6.5f, 2f), PointColor, 0.4f);
                s.Marker(new Vector2(6.5f, 2f), PointColor, 0.4f);

                var spawnerGo = s.World.Track(new GameObject("Spawner"));
                spawnerGo.SetActive(false);
                var spawner = spawnerGo.AddComponent<EnemySpawner>();
                TestWorld.Set(spawner, "prefab", template);
                TestWorld.Set(spawner, "spawnPoints", new[] { pointL.transform, pointR.transform });
                TestWorld.Set(spawner, "interval", 0.9f);
                TestWorld.Set(spawner, "maxAlive", 3);
                TestWorld.Set(spawner, "target", p.transform);
                spawner.OnSpawned += go =>
                {
                    s.World.Track(go);
                    go.SetActive(true);
                    s.Tint(go, DocPalette.Enemy);
                };
                s.Caption(() => $"Alive {spawner.AliveCount}/3   Spawned {spawner.SpawnedCount}");

                yield return DocStage.Settle(p);
                s.StartRecording(7f);
                spawnerGo.SetActive(true);
                yield return DocStage.Seconds(1.3f);
                Face(p.transform, -1f); input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.7f);
                Face(p.transform, 1f); input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.9f);
                Face(p.transform, -1f); input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.9f);
                Face(p.transform, 1f); input.TapAction(CharacterAction.Attack);
                yield return DocStage.Seconds(0.9f);
                Face(p.transform, -1f); input.TapAction(CharacterAction.Attack);
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
