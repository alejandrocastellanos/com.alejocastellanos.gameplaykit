# Personaje de plataformas

**GameplayKit → Create Player** te da un personaje con casi todas las habilidades de movimiento ya
puestas. Es ideal para probar, pero en tu propio juego normalmente vas a querer solo las mecánicas
que necesita. Esta guía arma un personaje de plataformas a mano, explica cómo encajan las piezas y
muestra cómo ajustar el salto hasta que se sienta bien.

## Ármalo paso a paso

1. **Crea un GameObject vacío** llamado `Player` y ponle el tag **Player**. El tag no hace falta
   para moverse, pero la cámara, el HUD, los checkpoints y los enemigos lo buscan por defecto.
2. **Agrega un `CapsuleCollider2D`** y ajusta su **Size** a más o menos `0.8 × 1.8`. Una cápsula
   pasa por escalones pequeños y esquinas mejor que una caja. Pon tu `SpriteRenderer` aquí o en un
   hijo.
3. **Agrega [CharacterController2D](../components/core/CharacterController2D.md).** Agrega un
   `Rigidbody2D` automáticamente y, en Awake, congela su rotación y le da al collider un material
   sin fricción (si no tiene uno) para que el personaje no se quede pegado a las paredes. Si pones
   **Interpolate** del Rigidbody2D en *Interpolate* y **Collision Detection** en *Continuous* (lo que
   hace Create Player), el movimiento se ve más suave a velocidades altas.
4. **Agrega las habilidades** que quieras, en el orden en que quieres que corran — mira las
   secciones siguientes. Como mínimo: [PlayerWalkRun](../components/movement/PlayerWalkRun.md) y
   [PlayerJump](../components/movement/PlayerJump.md).
5. **Agrega [CharacterCore](../components/core/CharacterCore.md) al final.** En Awake reúne todas
   las habilidades del objeto y de sus hijos y las ejecuta cada frame. Si no hay una fuente de
   input, agrega un [KeyboardInputReader](../components/core/KeyboardInputReader.md) por su cuenta.

Con eso ya tienes un personaje jugable. Para vida, daño y respawn, agrega los componentes que se
describen en [Vida y daño](health-and-damage.md).

!!! tip "Por qué CharacterCore va al final"
    En el Editor, todos los componentes ya existen cuando arranca la escena, así que `CharacterCore`
    encuentra todas las habilidades sin importar dónde esté. El orden igual importa en dos casos: las
    habilidades corren en el orden en que aparecen en el Inspector, y cuando armas un personaje
    **desde código**, `AddComponent<CharacterCore>()` ejecuta su Awake de inmediato — lo que agregues
    después nunca se descubre. Agregarlo al final es el hábito que funciona en los dos casos.

### Cómo corren las habilidades

Cada frame, `CharacterCore` ejecuta cuatro fases — `HandleInput`, `EarlyProcessAbility`,
`ProcessAbility`, `LateProcessAbility` — y cada fase corre para **todas** las habilidades antes de
pasar a la siguiente. Las habilidades nunca se llaman entre sí; comparten el
[CharacterController2D](../components/core/CharacterController2D.md) (velocidad, estado de suelo,
gravedad) y los estados de movimiento y condición del personaje.

- Mientras el personaje está muerto, aturdido o en knockback, esos sistemas llaman a `Suspend` y
  ninguna habilidad corre hasta que llamen a `Resume`.
- Si una habilidad lanza una excepción, solo esa habilidad se desactiva (con un error en la Console
  que la nombra); las demás siguen funcionando.
- Las habilidades que necesitan anular la gravedad (escaleras, nado, agarrarse de cornisas,
  volar…) registran un override en el controlador y lo liberan al terminar, así nunca se pelean por
  `Rigidbody2D.gravityScale`.

El orden importa en las habilidades que ajustan lo que calculó otra: pon
[PlayerCrouch](../components/movement/PlayerCrouch.md) **después** de `PlayerWalkRun` para que su
multiplicador de velocidad se aplique a la velocidad que se acaba de fijar.

## Combina habilidades para lograr una sensación de juego

