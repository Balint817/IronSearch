using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;
using System.Text.RegularExpressions;
using static IronPython.Modules._ast;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        // TODO: turn these into classes that implement an abstract class or interface (smth like IContainsEvaluator)
        static IEnumerable<string> GetStrings_Title(MusicInfo mi)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.name))
            {
                yield return item;
            }

            if (EvalCustom(musicInfo))
            {
                foreach (var item in GetStringsCustom_Title(mi))
                {
                    yield return item;
                }
            }
            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(mi.GetLocal(i).name))
                {
                    yield return item;
                }
            }
        }
        static IEnumerable<string> GetStringsCustom_Title(MusicInfo mi)
        {
            return RomanizationHelper.GetAllRomanizations(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == mi.uid).Info.NameRomanized);
        }
        internal static bool EvalTitle(PeroString pStr, MusicInfo musicInfo, string value)
        {
            return GetStrings_Title(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
        }
        internal static bool EvalTitle(MusicInfo musicInfo, Regex value)
        {
            return GetStrings_Title(musicInfo).Any(x => value.IsMatch(x));
        }
        internal static bool EvalTitle(MusicInfo musicInfo, FuzzyContains value)
        {
            return GetStrings_Title(musicInfo).Any(x => value.IsMatch(x));
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
