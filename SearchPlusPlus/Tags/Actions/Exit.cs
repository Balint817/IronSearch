using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Il2CppAssets.Scripts.Database;
using IronPython.Runtime.Operations;
using IronSearch.Patches;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalExit(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            if (varArgs[0] is not bool b)
            {
                throw new SearchInputException("invalid 'exit' argument");
            }
            throw new TerminateSearchException(b);
        }
    }
}
