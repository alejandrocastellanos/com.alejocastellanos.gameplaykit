---
title: Juego de plataformas
description: Empieza un juego de plataformas con Gameplay Kit — la demo, los controles y tu primer nivel en cinco minutos.
---

# Juego de plataformas

Un juego de plataformas se ve de costado y tiene gravedad: el personaje corre, salta, se cuelga de
paredes, sube escaleras y nada. Esta página es el punto de partida de ese estilo: abre la demo,
aprende los controles y arma tu primer nivel desde cero. Si todavía no instalaste el paquete, empieza
por [Primeros pasos](getting-started.md).

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/Showcase.mp4" poster="../../../assets/clips/Showcase.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Un recorrido por el nivel demo: plataformas, un enemigo, pinchos, checkpoint, escalera, llave, agua, cinta, un elevador con palanca y la puerta con llave.</figcaption></figure>

## La demo de plataformas

**GameplayKit → Create Demo Scene** (en la barra de menús principal) arma y guarda un nivel corto en
`Assets/GameplayKitDemo/GameplayKitDemo.unity` (con un número si ya existe), y los prefabs `Player` y
`Enemy` en `Assets/GameplayKitDemo/Prefabs/`. Antes te pide guardar la escena actual. Ábrela y dale
Play. Todo sale de los mismos constructores que el resto del menú **GameplayKit**, así que es una buena
referencia de cómo conectar las piezas.

El nivel se lee de izquierda a derecha (cara superior del suelo en y = 0 salvo que se indique otra cosa):

| x | Qué hay | Componentes |
|---|---|---|
| −10.5 | Una pared alta cierra el lado izquierdo. | `BoxCollider2D` simple |
| 0 | Aquí empieza el jugador; dos monedas más adelante. | Prefab del jugador, [Collectible](../components/environment/Collectible.md) |
| 6 | Una plataforma delgada que se atraviesa desde abajo, con una moneda encima. | [OneWayPlatform](../components/environment/OneWayPlatform.md) |
| 9 | Una caja que se rompe de un golpe (**J**). | [BreakableObject](../components/environment/BreakableObject.md) |
| 12.5 – 19.5 | Dos muros bajos con un enemigo patrullando entre ellos. | Prefab del enemigo ([EnemyPatrol](../components/ai/EnemyPatrol.md), daño por contacto) |
| 20 – 28 | Un hueco: una plataforma va 5 unidades a la derecha y vuelve, con pinchos abajo. | [MovingPlatform](../components/environment/MovingPlatform.md) (ping-pong), [HazardZone](../components/environment/HazardZone.md) |
| 28.8 | Un corazón para recuperar lo que quitaron los pinchos. | [HealthPickup](../components/environment/HealthPickup.md) |
| 30 | El checkpoint. | [Checkpoint](../components/environment/Checkpoint.md) |
| 33.4 | Una escalera hasta una torre (cima en y = 4) con una moneda y la llave en x = 42. | [LadderZone](../components/movement/LadderZone.md), [ItemPickup](../components/environment/ItemPickup.md) |
| 44 – 52 | Una piscina de dos unidades de profundidad. | [WaterZone](../components/environment/WaterZone.md) |
| 56 | Una cinta transportadora que empuja a la derecha. | [ConveyorBelt](../components/environment/ConveyorBelt.md) |
| 60 – 63 | Una palanca (**E**) cuyo evento **On Toggled** llama `Elevator.Activate`; el elevador sube 6 unidades. | [Lever](../components/environment/Lever.md), [Elevator](../components/environment/Elevator.md) |
| 65 – 75, y = 6 | El piso superior: una puerta con llave en x = 70 y la meta, un coleccionable de 100 puntos. | [DoorWithKey](../components/environment/DoorWithKey.md), `Collectible` |
| debajo de todo | Un trigger invisible de 120 unidades de ancho en y = −14 que hace 9999 de daño. | `HazardZone` |

El resto de la escena:

- **Main Camera**: tamaño ortográfico 6, con [CameraFollow](../components/camera/CameraFollow.md) sobre el
  jugador, [CameraShake](../components/camera/CameraShake.md) y [CameraBounds](../components/camera/CameraBounds.md)
  limitada a (−11, −12)–(80, 20).
