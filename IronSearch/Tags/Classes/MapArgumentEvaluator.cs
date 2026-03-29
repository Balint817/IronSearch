using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range= IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class MapArgumentEvaluator : Evaluator
        {
            static readonly Range argRange = new(0,1); // just for the error message, realistically not needed;
            public abstract IEnumerable<KeyValuePair<double, bool>> GetDoubles(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {
                ThrowIfNotEmpty(varKwargs, EvaluatorNameCalled);

                if (varArgs.Length == 0)
                {
                    return GetDoubles(M.I).Any();
                }

                ThrowIfNotMatching(varArgs, argRange, EvaluatorNameCalled);

                MultiRange mr0 = MultiRangeArgumentParser.GetMultiRange(varArgs[0], EvaluatorNameCalled);
                if (mr0 == MultiRange.InvalidRange)
                {
                    var arr = GetDoubles(M.I).ToArray();
                    if (arr.Length == 0)
                    {
                        return false;
                    }
                    return arr.MaxBy(x => x.Key).Value;
                }
                return GetDoubles(M.I).Any(x => x.Value && mr0.Contains(x.Key));
            }
        }
    }
}
