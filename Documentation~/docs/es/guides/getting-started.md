# Primeros pasos

Gameplay Kit es un paquete de Unity con componentes de gameplay 2D pequeños e independientes. Armas
personajes, enemigos y niveles apilando componentes desde **Add Component**, y todo funciona con
sus valores por defecto: no hay capas, tags ni referencias que configurar antes de darle Play.

Esta página cubre lo que comparten los dos estilos de juego: instalar el paquete y usar sus menús.
Después elige tu camino:

<div class="grid cards" markdown>

-   **Juego de plataformas**

    ---

    Vista lateral con gravedad: correr, saltar, paredes, escaleras, agua. La demo es un nivel de
    plataformas completo.

    [:octicons-arrow-right-24: Empieza un juego de plataformas](platformer-game.md)

-   **Juego top-down**

    ---

    Vista desde arriba sin gravedad: movimiento y dash en 8 direcciones, apuntar con el mouse y
    disparar. La demo es un dungeon de tres salas.

    [:octicons-arrow-right-24: Empieza un juego top-down](top-down-game.md)

</div>

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
    https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0
    ```

    O agrégalo directamente en el `Packages/manifest.json` de tu proyecto:

    ```json
    "com.alejocastellanos.gameplaykit": "https://github.com/alejandrocastellanos/com.alejocastellanos.gameplaykit.git#v1.1.0"
    ```

    El sufijo `#v1.1.0` fija la versión, así tu proyecto no cambia cuando llegan commits nuevos.

=== "Carpeta local"

    Clona o descarga el paquete en tu disco y luego usa **+ → Add package from disk…** en el
    Package Manager y elige su `package.json`, o referencia la carpeta en `Packages/manifest.json`:

    ```json
    "com.alejocastellanos.gameplaykit": "file:/ruta/a/com.alejocastellanos.gameplaykit"
    ```

    Es la mejor opción si quieres modificar el paquete mientras lo usas.

## Los menús de GameplayKit

El kit agrega dos menús que crean objetos listos para usar. Cada objeto aparece en el centro de la
vista Scene, queda seleccionado y se puede deshacer con ++ctrl+z++ (++cmd+z++ en Mac).

!!! info "Son dos menús distintos"
    - **GameplayKit** es un menú propio en la **barra de menús principal** de Unity, al mismo nivel
      que *File*, *Edit*, *Assets* o *GameObject*. Es el único que tiene las **escenas demo**.
    - **GameObject → GameplayKit** (el mismo submenú aparece con **clic derecho en la jerarquía**)
      tiene los objetos sueltos, sin las escenas demo. Si haces clic derecho sobre un objeto, los
      personajes, enemigos y plataformas se crean como hijos de ese objeto.

### Barra de menús → GameplayKit

| Opción del menú | Estilo | Qué crea |
|---|---|---|
| **Create Player** | Plataformas | Un personaje de plataformas completo con tag `Player`: collider de cápsula, [CharacterController2D](../components/core/CharacterController2D.md), caminar/correr, doble salto, dash, wall jump y wall slide, agacharse, escaleras, nado, atravesar plataformas, pendientes, interactuar, ataque cuerpo a cuerpo, vida, knockback, muerte, respawn, inventario, parpadeo de daño, puente al Animator y [CharacterCore](../components/core/CharacterCore.md). |
| **Create Top-Down Player** | Top-down | Un personaje visto desde arriba con tag `Player`: collider circular, [PlayerTopDownMovement](../components/movement/PlayerTopDownMovement.md), dash en 8 direcciones, interactuar, ataque cuerpo a cuerpo, vida, knockback, muerte y respawn. |
| **Create Enemy** | Plataformas | Una caja roja con gravedad que patrulla (se da vuelta en paredes y bordes), daña al jugador al tocarlo y se destruye tras recibir suficiente daño (20 de vida), con parpadeo y números de daño. |
| **Create Top-Down Enemy** | Top-down | Un enemigo sin gravedad que persigue al jugador en los dos ejes cuando está a menos de 7 unidades ([EnemyChase](../components/ai/EnemyChase.md) con **Move Vertically**), daña al tocarlo y tiene 30 de vida. |
| **Create Platform** | Los dos | Un bloque sólido de 4 × 0,5 con `BoxCollider2D`: piso en un plataformas, pared en un top-down. |
| **Create 2D Camera** | Los dos | Una `Main Camera` ortográfica (tamaño 6) con [CameraFollow](../components/camera/CameraFollow.md) y [CameraShake](../components/camera/CameraShake.md). Sigue a lo que tenga el tag `Player`. |
| **Create Managers** | Los dos | Un objeto `Managers` con [ScoreManager](../components/managers/ScoreManager.md) y [PauseManager](../components/managers/PauseManager.md). |
| **Create HUD** | Los dos | Un canvas con barra de vida, texto de puntaje y menú de pausa (reanudar / reiniciar), más un `EventSystem` si la escena no tiene uno. |
| **Create Demo Scene** | Plataformas | El nivel demo de plataformas. Mira [Juego de plataformas](platformer-game.md#la-demo-de-plataformas). |
| **Create Top-Down Demo Scene** | Top-down | El dungeon demo top-down. Mira [Juego top-down](top-down-game.md#el-dungeon-demo). |

Las dos escenas demo te piden guardar la escena actual, se guardan en `Assets/GameplayKitDemo/` y
nunca se pisan entre sí. También vienen como sample: **Package Manager → Gameplay Kit → Samples →
Demo 2D → Import**.

### Clic derecho en la jerarquía → GameplayKit

| Opción | Igual que |
|---|---|
| **Player** | **Create Player** |
| **Top-Down Player** | **Create Top-Down Player** |
| **Enemy** | **Create Enemy** |
| **Top-Down Enemy** | **Create Top-Down Enemy** |
| **Platform** | **Create Platform** |
| **2D Camera**, **Managers**, **HUD** | Las opciones del mismo nombre. Siempre se crean en la raíz de la escena, y una sola vez aunque tengas varios objetos seleccionados. |

!!! tip "Textos del HUD"
    El HUD se crea con textos de ejemplo en español ("Puntos: 0", "Pausa", "Reanudar",
    "Reiniciar"). Si tu juego está en otro idioma, cambia la etiqueta del puntaje con el campo
    **Format** de [UIScoreText](../components/ui/UIScoreText.md) y edita los textos de los botones
    directamente en la jerarquía.

## Controles

Los dos estilos leen el teclado y el gamepad con [KeyboardInputReader](../components/core/KeyboardInputReader.md),
pero cada uno usa teclas distintas (en top-down no hay salto, y se apunta con el mouse). Las tablas
están en cada camino: [controles de plataformas](platformer-game.md#controles) y
[controles top-down](top-down-game.md#controles).

!!! warning "Los botones del gamepad necesitan el Input System"
    Solo con el Input Manager clásico, el kit lee el movimiento de los ejes **Horizontal** /
    **Vertical** y el salto del botón **Jump** del Input Manager; el resto de los botones del
    gamepad no se leen. Los detalles están en [Input](input.md).

## Siguientes pasos

- [Juego de plataformas](platformer-game.md) — la demo, los controles y tu primer nivel de plataformas.
- [Juego top-down](top-down-game.md) — el dungeon demo, los controles y tu primera sala top-down.
- [Input](input.md) — reasigna teclas o controla un personaje con tu propia fuente de input.
