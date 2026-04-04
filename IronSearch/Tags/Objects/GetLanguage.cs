using IronSearch.Core;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetLanguageIndex(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetLanguage", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetLanguage", varArgs, varKwargs);
            return ActiveSearch.langIndex;
        }
    }
}
