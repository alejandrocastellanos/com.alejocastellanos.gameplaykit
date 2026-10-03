using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameplayKit.Core;
using GameplayKit.Tests;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayKit.DocCapture
{
    /// <summary>
    /// Escenario de captura para la documentación: un TestWorld + cámara que graba cuadros a un RenderTexture.
    /// Todo Collider2D de la escena se pinta solo (DocPainter) con un color según su rol, así los escenarios
    /// solo construyen física y scriptean input igual que los tests.
    /// </summary>
    public class DocStage : IDisposable
    {
        public const int Fps = 30;
        public const int Width = 960;
        public const int Height = 540;

        public readonly TestWorld World = new TestWorld();
        public readonly string Id;
        public Camera Camera { get; private set; }

        private readonly GameObject _rig;
        private readonly DocRecorder _recorder;
        private readonly DocOverlay _overlay;
        private readonly RenderTexture _rt;

        public static string OutputRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "../DocCaptures/frames"));

        public DocStage(string id, Vector2 center, float viewHeight = 9f)
        {
            Id = id;
            Application.runInBackground = true;
            Time.captureFramerate = Fps;

            _rig = World.Track(new GameObject("DocCamera"));
            Camera = _rig.AddComponent<Camera>();
            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = DocPalette.Background;
            Camera.nearClipPlane = 0.1f;
            Camera.farClipPlane = 100f;
            _rt = new RenderTexture(Width, Height, 24) { antiAliasing = 4 };
            Camera.targetTexture = _rt;
            Frame(center, viewHeight);

            _rig.AddComponent<DocPainter>();
            _recorder = _rig.AddComponent<DocRecorder>();
            _overlay = _rig.AddComponent<DocOverlay>();
            _overlay.Build(Camera);
        }

        /// <summary>Centra la cámara mostrando viewHeight unidades de alto (16:9).</summary>
        public void Frame(Vector2 center, float viewHeight)
        {
            Camera.transform.position = new Vector3(center.x, center.y, -10f);
            Camera.orthographicSize = viewHeight * 0.5f;
            var follow = _rig.GetComponent<DocFollow>();
            if (follow != null) follow.enabled = false;
        }

        /// <summary>La cámara sigue suavemente al objetivo (solo X si lockY).</summary>
        public void Follow(Transform target, Vector2 offset = default, bool lockY = false)
        {
            var follow = _rig.GetComponent<DocFollow>() ?? _rig.AddComponent<DocFollow>();
            follow.enabled = true;
            follow.Target = target;
            follow.Offset = offset;
            follow.LockY = lockY;
            follow.Snap();
        }

        /// <summary>Muestra las teclas que el input scripteado mantiene/pulsa (abajo a la izquierda).</summary>
        public void ShowInput(ScriptedCharacterInput input) => _overlay.Input = input;

        /// <summary>Texto dinámico arriba a la derecha (vida, vidas, puntaje, estado...).</summary>
        public void Caption(Func<string> text) => _overlay.Caption = text;

        /// <summary>Texto fijo arriba a la izquierda (contexto breve del clip; opcional).</summary>
        public void Title(string text) => _overlay.Title = text;

        /// <summary>Fuerza el color de un objeto (y de los que se pinten bajo él).</summary>
        public void Tint(GameObject go, Color color) => DocPainter.Paint(go, color);

        /// <summary>Pinta ya un objeto (útil antes de activar componentes que cachean SpriteRenderers en Awake, como DamageFlash).</summary>
        public void PaintNow(GameObject go) => DocPainter.Paint(go, null);

        /// <summary>Activa/desactiva la estela de movimiento de un objeto pintado.</summary>
        public void Trail(GameObject go, bool on = true)
        {
            var visual = DocPainter.Paint(go, null);
            if (visual != null) visual.SetTrail(on);
        }

        /// <summary>Personaje inactivo con collider, CharacterController2D e input scripteado. Agrega lo que necesites
        /// (configura campos con TestWorld.Set antes de activar) y llama a Activate.</summary>
        public GameObject PlayerShell(Vector2 position, out ScriptedCharacterInput input, string name = "Player")
        {
            var go = World.Track(new GameObject(name));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1.8f);
            go.AddComponent<CharacterController2D>();
            input = go.AddComponent<ScriptedCharacterInput>();
            return go;
        }

        /// <summary>Pinta, agrega CharacterCore (si falta) y activa un objeto creado inactivo.</summary>
        public CharacterCore Activate(GameObject go)
        {
            PaintNow(go);
            var core = go.GetComponent<CharacterCore>();
            if (core == null && go.GetComponent<CharacterController2D>() != null) core = go.AddComponent<CharacterCore>();
            go.SetActive(true);
            return core;
        }

        /// <summary>Punto visible sin collider (waypoints, objetivos, anclas). Se puede emparentar a otro objeto.</summary>
        public GameObject Marker(Vector2 position, Color color, float size = 0.3f, Transform parent = null)
        {
            var go = World.Track(new GameObject("~marker"));
            go.AddComponent<DocNoPaint>();
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;
            if (parent != null) go.transform.SetParent(parent, true);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = DocVisual.SpriteMaterial;
            sr.sprite = DocSprites.Circle;
            sr.color = color;
            sr.sortingOrder = 8;
            return go;
        }

        /// <summary>Línea visible sin collider (rutas, cuerdas, ziplines, alcances).</summary>
        public LineRenderer Line(Color color, float width, params Vector2[] points)
        {
            var go = World.Track(new GameObject("~line"));
            go.AddComponent<DocNoPaint>();
            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = DocVisual.SpriteMaterial;
            lr.startColor = lr.endColor = color;
            lr.startWidth = lr.endWidth = width;
            lr.sortingOrder = -1;
            lr.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++) lr.SetPosition(i, new Vector3(points[i].x, points[i].y, 0f));
            return lr;
        }

        /// <summary>Rectángulo que aparece un instante (área de un golpe, explosión, detección).</summary>
        public void Flash(Vector2 center, Vector2 size, Color color, float seconds = 0.15f)
        {
            var go = World.Track(new GameObject("~flash"));
            go.AddComponent<DocNoPaint>();
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = DocVisual.SpriteMaterial;
            sr.sprite = DocSprites.Block;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = color;
            sr.sortingOrder = 20;
            UnityEngine.Object.Destroy(go, seconds);
        }

        /// <summary>Espera n frames (1/30 s cada uno). Úsalo en lugar de Seconds cuando Time.timeScale es 0.</summary>
        public static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

        public void StartRecording(float seconds) => _recorder.Begin(Path.Combine(OutputRoot, Id), seconds, _rt);

        public IEnumerator WaitRecording()
        {
            while (_recorder.IsRecording) yield return null;
        }

        /// <summary>Atajo: graba seconds mientras corre la acción indicada.</summary>
        public IEnumerator Record(float seconds, IEnumerator action = null)
        {
            StartRecording(seconds);
            if (action != null) yield return action;
            yield return WaitRecording();
        }

        public static IEnumerator Seconds(float s) { yield return new WaitForSeconds(s); }

        /// <summary>Espera a que el personaje esté en el suelo (sin grabar).</summary>
        public static IEnumerator Settle(CharacterCore player, float extra = 0.2f, float timeout = 3f)
        {
            float t = 0f;
            while (player != null && !player.Controller.IsGrounded && t < timeout) { t += Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(extra);
        }

        public void Dispose()
        {
            World.Dispose();
            Time.captureFramerate = 0;
            Time.timeScale = 1f;
            if (_rt != null) { _rt.Release(); UnityEngine.Object.Destroy(_rt); }
        }
    }

    public static class DocPalette
    {
        public static readonly Color Background = Hex("1D2130");
        public static readonly Color Solid = Hex("5B6475");
        public static readonly Color Player = Hex("4FC3F7");
        public static readonly Color Enemy = Hex("EF5350");
        public static readonly Color Character = Hex("AB47BC");
        public static readonly Color Dynamic = Hex("FFA726");
        public static readonly Color Projectile = Hex("FFEB3B");
        public static readonly Color Trigger = Hex("FFFFFF", 0.14f);
        public static readonly Color Eye = Hex("15182A");
        public static readonly Color Accent = Hex("FFD54F");
        public static readonly Color Good = Hex("66BB6A");

        private static readonly Dictionary<string, Color> ByComponent = new Dictionary<string, Color>
        {
            { "WaterZone", Hex("2979FF", 0.35f) }, { "HazardZone", Hex("FF1744", 0.45f) },
            { "LadderZone", Hex("A1887F", 0.55f) }, { "WindZone2D", Hex("B2EBF2", 0.22f) },
            { "Checkpoint", Hex("FFD600", 0.6f) }, { "Collectible", Hex("FFC400") }, { "ItemPickup", Hex("FFAB00") },
            { "HealthPickup", Hex("FF4081") }, { "Teleporter", Hex("7C4DFF", 0.6f) }, { "LevelExit", Hex("00E676", 0.5f) },
            { "OneWayPlatform", Hex("A1887F") }, { "ConveyorBelt", Hex("455A64") }, { "SpringPad", Hex("66BB6A") },
            { "MovingPlatform", Hex("90A4AE") }, { "Elevator", Hex("80CBC4") }, { "FallingPlatform", Hex("BCAAA4") },
            { "PressurePlate", Hex("FFB300") }, { "Lever", Hex("FF9800") }, { "DoorWithKey", Hex("8D6E63") },
            { "BreakableObject", Hex("D7CCC8") }, { "DamageableObject", Hex("CE93D8") }, { "PushableBox", Hex("FFA726") },
            { "ProjectileBehaviour", Hex("FFEB3B") }, { "RopeAnchor", Hex("FFE082") }, { "ZiplinePath", Hex("FFE082", 0.5f) },
        };

        public static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            c.a = alpha;
            return c;
        }

        public static Color For(Collider2D col, out bool hasEye, out int order)
        {
            var go = col.gameObject;
            hasEye = false;
            order = 0;
            if (go.name.StartsWith("PlayerBullet")) { order = 15; return Hex("80DEEA"); }
            foreach (var mb in go.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                if (ByComponent.TryGetValue(mb.GetType().Name, out var c))
                {
                    order = col.isTrigger ? -2 : 1;
                    if (mb.GetType().Name == "ProjectileBehaviour") order = 15;
                    return c;
                }
            }

            bool isCharacter = go.GetComponent<CharacterCore>() != null;
            bool isEnemy = go.name.Contains("Enemy") || HasNamespace(go, "GameplayKit.AI");
            if (isCharacter || isEnemy || go.name.Contains("Player"))
            {
                hasEye = true;
                order = 10;
                if (go.name.Contains("Player") || go.CompareTag("Player")) return Player;
                if (isEnemy) return Enemy;
                return Character;
            }
            if (col.isTrigger) { order = -3; return Trigger; }
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) { order = 5; return Dynamic; }
            return Solid;
        }

        private static bool HasNamespace(GameObject go, string ns)
        {
            foreach (var mb in go.GetComponents<MonoBehaviour>())
                if (mb != null && mb.GetType().Namespace == ns) return true;
            return false;
        }
    }

    /// <summary>Crea un visual para cada Collider2D nuevo de la escena.</summary>
    [DefaultExecutionOrder(1000)]
    public class DocPainter : MonoBehaviour
    {
        private static readonly Dictionary<Collider2D, DocVisual> Painted = new Dictionary<Collider2D, DocVisual>();

        private void OnDestroy() => Painted.Clear();

        private void LateUpdate()
        {
            foreach (var col in FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude))
            {
                if (Painted.TryGetValue(col, out var v) && v != null) continue;
                if (col.GetComponent<Renderer>() != null || col.GetComponent<DocNoPaint>() != null) continue;
                if (col.GetComponentInParent<DocNoPaint>() != null) continue;
                Create(col, null);
            }
        }

        /// <summary>Pinta todos los colliders de go (o fuerza su color).</summary>
        public static DocVisual Paint(GameObject go, Color? color)
        {
            DocVisual first = null;
            foreach (var col in go.GetComponents<Collider2D>())
            {
                if (Painted.TryGetValue(col, out var v) && v != null)
                {
                    if (color.HasValue) v.SetColor(color.Value);
                }
                else v = Create(col, color);
                if (first == null) first = v;
            }
            return first;
        }

        private static DocVisual Create(Collider2D col, Color? forced)
        {
            Color c = DocPalette.For(col, out bool eye, out int order);
            if (forced.HasValue) c = forced.Value;
            var go = new GameObject("~visual");
            go.transform.SetParent(col.transform, false);
            var visual = go.AddComponent<DocVisual>();
            visual.Init(col, c, eye, order);
            Painted[col] = visual;
            return visual;
        }
    }

    /// <summary>Marca un objeto para que el pintor lo ignore.</summary>
    public class DocNoPaint : MonoBehaviour { }

    /// <summary>Sprite que sigue la forma, posición y estado del collider.</summary>
    public class DocVisual : MonoBehaviour
    {
        private Collider2D _col;
        private SpriteRenderer _sr;
        private Transform _eye;
        private TrailRenderer _trail;
        private static Material _material;

        public static Material SpriteMaterial
        {
            get
            {
                if (_material != null) return _material;
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader);
                return _material;
            }
        }

        public void Init(Collider2D col, Color color, bool eye, int order)
        {
            _col = col;
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sharedMaterial = SpriteMaterial;
            _sr.sortingOrder = order;
            _sr.color = color;
            if (col is CircleCollider2D) _sr.sprite = DocSprites.Circle;
            else
            {
                _sr.sprite = eye || col is CapsuleCollider2D ? DocSprites.Rounded : DocSprites.Block;
                _sr.drawMode = SpriteDrawMode.Sliced;
            }
            if (eye)
            {
                var e = new GameObject("~eye");
                e.transform.SetParent(transform, false);
                var esr = e.AddComponent<SpriteRenderer>();
                esr.sharedMaterial = SpriteMaterial;
                esr.sprite = DocSprites.Circle;
                esr.color = DocPalette.Eye;
                esr.sortingOrder = order + 1;
                _eye = e.transform;
                SetTrail(col.GetComponent<CharacterCore>() != null && (col.name.Contains("Player") || col.CompareTag("Player")));
            }
            Sync();
        }

        public void SetColor(Color c) { if (_sr != null) _sr.color = c; }

        public void SetTrail(bool on)
        {
            if (on && _trail == null)
            {
                _trail = gameObject.AddComponent<TrailRenderer>();
                _trail.sharedMaterial = SpriteMaterial;
                _trail.time = 0.45f;
                _trail.minVertexDistance = 0.05f;
                _trail.widthCurve = new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(1f, 0f));
                var c = _sr.color;
                _trail.colorGradient = new Gradient
                {
                    colorKeys = new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                    alphaKeys = new[] { new GradientAlphaKey(0.45f, 0f), new GradientAlphaKey(0f, 1f) }
                };
                _trail.sortingOrder = _sr.sortingOrder - 1;
            }
            if (_trail != null) _trail.emitting = on;
        }

        private void LateUpdate() => Sync();

        private void Sync()
        {
            if (_col == null) { Destroy(gameObject); return; }
            bool visible = _col.enabled && _col.gameObject.activeInHierarchy;
            _sr.enabled = visible;
            if (_eye != null) _eye.gameObject.SetActive(visible);

            Vector2 size;
            Vector2 offset = _col.offset;
            switch (_col)
            {
                case BoxCollider2D b: size = b.size; break;
                case CapsuleCollider2D cap: size = cap.size; break;
                case CircleCollider2D c: size = Vector2.one * c.radius * 2f; break;
                default:
                    var lossy = transform.lossyScale;
                    var bounds = _col.bounds;
                    size = new Vector2(bounds.size.x / Mathf.Max(0.0001f, Mathf.Abs(lossy.x)), bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(lossy.y)));
                    offset = _col.transform.InverseTransformPoint(bounds.center);
                    break;
            }
            transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            if (_sr.drawMode == SpriteDrawMode.Sliced) { transform.localScale = Vector3.one; _sr.size = size; }
            else transform.localScale = new Vector3(size.x, size.y, 1f);

            if (_eye != null)
            {
                float s = Mathf.Min(size.x, size.y) * 0.22f;
                _eye.localScale = _sr.drawMode == SpriteDrawMode.Sliced ? new Vector3(s, s, 1f) : new Vector3(s / size.x, s / size.y, 1f);
                var p = new Vector2(size.x * 0.22f, size.y * 0.22f);
                _eye.localPosition = _sr.drawMode == SpriteDrawMode.Sliced ? (Vector3)p : new Vector3(0.22f, 0.22f, 0f);
            }
        }
    }

    public static class DocSprites
    {
        private static Sprite _block, _rounded, _circle;
        // Comparación con el operador de Unity (no ??=): al salir de Play los sprites creados en runtime se destruyen
        // pero la referencia estática sobrevive si el dominio no se recarga.
        public static Sprite Block { get { if (_block == null) _block = RoundedRect(64, 7, 7); return _block; } }
        public static Sprite Rounded { get { if (_rounded == null) _rounded = RoundedRect(64, 18, 18); return _rounded; } }
        public static Sprite Circle { get { if (_circle == null) _circle = MakeCircle(128); return _circle; } }

        private static Sprite RoundedRect(int size, int radius, int border)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                byte a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255);
                if (d <= 0.0001f) a = 255;
                px[y * size + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d) * 255));
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }

    public class DocFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector2 Offset;
        public bool LockY;
        private Vector3 _vel;

        public void Snap()
        {
            if (Target == null) return;
            transform.position = Goal();
        }

        private Vector3 Goal()
        {
            var p = (Vector2)Target.position + Offset;
            return new Vector3(p.x, LockY ? transform.position.y : p.y, -10f);
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, Goal(), ref _vel, 0.18f);
        }
    }

    /// <summary>Teclas del input scripteado y textos de estado, dibujados en la propia cámara.</summary>
    public class DocOverlay : MonoBehaviour
    {
        public ScriptedCharacterInput Input;
        public Func<string> Caption;
        public string Title;

        private Text _keys, _caption, _title;
        private readonly Dictionary<string, float> _flash = new Dictionary<string, float>();

        public void Build(Camera cam)
        {
            var canvasGo = new GameObject("DocOverlay");
            canvasGo.transform.SetParent(transform, false);
            canvasGo.AddComponent<DocNoPaint>();
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1f;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(DocStage.Width, DocStage.Height);
            _keys = MakeText(canvasGo.transform, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(24, 18), 30);
            _caption = MakeText(canvasGo.transform, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-24, -16), 28);
            _title = MakeText(canvasGo.transform, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(24, -16), 26);
        }

        private static Text MakeText(Transform parent, TextAnchor anchor, Vector2 pivot, Vector2 pos, int size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(900, 120);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = anchor;
            t.color = new Color(1f, 1f, 1f, 0.92f);
            t.supportRichText = true;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(2, -2);
            return t;
        }

        private void Pulse(string key) => _flash[key] = Time.time + 0.35f;

        private void Update()
        {
            _title.text = Title ?? "";
            _caption.text = Caption != null ? Caption() : "";
            if (Input == null) { _keys.text = ""; return; }

            if (Input.DashPressedThisFrame) Pulse("Q Dash");
            if (Input.InteractPressedThisFrame) Pulse("E");
            if (Input.JumpPressedThisFrame) Pulse("Space");
            foreach (CharacterAction a in Enum.GetValues(typeof(CharacterAction)))
                if (Input.GetActionDown(a)) Pulse(KeyFor(a));

            var keys = new List<string>();
            var m = Input.Move;
            if (m.x < -0.1f) keys.Add("←");
            if (m.x > 0.1f) keys.Add("→");
            if (m.y > 0.1f) keys.Add("↑");
            if (m.y < -0.1f) keys.Add("↓");
            if (Input.Run) keys.Add("Shift");
            if (Input.Crouch) keys.Add("Ctrl");
            if (Input.Jump && !keys.Contains("Space")) keys.Add("Space");
            foreach (CharacterAction a in Enum.GetValues(typeof(CharacterAction)))
                if (Input.GetAction(a) && !keys.Contains(KeyFor(a))) keys.Add(KeyFor(a));
            foreach (var kv in _flash)
                if (kv.Value > Time.time && !keys.Contains(kv.Key)) keys.Add(kv.Key);

            var sb = new StringBuilder();
            foreach (var k in keys) sb.Append("[ ").Append(k).Append(" ]  ");
            _keys.text = sb.ToString();
        }

        private static string KeyFor(CharacterAction a)
        {
            switch (a)
            {
                case CharacterAction.Attack: return "J Attack";
                case CharacterAction.Special: return "K Special";
                case CharacterAction.Fly: return "F Fly";
                case CharacterAction.Roll: return "Alt Roll";
                case CharacterAction.Blink: return "C Blink";
                default: return a.ToString();
            }
        }
    }

    /// <summary>Lee el RenderTexture de la cámara al final de cada frame y lo guarda como PNG numerado.</summary>
    public class DocRecorder : MonoBehaviour
    {
        public bool IsRecording { get; private set; }
        private Texture2D _tex;

        public void Begin(string folder, float seconds, RenderTexture rt)
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            IsRecording = true;
            StartCoroutine(Run(folder, Mathf.RoundToInt(seconds * DocStage.Fps), rt));
        }

        private IEnumerator Run(string folder, int frames, RenderTexture rt)
        {
            if (_tex == null) _tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                _tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                _tex.Apply();
                RenderTexture.active = prev;
                File.WriteAllBytes(Path.Combine(folder, $"f_{i:0000}.png"), _tex.EncodeToPNG());
            }
            IsRecording = false;
        }

        private void OnDestroy()
        {
            if (_tex != null) Destroy(_tex);
        }
    }
}
