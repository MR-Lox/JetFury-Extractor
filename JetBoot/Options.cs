using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace JetBoot
{
    // Persistent settings stored next to the executable (JetFury_settings.ini).
    public static class Settings
    {
        public static int DisplayMode = 1;   // 0 = plein écran exclusif, 1 = plein écran fenêtré, 2 = fenêtré
        public static int Width, Height;
        public static bool VSync = true;
        public static bool Bezel = true;
        public static bool Crt = true;
        public static bool Outline = true;
        public static bool Beautify = true;
        public static bool LightHalftone = true;
        public static int AntiAliasing = -1;  // -1 = jeu d'origine, 0 = aucun, 1 = FXAA, 2 = SMAA, 3 = TAA
        public static float VolMaster = 1f, VolMusic = 1f, VolEngine = 1f, VolFx = 1f;
        public static string Soundtrack = GameAudio.Original;
        public static bool HudFit = true;
        public static float HudScale = 1f, HudSpreadX = 0f, HudSpreadY = 0f, HudMargin = 0.03f;
        public static HashSet<string> HudHidden = new HashSet<string>();
        public static bool HudShown(string key) => !HudHidden.Contains(key);
        public static void SetHudShown(string key, bool v) { if (v) HudHidden.Remove(key); else HudHidden.Add(key); }

        static string FilePath => Path.Combine(Application.dataPath, "..", "JetFury_settings.ini");

        public static void Load()
        {
            Width = Screen.currentResolution.width;
            Height = Screen.currentResolution.height;
            L.Code = L.Default();
            if (!File.Exists(FilePath)) return;
            foreach (var line in File.ReadAllLines(FilePath))
            {
                var kv = line.Split(new[] { '=' }, 2);
                if (kv.Length != 2) continue;
                string k = kv[0].Trim(), v = kv[1].Trim();
                int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n);
                float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float fl);
                bool b = v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);
                switch (k)
                {
                    case "Language": if (Array.IndexOf(L.Codes, v) >= 0) L.Code = v; break;
                    case "DisplayMode": DisplayMode = Mathf.Clamp(n, 0, 2); break;
                    case "Width": if (n > 0) Width = n; break;
                    case "Height": if (n > 0) Height = n; break;
                    case "VSync": VSync = b; break;
                    case "Bezel": Bezel = b; break;
                    case "Crt": Crt = b; break;
                    case "Outline": Outline = b; break;
                    case "Beautify": Beautify = b; break;
                    case "LightHalftone": LightHalftone = b; break;
                    case "AntiAliasing": AntiAliasing = Mathf.Clamp(n, -1, 3); break;
                    case "VolMaster": VolMaster = Mathf.Clamp01(fl); break;
                    case "VolMusic": VolMusic = Mathf.Clamp01(fl); break;
                    case "VolEngine": VolEngine = Mathf.Clamp01(fl); break;
                    case "VolFx": VolFx = Mathf.Clamp01(fl); break;
                    case "Soundtrack": Soundtrack = v; break;
                    case "HudFit": HudFit = b; break;
                    case "HudScale": HudScale = Mathf.Clamp(fl, 0.5f, 1.5f); break;
                    case "HudSpreadX": HudSpreadX = Mathf.Clamp01(fl); break;
                    case "HudSpreadY": HudSpreadY = Mathf.Clamp01(fl); break;
                    case "HudMargin": HudMargin = Mathf.Clamp(fl, 0f, 0.15f); break;
                    case "HudHidden": HudHidden = new HashSet<string>(v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())); break;
                }
            }
        }

        public static void Save()
        {
            try
            {
                File.WriteAllLines(FilePath, new[]
                {
                    "Language=" + L.Code, "DisplayMode=" + DisplayMode, "Width=" + Width, "Height=" + Height,
                    "VSync=" + B(VSync), "Bezel=" + B(Bezel), "Crt=" + B(Crt), "Outline=" + B(Outline),
                    "Beautify=" + B(Beautify), "LightHalftone=" + B(LightHalftone), "AntiAliasing=" + AntiAliasing,
                    "VolMaster=" + F(VolMaster), "VolMusic=" + F(VolMusic), "VolEngine=" + F(VolEngine), "VolFx=" + F(VolFx),
                    "Soundtrack=" + Soundtrack,
                    "HudFit=" + B(HudFit), "HudScale=" + F(HudScale), "HudSpreadX=" + F(HudSpreadX), "HudSpreadY=" + F(HudSpreadY),
                    "HudMargin=" + F(HudMargin), "HudHidden=" + string.Join(",", HudHidden),
                });
            }
            catch (Exception e) { Debug.LogWarning("[JetBoot] settings save failed: " + e.Message); }
        }

        static string B(bool b) => b ? "1" : "0";
        static string F(float f) => f.ToString("0.###", CultureInfo.InvariantCulture);

        public static FullScreenMode Mode =>
            DisplayMode == 0 ? FullScreenMode.ExclusiveFullScreen :
            DisplayMode == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        public static Vector2Int TargetSize()
        {
            int w = Width, h = Height;
            if (DisplayMode == 2)
            {
                // A window as large as the monitor would hide its bottom behind the taskbar:
                // shrink it to fit in ~90% of the desktop while keeping the aspect ratio.
                var native = Screen.currentResolution;
                float k = Mathf.Min(1f, native.width * 0.9f / w, native.height * 0.9f / h);
                w = Mathf.RoundToInt(w * k); h = Mathf.RoundToInt(h * k);
            }
            return new Vector2Int(w, h);
        }

        public static void ApplyDisplay()
        {
            var t = TargetSize();
            Screen.SetResolution(t.x, t.y, Mode);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }

        // True when the window no longer matches the settings (the original game re-applies its own
        // saved fullscreen / resolution when a scene loads).
        public static bool DisplayDiffers()
        {
            var t = TargetSize();
            if (Screen.fullScreenMode != Mode) return true;
            return DisplayMode != 2 && (Mathf.Abs(Screen.width - t.x) > 2 || Mathf.Abs(Screen.height - t.y) > 2);
        }
    }

    // In-game options overlay (F1). Drawn with IMGUI so it does not depend on the game's UI.
    public class OptionsMenu : MonoBehaviour
    {
        const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static OptionsMenu? Instance;
        bool open;
        public static bool IsOpen => Instance != null && Instance.open;
        public static void SetOpenForTest(bool v) => Instance?.SetOpen(v);   // test bench only
        public static void SetTabForTest(int t) { if (Instance != null) Instance.tab = t; }   // test bench only

        public static void Create()
        {
            if (Instance) return;
            var go = new GameObject("JetBootOptions");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<OptionsMenu>();
        }

        // ---------------- Esc: quit confirmation ----------------

        bool quitAsk;
        float quitTimeScale = 1f;
        bool quitCursorVisible;
        CursorLockMode quitLock;

        void AskQuit()
        {
            if (quitAsk) return;
            quitAsk = true;
            quitTimeScale = Time.timeScale; Time.timeScale = 0f;
            quitCursorVisible = Cursor.visible; quitLock = Cursor.lockState;
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
            AudioListener.pause = true;
        }

        void CloseQuit()
        {
            if (!quitAsk) return;
            quitAsk = false;
            if (Time.timeScale == 0f) Time.timeScale = quitTimeScale;
            Cursor.visible = quitCursorVisible; Cursor.lockState = quitLock;
            AudioListener.pause = false;
        }

        void ConfirmQuit()
        {
            Settings.Save();
            Application.Quit();
        }

        // Gamepad: A / Cross (menu next) quits, B / Circle (menu back) cancels.
        void QuitPad()
        {
            if (!quitAsk || !Rewired.ReInput.isReady) return;
            var p = Rewired.ReInput.players.GetPlayer(0);
            if (p == null) return;
            if (p.GetButtonDown("menu back")) CloseQuit();
            else if (p.GetButtonDown("menu next")) ConfirmQuit();
        }

        void DrawQuit()
        {
            float s = Mathf.Clamp(Screen.height / 1080f, 0.7f, 2f);
            BuildStyles(s);
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = 620 * s, h = 250 * s;
            var r = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
            GUI.Box(r, GUIContent.none, box);
            GUILayout.BeginArea(new Rect(r.x + 30 * s, r.y + 24 * s, r.width - 60 * s, r.height - 48 * s));
            GUILayout.Label(L.T("Quitter le jeu ?"), title);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L.T("Quitter"), toggleOn, GUILayout.Height(48 * s))) ConfirmQuit();
            GUILayout.Space(16 * s);
            if (GUILayout.Button(L.T("Annuler"), button, GUILayout.Height(48 * s))) CloseQuit();
            GUILayout.EndHorizontal();
            GUILayout.Space(8 * s);
            GUILayout.Label(L.T("Entrée : quitter   —   Échap : annuler"), small);
            GUILayout.EndArea();
        }

        // ---------------- menu ----------------

        int tab;
        float savedTimeScale = 1f;
        bool savedCursorVisible;
        CursorLockMode savedLock;
        Vector2 scroll;
        Resolution[] resolutions = new Resolution[0];
        int resIndex;
        float nextEnforce;
        int displayFixes;
        GUIStyle? title, header, label, button, toggleOn, toggleOff, box, small;
        Texture2D? bg, btnBg, btnHover, onBg;
        readonly Controls controls = new Controls();

        void Start()
        {
            Settings.Load();
            Settings.ApplyDisplay();
            RefreshResolutions();
            SceneManager.sceneLoaded += (s, m) => { nextEnforce = 0f; displayFixes = 0; cache.Clear(); Halftone.Apply(Settings.LightHalftone, true); };
            GameAudio.Install();
            Hud.Install();
        }

        void RefreshResolutions()
        {
            resolutions = Screen.resolutions
                .GroupBy(r => (r.width, r.height)).Select(g => g.First())
                .OrderBy(r => r.width).ThenBy(r => r.height).ToArray();
            resIndex = Array.FindIndex(resolutions, r => r.width == Settings.Width && r.height == Settings.Height);
            if (resIndex < 0) resIndex = resolutions.Length - 1;
        }

        void Update()
        {
            QuitPad();
            // The game shows the arcade bezel on its own (intro, end of pause): hide it again when disabled.
            if (!Settings.Bezel)
                foreach (var f in Bezels())
                    if (f.activeSelf) f.SetActive(false);

            // Re-apply the graphics toggles regularly: the game's own FSMs may reset them.
            if (Time.unscaledTime >= nextEnforce)
            {
                nextEnforce = Time.unscaledTime + 0.5f;
                ApplyGraphics();
                // A few attempts per scene only, so a mode the system refuses cannot cause endless flicker.
                if (displayFixes < 6 && Settings.DisplayDiffers())
                {
                    displayFixes++;
                    Debug.Log($"[JetBoot] display reset by the game ({Screen.fullScreenMode} {Screen.width}x{Screen.height}), re-applying settings");
                    Settings.ApplyDisplay();
                }
            }
        }

        // ---------- graphics ----------

        static readonly Dictionary<string, GameObject?> cache = new Dictionary<string, GameObject?>();
        static readonly List<GameObject> bezels = new List<GameObject>();
        static int bezelScene = -1;

        static GameObject? FindAnywhere(string name)
        {
            if (cache.TryGetValue(name, out var c) && c) return c;
            GameObject? found = null;
            foreach (var t in AllNamed(name)) { found = t.gameObject; break; }
            cache[name] = found;
            return found;
        }

        static IEnumerable<Transform> AllNamed(string name)
        {
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        if (t.name == name) yield return t;
            }
        }

        // Both arcade frames: the one of the "PRESS START" screen and the one of the game itself.
        static List<GameObject> Bezels()
        {
            int scene = SceneManager.GetActiveScene().handle;
            if (bezelScene != scene || bezels.Any(b => !b))
            {
                bezelScene = scene;
                bezels.Clear();
                bezels.AddRange(AllNamed("crt_frame").Select(t => t.gameObject));
            }
            return bezels;
        }

        static bool bezelHidden;

        public static void ApplyGraphics()
        {
            // Turning the bezel back on: show the frames again (the game hides them itself when it needs to).
            if (Settings.Bezel && bezelHidden) foreach (var f in Bezels()) f.SetActive(true);
            bezelHidden = !Settings.Bezel;

            var crt = FindAnywhere("post_process_crt");
            var normal = FindAnywhere("post process");
            if (crt && normal)
            {
                // Only swap when one of the two volumes is showing (game over etc. may change them).
                if (Settings.Crt && !crt!.activeSelf && normal!.activeSelf) { normal.SetActive(false); crt.SetActive(true); }
                if (!Settings.Crt && crt!.activeSelf) { crt.SetActive(false); normal!.SetActive(true); }
            }

            SetRendererFeature("OutlineRenderFeature", Settings.Outline);
            SetRendererFeature("BeautifyRendererFeature", Settings.Beautify);
            Halftone.Apply(Settings.LightHalftone);
            ApplyAntiAliasing();
        }

        // URP camera anti-aliasing (UniversalAdditionalCameraData.antialiasing), set by reflection.
        static void ApplyAntiAliasing()
        {
            if (Settings.AntiAliasing < 0) return;
            foreach (var cam in Camera.allCameras)
                foreach (var mb in cam.GetComponents<MonoBehaviour>())
                {
                    if (!mb || mb.GetType().Name != "UniversalAdditionalCameraData") continue;
                    var p = mb.GetType().GetProperty("antialiasing", F);
                    if (p == null) continue;
                    var want = Enum.ToObject(p.PropertyType, Settings.AntiAliasing);
                    if (!Equals(p.GetValue(mb), want)) p.SetValue(mb, want);
                }
        }

        static void SetRendererFeature(string typeName, bool active)
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (!rp) return;
            if (!(rp.GetType().GetField("m_RendererDataList", F)?.GetValue(rp) is Array list)) return;
            foreach (var rd in list)
            {
                if (rd == null) continue;
                if (!(rd.GetType().GetProperty("rendererFeatures", F)?.GetValue(rd) is IList feats)) continue;
                foreach (var f in feats)
                {
                    if (f == null || f.GetType().Name != typeName) continue;
                    var cur = f.GetType().GetProperty("isActive", F)?.GetValue(f) as bool?;
                    if (cur != active) f.GetType().GetMethod("SetActive", F)?.Invoke(f, new object[] { active });
                }
            }
        }

        // ---------- IMGUI ----------

        static Texture2D Tex(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c); t.Apply();
            return t;
        }

        void BuildStyles(float s)
        {
            bg ??= Tex(new Color(0.04f, 0.06f, 0.10f, 0.94f));
            btnBg ??= Tex(new Color(0.16f, 0.20f, 0.28f, 1f));
            btnHover ??= Tex(new Color(0.26f, 0.34f, 0.48f, 1f));
            onBg ??= Tex(new Color(0.95f, 0.55f, 0.10f, 1f));
            int fs = Mathf.RoundToInt(18 * s);
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg } };
            title = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(34 * s), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.6f, 0.1f) } };
            header = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(22 * s), fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            label = new GUIStyle(GUI.skin.label) { fontSize = fs, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } };
            small = new GUIStyle(label) { fontSize = Mathf.RoundToInt(14 * s), wordWrap = true, normal = { textColor = new Color(0.7f, 0.7f, 0.75f) } };
            button = new GUIStyle(GUI.skin.button)
            {
                fontSize = fs, normal = { background = btnBg, textColor = Color.white }, hover = { background = btnHover, textColor = Color.white },
                active = { background = onBg, textColor = Color.black }, padding = new RectOffset(10, 10, 6, 6),
            };
            toggleOn = new GUIStyle(button) { normal = { background = onBg, textColor = Color.black }, hover = { background = onBg, textColor = Color.black }, fontStyle = FontStyle.Bold };
            toggleOff = button;
        }

        void SetOpen(bool v)
        {
            if (open == v) return;
            open = v;
            if (open)
            {
                savedTimeScale = Time.timeScale; Time.timeScale = 0f;
                savedCursorVisible = Cursor.visible; savedLock = Cursor.lockState;
                Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
                AudioListener.pause = true;
                RefreshResolutions();
            }
            else
            {
                controls.CancelRebind();
                Time.timeScale = savedTimeScale;
                Cursor.visible = savedCursorVisible; Cursor.lockState = savedLock;
                AudioListener.pause = false;
                Settings.Save();
            }
        }

        void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F1 && !controls.Rebinding && !quitAsk)
            {
                SetOpen(!open);
                e.Use();
            }
            if (quitAsk)
            {
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.N) { CloseQuit(); e.Use(); return; }
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.O || e.keyCode == KeyCode.Y) { ConfirmQuit(); e.Use(); return; }
                }
                DrawQuit();
                return;
            }
            if (!open && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { AskQuit(); e.Use(); return; }
            if (!open)
            {
                if (Time.unscaledTime < 12f)
                {
                    BuildStyles(Mathf.Max(0.75f, Screen.height / 1080f));
                    GUI.Label(new Rect(20, Screen.height - 50, 900, 40), L.T("F1 : options   —   Échap : quitter"), small);
                }
                return;
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape && !controls.Rebinding) { SetOpen(false); e.Use(); return; }

            float s = Mathf.Clamp(Screen.height / 1080f, 0.7f, 2f);
            BuildStyles(s);
            float w = 900 * s, h = 860 * s;
            var r = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
            GUI.Box(r, GUIContent.none, box);
            GUILayout.BeginArea(new Rect(r.x + 30 * s, r.y + 20 * s, r.width - 60 * s, r.height - 40 * s));
            GUILayout.Label("JET FURY — OPTIONS", title);
            GUILayout.Space(10 * s);
            GUILayout.BeginHorizontal();
            string[] tabs = { L.T("Affichage"), L.T("Graphismes"), L.T("HUD"), L.T("Audio"), L.T("Contrôles") };
            for (int i = 0; i < tabs.Length; i++)
                if (GUILayout.Button(tabs[i], i == tab ? toggleOn : toggleOff, GUILayout.Height(44 * s))) tab = i;
            GUILayout.EndHorizontal();
            GUILayout.Space(16 * s);

            scroll = GUILayout.BeginScrollView(scroll);
            if (tab == 0) DrawDisplay(s);
            else if (tab == 1) DrawGraphics(s);
            else if (tab == 2) DrawHud(s);
            else if (tab == 3) DrawAudio(s);
            else controls.Draw(s, header!, label!, small!, button!, toggleOn!);
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L.T("Fermer (F1 / Échap)"), button, GUILayout.Height(44 * s))) SetOpen(false);
            if (GUILayout.Button(L.T("Quitter le jeu"), button, GUILayout.Height(44 * s), GUILayout.Width(220 * s))) { Settings.Save(); Application.Quit(); }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        bool Row(string name, bool value, float s)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, label, GUILayout.Height(40 * s));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(L.T("Oui"), value ? toggleOn : toggleOff, GUILayout.Width(110 * s), GUILayout.Height(40 * s))) value = true;
            if (GUILayout.Button(L.T("Non"), !value ? toggleOn : toggleOff, GUILayout.Width(110 * s), GUILayout.Height(40 * s))) value = false;
            GUILayout.EndHorizontal();
            GUILayout.Space(6 * s);
            return value;
        }

        void DrawDisplay(float s)
        {
            GUILayout.Label(L.T("Langue"), header);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < L.Codes.Length; i++)
                if (GUILayout.Button(L.Names[i], L.Code == L.Codes[i] ? toggleOn : toggleOff, GUILayout.Height(42 * s))) L.Code = L.Codes[i];
            GUILayout.EndHorizontal();
            GUILayout.Space(14 * s);
            GUILayout.Label(L.T("Mode d'affichage"), header);
            GUILayout.BeginHorizontal();
            string[] modes = { L.T("Plein écran"), L.T("Plein écran fenêtré"), L.T("Fenêtré") };
            for (int i = 0; i < 3; i++)
                if (GUILayout.Button(modes[i], Settings.DisplayMode == i ? toggleOn : toggleOff, GUILayout.Height(42 * s)))
                {
                    Settings.DisplayMode = i;
                    var native = Screen.currentResolution;
                    if (i != 2 && Settings.Width < 640) { Settings.Width = native.width; Settings.Height = native.height; }
                    Settings.ApplyDisplay();
                }
            GUILayout.EndHorizontal();
            GUILayout.Space(16 * s);

            GUILayout.Label(L.T("Résolution"), header);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄", button, GUILayout.Width(60 * s), GUILayout.Height(42 * s)) && resolutions.Length > 0)
                resIndex = (resIndex - 1 + resolutions.Length) % resolutions.Length;
            string cur = resolutions.Length > 0 && resIndex >= 0 ? resolutions[resIndex].width + " × " + resolutions[resIndex].height : "?";
            GUILayout.Label(cur, new GUIStyle(label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Height(42 * s));
            if (GUILayout.Button("►", button, GUILayout.Width(60 * s), GUILayout.Height(42 * s)) && resolutions.Length > 0)
                resIndex = (resIndex + 1) % resolutions.Length;
            if (GUILayout.Button(L.T("Appliquer"), button, GUILayout.Width(160 * s), GUILayout.Height(42 * s)) && resolutions.Length > 0)
            {
                Settings.Width = resolutions[resIndex].width;
                Settings.Height = resolutions[resIndex].height;
                Settings.ApplyDisplay();
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(L.T("Actuelle : ") + Screen.width + " × " + Screen.height, small);
            GUILayout.Space(16 * s);

            bool v = Row(L.T("Synchronisation verticale (V-Sync)"), Settings.VSync, s);
            if (v != Settings.VSync) { Settings.VSync = v; Settings.ApplyDisplay(); }
        }

        void DrawGraphics(float s)
        {
            Settings.Bezel = Row(L.T("Bezel (cadre de la borne d'arcade)"), Settings.Bezel, s);
            Settings.Crt = Row(L.T("Effet écran CRT (courbure de l'écran)"), Settings.Crt, s);
            Settings.Outline = Row(L.T("Contours noirs (cel shading)"), Settings.Outline, s);
            Settings.Beautify = Row(L.T("Post-traitement BD (trame, couleurs)"), Settings.Beautify, s);
            bool lh = Row(L.T("Trame BD sur les éclairages (halftone)"), Settings.LightHalftone, s);
            if (lh != Settings.LightHalftone) { Settings.LightHalftone = lh; Halftone.Apply(lh, true); }

            GUILayout.Space(10 * s);
            GUILayout.Label(L.T("Anticrénelage"), header);
            GUILayout.BeginHorizontal();
            string[] aa = { L.T("Jeu d'origine"), L.T("Aucun"), "FXAA", "SMAA", "TAA" };
            for (int i = 0; i < aa.Length; i++)
                if (GUILayout.Button(aa[i], Settings.AntiAliasing == i - 1 ? toggleOn : toggleOff, GUILayout.Height(42 * s))) Settings.AntiAliasing = i - 1;
            GUILayout.EndHorizontal();

            GUILayout.Space(10 * s);
            GUILayout.Label(L.T("Les changements sont appliqués immédiatement et enregistrés à la fermeture du menu."), small);
            ApplyGraphics();
        }

        void DrawHud(float s)
        {
            GUILayout.Label(L.T("Disposition"), header);
            Settings.HudFit = Row(L.T("Garder tout le HUD à l'écran (écrans larges)"), Settings.HudFit, s);
            GUILayout.Label(L.T("Le HUD d'origine est prévu pour le 4:3 : en 16:9 et 21:9, le haut et le bas sortent de l'écran. Cette option le ramène dans l'image."), small);
            Settings.HudScale = Slider(L.T("Taille"), Settings.HudScale, 0.5f, 1.5f, "", s);
            Settings.HudSpreadX = Slider(L.T("Écartement horizontal"), Settings.HudSpreadX, 0f, 1f, "", s);
            Settings.HudSpreadY = Slider(L.T("Écartement vertical"), Settings.HudSpreadY, 0f, 1f, "", s);
            Settings.HudMargin = Slider(L.T("Marge aux bords"), Settings.HudMargin, 0f, 0.15f, "", s);
            GUILayout.Label(L.T("Écartement à 1 : blocs plaqués contre les bords de l'image (avec le bezel, contre son écran)."), small);
            GUILayout.Space(10 * s);
            GUILayout.Label(L.T("Éléments affichés"), header);
            foreach (var g in Hud.Groups)
                if (g.Listed) Settings.SetHudShown(g.Key, Row(L.T(g.Name), Settings.HudShown(g.Key), s));
            Settings.SetHudShown(Hud.Instruments, Row(L.T("Instruments de vol (horizon, assiette)"), Settings.HudShown(Hud.Instruments), s));
            Settings.SetHudShown(Hud.Targets, Row(L.T("Cadres de visée sur les ennemis"), Settings.HudShown(Hud.Targets), s));
            if (GUILayout.Button(L.T("Valeurs par défaut"), button, GUILayout.Height(40 * s)))
            {
                Settings.HudFit = true; Settings.HudScale = 1f; Settings.HudSpreadX = 0f; Settings.HudSpreadY = 0f;
                Settings.HudMargin = 0.03f; Settings.HudHidden = new HashSet<string>();
            }
        }

        float Slider(string name, float v, float min, float max, string unit, float s)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, label, GUILayout.Height(36 * s), GUILayout.Width(300 * s));
            v = GUILayout.HorizontalSlider(v, min, max, GUILayout.Height(36 * s));
            GUILayout.Label(v.ToString("0.00") + unit, label, GUILayout.Width(110 * s));
            GUILayout.EndHorizontal();
            return v;
        }

        List<string>? trackIds;
        List<string>? trackNames;
        float tracksAt = -10f;

        void DrawAudio(float s)
        {
            GUILayout.Label(L.T("Volumes"), header);
            Settings.VolMaster = Slider(L.T("Général"), Settings.VolMaster, 0f, 1f, "", s);
            Settings.VolMusic = Slider(L.T("Musique"), Settings.VolMusic, 0f, 1f, "", s);
            Settings.VolEngine = Slider(L.T("Moteurs (avions)"), Settings.VolEngine, 0f, 1f, "", s);
            Settings.VolFx = Slider(L.T("Effets sonores"), Settings.VolFx, 0f, 1f, "", s);

            GUILayout.Space(14 * s);
            GUILayout.Label(L.T("Bande-son"), header);
            if (trackIds == null || (Event.current.type == EventType.Layout && Time.unscaledTime - tracksAt > 2f))
            {
                tracksAt = Time.unscaledTime;
                trackIds = new List<string> { GameAudio.Original };
                trackNames = new List<string> { L.T("Originale (Jet Fury)") };
                var files = GameAudio.UserFiles();
                if (files.Count > 1) { trackIds.Add(GameAudio.Random); trackNames.Add(L.T("Aléatoire (vos musiques)")); }
                foreach (var f in files) { trackIds.Add("file:" + f); trackNames.Add("♪ " + f); }
            }
            int idx = Mathf.Max(0, trackIds.IndexOf(Settings.Soundtrack));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄", button, GUILayout.Width(60 * s), GUILayout.Height(42 * s))) idx = (idx + trackIds.Count - 1) % trackIds.Count;
            GUILayout.Label(trackNames![idx], header, GUILayout.Height(42 * s));
            if (GUILayout.Button("►", button, GUILayout.Width(60 * s), GUILayout.Height(42 * s))) idx = (idx + 1) % trackIds.Count;
            GUILayout.EndHorizontal();
            Settings.Soundtrack = trackIds[idx];
            GUILayout.Label(L.T("Vos propres musiques (mp3, ogg, wav) : placez-les dans le dossier « Music » à côté de JetFury.exe."), small);
        }
    }

    // Comic "halftone" texture applied to the bloom (lens dirt) of the post-process volumes.
    public static class Halftone
    {
        const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly Dictionary<object, float> original = new Dictionary<object, float>();
        static float nextScan;

        public static void Apply(bool enabled, bool force = false)
        {
            if (!force && Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1f;
            foreach (var v in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (!v || v.GetType().Name != "Volume" || !v.gameObject.scene.IsValid()) continue;
                var prof = v.GetType().GetField("sharedProfile", F)?.GetValue(v);
                if (!(prof?.GetType().GetField("components", F)?.GetValue(prof) is IList comps)) continue;
                foreach (var c in comps)
                {
                    if (c == null || c.GetType().Name != "Bloom") continue;
                    var param = c.GetType().GetField("dirtIntensity", F)?.GetValue(c);
                    if (param == null) continue;
                    var valueProp = param.GetType().GetProperty("value", F);
                    var overrideProp = param.GetType().GetProperty("overrideState", F);
                    if (valueProp == null) continue;
                    float cur = (float)valueProp.GetValue(param);
                    if (!original.ContainsKey(param)) original[param] = cur;
                    float want = enabled ? original[param] : 0f;
                    if (cur != want)
                    {
                        overrideProp?.SetValue(param, true);
                        valueProp.SetValue(param, want);
                    }
                }
            }
        }
    }
}
