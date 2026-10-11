using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hushwake
{
    public sealed partial class Game : MonoBehaviour
    {
        const string SaveKey = "hushwake-unity-save-v1";
        const float AtlasScale = .17f;
        Catalog catalog;
        Progress progress;
        Transform environment, player;
        Camera view;
        Light sun;
        RainAtmosphere rain;
        CameraVisibility visibility;
        Dungeon dungeon;
        Room room;
        readonly List<Target> targets = new List<Target>();
        readonly List<Transform> labels = new List<Transform>();
        readonly List<Foe> foes = new List<Foe>();
        readonly List<Shot> shots = new List<Shot>();
        readonly List<Effect> effects = new List<Effect>();
        Target nearest;
        string notice = "", panel = "", selected = "pulse", dialogueTitle = "", dialogueText = "", saveError = "";
        Choice[] dialogueChoices;
        string dialogueNpc = "";
        bool started, dead;
        float noticeUntil, nextAttack, nextPulse, invulnerable, saveAt, cameraDistance = 19;
        Vector2 journalScroll;
        int frameCount;
        float frameTime, fps;
        bool detailedLighting=true;int slowLightingWindows;float lightingWarmup;

        sealed class Target
        {
            public Transform body;
            public string id, name, kind, destination;
            public Npc npc;
            public Site site;
            public SiteMechanism mechanism;
            public Dungeon entry;
            public Puzzle puzzle;
            public int step;
            public Link link;
            public Lore lore;
        }
        sealed class Foe
        {
            public Transform body;
            public float health, maxHealth, next, attackAt;
            public Vector3 home, impact;
            public string id, name;
            public bool boss, mini, winding;
            public Transform warning;
            public Phase phase;
        }
        sealed class Shot { public Transform body; public Vector3 direction; public float expires, speed; }
        sealed class Effect { public Transform body; public float expires, duration; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (FindFirstObjectByType<Game>() == null) new GameObject("Hushwake").AddComponent<Game>(); }

        void Awake()
        {
            try { catalog = Content.Load(); progress = new Progress(); }
            catch (Exception error) { saveError = error.Message; enabled = false; Debug.LogError(error); return; }
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 40;QualitySettings.shadowResolution=ShadowResolution.Medium;QualitySettings.shadowCascades=2;QualitySettings.antiAliasing=2;QualitySettings.pixelLightCount=1;
            view = new GameObject("Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            view.tag = "MainCamera"; view.fieldOfView = 52; view.nearClipPlane = .1f; view.farClipPlane = 200;view.gameObject.AddComponent<VisualFinish>();
            visibility=view.gameObject.AddComponent<CameraVisibility>();
            sun = new GameObject("Evening sun", typeof(Light)).GetComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.15f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48, -30, 0);
            sun.shadowStrength=.68f;sun.shadowBias=.035f;sun.shadowNormalBias=.2f;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .008f;RenderSettings.skybox=new Material(Shader.Find("Hushwake/Sky"));RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientLight = new Color(.42f, .52f, .53f);
            RenderSettings.defaultReflectionMode=UnityEngine.Rendering.DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=Resources.Load<Cubemap>("OriginalArt/SkyReflection");
            player = WorldArt.Character(transform, Vector3.zero, new Color(.82f, .43f, .24f), "Nera");
            player.rotation=Quaternion.Euler(0,180,0);
            var weather=new GameObject("Weather");weather.transform.SetParent(player,false);rain=weather.AddComponent<RainAtmosphere>();
            BuildWorld(Vector3.zero); RealmVisuals();
            saveAt = Time.unscaledTime + 20;
            lightingWarmup=Time.unscaledTime+8;
        }

        Vector3 Atlas(float x, float z) => WorldArt.Ground(x * AtlasScale, z * AtlasScale);
        void ClearEnvironment()
        {
            WorldShade.Clear();
            if (environment != null) Destroy(environment.gameObject);
            environment = new GameObject("World").transform; environment.SetParent(transform, false);
            targets.Clear(); labels.Clear(); foes.Clear(); shots.Clear(); effects.Clear(); nearest = null;
        }
        Target Marker(string id, string title, string kind, Vector3 position, Color color)
        {
            var body = new GameObject(title).transform; body.SetParent(environment, false); body.localPosition = position;
            if(kind!="station"){var instrument=WorldArt.Original(kind=="camp"?"lantern":"prop-copper",body,Vector3.zero);instrument.localScale=Vector3.one*.4f;}
            WorldArt.Shape(body, PrimitiveType.Sphere, Vector3.up * 1.1f, Vector3.one * .22f, color, "signal");
            WorldArt.Ring(body, Vector3.up * .03f, .85f, color);
            var label = WorldArt.Label(body, Vector3.up * 2.4f, title, new Color(.88f, .92f, .8f)); labels.Add(label);
            var target = new Target { id = id, name = title, kind = kind, body = body }; targets.Add(target); return target;
        }
        void BuildWorld(Vector3 position)
        {
            dungeon = null; room = null; progress.State.dungeon = ""; progress.State.room = "";
            ClearEnvironment(); WorldShade.Begin();WorldArt.Landscape(environment);WorldDetail.Water(environment);
            var random = new System.Random(42109);
            var reserved = WorldLayout.InteractionPoints(catalog, AtlasScale);
            var houses = WorldLayout.Houses(catalog, AtlasScale);
            foreach (var region in catalog.regions)
            {
                var center = Atlas(region.center.x, region.center.z);
                var color = RegionColor(region.id);
                for (var i = 0; i < 32; i++)
                {
                    var angle = (float)random.NextDouble() * Mathf.PI * 2; var radius = 9 + (float)random.NextDouble() * 15;
                    var location = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius; location.y = WorldArt.Height(location.x, location.z);
                    if (reserved.Any(p => WorldLayout.Distance(p, location) < 3)) continue;
                    if (i % 3 == 0) WorldArt.Shape(environment, PrimitiveType.Sphere, location + Vector3.up * .6f, new Vector3(1.8f, 1.2f, 1.5f), Color.Lerp(color, Color.gray, .4f), "weathered rock");
                    else WorldArt.Tree(environment, location, 7 + (float)random.NextDouble() * 5, color);
                }
                var town = Atlas(region.settlement.x, region.settlement.z);
                for(int i=0;i<houses[region.id].Count;i++) WorldArt.House(environment, houses[region.id][i], color,region.id,i);
                WorldDetail.Settlement(environment,town,houses[region.id]);WorldDetail.Meadow(environment,town,reserved,houses[region.id],region.id.Sum(c=>(int)c)*31);WorldFlora.Add(environment,town,reserved,houses[region.id],region.id.Sum(c=>(int)c)*31);
                var townLabel = WorldArt.Label(environment, town + Vector3.up * 6, region.settlement.name.ToUpperInvariant(), new Color(.96f, .76f, .38f)); labels.Add(townLabel);
                Marker("camp:" + region.id, "Rest • " + region.settlement.name, "camp", town + Vector3.right * 5, new Color(1, .63f, .24f));
                foreach (var site in region.sites)
                {
                    var mechanism = catalog.siteMechanisms.First(s => s.id == site.id);
                    for (var i = 0; i < mechanism.steps.Length; i++)
                    {
                        var point = WorldLayout.SiteStation(site, i, AtlasScale);
                        var station = Marker(site.id, site.name + " · " + (i + 1), "site", point, new Color(.62f, .83f, .81f)); station.site = site; station.mechanism = mechanism; station.step = i;
                    }
                }
            }
            foreach (var npc in catalog.npcs)
            {
                var pos = Atlas(npc.x, npc.z);
                // The source stores NPC coordinates in world space; retaining them keeps each person in their own settlement.
                var body = WorldArt.Character(environment, pos, RegionColor(npc.region), npc.name,npc.id);
                var label = WorldArt.Label(body, Vector3.up * 2.7f, npc.name, new Color(.97f, .79f, .49f)); labels.Add(label);
                targets.Add(new Target { id = npc.id, name = npc.name, kind = "npc", body = body, npc = npc });
            }
            foreach (var entry in catalog.dungeons)
            {
                var pos = Atlas(entry.entrance.x, entry.entrance.z);
                var arch=WorldArt.Original("portal-"+entry.id,environment,pos+Vector3.up*1.45f);arch.localScale=Vector3.one*.65f;
                Marker(entry.id, entry.title, "dungeon", pos + Vector3.back * 1.5f, new Color(.9f, .66f, .24f)).entry = entry;
            }
            foreach (var region in catalog.regions.Where(r => r.id != "hearth"))
            {
                var pos = Atlas(region.center.x + 60, region.center.z - 40);
                SpawnFoe("wild:" + region.id, "Hush scavenger", pos, false, false, 45);
            }
            player.position = WorldArt.Ground(position.x, position.z); cameraDistance = 10;
            WorldShade.Finish();ModelBatching.StaticWorld(environment);visibility.Track(environment,player);
            DetectRegion(); CameraSnap(); RealmVisuals();
        }
        static Color RegionColor(string id)
        {
            switch (id)
            {
                case "vale": return new Color(.5f, .54f, .26f);
                case "fen": return new Color(.22f, .43f, .38f);
                case "scar": return new Color(.48f, .6f, .65f);
                case "shelf": return new Color(.43f, .41f, .35f);
                case "steppe": return new Color(.62f, .57f, .37f);
                case "reach": return new Color(.36f, .55f, .57f);
                case "crown": return new Color(.48f, .34f, .52f);
                default: return new Color(.4f, .56f, .37f);
            }
        }

        void EnterDungeon(Dungeon entry)
        {
            if (!progress.MeetsAll(entry.prerequisite)) { Tell("You need: " + string.Join(", ", entry.prerequisite.Select(Friendly))); return; }
            dungeon = entry; progress.State.dungeon = entry.id; progress.Event("enter-dungeon", entry.id);
            LoadRoom(entry.rooms[0].id); Tell(entry.intro, 12); Save();
        }
        void LoadRoom(string id)
        {
            room = dungeon.rooms.First(r => r.id == id); progress.State.room = id; ClearEnvironment();
            var hue = RegionColor(dungeon.region);
            WorldDetail.Chamber(environment,hue,dungeon.id,room.id);
            var links = dungeon.links.Where(l => l.from == id || (!l.oneWay && l.to == id)).ToArray();
            for (var i = 0; i < links.Length; i++)
            {
                var link = links[i]; var to = link.from == id ? link.to : link.from;
                var destination = dungeon.rooms.First(r => r.id == to);
                var angle = (i + .5f) * Mathf.PI * 2 / Mathf.Max(links.Length, 1);
                var pos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 10;
                var gate = Marker("door:" + i, destination.name, "door", pos, progress.LinkOpen(dungeon, link) ? new Color(.35f, .85f, .64f) : new Color(.87f, .4f, .27f)); gate.link = link; gate.destination = to;
            }
            foreach (var puzzle in dungeon.puzzles)
                for (var i = 0; i < puzzle.steps.Length; i++)
                {
                    var step = puzzle.steps[i]; if ((string.IsNullOrEmpty(step.roomId) ? puzzle.roomId : step.roomId) != id) continue;
                    var ordinal = targets.Count(t => t.kind == "station");
                    var pos = new Vector3(-5 + ordinal % 3 * 4.5f, 0, -3 + ordinal / 3 * 3.5f);
                    var station = Marker(step.target, Pretty(step.target), "station", pos, new Color(.52f, .78f, .92f)); station.puzzle = puzzle; station.step = i;
                    var machine=WorldArt.Original("station-"+puzzle.id+"-"+i,station.body,Vector3.zero);machine.localScale=Vector3.one*.82f;
                }
            if (id == dungeon.rooms[0].id || room.kind == "reward") Marker("exit", "Return to the surface", "exit", new Vector3(-8, 0, -8), new Color(.92f, .7f, .28f));
            if (!string.IsNullOrEmpty(room.loreId))
            {
                var lore = catalog.lore.FirstOrDefault(l => l.id == room.loreId);
                if (lore != null) Marker(lore.id, lore.title, "lore", new Vector3(6, 0, 4), new Color(.84f, .74f, .46f)).lore = lore;
            }
            if (room.enemyArchetypes != null && !progress.State.flags.Contains("cleared:" + dungeon.id + ":" + id))
                for (var i = 0; i < room.enemyArchetypes.Length; i++) SpawnFoe("guard:" + i, Pretty(room.enemyArchetypes[i]), new Vector3(-3 + i * 3, 0, 6), false, false, 40);
            if (dungeon.miniboss != null && dungeon.miniboss.roomId == id && !progress.State.flags.Contains("enemy:" + dungeon.id + ":" + dungeon.miniboss.id))
                SpawnFoe(dungeon.miniboss.id, Pretty(dungeon.miniboss.archetype), new Vector3(0, 0, 4), false, true, 110);
            if (dungeon.boss.roomId == id && !progress.State.bosses.Contains(dungeon.boss.id)) SpawnFoe(dungeon.boss.id, dungeon.boss.name, new Vector3(0, 0, 5), true, false, 220);
            if (dungeon.id == "hush-engine" && progress.State.bosses.Contains(dungeon.boss.id) && !progress.State.flags.Contains("ending-decided")) Marker("ending", "The unfinished vote", "ending", new Vector3(0, 0, 5), new Color(.97f, .8f, .49f));
            player.position = new Vector3(0, 0, -6); cameraDistance = 14; CameraSnap(); RealmVisuals();
            ModelBatching.StaticWorld(environment);visibility.Track(environment,player);
            RefreshDoors();
            Tell(room.description, 7); progress.UpdateQuests(catalog);
        }

        void Update()
        {if(Input.GetKeyDown(KeyCode.F3))FrameDiagnostics.visible=!FrameDiagnostics.visible;FrameDiagnostics.ui=FrameDiagnostics.actors=0;long stamp=FrameDiagnostics.visible?FrameDiagnostics.Stamp():0;try{UpdateGame();}finally{if(FrameDiagnostics.visible)FrameDiagnostics.script=FrameDiagnostics.Milliseconds(stamp);FrameDiagnostics.fps=fps;}}
        void UpdateGame()
        {
            Time.timeScale = started && !dead && string.IsNullOrEmpty(panel) ? 1 : 0;
            frameCount++; frameTime += Time.unscaledDeltaTime; if (frameTime >= 1) { fps = frameCount / frameTime; frameCount = 0; frameTime = 0;
                if(detailedLighting&&started&&string.IsNullOrEmpty(panel)&&Application.isFocused&&Time.unscaledTime>lightingWarmup&&Time.unscaledDeltaTime<.2f){slowLightingWindows=fps<25?slowLightingWindows+1:0;if(slowLightingWindows>=3)ConfigureGraphics("balanced");}
            }
            var labelFocus=player.position;var labelRotation=view.transform.rotation;
            foreach (var label in labels) if (label != null) {bool visible=Vector3.Distance(labelFocus,label.position)<13;if(label.gameObject.activeSelf!=visible)label.gameObject.SetActive(visible);if(visible)label.rotation=labelRotation;}
            if (!started || dead || !string.IsNullOrEmpty(panel)) { if (started && Input.GetKeyDown(KeyCode.Escape)) panel = ""; FollowCamera(); return; }
            if (Input.GetKeyDown(KeyCode.Escape)) { panel = "pause"; return; }
            if (Input.GetKeyDown(KeyCode.M)) { panel = "map"; return; }
            if (Input.GetKeyDown(KeyCode.J)) { panel = "journal"; return; }
            if (Input.GetKeyDown(KeyCode.I)) { panel = "inventory"; return; }
            var verbs = new[] { "pulse", "flow", "lift", "witness", "graft", "counter", "concord" };
            for (var i = 0; i < verbs.Length; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selected = verbs[i]; Tell("Selected " + Pretty(selected)); }
            var movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); movement = Vector3.ClampMagnitude(movement, 1);
            var speed = Input.GetKey(KeyCode.LeftShift) ? 8 : 5;
            if (Input.GetKeyDown(KeyCode.Space) && Time.time >= invulnerable) { player.position += movement.sqrMagnitude > .01f ? movement * 3.5f : player.forward * 3.5f; invulnerable = Time.time + .5f; Burst(player.position + Vector3.up * .1f, new Color(.4f, .8f, .83f), 1.5f); }
            var pos = player.position + movement * (speed * Time.deltaTime);
            var bound = dungeon == null ? 119 : 11.2f; pos.x = Mathf.Clamp(pos.x, -bound, bound); pos.z = Mathf.Clamp(pos.z, -bound, bound); pos.y = dungeon == null ? WorldArt.Height(pos.x, pos.z) : 0; player.position = pos;
            if (movement.sqrMagnitude > .01f) player.rotation = Quaternion.Slerp(player.rotation, Quaternion.LookRotation(movement), Time.deltaTime * 14);
            // Imported character pivots animate independently; never rotate only the first body part.
            nearest = targets.Where(t => t.body != null).OrderBy(t => Vector3.Distance(player.position, t.body.position)).FirstOrDefault();
            if (nearest != null && Vector3.Distance(player.position, nearest.body.position) > 3.1f) nearest = null;
            if (Input.GetKeyDown(KeyCode.Q)) { progress.State.realm = progress.State.realm == "wake" ? "hush" : "wake"; RealmVisuals(); Burst(player.position, new Color(.65f, .72f, .96f), 3); Tell("The world turns into " + Pretty(progress.State.realm) + "."); }
            if (Input.GetKeyDown(KeyCode.E) && nearest != null) Interact(nearest, "interact");
            if (Input.GetKeyDown(KeyCode.R) && Time.time >= nextPulse) UseAbility();
            if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttack) Attack(false);
            TickCombat(); TickEffects(); DetectRegion(); FollowCamera(); progress.UpdateQuests(catalog);
            if (Time.unscaledTime >= saveAt) { Save(); saveAt = Time.unscaledTime + 25; }
        }
        void RealmVisuals()
        {
            rain.indoors=dungeon!=null;
            var hush = progress.State.realm == "hush";
            view.backgroundColor = hush ? new Color(.075f, .07f, .15f) : new Color(.26f, .42f, .48f);
            RenderSettings.fogColor = view.backgroundColor;
            RenderSettings.ambientSkyColor=hush?new Color(.22f,.25f,.40f):new Color(.42f,.51f,.53f);RenderSettings.ambientEquatorColor=hush?new Color(.18f,.22f,.29f):new Color(.28f,.36f,.33f);RenderSettings.ambientGroundColor=new Color(.15f,.18f,.13f);RenderSettings.skybox.SetFloat("_Hush",hush?1:0);
            sun.color = hush ? new Color(.67f, .68f, 1) : new Color(1, .94f, .82f); sun.intensity = hush ? .7f : .9f;
            // Runtime-created scenes need an explicit fill probe; their bake contains no sky lighting.
            var fill=new UnityEngine.Rendering.SphericalHarmonicsL2();fill.AddAmbientLight(hush?new Color(.075f,.09f,.15f):new Color(.10f,.135f,.115f));RenderSettings.ambientProbe=fill;
            view.clearFlags=dungeon==null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;
            if(dungeon!=null){view.backgroundColor=hush?new Color(.075f,.10f,.15f):new Color(.13f,.18f,.20f);RenderSettings.fogColor=view.backgroundColor;}
        }
        public void ConfigureGraphics(string profile){detailedLighting=profile!="balanced";slowLightingWindows=0;lightingWarmup=Time.unscaledTime+8;QualitySettings.shadows=detailedLighting?ShadowQuality.All:ShadowQuality.Disable;QualitySettings.antiAliasing=detailedLighting?2:0;Debug.Log("HUSHWAKE_GRAPHICS_PROFILE "+(detailedLighting?"detailed":"balanced"));}
        void DetectRegion()
        {
            if (dungeon != null) return;
            var region = catalog.regions.OrderBy(r => Vector3.Distance(player.position, Atlas(r.center.x, r.center.z))).First();
            if (progress.State.region != region.id) { progress.State.region = region.id; Tell(region.name + " • " + region.rumor, 8); }
            progress.Event("enter-region", region.id); if (!progress.State.visited.Contains(region.id)) progress.State.visited.Add(region.id);
            if (region.id == "crown") progress.Flag("at-crown");
        }
        void CameraSnap() { view.transform.position = player.position + new Vector3(0, cameraDistance * (dungeon==null?.42f:.66f), -cameraDistance); view.transform.LookAt(player.position + Vector3.up*1.2f); }
        void FollowCamera()
        {
            cameraDistance = Mathf.Clamp(cameraDistance - Input.mouseScrollDelta.y * 1.2f, 7, 26);
            view.transform.position = Vector3.Lerp(view.transform.position, player.position + new Vector3(0, cameraDistance * (dungeon==null?.42f:.66f), -cameraDistance), Time.unscaledDeltaTime * 6);
            view.transform.LookAt(player.position + Vector3.up*1.2f);
        }
        void Interact(Target target, string verb)
        {
            if (target.kind == "station")
            {
                var step = target.puzzle.steps[target.step]; var used = verb;
                if ((step.verb == "wake" || step.verb == "hush") && verb == "interact") used = progress.State.realm;
                if (progress.SolveStep(target.puzzle, target.step, used)) { Tell(target.puzzle.title + ": " + Pretty(step.target) + " answers."); Burst(target.body.position, new Color(.42f, .85f, .7f), 2); RefreshDoors(); Save(); }
                else Tell(target.puzzle.clue + "  Next: " + NextPuzzleHint(target.puzzle), 10);
                return;
            }
            if (verb != "interact" && target.kind != "site") return;
            switch (target.kind)
            {
                case "npc":
                    var beat = target.npc.dialogue.FirstOrDefault(b => progress.MeetsAll(b.when));
                    if (beat == null) return;
                    progress.Event("interact", target.id); progress.Flag("met-" + target.id); dialogueNpc = target.id; dialogueTitle = target.name; dialogueText = string.Join("\n\n", beat.lines); dialogueChoices = beat.choices; panel = "dialogue"; break;
                case "camp": progress.State.health = 100; Tell("Rested. Your journey is saved."); Save(); break;
                case "site":
                    if (!progress.MeetsAll(target.site.requires)) { Tell("You need: " + string.Join(", ", target.site.requires.Select(Friendly))); break; }
                    if (!string.IsNullOrEmpty(target.site.realm) && target.site.realm != "both" && target.site.realm != progress.State.realm) { Tell("This place is visible in " + Pretty(target.site.realm) + ". Press Q to turn the world."); break; }
                    var expectedIndex = progress.SiteStep(target.mechanism);
                    if (expectedIndex >= target.mechanism.steps.Length) { Tell(target.site.name + ": already explored."); break; }
                    var expected = target.mechanism.steps[expectedIndex];
                    var siteVerb = verb == "interact" && (expected.verb == "wake" || expected.verb == "hush") ? progress.State.realm : verb;
                    if (!progress.SolveSite(target.site, target.mechanism, target.step, siteVerb)) { Tell(target.site.name + " • Station " + (expectedIndex + 1) + ": " + expected.hint + "  Use " + Pretty(expected.verb) + ".", 10); break; }
                    Burst(target.body.position, new Color(.4f, .86f, .78f), 2); Save();
                    if (progress.SiteStep(target.mechanism) < target.mechanism.steps.Length) { Tell(target.mechanism.steps[expectedIndex + 1].hint + " • Next marked station.", 8); break; }
                    dialogueNpc = ""; dialogueTitle = target.site.name; dialogueText = target.site.clue + "\n\n" + LoreText(target.site.loreId); dialogueChoices = null; panel = "dialogue"; break;
                case "dungeon": EnterDungeon(target.entry); break;
                case "door":
                    if (target.link.shortcut && (target.link.requires == null || target.link.requires.Length == 0) && (room.id == target.link.from || progress.State.bosses.Contains(dungeon.boss.id))) progress.Flag("shortcut:" + dungeon.id + ":" + target.link.from + ":" + target.link.to);
                    if (!progress.LinkOpen(dungeon, target.link)) { Tell("The passage is held. Finish the machinery or guardian here. " + string.Join(", ", (target.link.requires ?? new string[0]).Select(Friendly)), 8); break; }
                    LoadRoom(target.destination); Save(); break;
                case "exit": var surface = Atlas(dungeon.entrance.x, dungeon.entrance.z) + Vector3.back * 5; BuildWorld(surface); Save(); break;
                case "lore": ReadLore(target.id); dialogueTitle = target.lore.title; dialogueText = target.lore.text; dialogueChoices = null; panel = "dialogue"; Save(); break;
                case "ending": panel = "ending"; break;
            }
        }
        void ReadLore(string id) { if (!string.IsNullOrEmpty(id) && !progress.State.lore.Contains(id)) progress.State.lore.Add(id); }
        string LoreText(string id) => catalog.lore.FirstOrDefault(l => l.id == id)?.text ?? "";
        string NextPuzzleHint(Puzzle puzzle)
        {
            var index = progress.PuzzleStep(puzzle); if (index >= puzzle.steps.Length) return "Complete";
            var step = puzzle.steps[index]; var where = dungeon.rooms.FirstOrDefault(r => r.id == (string.IsNullOrEmpty(step.roomId) ? puzzle.roomId : step.roomId));
            return Pretty(step.verb) + " at " + Pretty(step.target) + " (" + where?.name + ")";
        }
        void RefreshDoors()
        {
            if (dungeon == null) return;
            foreach (var target in targets.Where(t => t.kind == "door"))
            {
                var open = progress.LinkOpen(dungeon, target.link);
                target.name = dungeon.rooms.First(r => r.id == target.destination).name + (open ? "" : " · Locked");
                target.body.Find("label").GetComponent<TextMesh>().text = target.name;
                target.body.Find("signal").GetComponent<Renderer>().sharedMaterial = WorldArt.Material(open ? new Color(.35f, .85f, .64f) : new Color(.87f, .4f, .27f));
            }
        }
        static string Pretty(string text) => string.IsNullOrEmpty(text) ? "" : System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.Replace('-', ' '));
        static string Friendly(string text) => Pretty(text.Replace(':', ' '));
        void Tell(string text, float duration = 5) { notice = text; noticeUntil = Time.unscaledTime + duration; }

        void UseAbility()
        {
            var allowed = progress.State.abilities.Contains(selected) || (selected == "graft" && progress.State.items.Contains("graft-hook")) || (selected == "counter" && progress.State.items.Contains("mass-dial")) || (selected == "concord" && progress.State.items.Contains("concord-crest"));
            if (!allowed) { Tell("Find the instrument for " + Pretty(selected) + " first."); return; }
            nextPulse = Time.time + .7f; Burst(player.position, new Color(.41f, .88f, .86f), 4);
            if (nearest != null) Interact(nearest, selected);
            Attack(true);
        }
        void SpawnFoe(string id, string name, Vector3 position, bool boss, bool mini, float health)
        {
            var key=boss?"boss-"+dungeon.id:mini?"mini-"+dungeon.id:id.StartsWith("wild:")?"enemy-veil-stalker":"enemy-"+name.ToLowerInvariant().Replace(' ','-');
            var body = WorldArt.Character(environment, position, Color.white, name,key);
            if (boss || mini) body.localScale = Vector3.one * (boss ? .8f : 1.1f);
            if (boss) WorldArt.Ring(body, Vector3.up * 2.5f, 1.5f, new Color(.9f, .63f, .26f));
            var label = WorldArt.Label(body, Vector3.up * 2.8f, name, new Color(.99f, .61f, .4f)); labels.Add(label);
            foes.Add(new Foe { id = id, name = name, body = body, health = health, maxHealth = health, home = position, next = Time.time + 2, boss = boss, mini = mini });
        }
        void Attack(bool ability)
        {
            if (!ability) nextAttack = Time.time + .38f;
            Burst(player.position + player.forward * .8f + Vector3.up * .6f, ability ? new Color(.48f, .91f, .87f) : new Color(1, .79f, .42f), ability ? 3 : 1.7f);
            foreach (var foe in foes.ToArray())
            {
                if (foe.body == null || Vector3.Distance(player.position, foe.body.position) > (ability ? 6 : 3.1f)) continue;
                if (!ability && Vector3.Dot(player.forward, (foe.body.position - player.position).normalized) < -.25f) continue;
                var damage = ability ? 18 : 12;
                if (ability && foe.boss && foe.phase != null && foe.phase.vulnerableTo.Contains(selected)) damage = 32;
                foe.health -= damage;
                if (!foe.boss) foe.body.position += (foe.body.position - player.position).normalized * .6f;
                if (foe.health > 0) continue;
                if (foe.warning != null) Destroy(foe.warning.gameObject);
                Destroy(foe.body.gameObject); foes.Remove(foe);
                if (foe.boss)
                {
                    progress.Defeat(dungeon); Tell(dungeon.boss.reward, 12); progress.UpdateQuests(catalog);
                    if (dungeon.id == "hush-engine") Marker("ending", "The unfinished vote", "ending", new Vector3(0, 0, 5), new Color(.97f, .8f, .49f));
                }
                else if (foe.mini) { progress.Flag("enemy:" + dungeon.id + ":" + foe.id); Tell("The guardian releases the passage."); }
                if (dungeon != null && foes.Count == 0) progress.Flag("cleared:" + dungeon.id + ":" + room.id);
                RefreshDoors();
                progress.State.health = Mathf.Min(100, progress.State.health + 12); Save();
            }
        }
        void TickCombat()
        {
            foreach (var foe in foes)
            {
                if (foe.body == null) continue;
                var distance = Vector3.Distance(player.position, foe.body.position);
                if (distance > (dungeon == null ? 13 : 32)) continue;
                foe.body.LookAt(new Vector3(player.position.x, foe.body.position.y, player.position.z));
                if (foe.boss) foe.phase = dungeon.boss.phases.LastOrDefault(p => foe.health / foe.maxHealth <= p.threshold) ?? dungeon.boss.phases[0];
                if (!foe.winding && distance > (foe.boss ? 7 : 2.2f)) foe.body.position = Vector3.MoveTowards(foe.body.position, player.position, Time.deltaTime * (foe.boss ? 1.6f : 2.3f));
                if (dungeon == null) { var p = foe.body.position; p.y = WorldArt.Height(p.x, p.z); foe.body.position = p; }
                if (!foe.winding && Time.time >= foe.next)
                {
                    foe.winding = true; foe.attackAt = Time.time + (foe.boss ? 1.15f : .8f); foe.impact = player.position;
                    foe.warning = new GameObject("Attack warning").transform; foe.warning.SetParent(environment, false); foe.warning.position = foe.impact + Vector3.up * .05f;
                    WorldArt.Ring(foe.warning, Vector3.zero, foe.boss ? 2.6f : 1.5f, new Color(.99f, .3f, .18f));
                    if (foe.boss) Tell(foe.phase.telegraph + " • Dodge with Space", 2);
                }
                if (foe.winding && Time.time >= foe.attackAt)
                {
                    if (Vector3.Distance(player.position, foe.impact) < (foe.boss ? 2.6f : 1.5f)) Hurt(foe.boss ? 18 : 9);
                    Burst(foe.impact, new Color(.98f, .42f, .24f), foe.boss ? 2.6f : 1.5f);
                    if (foe.boss)
                    {
                        var count = foe.health < foe.maxHealth * .33f ? 10 : foe.health < foe.maxHealth * .67f ? 7 : 4;
                        for (var i = 0; i < count; i++)
                        {
                            var angle = Mathf.PI * 2 * i / count + Time.time;
                            var body = WorldArt.Shape(environment, PrimitiveType.Sphere, foe.body.position + Vector3.up * .7f, Vector3.one * .4f, new Color(.97f, .43f, .24f), "civic echo").transform;
                            shots.Add(new Shot { body = body, direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)), speed = 5, expires = Time.time + 4 });
                        }
                    }
                    Destroy(foe.warning.gameObject); foe.winding = false; foe.next = Time.time + (foe.boss ? 1.8f : 2.5f);
                }
            }
            foreach (var shot in shots.ToArray())
            {
                shot.body.position += shot.direction * (shot.speed * Time.deltaTime);
                if (Vector3.Distance(shot.body.position, player.position + Vector3.up * .7f) < .65f) { Hurt(10); Destroy(shot.body.gameObject); shots.Remove(shot); }
                else if (Time.time >= shot.expires) { Destroy(shot.body.gameObject); shots.Remove(shot); }
            }
        }
        void Hurt(float damage)
        {
            if (Time.time < invulnerable) return;
            invulnerable = Time.time + .75f; progress.State.health -= damage; Tell("Nera takes " + damage + " damage.", 2);
            if (progress.State.health <= 0) { dead = true; Tell("The thread holds. Rest, then try again.", 30); }
        }
        void Burst(Vector3 position, Color color, float radius)
        {
            var root = new GameObject("Resonance").transform; root.SetParent(environment, false); root.position = position + Vector3.up * .06f;
            WorldArt.Ring(root, Vector3.zero, radius, color); effects.Add(new Effect { body = root, expires = Time.time + .4f, duration = .4f });
        }
        void TickEffects()
        {
            foreach (var effect in effects.ToArray())
                if (Time.time >= effect.expires) { Destroy(effect.body.gameObject); effects.Remove(effect); }
                else effect.body.localScale = Vector3.one * Mathf.Lerp(1, .3f, (effect.expires - Time.time) / effect.duration);
        }

        void Save()
        {
            if (!started || dead) return;
            progress.State.x = player.position.x; progress.State.z = player.position.z;
            try { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress.State)); PlayerPrefs.Save(); saveError = ""; }
            catch (Exception error) { saveError = "Your browser could not save this journey. " + error.Message; Debug.LogWarning("Hushwake save failed: " + error.Message); }
        }
        void StartJourney(bool load)
        {
            try
            {
                var state = load ? JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey)) : new SaveData();
                if (state == null) throw new InvalidOperationException("No saved journey was found.");
                progress = new Progress(state);
                if (!catalog.regions.Any(r => r.id == state.region)) throw new InvalidOperationException("The saved region is unknown.");
                started = true; dead = false; panel = ""; saveError = "";
                var savedDungeon = catalog.dungeons.FirstOrDefault(d => d.id == state.dungeon);
                if (savedDungeon != null && savedDungeon.rooms.Any(r => r.id == state.room)) { dungeon = savedDungeon; LoadRoom(state.room); player.position = new Vector3(Mathf.Clamp(state.x, -11, 11), 0, Mathf.Clamp(state.z, -11, 11)); }
                else if (!string.IsNullOrEmpty(state.dungeon)) throw new InvalidOperationException("The saved chamber is unknown.");
                else BuildWorld(new Vector3(state.x, 0, state.z));
                RealmVisuals(); Tell(load ? "Welcome back, Nera." : "Speak to Mara by the houses. Then find the buried bell west of Harthwick.", 15); Save();
            }
            catch (Exception error) { started = false; saveError = "Cannot load this journey: " + error.Message + " Your existing save has been kept."; Debug.LogWarning(saveError); }
        }
        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() { Save(); }
    }
}
