using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class DesignerEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Designer";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.levelDesigner))
                {
                    yield return item;
                }

                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var i in availableMaps)
                {
                    foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLevelDesignerStringByIndex(i)))
                    {
                        yield return item;
                    }
                }
            }
        }
        internal static bool EvalDesigner(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<DesignerEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
