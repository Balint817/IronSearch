using System.Collections.Concurrent;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static ConcurrentDictionary<string, Range?> bpmDict = new();
        internal static bool EvalBPM(MusicInfo musicInfo, MultiRange value)
        {
            AddBPMInfo(musicInfo);
            var bpmInfo = bpmDict[musicInfo.uid];

            if (bpmInfo == null)
            {
                return value == MultiRange.InvalidRange;
            }
            if (value == MultiRange.InvalidRange)
            {
                return false;
            }

            if (value.IsOverlap(bpmInfo))
            {
                return true;
            }
            return false;
        }

        internal static void AddBPMInfo(MusicInfo musicInfo)
        {
            if (bpmDict.ContainsKey(musicInfo.uid))
            {
                return;
            }
            if (Utils.DetectParseBPM(musicInfo.bpm, out var range))
            {
                bpmDict[musicInfo.uid] = range;
                return;
            }
            bpmDict[musicInfo.uid] = null;
        }

        internal static bool EvalBPM(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "BPM()");
            ThrowIfNotEmpty(varKwargs, "BPM()");

            var mr = MultiRangeArgumentParser.GetMultiRange(varArgs[0], "BPM()");
            return EvalBPM(M.I, mr);
        }
    }
}
