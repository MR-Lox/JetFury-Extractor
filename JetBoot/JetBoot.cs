using UnityEngine;
using UnityEngine.SceneManagement;

namespace JetBoot
{
    // Boots the hidden "Bonus_Afterburner" plane game directly and keeps the player inside it.
    public static class Boot
    {
        public const int JetScene = 26;  // Assets/TAKEOVER/scenes/Bonus_Afterburner.unity
        const int IntroScene = 0;
        const int TransitionScene = 1;   // T2_transition_scene (loading screen)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            OptionsMenu.Create();
            Debug.Log("[JetBoot] installed");
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("[JetBoot] loaded " + scene.buildIndex + " " + scene.name + " " + mode);
            int i = scene.buildIndex;
            if (i == JetScene) { SceneDumper.Run(); TestBench.Run(); return; }
            if (mode == LoadSceneMode.Additive) return;
            // Intro (Takeover 2 logos), main menu and loading screen: let them run a couple of
            // frames so their global managers initialise, then jump to the plane game.
            int frames = i == IntroScene || i == TransitionScene ? 3 : 0;
            Redirect(frames);
        }

        static bool redirecting;

        static void Redirect(int frames)
        {
            if (redirecting) return;
            if (frames <= 0 || !OptionsMenu.Instance) { Load(); return; }
            redirecting = true;
            OptionsMenu.Instance!.StartCoroutine(Delayed(frames));
        }

        static System.Collections.IEnumerator Delayed(int frames)
        {
            for (int f = 0; f < frames; f++) yield return null;
            redirecting = false;
            if (SceneManager.GetActiveScene().buildIndex != JetScene) Load();
        }

        static void Load()
        {
            Debug.Log("[JetBoot] redirecting to plane game");
            SceneManager.LoadScene(JetScene, LoadSceneMode.Single);
        }
    }
}
