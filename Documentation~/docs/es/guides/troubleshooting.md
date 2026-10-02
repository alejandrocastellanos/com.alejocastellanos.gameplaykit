---
title: Solución de problemas
description: Respuestas a los problemas de configuración más comunes de Gameplay Kit.
---

# Solución de problemas

Casi todos los problemas se reducen a un componente que falta, un collider del tipo equivocado o una opción
del proyecto. Empieza por la consola: los mensajes del kit empiezan con `[GameplayKit]`.

## El personaje atraviesa el suelo

- **El suelo necesita un collider 2D**: `BoxCollider2D`, `TilemapCollider2D`, `CompositeCollider2D`,
  `EdgeCollider2D`... Un `BoxCollider` 3D no hace nada en la física 2D.
- **El suelo no debe ser trigger.** Los triggers no detienen cuerpos y nunca cuentan como suelo.
- **Revisa la Layer Collision Matrix** (**Project Settings → Physics 2D**). Si moviste al jugador o al suelo a
  capas propias, esas capas tienen que colisionar.
- **Atraviesa plataformas delgadas al caer rápido**: pon el **Collision Detection** del `Rigidbody2D` del
  jugador en *Continuous* (**GameplayKit → Create Player** ya lo hace).
- El `Rigidbody2D` debe ser **Dynamic** y **Simulated**. `CharacterController2D` agrega uno solo si falta.

## El personaje no detecta el suelo (no puede saltar)

`CharacterController2D.IsGrounded` sale de un círculo pequeño (**Ground Check Radius** 0.1) en el centro de la
base del collider del personaje, revisado en cada paso de física contra **Ground Layers** (*Everything* por
defecto). Selecciona el personaje en Play: el gizmo es verde cuando está en el suelo y rojo cuando no.

- **El collider debe estar en el mismo GameObject que `CharacterController2D`.** Lo busca con `GetComponent`,
  no en los hijos. Si tu collider está en un hijo, asigna un transform **Ground Check** a la altura de los
  pies. Sin ninguno de los dos avisa en la consola que necesita un Collider2D o un Ground Check.
- Si asignaste **Ground Check**, verifica que esté en los pies y no en el pivote, en medio del cuerpo.
- Si restringiste **Ground Layers**, todas las superficies caminables (plataformas, de un sentido, móviles)
  tienen que estar en alguna de esas capas.
- Los triggers nunca cuentan como suelo. Los colliders del propio personaje (incluso hijos bajo su
  Rigidbody2D) se ignoran, así que no necesitas una capa "Player".

## Las habilidades no hacen nada

- **¿Hay un `CharacterCore`?** Las habilidades son componentes normales: nadie las llama sin un core en el
  mismo GameObject (o en un padre del objeto donde están).
- **¿Armaste el personaje por código?** El core descubre las habilidades una sola vez, en su `Awake`. Agrégalo
  al final, o crea el GameObject inactivo, agrega todo y actívalo al terminar.
- **Busca `[GameplayKit] ... se desactivó` en la consola.** Una habilidad que lanza una excepción se desactiva
  (`AbilityEnabled = false`) para no romper a las demás. Corrige la causa (casi siempre una referencia que
  falta en el Inspector) y vuelve a entrar en Play.
- **El personaje puede estar suspendido.** Mientras está muerto, aturdido o en knockback, `CharacterCore` no
  ejecuta ninguna habilidad. Revisa `CharacterCore.AbilitiesSuspended` y `Condition.CurrentState`.
- **¿Está marcada la habilidad?** `CharacterCore` se salta las habilidades con el componente desmarcado en el
  Inspector, además de las que tienen `AbilityEnabled = false`. Volver a marcar una habilidad no revive una
  que el core desactivó por una excepción: su `AbilityEnabled` sigue en false hasta que vuelvas a entrar en
  Play.
