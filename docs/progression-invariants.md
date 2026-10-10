# Progression invariants

- Dungeon and outdoor puzzle actions must match both the next authored station and its instrument. Reject later stations and wrong verbs without advancing flags.
- Wake/Hush actions require the player's actual current realm; selecting a realm label alone cannot complete a step.
- Instrument ownership is required for action completion. Dungeon and site requirements must be evaluated from the same saved progress.
- The final outdoor mechanism action records `site:<id>`, its reward, recovered lore, and `solve-puzzle:<id>` together. Quest completion must not be inferred from merely approaching or interacting with a site.
- Boss and outdoor rewards are one-time operations. Reloading or revisiting cannot duplicate them.
- Preserve source-specific dialogue consequences such as Sava's toll-debt choice and the Counterking rescue flag.
- Paused menus freeze combat timers as well as movement. They must not expose the player to a delayed hit immediately after resuming.
- Save data is versioned. Reject unsupported or incomplete saves and keep the existing browser data until the user explicitly starts a replacement journey.

`Assets/Editor/Audit.cs` exercises ordered actions, instrument errors, outdoor quest events, one-time rewards, dungeon reachability, and save round trips. Browser proof is required for pause/resume and visible interactions.
