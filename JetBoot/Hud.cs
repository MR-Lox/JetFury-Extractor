using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JetBoot
{
    // Repositions / rescales / hides the blocks of the Jet Fury HUD.
    //  - AB_canvas_01/in_game (combat) and AB_race_canvas/in_game (race) are laid out for 800x600 and scaled
    //    to the screen width: on wide screens their top and bottom blocks leave the screen. "Fit" brings the
    //    whole layout back inside the visible area, then the blocks can be pushed towards the edges.
    //  - AB_canvas_02 holds the flight instruments and the target frames drawn on the enemies: these follow
    //    the 3D scene, so they are never moved, only shown or hidden.
    // The game keeps updating the texts; we only apply an offset and a scale on top of what it sets.
    public class Hud : MonoBehaviour
    {
        public class Group
        {
            public string Key = "", Name = "";
            public string Canvas = "AB_canvas_01";
            public string[] Parts = new string[0];   // paths under in_game
            public int SideX, SideY;                 // -1 / 0 / +1: edge the block goes to when spread
            public bool Movable = true;              // false: centred messages, only scaled
            public bool Fixed;                       // never moved nor scaled (crosshair)
            public bool Listed = true;               // has its own show / hide line in the menu
        }

        public static readonly Group[] Groups =
        {
            new Group { Key = "Timer",     Name = "Temps restant",            Parts = new[] { "timer" }, SideY = 1 },
            new Group { Key = "Jets",      Name = "Jets abattus",             Parts = new[] { "enemy count/jet_ui" }, SideX = -1, SideY = 1 },
            new Group { Key = "Frigates",  Name = "Frégates coulées",         Parts = new[] { "enemy count/frigate_ui" }, SideX = 1, SideY = 1 },
            new Group { Key = "Lives",     Name = "Vies",                     Parts = new[] { "life_count" }, SideX = -1, SideY = -1 },
            new Group { Key = "Altitude",  Name = "Altitude",                 Parts = new[] { "altitude" }, SideY = -1 },
            new Group { Key = "Buttons",   Name = "Aide des boutons",         Parts = new[] { "buttons" }, SideX = 1, SideY = -1 },
            new Group { Key = "Crosshair", Name = "Viseur",                   Parts = new[] { "crosshair", "crosshair_locked" }, Fixed = true },
            new Group { Key = "Messages",  Name = "Messages (missile, demi-tour…)", Movable = false,
                        Parts = new[] { "missile_hit", "incoming!", "turn_back", "turn_countdown", "text_StageClear" } },
            // Race mode.
            new Group { Key = "RaceTimer", Canvas = "AB_race_canvas", Listed = false, Parts = new[] { "timer_count_up" }, SideY = 1 },
            new Group { Key = "Position",  Canvas = "AB_race_canvas", Name = "Course : position", SideY = 1,
                        Parts = new[] { "race_ui/race_position", "race_ui/ai_count", "race_ui/position_frame" } },
            new Group { Key = "Laps",      Canvas = "AB_race_canvas", Name = "Course : tours", SideY = 1,
                        Parts = new[] { "race_ui/lap count", "race_ui/lap ammount", "race_ui/lap_frame" } },
            new Group { Key = "LapTimes",  Canvas = "AB_race_canvas", Name = "Course : temps au tour", SideX = -1, SideY = 1,
                        Parts = new[] { "race_ui/current_lap_time", "race_ui/last_lap_time", "race_ui/best_lap_time" } },
            new Group { Key = "RaceMsg",   Canvas = "AB_race_canvas", Listed = false, Movable = false, Parts = new[] { "finished!" } },
        };

        // Shown / hidden only (AB_canvas_02).
        public const string Instruments = "Instruments", Targets = "Targets";

        class Track { public Vector2 BasePos, WrittenPos; public Vector3 BaseScale, WrittenScale; public bool Written; }

        class Root
        {
            public RectTransform? InGame, Canvas;
            public readonly Dictionary<Group, List<RectTransform>> Rts = new Dictionary<Group, List<RectTransform>>();
            public readonly Dictionary<RectTransform, Bounds> LocalBounds = new Dictionary<RectTransform, Bounds>();
        }

        static Hud? instance;
        readonly Dictionary<string, Root> roots = new Dictionary<string, Root>();
        readonly Dictionary<RectTransform, Track> track = new Dictionary<RectTransform, Track>();
        Transform? canvas02;
        float nextTargets;

        public static void Install()
        {
            if (instance) return;
            var go = new GameObject("JetBootHud");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Hud>();
            SceneManager.sceneLoaded += (s, m) => { if (instance) { instance!.roots.Clear(); instance.track.Clear(); instance.canvas02 = null; } };
        }

        Root? Resolve(string canvasName)
        {
            if (roots.TryGetValue(canvasName, out var r) && r.InGame && r.InGame!.gameObject.scene.isLoaded) return r;
            RectTransform? inGame = null;
            foreach (var t in Resources.FindObjectsOfTypeAll<RectTransform>())
                if (t.name == "in_game" && t.parent && t.parent.name == canvasName && t.gameObject.scene.isLoaded) { inGame = t; break; }
            if (!inGame) { roots.Remove(canvasName); return null; }
            r = new Root { InGame = inGame, Canvas = inGame!.parent as RectTransform };
            foreach (var g in Groups)
            {
                if (g.Canvas != canvasName) continue;
                var list = new List<RectTransform>();
                foreach (var p in g.Parts)
                    if (inGame.Find(p) is RectTransform rt) { list.Add(rt); r.LocalBounds[rt] = MeasureLocal(rt); }
                r.Rts[g] = list;
            }
            roots[canvasName] = r;
            return r;
        }

        // Union of the rects of the element and of its descendants that draw something, in the element's own space.
        static Bounds MeasureLocal(RectTransform rt)
        {
            var b = new Bounds();
            bool first = true;
            var corners = new Vector3[4];
            foreach (var r in rt.GetComponentsInChildren<RectTransform>(true))
            {
                if (!r.GetComponent<CanvasRenderer>()) continue;
                r.GetWorldCorners(corners);
                foreach (var c in corners)
                {
                    var p = rt.InverseTransformPoint(c);
                    p.z = 0;
                    if (first) { b = new Bounds(p, Vector3.zero); first = false; } else b.Encapsulate(p);
                }
            }
            if (first) b = new Bounds(Vector3.zero, new Vector3(rt.rect.width, rt.rect.height, 0));
            return b;
        }

        Track Get(RectTransform rt)
        {
            if (!track.TryGetValue(rt, out var t)) { t = new Track(); track[rt] = t; }
            // Anything the game wrote since our last frame becomes the new base.
            // (with a tolerance: under a scaled parent, the value read back is not exactly the one written)
            if (!t.Written || (rt.anchoredPosition - t.WrittenPos).sqrMagnitude > 1e-4f) t.BasePos = rt.anchoredPosition;
            if (!t.Written || (rt.localScale - t.WrittenScale).sqrMagnitude > 1e-8f) t.BaseScale = rt.localScale;
            return t;
        }

        // Scale of an element's parent, expressed in in_game units.
        static float ParentScale(RectTransform rt, RectTransform root)
        {
            var p = rt.parent;
            if (!p || p == root) return 1f;
            float k = root.InverseTransformVector(p!.TransformVector(Vector3.right)).x;
            return Mathf.Abs(k) < 1e-5f ? 1f : k;
        }

        void LateUpdate()
        {
            foreach (var name in new[] { "AB_canvas_01", "AB_race_canvas" })
            {
                var r = Resolve(name);
                if (r != null && r.InGame!.gameObject.activeInHierarchy) Layout(r);
            }
            Canvas02();
        }

        void Layout(Root r)
        {
            var root = r.InGame!;
            // Screen area in in_game space.
            var corners = new Vector3[4];
            r.Canvas!.GetWorldCorners(corners);
            var a0 = root.InverseTransformPoint(corners[0]);
            var a2 = root.InverseTransformPoint(corners[2]);
            var area = new Rect(a0.x, a0.y, a2.x - a0.x, a2.y - a0.y);
            float marginY = Settings.HudMargin * area.height;
            // The arcade bezel covers the sides of the picture: keep the blocks inside its screen.
            float marginX = marginY + (Settings.Bezel ? 0.08f * area.width : 0f);
            var inner = new Rect(area.xMin + marginX, area.yMin + marginY, area.width - 2 * marginX, area.height - 2 * marginY);

            // Base bounds of every group (in in_game space, without our changes).
            var gb = new Dictionary<Group, Bounds>();
            var basePivots = new Dictionary<RectTransform, Vector2>();
            Bounds content = new Bounds();
            bool firstContent = true;
            foreach (var kv in r.Rts)
            {
                var g = kv.Key;
                bool first = true;
                var b = new Bounds();
                foreach (var rt in kv.Value)
                {
                    if (!rt) continue;
                    var t = Get(rt);
                    float ps = ParentScale(rt, root);
                    Vector2 now = root.InverseTransformPoint(rt.position);
                    var basePivot = now - (rt.anchoredPosition - t.BasePos) * ps;
                    basePivots[rt] = basePivot;
                    var lb = r.LocalBounds[rt];
                    var min = basePivot + Vector2.Scale(lb.min, t.BaseScale) * ps;
                    var max = basePivot + Vector2.Scale(lb.max, t.BaseScale) * ps;
                    var eb = new Bounds((min + max) / 2, max - min);
                    if (first) { b = eb; first = false; } else b.Encapsulate(eb);
                }
                if (first) continue;
                gb[g] = b;
                if (g.Movable && !g.Fixed)
                {
                    if (firstContent) { content = b; firstContent = false; } else content.Encapsulate(b);
                }
            }

            // Fit: shrink the layout (around the screen centre) until the movable blocks are all inside.
            float fit = 1f;
            if (Settings.HudFit && !firstContent)
            {
                Vector2 ctr = inner.center;
                float halfW = Mathf.Max(Mathf.Abs(content.max.x - ctr.x), Mathf.Abs(content.min.x - ctr.x));
                float halfH = Mathf.Max(Mathf.Abs(content.max.y - ctr.y), Mathf.Abs(content.min.y - ctr.y));
                if (halfW > 1e-3f) fit = Mathf.Min(fit, inner.width / 2 / halfW);
                if (halfH > 1e-3f) fit = Mathf.Min(fit, inner.height / 2 / halfH);
            }

            foreach (var kv in r.Rts)
            {
                var g = kv.Key;
                if (!gb.TryGetValue(g, out var b)) continue;
                float s = g.Fixed ? 1f : fit * Settings.HudScale;
                Vector2 c = b.center, size = b.size * s;
                Vector2 target = c;
                if (!g.Fixed)
                {
                    target = inner.center + (c - inner.center) * fit;
                    if (g.Movable)
                    {
                        if (g.SideX != 0)
                        {
                            float edge = g.SideX > 0 ? inner.xMax - size.x / 2 : inner.xMin + size.x / 2;
                            target.x = Mathf.Lerp(target.x, edge, Settings.HudSpreadX);
                        }
                        if (g.SideY != 0)
                        {
                            float edge = g.SideY > 0 ? inner.yMax - size.y / 2 : inner.yMin + size.y / 2;
                            target.y = Mathf.Lerp(target.y, edge, Settings.HudSpreadY);
                        }
                        if (Settings.HudFit)
                            target = new Vector2(ClampInside(target.x, size.x, inner.xMin, inner.xMax), ClampInside(target.y, size.y, inner.yMin, inner.yMax));
                    }
                }

                bool shown = Settings.HudShown(g.Key);
                foreach (var rt in kv.Value)
                {
                    if (!rt) continue;
                    var t = track[rt];
                    float ps = ParentScale(rt, root);
                    var basePivot = basePivots[rt];
                    var newPivot = target + (basePivot - c) * s;
                    t.WrittenPos = t.BasePos + (newPivot - basePivot) / ps;
                    t.WrittenScale = t.BaseScale * s;
                    t.Written = true;
                    rt.anchoredPosition = t.WrittenPos;
                    rt.localScale = t.WrittenScale;
                    SetShown(rt, shown);
                }
            }
        }

        static void SetShown(Component c, bool shown)
        {
            var cg = c.GetComponent<CanvasGroup>() ?? c.gameObject.AddComponent<CanvasGroup>();
            float a = shown ? 1f : 0f;
            if (cg.alpha != a) cg.alpha = a;
            cg.blocksRaycasts = false;
        }

        // Flight instruments and target frames: only shown or hidden.
        void Canvas02()
        {
            if (!canvas02)
            {
                if (Time.unscaledTime < nextTargets) return;
                nextTargets = Time.unscaledTime + 1f;
                foreach (var t in Resources.FindObjectsOfTypeAll<Canvas>())
                    if (t.name == "AB_canvas_02" && t.gameObject.scene.isLoaded) { canvas02 = t.transform; break; }
                if (!canvas02) return;
            }
            bool instruments = Settings.HudShown(Instruments), targets = Settings.HudShown(Targets);
            foreach (Transform ch in canvas02!)
            {
                if (ch.name == "player_ui") SetShown(ch, instruments);
                else if (ch.name.StartsWith("rectangle_target") || ch.name == "Cross") SetShown(ch, targets);
            }
        }

        static float ClampInside(float centre, float size, float min, float max)
        {
            if (size >= max - min) return (min + max) / 2f;
            return Mathf.Clamp(centre, min + size / 2f, max - size / 2f);
        }
    }
}
