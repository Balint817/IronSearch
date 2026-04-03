using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;
using IronSearch.Utils;
using System.Text.RegularExpressions;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class ContainsEvaluator : Evaluator
        {
            public abstract IEnumerable<string> GetStrings(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {

                ThrowIfNotEmpty(varKwargs, EvaluatorName, varArgs, varKwargs);
                ThrowIfNotMatching(varArgs, 1, EvaluatorName, varArgs, varKwargs);
                switch (varArgs[0])
                {
                    case string s:
                        return Evaluate(M.I, M.PS, s);
                    case Regex r:
                        return Evaluate(M.I, r);
                    case FuzzyContains fc:
                        return Evaluate(M.I, fc);
                }
                throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), EvaluatorName, varArgs, varKwargs);
            }
            public bool Evaluate(MusicInfo musicInfo, PeroString pStr, string value)
            {
                return GetStrings(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
            }
            public bool Evaluate(MusicInfo musicInfo, Regex value)
            {
                return GetStrings(musicInfo).Any(x => value.IsMatch(x));
            }
            public bool Evaluate(MusicInfo musicInfo, FuzzyContains value)
            {
                return GetStrings(musicInfo).Any(x => value.IsMatch(x));
            }
        }
    }
}
