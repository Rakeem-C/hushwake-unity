using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hushwake
{
    public static class WorldLayout
    {
        public static Vector3 SiteStation(Site site, int index, float scale)
        {
            var position = new Vector3(site.x * scale + (index - 1) * 3.2f, 0, site.z * scale + index % 2 * 3.2f);
            position.y = WorldArt.Height(position.x, position.z); return position;
        }
        public static List<Vector3> InteractionPoints(Catalog catalog, float scale)
        {
            var points = new List<Vector3>();
            foreach (var site in catalog.regions.SelectMany(r => r.sites))
                for (var i = 0; i < catalog.siteMechanisms.First(m => m.id == site.id).steps.Length; i++) points.Add(SiteStation(site, i, scale));
            points.AddRange(catalog.npcs.Select(n => new Vector3(n.x * scale, 0, n.z * scale)));
            points.AddRange(catalog.dungeons.Select(d => new Vector3(d.entrance.x * scale, 0, d.entrance.z * scale - 1.5f)));
            points.AddRange(catalog.regions.Select(r => new Vector3(r.settlement.x * scale + 5, 0, r.settlement.z * scale)));
            return points;
        }
        public static float Distance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
        public static Dictionary<string, List<Vector3>> Houses(Catalog catalog, float scale)
        {
            var result = new Dictionary<string, List<Vector3>>(); var reserved = InteractionPoints(catalog, scale); var buildings = new List<Vector3>();
            foreach (var region in catalog.regions)
            {
                var homes = new List<Vector3>(); result[region.id] = homes;
                for (var i = 0; i < 5; i++)
                {
                    var position = new Vector3(region.settlement.x * scale + (i % 3 - 1) * 6, 0, region.settlement.z * scale + i / 3 * 7 + 6);
                    for (var attempt = 0; attempt < 12; attempt++)
                    {
                        if (reserved.All(p => Distance(p, position) >= 5) && buildings.All(p => Distance(p, position) >= 6))
                        { position.y = WorldArt.Height(position.x, position.z); homes.Add(position); buildings.Add(position); break; }
                        position.z += 4;
                    }
                }
            }
            return result;
        }
    }
}
