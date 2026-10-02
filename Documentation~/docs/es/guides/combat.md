# Combate

El combate en Gameplay Kit tiene tres capas que nunca se referencian directamente entre sí:

- **[PlayerAttack](../components/combat/PlayerAttack.md)**, una habilidad que convierte las acciones
  Attack y Special en llamadas a las armas;
- **componentes de arma** (cuerpo a cuerpo, hitscan, proyectil, combo, carga, inventario) que
  deciden *cómo* ocurre un golpe;
- **objetivos** que implementan `IDamageable` y deciden qué *hace* un golpe.

Como las armas solo conocen `IDamageable`, la misma espada daña al jugador, a un enemigo, a una caja
o a un script tuyo.

## Las armas de un vistazo

| Componente | Qué hace | Valores por defecto clave |
|---|---|---|
| [WeaponMelee](../components/combat/WeaponMelee.md) | Una hitbox circular frente al personaje, activa durante una ventana corta. Cada objetivo recibe como mucho un golpe por ataque. | **Damage** `10`, **Hitbox Radius** `0.75`, **Hitbox Offset** `(0.75, 0)`, **Active Duration** `0.15`, **Cooldown** `0.4` |
| [WeaponHitscan](../components/combat/WeaponHitscan.md) | Un raycast instantáneo (láser, francotirador). Dispara `OnShotFired(origin, end)` para que puedas dibujar el disparo. | **Damage** `15`, **Range** `20`, **Cooldown** `0.2` |
| [WeaponProjectile](../components/combat/WeaponProjectile.md) | Instancia un prefab de proyectil (bala, flecha, bola de fuego) desde **Fire Point**. | **Damage** `8`, **Projectile Speed** `12`, **Cooldown** `0.3` |
| [ProjectileBehaviour](../components/combat/ProjectileBehaviour.md) | Va en el prefab del proyectil: vuela, opcionalmente rebota o cae, y daña al primer `IDamageable` que toca. | **Lifetime** `5`, **Max Bounces** `0`, **Affected By Gravity** desactivado |
| [WeaponCombo](../components/combat/WeaponCombo.md) | Encadena varios golpes de `WeaponMelee` si la siguiente pulsación llega dentro de la ventana. | **Combo Window** `0.6` |
| [WeaponCharge](../components/combat/WeaponCharge.md) | Mantén para cargar, suelta para golpear con un daño entre el mínimo y el máximo. | **Max Charge Time** `1.5`, **Min Damage** `5`, **Max Damage** `30` |
| [WeaponInventorySlot](../components/combat/WeaponInventorySlot.md) | Guarda varias armas como objetos hijos y deja activa solo la equipada. | **Starting Weapon Index** `0` |

Todas ignoran los colliders del propio atacante, así que sus máscaras de capas pueden quedarse en
*Everything*.

!!! tip "Prefabs de proyectil"
    `ProjectileBehaviour` reacciona a contactos de **trigger**, así que marca el collider del
    proyectil como **Is Trigger**. Atraviesa otros triggers (agua, checkpoints…), nunca lastima a
    quien lo disparó y, si no tiene un `IDamageable` al que golpear, rebota o se destruye contra los
    colliders sólidos de **Collidable Layers**.

## Cómo elige un arma PlayerAttack

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/PlayerAttack.mp4" poster="../../../assets/clips/PlayerAttack.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>PlayerAttack atacando con el arma cuerpo a cuerpo en la acción Attack.</figcaption></figure>

Cuando se presiona la **Attack Action** (`Attack`, ++j++ por defecto), `PlayerAttack` busca un arma
en el personaje y en sus hijos **activos**, en este orden de prioridad, y usa la primera que
encuentra:

1. `WeaponCombo`
2. `WeaponMelee`
3. `WeaponHitscan`
4. `WeaponProjectile`

Las armas a distancia disparan en la dirección de
[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) si el personaje tiene uno, o
hacia adelante (hacia donde mira) si no. El cuerpo a cuerpo ignora el apuntado: golpea del lado al
que mira el personaje, o donde esté su **Hitbox Origin**. Cada arma maneja su propio cooldown, así
que presionar más rápido que el cooldown no hace nada.

`PlayerAttack` no ataca mientras el personaje está muerto y, como toda habilidad, se pausa mientras
las habilidades están suspendidas (aturdimiento, knockback). También puedes atacar desde código con
`TryAttackNow()`.

### Cambiar de arma

Pon cada arma en su propio objeto hijo, agrega esos hijos a la lista de un `WeaponInventorySlot`
**en el mismo objeto que `PlayerAttack`**, y la **Switch Weapon Action** (`Special`, ++k++) equipa la
siguiente. Solo el hijo equipado queda activo, así que la búsqueda por prioridad solo ve esa arma.

!!! warning "Quita el arma de la raíz cuando uses un inventario"
    Create Player pone un `WeaponMelee` en el propio personaje. Como la búsqueda revisa primero el
    objeto raíz, esa arma le gana a la que esté equipada en un hijo (salvo que la equipada sea un
    `WeaponCombo`, que tiene más prioridad). Muévela a un hijo (y a la lista del inventario) o
    quítala.

## Ataques cargados

<figure class="gk-clip" markdown="0"><video src="../../../assets/clips/WeaponCharge.mp4" poster="../../../assets/clips/WeaponCharge.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>Un toque rápido hace el daño mínimo; mantener el botón lo sube hasta el máximo.</figcaption></figure>

