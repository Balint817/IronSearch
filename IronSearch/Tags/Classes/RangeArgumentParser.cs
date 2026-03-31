using System.Numerics;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range=IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal static class RangeArgumentParser
        {
            public static readonly BigInteger MaxDouble = (BigInteger)double.MaxValue;
            public static Range GetRange(dynamic arg, string parameterContext, bool allowTime = false)
            {
                switch (arg)
                {
                    case int n:
                        return new Range(n, n);
                    case long n:
                        return new Range(n, n);
                    case double n:
                        return new Range(n, n);
                    case BigInteger n:
                        if (n > MaxDouble)
                        {
                            throw new SearchValidationException($"The value {n} is too large to be used as a range argument.", parameterContext);
                        }
                        return new Range((double)n, (double)n);
                    case string s:
                        if (!Utils.ParseRange(s, out var range))
                        {
                            if (allowTime && Utils.TryTimeStringRangeToTimeRange(s, out var time))
                            {
                                return time;
                            }
                            throw SearchParseException.ForRange(s, parameterContext, null);
                        }
                        return range;
                    case Range r:
                        return r.Copy();
                    case PythonRange pr:
                        return (Range)pr;
                    case MultiRange mr:
                        if (mr.Ranges.Count == 1)
                        {
                            return mr.Ranges[0].Copy();
                        }
                        throw new SearchValidationException($"The multi-range '{mr}' cannot be interpreted as a range.", parameterContext);
                    default:
                        throw new SearchWrongTypeException("an integer, a string range, or range", arg?.GetType(), parameterContext);
                }
            }
        }
    }
}
