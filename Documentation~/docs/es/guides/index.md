---
title: Guías
description: Guías paso a paso para hacer juegos con Gameplay Kit.
---

# Guías

Las páginas de componentes explican qué hace cada pieza; estas guías muestran cómo combinarlas para armar un
personaje, una pelea o un nivel completo. Si recién empiezas con el kit, léelas más o menos en este orden.

## Para empezar

[Primeros pasos](getting-started.md)
:   Instala el paquete, abre la escena demo desde **GameplayKit → Create Demo Scene** y arma tu primer
    personaje desde un GameObject vacío. También repasa los controles por defecto y el menú **GameplayKit**,
    que crea jugadores, enemigos, cámaras, managers y HUD listos para usar.

[Input](input.md)
:   Cómo leen el input las habilidades a través de `ICharacterInput`, qué asigna por defecto
    `KeyboardInputReader`, cómo funciona el kit con el Input System, con el Input Manager clásico o con
    ambos, y cómo conectar tu propia fuente de input (IA, replays, `PlayerInput`).

## Personajes

[Personaje de plataformas](platformer-character.md)
:   Un personaje de vista lateral: caminar y correr, saltar y multi-saltar, dashes, paredes, cornisas,
    escaleras, agua y plataformas de un sentido, y qué habilidades no conviene combinar.

[Personaje top-down](top-down-character.md)
:   Un personaje visto desde arriba con `PlayerTopDownMovement`, dash en 8 direcciones, interacción y
    ataques, sin gravedad.

## Combate

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

## Niveles

[Construir un nivel](building-a-level.md)
:   Managers (juego, nivel, puntaje, inventario, pausa, audio, guardado, transiciones de escena), cámara,
    piezas de entorno, checkpoints y respawn, llaves y puertas, salida del nivel y HUD, con un recorrido por
    la escena demo.

## Extender el kit

[Habilidades propias](custom-abilities.md)
:   Cómo ejecuta `CharacterCore` las habilidades por fases, qué te dan `AbilityBase` y
    `CharacterController2D` (gravedad compartida, bloqueo del control, suspensión) y una habilidad nueva
    completa, escrita paso a paso.

[Solución de problemas](troubleshooting.md)
:   Los sospechosos de siempre: personajes que atraviesan el suelo, suelo que no se detecta, habilidades o
    input que no responden, Missing Script, enemigos que se caen de las plataformas, cámara que tiembla,
    capas y tags, y pruebas que no aparecen.
