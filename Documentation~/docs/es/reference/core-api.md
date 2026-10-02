---
title: API del núcleo
description: Referencia de los tipos de Gameplay Kit que no son componentes, como interfaces, helpers estáticos, enums y los tipos de máquinas de estados.
---

# API del núcleo

Estos tipos no son componentes, así que no tienen página de componente. Son los contratos y helpers sobre los
que se construyen los componentes, y los que más vas a usar al escribir tus propios scripts. Todos están en el
namespace `GameplayKit.Core` salvo que se indique otro.

## Interfaces

### ICharacterInput

Lo que leen las habilidades en lugar del teclado. `KeyboardInputReader` la implementa. Impleméntala tú para
manejar un personaje con IA, replays, red o el `PlayerInput` del Input System.

| Miembro | Descripción |
|---|---|
| `Vector2 MoveInput { get; }` | Movimiento, −1..1 por eje. |
| `bool JumpPressedThisFrame { get; }` | True solo en el frame en que se presiona saltar. |
| `bool JumpHeld { get; }` | True mientras se mantiene saltar. |
| `bool JumpReleasedThisFrame { get; }` | True solo en el frame en que se suelta saltar. |
| `bool RunHeld { get; }` | True mientras se mantiene correr. |
| `bool CrouchHeld { get; }` | True mientras se mantiene agacharse. |
| `bool DashPressedThisFrame { get; }` | True solo en el frame en que se presiona dash. |
| `bool InteractPressedThisFrame { get; }` | True solo en el frame en que se presiona interactuar. |
| `bool GetActionDown(CharacterAction action)` | True solo en el frame en que se presiona la acción (como `GetKeyDown`). |
| `bool GetAction(CharacterAction action)` | True mientras se mantiene la acción (como `GetKey`). |
| `bool GetActionUp(CharacterAction action)` | True solo en el frame en que se suelta la acción (como `GetKeyUp`). |

Las habilidades toman el componente de input una vez, en `Initialize`, con `GetComponent<ICharacterInput>()`.
Deja una sola implementación en el personaje: `CharacterCore` solo agrega un `KeyboardInputReader` cuando no
hay ninguna. Actualiza tus valores *antes* de que los lea `CharacterCore.Update`, por ejemplo con un
`[DefaultExecutionOrder]` negativo, como hace el input de las pruebas.

```csharp
using GameplayKit.Core;
using UnityEngine;

/// Auto-runner: siempre corre a la derecha; salta con Espacio, botón A del gamepad o clic.
[DefaultExecutionOrder(-100)]
public class AutoRunnerInput : MonoBehaviour, ICharacterInput
{
    public Vector2 MoveInput => Vector2.right;
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleasedThisFrame { get; private set; }
    public bool RunHeld => true;
    public bool CrouchHeld => false;
    public bool DashPressedThisFrame => false;
    public bool InteractPressedThisFrame => false;

    public bool GetActionDown(CharacterAction action) => false;
    public bool GetAction(CharacterAction action) => false;
    public bool GetActionUp(CharacterAction action) => false;

    private void Update()
    {
        JumpPressedThisFrame = InputCompat.JumpPressedThisFrame || InputCompat.GetKeyDown(KeyCode.Mouse0);
        JumpHeld = InputCompat.JumpHeld || InputCompat.GetKey(KeyCode.Mouse0);
        JumpReleasedThisFrame = InputCompat.JumpReleasedThisFrame || InputCompat.GetKeyUp(KeyCode.Mouse0);
    }
}
```

### IDamageable

Cualquier cosa que pueda recibir daño. Las armas, zonas de daño y enemigos solo conocen esta interfaz. La
implementan `CharacterHealth`, `DamageableObject` y `BreakableObject`.

| Miembro | Descripción |
|---|---|
| `void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)` | Aplica `amount` de daño. `hitDirection` apunta alejándose de la fuente (`CharacterHealth` la usa para el knockback). `instigator` es el atacante y puede ser `null`. |

Quien golpea la busca con `GetComponentInParent<IDamageable>()` desde el collider que tocó, así que puede estar
en la raíz de un personaje cuyos colliders son hijos.

```csharp
using GameplayKit.Core;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    public float TotalDamage { get; private set; }

    public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
    {
        TotalDamage += amount;
        Debug.Log($"{(instigator != null ? instigator.name : "Algo")} golpeó a {name} por {amount}");
    }
}
```

### IHealthSource

Cualquier cosa con vida. El feedback y la IA la leen sin depender de una clase concreta. La implementan
`CharacterHealth` y `DamageableObject`.

| Miembro | Descripción |
|---|---|
| `float CurrentHealth { get; }` | Vida actual. |
| `float MaxHealth { get; }` | Vida máxima. |
| `event Action<float> Damaged` | Se dispara con la cantidad de daño. |
| `event Action<float> Healed` | Se dispara con la cantidad curada. `DamageableObject` nunca se cura, así que ahí nunca se dispara. |

