using System.Reflection;

namespace DependentConsoleApp;

public static class InspectMusicData
{
    public static void Run()
    {
        var il2cppDir = @"D:\Steam\steamapps\common\Muse Dash\MelonLoader\Il2CppAssemblies";
        var net6Dir = @"D:\Steam\steamapps\common\Muse Dash\MelonLoader\net6";
        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

        var allDlls = new List<string>();
        allDlls.AddRange(Directory.GetFiles(il2cppDir, "*.dll"));
        allDlls.AddRange(Directory.GetFiles(net6Dir, "*.dll"));
        allDlls.AddRange(Directory.GetFiles(runtimeDir, "*.dll"));

        var resolver = new PathAssemblyResolver(allDlls);
        using var ctx = new MetadataLoadContext(resolver);
        var asm = ctx.LoadFromAssemblyPath(Path.Combine(il2cppDir, "Assembly-CSharp.dll"));

        // Find MusicData
        foreach (var t in asm.GetTypes())
        {
            if (t.Name == "MusicData")
            {
                Console.WriteLine($"=== {t.FullName} (Base: {t.BaseType?.FullName}) ===");
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.Name.StartsWith("Native")) continue;
                    Console.WriteLine($"  P: {p.PropertyType.Name,-20} {p.Name}");
                }
                Console.WriteLine();
            }
        }

        // Also show StageInfo properties
        var stageInfo = asm.GetType("Il2CppAssets.Scripts.GameCore.StageInfo");
        if (stageInfo != null)
        {
            Console.WriteLine($"=== {stageInfo.FullName} (Base: {stageInfo.BaseType?.FullName}) ===");
            foreach (var p in stageInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.Name.StartsWith("Native")) continue;
                Console.WriteLine($"  P: {p.PropertyType.Name,-20} {p.Name}");
            }
        }

        // Also check DBStageInfo  
        foreach (var t in asm.GetTypes())
        {
            if (t.Name == "DBStageInfo" || t.Name == "MusicConfigData")
            {
                Console.WriteLine($"\n=== {t.FullName} ===");
                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (m.Name.StartsWith("Native") || m.Name.StartsWith("get_") || m.Name.StartsWith("set_")) continue;
                    var ps = string.Join(", ", m.GetParameters().Select(p2 => $"{p2.ParameterType.Name} {p2.Name}"));
                    Console.WriteLine($"  M: {m.ReturnType.Name,-20} {m.Name}({ps})");
                }
                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.Name.StartsWith("Native")) continue;
                    Console.WriteLine($"  P: {p.PropertyType.Name,-20} {p.Name}");
                }
            }
        }
    }
}
