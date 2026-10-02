---
title: Pruebas
description: Cómo están organizadas las pruebas de Gameplay Kit, cómo correrlas, cómo escribir una nueva y cómo se producen los clips de la documentación.
---

# Pruebas

Gameplay Kit trae 92 pruebas automáticas: 4 de editor que cuidan el catálogo de componentes y 88 de play mode
que ejercitan las mecánicas con física real. Los mismos helpers manejan los escenarios que graban los clips de
esta documentación.

## Cómo están organizadas

| Carpeta | Assembly | Modo | Qué revisa |
|---|---|---|---|
| `Tests/Editor/` | `GameplayKit.Tests.Editor` | Edit mode | El catálogo de componentes. |
| `Tests/Runtime/` | `GameplayKit.Tests.Runtime` | Play mode | El gameplay, más los helpers `TestWorld` y `ScriptedCharacterInput`. |

Los dos assemblies usan el namespace `GameplayKit.Tests`, no se referencian automáticamente y solo compilan con
`UNITY_INCLUDE_TESTS`, que se define cuando el paquete está marcado como testable (ver más abajo).

### Pruebas de editor: el catálogo

`ComponentCatalogTests` atrapa errores que las pruebas de juego no ven:

| Prueba | Garantiza |
|---|---|
| `EveryComponent_HasItsOwnScriptFile` | Todo `MonoBehaviour` no abstracto del assembly de runtime está en un archivo con su mismo nombre. Si no, funciona con `AddComponent` pero aparece como *Missing Script* al guardarlo. |
| `EveryComponent_CanBeAddedToAnEmptyGameObject` | Todo componente se puede agregar a un GameObject vacío sin errores, es decir, funciona directo desde **Add Component**. |
| `NoComponent_SharesItsNameWithABuiltInUnityComponent` | Ningún nombre de clase choca con un componente nativo de Unity (por eso la zona de viento se llama `WindZone2D`). |
| `MenuBuilders_CreatePlayerAndEnemyWithoutMissingScripts` | El menú **GameplayKit** arma un jugador con tag, `CharacterCore` y `KeyboardInputReader`, y un enemigo, sin scripts faltantes. |

### Pruebas de play mode: las mecánicas

| Archivo | Pruebas | Cubre |
|---|---|---|
| `MovementTests.cs` | 15 | Detección de suelo sin configurar, caminar/correr, salto y multi-salto, dash, wall jump/slide, planeo, jetpack, agacharse, escaleras, nado. |
| `GameplayTests.cs` | 16 | Vida, muerte y respawn, checkpoints, zonas de daño, golpes melee/hitscan/proyectil (nunca al atacante), resortes, coleccionables y puntaje, llaves y puertas, plataformas móviles, cintas, persecución, patrulla en paredes y bordes, un `AIBrain` básico. |
| `MoreMechanicsTests.cs` | 26 | Vuelo, rodada, blink, `PlayerAttack`, interactuar + palanca + elevador, placas de presión, cornisas, tirolesas, cuerdas, borde, dash en 8 direcciones, gatear, ground slam, seguir rutas, daño por caída, aturdimiento, knockback, plataformas de un sentido y que caen, teletransportes, cajas empujables, viento, enemigos que disparan, daño por contacto. |
| `ThirdRoundTests.cs` | 31 | Wall cling, pendientes, top-down, atravesar plataformas, jump buffer, persistencia, ataques cargados, cambio de arma, apuntado, huida, patrulla acotada, pathfinding sin NavMesh, todas las acciones y decisiones de IA, el spawner, vidas, pickups de vida, parpadeo y números de daño, shake y zoom de cámara, todos los managers, salida de nivel, el puente al Animator. |

Los nombres de las pruebas describen el comportamiento esperado (`EnemyPatrol_DoesNotWalkOffEdges`,
`HazardZone_KeepsDamagingWhilePlayerStandsStill`...), lo que convierte a la suite en una referencia ejecutable
de cómo se configura cada componente.

## Correr las pruebas

1. Marca el paquete como testable en el `Packages/manifest.json` de tu proyecto:

    ```json
    {
      "dependencies": {
        "com.alejocastellanos.gameplaykit": "file:/ruta/a/com.alejocastellanos.gameplaykit"
      },
      "testables": ["com.alejocastellanos.gameplaykit"]
    }
    ```

2. Abre **Window → General → Test Runner**.
3. Corre la pestaña **EditMode** para las pruebas del catálogo y **PlayMode** para las mecánicas.

Desde la línea de comandos, usa los parámetros estándar de pruebas de Unity:

```bash
Unity -batchmode -projectPath /ruta/al/proyecto -runTests -testPlatform PlayMode -testResults results.xml
```

## Helpers de prueba

