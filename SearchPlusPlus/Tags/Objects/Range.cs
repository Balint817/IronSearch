using IronPython.Runtime;
using IronSearch.Records;
using System.Numerics;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRangeArgCount = new Range(1, 2);
        internal static dynamic EvalRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, evalRangeArgCount);
            bool? exclusiveStart = null;
            bool? exclusiveEnd = null;
            if (varKwargs.ContainsKey("end"))
            {
                if (varKwargs["end"] is not bool b)
                {
                    throw new SearchInputException("invalid 'end' argument in range");
                }

                exclusiveEnd = true;
                varKwargs.Remove("end");
            }

            if (varKwargs.ContainsKey("start"))
            {
                if (varKwargs["start"] is not bool b)
                {
                    throw new SearchInputException("invalid 'start' argument in range");
                }
                exclusiveStart = true;
                varKwargs.Remove("start");
            }
            ThrowIfNotEmpty(varKwargs);


            var arg0 = varArgs[0];

            if (varArgs.Length == 1)
            {
                switch (arg0)
                {
                    case string s:
                        if (!Utils.ParseRange(s, out var range))
                        {
                            throw new SearchInputException($"failed to parse range '{s}'");
                        }
                        if (exclusiveEnd.HasValue)
                        {
                            range.ExclusiveEnd = exclusiveEnd.Value;
                        }
                        if (exclusiveStart.HasValue)
                        {
                            range.ExclusiveStart = exclusiveStart.Value;
                        }

                        return range;
                    case PythonRange pr:
                        return (Range)pr;
                    case Range r:
                        return r;
                    case int i:
                        return new Range(i, i);
                    case BigInteger i:
                        return new Range((double)i, (double)i);
                    case double i:
                        return new Range(i, i);
                    default:
                        break;
                }
            }
            else
            {
                double start;
                double end;
                switch (arg0)
                {
                    case int i:
                        start = i;
                        break;
                    case BigInteger i:
                        start = (double)i;
                        break;
                    case double i:
                        start = i;
                        break;
                    default:
                        throw new SearchInputException($"unsupported type for range start: {arg0.GetType()}");
                }
                var arg1 = varArgs[1];
                switch (arg1)
                {
                    case int i:
                        end = i;
                        break;
                    case BigInteger i:
                        end = (double)i;
                        break;
                    case double i:
                        end = i;
                        break;
                    default:
                        throw new SearchInputException($"unsupported type for range start: {arg0.GetType()}");
                }
                return new Range(start, end);
            }
            return false;
        }
    }
}
