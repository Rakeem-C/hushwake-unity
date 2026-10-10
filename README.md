# Hushwake — Unity Edition

A separate Unity rebuild of Hushwake, the fantasy adventure following Nera Vale through the Wake and Hush realms.

**Status: gameplay implementation is awaiting its first Unity compile and browser verification. No playable Unity deployment has been published yet.**

The C# implementation includes a procedural world, movement, combat, instrument puzzles, dialogue, journal, map, inventory, seven dungeon graphs, and versioned browser saves. These systems still require Unity compilation and browser verification. The original Three.js visuals, score, detailed physical machinery, and systemic simulation are not reproduced by this first Unity edition.

## Toolchain

- Unity 6.3 LTS, Editor `6000.3.26f1`
- Web Build Support module
- An activated Unity license
- Web export hosted as static files on Vercel

## Content provenance

`Assets/Resources/game-content.json` imports the original regions, NPCs, quests, items, lore, and dungeon blueprints from Hushwake's ChatGPT Sites version 8, source commit `ac071c727210da99b34d654be92b38311698c58d`. The import script reads the original project without changing it. Engine-specific TypeScript and Three.js code are not represented as Unity gameplay.

## Release requirement

Publish only after Unity compilation and a browser pass verifies starting a journey, movement and camera control, combat, dungeon progression, pause, and save/reload. The public playable link will be added here after deployment is confirmed.

Credentials, local Unity license files, and browser profiles must stay outside this public repository.

## Local validation and build

```text
node Tools/validate-content.mjs
Unity.exe -batchmode -quit -projectPath <project> -executeMethod Hushwake.Editor.Audit.Run -logFile <audit-log>
Unity.exe -batchmode -quit -projectPath <project> -executeMethod Hushwake.Editor.BuildWeb.Run -logFile <build-log>
```

The Unity build is generated locally in `Build/Web`, which is excluded from Git. Deploy those static files to Vercel after ordinary keyboard/mouse browser verification.
