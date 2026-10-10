using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hushwake
{
    [Serializable] public class SaveData
    {
        public int version = 1;
        public string realm = "wake", region = "hearth", dungeon = "", room = "", ending = "";
        public float x = 0, z = 0, health = 100;
        public List<string> flags = new List<string>(), abilities = new List<string> { "pulse" }, items = new List<string>(), bosses = new List<string>(), puzzles = new List<string>(), lore = new List<string>(), events = new List<string>(), visited = new List<string> { "hearth" };
    }

    public sealed class Progress
    {
        public SaveData State { get; private set; }
        public Progress(SaveData data = null)
        {
            State = data ?? new SaveData();
            if (State.version != 1) throw new InvalidOperationException("This save uses an unsupported version.");
            if (State.flags == null || State.abilities == null || State.items == null || State.bosses == null || State.puzzles == null || State.lore == null || State.events == null || State.visited == null)
                throw new InvalidOperationException("The save is incomplete.");
            if (State.realm != "wake" && State.realm != "hush") throw new InvalidOperationException("The save has an invalid realm.");
            if (float.IsNaN(State.x) || float.IsNaN(State.z) || float.IsInfinity(State.x) || float.IsInfinity(State.z)) throw new InvalidOperationException("Invalid saved position.");
            State.health = Mathf.Clamp(State.health, 1, 100);
        }
        public bool Meets(string requirement)
        {
            if (string.IsNullOrEmpty(requirement)) return true;
            var split = requirement.IndexOf(':');
            var kind = split < 0 ? "" : requirement.Substring(0, split);
            var value = split < 0 ? requirement : requirement.Substring(split + 1);
            switch (kind)
            {
                case "ability": return State.abilities.Contains(value);
                case "item": return State.items.Contains(value);
                case "boss": return State.bosses.Contains(value);
                case "puzzle": return State.puzzles.Contains(value);
                case "realm": return State.realm == value;
                case "flag": return State.flags.Contains(value);
                case "not": return !Meets(value);
                default: return State.flags.Contains(requirement) || State.abilities.Contains(requirement) || State.puzzles.Contains(requirement) || State.bosses.Contains(requirement);
            }
        }
        public bool MeetsAll(string[] requirements) => requirements == null || requirements.All(Meets);
        public void Flag(string flag) { if (!string.IsNullOrEmpty(flag) && !State.flags.Contains(flag)) State.flags.Add(flag); Event("set-flag", flag); }
        public void Item(string item) { if (!string.IsNullOrEmpty(item) && !State.items.Contains(item)) State.items.Add(item); Event("obtain-item", item); }
        public void Ability(string ability) { if (!string.IsNullOrEmpty(ability) && !State.abilities.Contains(ability)) State.abilities.Add(ability); Event("gain-ability", ability); }
        public void Event(string type, string target) { var key = type + ":" + target; if (!State.events.Contains(key)) State.events.Add(key); }
        public int PuzzleStep(Puzzle puzzle)
        {
            if (State.puzzles.Contains(puzzle.id)) return puzzle.steps.Length;
            for (var i = 0; i < puzzle.steps.Length; i++) if (!State.flags.Contains("puzzle-step:" + puzzle.id + ":" + i)) return i;
            return puzzle.steps.Length;
        }
        public bool SolveStep(Puzzle puzzle, int index, string verb)
        {
            if (index != PuzzleStep(puzzle) || index >= puzzle.steps.Length) return false;
            var step = puzzle.steps[index];
            if (step.verb != verb || !MeetsAll(step.requires)) return false;
            if ((verb == "wake" || verb == "hush") && State.realm != verb) return false;
            if (verb == "graft" && !State.items.Contains("graft-hook")) return false;
            if (verb == "counter" && !State.items.Contains("mass-dial")) return false;
            if (verb == "concord" && !State.items.Contains("concord-crest")) return false;
            if (new[] { "pulse", "flow", "lift", "witness" }.Contains(verb) && !State.abilities.Contains(verb)) return false;
            Flag("puzzle-step:" + puzzle.id + ":" + index);
            if (index + 1 == puzzle.steps.Length)
            {
                State.puzzles.Add(puzzle.id); Flag(puzzle.completionFlag); Event("solve-puzzle", puzzle.id);
            }
            return true;
        }
        public bool LinkOpen(Dungeon dungeon, Link link)
        {
            if (State.flags.Contains("shortcut:" + dungeon.id + ":" + link.from + ":" + link.to)) return true;
            if (link.shortcut && (link.requires == null || link.requires.Length == 0)) return false;
            if (!MeetsAll(link.requires)) return false;
            var mini = dungeon.miniboss;
            if (mini != null && link.from == mini.roomId && Array.FindIndex(dungeon.rooms, r => r.id == link.to) > Array.FindIndex(dungeon.rooms, r => r.id == mini.roomId) && !State.flags.Contains("enemy:" + dungeon.id + ":" + mini.id)) return false;
            if (link.from == dungeon.boss.roomId && link.to != mini?.roomId && !State.bosses.Contains(dungeon.boss.id)) return false;
            return true;
        }
        public void Defeat(Dungeon dungeon)
        {
            if (State.bosses.Contains(dungeon.boss.id)) return;
            State.bosses.Add(dungeon.boss.id); Event("defeat-boss", dungeon.boss.id);
            if (!string.IsNullOrEmpty(dungeon.rewardAbility)) Ability(dungeon.rewardAbility);
            if (!string.IsNullOrEmpty(dungeon.rewardItem)) Item(dungeon.rewardItem);
        }
        public int QuestStep(Quest quest)
        {
            for (var i = 0; i < quest.stages.Length; i++)
                if (!State.events.Contains(quest.stages[i].completion.type + ":" + quest.stages[i].completion.target)) return i;
            return quest.stages.Length;
        }
        public void UpdateQuests(Catalog catalog)
        {
            foreach (var quest in catalog.quests)
            {
                if (!MeetsAll(quest.requires) || QuestStep(quest) != quest.stages.Length || State.flags.Contains("quest:" + quest.id)) continue;
                Flag("quest:" + quest.id);
                if (quest.reward != null) { Item(quest.reward.itemId); Ability(quest.reward.ability); Flag(quest.reward.flag); }
            }
        }
    }
}
