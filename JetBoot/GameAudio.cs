using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace JetBoot
{
    // Audio options: master / music / engines / effects volumes, and the soundtrack.
    //  - Volumes: the game keeps setting the volume of its sources; whatever it writes becomes the base, and
    //    the chosen level is applied on top (same idea as the HUD).
    //  - Soundtrack: the original Jet Fury music, or the player's own files (mp3 / ogg / wav) from the "Music"
    //    folder next to the executable, one of them or all at random. The game's music FSM is paused while
    //    another track plays.
    // Runs after the game's own LateUpdate FSMs (the jet engine FSM writes its volume every frame).
    [DefaultExecutionOrder(32000)]
    public class GameAudio : MonoBehaviour
    {
        public const string Original = "", Random = "*random";
        static GameAudio? inst;

        public static void Install()
        {
            if (inst) return;
            var go = new GameObject("JetBootAudio");
            DontDestroyOnLoad(go);
            inst = go.AddComponent<GameAudio>();
        }

        // ---------------- volumes ----------------

        class Vol { public float Base, Written; }
        readonly Dictionary<AudioSource, Vol> vols = new Dictionary<AudioSource, Vol>();
        AudioSource[] sources = new AudioSource[0];
        float nextScan;

        void LateUpdate()
        {
            AudioListener.volume = Settings.VolMaster;
            if (Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + 0.5f;
                sources = FindObjectsOfType<AudioSource>();
                if (vols.Count > 600) foreach (var k in vols.Keys.Where(k => !k).ToList()) vols.Remove(k);
                FindMusic();
            }
            foreach (var src in sources)
            {
                if (!src) continue;
                float mult = src == music ? Settings.VolMusic : IsEngine(src) ? Settings.VolEngine : Settings.VolFx;
                if (!vols.TryGetValue(src, out var v)) { v = new Vol { Base = src.volume }; vols[src] = v; }
                else if (Mathf.Abs(src.volume - v.Written) > 0.0001f) v.Base = src.volume;   // written by the game
                v.Written = v.Base * mult;
                src.volume = v.Written;
            }
            Soundtrack();
        }

        // Jet engines (player and enemies) have no mixer group: recognise them by their looping clip.
        static bool IsEngine(AudioSource s) => s.clip && s.clip!.name.IndexOf("engine", System.StringComparison.OrdinalIgnoreCase) >= 0;

        // ---------------- soundtrack ----------------

        AudioSource? music;
        MonoBehaviour? musicFsm;
        AudioClip? playing;
        string applied = Original;
        readonly Dictionary<string, AudioClip> files = new Dictionary<string, AudioClip>();
        readonly HashSet<string> loading = new HashSet<string>();

        static string MusicDir => Path.Combine(Application.dataPath, "..", "Music");

        void FindMusic()
        {
            if (music && music!.gameObject.activeInHierarchy) return;
            music = null; musicFsm = null;
            foreach (var s in Resources.FindObjectsOfTypeAll<AudioSource>())
                if (s && s.name == "music" && s.gameObject.scene.IsValid() && s.gameObject.activeInHierarchy && s.transform.parent && s.transform.parent.name == "loading object")
                {
                    music = s;
                    foreach (var mb in s.GetComponents<MonoBehaviour>())
                        if (mb.GetType().Name == "PlayMakerFSM" && mb.GetType().GetProperty("FsmName")?.GetValue(mb) as string == "music") musicFsm = mb;
                    break;
                }
        }

        public static List<string> UserFiles()
        {
            try
            {
                if (!Directory.Exists(MusicDir)) return new List<string>();
                return Directory.GetFiles(MusicDir).Where(f => f.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName).OrderBy(f => f).ToList()!;
            }
            catch { return new List<string>(); }
        }

        void Soundtrack()
        {
            if (!music) return;
            string want = Settings.Soundtrack;
            if (want == Original)
            {
                if (applied != Original)
                {
                    // Back to the game's own music: its FSM picks the Jet Fury track again.
                    applied = Original;
                    playing = null;
                    if (musicFsm) { musicFsm!.enabled = false; musicFsm.enabled = true; }
                }
                return;
            }

            if (musicFsm && musicFsm!.enabled) musicFsm.enabled = false;
            bool finished = playing && music!.clip == playing && !music.isPlaying && music.time == 0f;
            if (want != applied || !playing || music!.clip != playing || finished)
            {
                var clip = Pick(want, finished);
                if (!clip) return;   // file still loading
                applied = want;
                playing = clip;
                music!.Stop();
                music.clip = clip;
                music.loop = want != Random;
                music.time = 0f;
                music.Play();
            }
            else if (!music!.isPlaying && music.loop) music.Play();
        }

        AudioClip? Pick(string want, bool next)
        {
            if (want == Random)
            {
                // Next random file (not the one that just played).
                if (randomNext == null)
                {
                    var all = UserFiles();
                    if (all.Count == 0) return null;
                    string pick;
                    int guard = 0;
                    do pick = all[UnityEngine.Random.Range(0, all.Count)]; while (all.Count > 1 && playing && pick == playing!.name && guard++ < 10);
                    randomNext = pick;
                }
                var c = FileClip(randomNext);
                if (c) randomNext = null;
                return c;
            }
            if (want.StartsWith("file:")) return FileClip(want.Substring(5));
            return null;
        }

        string? randomNext;

        AudioClip? FileClip(string name)
        {
            if (files.TryGetValue(name, out var c)) return c;
            if (loading.Add(name)) StartCoroutine(LoadFile(name));
            return null;
        }

        IEnumerator LoadFile(string name)
        {
            string path = Path.Combine(MusicDir, name);
            var type = name.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase) ? AudioType.MPEG
                : name.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase) ? AudioType.OGGVORBIS : AudioType.WAV;
            using (var req = UnityWebRequestMultimedia.GetAudioClip(new System.Uri(Path.GetFullPath(path)).AbsoluteUri, type))
            {
                ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(req);
                    clip.name = name;
                    files[name] = clip;
                }
                else Debug.Log("[JetBoot] audio: cannot load " + name + ": " + req.error);
            }
            loading.Remove(name);
        }
    }
}
