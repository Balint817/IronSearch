using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalRecent(MusicInfo musicInfo, long tickOffset)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalRecentInternal(musicInfo, tickOffset);
        }
        internal static bool EvalRecentInternal(MusicInfo musicInfo, long tickOffset)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            return File.GetLastWriteTimeUtc(album.Path) >= DateTime.UtcNow.Subtract(new TimeSpan(tickOffset));
        }
        internal static bool EvalRecent(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalRecent(M.I, n);
                case long n:
                    return EvalRecent(M.I, n);
                case BigInteger n:
                    {
                        if (n > long.MaxValue)
                        {
                            throw new SearchInputException("time offset given as 'recent' argument is too large");
                        }
                        return EvalRecent(M.I, (long)n);
                    }
                default:
                    break;
            }
            throw new SearchInputException("expected time offset (integer) as 'recent' argument");
        }
    }
}