Algunas habilidades son alternativas: usa **o** `PlayerJump` **o**
[PlayerMultiJump](../components/movement/PlayerMultiJump.md), y **o**
[PlayerWallSlide](../components/movement/PlayerWallSlide.md) **o**
[PlayerWallCling](../components/movement/PlayerWallCling.md). Las dos variantes de pared necesitan
[PlayerWallJump](../components/movement/PlayerWallJump.md) (es el que detecta la pared) y lo
agregan automáticamente.

=== "Clásico"

    Controles precisos y fáciles de leer, al estilo de los plataformas de 8 y 16 bits.

    - [PlayerWalkRun](../components/movement/PlayerWalkRun.md) — mantén ++shift++ para correr.
    - [PlayerJump](../components/movement/PlayerJump.md) — altura variable, coyote time, jump buffer.
    - [CharacterGravityController](../components/health/CharacterGravityController.md) — cae más
      rápido de lo que sube, con una velocidad de caída máxima.
    - [PlayerCrouch](../components/movement/PlayerCrouch.md),
      [PlayerDropThrough](../components/movement/PlayerDropThrough.md),
      [PlayerClimbLadder](../components/movement/PlayerClimbLadder.md).

=== "Metroidvania"

    Un repertorio de movimientos que crece; activa las habilidades a medida que el jugador las
    desbloquea.

    - `PlayerWalkRun`, [PlayerMultiJump](../components/movement/PlayerMultiJump.md) (empieza con
      **Extra Jumps** en `0` y súbelo cuando se desbloquee el doble salto).
    - [PlayerDash](../components/movement/PlayerDash.md), `PlayerWallJump` + `PlayerWallSlide`.
    - [PlayerLedgeGrab](../components/movement/PlayerLedgeGrab.md) +
      [PlayerLedgeClimb](../components/movement/PlayerLedgeClimb.md),
      [PlayerCrouch](../components/movement/PlayerCrouch.md) +
      [PlayerCrawl](../components/movement/PlayerCrawl.md).
    - [PlayerGroundSlam](../components/movement/PlayerGroundSlam.md),
      [PlayerSwim](../components/movement/PlayerSwim.md) y
      [PlayerAttack](../components/combat/PlayerAttack.md) con un
      [WeaponCombo](../components/combat/WeaponCombo.md).

    Para bloquear una habilidad hasta que se desbloquee, pon su `AbilityEnabled` en `false` desde tu
    script de progresión — `CharacterCore` se salta las habilidades desactivadas.

=== "Precisión (tipo Celeste)"

    Movimiento rápido, permisivo y con mucho juego en las paredes.

    - `PlayerWalkRun`, `PlayerJump` (aquí el coyote time y el buffer importan mucho).
    - [PlayerDash8Directions](../components/movement/PlayerDash8Directions.md) — hace dash en la
      dirección que mantienes, en horizontal si no mantienes ninguna.
    - `PlayerWallJump` + [PlayerWallCling](../components/movement/PlayerWallCling.md) — mantén hacia
      la pared para quedarte pegado.
    - `CharacterGravityController` con un **Max Fall Speed** más bajo.

    Los dashes del kit se recargan con un temporizador (**Cooldown**, 0,5 s por defecto), no al
    aterrizar. Si quieres un dash por salto, contrólalo desde tu propia habilidad.

## Ajusta el salto

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerJump.mp4" poster="../../../assets/clips/PlayerJump.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Un toque da un saltito; mantener Jump da la altura completa.</figcaption></figure>

Gran parte de la sensación de un plataformas vive en unos pocos campos de
[PlayerJump](../components/movement/PlayerJump.md):

| Campo | Por defecto | Qué hace |
|---|---|---|
| **Jump Force** | `12` | Velocidad hacia arriba al despegar. |
| **Jump Cut Multiplier** | `0.5` | Cuando sueltas Jump mientras subes, la velocidad vertical se multiplica por esto. Más bajo = toques más cortos. `1` desactiva la altura variable. |
| **Coyote Time** | `0.1` | Segundos después de salir caminando de una cornisa durante los que todavía puedes saltar. |
| **Jump Buffer Time** | `0.12` | Si presionas Jump hasta este tiempo antes de aterrizar, el salto sale al tocar el suelo. |

- **Altura variable.** Soltar el botón antes de tiempo corta el salto. Un salto con buffer cuyo
  botón ya soltaste al momento de aterrizar sale directamente como un saltito.
