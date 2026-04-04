using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using IronSearch.Core;
using IronSearch.Exceptions;
using IronSearch.Loaders;
using IronSearch.Patches;
using IronSearch.Tags;

namespace IronSearch.Utils
{

    //[HarmonyLib.HarmonyPatch(typeof(SearchResults), "Merge")]
    internal static class SorterMethods
    {
        internal static bool sortingFlag = true;
        internal static int SortByUID(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByUID()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            return musicInfo1.uid.CompareTo(musicInfo2.uid);
        }
        internal static int SortByName(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByName()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            return musicInfo1.GetLocalSafe(ActiveSearch.langIndex).Name.CompareTo(musicInfo2.GetLocalSafe(ActiveSearch.langIndex).Name);
        }

        internal static int SortByAccuracy(MusicInfo m1, MusicInfo m2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByAccuracy()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }

            var acc1 = GetMaxAccuracy(m1);
            var acc2 = GetMaxAccuracy(m2);

            return acc1.CompareTo(acc2);
        }

        private static float GetMaxAccuracy(MusicInfo musicInfo)
        {
            if (!MapUtils.GetAvailableMaps(musicInfo, out var maps))
            {
                return float.MinValue;
            }

            float max = float.MinValue;

            foreach (var map in maps)
            {
                string uid = musicInfo.uid + "_" + map;

                if (ActiveSearch.highScores.TryGetValue(uid, out var score) && score.Accuracy > max)
                {
                    max = score.Accuracy;
                }
            }

            return max;
        }

        public static int SortByBPM(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByBPM()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            if (!BuiltIns.bpmDict.ContainsKey(musicInfo1.uid))
            {
                BuiltIns.AddBPMInfo(musicInfo1);
            }
            if (!BuiltIns.bpmDict.ContainsKey(musicInfo2.uid))
            {
                BuiltIns.AddBPMInfo(musicInfo2);
            }
            var bpmInfo1 = BuiltIns.bpmDict[musicInfo1.uid];
            var bpmInfo2 = BuiltIns.bpmDict[musicInfo2.uid];
            if (bpmInfo1 == null)
            {
                if (bpmInfo2 == null)
                {
                    return 0;
                }
                return -1;
            }
            return bpmInfo1.CompareTo(bpmInfo2);
        }

        internal static readonly Dictionary<string, Dictionary<string, int>> _randomDictionary = new Dictionary<string, Dictionary<string, int>>();
        internal static int SortByRandom(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByRandom()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            if (musicInfo1.uid == musicInfo2.uid)
            {
                // must compare equal to itself
                return 0;
            }

            if (_randomDictionary.TryGetValue(musicInfo1.uid, out var stored))
            {
                if (stored.TryGetValue(musicInfo2.uid, out var value))
                {
                    return value;
                }
            }
            else
            {
                _randomDictionary[musicInfo1.uid] = new Dictionary<string, int>();
            }
            return _randomDictionary[musicInfo1.uid][musicInfo2.uid] = Random.Shared.Next(-1, 2);

        }
        public static int SortByModified(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByModified()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            BuiltIns.InitNewIfNeeded();
            var idx1 = BuiltIns.sortedByLastModified!.IndexOf(musicInfo1.uid);
            var idx2 = BuiltIns.sortedByLastModified.IndexOf(musicInfo2.uid);

            if (idx1 == -1)
            {
                if (idx2 == -1)
                {
                    return 0;
                }
                return 1;
            }
            return idx1.CompareTo(idx2);
        }
        public static int SortByScene(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByScene()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            return musicInfo1.scene.CompareTo(musicInfo2.scene);
        }
        public static int SortByLength(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByLength()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }
            var length1null = LengthLoader.GetMusicLength(musicInfo1);
            var length2null = LengthLoader.GetMusicLength(musicInfo2);

            if (length1null is not { } length1)
            {
                if (length2null is null)
                {
                    return 0;
                }
                return -1;
            }

            if (length2null is not { } length2)
            {
                return 1;
            }

            return length1.CompareTo(length2);

        }
        public static int SortByCallback(MusicInfo m1, MusicInfo m2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByDifficulty()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }

            var max1 = GetMaxCallback(m1);
            var max2 = GetMaxCallback(m2);

            return max1.CompareTo(max2);
        }

        private static int GetMaxCallback(MusicInfo musicInfo)
        {
            if (!MapUtils.GetMapCallbacks(musicInfo, out var diffs))
            {
                return int.MinValue;
            }

            return diffs.Where(x => x != int.MinValue).Max();
        }
        public static int SortByDifficulty(MusicInfo m1, MusicInfo m2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByDifficulty()", Array.Empty<dynamic>(), new Dictionary<string, dynamic>());
            }

            var max1 = GetMaxDifficulty(m1);
            var max2 = GetMaxDifficulty(m2);

            return max1.CompareTo(max2);
        }

        private static int GetMaxDifficulty(MusicInfo musicInfo)
        {
            if (!MapUtils.GetMapDifficulties(musicInfo, out var diffs))
            {
                return int.MinValue;
            }

            int max = int.MinValue;

            foreach (var diff in diffs)
            {
                if (NumberUtils.TryParseInt(diff, out var val) && val > max)
                {
                    max = val;
                }
            }

            return max;
        }

    }
}