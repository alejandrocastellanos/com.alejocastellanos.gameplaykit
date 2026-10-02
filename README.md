# Gameplay Kit

Kit modular de gameplay 2D para Unity 6: movimiento de personaje, combate, vida, IA de enemigos,
entorno interactivo, cámara, managers y UI. Todo se arma por componentes desde **Add Component**,
sin escribir código para combinarlos, y funciona con valores por defecto (capas, tags y referencias
opcionales).

## Documentación

La documentación completa (inglés y español), con un clip de cada componente, guías y referencia del
Inspector, vive en `Documentation~/` y se publica en GitHub Pages con el workflow
`.github/workflows/docs.yml` (activa *Settings → Pages → Source: GitHub Actions*). Para verla en local:
`cd Documentation~ && pip install -r requirements.txt && python tools/generate.py && mkdocs serve`.

## Empezar en un minuto

- **GameplayKit → Create Demo Scene** genera un nivel jugable que recorre las mecánicas principales.
- **GameplayKit → Create Top-Down Demo Scene** genera un dungeon visto desde arriba con el jugador top-down,
  enemigos que persiguen en 8 direcciones, torreta, llave y puerta, palanca y teletransportes.
  También está como sample: *Package Manager → Gameplay Kit → Samples → Demo 2D → Import*.
- **GameplayKit → Create Player / Create Top-Down Player / Create Enemy / Create Platform / Create 2D Camera / Create Managers / Create HUD**
  crean objetos listos para usar (también con clic derecho en la jerarquía → *GameplayKit*).

Controles por defecto: A/D o flechas para moverse, Espacio saltar, Shift correr, Ctrl agacharse,
Q dash, J atacar, E interactuar, Esc pausa; abajo + salto atraviesa plataformas de un sentido. Con gamepad: stick izquierdo, A/Cruz saltar, X/Cuadrado
atacar. Las acciones (atacar, especial, volar, rodar, blink) se reasignan en `KeyboardInputReader`.

## Cómo se arma un personaje

1. Un GameObject con un `Collider2D` (capsule recomendado) y `CharacterController2D`.
2. Las habilidades que quieras: `PlayerWalkRun`, `PlayerJump` o `PlayerMultiJump`, `PlayerDash`,
   `PlayerWallJump`, `PlayerCrouch`, `PlayerClimbLadder`, `PlayerSwim`, `PlayerAttack`, etc.
3. `CharacterCore`, que descubre las habilidades y las ejecuta cada frame. Si no hay una fuente de
   input, agrega `KeyboardInputReader` sola.

Para un juego de vista superior usa `PlayerTopDownMovement` (anula la gravedad) en lugar de
`PlayerWalkRun` + saltos; *Create Top-Down Player* lo arma completo.

Para controlarlo con otra cosa (IA, replays, PlayerInput con acciones) implementa `ICharacterInput`.

## Qué incluye

- **Movimiento:** caminar/correr, salto con *coyote time* y *jump buffer*, multi-salto, dash (también en
  8 direcciones), wall jump/slide/cling, agacharse y gatear, escaleras, cuerdas, ziplines, bordes
  (grab, climb, dangle), nado, vuelo, planeo, jetpack, ground slam, roll, blink, pendientes (sin
  deslizarse quieto), atravesar plataformas (`PlayerDropThrough`) y movimiento top-down.
- **Combate:** melee, hitscan, proyectiles, combos, carga (`WeaponCharge` integrado en `PlayerAttack`),
  inventario de armas con cambio por acción *Special*, apuntado con mouse o stick derecho.
- **Vida:** `CharacterHealth`, knockback, muerte, respawn, vidas y game over (`CharacterLives`),
  persistencia entre escenas, `DamageFlash` y números de daño (`DamagePopupSpawner`).
- **IA:** patrullas, persecución, huida, disparo, `EnemySpawner` y la máquina de estados `AIBrain`.
  `EnemyPathfindingAgent` es experimental: requiere un NavMesh 2D (p. ej. NavMeshPlus).
- **Entorno:** checkpoints, puertas con llave, palancas y placas de presión, plataformas móviles,
  que caen y de un sentido, ascensores, cintas, resortes, viento, agua, zonas de daño, cajas
  empujables, objetos rompibles, teletransportes, coleccionables, `HealthPickup` y `LevelExit`
  (escena por nombre o la siguiente del build).
- **Cámara, managers y UI:** seguimiento, límites (o los del `LevelManager`), shake, zoom por velocidad,
  audio con crossfade, guardado, transiciones con fade automático, HUD, barra de vida y pausa.
- **Animación:** `CharacterAnimatorBridge` envía velocidad, suelo y estados al Animator si los
  parámetros existen.

## Arquitectura

- `CharacterCore` + `AbilityBase`: cada habilidad es un componente independiente. Una habilidad que
  lanza una excepción se desactiva sola sin detener a las demás. `Suspend/Resume` pausa todas
  (muerte, aturdimiento, knockback).
- `CharacterController2D`: suelo sin configuración, gravedad compartida (`OverrideGravity` /
  `ReleaseGravity`) y bloqueo temporal del control horizontal.
- `PhysicsQuery2D`: raycasts y overlaps que ignoran al propio personaje, por eso las capas por defecto
  pueden ser *Everything*.
- `InputCompat`: funciona con el Input System nuevo, el Input Manager clásico o ambos.
- `IDamageable`, `IHealthSource`, `IKeyHolder`, `IInteractable`: contratos entre categorías (armas, vida, puertas,
  inventario, palancas) sin dependencias directas.
- `AIBrain` / `AIState` / `AIActionBase` / `AIDecisionBase`: máquina de estados de IA armable desde el
  Inspector.

## Pruebas

`Tests/Runtime` (play mode) cubre las mecánicas de juego y `Tests/Editor` garantiza que todo
componente se pueda agregar y guardar en escenas y prefabs. Para correrlas en un proyecto que usa el
paquete, agrega en su `Packages/manifest.json`:

```
"testables": ["com.alejocastellanos.gameplaykit"]
```

y ábrelas desde *Window → General → Test Runner*.

## Instalación

Como paquete local (desarrollo), en el `Packages/manifest.json` del proyecto:

```
"com.alejocastellanos.gameplaykit": "file:/ruta/a/com.alejocastellanos.gameplaykit"
```

Desde git, una vez publicado:

```
"com.alejocastellanos.gameplaykit": "https://github.com/<usuario>/<repo>.git#v1.1.0"
```

Requiere Unity 6000.0 o superior.

## Licencia

MIT. Ver [LICENSE.md](LICENSE.md). Historial de cambios en [CHANGELOG.md](CHANGELOG.md).
