using HarmonyLib;
using MelonLoader;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace IronSearch.Patches
{
    internal class BmsLoader_MathPatch
    {
        private static readonly Dictionary<MethodInfo, MethodInfo> Replacements = new();

        internal static void RunPatch(HarmonyLib.Harmony harmonyInstance)
        {
            try
            {
                var mathfType = typeof(Mathf);
                Replacements[AccessTools.Method(mathfType, nameof(Mathf.FloorToInt), new[] { typeof(float) })] =
                    AccessTools.Method(typeof(BmsLoader_MathPatch), nameof(SafeFloorToInt));
                Replacements[AccessTools.Method(mathfType, nameof(Mathf.CeilToInt), new[] { typeof(float) })] =
                    AccessTools.Method(typeof(BmsLoader_MathPatch), nameof(SafeCeilToInt));
                Replacements[AccessTools.Method(mathfType, nameof(Mathf.RoundToInt), new[] { typeof(float) })] =
                    AccessTools.Method(typeof(BmsLoader_MathPatch), nameof(SafeRoundToInt));

                var type = AccessTools.TypeByName("CustomAlbums.BmsLoader");
                var loadMethod = AccessTools.Method(type, "Load");
                var transpiler = new HarmonyMethod(typeof(BmsLoader_MathPatch), nameof(Transpiler));
                harmonyInstance.Patch(loadMethod, transpiler: transpiler);
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.Red, ex);
                MelonLogger.Msg(ConsoleColor.Red, "Error occurred while patching BmsLoader, thread-safe math replacements will not be applied.");
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.operand is MethodInfo method && Replacements.TryGetValue(method, out var replacement))
                {
                    var newInstruction = new CodeInstruction(OpCodes.Call, replacement);
                    newInstruction.labels.AddRange(instruction.labels);
                    newInstruction.blocks.AddRange(instruction.blocks);
                    yield return newInstruction;
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static int SafeFloorToInt(float f) => (int)MathF.Floor(f);
        public static int SafeCeilToInt(float f) => (int)MathF.Ceiling(f);
        public static int SafeRoundToInt(float f) => (int)MathF.Round(f);
    }
}
