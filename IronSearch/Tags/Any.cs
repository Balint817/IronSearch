using Il2CppAssets.Scripts.Database;
using System.Collections.ObjectModel;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class AnyEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Any";

            public ReadOnlyCollection<ContainsEvaluator> EvaluatorInstances = new(new ContainsEvaluator[]
            {
                ManagedSingleton<TagEvaluator>.Instance,
                ManagedSingleton<TitleEvaluator>.Instance,
                ManagedSingleton<AuthorEvaluator>.Instance,
                ManagedSingleton<DesignerEvaluator>.Instance,
                ManagedSingleton<AlbumEvaluator>.Instance
            });
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                foreach (var evaluator in EvaluatorInstances)
                {
                    foreach (var item in evaluator.GetStrings(musicInfo))
                    {
                        yield return item;
                    }
                }
            }
        }
        internal static bool EvalAny(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<AnyEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
