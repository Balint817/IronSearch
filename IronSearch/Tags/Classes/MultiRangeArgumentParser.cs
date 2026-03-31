using System.Numerics;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range=IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal static class MultiRangeArgumentParser
        {
            public static readonly BigInteger MaxDouble = (BigInteger)double.MaxValue;
            public static MultiRange GetMultiRange(dynamic arg, string parameterContext, bool allowTime = false)
            {
                switch (arg)
                {
                    case int n:
                        return new MultiRange(new Range(n, n));
                    case long n:
                        return new MultiRange(new Range(n, n));
                    case double n:
                        return new MultiRange(new Range(n, n));
                    case BigInteger n:
                        if (n > MaxDouble)
                        {
                            throw new SearchValidationException($"The value {n} is too large to be used as a range argument.", parameterContext);
                        }
                        return new MultiRange(new Range((double)n, (double)n));
                    case string s:
                        if (!Utils.ParseMultiRange(s, out var range))
                        {
                            if (allowTime && Utils.TryTimeStringRangeToTimeRange(s, out var time))
                            {
                                return time.AsMultiRange();
                            }
                            throw SearchParseException.ForRange(s, parameterContext, null);
                        }
                        return range;
                    case Range r:
                        return r.AsMultiRange();
                    case PythonRange pr:
                        return ((Range)pr).AsMultiRange();
                    case MultiRange mr:
                        return mr.Add();
                    default:
                        throw new SearchWrongTypeException("an integer, a string range, range, or multi-range", arg?.GetType(), parameterContext);
                }
            }
        }
    }
}