Los dos están en `Tests/Runtime/TestSupport.cs`.

### TestWorld

Construye niveles mínimos por código y destruye todo lo que creó al hacer `Dispose`.

| Miembro | Descripción |
|---|---|
| `GameObject Track(GameObject go)` | Registra un objeto para destruirlo en `Dispose`. Lo devuelve. |
| `GameObject Block(string name, Vector2 center, Vector2 size, bool trigger = false)` | Un GameObject registrado con un `BoxCollider2D`. |
| `GameObject Ground(float y = -0.5f, float width = 200f)` | Un suelo de 1 unidad de grosor cuya **cara superior** está en `y`. |
| `CharacterCore Player(Vector2 position, out ScriptedCharacterInput input, params Type[] abilities)` | Un personaje armado como lo haría un usuario: `CapsuleCollider2D` (0.8 × 1.8), `CharacterController2D`, `ScriptedCharacterInput`, las habilidades en orden y `CharacterCore` al final. |
| `GameObject Point(Vector2 position)` | Un GameObject vacío registrado, útil como objetivo o waypoint. |
| `static void Set(object target, string field, object value)` | Asigna un `[SerializeField]` privado (buscando también en las clases base), como lo haría el Inspector. Lanza un error si el campo no existe. |
| `void Dispose()` | Destruye todos los objetos registrados. |

### ScriptedCharacterInput

Un `ICharacterInput` que manejas desde la prueba. Tú fijas el estado *mantenido* y las banderas de "presionado
o soltado este frame" se derivan solas, como con un teclado. Tiene `[DefaultExecutionOrder(-1000)]`, así las
habilidades ven el input del frame actual.

| Miembro | Descripción |
|---|---|
| `Vector2 Move` | Input de movimiento. |
| `bool Jump`, `Run`, `Crouch` | Botones mantenidos. Pasar `Jump` a true genera un `JumpPressedThisFrame`; a false, un `JumpReleasedThisFrame`. |
| `void TapDash()`, `void TapInteract()` | Pulsa dash o interactuar por un solo frame. |
| `void Hold(CharacterAction)`, `void Release(CharacterAction)` | Mantiene o suelta una acción. |
| `void TapAction(CharacterAction)` | Pulsa una acción por un solo frame. |

### Patrones que se repiten en la suite

- **Esperar a tocar suelo** antes de actuar: un bucle `while (!player.Controller.IsGrounded) yield return null;`
  con un tiempo límite.
- **Configurar antes de `Awake`**: para componentes que leen sus campos en `Awake`/`OnEnable` (o arman tablas
  ahí, como `AIBrain`), crea el GameObject inactivo, agrega los componentes, completa los campos con
  `TestWorld.Set` y después `SetActive(true)`.
- **Evitar la física que no hace falta**: las pruebas de lógica pura ubican los objetos bien arriba (y = 50)
  para que nada choque.
- **Correr en segundo plano**: el `SetUp` pone `Application.runInBackground = true` para que las pruebas sigan
  aunque el editor pierda el foco.

## Escribir una prueba nueva

Agrega una clase en `Tests/Runtime` (o en tu propio assembly de pruebas de play mode que referencie
`GameplayKit.Runtime` y `GameplayKit.Tests.Runtime`). Esta prueba verifica que un `AIBrain` vuelva a reposo
cuando su objetivo sale de rango:

```csharp
using System.Collections;
using System.Collections.Generic;
using GameplayKit.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayKit.Tests
{
    public class MyAITests
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

        [UnityTest]
        public IEnumerator AIBrain_GoesBackToIdle_WhenTargetLeavesRange()
        {
            var target = _world.Point(new Vector2(3f, 50f));

            // El enemigo se arma inactivo para que AIBrain.Awake vea los estados configurados, como en una escena.
            var enemy = _world.Track(new GameObject("Enemy"));
            enemy.SetActive(false);
            enemy.transform.position = new Vector2(0f, 50f);
            var wait = enemy.AddComponent<AIActionWait>();
            var inRange = enemy.AddComponent<AIDecisionTargetInRange>();
            TestWorld.Set(inRange, "range", 5f);
            var brain = enemy.AddComponent<AIBrain>();
            TestWorld.Set(brain, "target", target.transform);
            TestWorld.Set(brain, "states", new List<AIState>
            {
                new AIState { name = "Idle", actions = new List<AIActionBase> { wait },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, trueTargetState = "Alert" } } },
                new AIState { name = "Alert", actions = new List<AIActionBase> { wait },
                    transitions = new List<AITransition> { new AITransition { decision = inRange, falseTargetState = "Idle" } } },
            });
            enemy.SetActive(true);

            yield return null;
            yield return null;
            Assert.AreEqual("Alert", brain.CurrentState.name, "El objetivo está a menos de 5 unidades");

            target.transform.position = new Vector2(20f, 50f);
            yield return null;
            yield return null;
            Assert.AreEqual("Idle", brain.CurrentState.name, "El objetivo salió de rango");
        }
    }
}
```

