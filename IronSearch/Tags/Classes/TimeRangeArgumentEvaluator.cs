using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class TimeRangeArgumentEvaluator: Evaluator
        {
            public abstract IEnumerable<double> GetDoubles(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {
                ThrowIfNotEmpty(varKwargs, EvaluatorNameCalled);
                ThrowIfEmpty(varArgs, EvaluatorNameCalled);

                MultiRange mr = MultiRangeArgumentParser.GetMultiRange(varArgs[0], EvaluatorNameCalled, true);

                return GetDoubles(M.I).Any(value => mr.Contains(value));
            }
        }
    }
}
