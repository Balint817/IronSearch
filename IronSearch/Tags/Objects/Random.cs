using IronSearch.Exceptions;
using System.Numerics;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRandomArgCount = new(0, 2);
        internal static dynamic EvalRandom(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "Random()");
            ThrowIfNotMatching(varArgs, 2, "Random()");
            if (varArgs.Length == 0)
            {
                return Random.Shared.NextDouble();
            }

            if (varArgs.Length == 1)
            {
                Range r = RangeArgumentParser.GetRange(varArgs[0], "Random()", allowTime: true);
                if (r.Start == r.End)
                {
                    throw new SearchValidationException("min and max cannot be equal.", "Random()");
                }
                if (r.Start == double.NegativeInfinity || r.End == double.PositiveInfinity)
                {
                    throw new SearchValidationException("min and max cannot be infinity.", "Random()");
                }

                return UniformDouble.NextDouble(r.Start, r.End);
            }

            long start;

            switch (varArgs[0])
            {
                case int n1:
                    start = n1;
                    break;
                case long n1:
                    start = n1;
                    break;
                case BigInteger n1:
                    if (n1 > RangeArgumentParser.MaxDouble)
                    {
                        throw new SearchValidationException($"The value {n1} is too large to be used as a start value.", "Random()");
                    }
                    start = (long)n1;
                    break;
                default:
                    throw new SearchWrongTypeException($"expected 2 integers", varArgs[0]?.GetType(), "Random()");
            }

            long end;
            switch (varArgs[0])
            {
                case int n1:
                    end = n1;
                    break;
                case long n1:
                    end = n1;
                    break;
                case BigInteger n1:
                    if (n1 > RangeArgumentParser.MaxDouble)
                    {
                        throw new SearchValidationException($"The value {n1} is too large to be used as a start value.", "Random()");
                    }
                    end = (long)n1;
                    break;
                default:
                    throw new SearchWrongTypeException($"expected 2 integers", varArgs[0]?.GetType(), "Random()");
            }

            return Random.Shared.NextInt64(start, end);
        }
    }
}
