# Hushwake

**[Play Hushwake in your browser](https://hushwake-unity.vercel.app/)** — keyboard and mouse recommended.

Follow Nera Vale through a world caught between Wake and Hush. Beneath its settlements, old civic machines still enforce decisions their people never finished making. Trace the missing records, restore the regional instruments, and return the final choice to the city.

## Explore, listen, and change the world

- Explore eight regions, from the Chalkmere Terraces to the Hollow Crown, with settlements, hidden records, and people whose accounts change as your journey unfolds.
- Turn between Wake and Hush to uncover another side of the world and operate machinery that remembers both.
- Fight with the threadblade, dodge telegraphed attacks, and use Pulse, Flow, Lift, Witness, Graft, Counter, and Concord to solve instrument puzzles.
- Follow seven dungeon routes through guarded passages, ordered puzzle stations, and bosses with changing attack phases.
- Explore 24 outdoor mechanisms with ordered actions, realm changes, and one-time rewards.
- Keep a journal of quests and recovered lore, collect regional tools, and return to visited settlements through the map.
- Choose how the city records its future, then continue exploring. Your journey saves locally in your browser.

## Development status

The playable edition includes the original detailed characters, regional buildings, tree canopies, dungeon architecture, equipment and enemy models, with textured ground, river surfaces, rain and distinct Wake/Hush lighting. Compilation, 543 progression/placement assertions, 372 art checks, and an ordinary-input browser replay passed, including the bell, first dungeon puzzle, combat controls, menus, camera zoom and save/reload. The full seven-dungeon campaign, every boss phase, optional branch and ending still need browser playthroughs. The original score and complete physical machinery/systemic simulation are not reproduced in this edition. Frame rate depends on hardware; the detailed art is substantially heavier than the initial primitive prototype.

## Toolchain

- Unity 6.3 LTS, Editor `6000.3.26f1`
- Web Build Support module
- An activated Unity license
- Web export hosted as static files on Vercel

## Content provenance

`Assets/Resources/game-content.json` imports the original regions, NPCs, quests, items, lore, and dungeon blueprints from Hushwake's ChatGPT Sites version 8, source commit `ac071c727210da99b34d654be92b38311698c58d`. `Tools/ExportArt.ts` and the Unity importer preserve 240 original models, 2,360 meshes, 385 materials and 59 textures, including normals, UVs, vertex colors and animated limb hierarchy. The import scripts read the original project without changing it. Engine-specific TypeScript and Three.js code are not represented as Unity gameplay. Cormorant Garamond's bundled OFL license covers the menu font.

## Release requirement

Future releases require Unity compilation and a browser pass verifying starting a journey, movement and camera control, combat, dungeon progression, pause, and save/reload. The graphics player was built from source commit `6f606e860833720962883c2fc4f2c6c960d1422b`; subsequent documentation updates are recorded separately in Git. Its public `build-info.json` identifies the compiled source. See `docs/graphics-parity.md` and `docs/rendering-guardrails.md` for the comparison and retained failure cases.

Credentials, local Unity license files, and browser profiles must stay outside this public repository.

## Local validation and build

```text
node Tools/validate-content.mjs
Unity.exe -batchmode -quit -projectPath <project> -executeMethod Hushwake.Editor.Audit.Run -logFile <audit-log>
Unity.exe -batchmode -quit -projectPath <project> -executeMethod Hushwake.Editor.BuildWeb.Run -logFile <build-log>
```

The Unity build is generated locally in `Build/Web`, which is excluded from Git. Deploy those static files to Vercel after ordinary keyboard/mouse browser verification.
