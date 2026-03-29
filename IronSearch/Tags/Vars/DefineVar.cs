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
using IronSearch.Exceptions;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalDefineVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "DefineVar()");
            ThrowIfNotEmpty(varKwargs, "DefineVar()");

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "DefineVar()");
            }
            LocalVariables.TryAdd(M.I.uid, new());

            var d = LocalVariables[M.I.uid];

            if (d.TryGetValue(s, out var v))
            {
                return true;
            }

            d[M.I.uid][s] = varArgs[1];

            return true;
        }
    }
}
