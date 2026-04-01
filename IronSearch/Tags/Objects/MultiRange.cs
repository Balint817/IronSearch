using Harmony;
using IronPython.Modules;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfEmpty(varArgs, "MultiRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "MultiRange", varArgs, varKwargs);

            var multiRanges = new List<MultiRange>();

            for (int i = 0; i < varArgs.Length; i++)
            {
                multiRanges.Add(MultiRangeArgumentParser.GetMultiRange(varArgs[i], "MultiRange", varArgs, varKwargs, true));
            }
            var result = new MultiRange();
            foreach (var item in multiRanges)
            {
                result.AddSelf(item);
            }

            return result;
        }
    }
}
