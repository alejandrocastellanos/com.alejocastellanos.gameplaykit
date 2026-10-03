---
title: Guías
description: Guías paso a paso para hacer juegos de plataformas y top-down con Gameplay Kit.
---

# Guías

Las páginas de componentes explican qué hace cada pieza; estas guías muestran cómo combinarlas para armar un
juego. Empieza por **Primeros pasos**, sigue el camino de tu estilo de juego (plataformas o top-down) y
después usa las guías comunes cuando las necesites.

## Para empezar

[Primeros pasos](getting-started.md)
:   Instala el paquete y conoce los dos menús del kit: **GameplayKit** en la barra de menús principal (con
    las escenas demo) y **GameObject → GameplayKit** / clic derecho en la jerarquía. Cada opción indica si
    es para plataformas, para top-down o para los dos.

[Input](input.md)
:   Cómo leen el input las habilidades a través de `ICharacterInput`, qué asigna por defecto
    `KeyboardInputReader`, cómo funciona el kit con el Input System, con el Input Manager clásico o con
    ambos, y cómo conectar tu propia fuente de input (IA, replays, `PlayerInput`).

## Juego de plataformas

Vista lateral con gravedad: correr, saltar, paredes, escaleras y agua.

[Juego de plataformas](platformer-game.md)
:   El punto de partida: la demo de plataformas recorrida de izquierda a derecha, los controles de
    plataformas y tu primer nivel en cinco minutos.

[Personaje de plataformas](platformer-character.md)
:   Arma el personaje a mano: caminar y correr, saltar y multi-saltar, dashes, paredes, cornisas,
    escaleras, agua y plataformas de un sentido, qué habilidades no conviene combinar y cómo ajustar el
    salto.

## Juego top-down

Vista desde arriba sin gravedad: movimiento en 8 direcciones, apuntar con el mouse y disparar.

[Juego top-down](top-down-game.md)
:   El punto de partida: el dungeon demo con su mapa sala por sala, los controles top-down y tu primera
    sala en cinco minutos.

[Personaje top-down](top-down-character.md)
:   `PlayerTopDownMovement` y el override de gravedad, dash en 8 direcciones, ataques en cuatro
    direcciones, disparar hacia el mouse con dos armas, la cámara y los enemigos que se mueven en los dos
    ejes.

## Para los dos estilos

[Combate](combat.md)
:   `PlayerAttack` y las armas: hitboxes melee, hitscan, proyectiles, combos, ataques cargados, cambio de
    arma y apuntado con mouse o stick derecho.

[Vida y daño](health-and-damage.md)
:   `CharacterHealth` y `DamageableObject`, invulnerabilidad, knockback, aturdimiento, muerte, respawn, vidas
    y game over, además del feedback con `DamageFlash` y los números de daño.

[Enemigos e IA](enemies-and-ai.md)
:   Comportamientos de enemigo listos para usar frente a la máquina de estados `AIBrain`. Un ejemplo
    completo de un enemigo que patrulla, te persigue cuando te ve y huye cuando está herido. También cubre
    `EnemySpawner` y cómo escribir tus propias acciones y decisiones.

[Construir un nivel](building-a-level.md)
:   Managers (juego, nivel, puntaje, inventario, pausa, audio, guardado, transiciones de escena), cámara,
    piezas de entorno, checkpoints y respawn, llaves y puertas, salida del nivel y HUD.

[Habilidades propias](custom-abilities.md)
:   Cómo ejecuta `CharacterCore` las habilidades por fases, qué te dan `AbilityBase` y
    `CharacterController2D` (gravedad compartida, bloqueo del control, suspensión) y una habilidad nueva
    completa, escrita paso a paso.

[Solución de problemas](troubleshooting.md)
:   Los sospechosos de siempre: personajes que atraviesan el suelo, suelo que no se detecta, habilidades o
    input que no responden, Missing Script, enemigos que se caen de las plataformas, cámara que tiembla,
    capas y tags, y pruebas que no aparecen.
