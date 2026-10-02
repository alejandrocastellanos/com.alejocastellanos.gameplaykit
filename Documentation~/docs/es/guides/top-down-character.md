# Personaje top-down

En los juegos vistos desde arriba — action RPGs, twin-stick shooters, dungeon crawlers — el
personaje se mueve libremente en los dos ejes y la gravedad no lo puede arrastrar hacia abajo de la
pantalla. [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md) hace justo eso y
reemplaza a `PlayerWalkRun` y a las habilidades de salto. Todo lo demás del kit (vida, armas,
interacción, cámara) funciona igual que en un plataformas.

## La forma rápida

**GameplayKit → Create Top-Down Player** crea un objeto `Player (Top-Down)`, con tag `Player`, que
tiene:

- un `CircleCollider2D` (radio `0.4`), un `Rigidbody2D` con interpolación y
  [CharacterController2D](../components/core/CharacterController2D.md);
- [KeyboardInputReader](../components/core/KeyboardInputReader.md),
  [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md),
  [PlayerDash8Directions](../components/movement/PlayerDash8Directions.md),
  [PlayerInteract](../components/environment/PlayerInteract.md),
  [PlayerAttack](../components/combat/PlayerAttack.md) y
  [WeaponMelee](../components/combat/WeaponMelee.md);
- [CharacterHealth](../components/health/CharacterHealth.md),
  [CharacterKnockback](../components/health/CharacterKnockback.md),
  [CharacterDeath](../components/health/CharacterDeath.md),
  [CharacterRespawn](../components/health/CharacterRespawn.md),
  [InventoryManager](../components/managers/InventoryManager.md),
  [DamageFlash](../components/health/DamageFlash.md),
  [CharacterAnimatorBridge](../components/core/CharacterAnimatorBridge.md) y, al final,
  [CharacterCore](../components/core/CharacterCore.md).

Para armarlo a mano, agrega los mismos componentes en ese orden. Un círculo (o una cápsula corta) es
el collider habitual en top-down: se desliza por las paredes y rodea las esquinas sin engancharse.

## Movimiento y el override de gravedad

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerTopDownMovement.mp4" poster="../../../assets/clips/PlayerTopDownMovement.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Movimiento libre en ocho direcciones con aceleración suave.</figcaption></figure>

Al inicializarse, `PlayerTopDownMovement` registra un override de gravedad de `0` en el
`CharacterController2D`, y lo vuelve a registrar cada vez que el personaje se reinicia (por ejemplo,
al respawnear). No necesitas tocar el **Gravity Scale** del Rigidbody2D.

| Campo | Por defecto | Qué hace |
|---|---|---|
| **Walk Speed** | `4` | Velocidad con el stick o WASD / flechas. |
| **Run Speed** | `6.5` | Velocidad mientras mantienes ++shift++ (o el gatillo izquierdo). |
| **Acceleration** | `0.05` | Segundos para llegar a la velocidad objetivo (suavizado). `0` = arranca y frena al instante. |
| **Flip With Direction** | activado | Refleja al personaje en X al moverse a la izquierda o a la derecha. |

- El input se limita a longitud 1, así que moverse en diagonal no es más rápido que moverse en línea
  recta.
- Mientras el personaje está aturdido, en knockback o muerto, `CharacterCore` suspende las
  habilidades, así que `PlayerTopDownMovement` no corre: el `Rigidbody2D` conserva la velocidad que
  tenía y, como no hay gravedad ni fricción con el suelo, el personaje sigue deslizándose hasta que
  las habilidades se reanudan (al respawnear, la velocidad vuelve a cero). Si prefieres que se
  detenga, pon su velocidad en cero desde tu propio script, por ejemplo al recibir
  `OnCharacterDeath` de [CharacterDeath](../components/health/CharacterDeath.md).
- `LastMoveDirection` recuerda la última dirección en la que te moviste (empieza en
  `Vector2.down`), lo que viene bien para las animaciones de reposo y para saber hacia dónde mira.

!!! warning "No le pongas habilidades de plataformas a un personaje top-down"
    Los overrides de gravedad se apilan y gana el más reciente. Por ejemplo,
    [PlayerSwim](../components/movement/PlayerSwim.md) registra una gravedad de `0.2` dentro del
    agua, lo que haría que un personaje top-down se deslizara hacia abajo de la pantalla, y
    [PlayerClimbLadder](../components/movement/PlayerClimbLadder.md) escribe su propia velocidad
    vertical. No combines `PlayerTopDownMovement` con `PlayerWalkRun`, las habilidades de salto,
    `CharacterGravityController` ni las habilidades de pared, escalera y nado.

