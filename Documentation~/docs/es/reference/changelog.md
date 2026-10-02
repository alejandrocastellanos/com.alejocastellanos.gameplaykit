---
title: Historial de cambios
description: Cambios relevantes del paquete Gameplay Kit.
---

# Historial de cambios

Todos los cambios relevantes de este paquete se documentan aquí.
El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el versionado,
[Semantic Versioning](https://semver.org/lang/es/).

## 1.1.0 — 2026-10-02

### Agregado

- Escena demo top-down (**GameplayKit → Create Top-Down Demo Scene**, también en el sample Demo) y opción **Move Vertically** en `EnemyChase` y `EnemyFlee` para enemigos top-down.
- Sitio de documentación (MkDocs Material, inglés y español) en `Documentation~/`, publicado en GitHub Pages por `.github/workflows/docs.yml`, con un clip animado de cada componente.
- Helper `PlayerLocator` y propiedades públicas `Target` en `EnemyChase`, `EnemyFlee` y `EnemyShootOnSight`.

### Corregido

- `CharacterCore` ahora se salta las habilidades con el componente desmarcado en el Inspector (no solo con `AbilityEnabled = false`), y `AIBrain` se salta los componentes de acción y decisión desactivados.
- `PlayerRopeGrab` ahora es un péndulo de verdad: la gravedad lo hace oscilar, el input horizontal empuja en la dirección pulsada, nuevo campo **Swing Damping** (0.25), y conserva el impulso al agarrarse y al soltarse con Salto.
- `PlayerWallCling` ya no le quita la altura al wall jump: no se aferra en el frame del salto ni durante el **Control Lock Time** del wall jump.
- `EnemyChase`, `EnemyFlee` y `EnemyShootOnSight` buscan el objeto con tag `Player` cuando **Target** está vacío (nuevo helper `PlayerLocator`) y exponen una propiedad pública `Target`; `EnemySpawner` ahora también les asigna su objetivo, no solo a `AIBrain`.
- `PauseManager.SetPaused` mantiene sincronizado el `GameManager` (`Playing` ↔ `Paused`, sin pisar `GameOver`), y `GameManager`, `AudioManager` y `SaveLoadSystem` se sueltan de su padre antes de `DontDestroyOnLoad`.
- `CharacterKnockback` ya no empuja con la cantidad de daño: nuevos campos **Force** (8 u/s, independiente del daño y de la masa) y **Upward Lift** (0.35), más la sobrecarga `ApplyKnockback(direction, speed)` para una velocidad explícita.
- **Base Gravity Scale** de `CharacterGravityController` ahora vale 0 por defecto, lo que conserva el **Gravity Scale** del `Rigidbody2D`; solo un valor mayor que 0 lo reemplaza.

## 1.0.0 — 2026-10-02

Primera versión publicada.

### Agregado

- **Núcleo:** `CharacterCore` + `AbilityBase` (habilidades independientes, aisladas si fallan,
  con `Suspend`/`Resume`), `CharacterController2D` (suelo sin configuración, gravedad compartida,
  bloqueo de control horizontal), `CharacterStateMachine`, `ICharacterInput` con
  `KeyboardInputReader` (Input System nuevo, clásico y gamepad), `CharacterAnimatorBridge`, y los
  contratos `IDamageable`, `IHealthSource`, `IKeyHolder` e `IInteractable`.
- **Movimiento:** caminar/correr, salto con coyote time y jump buffer, multi-salto, agacharse,
  gatear, wall jump/slide/cling, dash y dash en 8 direcciones, ground slam, cornisas (grab,
  climb, dangle), escaleras, cuerdas, ziplines, planeo, jetpack, vuelo, nado, pendientes, roll,
  blink, seguir rutas, atravesar plataformas de un sentido y movimiento top-down.
- **Vida y física:** vida con invulnerabilidad, gravedad configurable, daño por caída,
  aturdimiento, knockback, muerte, respawn, vidas con game over, persistencia entre escenas y
  `DamageFlash`.
- **Combate:** melee, proyectiles, hitscan, combos, carga, inventario de armas, apuntado con mouse,
  stick o al objetivo más cercano, objetos dañables y `PlayerAttack` que conecta el input con las
  armas.
- **IA:** comportamientos listos para usar (patrulla, patrulla en límites, persecución, huida,
  disparo a la vista, daño por contacto), `EnemySpawner` y la máquina de estados `AIBrain` con
  acciones y decisiones armables desde el Inspector.
- **Entorno:** plataformas móviles, de un sentido y que caen, ascensor, resorte, placa de presión,
  palanca, puerta con llave, objetos rompibles, caja empujable, checkpoint, teletransporte, cinta,
  zonas de daño, viento y agua, coleccionables, pickups de objetos y de vida, salida de nivel.
- **Cámara:** seguimiento, límites (propios o del `LevelManager`), shake y zoom por velocidad.
- **Managers:** juego, nivel (muerte bajo el vacío, checkpoints), puntaje, inventario, pausa,
  audio con crossfade, guardado y transiciones de escena con fade automático.
- **UI:** barra de vida, números de daño con `DamagePopupSpawner`, menú de pausa y texto de puntaje.
- **Editor:** menú *GameplayKit* para crear Player (plataformas y top-down), Enemy, plataforma,
  cámara 2D, managers, HUD y una escena demo jugable.
- **Sample** "Demo 2D" importable desde el Package Manager.
- **Pruebas:** 88 de play mode sobre las mecánicas y 4 de editor sobre el catálogo de componentes.

### Experimental

- `EnemyPathfindingAgent` requiere un NavMesh 2D externo (por ejemplo NavMeshPlus); sin NavMesh
  no hace nada.