- **Dos habilidades se pelean por el mismo valor.** Dentro de una fase corren en el orden del Inspector y gana
  la última; por ejemplo, `PlayerCrouch` debe ir después de `PlayerWalkRun`. No combines habilidades que son
  alternativas: `PlayerJump` *o* `PlayerMultiJump`, `PlayerWallSlide` *o* `PlayerWallCling`,
  `PlayerTopDownMovement` *en lugar de* `PlayerWalkRun` + saltos.
- **¿Se usa la fuente de input correcta?** `CharacterCore` solo agrega `KeyboardInputReader` cuando ningún
  componente implementa `ICharacterInput`. Si agregaste tu propio componente de input, el de teclado no está y
  el tuyo tiene que aportar todo.

## El input no funciona

El kit lee todo el input a través de `InputCompat`, que elige el sistema según **Project Settings → Player →
Other Settings → Active Input Handling**:

| Active Input Handling | Qué usa el kit |
|---|---|
| *Input System Package (New)* | El Input System. El paquete debe estar instalado; el kit lo detecta solo (versión 1.0.0 o superior). |
| *Input Manager (Old)* | El Input Manager clásico: ejes `Horizontal`/`Vertical`, botón `Jump` y `KeyCode`s. Los botones de gamepad asignados como `PadButton` y el stick derecho (apuntar) **no funcionan** en este modo. |
| *Both* | El Input System. |

Otras cosas para revisar:

- **La ventana Game necesita el foco** para recibir el teclado.
- **Atacar, correr, dash y pausa con gamepad necesitan el Input System.** Agacharse no tiene botón de gamepad
  por defecto.
- **La pausa no responde justo después de dar Play.** `PauseManager` ignora su tecla durante el primer segundo
  y hasta verla suelta; es a propósito.
- **Errores de `StandaloneInputModule`** con el Input System nuevo: tu `EventSystem` usa el módulo viejo.
  Reemplázalo por *Input System UI Input Module* (**GameplayKit → Create HUD** elige el correcto solo).
- **Las teclas de acciones** (Attack J, Special K, Fly F, Roll Alt izquierdo, Blink C) y sus botones de
  gamepad se configuran en `KeyboardInputReader` → **Action Bindings**. Ver [Input](input.md).

## "Missing Script" en un componente del kit o propio

Unity solo puede guardar un `MonoBehaviour` en escenas y prefabs si está en un archivo con **exactamente el
mismo nombre** que la clase. Una clase en un archivo con otro nombre funciona con `AddComponent` por código,
pero aparece como *Missing Script* al guardarla. Todos los componentes del kit cumplen esta regla (la prueba de
editor `EveryComponent_HasItsOwnScriptFile` lo verifica). Aplica la misma regla a tus habilidades, acciones y
decisiones.

Otras causas: se quitó el paquete o cambió su ruta en `Packages/manifest.json` (desaparecen todos los
componentes del kit a la vez), o hay errores de compilación en algún script (Unity no carga *ningún* script
hasta corregirlos).

## Los enemigos se caen de las plataformas

Solo [EnemyPatrol](../components/ai/EnemyPatrol.md) revisa los bordes: lanza un rayo de **Edge Check Distance**
(0.5) hacia abajo justo delante de su esquina y gira si no hay suelo. Ninguno de los demás lo hace:
`EnemyPatrolWithinBounds`, `EnemyChase`, `EnemyFlee`, `AIActionPatrolPoints`, `AIActionMoveToTarget` y
`AIActionFlee` caminan hacia donde estén su objetivo o sus límites. Para mantenerlos sobre una plataforma:

