---
title: Componentes
description: Todos los componentes de Gameplay Kit, agrupados por categoría.
---

# Componentes

Gameplay Kit tiene 113 componentes en nueve categorías. Cada categoría corresponde a un namespace
(`GameplayKit.Movement`, `GameplayKit.AI`...) y a una carpeta de `Runtime/`. Los componentes se agregan desde
**Add Component** buscando su nombre, y no hace falta conectar nada a mano para empezar. Si prefieres partir
de algo que ya funciona, el menú **GameplayKit** crea jugadores, enemigos, cámaras, managers y HUD listos para
usar (ver [Primeros pasos](../guides/getting-started.md)).

## Cómo leer la página de un componente

Todas las páginas de componentes siguen la misma estructura:

- **Insignias y resumen**: la categoría, la clase base cuando no es `MonoBehaviour` (por ejemplo
  `AbilityBase` para las habilidades del personaje) y una insignia *Experimental* cuando corresponde, seguidas
  de un resumen de una línea.
- **Clip**: un loop corto grabado en juego que muestra al componente haciendo lo suyo. Los componentes sin
  nada visible que mostrar (managers que solo guardan datos, clases base) lo indican en su lugar.
- **Descripción**: qué hace, cómo se comporta en detalle y cómo se combina con otros componentes del kit.
- **Tabla de información**: namespace, clase base, componentes requeridos (Unity los agrega solo) e
  interfaces que implementa, como `IDamageable` o `IInteractable`.
- **Cómo se usa**: pasos numerados para ponerlo a funcionar.
- **Inspector**: cada campo serializado, con su etiqueta del Inspector en negrita y el nombre del campo debajo,
  el tipo, el valor por defecto y qué controla. Los valores por defecto son los reales del código y están
  pensados para funcionar tal cual.
- **Eventos**: los `UnityEvent` que puedes conectar en el Inspector y los eventos C# a los que puedes
  suscribirte desde código.
- **API para scripts**: propiedades y métodos públicos que vale la pena conocer.
- **Ejemplo** y **Consejos**: un fragmento de código realista, y consejos prácticos y errores comunes.
- **Ver también**: componentes muy relacionados.

Las habilidades (componentes derivados de `AbilityBase`) solo se ejecutan en un personaje que tenga un
`CharacterCore`. Las clases base (`AbilityBase`, `AIActionBase`, `AIDecisionBase`) se heredan, no se agregan, y
sus páginas muestran cómo.

## Categorías

<div class="grid cards" markdown>

-   :material-cog:{ .lg .middle } __Núcleo__

    ---

    La base del personaje: `CharacterCore`, `AbilityBase`, `CharacterController2D`, el lector de input y el
    puente al Animator.

    [:octicons-arrow-right-24: Núcleo](core/index.md)

-   :material-run-fast:{ .lg .middle } __Movimiento__

    ---

    Treinta habilidades y marcadores para juegos de plataformas y top-down: saltos, dashes, paredes, cornisas,
    escaleras, cuerdas, tirolesas, agua, vuelo y más.

    [:octicons-arrow-right-24: Movimiento](movement/index.md)

-   :material-heart-pulse:{ .lg .middle } __Vida y física__

    ---

    Vida, knockback, aturdimiento, muerte, respawn, vidas, daño por caída, control de la gravedad, persistencia
    y parpadeo al recibir daño.

    [:octicons-arrow-right-24: Vida y física](health/index.md)

-   :material-sword:{ .lg .middle } __Combate__

    ---

    `PlayerAttack` y las armas: melee, hitscan, proyectiles, combos, carga, inventario, apuntado y objetos
    dañables.

    [:octicons-arrow-right-24: Combate](combat/index.md)

-   :material-robot:{ .lg .middle } __IA y enemigos__

    ---

    Comportamientos de enemigo independientes, el spawner y la máquina de estados `AIBrain` con sus acciones y
    decisiones.

    [:octicons-arrow-right-24: IA y enemigos](ai/index.md)

-   :material-terrain:{ .lg .middle } __Entorno__

    ---

    Plataformas, ascensores, palancas, placas, puertas y llaves, zonas de daño, agua, viento, pickups,
    checkpoints y salidas.

    [:octicons-arrow-right-24: Entorno](environment/index.md)

-   :material-video:{ .lg .middle } __Cámara__

    ---

    Seguimiento, límites, sacudidas y zoom por velocidad para cámaras ortográficas.

    [:octicons-arrow-right-24: Cámara](camera/index.md)

-   :material-tune:{ .lg .middle } __Managers__

    ---

    Estado del juego, nivel, puntaje, inventario, pausa, audio, guardado y transiciones de escena.

    [:octicons-arrow-right-24: Managers](managers/index.md)

-   :material-monitor-dashboard:{ .lg .middle } __UI__

    ---

    Barra de vida, texto de puntaje, menú de pausa y números de daño flotantes.

    [:octicons-arrow-right-24: UI](ui/index.md)

</div>
