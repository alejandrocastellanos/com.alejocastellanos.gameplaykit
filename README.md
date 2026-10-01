# Gameplay Kit

Kit modular de gameplay 2D para Unity 6: movimiento de personaje, combate, vida, IA de enemigos,
entorno interactivo, cámara, managers y UI. Todo se arma por componentes desde **Add Component**,
sin escribir código para combinarlos, y funciona con valores por defecto (capas, tags y referencias
opcionales).

## Empezar en un minuto

- **GameplayKit → Create Demo Scene** genera un nivel jugable que recorre las mecánicas principales.
  También está como sample: *Package Manager → Gameplay Kit → Samples → Demo 2D → Import*.
- **GameplayKit → Create Player / Create Enemy / Create Platform / Create 2D Camera / Create Managers / Create HUD**
  crean objetos listos para usar (también con clic derecho en la jerarquía → *GameplayKit*).

Controles por defecto: A/D o flechas para moverse, Espacio saltar, Shift correr, Ctrl agacharse,
Q dash, J atacar, E interactuar, Esc pausa. Con gamepad: stick izquierdo, A/Cruz saltar, X/Cuadrado
atacar. Las acciones (atacar, especial, volar, rodar, blink) se reasignan en `KeyboardInputReader`.

## Cómo se arma un personaje

1. Un GameObject con un `Collider2D` (capsule recomendado) y `CharacterController2D`.
2. Las habilidades que quieras: `PlayerWalkRun`, `PlayerJump` o `PlayerMultiJump`, `PlayerDash`,
   `PlayerWallJump`, `PlayerCrouch`, `PlayerClimbLadder`, `PlayerSwim`, `PlayerAttack`, etc.
3. `CharacterCore`, que descubre las habilidades y las ejecuta cada frame. Si no hay una fuente de
   input, agrega `KeyboardInputReader` sola.

Para controlarlo con otra cosa (IA, replays, PlayerInput con acciones) implementa `ICharacterInput`.

## Arquitectura

- `CharacterCore` + `AbilityBase`: cada habilidad es un componente independiente. Una habilidad que
  lanza una excepción se desactiva sola sin detener a las demás. `Suspend/Resume` pausa todas
  (muerte, aturdimiento, knockback).
- `CharacterController2D`: suelo sin configuración, gravedad compartida (`OverrideGravity` /
  `ReleaseGravity`) y bloqueo temporal del control horizontal.
- `PhysicsQuery2D`: raycasts y overlaps que ignoran al propio personaje, por eso las capas por defecto
  pueden ser *Everything*.
- `InputCompat`: funciona con el Input System nuevo, el Input Manager clásico o ambos.
- `IDamageable`, `IKeyHolder`, `IInteractable`: contratos entre categorías (armas, vida, puertas,
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
"com.alejocastellanos.gameplaykit": "https://github.com/<usuario>/<repo>.git#v0.1.0"
```

Requiere Unity 6000.0 o superior.