Consejos:

- Nombra las pruebas `Sujeto_ComportamientoEsperado` y escribe mensajes de assert que expliquen qué *debería*
  pasar.
- Verifica comportamiento (posiciones, vida, estados), no detalles de implementación.
- Usa `WaitForSeconds` para chequeos que dependen de la física y deja márgenes: la física no es exacta al frame.
- Que cada prueba sea autocontenida: todo pasa por `TestWorld` para que `TearDown` limpie.

## Cómo se hacen los clips de la documentación

Cada clip de este sitio lo graba una prueba de play mode explícita. No se captura nada a mano, así que los clips
se pueden regenerar cada vez que cambia un componente.

### El framework de captura

Las fuentes de captura viven en `Documentation~/capture/`:

| Archivo | Contenido |
|---|---|
| `DocStage.cs` | `DocStage` (un `TestWorld` más una cámara que graba) y sus helpers: `DocPalette` (colores), `DocPainter`/`DocVisual` (dibujan cada `Collider2D` como una forma de color según su rol, así los escenarios solo arman física), `DocFollow` (seguimiento de cámara), `DocOverlay` (teclas presionadas y un texto en vivo) y `DocRecorder` (escribe los cuadros). |
| `GameplayKit.DocCapture.asmdef` | Un assembly de pruebas de play mode que referencia `GameplayKit.Runtime`, `GameplayKit.Tests.Runtime`, `UnityEngine.UI` y NUnit. |
| `Scenarios/*.cs` | Un método `[UnityTest, Explicit, Category("DocCapture")]` por clip, llamado `<Componente>_Clip`. |

Un `DocStage` fija `Time.captureFramerate` en 30 fps y renderiza una cámara ortográfica 16:9 a un
`RenderTexture` de 960×540. Desde que se llama `StartRecording(segundos)`, `DocRecorder` lee la textura al final
de cada frame y escribe PNG numerados (`f_0000.png`, `f_0001.png`...) en `<proyecto>/DocCaptures/frames/<Id>/`,
donde `Id` es el nombre de la clase del componente (o `Showcase`). Un escenario se ve igual que una prueba de
juego:

```csharp
[UnityTest, Explicit, Category("DocCapture")]
public IEnumerator PlayerDash_Clip()
{
    var s = new DocStage("PlayerDash", new Vector2(2f, 2.5f), 9f);
    try
    {
        s.World.Ground();
        var p = s.World.Player(new Vector2(-4f, 0.5f), out var input, typeof(PlayerWalkRun), typeof(PlayerDash));
        s.ShowInput(input);
        yield return DocStage.Settle(p);
        s.StartRecording(4f);
        yield return DocStage.Seconds(0.4f);
        input.Move = Vector2.right; yield return DocStage.Seconds(0.5f);
        input.TapDash();            yield return DocStage.Seconds(0.8f);
        input.Move = Vector2.zero;
        yield return s.WaitRecording();
    }
    finally { s.Dispose(); }
}
```

### Grabar los clips

1. Unity ignora las carpetas cuyo nombre termina en `~`, así que `Documentation~/capture/` nunca se compila
   dentro del paquete. **Copia la carpeta `capture` dentro de `Assets/` de un proyecto de Unity** que use el
   paquete.
2. Asegúrate de que el paquete esté en `testables` (el assembly de captura referencia
   `GameplayKit.Tests.Runtime`).
3. En la pestaña **PlayMode** del Test Runner, selecciona las pruebas `DocCapture` que quieras y córrelas. Son
   `Explicit`, así que nunca se ejecutan en una corrida normal.
4. Codifica cada carpeta de cuadros con ffmpeg a un MP4 H.264 y un póster JPG con el nombre del componente,
   por ejemplo:

    ```bash
    ffmpeg -framerate 30 -i DocCaptures/frames/PlayerDash/f_%04d.png \
           -c:v libx264 -pix_fmt yuv420p -movflags +faststart docs/assets/clips/PlayerDash.mp4
    ffmpeg -i DocCaptures/frames/PlayerDash/f_0060.png docs/assets/clips/PlayerDash.jpg
    ```

5. Deja los archivos en `docs/assets/clips/`. El generador del sitio inserta `<Componente>.mp4` en la página del
   componente y usa `<Componente>.jpg` en su tarjeta del catálogo siempre que existan.

Los clips son los mismos para los dos idiomas, por eso los textos en pantalla son cortos y neutros ("HP 3/5",
"Score 30").
