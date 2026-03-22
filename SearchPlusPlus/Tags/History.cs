using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHistory(MusicInfo musicInfo)
        {
            return RefreshPatch.history.Contains(musicInfo.uid);
        }
        internal static bool EvalHistory(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalHistory(M.I);
        }
    }
}
