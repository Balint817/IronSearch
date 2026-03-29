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

            ThrowIfNotEmpty(varKwargs, "Fuzzy()");
            ThrowIfEmpty(varArgs, "Fuzzy()");

            if (varArgs[^1] is bool b2)
            {
                caseSensitive = b2;
                varArgs = varArgs[..^1];
            }

            ThrowIfNotMatching(varArgs, evalFuzzyArgCount, "Fuzzy()");

            if (varArgs[0] is string s0)
            {
                if (string.IsNullOrEmpty(s0))
                {
                    throw new SearchValidationException("pattern text was empty!", "Fuzzy()");
                }
                if (s0.Length > 63)
                {
                    throw new SearchValidationException("pattern text is too long to support fuzzy matching!", "Fuzzy()");
                }
                if (varArgs.Length == 1)
                {
                    return new FuzzyContains(s0, caseInsensitive: !caseSensitive);
                }

                if (varArgs[1] is string s1)
                {
                    return new FuzzyContains(s0, caseInsensitive: !caseSensitive).IsMatch(s1);
                }

            }

            throw new SearchValidationException("Fuzzy() expects a pattern string, or two strings (pattern, text) to test a match.", "Regex()");
        }
    }
}
