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
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.name))
            {
                if (pStr.LowerContains(item, value))
                {
                    return true;
                }
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(pStr, musicInfo, value))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).name))
                {
                    if (pStr.LowerContains(item, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool EvalTitleCustom(PeroString pStr, MusicInfo musicInfo, string value)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized))
            {
                if (pStr.LowerContains(item, value))
                {
                    return true;
                }
            }
            return false;
        }





        internal static bool EvalTitle(MusicInfo musicInfo, Regex re)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.name))
            {
                if (re.IsMatch(item))
                {
                    return true;
                }
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(re, musicInfo))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).name ?? ""))
                {
                    if (re.IsMatch(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        internal static bool EvalTitleCustom(Regex re, MusicInfo musicInfo)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized ?? ""))
            {
                if (re.IsMatch(item))
                {
                    return true;
                }
            }
            return false;
        }





        internal static bool EvalTitle(MusicInfo musicInfo, FuzzyContains fc)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.name))
            {
                if (fc.IsMatch(item))
                {
                    return true;
                }
            }

            if (EvalCustom(musicInfo) && EvalTitleCustom(fc, musicInfo))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).name ?? ""))
                {
                    if (fc.IsMatch(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        internal static bool EvalTitleCustom(FuzzyContains fc, MusicInfo musicInfo)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Info.NameRomanized ?? ""))
            {
                if (fc.IsMatch(item))
                {
                    return true;
                }
            }
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
