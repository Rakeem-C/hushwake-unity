# Hushwake

Follow Nera Vale through a world caught between Wake and Hush. Beneath its settlements, old civic machines still enforce decisions their people never finished making. Trace the missing records, restore the regional instruments, and return the final choice to the city.

## Explore, listen, and change the world

- Explore eight regions, from the Chalkmere Terraces to the Hollow Crown, with settlements, hidden records, and people whose accounts change as your journey unfolds.
- Turn between Wake and Hush to uncover another side of the world and operate machinery that remembers both.
- Fight with the threadblade, dodge telegraphed attacks, and use Pulse, Flow, Lift, Witness, Graft, Counter, and Concord to solve instrument puzzles.
- Follow seven dungeon routes through guarded passages, ordered puzzle stations, and bosses with changing attack phases.
- Keep a journal of quests and recovered lore, collect regional tools, and return to visited settlements through the map.
- Choose how the city records its future, then continue exploring. Your journey saves locally in your browser.

## Development status

The game implementation is awaiting its first compile and browser verification. A playable link will be added after the release passes those checks. The original detailed physical machinery, score, and systemic simulation are not reproduced in this edition.

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