La usan `DamageFlash`, `DamagePopupSpawner`, `AIDecisionHealthThreshold` y `CharacterAnimatorBridge`.

```csharp
var health = GetComponentInParent<IHealthSource>();
health.Damaged += amount => Debug.Log($"-{amount} ({health.CurrentHealth}/{health.MaxHealth})");
```

### IKeyHolder

Algo que lleva ids de ítems o llaves, normalmente el jugador. La implementa `InventoryManager`. La usan
`ItemPickup` (agrega), `DoorWithKey` (revisa y, opcionalmente, quita) y `LevelExit` (revisa).

| Miembro | Descripción |
|---|---|
| `void AddKey(string keyId)` | Guarda un id. |
| `bool HasKey(string keyId)` | Si se tiene el id. |
| `void RemoveKey(string keyId)` | Quita un id. |

### IInteractable

Algo que el jugador puede accionar con el botón de interactuar. `PlayerInteract` busca el más cercano dentro
de su radio (triggers incluidos) y llama `Interact`. La implementa `Lever`.

| Miembro | Descripción |
|---|---|
| `void Interact(GameObject interactor)` | Se llama con el GameObject del personaje que interactúa. |

```csharp
using GameplayKit.Core;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId = "llave_dorada";
    private bool _opened;

    public void Interact(GameObject interactor)
    {
        if (_opened) return;
        var holder = interactor.GetComponentInParent<IKeyHolder>();
        if (holder == null) return;
        holder.AddKey(itemId);
        _opened = true;
    }
}
```

Dale al cofre un `Collider2D` (puede ser trigger) para que `PlayerInteract` lo encuentre.

## Helpers estáticos

### PhysicsQuery2D

Consultas de física 2D que ignoran al personaje que pregunta. Los chequeos de suelo, pared, borde y visión
empiezan dentro del collider de quien pregunta, y `Physics2D` directo devolvería ese collider. Con estas
funciones las máscaras de capas pueden quedarse en *Everything*.

| Miembro | Descripción |
|---|---|
| `static Transform OwnerOf(Component component)` | La raíz física de un componente: el Transform de su `Rigidbody2D` padre, o el suyo si no hay. |
| `static bool IsPartOf(Collider2D collider, Transform owner)` | True si el collider es el propio dueño, un hijo suyo o está unido a su Rigidbody2D. |
| `static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask mask, Transform ignore, Transform ignoreAlso = null, bool includeTriggers = false)` | El impacto más cercano que no pertenezca a `ignore` ni a `ignoreAlso`. Salta los triggers salvo con `includeTriggers`. Revisa `hit.collider != null`. |
| `static bool OverlapCircle(Vector2 point, float radius, LayerMask mask, Transform ignore, bool includeTriggers = false)` | True si en el círculo hay algún collider que no pertenezca a `ignore` (por defecto, solo sólidos). |
| `static float Facing(Transform transform)` | 1 o −1: el signo de `lossyScale.x`, que es como voltean a los personajes `PlayerWalkRun` y los movimientos de enemigos. |

```csharp
// ¿Hay un techo justo encima del personaje? Ignora sus propios colliders.
Bounds b = GetComponent<Collider2D>().bounds;
RaycastHit2D hit = PhysicsQuery2D.Raycast(new Vector2(b.center.x, b.max.y), Vector2.up, 0.2f, ~0, transform);
bool blocked = hit.collider != null;
```

### InputCompat

La capa de input: todos los componentes del kit leen teclado, mouse y gamepad a través de ella, nunca con
`UnityEngine.Input` directamente. Usa el Input System cuando el paquete está instalado y habilitado en
**Active Input Handling** (*New* o *Both*), y el Input Manager clásico en caso contrario. Las teclas se siguen
configurando como `KeyCode` y se traducen.

| Miembro | Descripción |
|---|---|
| `static bool GetKey(KeyCode key)` / `GetKeyDown` / `GetKeyUp` | Como los métodos clásicos de `Input`. `Mouse0`–`Mouse4` son los botones del mouse. |
| `static Vector2 MousePosition { get; }` | Posición del puntero en píxeles de pantalla. |
| `static Vector2 MoveAxis { get; }` | WASD/flechas, o el stick izquierdo si no hay ninguna tecla presionada (con largo máximo 1). Con el Input Manager clásico, los ejes `Horizontal`/`Vertical`. |
| `static Vector2 AimStick { get; }` | Stick derecho del gamepad. Siempre cero con el Input Manager clásico. |
| `static bool JumpHeld` / `JumpPressedThisFrame` / `JumpReleasedThisFrame { get; }` | Espacio o el botón sur del gamepad (A/Cruz). Con el Input Manager clásico, el botón `Jump`. |
| `enum PadButton` | `None`, `South`, `East`, `West`, `North`, `LeftShoulder`, `RightShoulder`, `LeftTrigger`, `RightTrigger`, `Start`, `Select`. |
| `static bool GetPadButton(PadButton button)` / `GetPadButtonDown` / `GetPadButtonUp` | Botones de gamepad, independientes del paquete de input. Siempre false con el Input Manager clásico. |

