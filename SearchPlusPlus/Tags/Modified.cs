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
                case string s:
                    // AI
                    {
                        s = s.Replace(" ", "").ToLowerInvariant();
                        long totalTicks = 0;
                        int pos = 0;
                        while (pos < s.Length)
                        {
                            // parse number
                            int start = pos;
                            while (pos < s.Length && char.IsDigit(s[pos]))
                            {
                                pos++;
                            }
                            if (start == pos)
                            {
                                throw new SearchInputException("expected time offset (integer) as 'modified' argument");
                            }
                            long value = long.Parse(s[start..pos]);
                            // parse unit
                            if (pos >= s.Length)
                            {
                                throw new SearchInputException("expected time unit after number in 'modified' argument");
                            }
                            char unit = s[pos];
                            pos++;
                            totalTicks += unit switch
                            {
                                's' => value * TimeSpan.TicksPerSecond,
                                'm' => value * TimeSpan.TicksPerMinute,
                                'h' => value * TimeSpan.TicksPerHour,
                                'd' => value * TimeSpan.TicksPerDay,
                                _ => throw new SearchInputException($"invalid time unit '{unit}' in 'modified' argument"),
                            };
                        }
                        return EvalModified(M.I, totalTicks);
                    }
                default:
                    break;
            }
            throw new SearchInputException("expected time offset (integer) as 'modified' argument");
        }
    }
}
