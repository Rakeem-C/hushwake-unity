using System;
using System.Linq;
using UnityEngine;

namespace Hushwake
{
    public sealed partial class Game
    {
        GUIStyle heading, title, body, small, button;
        bool uiReady;
        Texture2D Solid(Color color){var t=new Texture2D(1,1);t.SetPixel(0,0,color);t.Apply();return t;}
        void InitUI()
        {
            if (uiReady) return;
            heading = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold, wordWrap = true }; heading.normal.textColor = new Color(.96f, .81f, .5f);
            title = new GUIStyle(heading) { fontSize = 74, alignment = TextAnchor.MiddleLeft };title.normal.textColor=new Color(.94f,.91f,.80f);
            var serif=Resources.Load<Font>("Typography/CormorantGaramond");if(serif!=null){title.font=serif;heading.font=serif;}
            body = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true }; body.normal.textColor = new Color(.9f, .92f, .85f);
            small = new GUIStyle(body) { fontSize = 13 };
            button = new GUIStyle(GUI.skin.button) { fontSize = 17, wordWrap = true, padding = new RectOffset(12, 12, 9, 9) };
            button.normal.background=Solid(new Color(.10f,.17f,.18f,.96f));button.hover.background=Solid(new Color(.63f,.54f,.36f,1));button.active.background=Solid(new Color(.78f,.67f,.45f,1));button.normal.textColor=new Color(.94f,.90f,.77f);button.hover.textColor=new Color(.98f,.96f,.88f);button.active.textColor=Color.white;
            uiReady = true;
        }
        void Backdrop(Rect rect, float alpha = .93f)
        {
            var previous = GUI.color; GUI.color = new Color(.018f, .040f, .045f, alpha); GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = previous;
        }
        void OnGUI()
        {
            InitUI(); var scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f); if (scale <= 0) return;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
            var width = Screen.width / scale; var height = Screen.height / scale;
            if (catalog == null) { Backdrop(new Rect(0, 0, width, height)); GUI.Label(new Rect(40, 40, width - 80, 200), saveError, heading); return; }
            if (!started)
            {
                Backdrop(new Rect(0, 0, width, height), .20f);Backdrop(new Rect(0,0,540,height),.92f);
                GUI.Label(new Rect(55,85,450,28),"A WORLD BETWEEN THE HELD NOTES",small);
                GUI.Label(new Rect(50,135,640,105), "HUSHWAKE", title);
                GUI.Label(new Rect(55,255,390,80), "Follow the sound\nthat should have faded.", new GUIStyle(body) { fontSize=25,fontStyle=FontStyle.Italic });
                var x = 55f; var y = height * .56f;
                if (GUI.Button(new Rect(x, y, 390, 50), "Begin a new journey     →", button)) { if (PlayerPrefs.HasKey(SaveKey)) panel = "replace"; else StartJourney(false); }
                GUI.enabled = PlayerPrefs.HasKey(SaveKey);
                if (GUI.Button(new Rect(x, y + 62, 390, 50), "Continue your journey", button)) StartJourney(true);
                GUI.enabled = true;
                GUI.Label(new Rect(55,height-140,400,100),"Explore lost records. Restore the instruments.\nReturn the choice.\n\nKeyboard and mouse · Your journey saves here.",small);
                if (!string.IsNullOrEmpty(saveError)) GUI.Label(new Rect(40, height - 190, width - 80, 70), saveError, body);
                if (panel == "replace")
                {
                    var box = new Rect(width / 2 - 270, height / 2 - 110, 540, 230); Backdrop(box);
                    GUI.Label(new Rect(box.x + 25, box.y + 20, 490, 80), "Starting over replaces this browser’s saved journey. Continue?", body);
                    if (GUI.Button(new Rect(box.x + 25, box.y + 120, 230, 50), "Start over", button)) StartJourney(false);
                    if (GUI.Button(new Rect(box.x + 285, box.y + 120, 230, 50), "Keep my journey", button)) panel = "";
                }
                return;
            }

            Backdrop(new Rect(18, 18, 350, 95), .87f);
            var location = dungeon == null ? catalog.regions.First(r => r.id == progress.State.region).name : dungeon.title + " · " + room.name;
            GUI.Label(new Rect(32, 23, 325, 28), "NERA  /  " + progress.State.realm.ToUpperInvariant(), heading);
            GUI.Label(new Rect(32, 62, 320, 38), location, small);
            var healthRect = new Rect(32, 103, 320, 7); GUI.color = new Color(.2f, .25f, .26f); GUI.DrawTexture(healthRect, Texture2D.whiteTexture); GUI.color = new Color(.8f, .45f, .28f); GUI.DrawTexture(new Rect(healthRect.x, healthRect.y, healthRect.width * Mathf.Clamp01(progress.State.health / 100), healthRect.height), Texture2D.whiteTexture); GUI.color = Color.white;
            Backdrop(new Rect(width - 232, 18, 214, 62), .87f); GUI.Label(new Rect(width - 218, 27, 190, 45), Pretty(selected) + "  [R]\n" + progress.State.health.ToString("0") + " health  ·  " + fps.ToString("0") + " fps", small);
            var main = catalog.quests.FirstOrDefault(q => q.kind == "main" && progress.MeetsAll(q.requires) && progress.QuestStep(q) < q.stages.Length);
            if (main != null)
            {
                Backdrop(new Rect(width-368, 108, 350, 107), .87f); GUI.Label(new Rect(width-354, 119, 320, 26), main.title, body);
                GUI.Label(new Rect(width-354, 151, 318, 55), main.stages[progress.QuestStep(main)].text + "  [J]", small);
            }
            if (nearest != null && string.IsNullOrEmpty(panel))
            {
                var prompt = "[E] " + nearest.name;
                if (nearest.kind == "station")
                {
                    var step = nearest.puzzle.steps[nearest.step];
                    prompt = Pretty(step.target) + " · " + ((step.verb == "interact" || step.verb == "wake" || step.verb == "hush") ? "[E] " : "[R] ") + Pretty(step.verb);
                    if (progress.PuzzleStep(nearest.puzzle) > nearest.step) prompt += " · Complete";
                }
                if (nearest.kind == "site")
                {
                    var step = nearest.mechanism.steps[nearest.step];
                    prompt = nearest.name + " · " + ((step.verb == "interact" || step.verb == "wake" || step.verb == "hush") ? "[E] " : "[R] ") + Pretty(step.verb);
                    if (progress.SiteStep(nearest.mechanism) > nearest.step) prompt += " · Complete";
                }
                Backdrop(new Rect(width / 2 - 285, height - 133, 570, 46), .84f); GUI.Label(new Rect(width / 2 - 270, height - 125, 540, 35), prompt, new GUIStyle(body) { alignment = TextAnchor.MiddleCenter });
            }
            if (Time.unscaledTime < noticeUntil && !string.IsNullOrEmpty(notice))
            {
                Backdrop(new Rect(width / 2 - 365, height - 230, 730, 83), .9f); GUI.Label(new Rect(width / 2 - 348, height - 222, 696, 70), notice, body);
            }
            GUI.Label(new Rect(20, height - 38, width - 40, 28), "E  Interact    R  Instrument    Q  Wake / Hush                 M  Map    J  Journal    I  Inventory    Esc  Pause", small);
            foreach (var foe in foes.Where(f => f.boss))
            {
                Backdrop(new Rect(width / 2 - 260, 20, 520, 72), .86f); GUI.Label(new Rect(width / 2 - 248, 28, 496, 25), foe.name, new GUIStyle(body) { alignment = TextAnchor.MiddleCenter });
                GUI.color = new Color(.79f, .3f, .37f); GUI.DrawTexture(new Rect(width / 2 - 240, 64, 480 * foe.health / foe.maxHealth, 9), Texture2D.whiteTexture); GUI.color = Color.white;
            }
            if (!string.IsNullOrEmpty(saveError)) GUI.Label(new Rect(25, height - 80, width - 50, 36), saveError, body);
            if (dead)
            {
                Backdrop(new Rect(0, 0, width, height), .78f); GUI.Label(new Rect(width / 2 - 260, height / 2 - 80, 520, 50), "The thread holds.", heading);
                if (GUI.Button(new Rect(width / 2 - 140, height / 2, 280, 50), "Rest and try again", button)) { dead = false; progress.State.health = 100; if (dungeon != null) LoadRoom(room.id); else BuildWorld(Atlas(catalog.regions.First(r => r.id == progress.State.region).settlement.x, catalog.regions.First(r => r.id == progress.State.region).settlement.z)); Save(); }
                return;
            }
            if (string.IsNullOrEmpty(panel)) return;
            DrawPanel(width, height);
        }

        void DrawPanel(float width, float height)
        {
            Backdrop(new Rect(0, 0, width, height), .58f);
            var rect = new Rect(width / 2 - 430, 55, 860, height - 110); Backdrop(rect);
            GUILayout.BeginArea(new Rect(rect.x + 26, rect.y + 20, rect.width - 52, rect.height - 40));
            GUILayout.BeginHorizontal(); GUILayout.Label(panel == "dialogue" ? dialogueTitle : Pretty(panel), heading,GUILayout.MinWidth(500)); GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", button, GUILayout.Width(110))) panel = ""; GUILayout.EndHorizontal(); GUILayout.Space(18);
            journalScroll = GUILayout.BeginScrollView(journalScroll);
            switch (panel)
            {
                case "pause":
                    GUILayout.Label("Your journey is saved automatically in this browser. Clearing site data removes the save.", body); GUILayout.Space(15);
                    if (GUILayout.Button("Save and resume", button)) { Save(); panel = ""; }
                    if (GUILayout.Button("Map", button)) panel = "map";
                    if (GUILayout.Button("Journal", button)) panel = "journal";
                    if (GUILayout.Button("Inventory and instruments", button)) panel = "inventory";
                    if (GUILayout.Button("Save and return to title", button)) { Save(); started = false; panel = ""; }
                    if (GUILayout.Button("Lighting: "+(detailedLighting?"detailed":"lighter")+" — change",button))ConfigureGraphics(detailedLighting?"balanced":"detailed");
                    GUILayout.Space(24); GUILayout.Label("Controls", heading);
                    GUILayout.Label("WASD / arrows: move • Shift: run • Space: dodge\nE: interact • F / left click: threadblade • R: selected instrument\nQ: turn Wake / Hush • Mouse wheel: zoom\n1 Pulse • 2 Flow • 3 Lift • 4 Witness • 5 Graft • 6 Counter • 7 Concord\nM Map • J Journal • I Inventory • Esc Pause", body); break;
                case "dialogue":
                    GUILayout.Label(dialogueText, body); GUILayout.Space(22);
                    if (dialogueChoices != null) foreach (var choice in dialogueChoices)
                        if (progress.MeetsAll(choice.requires) && GUILayout.Button(choice.text, button)) { progress.Flag(choice.setsFlag); if (dialogueNpc == "sava" && (choice.id == "erase" || choice.id == "preserve")) progress.Flag("toll-debt-cleared"); dialogueText = choice.reply; dialogueChoices = null; progress.UpdateQuests(catalog); Save(); break; }
                    break;
                case "inventory":
                    GUILayout.Label("Instruments", heading);
                    foreach (var ability in progress.State.abilities) if (GUILayout.Button(Pretty(ability) + " — select", button)) { selected = ability; panel = ""; }
                    GUILayout.Space(15); GUILayout.Label("What you carry", heading);
                    foreach (var itemId in progress.State.items)
                    {
                        var item = catalog.items.FirstOrDefault(i => i.id == itemId); if (item == null) continue;
                        GUILayout.Label(item.name, body); GUILayout.Label(item.description, small); GUILayout.Space(10);
                    }
                    if (progress.State.items.Count == 0) GUILayout.Label("Find records and instruments in the world.", body);
                    break;
                case "journal":
                    foreach (var quest in catalog.quests.Where(q => progress.MeetsAll(q.requires)))
                    {
                        var step = progress.QuestStep(quest); GUILayout.Label(quest.title + (step == quest.stages.Length ? " ✓" : ""), heading);
                        GUILayout.Label(quest.introduction, small);
                        if (step < quest.stages.Length) { GUILayout.Label(quest.stages[step].text, body); GUILayout.Label(quest.stages[step].journal, small); }
                        GUILayout.Space(20);
                    }
                    if (dungeon != null)
                    {
                        GUILayout.Label("Chamber machinery", heading);
                        foreach (var puzzle in dungeon.puzzles) { GUILayout.Label(puzzle.title + ": " + NextPuzzleHint(puzzle), body); GUILayout.Label(puzzle.clue, small); GUILayout.Space(12); }
                    }
                    GUILayout.Label("Records found", heading);
                    foreach (var loreId in progress.State.lore) { var lore = catalog.lore.FirstOrDefault(l => l.id == loreId); if (lore != null) { GUILayout.Label(lore.title, body); GUILayout.Label(lore.text, small); GUILayout.Space(12); } }
                    break;
                case "map":
                    GUILayout.Label(dungeon != null ? "Finish or leave this dungeon to travel between settlements." : "Travel to a settlement you have visited, or walk there using the directions below.", body); GUILayout.Space(15);
                    foreach (var region in catalog.regions)
                    {
                        var delta = Atlas(region.center.x, region.center.z) - player.position;
                        var direction = (delta.z > 4 ? "north " : delta.z < -4 ? "south " : "") + (delta.x > 4 ? "east" : delta.x < -4 ? "west" : "");
                        GUILayout.BeginHorizontal(); GUILayout.Label(region.name + " • " + region.settlement.name + "\n" + direction + " · " + delta.magnitude.ToString("0") + "m", body);
                        GUI.enabled = dungeon == null && progress.State.visited.Contains(region.id);
                        if (GUILayout.Button("Travel", button, GUILayout.Width(140))) { panel = ""; BuildWorld(Atlas(region.settlement.x, region.settlement.z)); Save(); }
                        GUI.enabled = true; GUILayout.EndHorizontal(); GUILayout.Space(10);
                    }
                    GUILayout.Space(10); GUILayout.Label("Dungeon entrances", heading);
                    foreach (var entry in catalog.dungeons)
                    {
                        var delta = Atlas(entry.entrance.x, entry.entrance.z) - player.position;
                        GUILayout.Label(entry.title + " — " + catalog.regions.First(r => r.id == entry.region).name + " · " + delta.magnitude.ToString("0") + "m" + (progress.State.bosses.Contains(entry.boss.id) ? " ✓" : ""), body);
                        GUILayout.Label(progress.MeetsAll(entry.prerequisite) ? "Open" : "Needs " + string.Join(", ", entry.prerequisite.Select(Friendly)), small); GUILayout.Space(10);
                    }
                    break;
                case "ending":
                    GUILayout.Label("The Engine has stopped enforcing the emergency order. The next choice belongs to the city.", body); GUILayout.Space(18);
                    EndingButton("open-record", "Open the full civic record", "Every settlement receives the whole question. The city’s morning begins with an argument its people can finally have.");
                    EndingButton("safe-passage", "Begin with safe passage", "The evacuation routes open first. The city has time to read its records while its people find their way home.");
                    EndingButton("shared-council", "Return the choice to a shared council", "The six instruments become public tools. No single machine or ruler may decide the city’s future alone.");
                    break;
            }
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        void EndingButton(string ending, string label, string outcome)
        {
            if (!GUILayout.Button(label, button)) return;
            progress.State.ending = ending; progress.Flag("ending-decided"); progress.UpdateQuests(catalog); Save();
            dialogueTitle = "The Morning Owed"; dialogueText = outcome + "\n\nThe world remains yours to explore."; dialogueChoices = null; panel = "dialogue";
        }
    }
}
