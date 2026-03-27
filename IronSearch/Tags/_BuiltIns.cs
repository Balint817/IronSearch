using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using PythonExpressionManager;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{

    internal delegate dynamic ExpressionDelegate(SearchArgument input, PythonTuple varArgs, PythonDictionary varKwargs);
    internal delegate bool BuiltInDelegate(SearchArgument input, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs);
    internal delegate dynamic BuiltInObjectDelegate(SearchArgument input, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs);
    internal partial class BuiltIns
    {
        internal static void ThrowIfNotEmpty(IReadOnlyDictionary<string, dynamic> d)
        {
            if (d.Count != 0)
            {
                throw SearchArgumentException.UnexpectedKeywords(d.Keys);
            }
        }
        internal static void ThrowIfNotEmpty(IList<dynamic> d)
        {
            if (d.Count != 0)
            {
                throw SearchArgumentException.UnexpectedPositionalArguments();
            }
        }
        internal static void ThrowIfEmpty(IList<dynamic> d)
        {
            if (d.Count == 0)
            {
                throw SearchArgumentException.ExpectedAtLeastOnePositional();
            }
        }
        internal static void ThrowIfNotMatching(IList<dynamic> d, Range r)
        {
            if (!r.Contains(d.Count))
            {
                throw SearchArgumentException.ArgumentCountNotInRange(r, d.Count);
            }
        }
        internal static void ThrowIfNotMatching(IList<dynamic> d, int n)
        {
            if (d.Count != n)
            {
                throw SearchArgumentException.ArgumentCountMismatch(n, d.Count);
            }
        }

        internal static WrappedCLRDelegate WrapCommonChecks(BuiltInDelegate baseDel)
        {
            WrappableCLRDelegate castingDel = (input, tagDict, varArgs, varKwargs) =>
            {
                return baseDel((SearchArgument)input, varArgs, varKwargs);
            };
            WrappedCLRDelegate wrappedDel = ScriptExecutor.FromUnwrapped(castingDel);
            WrappedCLRDelegate del2 = (input, tagDict, args, kwargs) =>
            {
                if (input is not SearchArgument SA)
                {
                    throw new SearchValidationException("Invalid search context.");
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
                        throw new SearchValidationException("Invalid search context.");
                }

                return baseDel(input, args, kwargs);
            };
            return del2;
        }
        internal static WrappedCLRDelegate WrapCommonChecks(BuiltInObjectDelegate baseDel)
        {
            WrappableCLRDelegate castingDel = (input, tagDict, varArgs, varKwargs) =>
            {
                return baseDel((SearchArgument)input, varArgs, varKwargs);
            };
            WrappedCLRDelegate wrappedDel = ScriptExecutor.FromUnwrapped(castingDel);
            WrappedCLRDelegate del2 = (input, tagDict, args, kwargs) =>
            {
                if (input is not SearchArgument SA)
                {
                    throw new SearchValidationException("Invalid search context.");
                }

                return wrappedDel(input, tagDict, args, kwargs);
            };
            return del2;
        }
    }
}
