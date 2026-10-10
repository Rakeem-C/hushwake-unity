using System;
using UnityEngine;

namespace Hushwake
{
    [Serializable] public class Point { public float x, z; }
    [Serializable] public class Catalog { public int schema; public string sourceCommit; public Region[] regions; public Item[] items; public Lore[] lore; public Npc[] npcs; public Quest[] quests; public Dungeon[] dungeons; }
    [Serializable] public class Region { public string id, name, visual, musicCue, rumor; public Point center; public Settlement settlement; public Site[] sites; }
    [Serializable] public class Settlement { public string name; public float x, z; }
    [Serializable] public class Item { public string id, name, description, type; }
    [Serializable] public class Site { public string id, name, kind, clue, challenge, rewardItem, loreId, realm; public float x, z; public string[] requires; }
    [Serializable] public class Lore { public string id, title, region, source, text, deduction; }
    [Serializable] public class Choice { public string id, text, reply, setsFlag; public string[] requires; }
    [Serializable] public class Beat { public string id; public string[] when, lines; public Choice[] choices; }
    [Serializable] public class Npc { public string id, name, occupation, region, appearance; public float x, z; public Beat[] dialogue; }
    [Serializable] public class Trigger { public string type, target; }
    [Serializable] public class Stage { public string id, text, journal; public Trigger completion; }
    [Serializable] public class Reward { public string itemId, ability, flag; }
    [Serializable] public class Quest { public string id, title, kind, region, introduction, discovery; public int act; public string[] requires; public Stage[] stages; public Reward reward; public Choice[] choices; }
    [Serializable] public class Room { public string id, name, kind, description, realm, loreId, puzzleId; public float x, z, y; public string[] enemyArchetypes; }
    [Serializable] public class Link { public string from, to; public string[] requires; public bool shortcut, oneWay; }
    [Serializable] public class Step { public string target, roomId, verb; public string[] requires; }
    [Serializable] public class Puzzle { public string id, roomId, title, mechanic, clue, completionFlag; public Step[] steps; }
    [Serializable] public class MiniBoss { public string id, roomId, archetype, reveal; }
    [Serializable] public class Phase { public float threshold; public string telegraph; public string[] attacks, vulnerableTo; }
    [Serializable] public class Boss { public string id, name, roomId, silhouette, reward; public Phase[] phases; }
    [Serializable] public class Dungeon { public string id, title, region, rewardAbility, rewardItem, intro, architecture; public Point entrance; public string[] prerequisite, optionalRooms; public Room[] rooms; public Link[] links; public Puzzle[] puzzles; public MiniBoss miniboss; public Boss boss; }

    public static class Content
    {
        public static Catalog Load()
        {
            var resource = Resources.Load<TextAsset>("game-content");
            if (resource == null) throw new InvalidOperationException("The Hushwake content catalog is missing.");
            var catalog = JsonUtility.FromJson<Catalog>(resource.text);
            if (catalog == null || catalog.schema != 1 || catalog.regions == null || catalog.dungeons == null)
                throw new InvalidOperationException("Unsupported Hushwake content catalog.");
            return catalog;
        }
    }
}
