using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalOnline(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo)) return false;
            return EvalOnlineInternal(musicInfo);
        }

        private static bool EvalOnlineInternal(MusicInfo musicInfo)
        {
            return ((Album)ModMain.uidToAlbum[musicInfo.uid]).Sheets.Values.Any(x => ModMain._hqChartDict.ContainsKey(x.Md5));
        }

        internal static bool EvalOnline(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Online()");
            ThrowIfNotEmpty(varKwargs, "Online()");
            return EvalOnline(M.I);
        }
    }
}
