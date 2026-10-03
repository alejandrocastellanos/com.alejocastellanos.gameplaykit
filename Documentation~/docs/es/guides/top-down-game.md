---
title: Juego top-down
description: Empieza un juego top-down con Gameplay Kit — el dungeon demo, los controles y tu primera sala en cinco minutos.
---

# Juego top-down

Un juego top-down se ve desde arriba y no tiene gravedad: el personaje se mueve libremente en ocho
direcciones, apunta con el mouse o con el stick y pelea contra enemigos que lo persiguen por todo el
mapa — action RPGs, twin-stick shooters, dungeon crawlers. Esta página es el punto de partida de ese
estilo: abre la demo, aprende los controles y arma tu primera sala desde cero. Si todavía no instalaste
el paquete, empieza por [Primeros pasos](getting-started.md).

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/TopDownShooter.mp4" poster="../../../assets/clips/TopDownShooter.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Apuntando en 360° y disparando a perseguidores que se mueven en los dos ejes mientras una torreta responde; al final, cambio a la espada con K.</figcaption></figure>

## El dungeon demo

**GameplayKit → Create Top-Down Demo Scene** (en la barra de menús principal) arma y guarda un dungeon
de tres salas en `Assets/GameplayKitDemo/GameplayKitTopDownDemo.unity`, con los prefabs `TopDownPlayer`,
`TopDownChaser`, `TopDownTurret`, `TopDownBullet` y `TopDownPlayerBullet` en
`Assets/GameplayKitDemo/Prefabs/`. Antes te pide guardar la escena actual, y nunca toca la demo de
plataformas. Ábrela y dale Play.

<figure markdown>
  ![Mapa del dungeon demo top-down](../../assets/images/topdown-demo-map.jpg)
  <figcaption>El dungeon demo visto completo: sala de inicio (izquierda), pasillo con pinchos, arena (derecha) y sala del tesoro detrás de la puerta con llave (arriba).</figcaption>
</figure>

| Zona | Qué hay | Componentes |
|---|---|---|
| Sala de inicio (izquierda) | Aquí empieza el jugador. Cuatro cajas que se rompen a disparos o espadazos, monedas, un corazón y un teletransporte (abajo a la izquierda) que lleva a la sala del tesoro. | [BreakableObject](../components/environment/BreakableObject.md), [Collectible](../components/environment/Collectible.md), [HealthPickup](../components/environment/HealthPickup.md), [Teleporter](../components/environment/Teleporter.md) |
| Pasillo | Una franja de pinchos que ocupa todo el ancho y, al final, el checkpoint. | [HazardZone](../components/environment/HazardZone.md), [Checkpoint](../components/environment/Checkpoint.md) |
| Arena (derecha) | Dos enemigos que te persiguen, una torreta que dispara en cualquier dirección, una patrulla que va y viene por la parte de abajo, dos pilares para cubrirte, monedas y la llave (abajo a la derecha). | [EnemyChase](../components/ai/EnemyChase.md), [EnemyShootOnSight](../components/ai/EnemyShootOnSight.md), [EnemyPatrolWithinBounds](../components/ai/EnemyPatrolWithinBounds.md), [ItemPickup](../components/environment/ItemPickup.md) |
| Puerta (arriba de la arena) | Se abre si llevas la llave. | [DoorWithKey](../components/environment/DoorWithKey.md) |
| Sala del tesoro (arriba) | Una palanca (++e++) cuyo evento desactiva una reja; detrás, la meta (un coleccionable de 100 puntos) y dos monedas. Un teletransporte te devuelve a la sala de inicio. | [Lever](../components/environment/Lever.md), `Collectible`, `Teleporter` |

El resto de la escena:

