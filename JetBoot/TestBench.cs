using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace JetBoot
{
    // Development test bench, enabled by "testbench.txt" next to the executable.
    //  Line 1: number of screenshots (default 20), line 2: interval in seconds (default 2).
    //  Saves shots/sNN.png and logs the active state of every PlayMaker FSM over time.
    public class TestBench : MonoBehaviour
    {
        static string Dir => Path.Combine(Application.dataPath, "..");

        public static void Run()
        {
            var f = Path.Combine(Dir, "testbench.txt");
            if (!File.Exists(f) || FindObjectOfType<TestBench>()) return;
            var go = new GameObject("JetBootTestBench");
            DontDestroyOnLoad(go);
            go.AddComponent<TestBench>();
        }

        IEnumerator Start()
        {
            Application.runInBackground = true;
            var lines = File.ReadAllLines(Path.Combine(Dir, "testbench.txt"));
            int count = lines.Length > 0 && int.TryParse(lines[0], out var c) ? c : 20;
            float every = lines.Length > 1 && float.TryParse(lines[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var e) ? e : 2f;
            string shots = Path.Combine(Dir, "shots");
            Directory.CreateDirectory(shots);
            var log = new StringBuilder();
            bool pressStart = lines.Length > 2 && lines[2].Trim() == "start";
            for (int i = 0; i < count; i++)
            {
                if (pressStart && i == 2) SendFsm("AB_intro", "intro", "next");
                yield return new WaitForSecondsRealtime(every);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, $"s{i:00}.png"));
                log.AppendLine($"--- t={Time.realtimeSinceStartup:F1} timeScale={Time.timeScale}");
                foreach (var m in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
                {
                    if (!m || m.GetType().Name != "PlayMakerFSM" || !m.gameObject.scene.IsValid() || !m.isActiveAndEnabled) continue;
                    var st = m.GetType().GetProperty("ActiveStateName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(m);
                    log.AppendLine($" {m.gameObject.name} :: {m.GetType().GetProperty("FsmName")?.GetValue(m)} -> {st}");
                }
                File.WriteAllText(Path.Combine(shots, "fsm_log.txt"), log.ToString());
            }
            if (lines.Length > 3 && lines[3].Trim() == "menu")
            {
                for (int t = 0; t < 5; t++)
                {
                    OptionsMenu.SetOpenForTest(true);
                    OptionsMenu.SetTabForTest(t);
                    yield return new WaitForSecondsRealtime(1f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(shots, $"menu{t}.png"));
                    yield return new WaitForSecondsRealtime(0.5f);
                }
                OptionsMenu.SetOpenForTest(false);
                Settings.Bezel = false; Settings.Crt = false; Settings.Outline = false; Settings.Beautify = false;
                yield return new WaitForSecondsRealtime(1.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "fx_off.png"));
                yield return new WaitForSecondsRealtime(0.5f);
                Settings.Bezel = true; Settings.Crt = true; Settings.Outline = true; Settings.Beautify = true;
                yield return new WaitForSecondsRealtime(1.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "fx_on.png"));
                yield return new WaitForSecondsRealtime(0.5f);
            }
            if (lines.Length > 3 && lines[3].Trim() == "hud")
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "hud_fit.png"));
                yield return new WaitForSecondsRealtime(0.5f);
                Settings.HudSpreadX = 1f; Settings.HudSpreadY = 1f;
                yield return new WaitForSecondsRealtime(0.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "hud_spread.png"));
                yield return new WaitForSecondsRealtime(0.5f);
                Settings.Bezel = false;
                yield return new WaitForSecondsRealtime(1f);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "hud_spread_nobezel.png"));
                yield return new WaitForSecondsRealtime(0.5f);
                Settings.Bezel = true; Settings.HudSpreadX = 0f; Settings.HudSpreadY = 0f; Settings.HudFit = false;
                yield return new WaitForSecondsRealtime(0.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(shots, "hud_orig.png"));
                yield return new WaitForSecondsRealtime(0.5f);
                Settings.HudFit = true;
            }
            HudDump(shots);
            yield return new WaitForEndOfFrame();   // volumes as the audio engine gets them
            var a = new StringBuilder();
            foreach (var s in Resources.FindObjectsOfTypeAll<AudioSource>())
            {
                if (!s || !s.gameObject.scene.IsValid()) continue;
                var g = s.outputAudioMixerGroup;
                a.AppendLine($"{Full(s.transform)} active={s.gameObject.activeInHierarchy} playing={s.isPlaying} vol={s.volume:F2} clip={(s.clip ? s.clip.name : "")} mixer={(g ? g.audioMixer.name + "/" + g.name : "")}");
            }
            File.WriteAllText(Path.Combine(shots, "audio.txt"), a.ToString());
            Application.Quit();
        }

        static string Full(Transform t) => t.parent ? Full(t.parent) + "/" + t.name : t.name;

        // Canvases of the game HUD and the screen rectangle of each of their blocks (2 levels deep).
        static void HudDump(string dir)
        {
            var sb = new StringBuilder($"screen {Screen.width}x{Screen.height}\n");
            var corners = new Vector3[4];
            Camera? cam = null;
            foreach (var cv in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (!cv || !cv.gameObject.scene.IsValid() || !cv.isRootCanvas) continue;
                var sc = cv.GetComponent<UnityEngine.UI.CanvasScaler>();
                sb.AppendLine($"CANVAS {Full(cv.transform)} active={cv.gameObject.activeInHierarchy} mode={cv.renderMode} cam={(cv.worldCamera ? cv.worldCamera.name : "")} sort={cv.sortingOrder} scale={cv.scaleFactor:F3}"
                    + (sc ? $" scaler={sc.uiScaleMode} ref={sc.referenceResolution} match={sc.matchWidthOrHeight} screenMatch={sc.screenMatchMode}" : ""));
                cam = cv.renderMode == RenderMode.ScreenSpaceOverlay ? null : cv.worldCamera;
                Walk(cv.transform as RectTransform, 1);
            }
            File.WriteAllText(Path.Combine(dir, "hud.txt"), sb.ToString());

            void Walk(RectTransform? rt, int depth)
            {
                if (!rt || depth > 3) return;
                foreach (Transform ch in rt!)
                {
                    var r = ch as RectTransform;
                    if (!r) continue;
                    r!.GetWorldCorners(corners);
                    Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, corners[0]), b = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
                    sb.AppendLine($"{new string(' ', depth * 2)}{(r.gameObject.activeSelf ? "+" : "-")} {r.name} anchor={r.anchorMin}-{r.anchorMax} pos={r.anchoredPosition} size={r.sizeDelta} scr=({a.x:F0},{a.y:F0})-({b.x:F0},{b.y:F0})");
                    Walk(r, depth + 1);
                }
            }
        }

        static void SendFsm(string go, string fsmName, string evt)
        {
            foreach (var m in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
                if (m && m.GetType().Name == "PlayMakerFSM" && m.gameObject.name == go
                    && (string?)m.GetType().GetProperty("FsmName")?.GetValue(m) == fsmName)
                {
                    m.GetType().GetMethod("SendEvent", new[] { typeof(string) })?.Invoke(m, new object[] { evt });
                    Debug.Log($"[JetBoot] sent {evt} to {go}/{fsmName}");
                }
        }
    }
}
