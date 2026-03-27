using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static List<string>? sortedByLastModified;
        internal static bool EvalNew(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw SearchParseException.ForRange(s, "New()", "a rank/index range such as 0-10");
            }
            return EvalNew(musicInfo, r);
        }
        internal static bool EvalNew(MusicInfo musicInfo, int n)
        {
            return EvalNew(musicInfo, new Range(0, n));
        }
        internal static bool EvalNew(MusicInfo musicInfo, Range r)
        {
            return EvalNew(musicInfo, r.AsMultiRange());
        }
        internal static bool EvalNew(MusicInfo musicInfo, MultiRange mr)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalNewInternal(musicInfo, mr);
        }
        internal static bool EvalNewInternal(MusicInfo musicInfo, MultiRange mr)
        {
            InitNewIfNeeded();
            var idx = sortedByLastModified!.IndexOf(musicInfo.uid);
            if (idx == -1)
            {
                return false;
            }
            return mr.Contains(idx);
        }
        internal static bool EvalNew(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalNew(M.I, n);
                case string s:
                    return EvalNew(M.I, s);
                case Range r:
                    return EvalNew(M.I, r);
                case PythonRange pr:
                    return EvalNew(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalNew(M.I, mr);
                default:
                    break;
            }
            throw new SearchWrongTypeException("an integer, a range string, or a range object", varArgs[0]?.GetType(), "New()");
        }
        internal static void InitNewIfNeeded()
        {
            if (!ModMain.CustomAlbumsLoaded)
            {
                return;
            }
            InitNewIfNeededInternal();
        }
        internal static void InitNewIfNeededInternal()
        {
            sortedByLastModified ??= AlbumManager.LoadedAlbums.Values
                .OrderByDescending(x => File.GetLastWriteTimeUtc(x.Path))
                .Select(x => x.Uid).ToList();
        }
    }
}
