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

- [ ] Start from title, normal keyboard movement, camera and zoom
- [ ] Wake/Hush switch and instrument selection
- [ ] NPC dialogue and response
- [ ] Buried bell Pulse and first quest progression
- [ ] Dungeon entry, first ordered instrument puzzle, locked/open passage
- [ ] Combat, visible attack warnings, dodge and health/recovery
- [ ] Journal, inventory, map, pause/resume
- [ ] Save, reload and continue in the same browser
- [ ] Public production play URL loads without sign-in
- [ ] Browser console scan for runtime errors

Full seven-dungeon browser playthrough, each boss phase, every optional quest/dialogue branch, every ending, mobile play, and cross-browser compatibility require separate evidence before they can be claimed verified.
