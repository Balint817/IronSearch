using System.Text.RegularExpressions;
using IronSearch.Exceptions;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRegexArgCount = new(1, 2);
        internal static dynamic EvalRegex(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var flags = RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase;
            ThrowIfNotMatching(varArgs, evalRegexArgCount, "Regex()");

            if (varKwargs.ContainsKey("case"))
            {
                if (varKwargs["case"] is bool b)
                {
                    if (b)
                    {
                        //default behavior
                        flags &= ~RegexOptions.IgnoreCase;
                    }
                    else
                    {
                        //flags |= RegexOptions.IgnoreCase;
                    }
                }
                else
                {
                    throw new SearchWrongTypeException("True or False for `case=`", varKwargs["case"]?.GetType(), "Regex()");
                }
                varKwargs.Remove("case");
            }

            ThrowIfNotEmpty(varKwargs, "Regex()");

            if (varArgs.Length == 1)
            {
                if (varArgs[0] is string s)
                {
                    return new Regex(s, flags);
                }
            }
            else
            {
                if (varArgs[0] is string s0 && varArgs[1] is string s1)
                {
                    return Regex.IsMatch(s1, s0, flags);
                }
            }
            throw new SearchValidationException("Regex() expects a pattern string, or two strings (pattern, text) to test a match.", "Regex()");
        }
    }
}
