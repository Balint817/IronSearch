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
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.author))
                {
                    yield return item;
                }

                for (int i = 1; i <= 5; i++)
                {
                    foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author))
                    {
                        yield return item;
                    }
                }
            }
        }
        internal static bool EvalAuthor(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<AuthorEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
