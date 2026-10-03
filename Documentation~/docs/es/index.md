---
title: Gameplay Kit
description: Gameplay 2D modular para Unity 6, armado desde Add Component.
hide:
  - navigation
  - toc
---

<div class="gk-hero" markdown>

<div markdown>

# Gameplay Kit

Gameplay 2D modular para Unity 6: movimiento, combate, vida, IA de enemigos, niveles interactivos, cámara,
managers y UI. Cada pieza es un componente que agregas a un GameObject y funciona con sus valores por
defecto, sin configurar capas, tags ni referencias antes.

[Empezar](guides/getting-started.md){ .md-button .md-button--primary }
[Ver componentes](components/index.md){ .md-button }

</div>

<div markdown="0"><video src="../assets/clips/Showcase.mp4" poster="../assets/clips/Showcase.jpg" autoplay loop muted playsinline preload="metadata"></video></div>

</div>

## Qué incluye

113 componentes en nueve categorías. Cada uno tiene su propia página con un clip, los pasos para usarlo y la
referencia completa del Inspector.

<div class="grid cards" markdown>

-   :material-cog:{ .lg .middle } __Núcleo__

    ---

    `CharacterCore`, `AbilityBase`, `CharacterController2D`, el lector de teclado y gamepad y el puente al
    Animator: la base sobre la que se arma cualquier personaje.

    [:octicons-arrow-right-24: Núcleo](components/core/index.md)

-   :material-run-fast:{ .lg .middle } __Movimiento__

    ---

    Caminar y correr, salto con coyote time y jump buffer, multi-salto, dashes, paredes, cornisas,
    escaleras, cuerdas, tirolesas, nado, vuelo, planeo, jetpack, pendientes y movimiento top-down.

    [:octicons-arrow-right-24: Movimiento](components/movement/index.md)

-   :material-heart-pulse:{ .lg .middle } __Vida y física__

    ---

    Vida con invulnerabilidad, knockback, aturdimiento, muerte, respawn, vidas con game over, daño por
    caída, control de la gravedad, persistencia entre escenas y parpadeo al recibir daño.

    [:octicons-arrow-right-24: Vida y física](components/health/index.md)

-   :material-sword:{ .lg .middle } __Combate__

    ---

    Armas melee, hitscan y de proyectiles, combos, ataques cargados, inventario de armas, apuntado con mouse o
    stick y objetos dañables, todo conectado por `PlayerAttack`.

    [:octicons-arrow-right-24: Combate](components/combat/index.md)

-   :material-robot:{ .lg .middle } __IA y enemigos__

    ---

    Comportamientos listos (patrullar, perseguir, huir, disparar a la vista, daño por contacto), un
    `EnemySpawner` y la máquina de estados `AIBrain`, que armas desde el Inspector.

    [:octicons-arrow-right-24: IA y enemigos](components/ai/index.md)

-   :material-terrain:{ .lg .middle } __Entorno__

    ---

    Plataformas móviles, que caen y de un sentido, ascensores, palancas, placas de presión, llaves y puertas,
    zonas de daño, agua, viento, pickups, checkpoints y salidas de nivel.

    [:octicons-arrow-right-24: Entorno](components/environment/index.md)

-   :material-video:{ .lg .middle } __Cámara__

    ---

    Seguimiento suave, límites del nivel, sacudidas y zoom según la velocidad para cámaras 2D ortográficas.

    [:octicons-arrow-right-24: Cámara](components/camera/index.md)

-   :material-tune:{ .lg .middle } __Managers__

    ---

    Estado del juego, nivel (vacío y checkpoints), puntaje, inventario, pausa, audio con crossfade,
    guardado y transiciones de escena con fundido.

    [:octicons-arrow-right-24: Managers](components/managers/index.md)

-   :material-monitor-dashboard:{ .lg .middle } __UI__

    ---

    Barra de vida, texto de puntaje, menú de pausa y números de daño flotantes que se conectan solos con los
    managers.

    [:octicons-arrow-right-24: UI](components/ui/index.md)

</div>

## Instalación

Gameplay Kit necesita **Unity 6000.0** o superior. Agrégalo al `Packages/manifest.json` de tu proyecto. Como
paquete local:

```json
"com.alejocastellanos.gameplaykit": "file:/ruta/a/com.alejocastellanos.gameplaykit"
```

O desde git:

```json
"com.alejocastellanos.gameplaykit": "https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0"
```

El paquete Input System es opcional: el kit lo usa si está instalado y activo, y si no, usa el Input Manager
clásico. Después abre **GameplayKit → Create Demo Scene** y dale Play.

## Cómo funciona

Un personaje es simplemente un GameObject con un collider 2D, un `CharacterController2D` y las habilidades que
quieras. `CharacterCore` encuentra todas las habilidades del objeto y de sus hijos y las ejecuta cada frame,
así que sumar o quitar una mecánica es solo Add Component o Remove Component.

```text
Player  (tag: Player)
├─ CapsuleCollider2D
├─ Rigidbody2D             ← lo agrega solo CharacterController2D
├─ CharacterController2D   ← detección de suelo, gravedad compartida, ayudas de velocidad
├─ PlayerWalkRun           ┐
├─ PlayerJump              │ habilidades (AbilityBase)
├─ PlayerDash              ┘
└─ CharacterCore           ← encuentra las habilidades y las ejecuta cada frame
```

Si nada en el personaje aporta el input, `CharacterCore` agrega solo un `KeyboardInputReader`. Los controles
por defecto son A/D o flechas para moverte, Espacio para saltar, Shift para correr, Ctrl para agacharte, Q
para el dash, J para atacar, K para la acción especial, E para interactuar y Esc para pausar. Con gamepad:
stick izquierdo, A/Cruz para saltar y X/Cuadrado para atacar.

Las categorías se comunican con interfaces pequeñas (`IDamageable`, `IHealthSource`, `IKeyHolder`,
`IInteractable`). Un arma puede dañar cualquier cosa que reciba daño y una puerta puede consultar cualquier
inventario, sin conocer las clases concretas. En [Arquitectura](reference/architecture.md) está el panorama
completo.

## Por dónde seguir

- [Primeros pasos](guides/getting-started.md): instala el paquete, abre la demo y arma tu primer personaje.
- [Personaje de plataformas](guides/platformer-character.md) y [Personaje top-down](guides/top-down-character.md): elige las habilidades de tu juego.
- [Combate](guides/combat.md), [Vida y daño](guides/health-and-damage.md) e [Enemigos e IA](guides/enemies-and-ai.md): las peleas desde los dos lados.
- [Construir un nivel](guides/building-a-level.md): managers, cámara, checkpoints, puertas y HUD.
- [Habilidades propias](guides/custom-abilities.md): escribe tu propia mecánica sobre `AbilityBase`.
- [Solución de problemas](guides/troubleshooting.md): respuestas a los problemas de configuración más comunes.
