# Combat

Combat in Gameplay Kit has three layers that never reference each other directly:

- **[PlayerAttack](../components/combat/PlayerAttack.md)**, an ability that turns the Attack and
  Special actions into weapon calls;
- **weapon components** (melee, hitscan, projectile, combo, charge, inventory) that decide *how* a
  hit happens;
- **targets** that implement `IDamageable` and decide what a hit *does*.

Because weapons only know `IDamageable`, the same sword damages the player, an enemy, a crate or a
script of yours.

## Weapons at a glance

| Component | What it does | Key defaults |
|---|---|---|
| [WeaponMelee](../components/combat/WeaponMelee.md) | A circular hitbox in front of the character, active for a short window. Each target is hit at most once per swing. | **Damage** `10`, **Hitbox Radius** `0.75`, **Hitbox Offset** `(0.75, 0)`, **Active Duration** `0.15`, **Cooldown** `0.4` |
| [WeaponHitscan](../components/combat/WeaponHitscan.md) | An instant raycast (laser, sniper). Fires `OnShotFired(origin, end)` so you can draw the shot. | **Damage** `15`, **Range** `20`, **Cooldown** `0.2` |
| [WeaponProjectile](../components/combat/WeaponProjectile.md) | Spawns a projectile prefab (bullet, arrow, fireball) from **Fire Point**. | **Damage** `8`, **Projectile Speed** `12`, **Cooldown** `0.3` |
| [ProjectileBehaviour](../components/combat/ProjectileBehaviour.md) | Goes on the projectile prefab: flies, optionally bounces or falls, damages the first `IDamageable` it touches. | **Lifetime** `5`, **Max Bounces** `0`, **Affected By Gravity** off |
| [WeaponCombo](../components/combat/WeaponCombo.md) | Chains several `WeaponMelee` hits if the next press arrives within the window. | **Combo Window** `0.6` |
| [WeaponCharge](../components/combat/WeaponCharge.md) | Hold to charge, release to hit with damage between min and max. | **Max Charge Time** `1.5`, **Min Damage** `5`, **Max Damage** `30` |
| [WeaponInventorySlot](../components/combat/WeaponInventorySlot.md) | Holds several weapons as child objects and keeps only the equipped one active. | **Starting Weapon Index** `0` |

All of them ignore the attacker's own colliders, so their layer masks can stay on *Everything*.

!!! tip "Projectile prefabs"
    `ProjectileBehaviour` reacts to **trigger** contacts, so mark the projectile's collider as
    **Is Trigger**. It flies through other triggers (water, checkpoints…), never hurts whoever
    fired it, and if it has no `IDamageable` to hit it bounces or is destroyed on solid colliders
    in **Collidable Layers**.

## How PlayerAttack picks a weapon

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/PlayerAttack.mp4" poster="../../assets/clips/PlayerAttack.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>PlayerAttack swinging the melee weapon on the Attack action.</figcaption></figure>

When the **Attack Action** (`Attack`, ++j++ by default) is pressed, `PlayerAttack` looks for a
weapon on the character and its **active** children, in this priority order, and uses the first one
it finds:

1. `WeaponCombo`
2. `WeaponMelee`
3. `WeaponHitscan`
4. `WeaponProjectile`

Ranged weapons fire along the direction from
[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) if the character has one, or
straight ahead (the facing direction) if not. Melee ignores aiming: it hits on the facing side, or
wherever its **Hitbox Origin** is. Each weapon handles its own cooldown, so pressing faster than the cooldown does nothing.

`PlayerAttack` doesn't attack while the character is dead, and like every ability it pauses while
abilities are suspended (stun, knockback). You can also attack from code with `TryAttackNow()`.

### Switching weapons

Put each weapon on its own child object, list those children in a `WeaponInventorySlot` **on the
same object as `PlayerAttack`**, and the **Switch Weapon Action** (`Special`, ++k++) equips the next
one. Only the equipped child stays active, so the priority search only sees that weapon.

!!! warning "Remove the root weapon when using an inventory"
    Create Player puts a `WeaponMelee` on the character itself. A weapon on the root wins over the
    equipped child (unless the equipped one is a `WeaponCombo`, which has higher priority), so with an
    inventory it would shadow your equipped weapon. Move it to a child (and into
    the inventory list) or remove it.

