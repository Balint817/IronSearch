using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class FCEvaluator : MapArgumentEvaluator
        {
            public override string EvaluatorName => "FC";
            public override IEnumerable<KeyValuePair<double, bool>> GetPairs(MusicInfo musicInfo)
            {
                MapUtils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;

                    yield return new(diff, RefreshPatch.fullCombos.Contains(s));
                }
            }
        }

        internal static bool EvalFC(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<FCEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
