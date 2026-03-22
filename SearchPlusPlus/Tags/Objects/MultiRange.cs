using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Harmony;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);

            var multiRanges = new List<MultiRange>();

            for (int i = 0; i < varArgs.Length; i++)
            {
                var arg = varArgs[i];
                if (arg is string s)
                {
                    if (!Utils.ParseMultiRange(s, out var mr))
                    {
                        throw new SearchInputException($"failed to parse range '{s}'");
                    }
                    multiRanges.Add(mr);
                }
                else if (arg is Range r)
                {
                    multiRanges.Add(r.AsMultiRange());
                }
                else if (arg is MultiRange mr)
                {
                    multiRanges.Add(mr);
                }
                else
                {
                    throw new SearchInputException($"received invalid type in multi range");
                }
            }
            var result = new MultiRange();
            foreach (var item in multiRanges)
            {
                result.AddSelf(item);
            }

            return result;
        }
    }
}
