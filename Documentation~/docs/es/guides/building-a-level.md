---
title: Construir un nivel
description: Managers, cámara, piezas de entorno, checkpoints, llaves y puertas, salidas de nivel y HUD, para juegos de plataformas y top-down.
---

# Construir un nivel

Un nivel jugable con Gameplay Kit es un jugador, algunos colliders sólidos, unas cuantas piezas de entorno, una
cámara, un puñado de managers y un HUD. Todo esto sirve igual para los dos estilos: en un plataformas los
colliders son pisos y en un top-down son paredes, pero los managers, los checkpoints, las puertas y el HUD
son los mismos.

Si quieres ver todas estas capas armadas antes de empezar, recorre una de las demos: la
[demo de plataformas](platformer-game.md#la-demo-de-plataformas) o el
[dungeon top-down](top-down-game.md#el-dungeon-demo).

## Managers

Los managers son componentes normales. Casi todos son singletons con un `Instance` estático, y los demás
componentes los encuentran solos. Agrega solo los que necesites.

| Manager | Hace | Persiste entre escenas |
|---|---|---|
| [GameManager](../components/managers/GameManager.md) | Guarda un `GameState` global (`Playing`, `Paused`, `GameOver`) y dispara `OnStateChanged`. `CharacterLives` lo pone en `GameOver`, y `PauseManager` lo alterna entre `Playing` y `Paused` (sin pisar nunca `GameOver`). | Sí |
| [LevelManager](../components/managers/LevelManager.md) | Límites del nivel (los usa `CameraBounds` si la cámara no tiene los suyos), el checkpoint activo y el vacío: con **Kill Below Void** activo, cualquier `CharacterHealth` por debajo de **Void Y** (−30) recibe daño letal. | No |
| [ScoreManager](../components/managers/ScoreManager.md) | Suma solo el **Value** de cada `Collectible` (escucha el evento estático `Collectible.OnAnyCollected`). `AddScore`, `ResetScore`, `OnScoreChanged`. | No |
| [InventoryManager](../components/managers/InventoryManager.md) | Ids de ítems y llaves. Implementa `IKeyHolder`, así que pickups, puertas y salidas lo usan. **Va en el jugador**, no en un objeto de managers, y no es singleton. Los ítems son un conjunto: cada id se tiene o no se tiene, sin cantidades. | Con el jugador |
| [PauseManager](../components/managers/PauseManager.md) | Alterna `Time.timeScale` con **Pause Key** (Esc) o **Pause Pad Button** (Start) y dispara `OnPauseChanged`, manteniendo sincronizado el estado del `GameManager`. Ignora la tecla durante el primer segundo y hasta verla suelta, así una tecla mantenida al cargar no pausa el juego. | No |
| [AudioManager](../components/managers/AudioManager.md) | `PlayMusic(clip)` hace crossfade entre dos fuentes de música durante **Music Fade Duration** (también en pausa); `PlaySfx(clip, volume)` reproduce efectos. Crea sus fuentes de audio si no las asignas. | Sí |
| [SaveLoadSystem](../components/managers/SaveLoadSystem.md) | `Save<T>(data)` / `TryLoad<T>(out data)` guardan cualquier clase `[Serializable]` como JSON en `PlayerPrefs` bajo **Save Key**. Un guardado corrupto se ignora con un aviso. | Sí |
| [SceneTransitionManager](../components/managers/SceneTransitionManager.md) | `LoadScene(nombre)` o `LoadScene(índice)` con fundido a **Fade Color**. Crea su propia capa de fundido a pantalla completa si no asignas una, ignora pedidos mientras hay una transición y vuelve `Time.timeScale` a 1. | Sí |

**GameplayKit → Create Managers** crea un objeto `Managers` con `ScoreManager` y `PauseManager`, lo básico de
cada escena.

!!! warning "Los managers persistentes van en su propio GameObject"
    `GameManager`, `AudioManager`, `SaveLoadSystem` y `SceneTransitionManager` llaman `DontDestroyOnLoad`, y un
    duplicado que despierta en la escena siguiente destruye **todo su GameObject**. Si pones `GameManager` en
    el mismo objeto que `ScoreManager`, el score manager viejo viaja a la escena siguiente y el objeto
    `Managers` de la escena nueva (con su `ScoreManager` recién creado) se destruye. Pon cada manager
    persistente en su propio GameObject y deja juntos los de escena (`LevelManager`, `ScoreManager`,
    `PauseManager`). Ese GameObject puede ser hijo de tu objeto `Managers` para tener todo ordenado:
    los managers persistentes se mueven solos a la raíz de la escena antes de llamar `DontDestroyOnLoad`,
    que solo funciona con objetos raíz.

Como `ScoreManager` vive en la escena, el puntaje empieza en cero en cada una. Para conservarlo, o guardarlo
entre sesiones, usa el sistema de guardado:

```csharp
using GameplayKit.Managers;
using UnityEngine;

[System.Serializable]
public class Progress
{
    public int score;
    public string lastLevel;
}

public class ProgressSaver : MonoBehaviour
{
    public void SaveNow(string levelName)
    {
        SaveLoadSystem.Instance.Save(new Progress { score = ScoreManager.Instance.CurrentScore, lastLevel = levelName });
    }

    private void Start()
    {
        if (SaveLoadSystem.Instance != null && SaveLoadSystem.Instance.TryLoad(out Progress progress))
            ScoreManager.Instance.AddScore(progress.score);
    }
}
```

## Cámara

**GameplayKit → Create 2D Camera** crea una cámara ortográfica (tamaño 6) con tag `MainCamera`, `CameraFollow`
y `CameraShake`. Los cuatro componentes de cámara se apilan en un orden fijo: `CameraFollow` mueve la
cámara, `CameraBounds` (orden de ejecución 50) la limita y `CameraShake` (orden 100) suma su desplazamiento
encima y lo quita al frame siguiente.

| Componente | Notas |
|---|---|
| [CameraFollow](../components/camera/CameraFollow.md) | Sigue a **Target** con `SmoothDamp` (**Smooth Time** 0.2) más **Offset** (0, 0, −10). Con **Target** vacío busca el objeto con tag `Player` y se coloca sobre él en el primer frame. `SetTarget()` cambia el objetivo en runtime. |
| [CameraBounds](../components/camera/CameraBounds.md) | Mantiene la vista dentro de **Min Bounds**/**Max Bounds** considerando el tamaño ortográfico y la proporción de pantalla. Si el área es más chica que la vista en un eje, centra la cámara. Con (0, 0)/(0, 0) usa los límites del `LevelManager`, y si no hay, no limita. |
| [CameraShake](../components/camera/CameraShake.md) | `Shake()` usa la duración y magnitud por defecto; `Shake(duración, magnitud)` las indica. Usa tiempo sin escalar. Nada del kit la llama por ti: conéctala a un evento. |
| [CameraZoomBySpeed](../components/camera/CameraZoomBySpeed.md) | Aleja el zoom de **Min Zoom** (5) a **Max Zoom** (8) a medida que la velocidad del objetivo se acerca a **Speed For Max Zoom** (10). Sin objetivo usa el `Rigidbody2D` del objeto con tag `Player`. |

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/CameraShake.mp4" poster="../../../assets/clips/CameraShake.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>CameraShake suma su temblor sobre la posición de seguimiento y después lo retira.</figcaption></figure>

Una sacudida cuando el jugador recibe daño:

```csharp
using GameplayKit.CameraSystem;
using GameplayKit.Health;
using UnityEngine;

public class ShakeOnHit : MonoBehaviour
{
    [SerializeField] private CharacterHealth health;
    [SerializeField] private CameraShake cameraShake;

    private void OnEnable() => health.OnDamaged += HandleDamaged;
    private void OnDisable() => health.OnDamaged -= HandleDamaged;

    private void HandleDamaged(float amount) => cameraShake.Shake(0.2f, Mathf.Clamp(amount * 0.01f, 0.05f, 0.4f));
}
```

`PlayerGroundSlam.OnSlamLanded` es otro buen lugar para conectar una sacudida.

## Piezas de entorno

Cada componente de entorno funciona sobre un GameObject con el tipo de collider adecuado; la columna
"Collider" dice cuál ponerle.

| Pieza | Collider | Notas |
|---|---|---|
| [OneWayPlatform](../components/environment/OneWayPlatform.md) | Sólido (agrega un `BoxCollider2D` si no hay) | Configura un `PlatformEffector2D`. Con `PlayerDropThrough`, abajo + salto la atraviesa. |
| [MovingPlatform](../components/environment/MovingPlatform.md) | Sólido + `Rigidbody2D` (lo pone en Kinematic) | Recorre los **Waypoints** en loop o ping-pong; pueden ser hijos porque sus posiciones se copian al iniciar. Lleva consigo a los cuerpos dinámicos parados encima. |
| [Elevator](../components/environment/Elevator.md) | Sólido + `Rigidbody2D` | Se mueve entre **Bottom Point** y **Top Point** cuando llamas `Activate()` (desde el evento de una palanca o placa). |
| [FallingPlatform](../components/environment/FallingPlatform.md) | Sólido + `Rigidbody2D` | Cae **Fall Delay** segundos después de que el jugador la pisa; puede reaparecer. |
| [ConveyorBelt](../components/environment/ConveyorBelt.md) | Sólido | Suma su **Speed** en **Direction** a quien esté encima, aunque esté quieto. |
| [SpringPad](../components/environment/SpringPad.md) | Trigger (o sólido con **Use Trigger** apagado) | Lanza en **Launch Direction** con **Launch Force**. |
| [HazardZone](../components/environment/HazardZone.md) | Trigger | Daña a cualquier `IDamageable` al entrar y luego cada **Damage Interval**, aunque esté quieto. |
| [WaterZone](../components/environment/WaterZone.md) | Trigger | Arrastre y flotabilidad para cualquier cuerpo; `PlayerSwim` la reconoce sin tag. |
| [WindZone2D](../components/environment/WindZone2D.md) | Trigger | Una fuerza constante sobre los cuerpos de adentro. |
| [Teleporter](../components/environment/Teleporter.md) | Trigger | Envía a quien tenga **Target Tag** a **Destination**; un par de teletransportes no te devuelve de rebote. |
| [PushableBox](../components/environment/PushableBox.md) | Sólido + `Rigidbody2D` | El jugador la empuja al caminar contra ella. |
| [BreakableObject](../components/environment/BreakableObject.md) | Sólido | Se rompe tras recibir suficiente daño; puede soltar loot. |
| [Lever](../components/environment/Lever.md) | Cualquiera (los triggers cuentan) | La acciona `PlayerInteract` (**E**). Conecta **On Turned On**, **On Turned Off** u **On Toggled** en el Inspector. |
| [PressurePlate](../components/environment/PressurePlate.md) | Trigger | La presiona cualquier cuerpo (o uno con **Required Tag**). Eventos **On Pressed**/**On Released**. |
| [Collectible](../components/environment/Collectible.md), [HealthPickup](../components/environment/HealthPickup.md), [ItemPickup](../components/environment/ItemPickup.md) | Trigger | Monedas y puntaje, curación (por defecto solo si estás herido) e ítems o llaves. |
| [Checkpoint](../components/environment/Checkpoint.md), [DoorWithKey](../components/environment/DoorWithKey.md), [LevelExit](../components/environment/LevelExit.md) | Trigger | Se explican más abajo. |

Conectar una palanca con un elevador no necesita código: selecciona la palanca, agrega una entrada en **On
Toggled**, arrastra el elevador y elige `Elevator → Activate`. La primera activación lo sube y cada una de las
siguientes lo manda al otro extremo.

## Checkpoints y respawn

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/Checkpoint.mp4" poster="../../../assets/clips/Checkpoint.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Después de tocar el checkpoint, al morir el jugador reaparece ahí en lugar de al inicio.</figcaption></figure>

El jugador necesita [CharacterHealth](../components/health/CharacterHealth.md),
[CharacterDeath](../components/health/CharacterDeath.md) y [CharacterRespawn](../components/health/CharacterRespawn.md)
(*Create Player* agrega los tres). Esto es lo que pasa:

1. El jugador entra en el trigger de un `Checkpoint`. Si el collider (o su Rigidbody2D) tiene el **Player
   Tag**, el checkpoint llama `CharacterRespawn.SetCheckpoint` y, si hay un `LevelManager`,
   `SetActiveCheckpoint`. Enciende **Active Visual** y dispara `OnActivated`. Un checkpoint se activa **una
   sola vez**, así que volver a pasar por uno anterior no mueve tu punto de reaparición hacia atrás.
2. Algo mata al jugador: una `HazardZone` fuerte, un enemigo o el vacío del `LevelManager`.
3. `CharacterDeath` pone la condición en `Dead` y suspende todas las habilidades.
4. Después de **Respawn Delay** (1 s), `CharacterRespawn` mueve al jugador al último checkpoint (o a su
   **Initial Checkpoint**, o a donde empezó), pone su velocidad en cero, restaura la vida, lo revive y
   reinicia todas las habilidades.

Agrega [CharacterLives](../components/health/CharacterLives.md) y cada muerte cuesta una vida. Cuando se acaba
la última, el jugador se queda muerto, se dispara `OnGameOver` y `GameManager` (si existe) pasa a `GameOver`:

```csharp
using GameplayKit.Health;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private CharacterLives lives;
    [SerializeField] private GameObject panel;

    private void OnEnable() => lives.OnGameOver += Show;
    private void OnDisable() => lives.OnGameOver -= Show;

    private void Show() => panel.SetActive(true);
}
```

## Llaves y puertas

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/DoorWithKey.mp4" poster="../../../assets/clips/DoorWithKey.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>La puerta sigue cerrada hasta que el jugador recoge la llave; después se abre al tocarla.</figcaption></figure>

1. Asegúrate de que el jugador tenga un `IKeyHolder`; en la práctica, `InventoryManager` (*Create Player* lo
   agrega).
2. Coloca una llave: un trigger pequeño con [ItemPickup](../components/environment/ItemPickup.md). Al tocarlo
   agrega **Item Id** a quien lo lleva y se destruye.
3. Arma la puerta: un GameObject con un collider **trigger** un poco más grande que el hueco y
   [DoorWithKey](../components/environment/DoorWithKey.md). Agrégale un hijo con un collider **sólido** y el
   sprite de la puerta, y asigna ese hijo tanto a **Blocking Collider** como a **Visual When Closed**.
4. Cuando entra al trigger alguien que tiene el **Required Item Id**, la puerta desactiva el collider que
   bloquea, oculta el visual y dispara `OnOpened`. Con **Consume Item On Open** activo, la llave se gasta.

`ItemPickup` y `DoorWithKey` comparten el mismo id por defecto (`llave_dorada`), así que una llave y una
puerta funcionan juntas de entrada. Cambia ambos ids cuando un nivel tenga varias cerraduras.
`DoorWithKey.Open()` es público, así que el evento de una palanca o placa puede abrir una puerta sin llave.

## Salir del nivel

[LevelExit](../components/environment/LevelExit.md) es un trigger que carga otra escena cuando lo toca el
jugador (por **Player Tag**):

- Con **Scene Name** se carga esa escena; vacío, la siguiente de la lista del build. Si no hay siguiente,
  avisa en la consola y no hace nada.
- Con **Required Item Id**, solo un jugador que tenga ese ítem puede usarla.
- Si hay un `SceneTransitionManager` en la escena, la carga tiene fundido; si no, carga de inmediato.

Las escenas deben estar en la lista del build (**File → Build Profiles**) para cargarse. Para conservar al
jugador entre niveles, agrégale [CharacterPersistence](../components/health/CharacterPersistence.md) y quita
el jugador de las escenas siguientes (o déjalo: el duplicado se destruye solo). La cámara y la barra de vida
vuelven a encontrar solas al objeto con tag `Player`.

## El HUD

**GameplayKit → Create HUD** arma un canvas Screen Space Overlay (escalado para 1920×1080) con:

- **HealthBar**: un [UIHealthBar](../components/ui/UIHealthBar.md) que mueve un `Image` de tipo Filled. Con
  **Health** vacío encuentra al personaje con tag `Player`, y lo vuelve a buscar si ese personaje se
  reemplaza.
- **Score**: un [UIScoreText](../components/ui/UIScoreText.md). Su **Format** por defecto es `Puntos: {0}`;
  cámbialo por lo que quieras mientras tenga `{0}`.
- **PausePanel**: lo muestra y oculta un [UIPauseMenu](../components/ui/UIPauseMenu.md) en el canvas, que
  escucha a `PauseManager`. Sus dos botones llaman `OnResumeButtonPressed` y `OnRestartButtonPressed`.
  Reiniciar recarga la escena activa: en el editor funciona aunque la escena no esté en la lista del build,
  pero en un build tiene que estar. También existe `OnQuitButtonPressed`, que carga **Main Menu Scene Name**.
- Un `EventSystem`, con el módulo de UI del Input System si ese paquete está instalado y
  `StandaloneInputModule` si no.

Los textos del HUD generado ("Pausa", "Reanudar", "Reiniciar") son componentes `Text` normales que puedes
cambiar. Para números de daño sobre personajes y enemigos, agrégales
[DamagePopupSpawner](../components/ui/DamagePopupSpawner.md); no necesita canvas.

## Ver también

- [Enemigos e IA](enemies-and-ai.md): llena el nivel.
- [Vida y daño](health-and-damage.md): todo lo que pasa entre un golpe y un respawn.
- [Solución de problemas](troubleshooting.md): cuando un pickup, una puerta o la cámara no reaccionan.
