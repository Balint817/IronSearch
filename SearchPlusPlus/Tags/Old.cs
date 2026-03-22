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
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalOld(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw new SearchInputException($"failed to parse range '{s}'");
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
                case MultiRange mr:
                    return EvalOld(M.I, mr);
                default:
                    break;
            }
            throw new SearchInputException("expected integer, string range, or range, as 'old' argument");
        }
    }
}