- Pon los límites o waypoints dentro de la plataforma.
- Cierra los bordes con paredes, o pon una [HazardZone](../components/environment/HazardZone.md) letal abajo.
- Usa `EnemyPatrol` donde importe detectar bordes, o escribe una decisión propia que lance un rayo hacia abajo
  delante del enemigo (ver [Enemigos e IA](enemies-and-ai.md#escribir-tus-propias-acciones-y-decisiones)).

Si `EnemyPatrol` gira una y otra vez en el lugar, su rayo de borde o de pared está chocando con algo
inesperado: revisa **Ground Layers**/**Obstacle Layers**, que el collider del enemigo no sea trigger y que no
esté flotando sobre el suelo.

## La cámara tiembla

- Pon **Interpolate** del `Rigidbody2D` del jugador en *Interpolate*. La física corre a ritmo fijo y la cámara
  cada frame; sin interpolación el jugador parece dar saltitos. *Create Player* ya lo configura.
- No emparentes la cámara al jugador si tiene `CameraFollow`: `CameraFollow` ya lo sigue suavemente en
  `LateUpdate`.
- No cambies el orden de ejecución: `CameraFollow` mueve, `CameraBounds` (orden 50) limita y `CameraShake`
  (orden 100) suma su desplazamiento al final. Mover la cámara desde otro script en `LateUpdate` puede
  pelearse con ellos.
- Si la cámara se desliza de un lado a otro en un borde del nivel, el área de límites es más chica que la
  vista en ese eje y `CameraBounds` la centra; revisa que **Min Bounds**/**Max Bounds** sean los que querías.

## Capas y tags

El kit está pensado para funcionar en un proyecto nuevo sin agregar capas ni tags:

- **Las máscaras de capas son *Everything* por defecto.** Los chequeos de suelo, pared y visión pasan por
  `PhysicsQuery2D`, que ignora los colliders del propio personaje, así que no necesitas una capa "Player"
  para que no se detecte a sí mismo.
- **El jugador debe tener el tag `Player`** (viene con Unity). Lo buscan `Checkpoint`, `Collectible`,
  `LevelExit`, `FallingPlatform`, `Teleporter`, `PushableBox`, `EnemyMeleeOnContact` y la búsqueda automática de
  objetivo de `CameraFollow`, `CameraZoomBySpeed`, `UIHealthBar`, `EnemyChase`, `EnemyFlee`,
  `EnemyShootOnSight` y `EnemySpawner`. *Create Player* pone el tag.
- **Los tags opcionales no necesitan existir.** Escaleras, agua y anclas de cuerda se reconocen por
  componentes marcadores (`LadderZone`, `WaterZone`, `RopeAnchor`); los tags `Ladder`, `Water` y `RopeAnchor`
  son una alternativa opcional. Las comparaciones de tags pasan por `TagFilter`, que no lanza error si un tag
  no está definido en el Tag Manager.

## Otras preguntas comunes

**El jugador se resbala por las pendientes estando quieto.** El controlador le pone al collider un material
sin fricción (para que no se pegue a las paredes). Agrega [PlayerSlopeWalk](../components/movement/PlayerSlopeWalk.md).

**El personaje se pega a las paredes.** Seguramente le asignaste un material físico con fricción al collider o
al Rigidbody2D; el kit solo agrega su material sin fricción cuando ninguno de los dos tiene uno.

**Reiniciar desde el menú de pausa no hace nada en un build.** La escena debe estar en la lista del build
(**File → Build Profiles**); en el editor `UIPauseMenu` puede recargarla aunque no esté.

**`LevelExit` avisa "no hay escena siguiente".** No tiene **Scene Name** y la escena actual es la última de la
lista del build (o no está en ella). Asigna **Scene Name** o agrega la escena siguiente a la lista.

**Los managers o el puntaje desaparecen al cargar una escena.** Un manager persistente (`GameManager`,
`AudioManager`, `SaveLoadSystem`, `SceneTransitionManager`) comparte GameObject con otros componentes y su
duplicado destruyó el objeto entero. Ver [Construir un nivel](building-a-level.md#managers).

## Las pruebas no aparecen en el Test Runner

Las pruebas de un paquete solo se compilan si el paquete está marcado como testable. Agrega esto al
`Packages/manifest.json` de tu proyecto, al lado de `dependencies`:

```json
"testables": ["com.alejocastellanos.gameplaykit"]
```

Después abre **Window → General → Test Runner**: las pruebas del catálogo están en **EditMode** y las de juego
en **PlayMode**. Ver [Pruebas](../reference/testing.md).