## Charge attacks

<figure class="gk-clip" markdown="0"><video src="../../assets/clips/WeaponCharge.mp4" poster="../../assets/clips/WeaponCharge.jpg" autoplay loop muted playsinline preload="metadata"></video><figcaption>A quick tap deals the minimum damage; holding the button builds up to the maximum.</figcaption></figure>

Add `WeaponCharge` next to a melee, hitscan or projectile weapon and `PlayerAttack` switches to a
hold-and-release flow:

1. Pressing Attack calls `BeginCharge()`.
2. While the button is held, the charge grows until **Max Charge Time**. `ChargeRatio01` goes from
   0 to 1.
3. When the button is released, `ReleaseCharge()` computes the damage — a linear blend from **Min
   Damage** to **Max Damage** by the charge ratio — fires `OnReleased(damage)` and the weapon attacks
   with that damage instead of its own.

A quick tap therefore attacks with roughly **Min Damage**. The damage override doesn't apply to
`WeaponCombo`, which always uses each hit's own damage. A charge bar is a few lines:

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

`WeaponCombo` holds an ordered list of `WeaponMelee` components (**Combo Hits**) — typically on the
same object, each with its own damage, radius and offset. Every press uses the next hit if it
arrives within **Combo Window** seconds of the previous one; otherwise the sequence restarts at the
first hit. The combo only advances when the hit actually fires, so keep each hit's **Cooldown**
shorter than the window.

## Aiming

[CharacterAimAndOrient](../components/combat/CharacterAimAndOrient.md) computes `AimDirection` with
one of three **Aim Mode**s:

- **Mouse** (default) — towards the cursor, using **Aim Camera** or `Camera.main`.
- **Stick** — towards the right stick, ignoring values under **Stick Dead Zone** (`0.25`). Needs the
  Input System.
- **Closest Target** — towards the nearest object with an `IDamageable` within **Target Search
  Radius** (`10`), excluding the character itself.

If you assign **Part To Rotate** (an arm or a gun), it rotates towards the aim without ending up
upside down when the character faces left. The body itself never rotates. From code, call
`SetAimDirection` (in **Stick** mode it keeps the value until the stick moves).

## Making things hittable

Anything with a component that implements `IDamageable` can be hit:

```csharp
public interface IDamageable
{
    void ApplyDamage(float amount, Vector2 hitPoint, Vector2 hitDirection, GameObject instigator);
}
```

Weapons look it up with `GetComponentInParent` from the collider they touched, so a hurtbox can be
a child collider. The kit includes three implementations:

| Component | Use it for |
|---|---|
| [CharacterHealth](../components/health/CharacterHealth.md) | Characters: invulnerability after a hit, knockback, death, events. See [Health and damage](health-and-damage.md). |
| [DamageableObject](../components/combat/DamageableObject.md) | Simple props and enemies: **Max Health** `20`, destroyed at 0 (**Destroy On Death**), `OnDamaged(amount, instigator)` and `OnDestroyedByDamage` events. |
| [BreakableObject](../components/environment/BreakableObject.md) | Crates and pots: **Health** `1`, optional **Break Effect Prefab** and **Loot Drops**, `OnBroken` event. |

Writing your own takes one method. `instigator` is the GameObject of the weapon that hit — the
character itself when the weapon sits on its root:

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

## Damage numbers and feedback

[DamagePopupSpawner](../components/ui/DamagePopupSpawner.md) shows a floating number every time the
object (or its parent) takes damage — `-10` in red — and, with **Show Heals**, `+N` in green when it
heals. It spawns at **Offset** `(0, 1.2)` above the object as a small world-space text, or from your
own **Popup Prefab** (a uGUI `Text` with [UIDamagePopup](../components/ui/UIDamagePopup.md) on a
world-space canvas). Add [DamageFlash](../components/health/DamageFlash.md) to tint the sprites on
each hit.

Both listen to `IHealthSource`, so they work with `CharacterHealth` and `DamageableObject`, but
**not** with `BreakableObject`, which has no health events.

## Next steps

- [Health and damage](health-and-damage.md) — what happens after the hit.
- [Enemies and AI](enemies-and-ai.md) — enemies that shoot and hit back.
