using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalOld(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw SearchParseException.ForRange(s, "Old()", "a rank/index range such as 0-10");
            }
            return EvalOld(musicInfo, r);
        }
        internal static bool EvalOld(MusicInfo musicInfo, int n)
        {
            return EvalOld(musicInfo, new Range(0, n));
        }
        internal static bool EvalOld(MusicInfo musicInfo, Range r)
        {
            return EvalOld(musicInfo, r.AsMultiRange());
        }
        internal static bool EvalOld(MusicInfo musicInfo, MultiRange mr)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalOldInternal(musicInfo, mr);
        }
        internal static bool EvalOldInternal(MusicInfo musicInfo, MultiRange mr)
        {
            InitNewIfNeeded();
            var idx = sortedByLastModified!.IndexOf(musicInfo.uid);
            if (idx == -1)
            {
                return false;
            }
            idx = sortedByLastModified.Count - 1 - idx;
            return mr.Contains(idx);
        }
        internal static bool EvalOld(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalOld(M.I, n);
                case string s:
                    return EvalOld(M.I, s);
                case Range r:
                    return EvalOld(M.I, r);
                case PythonRange pr:
                    return EvalOld(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalOld(M.I, mr);
                default:
                    break;
            }
            throw new SearchWrongTypeException("an integer, a range string, or a range object", varArgs[0]?.GetType(), "Old()");
        }
    }
}
