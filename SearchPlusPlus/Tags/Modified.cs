using System.Numerics;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalModified(MusicInfo musicInfo, long tickOffset)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalModifiedInternal(musicInfo, tickOffset);
        }
        internal static bool EvalModifiedInternal(MusicInfo musicInfo, long tickOffset)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            return File.GetLastWriteTimeUtc(album.Path) >= DateTime.UtcNow.Subtract(new TimeSpan(tickOffset));
        }
        internal static bool EvalModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalModified(M.I, n);
                case long n:
                    return EvalModified(M.I, n);
                case BigInteger n:
                    {
                        if (n > long.MaxValue)
                        {
                            throw new SearchValidationException("The time offset is too large (must fit in a 64-bit signed integer).", "Modified()");
                        }
                        return EvalModified(M.I, (long)n);
                    }
                case string s:
                    if (!s.TryTimeStringToTicks(out var l))
                    {
                        throw new SearchValidationException("Could not parse that string as a time offset (e.g. duration like 1h30m or a tick count).", "Modified()");
                    }
                    return EvalModified(M.I, l);
                default:
                    break;
            }
            throw new SearchWrongTypeException("an integer, long, or time string for how far back to look", varArgs[0]?.GetType(), "Modified()");
        }
    }
}
