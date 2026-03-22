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
        internal static bool EvalSince(MusicInfo musicInfo, long tickOffset)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalSinceInternal(musicInfo, tickOffset);
        }
        internal static bool EvalSinceInternal(MusicInfo musicInfo, long tickOffset)
        {
            var album = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            return File.GetLastWriteTimeUtc(album.Path) >= DateTime.UtcNow.Subtract(new TimeSpan(tickOffset));
        }
        internal static bool EvalSince(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case int n:
                    return EvalSince(M.I, n);
                case long n:
                    return EvalSince(M.I, n);
                case BigInteger n:
                    {
                        if (n > long.MaxValue)
                        {
                            throw new SearchInputException("time offset given as 'recent' argument is too large");
                        }
                        return EvalSince(M.I, (long)n);
                    }
                default:
                    break;
            }
            throw new SearchInputException("expected time offset (integer) as 'recent' argument");
        }
    }
}