- **El coyote time** hace que las cornisas perdonen: el salto todavía cuenta un momento después de
  que desaparece el suelo, y el salto lo consume, así que no te puede dar un doble salto gratis.
- **La altura del salto** sin gravedad extra es aproximadamente
  `Jump Force² / (2 × 9.81 × gravity scale)` — unas 7 unidades con los valores por defecto. Baja
  **Jump Force** para un personaje más pequeño, o sube la gravedad.

[PlayerMultiJump](../components/movement/PlayerMultiJump.md) tiene los mismos **Jump Force**,
**Jump Cut Multiplier** y **Jump Buffer Time**, más **Extra Jumps** (`1`) y **Air Jump Force**
(`10`) para los saltos en el aire. **No tiene coyote time**: si sales caminando de una cornisa y
presionas Jump, gastas uno de los saltos en el aire.

### Dale forma a la gravedad

[CharacterGravityController](../components/health/CharacterGravityController.md) cambia la gravedad
mientras el personaje está en el aire: ×2 al caer (**Fall Gravity Multiplier**), ×1,5 al subir sin
mantener Jump (**Low Jump Gravity Multiplier**) y un **Max Fall Speed** de 20. Los multiplicadores se
aplican sobre el **Gravity Scale** del Rigidbody2D, que conserva por defecto; pon **Base Gravity
Scale** por encima de `0` solo si quieres reemplazarlo desde aquí. Caer más rápido de lo que subes es lo que hace que un salto
se sienta ágil en vez de flotante.

!!! warning "Dos fuentes de recorte del salto"
    `PlayerJump`/`PlayerMultiJump` cortan el salto al soltar el botón, y `CharacterGravityController`
    agrega gravedad extra cuando no mantienes Jump. Create Player usa los dos, así que los saltitos
    salen todavía más cortos. Si los toques se sienten demasiado cortos, pon **Jump Cut Multiplier**
    en `1` o **Low Jump Gravity Multiplier** en `1` para que solo uno de los dos le dé forma al
    saltito.

## Paredes

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerWallJump.mp4" poster="../../../assets/clips/PlayerWallJump.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Impulsándose desde una pared con PlayerWallJump.</figcaption></figure>

[PlayerWallJump](../components/movement/PlayerWallJump.md) lanza un rayo desde el collider hacia el
lado al que mira el personaje. En el aire y contra una pared, **Jump** aplica **Wall Jump Force**
(`8, 12`) alejándose de ella e ignora el input horizontal durante **Control Lock Time** (`0.2` s),
para que mantener hacia la pared no anule el impulso. `PlayerWallSlide` limita la caída a **Slide
Speed** (`2`) mientras empujas hacia la pared; `PlayerWallCling` detiene la caída por completo y se suelta
en el frame del salto y durante **Control Lock Time**, así que el wall jump desde el agarre conserva toda
su altura.

## Detección de suelo sin configurar nada

No necesitas una capa "Ground". `CharacterController2D` revisa un círculo pequeño (**Ground Check
Radius** `0.1`) en la base del collider del personaje en cada paso de física:

- **Ground Layers** viene en *Everything*. El chequeo ignora los colliders del propio personaje y
  todos los triggers, así que las escaleras, el agua y los pickups nunca cuentan como suelo.
- **Ground Check** es opcional; asigna un Transform hijo solo si la base de tu collider no está
  donde están los pies.
- Selecciona el personaje en Play mode para ver la sonda como gizmo: verde cuando está en el suelo,
  roja en el aire.

Cualquier collider sólido cuenta como suelo, incluidos los enemigos, así que el jugador puede caer
encima de ellos. Si no quieres eso, pon esos objetos en una capa y quítala de **Ground Layers**. Los
chequeos de pared, de cornisa y las armas siguen la misma regla; por eso todas las máscaras de capas
del kit pueden quedarse en *Everything*.

## Siguientes pasos

- [Input](input.md) — reasigna teclas o controla al personaje desde una IA.
- [Habilidades propias](custom-abilities.md) — escribe tu propio `AbilityBase`.
- [Construir un nivel](building-a-level.md) — plataformas, escaleras, agua y peligros.