- **Managers**: [ScoreManager](../components/managers/ScoreManager.md) y [PauseManager](../components/managers/PauseManager.md).
- **HUD**: barra de vida, puntaje y menú de pausa, más un `EventSystem`.

Hay dos detalles que vale la pena notar. Caerse del nivel no necesita un `LevelManager`: la zona de muerte es
simplemente una `HazardZone` con daño suficiente, y el `CharacterRespawn` del jugador lo devuelve al último
checkpoint. Y la meta es un `Collectible`, no un `LevelExit`, así que la demo nunca carga otra escena.

## Controles

| Acción | Teclado | Gamepad (Input System) |
|---|---|---|
| Moverse | ++a++ / ++d++ o ++arrow-left++ / ++arrow-right++ | Stick izquierdo |
| Arriba / abajo (escaleras, nado, vuelo) | ++w++ / ++s++ o ++arrow-up++ / ++arrow-down++ | Stick izquierdo |
| Saltar | ++space++ | Botón sur (A / Cruz) |
| Correr (mantener) | ++shift++ izquierdo | Gatillo izquierdo |
| Agacharse (mantener) | ++ctrl++ izquierdo | — |
| Dash | ++q++ | Gatillo derecho |
| Interactuar | ++e++ | Select / View |
| Atacar | ++j++ | Botón oeste (X / Cuadrado) |
| Especial (cambiar de arma) | ++k++ | Bumper derecho |
| Volar (activar/desactivar) | ++f++ | Botón norte (Y / Triángulo) |
| Rodar | ++alt++ izquierdo | Botón este (B / Círculo) |
| Blink | ++c++ | Bumper izquierdo |
| Pausa | ++esc++ | Start |

Algunas combinaciones: ++s++ + ++space++ sobre una plataforma de un sentido la atraviesa hacia
abajo, y ++s++ + ++q++ en el aire hace un ground slam (si el personaje tiene
[PlayerGroundSlam](../components/movement/PlayerGroundSlam.md)). Volar, rodar y blink solo funcionan
si agregas esas habilidades. Las cinco acciones (Attack, Special, Fly, Roll, Blink) se reasignan en
[KeyboardInputReader](../components/core/KeyboardInputReader.md).

## Tu primer nivel en cinco minutos

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerWalkRun.mp4" poster="../../../assets/clips/PlayerWalkRun.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Caminar y luego mantener Shift para correr: lo primero que vas a probar después de Create Player.</figcaption></figure>

1. Crea una escena nueva y borra su **Main Camera** por defecto (la cámara del kit la reemplaza).
2. **GameplayKit → Create Platform**. Escálala en X (por ejemplo a 8) para tener un piso más ancho.
3. Mueve la vista Scene arriba de la plataforma y usa **GameplayKit → Create Player**.
4. **GameplayKit → Create 2D Camera**, **Create Managers** y **Create HUD**.
5. Dale Play. Camina, corre con ++shift++, haz doble salto, dash con ++q++ y salta contra el costado
   de la plataforma para deslizarte por la pared y hacer wall jump.
6. Agrega un enemigo con **GameplayKit → Create Enemy** sobre la plataforma. Patrulla y te daña al
   tocarte; pégale dos veces con ++j++ para destruirlo (tiene 20 de vida y el ataque por defecto hace 10).

No hizo falta ninguna capa, tag ni referencia: la detección de suelo, los chequeos de pared y las
armas ignoran los colliders del propio personaje, así que todas las máscaras pueden quedar en
*Everything*.

Ahora cambia algo. Selecciona el Player y pon **Extra Jumps** de
[PlayerMultiJump](../components/movement/PlayerMultiJump.md) en `2` para un triple salto, o quita
[PlayerDash](../components/movement/PlayerDash.md) para eliminar el dash. Cada habilidad es su
propio componente, así que agregar o quitar una mecánica es solo **Add Component** / **Remove
Component**.

## Siguientes pasos

- [Personaje de plataformas](platformer-character.md) — arma un personaje a mano, combina habilidades y ajusta el salto.
- [Construir un nivel](building-a-level.md) — managers, cámara, piezas de entorno, checkpoints, puertas y HUD.
- [Combate](combat.md), [Vida y daño](health-and-damage.md) y [Enemigos e IA](enemies-and-ai.md) — las peleas.
