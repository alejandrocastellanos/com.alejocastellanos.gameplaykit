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
    /// <summary>Clips de la guía top-down: disparo apuntado en 360°, cambio de arma y enemigos en 2 ejes.</summary>
    public class TopDownCaptures
    {
        private static ProjectileBehaviour BulletTemplate(DocStage s, string name)
        {
            var go = s.World.Track(new GameObject(name));
            go.SetActive(false);
            go.transform.position = new Vector2(500f, 500f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            return go.AddComponent<ProjectileBehaviour>();
        }

        private static GameObject Room(DocStage s, float halfW, float halfH)
        {
            s.World.Block("WallTop", new Vector2(0f, halfH + 0.5f), new Vector2(halfW * 2f + 2f, 1f));
            s.World.Block("WallBottom", new Vector2(0f, -halfH - 0.5f), new Vector2(halfW * 2f + 2f, 1f));
            s.World.Block("WallLeft", new Vector2(-halfW - 0.5f, 0f), new Vector2(1f, halfH * 2f));
            return s.World.Block("WallRight", new Vector2(halfW + 0.5f, 0f), new Vector2(1f, halfH * 2f));
        }

        private static GameObject Chaser(DocStage s, Vector2 position)
        {
            var go = s.World.Track(new GameObject("Enemy Chaser"));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            var chase = go.AddComponent<EnemyChase>();
            TestWorld.Set(chase, "moveVertically", true);
            TestWorld.Set(chase, "detectionRange", 12f);
            TestWorld.Set(chase, "speed", 2.2f);
            TestWorld.Set(chase, "stoppingDistance", 0.2f);
            go.AddComponent<EnemyMeleeOnContact>();
            var hp = go.AddComponent<DamageableObject>();
            TestWorld.Set(hp, "maxHealth", 30f);
            s.PaintNow(go);
            s.Tint(go, DocPalette.Enemy);
            go.AddComponent<DamageFlash>();
            go.SetActive(true);
            return go;
        }

        private static GameObject Turret(DocStage s, Vector2 position, ProjectileBehaviour bullet)
        {
            var go = s.World.Track(new GameObject("Enemy Turret"));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            var weapon = go.AddComponent<WeaponProjectile>();
            TestWorld.Set(weapon, "projectilePrefab", bullet);
            TestWorld.Set(weapon, "projectileSpeed", 6f);
            var shooter = go.AddComponent<EnemyShootOnSight>();
            TestWorld.Set(shooter, "sightAngle", 360f);
            TestWorld.Set(shooter, "sightRange", 12f);
            TestWorld.Set(shooter, "fireInterval", 1.1f);
            var hp = go.AddComponent<DamageableObject>();
            TestWorld.Set(hp, "maxHealth", 40f);
            s.PaintNow(go);
            s.Tint(go, DocPalette.Hex("C2185B"));
            go.AddComponent<DamageFlash>();
            go.SetActive(true);
            return go;
        }

        /// <summary>Jugador top-down con pistola (apuntado manual) y espada en un WeaponInventorySlot, como en la demo.</summary>
        private static GameObject ArmedPlayer(DocStage s, Vector2 position, ProjectileBehaviour bullet, out ScriptedCharacterInput input, out CharacterAimAndOrient aim)
        {
            var go = s.World.Track(new GameObject("Player"));
            go.SetActive(false);
            go.tag = "Player"; // los enemigos sin objetivo buscan este tag
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            go.AddComponent<CharacterController2D>();
            input = go.AddComponent<ScriptedCharacterInput>();
            go.AddComponent<PlayerTopDownMovement>();
            go.AddComponent<PlayerAttack>();
            go.AddComponent<CharacterHealth>();
            go.AddComponent<DamageFlash>();

            var gun = new GameObject("Gun");
            gun.transform.SetParent(go.transform, false);
            var weapon = gun.AddComponent<WeaponProjectile>();
            TestWorld.Set(weapon, "projectilePrefab", bullet);
            TestWorld.Set(weapon, "projectileSpeed", 14f);
            TestWorld.Set(weapon, "cooldown", 0.2f);
            s.Marker((Vector2)go.transform.position + new Vector2(0.6f, 0f), DocPalette.Hex("E3F2FD"), 0.28f, gun.transform);

            var sword = new GameObject("Sword");
            sword.transform.SetParent(go.transform, false);
            TestWorld.Set(sword.AddComponent<WeaponMelee>(), "damage", 20f);
            var blade = s.Marker((Vector2)go.transform.position + new Vector2(0.75f, 0.1f), DocPalette.Hex("FFF59D"), 0.32f, sword.transform);
            blade.transform.localScale = new Vector3(0.6f, 0.14f, 1f);
            blade.GetComponent<SpriteRenderer>().sprite = DocSprites.Block;

            aim = go.AddComponent<CharacterAimAndOrient>();
            TestWorld.Set(aim, "aimMode", CharacterAimAndOrient.AimMode.Stick); // apuntado scripteado
            TestWorld.Set(aim, "partToRotate", gun.transform);
            var inventory = go.AddComponent<WeaponInventorySlot>();
            TestWorld.Set(inventory, "weapons", new[] { gun, sword });
            s.Activate(go);
            return go;
        }

        private static Transform Nearest(Vector2 from, List<GameObject> targets)
        {
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var t in targets)
            {
                if (t == null || !t.activeInHierarchy) continue;
                float d = Vector2.Distance(from, t.transform.position);
                if (d < bestDist) { bestDist = d; best = t.transform; }
            }
            return best;
        }

        [UnityTest, Explicit, Category("DocCapture")]
        public IEnumerator TopDownShooter_Clip()
        {
            var s = new DocStage("TopDownShooter", Vector2.zero, 10f);
            try
            {
                Room(s, 8f, 4.5f);
                var playerBullet = BulletTemplate(s, "PlayerBullet");
                var turretBullet = BulletTemplate(s, "TurretBullet");
                var player = ArmedPlayer(s, new Vector2(-5f, 0f), playerBullet, out var input, out var aim);
                s.ShowInput(input);
                var enemies = new List<GameObject>
                {
                    Chaser(s, new Vector2(5f, 3f)),
                    Chaser(s, new Vector2(4f, -3f)),
                    Chaser(s, new Vector2(-2f, 3.5f)),
                    Turret(s, new Vector2(6.8f, 0f), turretBullet),
                };
                var inventory = player.GetComponent<WeaponInventorySlot>();
                s.Caption(() =>
                {
                    int alive = 0;
                    foreach (var e in enemies) if (e != null && e.activeInHierarchy) alive++;
                    return (inventory.EquippedIndex == 0 ? "Gun" : "Sword") + "   Enemies " + alive;
                });
                yield return DocStage.Seconds(0.3f);

                s.StartRecording(6f);
                float t = 0f;
                float nextShot = 0.25f;
                bool switched = false;
                while (t < 5.8f)
                {
                    // Se mueve en un arco mientras dispara al enemigo más cercano.
                    input.Move = new Vector2(Mathf.Cos(t * 1.3f), Mathf.Sin(t * 1.7f)) * 0.7f;
                    var target = Nearest(player.transform.position, enemies);
                    if (target != null) aim.SetAimDirection((Vector2)target.position - (Vector2)player.transform.position);
                    if (!switched && t >= 4.6f) { input.TapAction(CharacterAction.Special); switched = true; }
                    if (t >= nextShot) { input.TapAction(CharacterAction.Attack); nextShot = t + 0.3f; }
                    t += Time.deltaTime;
                    yield return null;
                }
                input.Move = Vector2.zero;
                yield return s.WaitRecording();
            }
            finally { s.Dispose(); }
        }
    }
}