## Dash en 8 direcciones

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerDash8Directions.mp4" poster="../../../assets/clips/PlayerDash8Directions.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Dash en la dirección del input, diagonales incluidas.</figcaption></figure>

[PlayerDash8Directions](../components/movement/PlayerDash8Directions.md) hace un dash en la
dirección del input de movimiento actual cuando presionas ++q++ (o el gatillo derecho): **Dash
Speed** `16` durante **Dash Duration** `0.15` s, y luego un **Cooldown** de `0.5` s. Sin input, hace
el dash en horizontal hacia el lado al que mira el personaje — no según `LastMoveDirection`.

## Interactuar y atacar

[PlayerInteract](../components/environment/PlayerInteract.md) funciona igual en las dos vistas:
presiona ++e++ y se activa el `IInteractable` más cercano dentro de **Interact Radius** (`1.2`) —
palancas o cualquier script tuyo que implemente la interfaz. Los triggers cuentan, así que los
objetos interactuables no necesitan un collider sólido.

[PlayerAttack](../components/combat/PlayerAttack.md) con [WeaponMelee](../components/combat/WeaponMelee.md)
ataca con ++j++. Por defecto, la hitbox cuerpo a cuerpo se coloca en **Hitbox Offset** `(0.75, 0)`,
reflejada según hacia dónde mira el personaje, así que solo alcanza **a la izquierda o a la
derecha**. Para un ataque en cuatro u ocho direcciones, dale al arma un hijo como **Hitbox Origin**
y muévelo según la última dirección de movimiento:

```csharp
using GameplayKit.Movement;
using UnityEngine;

// Pon esto en el jugador. Asigna el mismo Transform hijo que usas como Hitbox Origin de WeaponMelee.
public class HitboxFollowsMovement : MonoBehaviour
{
    [SerializeField] private PlayerTopDownMovement movement;
    [SerializeField] private Transform hitboxOrigin;
    [SerializeField] private float distance = 0.75f;

    private void LateUpdate()
    {
        Vector2 direction = movement.LastMoveDirection;
        // El personaje se refleja con una escala X negativa, así que lo compensamos en la posición local.
        float facing = Mathf.Sign(transform.lossyScale.x);
        hitboxOrigin.localPosition = new Vector2(direction.x * facing, direction.y) * distance;
    }
}
```

Para combate a distancia, usa [WeaponProjectile](../components/combat/WeaponProjectile.md) o
[WeaponHitscan](../components/combat/WeaponHitscan.md) junto con
[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md): en modo **Mouse** los
disparos van hacia el cursor, en modo **Stick** hacia el stick derecho y en modo **Closest Target**
hacia el objeto dañable más cercano. Es un twin-stick shooter sin escribir código. Mira
[Combate](combat.md).

## Configura la cámara

**GameplayKit → Create 2D Camera** crea una cámara ortográfica (tamaño `6`) con
[CameraFollow](../components/camera/CameraFollow.md) y
[CameraShake](../components/camera/CameraShake.md). Con su **Target** vacío, `CameraFollow` busca el
objeto con tag `Player` — el jugador top-down lo tiene — y salta hasta él en el primer frame; luego
lo sigue con **Smooth Time** `0.2` y **Offset** `(0, 0, -10)`.

- Para mantener la cámara dentro del mapa, agrega [CameraBounds](../components/camera/CameraBounds.md)
  y pon en **Min Bounds** / **Max Bounds** las esquinas de tu nivel. Si los dejas vacíos, usa los
  límites de un [LevelManager](../components/managers/LevelManager.md) de la escena, si hay uno. Si
  el nivel es más chico que la vista en un eje, la cámara se centra en ese eje.
- Las vistas top-down suelen verse mejor con un **Size** un poco mayor en la Camera (se ve más del
  mapa) y un **Smooth Time** más bajo, para que los dashes rápidos no dejen al jugador cerca del
  borde de la pantalla.
- [CameraZoomBySpeed](../components/camera/CameraZoomBySpeed.md) aleja la cámara a medida que el
  jugador se mueve más rápido. Toma el control del **Size** de la cámara, entre su **Min Zoom**
  (`5`) y su **Max Zoom** (`8`).

## Siguientes pasos

- [Input](input.md) — soporte para gamepad y fuentes de input propias.
- [Combate](combat.md) — armas, apuntado y daño.
- [Enemigos e IA](enemies-and-ai.md) — enemigos que persiguen y disparan.
