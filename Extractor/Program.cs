using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;

namespace Extractor;

static class Program
{
    const string GameExe = "Takeover2.exe";
    const string GameData = "Takeover2_Data";
    const string OutExe = "JetFury.exe";
    const string OutData = "JetFury_Data";

    static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== Extracteur du jeu d'avion caché de Takeover 2 « Jet Fury » (scène Bonus_Afterburner) ===\n");
        try
        {
            string game = FindGameFolder(args.Length > 0 ? args[0] : null);
            string output = args.Length > 1 ? Path.GetFullPath(args[1])
                : Path.Combine(Path.GetDirectoryName(game.TrimEnd('\\', '/')) ?? game, "JetFury");
            Console.WriteLine($"Jeu d'origine : {game}");
            Console.WriteLine($"Destination   : {output}\n");
            Extract(game, output);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nTerminé ! Lancez : {Path.Combine(output, OutExe)}");
            Console.ResetColor();
            Pause(args);
            return 0;
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nErreur : {e.Message}");
            Console.ResetColor();
            Pause(args);
            return 1;
        }
    }

    static void Pause(string[] args)
    {
        if (args.Length == 0 && !Console.IsInputRedirected)
        {
            Console.WriteLine("\nAppuyez sur Entrée pour fermer.");
            Console.ReadLine();
        }
    }

    static bool IsGameFolder(string? d) =>
        !string.IsNullOrEmpty(d) && File.Exists(Path.Combine(d, GameExe)) && Directory.Exists(Path.Combine(d, GameData));

    static string FindGameFolder(string? arg)
    {
        var candidates = new List<string?> { arg, Environment.CurrentDirectory, AppContext.BaseDirectory };
        foreach (var c in candidates.ToList())
            if (!string.IsNullOrEmpty(c)) candidates.Add(Path.GetDirectoryName(Path.GetFullPath(c).TrimEnd('\\', '/')));
        foreach (var c in candidates)
            if (IsGameFolder(c)) return Path.GetFullPath(c!);
        if (arg != null) throw new Exception($"'{arg}' ne contient pas {GameExe} et {GameData}.");
        if (Console.IsInputRedirected) throw new Exception("Dossier du jeu introuvable (passez-le en argument).");
        while (true)
        {
            Console.Write($"Chemin du dossier de Takeover 2 (contenant {GameExe}) : ");
            string? p = Console.ReadLine()?.Trim().Trim('"');
            if (IsGameFolder(p)) return Path.GetFullPath(p!);
            Console.WriteLine("  -> Dossier invalide, réessayez.");
        }
    }

    static void Extract(string game, string output)
    {
        if (Path.GetFullPath(output).TrimEnd('\\').Equals(game.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            throw new Exception("La destination ne peut pas être le dossier du jeu d'origine.");
        Directory.CreateDirectory(output);

        Step("Copie du moteur Unity");
        foreach (var f in new[] { "UnityPlayer.dll", "UnityCrashHandler64.exe", "steam_appid.txt" })
            if (File.Exists(Path.Combine(game, f))) CopyFile(Path.Combine(game, f), Path.Combine(output, f));
        CopyFile(Path.Combine(game, GameExe), Path.Combine(output, OutExe));
        foreach (var d in new[] { "D3D12", "MonoBleedingEdge" })
            if (Directory.Exists(Path.Combine(game, d))) CopyDir(Path.Combine(game, d), Path.Combine(output, d));

        Step("Copie des données du jeu (plusieurs Go, patience)");
        string outData = Path.Combine(output, OutData);
        CopyDir(Path.Combine(game, GameData), outData);

        Step("Installation du module de démarrage JetBoot");
        WriteResource("JetBoot.dll", Path.Combine(outData, "Managed", "JetBoot.dll"));
        PatchScriptingAssemblies(Path.Combine(outData, "ScriptingAssemblies.json"));
        PatchRuntimeInit(Path.Combine(outData, "RuntimeInitializeOnLoads.json"));

        Step("Vérification de 'unity default resources'");
        string res = Path.Combine(outData, "Resources", "unity default resources");
        if (IsSerializedFileIntact(res))
            Console.WriteLine("  OK");
        else if (HasResource("unity default resources"))
        {
            Console.WriteLine("  Fichier corrompu -> remplacé par la version Unity 6000.0.66f2 intégrée.");
            WriteResource("unity default resources", res);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ATTENTION : ce fichier semble corrompu dans votre installation (le jeu risque de planter).");
            Console.WriteLine("  Sur Steam : clic droit sur Takeover 2 > Propriétés > Fichiers installés >");
            Console.WriteLine("  « Vérifier l'intégrité des fichiers », puis relancez l'extracteur.");
            Console.ResetColor();
        }
    }

    static void Step(string s)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"> {s}");
        Console.ResetColor();
    }

    // Unity serialized file (format 22+): big-endian int64 file size at offset 0x18.
    static bool IsSerializedFileIntact(string path)
    {
        if (!File.Exists(path)) return false;
        using var fs = File.OpenRead(path);
        var h = new byte[0x30];
        if (fs.Read(h, 0, h.Length) != h.Length) return false;
        uint version = BEU32(h, 8);
        long declared = version >= 22 ? (long)BEU32(h, 0x18) << 32 | BEU32(h, 0x1C) : BEU32(h, 4);
        return declared == fs.Length;
    }

    static uint BEU32(byte[] b, int o) => (uint)(b[o] << 24 | b[o + 1] << 16 | b[o + 2] << 8 | b[o + 3]);

    static void PatchScriptingAssemblies(string path)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var names = root["names"]!.AsArray();
        var types = root["types"]!.AsArray();
        if (!names.Any(n => (string?)n == "JetBoot.dll")) { names.Add("JetBoot.dll"); types.Add(16); }
        File.WriteAllText(path, root.ToJsonString());
    }

    static void PatchRuntimeInit(string path)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var list = root["root"]!.AsArray();
        if (!list.Any(n => (string?)n?["assemblyName"] == "JetBoot"))
            list.Add(new JsonObject
            {
                ["assemblyName"] = "JetBoot", ["nameSpace"] = "JetBoot", ["className"] = "Boot",
                ["methodName"] = "Init", ["loadTypes"] = 1, ["isUnityClass"] = false
            });
        File.WriteAllText(path, root.ToJsonString());
    }

    static bool HasResource(string name) => Assembly.GetExecutingAssembly().GetManifestResourceInfo(name) != null;

    static void WriteResource(string name, string dest)
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                      ?? throw new Exception($"Ressource intégrée manquante : {name}");
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        using var o = File.Create(dest);
        s.CopyTo(o);
    }

    static void CopyDir(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var d in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(dst, Path.GetRelativePath(src, d)));
        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            CopyFile(f, Path.Combine(dst, Path.GetRelativePath(src, f)));
    }

    static void CopyFile(string src, string dst)
    {
        var si = new FileInfo(src);
        var di = new FileInfo(dst);
        // Skip files already copied by a previous run (same size and date).
        if (di.Exists && di.Length == si.Length && di.LastWriteTimeUtc == si.LastWriteTimeUtc) return;
        if (si.Length < 64L << 20)
        {
            File.Copy(src, dst, true);
            File.SetLastWriteTimeUtc(dst, si.LastWriteTimeUtc);
            return;
        }

        Console.WriteLine($"  {si.Name} ({si.Length / (1024 * 1024)} Mo)");
        var buf = new byte[4 << 20];
        using (var i = File.OpenRead(src))
        using (var o = File.Create(dst))
        {
            long done = 0; int lastPct = -1; int n;
            while ((n = i.Read(buf, 0, buf.Length)) > 0)
            {
                o.Write(buf, 0, n); done += n;
                int pct = (int)(done * 100 / si.Length);
                if (pct != lastPct) { Console.Write($"\r    {pct,3} %"); lastPct = pct; }
            }
            Console.WriteLine();
        }
        File.SetLastWriteTimeUtc(dst, si.LastWriteTimeUtc);
    }
}
