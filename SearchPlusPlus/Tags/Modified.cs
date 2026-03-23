using System.Numerics;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
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
                            throw new SearchInputException("time offset given as 'modified' argument is too large");
                        }
                        return EvalModified(M.I, (long)n);
                    }
                default:
                    break;
            }
            throw new SearchInputException("expected time offset (integer) as 'modified' argument");
        }
    }
}
