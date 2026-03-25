using System.Numerics;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRandomArgCount = new(0, 1);
        internal static dynamic EvalRandom(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, evalRandomArgCount);
            if (varArgs.Length == 0)
            {
                return Random.Shared.NextDouble();
            }
            switch (varArgs[0])
            {
                case int n1:
                    {
                        if (varArgs.Length != 2 || varArgs[1] is not int n2)
                        {
                            throw new ArgumentException("invalid random arguments");
                        }
                        if (n2 < n1)
                        {
                            (n1, n2) = (n2, n1);
                        }
                        return Random.Shared.Next(n1, n2);
                    }

                case long n1:
                    {
                        if (varArgs.Length != 2 || varArgs[1] is not long n2)
                        {
                            throw new ArgumentException("invalid random arguments");
                        }
                        if (n2 < n1)
                        {
                            (n1, n2) = (n2, n1);
                        }
                        return Random.Shared.NextInt64(n1, n2);
                    }
                case BigInteger n1:
                    {
                        if (varArgs.Length != 2 || varArgs[1] is not BigInteger n2 || n1 > long.MaxValue || n2 > long.MaxValue)
                        {
                            throw new ArgumentException("invalid random arguments");
                        }
                        if (n2 < n1)
                        {
                            (n1, n2) = (n2, n1);
                        }
                        return Random.Shared.NextInt64((long)n1, (long)n2);
                    }
                default:
                    break;
            }
            throw new SearchInputException("invalid random arguments");
        }
    }
}
