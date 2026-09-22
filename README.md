# Gameplay Kit

Kit modular de scripts de gameplay para Unity: habilidades de personaje, IA de enemigos e
interactuables que se agregan por componente ("Add Component"), sin escribir código para
combinarlos.

## Estado

En desarrollo. La capa de núcleo (arquitectura base) está implementada; el resto del catálogo
de scripts (movimiento, combate, IA, entorno, cámara, managers) se agrega incrementalmente.

## Arquitectura

- `CharacterCore` — orquestador central del personaje: descubre las `AbilityBase` presentes en
  el GameObject y corre su ciclo de vida cada frame.
- `AbilityBase` — clase base de la que heredan todas las habilidades de movimiento/combate.
- `CharacterStateMachine<T>` — máquina de estados genérica (se usa para `MovementState` y
  `ConditionState`).
- `CharacterController2D` — wrapper sobre `Rigidbody2D` con detección de suelo.
- `ICharacterInput` / `KeyboardInputReader` — capa de entrada desacoplada de las habilidades.
- `AIBrain` / `AIState` / `AIActionBase` / `AIDecisionBase` — máquina de estados de IA armable
  desde el Inspector combinando acciones y decisiones.

## Instalación (una vez esté en un repo remoto)

En el `Packages/manifest.json` del proyecto consumidor:

```
"com.alejocastellanos.gameplaykit": "https://github.com/<usuario>/<repo>.git#v0.1.0"
```

## Desarrollo local

Este repo vive fuera de cualquier proyecto de Unity. Para probarlo, agrégalo como paquete local
en el `Packages/manifest.json` de un proyecto de Unity (ajusta la ruta relativa):

```
"com.alejocastellanos.gameplaykit": "file:../com.alejocastellanos.gameplaykit"
```

Unity lo tratará como un paquete instalado, pero editando estos archivos directamente.