Agrega `WeaponCharge` junto a un arma cuerpo a cuerpo, hitscan o de proyectil, y `PlayerAttack`
pasa a un flujo de mantener y soltar:

1. Presionar Attack llama a `BeginCharge()`.
2. Mientras mantienes el botón, la carga crece hasta **Max Charge Time**. `ChargeRatio01` va de 0
   a 1.
3. Al soltar el botón, `ReleaseCharge()` calcula el daño — una interpolación lineal de **Min
   Damage** a **Max Damage** según la proporción de carga —, dispara `OnReleased(damage)` y el arma
   ataca con ese daño en lugar del suyo.

Por eso un toque rápido ataca con más o menos **Min Damage**. El reemplazo de daño no se aplica a
`WeaponCombo`, que siempre usa el daño propio de cada golpe. Una barra de carga son unas pocas
líneas:

```csharp
using GameplayKit.Combat;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private WeaponCharge charge;
    [SerializeField] private Image fill; // Image Type: Filled

    private void OnEnable() => charge.OnReleased += HandleReleased;
    private void OnDisable() => charge.OnReleased -= HandleReleased;

    private void Update()
    {
        fill.enabled = charge.IsCharging;
        fill.fillAmount = charge.ChargeRatio01;
    }

    private void HandleReleased(float damage) => Debug.Log($"Charged hit for {damage:0}");
}
```

## Combos

`WeaponCombo` tiene una lista ordenada de componentes `WeaponMelee` (**Combo Hits**) — normalmente en
el mismo objeto, cada uno con su propio daño, radio y offset. Cada pulsación usa el siguiente golpe
si llega dentro de **Combo Window** segundos desde el anterior; si no, la secuencia vuelve a empezar
desde el primer golpe. El combo solo avanza cuando el golpe realmente sale, así que mantén el
**Cooldown** de cada golpe más corto que la ventana.

## Apuntado

[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) calcula `AimDirection` con
uno de tres **Aim Mode**:

- **Mouse** (por defecto) — hacia el cursor, usando **Aim Camera** o `Camera.main`.
- **Stick** — hacia el stick derecho, ignorando los valores por debajo de **Stick Dead Zone**
  (`0.25`). Necesita el Input System.
- **Closest Target** — hacia el objeto con un `IDamageable` más cercano dentro de **Target Search
  Radius** (`10`), sin contar al propio personaje.

Si asignas **Part To Rotate** (un brazo o un arma), gira hacia donde apuntas sin quedar al revés
cuando el personaje mira a la izquierda. El cuerpo en sí nunca gira. Desde código, llama a
`SetAimDirection` (en modo **Stick** mantiene el valor hasta que el stick se mueva).

## Haz que las cosas se puedan golpear

Cualquier cosa con un componente que implemente `IDamageable` puede recibir golpes:

```csharp
public interface IDamageable
{
    void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator);
}
```

Las armas lo buscan con `GetComponentInParent` a partir del collider que tocaron, así que una
hurtbox puede ser un collider hijo. El kit incluye tres implementaciones:

| Componente | Úsalo para |
|---|---|
| [CharacterHealth](../components/health/CharacterHealth.md) | Personajes: invulnerabilidad tras un golpe, knockback, muerte, eventos. Mira [Vida y daño](health-and-damage.md). |
| [DamageableObject](../components/combat/DamageableObject.md) | Objetos y enemigos simples: **Max Health** `20`, se destruye en 0 (**Destroy On Death**), eventos `OnDamaged(amount, instigator)` y `OnDestroyedByDamage`. |
| [BreakableObject](../components/environment/BreakableObject.md) | Cajas y vasijas: **Health** `1`, **Break Effect Prefab** y **Loot Drops** opcionales, evento `OnBroken`. |

Escribir el tuyo lleva un solo método. `instigator` es el GameObject del arma que golpeó — el propio
personaje cuando el arma está en su raíz:

```csharp
using GameplayKit.Core;
using UnityEngine;

public class Gong : MonoBehaviour, IDamageable
{
    [SerializeField] private AudioSource sound;

    public void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator)
    {
        sound.Play();
        string who = instigator != null ? instigator.name : "something";
        Debug.Log($"{who} hit the gong for {amount} at {hitPoint}");
    }
}
```

## Números de daño y feedback

[DamagePopupSpawner](../components/ui/DamagePopupSpawner.md) muestra un número flotante cada vez que
el objeto (o su padre) recibe daño — `-10` en rojo — y, con **Show Heals**, `+N` en verde cuando se
cura. Aparece en **Offset** `(0, 1.2)` sobre el objeto como un texto pequeño en el mundo, o a partir
de tu propio **Popup Prefab** (un `Text` de uGUI con [UIDamagePopup](../components/ui/UIDamagePopup.md)
en un canvas world-space). Agrega [DamageFlash](../components/health/DamageFlash.md) para teñir los
sprites en cada golpe.

Los dos escuchan a `IHealthSource`, así que funcionan con `CharacterHealth` y `DamageableObject`,
pero **no** con `BreakableObject`, que no tiene eventos de vida.

## Siguientes pasos

- [Vida y daño](health-and-damage.md) — qué pasa después del golpe.
- [Enemigos e IA](enemies-and-ai.md) — enemigos que disparan y devuelven los golpes.
