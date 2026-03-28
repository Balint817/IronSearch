using System.Text.RegularExpressions;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalTitle(PeroString pStr, MusicInfo musicInfo, string value)
        {

            if (pStr.LowerContains(musicInfo.name ?? "", value))
            {
                return true;
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(pStr, musicInfo, value))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (pStr.LowerContains(musicInfo.GetLocal(i).name ?? "", value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalTitleCustom(PeroString pStr, MusicInfo musicInfo, string value)
        {
            return pStr.LowerContains(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized ?? "", value);
        }





        internal static bool EvalTitle(MusicInfo musicInfo, Regex re)
        {

            if (re.IsMatch(musicInfo.name ?? ""))
            {
                return true;
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(re, musicInfo))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (re.IsMatch(musicInfo.GetLocal(i).name ?? ""))
                {
                    return true;
                }
            }

            return false;
        }
        internal static bool EvalTitleCustom(Regex re, MusicInfo musicInfo)
        {
            return re.IsMatch(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized ?? "");
        }





        internal static bool EvalTitle(MusicInfo musicInfo, FuzzyContains fc)
        {

            if (fc.IsMatch(musicInfo.name ?? ""))
            {
                return true;
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(fc, musicInfo))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (fc.IsMatch(musicInfo.GetLocal(i).name ?? ""))
                {
                    return true;
                }
            }

            return false;
        }
        internal static bool EvalTitleCustom(FuzzyContains fc, MusicInfo musicInfo)
        {
            return fc.IsMatch(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized ?? "");
        }


        internal static bool EvalTitle(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case Regex re:
                    return EvalTitle(M.I, re);
                case FuzzyContains fc:
                    return EvalTitle(M.I, fc);
                case string s:
                    return EvalTitle(M.PS, M.I, s);
            }
            throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), "Title()");
        }
    }
}
