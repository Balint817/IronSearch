using IronPython.Runtime;
using IronSearch.Exceptions;
using PythonExpressionManager;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{

    internal delegate dynamic ExpressionDelegate(SearchArgument input, PythonTuple varArgs, PythonDictionary varKwargs);
    internal delegate bool BuiltInDelegate(SearchArgument input, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs);
    public delegate dynamic BuiltInObjectDelegate(SearchArgument input, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs);
    internal partial class BuiltIns
    {
        internal static void ThrowIfNotEmpty(IReadOnlyDictionary<string, dynamic> d, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (d.Count != 0)
            {
                throw SearchArgumentException.UnexpectedKeywords(d.Keys, parameterContext, varArgs, varKwargs);
            }
        }
        internal static void ThrowIfNotEmpty(IList<dynamic> d, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (d.Count != 0)
            {
                throw SearchArgumentException.UnexpectedPositionalArguments(parameterContext, varArgs, varKwargs);
            }
        }
        internal static void ThrowIfEmpty(IList<dynamic> d, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (d.Count == 0)
            {
                throw SearchArgumentException.ExpectedAtLeastOnePositional(parameterContext, varArgs, varKwargs);
            }
        }
        internal static void ThrowIfLess(IList<dynamic> d, int n, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (d.Count < n)
            {
                throw SearchArgumentException.ExpectedAtLeastNPositional(n, parameterContext, varArgs, varKwargs);
            }
        }
        internal static void ThrowIfNotMatching(IList<dynamic> d, Range r, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (!r.Contains(d.Count))
            {
                throw SearchArgumentException.ArgumentCountNotInRange(r, d.Count, parameterContext, varArgs, varKwargs);
            }
        }
        internal static void ThrowIfNotMatching(IList<dynamic> d, int n, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            if (d.Count != n)
            {
                throw SearchArgumentException.ArgumentCountMismatch(n, d.Count, parameterContext, varArgs, varKwargs);
            }
        }

        private static dynamic[] ConvertArgs(PythonTuple args) => args!.ToArray<dynamic>();
        private static Dictionary<string, dynamic> ConvertKwargs(PythonDictionary kwargs) => kwargs.ToDictionary(x => (string)x.Key, x => (dynamic)x.Value);

        internal static WrappedCLRDelegate WrapCommonChecks(UserScriptManager scriptManager, BuiltInDelegate baseDel)
        {
            WrappableCLRDelegate castingDel = (input, tagDict, varArgs, varKwargs) =>
            {
                return baseDel((SearchArgument)input, varArgs, varKwargs);
            };
            WrappedCLRDelegate wrappedDel = scriptManager.ScriptExecutor.FromUnwrapped(castingDel);
            WrappedCLRDelegate del2 = (input, tagDict, args, kwargs) =>
            {
                if (input is not SearchArgument SA)
                {
                    throw new SearchValidationException("Invalid search context.", "<unknown>", ConvertArgs(args), ConvertKwargs(kwargs));
                }

                return wrappedDel(input, tagDict, args, kwargs);
            };
            return del2;
        }
        internal static WrappedCLRDelegate WrapCommonChecks(ExpressionDelegate baseDel)
        {

            WrappedCLRDelegate del2 = (input, tagDict, args, kwargs) =>
            {
                switch (input)
                {
                    case ExpressionSearchArgument ESA:
                        break;
                    case SearchArgument SA:
                        input = new ExpressionSearchArgument(SA, new(), new());
                        break;
                    default:
                        throw new SearchValidationException("Invalid search context.", "<unknown>", ConvertArgs(args), ConvertKwargs(kwargs));
                }

                return baseDel(input, args, kwargs);
            };
            return del2;
        }
        internal static WrappedCLRDelegate WrapCommonChecks(UserScriptManager scriptManager, BuiltInObjectDelegate baseDel)
        {
            WrappableCLRDelegate castingDel = (input, tagDict, varArgs, varKwargs) =>
            {
                return baseDel((SearchArgument)input, varArgs, varKwargs);
            };
            WrappedCLRDelegate wrappedDel = scriptManager.ScriptExecutor.FromUnwrapped(castingDel);
            WrappedCLRDelegate del2 = (input, tagDict, args, kwargs) =>
            {
                if (input is not SearchArgument SA)
                {
                    throw new SearchValidationException("Invalid search context.", "<unknown>", ConvertArgs(args), ConvertKwargs(kwargs));
                }

                return wrappedDel(input, tagDict, args, kwargs);
            };
            return del2;
        }
    }
}