Si no hay ningún sistema de input disponible, todos devuelven false o cero.

```csharp
if (InputCompat.GetKeyDown(KeyCode.Tab) || InputCompat.GetPadButtonDown(InputCompat.PadButton.North))
    ToggleMap();
```

### TagFilter

Comparación de tags tolerante. `CompareTag` lanza un error si el tag no está definido en el Tag Manager; estos
métodos no, y además aceptan el tag del `Rigidbody2D` unido al collider, así un collider en un hijo del jugador
cuenta como el jugador.

| Miembro | Descripción |
|---|---|
| `static bool Matches(Component component, string tag)` | True solo si `tag` no está vacío y el objeto (o el Rigidbody2D de su collider) lo tiene. |
| `static bool PassesOptional(Component component, string tag)` | Filtro opcional: un tag vacío acepta cualquier objeto. |

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (!TagFilter.Matches(other, "Player")) return;
    // ...
}
```

### PlayerLocator

Encuentra al jugador para los componentes cuyo objetivo está vacío. Lo usan `EnemyChase`, `EnemyFlee` y
`EnemyShootOnSight`, así los prefabs y los enemigos generados en runtime no necesitan una referencia a la escena.
Busca el GameObject con tag `Player` con `GameObject.FindWithTag`, pero como mucho una vez cada medio segundo, así
que llamarlo cada frame desde muchos enemigos sigue siendo barato.

| Miembro | Descripción |
|---|---|
| `const string PlayerTag` | `"Player"`, el tag que busca. |
| `static bool Resolve(ref Transform target, ref float nextSearchTime)` | Devuelve true si `target` ya tiene valor. Si está vacío y ya pasó `nextSearchTime`, busca al jugador con el tag, llena `target` si lo encuentra y programa la siguiente búsqueda 0,5 s después. Usa un campo `nextSearchTime` por componente. |

```csharp
using GameplayKit.Core;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform target; // vacío = el objeto con tag Player
    private float _nextSearch;

    private void Update()
    {
        if (!PlayerLocator.Resolve(ref target, ref _nextSearch)) return;
        float side = Mathf.Sign(target.position.x - transform.position.x);
        transform.localScale = new Vector3(side, 1f, 1f);
    }
}
```

### TriggerOccupants&lt;T&gt; y PlatformRiders

Estos dos viven en `GameplayKit.Environment` y son **internal**: son helpers de implementación de las zonas y
plataformas del kit y no se pueden usar desde tus assemblies. Se documentan aquí porque explican un
comportamiento que puedes observar.

- **`TriggerOccupants<T>`** registra quién está dentro de un trigger con Enter/Exit, porque
  `OnTriggerStay2D` deja de llamarse cuando un cuerpo se duerme (un jugador quieto). Cuenta colliders por
  ocupante, así un personaje con varios colliders no sale antes de tiempo, y su snapshot descarta objetos
  destruidos. Miembros: `Enter(T)`, `Exit(T)` (true cuando el ocupante salió del todo) y `Snapshot()`. Lo usan
  `HazardZone`, `HealthPickup`, `WaterZone` y `WindZone2D`.
- **`PlatformRiders`** lleva a los cuerpos dinámicos parados sobre una plataforma cinemática. Emparentar no
  funciona con Rigidbody2D dinámicos, así que en cada paso de física desplaza a cada pasajero lo mismo que la
  plataforma y lo despierta. Un cuerpo cuenta como pasajero si la base de su collider está a la altura de la
  cara superior de la plataforma o por encima (con 0.1 unidades de tolerancia). Miembros:
  `OnContact(Collision2D)`, `OnContactEnded(Collision2D)` y `Carry(Vector2 delta)`. Lo usan `MovingPlatform`,
  `Elevator` y `ConveyorBelt`.

Para lograr lo mismo en tus scripts, copia el patrón: registra ocupantes en Enter/Exit y actúa desde
`Update`/`FixedUpdate`.

## Enums

### CharacterAction

Botones más allá de mover/saltar/correr/agacharse/dash/interactuar. `KeyboardInputReader` asigna a cada uno una
tecla y un botón de gamepad en **Action Bindings**.

| Valor | Tecla por defecto | Botón de gamepad por defecto | Lo usa |
|---|---|---|---|
| `Attack` | J | West (X/Cuadrado) | `PlayerAttack` |
| `Special` | K | Right Shoulder | `PlayerAttack` (cambiar de arma con `WeaponInventorySlot`) |
| `Fly` | F | North | `PlayerFly` |
| `Roll` | Alt izquierdo | East | `PlayerRollDodge` |
| `Blink` | C | Left Shoulder | `PlayerTeleportBlink` |

### MovementState

Qué está haciendo el personaje. Se guarda en `CharacterCore.Movement` y cualquier habilidad puede leerlo o
cambiarlo. `CharacterAnimatorBridge` lo escribe como entero en el parámetro **MovementState** del Animator, en
este orden:

`Idle` (0), `Walking` (1), `Running` (2), `Jumping` (3), `Falling` (4), `Dashing` (5), `WallSliding` (6),
`Crouching` (7), `Swimming` (8).

### ConditionState

La condición general del personaje, independiente del movimiento. Se guarda en `CharacterCore.Condition`.

| Valor | Lo pone | Efecto |
|---|---|---|
| `Normal` | El inicio, `CharacterRespawn` (vía `ResetCharacter`), el fin de un aturdimiento | — |
| `Stunned` | `CharacterStun` | Habilidades suspendidas; `PlayerWalkRun` además ignora el input en este estado. |
| `Dead` | `CharacterDeath` | Habilidades suspendidas hasta revivir; `CharacterAnimatorBridge` activa **Dead**. |
| `Paused` | Tu código | `CharacterCore` no ejecuta ninguna habilidad mientras está en pausa. Nada del kit lo pone: `PauseManager` usa `Time.timeScale`. |

## CharacterStateMachine&lt;TState&gt;

Una máquina de estados genérica y liviana (`where TState : Enum`). `CharacterCore` tiene una para
`MovementState` (`Movement`) y otra para `ConditionState` (`Condition`).

| Miembro | Descripción |
|---|---|
| `CharacterStateMachine(TState initialState)` | Empieza con `CurrentState` y `PreviousState` en `initialState`. |
| `TState CurrentState { get; }` | El estado actual. |
| `TState PreviousState { get; }` | El estado antes del último cambio. |
| `event Action<TState, TState> OnStateChanged` | Se dispara con `(anterior, actual)` en cada cambio. |
| `void ChangeState(TState newState)` | Cambia de estado. Cambiar al estado actual no hace nada ni dispara el evento. |

```csharp
using GameplayKit.Core;
using UnityEngine;

