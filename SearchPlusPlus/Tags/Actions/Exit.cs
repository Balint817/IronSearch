using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Il2CppAssets.Scripts.Database;
using IronPython.Runtime.Operations;
using IronSearch.Exceptions;
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
                throw new SearchWrongTypeException("True or False for whether to exit the search", varArgs[0]?.GetType(), "Exit()");
            }
            throw new TerminateSearchException(b);
        }
    }
}
