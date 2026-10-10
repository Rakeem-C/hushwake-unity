using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hushwake.Editor
{
    public static class Audit
    {
        static int checks;
        static void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Audit failed: " + message); }
        public static void Run()
        {
            checks = 0;
            var catalog = Content.Load();
            Check(catalog.regions.Length == 8 && catalog.dungeons.Length == 7, "complete source catalog");
            var reserved = WorldLayout.InteractionPoints(catalog, .17f);
            foreach (var house in WorldLayout.Houses(catalog, .17f).Values.SelectMany(h => h)) Check(reserved.All(point => WorldLayout.Distance(point, house) >= 5), "settlement geometry leaves all authored interaction points clear");
            var p = new Progress();
            Check(p.Meets("ability:pulse") && !p.Meets("ability:flow"), "initial abilities");
            Check(p.Meets("not:ability:flow"), "negative requirement");
            p.Flag("tutorial:pulse");
            foreach (var d in catalog.dungeons)
            {
                Check(p.MeetsAll(d.prerequisite), "dungeon prerequisites are satisfiable in campaign order: " + d.id);
                p.Event("enter-region", d.region); p.Event("enter-dungeon", d.id);
                var reachable = new HashSet<string> { d.rooms[0].id };
                var madeProgress = true; var attempts = 0;
                while (madeProgress && attempts++ < 100)
                {
                    madeProgress = false;
                    foreach (var puzzle in d.puzzles)
                    {
                        var stepIndex = p.PuzzleStep(puzzle); if (stepIndex >= puzzle.steps.Length) continue;
                        var step = puzzle.steps[stepIndex]; var room = string.IsNullOrEmpty(step.roomId) ? puzzle.roomId : step.roomId;
                        if (!reachable.Contains(room)) continue;
                        Check(!p.SolveStep(puzzle, stepIndex + 1, step.verb), "out-of-order puzzle step rejected");
                        Check(!p.SolveStep(puzzle, stepIndex, "invalid"), "wrong instrument rejected");
                        if (step.verb == "wake" || step.verb == "hush") p.State.realm = step.verb;
                        Check(p.SolveStep(puzzle, stepIndex, step.verb), "legal station can be completed: " + step.target);
                        madeProgress = true;
                    }
                    if (d.miniboss != null && reachable.Contains(d.miniboss.roomId) && !p.State.flags.Contains("enemy:" + d.id + ":" + d.miniboss.id))
                    { p.Flag("enemy:" + d.id + ":" + d.miniboss.id); madeProgress = true; }
                    foreach (var link in d.links)
                    {
                        if (!p.LinkOpen(d, link)) continue;
                        if (reachable.Contains(link.from) && reachable.Add(link.to)) madeProgress = true;
                        if (!link.oneWay && reachable.Contains(link.to) && reachable.Add(link.from)) madeProgress = true;
                    }
                }
                Check(reachable.Contains(d.boss.roomId), "boss chamber reachable by authored machinery: " + d.id);
                Check(d.puzzles.All(q => p.State.puzzles.Contains(q.id)), "all authored puzzles solved: " + d.id);
                p.Defeat(d); p.UpdateQuests(catalog);
                Check(p.State.bosses.Contains(d.boss.id), "boss and instrument reward recorded");
                var count = p.State.items.Count + p.State.abilities.Count; p.Defeat(d);
                Check(p.State.items.Count + p.State.abilities.Count == count, "boss rewards idempotent");
            }
            p.Flag("ending-decided"); p.UpdateQuests(catalog);
            Check(p.State.flags.Contains("campaign-complete"), "final campaign choice completes story");
            var restored = new Progress(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(p.State)));
            Check(restored.State.bosses.Count == 7 && restored.State.puzzles.Count == 21 && restored.State.flags.Contains("campaign-complete"), "save round trip preserves progression");
            foreach (var site in catalog.regions.SelectMany(r => r.sites))
            {
                var mechanism = catalog.siteMechanisms.Single(m => m.id == site.id);
                Check(mechanism.steps.Length >= 2, "outdoor site uses authored ordered steps: " + site.id);
                for (var i = 0; i < mechanism.steps.Length; i++)
                {
                    var step = mechanism.steps[i];
                    Check(!p.SolveSite(site, mechanism, i + 1, step.verb), "outdoor out-of-order action rejected");
                    Check(!p.SolveSite(site, mechanism, i, "invalid"), "outdoor wrong instrument rejected");
                    if (step.verb == "wake" || step.verb == "hush") p.State.realm = step.verb;
                    Check(p.SolveSite(site, mechanism, i, step.verb), "legal outdoor mechanism completes: " + site.id);
                }
                Check(p.State.events.Contains("solve-puzzle:" + site.id), "outdoor puzzle emits quest completion");
                Check(p.State.flags.Contains("site:" + site.id), "outdoor explored flag saved");
                var count = p.State.items.Count;
                Check(!p.SolveSite(site, mechanism, mechanism.steps.Length - 1, mechanism.steps.Last().verb) && p.State.items.Count == count, "outdoor reward remains one-time");
            }
            var rejected = false;
            try { new Progress(new SaveData { version = 99 }); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "unsupported save version rejected");
            Debug.Log("HUSHWAKE_AUDIT_SUCCEEDED checks=" + checks);
        }
    }
}
