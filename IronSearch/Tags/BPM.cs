using System.Collections.Concurrent;
using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static ConcurrentDictionary<string, Range?> bpmDict = new();
        internal static bool EvalBPM(MusicInfo musicInfo, string value)
        {
            if (!Utils.ParseRange(value, out var bpmRange))
            {
                throw SearchParseException.ForRange(value, "BPM()", "a BPM range (e.g. 160-180)");
            }
            return EvalBPM(musicInfo, bpmRange);
        }
        internal static bool EvalBPM(MusicInfo musicInfo, Range value)
        {
                return EvalBPM(musicInfo, value.AsMultiRange());
        }
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
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case string s:
                    return EvalBPM(M.I, s);
                case Range r:
                    return EvalBPM(M.I, r);
                case PythonRange pr:
                    return EvalBPM(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalBPM(M.I, mr);
            }
            throw new SearchWrongTypeException("a BPM range string, Python range, or multi-range object", varArgs[0]?.GetType(), "BPM()");
        }
    }
}
