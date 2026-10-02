# Primeros pasos

Gameplay Kit es un paquete de Unity con componentes de gameplay 2D pequeños e independientes. Armas
personajes, enemigos y niveles apilando componentes desde **Add Component**, y todo funciona con
sus valores por defecto: no hay capas, tags ni referencias que configurar antes de darle Play. En
esta página instalas el paquete y tienes un personaje jugable en pantalla en pocos minutos.

## Requisitos

- **Unity 6** (6000.0 o superior).
- Física 2D y uGUI. Están declarados como dependencias del paquete, así que Unity los activa solo.
- El paquete **Input System** es opcional. El kit funciona con *Input System Package (New)*,
  *Input Manager (Old)* o *Both* — mira [Input](input.md).

## Instala el paquete

=== "URL de git"

    Abre **Window → Package Manager**, haz clic en **+ → Add package from git URL…** y pega la URL
    del repositorio con la etiqueta de versión:

    ```text
    https://github.com/<user>/<repo>.git#v1.1.0
    ```

    O agrégalo directamente en el `Packages/manifest.json` de tu proyecto:

    ```json
    "com.alejocastellanos.gameplaykit": "https://github.com/<user>/<repo>.git#v1.1.0"
    ```

    Reemplaza `<user>/<repo>` por el repositorio desde el que instalas. El sufijo `#v1.1.0` fija la
    versión, así tu proyecto no cambia cuando llegan commits nuevos.

=== "Carpeta local"

    Clona o descarga el paquete en tu disco y luego usa **+ → Add package from disk…** en el
    Package Manager y elige su `package.json`, o referencia la carpeta en `Packages/manifest.json`:

    ```json
    "com.alejocastellanos.gameplaykit": "file:/ruta/a/com.alejocastellanos.gameplaykit"
    ```

    Es la mejor opción si quieres modificar el paquete mientras lo usas.

## Prueba la demo

La forma más rápida de ver lo que hace el kit es el nivel demo. Recorre las mecánicas principales:
plataformas, una plataforma de un sentido, un enemigo, pinchos, un checkpoint, una escalera, una
llave y una puerta cerrada, agua, una cinta transportadora, una palanca con un elevador, un pickup
de vida, el HUD y el menú de pausa.

- **GameplayKit → Create Demo Scene** arma el nivel y lo guarda como
  `Assets/GameplayKitDemo/GameplayKitDemo.unity`, con los prefabs `Player` y `Enemy` en
  `Assets/GameplayKitDemo/Prefabs`. Antes te pide guardar la escena actual.
- **GameplayKit → Create Top-Down Demo Scene** arma un pequeño dungeon visto desde arriba y lo guarda
  como `Assets/GameplayKitDemo/GameplayKitTopDownDemo.unity` (prefabs `TopDownPlayer`, `TopDownChaser`,
  `TopDownTurret`, `TopDownBullet` y `TopDownPlayerBullet`). Nunca toca la escena de plataformas: movimiento y
  dash en 8 direcciones, una pistola que apunta con el mouse (dispara con clic izquierdo o ++j++) más una espada (se cambian con ++k++), enemigos que persiguen, una torreta, una patrulla, pinchos, cajas rompibles, llave y puerta,
  una palanca que abre una reja, teletransportes, checkpoint, HUD y menú de pausa.
- El mismo nivel aparece como sample: **Package Manager → Gameplay Kit → Samples → Demo 2D →
  Import**.

Abre la escena y dale Play.

## El menú GameplayKit

El menú **GameplayKit** de la barra de menús crea objetos listos para usar. Cada uno aparece en el
centro de la vista Scene, queda seleccionado y se puede deshacer con ++ctrl+z++.

