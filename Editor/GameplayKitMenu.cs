using System;
using GameplayKit.AI;
using GameplayKit.CameraSystem;
using GameplayKit.Combat;
using GameplayKit.Core;
using GameplayKit.Environment;
using GameplayKit.Health;
using GameplayKit.Managers;
using GameplayKit.Movement;
using GameplayKit.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameplayKit.Editor
{
    /// <summary>
    /// Menús para armar objetos listos para jugar sin configurar nada a mano:
    /// "GameplayKit/..." en la barra de menús y "GameObject/GameplayKit/..." (clic derecho en la jerarquía).
    /// Los mismos constructores generan la escena demo.
    /// </summary>
    public static class GameplayKitMenu
    {
        private const string SpritePath = "Packages/com.alejocastellanos.gameplaykit/Runtime/Art/GameplayKitSquare.png";
        private const string DemoFolder = "Assets/GameplayKitDemo";

        public static readonly Color PlayerColor = new Color(0.25f, 0.6f, 1f);
        public static readonly Color EnemyColor = new Color(0.9f, 0.25f, 0.25f);
        public static readonly Color GroundColor = new Color(0.35f, 0.32f, 0.3f);

        // ------------------------------------------------------------------ Menús

        [MenuItem("GameplayKit/Create Player", false, 0)]
        private static void CreatePlayerMenu() => Place(BuildPlayer(SpawnPoint()), "Crear Player");

        [MenuItem("GameplayKit/Create Top-Down Player", false, 1)]
        private static void CreateTopDownPlayerMenu() => Place(BuildTopDownPlayer(SpawnPoint()), "Crear Top-Down Player");

        [MenuItem("GameplayKit/Create Enemy", false, 2)]
        private static void CreateEnemyMenu() => Place(BuildEnemy(SpawnPoint()), "Crear Enemy");

        [MenuItem("GameplayKit/Create Platform", false, 3)]
        private static void CreatePlatformMenu() => Place(BuildBlock("Platform", SpawnPoint(), new Vector2(4f, 0.5f), GroundColor), "Crear Platform");

        [MenuItem("GameplayKit/Create 2D Camera", false, 20)]
        private static void CreateCameraMenu() => Place(BuildCamera().gameObject, "Crear cámara");

        [MenuItem("GameplayKit/Create Managers", false, 21)]
        private static void CreateManagersMenu() => Place(BuildManagers(), "Crear managers");

        [MenuItem("GameplayKit/Create HUD", false, 22)]
        private static void CreateHudMenu() => Place(BuildHud(), "Crear HUD");

        [MenuItem("GameplayKit/Create Demo Scene", false, 40)]
        private static void CreateDemoSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string path = BuildDemoScene(DemoFolder);
            EditorUtility.DisplayDialog("Gameplay Kit", $"Escena demo creada en {path}.\n\nControles: A/D mover, Espacio saltar, Shift correr, Q dash, J atacar, E interactuar, Esc pausa.", "OK");
        }

        [MenuItem("GameplayKit/Create Top-Down Demo Scene", false, 41)]
        private static void CreateTopDownDemoSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string path = BuildTopDownDemoScene(DemoFolder);
            EditorUtility.DisplayDialog("Gameplay Kit", $"Escena demo top-down creada en {path}.\n\nControles: WASD o flechas mover, Shift correr, Q dash en 8 direcciones, J atacar, E interactuar, Esc pausa.", "OK");
        }

        [MenuItem("GameObject/GameplayKit/Player", false, 10)]
        private static void CreatePlayerContext(MenuCommand command) => Place(BuildPlayer(SpawnPoint()), "Crear Player", command.context as GameObject);

        [MenuItem("GameObject/GameplayKit/Enemy", false, 11)]
        private static void CreateEnemyContext(MenuCommand command) => Place(BuildEnemy(SpawnPoint()), "Crear Enemy", command.context as GameObject);

        private static void Place(GameObject go, string undoName, GameObject parent = null)
        {
            if (parent != null) GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, undoName);
            Selection.activeGameObject = go;
        }

        private static Vector2 SpawnPoint()
        {
            var view = SceneView.lastActiveSceneView;
            return view != null ? (Vector2)view.pivot : Vector2.zero;
        }

        // ------------------------------------------------------------------ Constructores

        /// <summary>Personaje de plataformas completo: moverse, saltar (doble), dash, paredes, agacharse, escaleras,
        /// nadar, interactuar, atacar cuerpo a cuerpo, vida, knockback, muerte, respawn e inventario.</summary>
        public static GameObject BuildPlayer(Vector2 position)
        {
            var go = new GameObject("Player") { tag = "Player" };
            go.transform.position = position;
            AddVisual(go, new Vector2(0.8f, 1.8f), PlayerColor, 10);
            go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CharacterController2D>();
            go.AddComponent<KeyboardInputReader>();

            go.AddComponent<CharacterGravityController>();
            go.AddComponent<PlayerWalkRun>();
            go.AddComponent<PlayerMultiJump>();
            go.AddComponent<PlayerDash>();
            go.AddComponent<PlayerWallJump>();
            go.AddComponent<PlayerWallSlide>();
            go.AddComponent<PlayerCrouch>();
            go.AddComponent<PlayerClimbLadder>();
            go.AddComponent<PlayerSwim>();
            go.AddComponent<PlayerDropThrough>();
            go.AddComponent<PlayerSlopeWalk>();
            go.AddComponent<PlayerInteract>();
            go.AddComponent<PlayerAttack>();
            go.AddComponent<WeaponMelee>();

            go.AddComponent<CharacterHealth>();
            go.AddComponent<CharacterKnockback>();
            go.AddComponent<CharacterDeath>();
            go.AddComponent<CharacterRespawn>();
            go.AddComponent<InventoryManager>();
            go.AddComponent<DamageFlash>();
            go.AddComponent<CharacterAnimatorBridge>();

            go.AddComponent<CharacterCore>(); // al final: descubre las habilidades al iniciar
            return go;
        }

        /// <summary>Personaje para juegos vistos desde arriba: movimiento en 8 direcciones sin gravedad, ataque, vida y respawn.</summary>
        public static GameObject BuildTopDownPlayer(Vector2 position)
        {
            var go = new GameObject("Player (Top-Down)") { tag = "Player" };
            go.transform.position = position;
            AddVisual(go, new Vector2(0.8f, 0.8f), PlayerColor, 10);
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<CharacterController2D>();
            go.AddComponent<KeyboardInputReader>();
            go.AddComponent<PlayerTopDownMovement>();
            go.AddComponent<PlayerDash8Directions>();
            go.AddComponent<PlayerInteract>();
            go.AddComponent<PlayerAttack>();
            go.AddComponent<WeaponMelee>();
            go.AddComponent<CharacterHealth>();
            go.AddComponent<CharacterKnockback>();
            go.AddComponent<CharacterDeath>();
            go.AddComponent<CharacterRespawn>();
            go.AddComponent<InventoryManager>();
            go.AddComponent<DamageFlash>();
            go.AddComponent<CharacterAnimatorBridge>();
            go.AddComponent<CharacterCore>();
            return go;
        }

        /// <summary>Enemigo que patrulla (se da vuelta en paredes y bordes), daña al tocar y se destruye al recibir suficiente daño.</summary>
        public static GameObject BuildEnemy(Vector2 position)
        {
            var go = new GameObject("Enemy");
            go.transform.position = position;
            AddVisual(go, new Vector2(0.9f, 0.9f), EnemyColor, 9);
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            go.AddComponent<EnemyPatrol>();
            go.AddComponent<EnemyMeleeOnContact>();
            go.AddComponent<DamageableObject>();
            go.AddComponent<DamageFlash>();
            go.AddComponent<DamagePopupSpawner>();
            return go;
        }

        public static GameObject BuildBlock(string name, Vector2 center, Vector2 size, Color color, bool trigger = false, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            AddVisual(go, size, color, order);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.isTrigger = trigger;
            return go;
        }

        public static Camera BuildCamera(Transform target = null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
            var follow = go.AddComponent<CameraFollow>();
            if (target != null) follow.SetTarget(target);
            go.AddComponent<CameraShake>();
            return cam;
        }

        public static GameObject BuildManagers()
        {
            var go = new GameObject("Managers");
            go.AddComponent<ScoreManager>();
            go.AddComponent<PauseManager>();
            return go;
        }

        /// <summary>Canvas con barra de vida, puntaje y menú de pausa (Reanudar / Reiniciar), más su EventSystem.</summary>
        public static GameObject BuildHud(CharacterHealth health = null)
        {
            var canvasGo = new GameObject("HUD", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Barra de vida (arriba a la izquierda)
            var barBg = UIImage("HealthBar", canvasGo.transform, new Color(0f, 0f, 0f, 0.6f));
            Anchor(barBg, new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(420f, 36f));
            var fill = UIImage("Fill", barBg.transform, new Color(0.3f, 0.85f, 0.4f));
            Stretch(fill, 4f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            var bar = barBg.gameObject.AddComponent<UIHealthBar>();
            SetRef(bar, "fillImage", fill);
            if (health != null) SetRef(bar, "health", health);

            // Puntaje (arriba a la derecha)
            var score = UIText("Score", canvasGo.transform, "Puntos: 0", 40, TextAnchor.MiddleRight);
            Anchor(score.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(420f, 50f));
            score.gameObject.AddComponent<UIScoreText>();

            // Menú de pausa
            var panel = UIImage("PausePanel", canvasGo.transform, new Color(0f, 0f, 0f, 0.7f));
            Stretch(panel, 0f);
            var title = UIText("Title", panel.transform, "Pausa", 80, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(600f, 120f));
            var menu = canvasGo.AddComponent<UIPauseMenu>();
            SetRef(menu, "menuRoot", panel.gameObject);
            var resume = UIButton("Resume", panel.transform, "Reanudar", new Vector2(0f, 20f));
            UnityEventTools.AddPersistentListener(resume.onClick, menu.OnResumeButtonPressed);
            var restart = UIButton("Restart", panel.transform, "Reiniciar", new Vector2(0f, -100f));
            UnityEventTools.AddPersistentListener(restart.onClick, menu.OnRestartButtonPressed);
            panel.gameObject.SetActive(false);

            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null) BuildEventSystem();
            return canvasGo;
        }

        public static GameObject BuildEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            // Con el Input System nuevo, StandaloneInputModule lanza errores: se usa su módulo si está instalado.
            var inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null) go.AddComponent(inputSystemModule);
            else go.AddComponent<StandaloneInputModule>();
            return go;
        }

        // ------------------------------------------------------------------ Escena demo

        /// <summary>Crea (y guarda) un nivel corto que recorre las mecánicas principales del kit. Devuelve la ruta de la escena.</summary>
        public static string BuildDemoScene(string folder)
        {
            EnsureFolder(folder);
            EnsureFolder(folder + "/Prefabs");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var level = new GameObject("Level").transform;
            Transform Add(GameObject g) { g.transform.SetParent(level, true); return g.transform; }

            // Suelo y tramos
            Add(BuildBlock("Ground_Start", new Vector2(5f, -1f), new Vector2(30f, 2f), GroundColor));      // x -10..20, cara superior y=0
            Add(BuildBlock("LowWall_A", new Vector2(12.5f, 0.5f), new Vector2(0.5f, 1f), GroundColor));
            Add(BuildBlock("LowWall_B", new Vector2(19.5f, 0.5f), new Vector2(0.5f, 1f), GroundColor));
            Add(BuildBlock("Ground_Mid", new Vector2(36f, -1f), new Vector2(16f, 2f), GroundColor));        // x 28..44
            Add(BuildBlock("Tower", new Vector2(39f, 2f), new Vector2(10f, 4f), GroundColor));              // x 34..44, cara superior y=4
            Add(BuildBlock("PoolFloor", new Vector2(48f, -3f), new Vector2(8f, 2f), GroundColor));          // x 44..52, cara superior y=-2
            Add(BuildBlock("Ground_End", new Vector2(62f, -1f), new Vector2(20f, 2f), GroundColor));        // x 52..72
            Add(BuildBlock("UpperFloor", new Vector2(70f, 5.75f), new Vector2(10f, 0.5f), GroundColor));    // x 65..75, y 6
            Add(BuildBlock("Wall_Left", new Vector2(-10.5f, 5f), new Vector2(1f, 14f), GroundColor));

            // Plataforma de una vía y caja rompible
            var oneWay = BuildBlock("OneWayPlatform", new Vector2(6f, 2.5f), new Vector2(3f, 0.3f), new Color(0.55f, 0.45f, 0.3f));
            oneWay.AddComponent<OneWayPlatform>();
            Add(oneWay);
            var crate = BuildBlock("BreakableCrate", new Vector2(9f, 0.4f), new Vector2(0.8f, 0.8f), new Color(0.7f, 0.5f, 0.25f));
            crate.AddComponent<BreakableObject>();
            Add(crate);

            // Enemigo entre los muros bajos
            var enemyPrefab = SavePrefab(BuildEnemy(new Vector2(16f, 0.5f)), folder + "/Prefabs/Enemy.prefab");
            Add(InstantiatePrefab(enemyPrefab, new Vector2(16f, 0.5f)));

            // Hueco con plataforma móvil y pinchos
            var mover = BuildBlock("MovingPlatform", new Vector2(21.5f, 0f), new Vector2(2.5f, 0.4f), new Color(0.4f, 0.6f, 0.7f));
            mover.AddComponent<Rigidbody2D>();
            var a = new GameObject("WaypointA"); a.transform.SetParent(mover.transform); a.transform.localPosition = Vector3.zero;
            var b = new GameObject("WaypointB"); b.transform.SetParent(mover.transform); b.transform.localPosition = new Vector3(5f, 0f, 0f);
            var moving = mover.AddComponent<MovingPlatform>();
            SetRefArray(moving, "waypoints", a.transform, b.transform);
            Add(mover);
            var spikes = BuildBlock("Spikes", new Vector2(24f, -4f), new Vector2(8f, 1f), new Color(0.85f, 0.2f, 0.6f), trigger: true);
            spikes.AddComponent<HazardZone>();
            Add(spikes);

            // Corazón para recuperar vida después de los pinchos
            var heart = BuildBlock("HealthPickup", new Vector2(28.8f, 0.8f), new Vector2(0.5f, 0.5f), new Color(1f, 0.35f, 0.5f), trigger: true, order: 5);
            heart.AddComponent<HealthPickup>();
            Add(heart);

            // Checkpoint, escalera a la torre y llave arriba
            var checkpoint = BuildBlock("Checkpoint", new Vector2(30f, 1f), new Vector2(0.4f, 2f), new Color(1f, 0.85f, 0.2f, 0.6f), trigger: true);
            checkpoint.AddComponent<Checkpoint>();
            Add(checkpoint);
            var ladder = BuildBlock("Ladder", new Vector2(33.4f, 2.5f), new Vector2(0.8f, 5f), new Color(0.6f, 0.4f, 0.2f, 0.7f), trigger: true, order: -1);
            ladder.AddComponent<LadderZone>();
            Add(ladder);
            var key = BuildBlock("Key", new Vector2(42f, 4.8f), new Vector2(0.5f, 0.5f), new Color(1f, 0.8f, 0f), trigger: true, order: 5);
            key.AddComponent<ItemPickup>();
            Add(key);

            // Piscina, cinta y palanca que sube el elevador
            var water = BuildBlock("Water", new Vector2(48f, -1f), new Vector2(8f, 2f), new Color(0.2f, 0.5f, 1f, 0.45f), trigger: true, order: 20);
            water.AddComponent<WaterZone>();
            Add(water);
            var belt = BuildBlock("ConveyorBelt", new Vector2(56f, 0.15f), new Vector2(6f, 0.3f), new Color(0.3f, 0.3f, 0.35f));
            belt.AddComponent<ConveyorBelt>();
            Add(belt);
            var elevatorGo = BuildBlock("Elevator", new Vector2(63f, 0.2f), new Vector2(3f, 0.4f), new Color(0.4f, 0.7f, 0.5f));
            elevatorGo.AddComponent<Rigidbody2D>();
            var bottom = new GameObject("Bottom"); bottom.transform.SetParent(elevatorGo.transform); bottom.transform.localPosition = Vector3.zero;
            var top = new GameObject("Top"); top.transform.SetParent(elevatorGo.transform); top.transform.localPosition = new Vector3(0f, 6f, 0f);
            var elevator = elevatorGo.AddComponent<Elevator>();
            SetRef(elevator, "bottomPoint", bottom.transform);
            SetRef(elevator, "topPoint", top.transform);
            Add(elevatorGo);
            var leverGo = BuildBlock("Lever", new Vector2(60f, 0.5f), new Vector2(0.4f, 1f), new Color(0.9f, 0.5f, 0.1f), trigger: true);
            var lever = leverGo.AddComponent<Lever>();
            AddPersistentListener(lever, "onToggled", elevator.Activate);
            Add(leverGo);

            // Puerta con llave arriba y meta
            var door = BuildBlock("Door", new Vector2(70f, 7.25f), new Vector2(1.2f, 2.5f), new Color(0.5f, 0.3f, 0.15f), trigger: true);
            var doorBlocker = BuildBlock("DoorBlocker", new Vector2(70f, 7.25f), new Vector2(0.4f, 2.5f), new Color(0.5f, 0.3f, 0.15f));
            doorBlocker.transform.SetParent(door.transform, true);
            door.GetComponentInChildren<SpriteRenderer>().enabled = false; // lo visible es la hoja (blocker), que desaparece al abrir
            var doorComp = door.AddComponent<DoorWithKey>();
            SetRef(doorComp, "blockingCollider", doorBlocker.GetComponent<Collider2D>());
            SetRef(doorComp, "visualWhenClosed", doorBlocker);
            Add(door);
            var goal = BuildBlock("Goal", new Vector2(73.5f, 7f), new Vector2(0.8f, 0.8f), new Color(0.3f, 1f, 0.5f), trigger: true, order: 5);
            var goalCoin = goal.AddComponent<Collectible>();
            SetFloatOrInt(goalCoin, "value", 100);
            Add(goal);

            // Monedas a lo largo del recorrido
            foreach (var p in new[] { new Vector2(2f, 0.8f), new Vector2(4f, 0.8f), new Vector2(6f, 3.3f), new Vector2(29f, 0.8f), new Vector2(37f, 4.8f), new Vector2(48f, -1f), new Vector2(56f, 1f) })
            {
                var coin = BuildBlock("Coin", p, new Vector2(0.4f, 0.4f), new Color(1f, 0.85f, 0.1f), trigger: true, order: 5);
                coin.AddComponent<Collectible>();
                Add(coin);
            }

            // Zona de muerte bajo todo el nivel: devuelve al último checkpoint
            var killZone = BuildBlock("KillZone", new Vector2(32f, -14f), new Vector2(120f, 4f), new Color(0f, 0f, 0f, 0f), trigger: true);
            killZone.GetComponentInChildren<SpriteRenderer>().enabled = false;
            SetFloatOrInt(killZone.AddComponent<HazardZone>(), "damage", 9999f);
            Add(killZone);

            // Jugador, cámara, managers y HUD
            var playerPrefab = SavePrefab(BuildPlayer(new Vector2(0f, 1f)), folder + "/Prefabs/Player.prefab");
            var player = InstantiatePrefab(playerPrefab, new Vector2(0f, 1f));
            var cam = BuildCamera(player.transform);
            var bounds = cam.gameObject.AddComponent<CameraBounds>();
            SetVector2(bounds, "minBounds", new Vector2(-11f, -12f));
            SetVector2(bounds, "maxBounds", new Vector2(80f, 20f));
            BuildManagers();
            BuildHud(player.GetComponent<CharacterHealth>());

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/GameplayKitDemo.unity");
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        // ------------------------------------------------------------------ Escena demo top-down

        public static readonly Color FloorColor = new Color(0.19f, 0.21f, 0.27f);
        public static readonly Color TurretColor = new Color(0.65f, 0.2f, 0.55f);

        /// <summary>Enemigo top-down que persigue al jugador en 8 direcciones (sin gravedad), daña al tocar y se destruye al recibir daño.</summary>
        public static GameObject BuildTopDownChaser(Vector2 position)
        {
            var go = new GameObject("Enemy Chaser");
            go.transform.position = position;
            AddVisual(go, new Vector2(0.8f, 0.8f), EnemyColor, 9);
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = 2f;
            var chase = go.AddComponent<EnemyChase>();
            SetBool(chase, "moveVertically", true);
            SetFloatOrInt(chase, "detectionRange", 7f);
            SetFloatOrInt(chase, "speed", 2.5f);
            SetFloatOrInt(chase, "stoppingDistance", 0.2f);
            go.AddComponent<EnemyMeleeOnContact>();
            SetFloatOrInt(go.AddComponent<DamageableObject>(), "maxHealth", 30f);
            go.AddComponent<DamageFlash>();
            go.AddComponent<DamagePopupSpawner>();
            return go;
        }

        /// <summary>Torreta fija que dispara proyectiles al jugador cuando lo ve en cualquier dirección.</summary>
        public static GameObject BuildTurret(Vector2 position, ProjectileBehaviour bulletPrefab)
        {
            var go = new GameObject("Enemy Turret");
            go.transform.position = position;
            AddVisual(go, new Vector2(1f, 1f), TurretColor, 9);
            go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f);
            var weapon = go.AddComponent<WeaponProjectile>();
            if (bulletPrefab != null) SetRef(weapon, "projectilePrefab", bulletPrefab);
            SetFloatOrInt(weapon, "projectileSpeed", 7f);
            SetFloatOrInt(weapon, "damage", 10f);
            var shooter = go.AddComponent<EnemyShootOnSight>();
            SetFloatOrInt(shooter, "sightAngle", 360f);
            SetFloatOrInt(shooter, "sightRange", 9f);
            SetFloatOrInt(shooter, "fireInterval", 1.4f);
            SetFloatOrInt(go.AddComponent<DamageableObject>(), "maxHealth", 40f);
            go.AddComponent<DamageFlash>();
            go.AddComponent<DamagePopupSpawner>();
            return go;
        }

        /// <summary>Proyectil para las torretas: trigger pequeño sin gravedad.</summary>
        public static GameObject BuildBullet()
        {
            var go = new GameObject("Bullet");
            AddVisual(go, new Vector2(0.3f, 0.3f), new Color(1f, 0.85f, 0.2f), 15);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            go.AddComponent<ProjectileBehaviour>();
            return go;
        }

        /// <summary>Crea (y guarda) un pequeño dungeon visto desde arriba que recorre las mecánicas top-down del kit.
        /// No toca otras escenas ni prefabs de la carpeta. Devuelve la ruta de la escena.</summary>
        public static string BuildTopDownDemoScene(string folder)
        {
            EnsureFolder(folder);
            EnsureFolder(folder + "/Prefabs");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var level = new GameObject("Level").transform;
            Transform Add(GameObject g) { g.transform.SetParent(level, true); return g.transform; }
            void Wall(string name, float minX, float minY, float maxX, float maxY) =>
                Add(BuildBlock(name, new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f), new Vector2(maxX - minX, maxY - minY), GroundColor));
            void Floor(string name, float minX, float minY, float maxX, float maxY)
            {
                var floor = new GameObject(name);
                floor.transform.position = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
                AddVisual(floor, new Vector2(maxX - minX, maxY - minY), FloorColor, -10);
                Add(floor);
            }
            GameObject Trigger(string name, Vector2 center, Vector2 size, Color color, int order = 5) =>
                Add(BuildBlock(name, center, size, color, trigger: true, order: order)).gameObject;

            // Sala A (inicio): x -10..10, y -7..7, salida a la derecha
            Floor("Floor_RoomA", -10f, -7f, 10f, 7f);
            Wall("RoomA_Left", -11f, -8f, -10f, 8f);
            Wall("RoomA_Top", -11f, 7f, 11f, 8f);
            Wall("RoomA_Bottom", -11f, -8f, 11f, -7f);
            Wall("RoomA_RightUpper", 10f, 1.5f, 11f, 7f);
            Wall("RoomA_RightLower", 10f, -7f, 11f, -1.5f);

            // Pasillo con pinchos: x 11..22, y -1.5..1.5
            Floor("Floor_Corridor", 10f, -1.5f, 23f, 1.5f);
            Wall("Corridor_Top", 11f, 1.5f, 22f, 2.5f);
            Wall("Corridor_Bottom", 11f, -2.5f, 22f, -1.5f);
            Trigger("Spikes", new Vector2(15.5f, 0f), new Vector2(3f, 3f), new Color(0.85f, 0.2f, 0.6f, 0.8f), order: -5).AddComponent<HazardZone>();

            // Sala B (arena): x 23..43, y -8..8, puerta con llave arriba
            Floor("Floor_RoomB", 23f, -8f, 43f, 8f);
            Wall("RoomB_LeftUpper", 22f, 1.5f, 23f, 9f);
            Wall("RoomB_LeftLower", 22f, -9f, 23f, -1.5f);
            Wall("RoomB_Right", 43f, -9f, 44f, 9f);
            Wall("RoomB_Bottom", 22f, -9f, 44f, -8f);
            Wall("RoomB_TopLeft", 22f, 8f, 31f, 9f);
            Wall("RoomB_TopRight", 35f, 8f, 44f, 9f);
            Wall("Pillar_A", 28f, 2f, 29.5f, 3.5f);
            Wall("Pillar_B", 36f, -3.5f, 37.5f, -2f);

            // Sala C (tesoro): x 27..39, y 9..19, con reja que abre una palanca
            Floor("Floor_RoomC", 27f, 8f, 39f, 19f);
            Wall("RoomC_Left", 26f, 9f, 27f, 20f);
            Wall("RoomC_Right", 39f, 9f, 40f, 20f);
            Wall("RoomC_Top", 26f, 19f, 40f, 20f);
            Wall("RoomC_InnerLeft", 27f, 15f, 32f, 16f);
            Wall("RoomC_InnerRight", 34f, 15f, 39f, 16f);

            // Puerta con llave entre B y C (como en la demo de plataformas: trigger + hoja que bloquea)
            var door = BuildBlock("Door", new Vector2(33f, 8.5f), new Vector2(4f, 2.5f), new Color(0.5f, 0.3f, 0.15f), trigger: true);
            var doorBlocker = BuildBlock("DoorBlocker", new Vector2(33f, 8.5f), new Vector2(4f, 1f), new Color(0.5f, 0.3f, 0.15f));
            doorBlocker.transform.SetParent(door.transform, true);
            door.GetComponentInChildren<SpriteRenderer>().enabled = false;
            var doorComp = door.AddComponent<DoorWithKey>();
            SetRef(doorComp, "blockingCollider", doorBlocker.GetComponent<Collider2D>());
            SetRef(doorComp, "visualWhenClosed", doorBlocker);
            Add(door);
            Trigger("Key", new Vector2(40.5f, -6f), new Vector2(0.5f, 0.5f), new Color(1f, 0.8f, 0f)).AddComponent<ItemPickup>();

            // Reja + palanca (Lever -> UnityEvent -> SetActive)
            var gate = Add(BuildBlock("Gate", new Vector2(33f, 15.5f), new Vector2(2f, 1f), new Color(0.55f, 0.55f, 0.6f))).gameObject;
            var lever = Trigger("Lever", new Vector2(29f, 11f), new Vector2(0.6f, 0.6f), new Color(0.9f, 0.5f, 0.1f)).AddComponent<Lever>();
            AddBoolPersistentListener(lever, "onTurnedOn", gate.SetActive, false);
            AddBoolPersistentListener(lever, "onTurnedOff", gate.SetActive, true);
            var goal = Trigger("Goal", new Vector2(33f, 17.5f), new Vector2(0.8f, 0.8f), new Color(0.3f, 1f, 0.5f));
            SetFloatOrInt(goal.AddComponent<Collectible>(), "value", 100);

            // Teletransportes de ida y vuelta entre la sala C y la sala A
            var tpA = Trigger("Teleporter_A", new Vector2(-8f, -5f), new Vector2(1f, 1f), new Color(0.5f, 0.3f, 1f, 0.8f), order: -5);
            var tpC = Trigger("Teleporter_C", new Vector2(37f, 11f), new Vector2(1f, 1f), new Color(0.5f, 0.3f, 1f, 0.8f), order: -5);
            var toC = new GameObject("Destination"); toC.transform.SetParent(tpA.transform); toC.transform.position = new Vector2(35.5f, 11f);
            var toA = new GameObject("Destination"); toA.transform.SetParent(tpC.transform); toA.transform.position = new Vector2(-6.5f, -5f);
            SetRef(tpA.AddComponent<Teleporter>(), "destination", toC.transform);
            SetRef(tpC.AddComponent<Teleporter>(), "destination", toA.transform);

            // Sala A: cajas rompibles, corazón, monedas; checkpoint al final del pasillo
            foreach (var p in new[] { new Vector2(2f, 4.5f), new Vector2(3f, 4.5f), new Vector2(4f, 4.5f), new Vector2(3f, 5.5f) })
                Add(BuildBlock("BreakableCrate", p, new Vector2(0.9f, 0.9f), new Color(0.7f, 0.5f, 0.25f))).gameObject.AddComponent<BreakableObject>();
            Trigger("HealthPickup", new Vector2(6f, -4f), new Vector2(0.5f, 0.5f), new Color(1f, 0.35f, 0.5f)).AddComponent<HealthPickup>();
            Trigger("Checkpoint", new Vector2(21f, 0f), new Vector2(0.6f, 3f), new Color(1f, 0.85f, 0.2f, 0.6f), order: -4).AddComponent<Checkpoint>();
            foreach (var p in new[] { new Vector2(-4f, 3f), new Vector2(-2f, 3f), new Vector2(0f, 3f), new Vector2(-3f, -3f), new Vector2(26f, 6f), new Vector2(41f, 6.5f), new Vector2(25f, -6f), new Vector2(31f, 17.5f), new Vector2(35f, 17.5f) })
                Trigger("Coin", p, new Vector2(0.4f, 0.4f), new Color(1f, 0.85f, 0.1f)).AddComponent<Collectible>();

            // Enemigos de la sala B
            var bulletPrefab = SavePrefab(BuildBullet(), folder + "/Prefabs/TopDownBullet.prefab").GetComponent<ProjectileBehaviour>();
            var chaserPrefab = SavePrefab(BuildTopDownChaser(Vector2.zero), folder + "/Prefabs/TopDownChaser.prefab");
            var turretPrefab = SavePrefab(BuildTurret(Vector2.zero, bulletPrefab), folder + "/Prefabs/TopDownTurret.prefab");
            Add(InstantiatePrefab(chaserPrefab, new Vector2(32f, 5f)));
            Add(InstantiatePrefab(chaserPrefab, new Vector2(39f, -4f)));
            Add(InstantiatePrefab(turretPrefab, new Vector2(41.5f, 6.5f)));

            var patroller = new GameObject("Enemy Patroller");
            patroller.transform.position = new Vector2(30f, -6f);
            AddVisual(patroller, new Vector2(0.9f, 0.9f), EnemyColor, 9);
            patroller.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
            var patrolRb = patroller.AddComponent<Rigidbody2D>();
            patrolRb.gravityScale = 0f;
            patrolRb.freezeRotation = true;
            var leftBound = new GameObject("LeftBound"); leftBound.transform.SetParent(patroller.transform); leftBound.transform.position = new Vector2(25f, -6f);
            var rightBound = new GameObject("RightBound"); rightBound.transform.SetParent(patroller.transform); rightBound.transform.position = new Vector2(37f, -6f);
            var patrol = patroller.AddComponent<EnemyPatrolWithinBounds>();
            SetRef(patrol, "leftBound", leftBound.transform);
            SetRef(patrol, "rightBound", rightBound.transform);
            patroller.AddComponent<EnemyMeleeOnContact>();
            patroller.AddComponent<DamageableObject>();
            patroller.AddComponent<DamageFlash>();
            patroller.AddComponent<DamagePopupSpawner>();
            Add(patroller);

            // Jugador, cámara, managers y HUD
            var playerPrefab = SavePrefab(BuildTopDownPlayer(new Vector2(-6f, 0f)), folder + "/Prefabs/TopDownPlayer.prefab");
            var player = InstantiatePrefab(playerPrefab, new Vector2(-6f, 0f));
            var cam = BuildCamera(player.transform);
            cam.orthographicSize = 7f;
            var bounds = cam.gameObject.AddComponent<CameraBounds>();
            SetVector2(bounds, "minBounds", new Vector2(-11f, -9f));
            SetVector2(bounds, "maxBounds", new Vector2(44f, 20f));
            BuildManagers();
            BuildHud(player.GetComponent<CharacterHealth>());

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/GameplayKitTopDownDemo.unity");
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        // ------------------------------------------------------------------ Utilidades

        private static Sprite Square()
        {
            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || Math.Abs(importer.spritePixelsPerUnit - 4f) > 0.01f))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 4f;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static void AddVisual(GameObject owner, Vector2 size, Color color, int order)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(owner.transform, false);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = Square();
            sr.color = color;
            sr.sortingOrder = order;
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Vector2 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            return instance;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static void SetRef(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRefArray(UnityEngine.Object target, string field, params UnityEngine.Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector2(UnityEngine.Object target, string field, Vector2 value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).vector2Value = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloatOrInt(UnityEngine.Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop.propertyType == SerializedPropertyType.Integer) prop.intValue = Mathf.RoundToInt(value);
            else prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(UnityEngine.Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddBoolPersistentListener(UnityEngine.Object target, string eventField, UnityAction<bool> call, bool argument)
        {
            var field = target.GetType().GetField(eventField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            UnityEventTools.AddBoolPersistentListener((UnityEvent)field.GetValue(target), call, argument);
            EditorUtility.SetDirty(target);
        }

        private static void AddPersistentListener(UnityEngine.Object target, string eventField, UnityAction call)
        {
            var field = target.GetType().GetField(eventField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            UnityEventTools.AddPersistentListener((UnityEvent)field.GetValue(target), call);
            EditorUtility.SetDirty(target);
        }

        private static Image UIImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = Square();
            img.color = color;
            return img;
        }

        private static Text UIText(string name, Transform parent, string content, int size, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static Button UIButton(string name, Transform parent, string label, Vector2 position)
        {
            var img = UIImage(name, parent, new Color(1f, 1f, 1f, 0.9f));
            Anchor(img.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(360f, 90f));
            var button = img.gameObject.AddComponent<Button>();
            var text = UIText("Label", img.transform, label, 40, TextAnchor.MiddleCenter);
            text.color = new Color(0.1f, 0.1f, 0.15f);
            Stretch(text, 0f);
            return button;
        }

        private static void Anchor(Graphic graphic, Vector2 anchor, Vector2 position, Vector2 size) => Anchor(graphic.rectTransform, anchor, position, size);

        private static void Anchor(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        private static void Stretch(Graphic graphic, float padding)
        {
            var rt = graphic.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }
    }
}