/// Muestra un ícono de "mareado" mientras el personaje está aturdido.
public class StunIcon : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    private CharacterCore _core;

    private void Start()
    {
        _core = GetComponent<CharacterCore>();
        _core.Condition.OnStateChanged += HandleChanged;
    }

    private void OnDestroy()
    {
        if (_core != null) _core.Condition.OnStateChanged -= HandleChanged;
    }

    private void HandleChanged(ConditionState previous, ConditionState current) =>
        icon.SetActive(current == ConditionState.Stunned);
}
```

Suscríbete en `Start` o después: las máquinas de estados se crean en `CharacterCore.Awake`. Varias habilidades
escriben `Movement` cada frame (por ejemplo `PlayerWalkRun` escribe `Idle`/`Walking`/`Running` incluso en el
aire mientras `PlayerJump` escribe `Falling`), así que `Movement.OnStateChanged` puede dispararse seguido. Para
cosas como efectos al aterrizar, mejor lee el controlador (`IsGrounded`, `Velocity`).

## AIState y AITransition

Clases serializables de `GameplayKit.AI` que forman la lista **States** de un `AIBrain`. Normalmente se editan
en el Inspector; ver [Enemigos e IA](../guides/enemies-and-ai.md#aibrain-estados-acciones-y-decisiones).

**`AIState`**

| Campo | Descripción |
|---|---|
| `string name` | Nombre del estado; lo usan las transiciones y `AIBrain.TransitionTo` (distingue mayúsculas). |
| `List<AIActionBase> actions` | Acciones cuyo `PerformAction` corre cada frame mientras el estado está activo, en orden. Las acciones con el componente desactivado se saltan. |
| `List<AITransition> transitions` | Se revisan en orden después de las acciones; gana la primera que lleva a un estado distinto. |

**`AITransition`**

| Campo | Descripción |
|---|---|
| `AIDecisionBase decision` | Se evalúa cada frame. Las transiciones sin decisión, o con el componente de la decisión desactivado, se saltan. |
| `string trueTargetState` | Estado al que ir cuando la decisión devuelve true. Vacío = quedarse. |
| `string falseTargetState` | Estado al que ir cuando devuelve false. Vacío = quedarse. |

`AIBrain.states` es un campo serializado privado, así que los estados de un cerebro se configuran en el
Inspector, no por código. Las pruebas del kit lo llenan por reflexión con `TestWorld.Set(brain, "states", ...)`.