| Opción del menú | Qué crea |
|---|---|
| **Create Player** | Un personaje de plataformas completo con tag `Player`: collider de cápsula, [CharacterController2D](../components/core/CharacterController2D.md), caminar/correr, doble salto, dash, wall jump y wall slide, agacharse, escaleras, nado, atravesar plataformas, pendientes, interactuar, ataque cuerpo a cuerpo, vida, knockback, muerte, respawn, inventario, parpadeo de daño, puente al Animator y [CharacterCore](../components/core/CharacterCore.md). |
| **Create Top-Down Player** | Un personaje para juegos vistos desde arriba: collider circular, [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md), dash en 8 direcciones, interactuar, ataque cuerpo a cuerpo, vida, knockback, muerte y respawn. Mira [Personaje top-down](top-down-character.md). |
| **Create Enemy** | Una caja roja que patrulla (se da vuelta en paredes y bordes), daña al jugador al tocarlo y se destruye tras recibir suficiente daño, con parpadeo y números de daño. |
| **Create Platform** | Un bloque sólido de 4 × 0,5 con `BoxCollider2D`. |
| **Create 2D Camera** | Una `Main Camera` ortográfica (tamaño 6) con [CameraFollow](../components/camera/CameraFollow.md) y [CameraShake](../components/camera/CameraShake.md). Sigue a lo que tenga el tag `Player`. |
| **Create Managers** | Un objeto `Managers` con [ScoreManager](../components/managers/ScoreManager.md) y [PauseManager](../components/managers/PauseManager.md). |
| **Create HUD** | Un canvas con barra de vida, texto de puntaje y menú de pausa (reanudar / reiniciar), más un `EventSystem` si la escena no tiene uno. |
| **Create Demo Scene** | El nivel demo de plataformas descrito arriba. |
| **Create Top-Down Demo Scene** | El dungeon demo top-down descrito arriba. |

**Player** y **Enemy** también están en el menú contextual de la jerarquía (clic derecho →
**GameplayKit**), que los crea como hijos del objeto donde hiciste clic.

!!! tip "Textos del HUD"
    El HUD se crea con textos de ejemplo en español ("Puntos: 0", "Pausa", "Reanudar",
    "Reiniciar"). Si tu juego está en otro idioma, cambia la etiqueta del puntaje con el campo
    **Format** de [UIScoreText](../components/ui/UIScoreText.md) y edita los textos de los botones
    directamente en la jerarquía.

## Controles por defecto

| Acción | Teclado | Gamepad (Input System) |
|---|---|---|
| Moverse | ++a++ / ++d++ o ++arrow-left++ / ++arrow-right++ | Stick izquierdo |
| Arriba / abajo (escaleras, nado, vuelo, top-down) | ++w++ / ++s++ o ++arrow-up++ / ++arrow-down++ | Stick izquierdo |
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
[PlayerGroundSlam](../components/movement/PlayerGroundSlam.md)). Las cinco acciones (Attack,
Special, Fly, Roll, Blink) se reasignan en
[KeyboardInputReader](../components/core/KeyboardInputReader.md).

!!! warning "Los botones del gamepad necesitan el Input System"
    Solo con el Input Manager clásico, el kit lee el movimiento de los ejes **Horizontal** /
    **Vertical** y el salto del botón **Jump** del Input Manager; el resto de los botones del
    gamepad no se leen. Los detalles están en [Input](input.md).

## Tus primeros cinco minutos

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerWalkRun.mp4" poster="../../../assets/clips/PlayerWalkRun.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Caminar y luego mantener Shift para correr: lo primero que vas a probar después de Create Player.</figcaption></figure>

1. Crea una escena nueva y borra su **Main Camera** por defecto (la cámara del kit la reemplaza).
2. **GameplayKit → Create Platform**. Escálala en X (por ejemplo a 8) para tener un piso más ancho.
3. Mueve la vista Scene arriba de la plataforma y usa **GameplayKit → Create Player**.
4. **GameplayKit → Create 2D Camera**, **Create Managers** y **Create HUD**.
5. Dale Play. Camina, corre con ++shift++, haz doble salto, dash con ++q++ y salta contra el costado
   de la plataforma para deslizarte por la pared y hacer wall jump.
6. Agrega un enemigo con **Create Enemy** sobre la plataforma. Patrulla y te daña al tocarte;
   pégale dos veces con ++j++ para destruirlo (tiene 20 de vida y el ataque por defecto hace 10).

No hizo falta ninguna capa, tag ni referencia: la detección de suelo, los chequeos de pared y las
armas ignoran los colliders del propio personaje, así que todas las máscaras pueden quedar en
*Everything*.

Ahora cambia algo. Selecciona el Player y pon **Extra Jumps** de
[PlayerMultiJump](../components/movement/PlayerMultiJump.md) en `2` para un triple salto, o quita
[PlayerDash](../components/movement/PlayerDash.md) para eliminar el dash. Cada habilidad es su
propio componente, así que agregar o quitar una mecánica es solo **Add Component** / **Remove
Component**.

## Siguientes pasos

- [Personaje de plataformas](platformer-character.md) — arma un personaje a mano y ajusta el salto.
- [Personaje top-down](top-down-character.md) — movimiento sin gravedad.
- [Input](input.md) — reasigna teclas o controla un personaje con tu propia fuente de input.
- [Combate](combat.md) y [Vida y daño](health-and-damage.md) — armas, golpes, muerte y respawn.
