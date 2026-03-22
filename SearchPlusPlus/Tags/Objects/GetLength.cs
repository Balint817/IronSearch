using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetLength(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotEmpty(varArgs);

            var l = AudioHelper.GetMusicLength(M.I);
            if (l is { } ts)
            {
                return ts.TotalSeconds;
            }
            return null;
        }
    }
}
