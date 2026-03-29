using System;
using System.Collections.Concurrent;
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
        internal static readonly ConcurrentDictionary<string, dynamic> GlobalVariables = new();
        internal static dynamic EvalSetGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "SetGlobal()");
            ThrowIfNotEmpty(varKwargs, "SetGlobal()");

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "SetGlobal()");
            }

            GlobalVariables[s] = varArgs[1];
            return true;
        }
    }
}
