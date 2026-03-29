using IronSearch.Exceptions;
using System.Numerics;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRangeArgCount = new Range(1, 2);
        internal static dynamic EvalRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, evalRangeArgCount, "Range()");
            bool? exclusiveStart = null;
            bool? exclusiveEnd = null;
            if (varKwargs.ContainsKey("end"))
            {
                if (varKwargs["end"] is not bool b)
                {
                    throw new SearchWrongTypeException("True or False for `end=` (exclusive end)", varKwargs["end"]?.GetType(), "Range()");
                }

                exclusiveEnd = true;
                varKwargs.Remove("end");
            }

            if (varKwargs.ContainsKey("start"))
            {
                if (varKwargs["start"] is not bool b)
                {
                    throw new SearchWrongTypeException("True or False for `start=` (exclusive start)", varKwargs["start"]?.GetType(), "Range()");
                }
                exclusiveStart = true;
                varKwargs.Remove("start");
            }
            ThrowIfNotEmpty(varKwargs, "Range()");


            var arg0 = varArgs[0];

            if (varArgs.Length == 1)
            {
                if (exclusiveEnd.HasValue || exclusiveStart.HasValue)
                {
                    throw new SearchValidationException("'end=' and 'start=' keyword arguments are not valid when there is only 1 positional argument.", "Range()");
                }
                return RangeArgumentParser.GetRange(arg0, "Range()", allowTime: true);
            }

            double start;
            double end;
            switch (arg0)
            {
                case int i:
                    start = i;
                    break;
                case BigInteger i:
                    if (i > RangeArgumentParser.MaxDouble)
                    {
                        throw new SearchValidationException($"The value {i} is too large to be used as a range argument.", "Range()");
                    }
                    start = (double)i;
                    break;
                case double i:
                    start = i;
                    break;
                default:
                    throw new SearchWrongTypeException("a number for the range start", arg0?.GetType(), "Range()");
            }
            var arg1 = varArgs[1];
            switch (arg1)
            {
                case int i:
                    end = i;
                    break;
                case BigInteger i:
                    if (i > RangeArgumentParser.MaxDouble)
                    {
                        throw new SearchValidationException($"The value {i} is too large to be used as a range argument.", "Range()");
                    }
                    end = (double)i;
                    break;
                case double i:
                    end = i;
                    break;
                default:
                    throw new SearchWrongTypeException("a number for the range end", arg1?.GetType(), "Range()");
            }
            if (end < start)
            {
                (start, end) = (end, start);
            }
            return new Range(start, end)
            {
                ExclusiveStart = exclusiveStart ?? false,
                ExclusiveEnd = exclusiveEnd ?? false
            };
        }
    }
}