- **TopDownPlayer**: el personaje de **Create Top-Down Player** con dos armas en un
  [WeaponInventorySlot](../components/combat/WeaponInventorySlot.md) — un hijo `Gun` con
  [WeaponProjectile](../components/combat/WeaponProjectile.md) y un hijo `Sword` con
  [WeaponMelee](../components/combat/WeaponMelee.md) — y
  [CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) en modo **Mouse**, que gira la
  pistola hacia el cursor. Cómo armarlo está en
  [Personaje top-down](top-down-character.md#disparar-hacia-el-mouse).
- **Main Camera**: tamaño ortográfico 7, con [CameraFollow](../components/camera/CameraFollow.md),
  [CameraShake](../components/camera/CameraShake.md) y [CameraBounds](../components/camera/CameraBounds.md)
  limitada al dungeon, de (−11, −9) a (44, 20).
- **Managers** y **HUD**, igual que en la demo de plataformas.

Igual que en la demo de plataformas, la meta es un `Collectible` y no un `LevelExit`, así que la demo
nunca carga otra escena.

## Controles

| Acción | Teclado y mouse | Gamepad (Input System) |
|---|---|---|
| Moverse (8 direcciones) | ++w++ ++a++ ++s++ ++d++ o flechas | Stick izquierdo |
| Correr (mantener) | ++shift++ izquierdo | Gatillo izquierdo |
| Dash en 8 direcciones | ++q++ | Gatillo derecho |
| Apuntar | Mouse (con **CharacterAimAndOrient** en modo **Mouse**) | Stick derecho (modo **Stick**) |
| Atacar / disparar | ++j++ — en la demo también clic izquierdo | Botón oeste (X / Cuadrado) |
| Cambiar de arma | ++k++ | Bumper derecho |
| Interactuar | ++e++ | Select / View |
| Pausa | ++esc++ | Start |

Salto, agacharse y las demás teclas de plataformas no hacen nada en un personaje top-down. El clic
izquierdo no viene asignado por defecto: la demo lo agrega como un binding extra de **Attack** (`Mouse0`)
en su [KeyboardInputReader](../components/core/KeyboardInputReader.md), y tú puedes hacer lo mismo.

## Tu primera sala en cinco minutos

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerTopDownMovement.mp4" poster="../../../assets/clips/PlayerTopDownMovement.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Movimiento libre en ocho direcciones con aceleración suave.</figcaption></figure>

1. Crea una escena nueva y borra su **Main Camera** por defecto (la cámara del kit la reemplaza).
2. **GameplayKit → Create Top-Down Player**.
3. **GameplayKit → Create 2D Camera**, **Create Managers** y **Create HUD**.
4. Con el jugador cerca de (0, 0), cierra una sala con cuatro **GameplayKit → Create Platform**: dos con **Scale** X `5` arriba y
   abajo del jugador (por ejemplo en y = 5 y y = −5), y dos con **Rotation** Z `90` y **Scale** X `3`
   a los costados (x = 10 y x = −10). En top-down, una plataforma es simplemente una pared.
5. Dale Play. Muévete en ocho direcciones con ++w++ ++a++ ++s++ ++d++, corre con ++shift++ y haz dash
   con ++q++ en cualquier dirección.
6. Agrega un enemigo con **GameplayKit → Create Top-Down Enemy** dentro de la sala, a unas 5 unidades
   del jugador. Te persigue cuando estás a menos de 7 unidades y te daña al tocarte. Pégale con ++j++
   tres veces para destruirlo (tiene 30 de vida y el ataque por defecto hace 10). El golpe sale hacia
   la izquierda o la derecha, según hacia dónde mira el personaje.

No hizo falta ninguna capa, tag ni referencia, ni tocar el **Gravity Scale**:
[PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md) anula la gravedad del
personaje por su cuenta, y el enemigo top-down ya viene con gravedad `0`.

Para que dispare hacia el mouse como en la demo, sigue
[Disparar hacia el mouse](top-down-character.md#disparar-hacia-el-mouse), o arrastra a la escena el
prefab `TopDownPlayer` de la demo en lugar del paso 2.

## Siguientes pasos

- [Personaje top-down](top-down-character.md) — movimiento, dash, ataques en cuatro direcciones, disparar hacia el mouse y la cámara.
- [Enemigos e IA](enemies-and-ai.md) — perseguidores, torretas, patrullas y máquinas de estado.
- [Construir un nivel](building-a-level.md) — managers, piezas de entorno, checkpoints, puertas y HUD.
- [Combate](combat.md) y [Vida y daño](health-and-damage.md) — armas, apuntado, muerte y respawn.
