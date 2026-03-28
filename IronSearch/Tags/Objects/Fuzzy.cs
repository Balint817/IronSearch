using System.Text.RegularExpressions;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalFuzzyArgCount = new(1, 2);
        internal static dynamic EvalFuzzy(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            bool caseSensitive = true;
            if (varKwargs.ContainsKey("case"))
            {
                if (varKwargs["case"] is bool b)
                {
                    caseSensitive = b;
                }
                else
                {
                    throw new SearchWrongTypeException("True or False for `case=`", varKwargs["case"]?.GetType(), "Regex()");
                }
                varKwargs.Remove("case");
            }

            ThrowIfNotEmpty(varKwargs);
            ThrowIfEmpty(varArgs);

            if (varArgs[^1] is bool b2)
            {
                caseSensitive = b2;
                varArgs = varArgs[..^1];
            }

            ThrowIfNotMatching(varArgs, evalFuzzyArgCount);

            if (varArgs.Length == 1)
            {
                if (varArgs[0] is string s)
                {
                    return new FuzzyContains(s, caseInsensitive: !caseSensitive);
                }
            }
            else
            {
                if (varArgs[0] is string s0 && varArgs[1] is string s1)
                {
                    return new FuzzyContains(s0, caseInsensitive: !caseSensitive).IsMatch(s1);
                }
            }
            throw new SearchValidationException("Fuzzy() expects a pattern string, or two strings (pattern, text) to test a match.", "Regex()");
        }
    }
}
