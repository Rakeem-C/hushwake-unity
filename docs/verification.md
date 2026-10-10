# Verification record

## October 10, 2026

- Content import validation: passed. Eight regions, 22 items, 24 lore records, 12 NPCs, 18 quests, seven dungeons.
- Unity Editor 6000.3.26f1 compile: passed.
- `Hushwake.Editor.Audit.Run`: passed, 230 assertions. Covered authored dungeon prerequisites and reachable boss rooms, all 21 instrument puzzles, rejected wrong/out-of-order puzzle actions, idempotent rewards, campaign completion, save round trip, and unsupported save version rejection.
- These are model and compile checks. Browser gameplay and Web deployment are pending; the audit does not prove a full campaign played through the browser.
- Initial Web export succeeded: 19,741,880 bytes. A final export is required after the subsequent outdoor mechanism and pause-timer improvements.

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
