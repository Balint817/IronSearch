using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class AuthorEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Author";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                var result = new List<string>();
                result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.author));

                for (int i = 1; i <= 5; i++)
                {
                    result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author));
                }
                return result;
            }
        }
        internal static bool EvalAuthor(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<AuthorEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
