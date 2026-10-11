# Verification record

## October 10, 2026

- Content import validation: passed. Eight regions, 22 items, 24 lore records, 12 NPCs, 18 quests, seven dungeons.
- Unity Editor 6000.3.26f1 compile: passed.
- `Hushwake.Editor.Audit.Run`: passed, 230 assertions. Covered authored dungeon prerequisites and reachable boss rooms, all 21 instrument puzzles, rejected wrong/out-of-order puzzle actions, idempotent rewards, campaign completion, save round trip, and unsupported save version rejection.
- These are model and compile checks. Browser gameplay and Web deployment are pending; the audit does not prove a full campaign played through the browser.
- Initial Web export succeeded: 19,741,880 bytes. A final export is required after the subsequent outdoor mechanism and pause-timer improvements.
- Expanded progression/placement audit: passed, 543 assertions, including 24 ordered outdoor mechanisms and clearance around every authored interaction point.
- Browser candidate `deeffde88eaf5bcb3bf5d246624b71c7306b0035`: normal-input start, Mara dialogue/choice, three-station bell completion, realm change, Tidemark entry, first station and rejected out-of-order action worked. Publication was withheld because the console reported stripped `BoxCollider`, `SphereCollider`, and `CapsuleCollider` components; a house also hid the third bell station. Fixes: `Assets/link.xml`, shared `WorldLayout` exclusion points and regression assertions, larger world labels, and immediate passage status refresh. These fixes require a new Web browser replay.

Unity documents runtime primitive stripping dependencies in its [CreatePrimitive reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.CreatePrimitive.html).
- Candidate `d926a8487e3339f5b15c75c33b4fd8c51dcd5551` removed the primitive collider errors and cleared the bell stations, but a clean-origin browser replay found native class 115 stripping errors and missing default menu button styling. Publication remains withheld. The build now disables native engine stripping and uses Low managed stripping; a new browser replay is required. Unity's [stripEngineCode reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings-stripEngineCode.html) documents the native/managed distinction.

## Browser acceptance checklist

- [x] Start from title, normal keyboard movement and follow camera (zoom pending)
- [x] Wake/Hush switch and Pulse instrument use (other selections pending)
- [x] NPC dialogue and response
- [x] Buried bell Pulse and first quest progression
- [x] Dungeon entry, first ordered instrument puzzle, locked/open passage
- [x] Combat attack effects, visible warnings and dodge (kill/recovery pending)
- [x] Journal, inventory, map, pause/resume
- [x] Save, reload and continue in the same browser
- [x] Public production play URL loads without sign-in
- [x] Browser console scan for runtime errors

Corrected candidate `17f0848cd1a1c7d4607863dde01c21dc91d553ef`: Web export passed at 28,966,277 bytes and 543 audit assertions. Fresh browser replay confirmed styled menus, Mara response, all three bell stations and Waterkeeper Note, Hush switch, Tidemark entry, West Float, rejection of East Float before Low Gate, successful gauge sequence, immediate unlocked label/green signal, Silt Gallery entry, attack effects/telegraphs/dodge, paused health at 64, journal/inventory/map, and full reload/Continue restoring Silt Gallery, Hush, quest progress and 64 health. No browser console errors occurred; Unity emitted one persistent-data synchronization deprecation warning. Testing initially encountered background-tab throttling; bringing the tab forward restored approximately 57–60 fps. Temporary focus emulation is removed after testing.

Full seven-dungeon browser playthrough, each boss phase, every optional quest/dialogue branch, every ending, mobile play, and cross-browser compatibility require separate evidence before they can be claimed verified.

## Public deployment

- Play URL: https://hushwake-unity.vercel.app/
- Immutable deployment: https://hushwake-unity-hqbpsdftv-rakeem-cs-projects.vercel.app/
- Vercel project: `hushwake-unity`, `prj_m3qdIGY7wihbkHpmZkzDBymYoSke`; team: `rakeem-cs-projects`, `team_ONQhLqisJWoZvz12akNd92tk`.
- Deployment ID: `dpl_E4h9FQrvLcyy8wQNt64ZXesJswvZ`, production.
- Public `build-info.json` matched source commit `17f0848cd1a1c7d4607863dde01c21dc91d553ef`; WebAssembly response returned HTTP 200 and `application/wasm`.
- Public browser opened without sign-in; title, new journey, Mara dialogue/response, normal movement and bell Pulse quest progression passed at approximately 59 fps. Browser console error scan was empty.
- Local evidence: `.qa/public-hushwake.png` and `.qa/save-reload.png` (excluded from source control).
- Public save/reload/Continue also passed: the bell Pulse quest stage and 100 health remained after a full page reload.

## Detailed graphics release candidate

Source `6f606e860833720962883c2fc4f2c6c960d1422b`, built 2026-10-11 00:45 UTC: successful Web export, 43,817,265 bytes, 543 progression assertions and 372 art assertions. Fresh browser replay passed the bell reward, Wake/Hush landscape lighting, entrance/room cutaway, gauge sequence and rejected out-of-order action, gallery combat effects/telegraphs/dodge, moving enemy visibility, map/journal/inventory, pause, wheel zoom, and save/full reload/Continue restoring Silt Gallery, Hush, quest stage and 91 health. Browser console errors: none. See `graphics-parity.md` for device-specific frame-rate measurements and visual scope.

Published graphics deployment: `dpl_Cjp853GPFguChLw5jCibvGJnhatd`, immutable URL https://hushwake-unity-lfm6b6fs3-rakeem-cs-projects.vercel.app/ and production alias https://hushwake-unity.vercel.app/. Public `build-info.json` matched the compiled source above. The WebAssembly HEAD response was HTTP 200, `application/wasm`, and `Content-Encoding: gzip`. The existing public save from the initial primitive edition restored with the detailed art, quest stage and 100 health. Normal-input replay completed the remaining bell stations and reward, switched realms, and saved/title/reloaded/continued with Hush, completed bell and 100 health preserved. Public browser console error scan: empty. Local public preview evidence: `.qa/public-hushwake-graphics.png`.

Public GitHub: https://github.com/Rakeem-C/hushwake-unity, verified PUBLIC, story/mechanics-focused description and playable homepage. The restored art/source and verification documents are tracked; generated Web output, local licenses, credentials, import scratch files and QA images are excluded. Documentation-only commits follow the compiled source commit and do not change the deployed player.
