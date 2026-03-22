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
        static readonly Range evalRegexArgCount = new(1, 2);
        internal static dynamic EvalRegex(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var flags = RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase;
            ThrowIfNotInRange(varArgs, evalRegexArgCount);

            if (varKwargs.ContainsKey("case"))
            {
                if (varKwargs["case"] is bool b)
                {
                    if (b)
                    {
                        //default behavior
                        //flags &= ~RegexOptions.IgnoreCase;
                    }
                    else
                    {
                        flags |= RegexOptions.IgnoreCase;
                    }
                }
                else
                {
                    throw new SearchInputException("invalid 'case' argument");
                }
                varKwargs.Remove("case");
            }

            ThrowIfNotEmpty(varKwargs);

            if (varArgs.Length == 1)
            {
                if (varArgs[0] is string s)
                {
                    return new Regex(s, flags);
                }
            }
            else
            {
                if (varArgs[0] is string s0 && varArgs[1] is string s1)
                {
                    return Regex.IsMatch(s0, s1, flags);
                }
            }
            throw new SearchInputException("invalid regex arguments");
        }
    }
}
