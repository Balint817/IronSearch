using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalRangeArgCount = new Range(1, 2);
        internal static dynamic EvalRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotInRange(varArgs, evalRangeArgCount);
            ThrowIfNotEmpty(varKwargs);
            var arg0 = varArgs[0];
            switch (arg0)
            {
                case string s:
                    if (!Utils.ParseRange(s, out var range))
                    {
                        throw new SearchInputException($"failed to parse range '{s}'");
                    }
                    return range;
                case Range r:
                    return r;
                default:
                    break;
            }
            return false;
        }
    }
}
