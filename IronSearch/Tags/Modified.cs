using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalModified(MusicInfo musicInfo, int seconds)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalModifiedInternal(musicInfo, new Range(double.NegativeInfinity, seconds));
        }
        internal static bool EvalModified(MusicInfo musicInfo, Range r)
        {
            if (r == Range.InvalidRange)
            {
                throw new SearchValidationException("The wildcard '?' cannot be used as a time range", "Modified()");
            }
            return EvalModifiedInternal(musicInfo, r);
        }
        internal static bool EvalModifiedInternal(MusicInfo musicInfo, Range r)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            return r.Contains(DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(album.Path)).TotalSeconds);
        }
        internal static bool EvalModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalModified(M.I, n);
                case string s:
                    if (!s.TryTimeStringRangeToTimeRange(out var r))
                    {
                        throw new SearchValidationException("Could not parse that string as a time offset (e.g. duration like 1h30m).", "Modified()");
                    }
                    return EvalModified(M.I, r);
                case Range range:
                    return EvalModified(M.I, range);
                case PythonRange pr:
                    return EvalModified(M.I, (Range)pr);
                default:
                    break;
            }
            throw new SearchWrongTypeException("an integer, range, or time string for how far back to look", varArgs[0]?.GetType(), "Modified()");
        }
    }
}
