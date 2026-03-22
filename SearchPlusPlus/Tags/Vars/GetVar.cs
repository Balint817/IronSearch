using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppPeroPeroGames.GlobalDefines;
using IronPython.Runtime;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchInputException("expected string as variable name");
            }
            LocalVariables.TryAdd(M.I.uid, new());

            var d = LocalVariables[M.I.uid];

            if (d.TryGetValue(s, out var v))
            {
                return v;
            }
            throw new SearchInputException($"attempted to access undefined local variable '{s}'");
        }
    }
}
