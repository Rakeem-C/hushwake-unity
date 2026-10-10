# Progression invariants

- Dungeon and outdoor puzzle actions must match both the next authored station and its instrument. Reject later stations and wrong verbs without advancing flags.
- Wake/Hush actions require the player's actual current realm; selecting a realm label alone cannot complete a step.
- Instrument ownership is required for action completion. Dungeon and site requirements must be evaluated from the same saved progress.
- The final outdoor mechanism action records `site:<id>`, its reward, recovered lore, and `solve-puzzle:<id>` together. Quest completion must not be inferred from merely approaching or interacting with a site.
- Boss and outdoor rewards are one-time operations. Reloading or revisiting cannot duplicate them.
- Preserve source-specific dialogue consequences such as Sava's toll-debt choice and the Counterking rescue flag.
- Paused menus freeze combat timers as well as movement. They must not expose the player to a delayed hit immediately after resuming.
- Save data is versioned. Reject unsupported or incomplete saves and keep the existing browser data until the user explicitly starts a replacement journey.
- Runtime primitive creation requires explicit linker preservation of BoxCollider, SphereCollider, and CapsuleCollider even when decorative colliders are immediately removed. A successful Editor compile cannot verify Web stripping behavior; replay world creation in a Web player and scan its console.
- This runtime-created scene uses native types in IMGUI's default skin that static scene analysis missed. Keep `stripEngineCode` disabled for the initial release and managed stripping at Low. Any later optimization must replay menu rendering and world creation in a fresh Web browser before release.
- Procedural settlement buildings must leave at least five meters around every authored station, NPC, dungeon entrance, and rest point. Scenery must not hide the third bell station inside a house. Use the same station positions for placement exclusion, interaction, and regression checks.
- Refresh door labels and signal colors immediately after puzzle or guardian progress changes; the visible locked state must match `Progress.LinkOpen`.

`Assets/Editor/Audit.cs` exercises ordered actions, instrument errors, outdoor quest events, one-time rewards, dungeon reachability, and save round trips. Browser proof is required for pause/resume and visible interactions.
