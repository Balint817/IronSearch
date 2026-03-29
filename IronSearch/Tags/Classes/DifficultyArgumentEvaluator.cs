using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range= IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class DifficultyArgumentEvaluator : Evaluator
        {
            static readonly Range argRange = new(1, 2); // just for the error message, realistically not needed;
            // varArg[0] should filter by value, varArg[1] should filter by key.
            public abstract IEnumerable<KeyValuePair<double, double>> GetDoubles(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {
                ThrowIfNotEmpty(varKwargs, EvaluatorNameCalled);
                ThrowIfEmpty(varArgs, EvaluatorNameCalled);


                MultiRange mr0 = MultiRangeArgumentParser.GetMultiRange(varArgs[0], EvaluatorNameCalled);

                if (varArgs.Length == 1)
                {
                    return GetDoubles(M.I).Any(kv => mr0.Contains(kv.Value));
                }
                MultiRange mr1 = MultiRangeArgumentParser.GetMultiRange(varArgs[1], EvaluatorNameCalled);

                varArgs = varArgs[2..];

                ThrowIfNotMatching(varArgs, argRange, EvaluatorNameCalled);

                if (mr1 == MultiRange.InvalidRange)
                {
                    var arr = GetDoubles(M.I).ToArray();
                    if (arr.Length == 0)
                    {
                        return false;
                    }
                    if (mr0 == MultiRange.InvalidRange)
                    {
                        return double.IsNaN(arr.MaxBy(x => x.Key).Value);
                    }
                    return mr0.Contains(arr.MaxBy(x => x.Key).Value);
                }

                if (mr0 == MultiRange.InvalidRange)
                {
                    return GetDoubles(M.I).Where(kv => mr1.Contains(kv.Key)).Any(kv => double.IsNaN(kv.Value));
                }
                return GetDoubles(M.I).Where(kv => mr1.Contains(kv.Key)).Any(kv => mr0.Contains(kv.Value));
            }
        }
    }
}
