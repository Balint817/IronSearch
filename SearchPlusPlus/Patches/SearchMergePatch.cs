using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppPeroPeroGames.GlobalDefines;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronSearch.Records;
using IronSearch.Tags;

namespace IronSearch.Patches
{

    [HarmonyLib.HarmonyPatch(typeof(SearchResults), "Merge")]
    internal static class SearchMergePatch
    {
        private static int _langIndex;
        private static bool sortingFlag = true;

        internal static void Postfix(SearchResults __instance)
        {
            sortingFlag = true;
            bool criticalFlag = true;
            try
            {
                _langIndex = Language.LanguageToIndex(SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language"));
                _randomDictionary.Clear();

                if (SearchPatch.isAdvancedSearch != true || SearchPatch.searchError != null)
                {
                    _activeSorters.Clear();
                    return;
                }

                if (_activeSorters.Count == 0)
                {
                    return;
                }

                _activeSorters.Sort();
                var filteredByPriority = new SortedDictionary<int, SorterInfo>();

                foreach (var sorterInfo in _activeSorters)
                {
                    filteredByPriority[sorterInfo.Priority] = sorterInfo;
                }

                var sorterInfos = filteredByPriority.Values.ToList();

                Comparison<MusicInfo> comparer = (MusicInfo musicInfo1, MusicInfo musicInfo2) =>
                {
                    foreach (var sorterInfo in sorterInfos)
                    {
                        foreach (var processor in sorterInfo.Comparers)
                        {
                            var t = processor(musicInfo1, musicInfo2);
                            if (t is not int result)
                            {
                                throw new InvalidCastException($"expected an integer as comparison result, got '{(t is null ? "null" : t.GetType().Name)}'");
                            }
                            if (result != 0)
                            {
                                if (sorterInfo.Reverse)
                                {
                                    return -result;
                                }
                                return result;
                            }
                        }
                    }
                    return 0;
                };

                List<MusicInfo> m_lock = __instance.m_LevelDesignerResult.m_Lock.ToSystem();
                List<MusicInfo> m_unlock = __instance.m_LevelDesignerResult.m_Unlock.ToSystem();

                try
                {
                    m_lock.Sort(comparer);
                    m_unlock.Sort(comparer);
                }
                catch (Exception)
                {
                    criticalFlag = false;
                    ShowText.ShowInfo("An error occured and sorting failed. This is likely a mistake in the search.");
                    throw;
                }

                __instance.m_LevelDesignerResult.m_Lock = m_lock.ToIL2CPP();
                __instance.m_LevelDesignerResult.m_Unlock = m_unlock.ToIL2CPP();

            }
            catch (Exception)
            {
                if (criticalFlag)
                {
                    ShowText.ShowInfo("A critical error occured and sorting failed. This is likely a bug in the mod.");
                }
                throw;
            }
            finally
            {
                sortingFlag = false;
                _activeSorters.Clear();
            }
        }
        internal static int SortByUID(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            return musicInfo1.uid.CompareTo(musicInfo2.uid);
        }
        internal static int SortByName(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            return musicInfo1.GetLocal(_langIndex).name.CompareTo(musicInfo2.GetLocal(_langIndex).name);
        }
        internal static int SortByAccuracy(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            var hasMaps1 = Utils.GetAvailableMaps(musicInfo1, out var availableMaps1);
            var hasMaps2 = Utils.GetAvailableMaps(musicInfo2, out var availableMaps2);

            if (!hasMaps1)
            {
                if (!hasMaps2)
                {
                    return 0;
                }
                return -1;
            }
            else if (!hasMaps2)
            {
                return 1;
            }

            foreach (var i in availableMaps1.Intersect(availableMaps2).OrderByDescending(x => x))
            {
                string s1 = musicInfo1.uid + "_" + i;
                string s2 = musicInfo2.uid + "_" + i;
                var score1 = RefreshPatch.highScores.Where(x => x.Uid == s1).MaxByOrDefault(x => x.Accuracy, null);
                if (score1 == null)
                {
                    continue;
                }
                var score2 = RefreshPatch.highScores.Where(x => x.Uid == s1).MaxByOrDefault(x => x.Accuracy, null);
                if (score2 == null)
                {
                    continue;
                }
                var acc1 = score1.Accuracy;
                var acc2 = score2.Accuracy;
                int result = acc1.CompareTo(acc2);
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        public static int SortByBPM(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
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

        private static readonly Dictionary<string, Dictionary<string, int>> _randomDictionary = new Dictionary<string, Dictionary<string, int>>();
        internal static int SortByRandom(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
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
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
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
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            return musicInfo1.scene.CompareTo(musicInfo2.scene);
        }
        public static int SortByLength(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            var length1null = AudioHelper.GetMusicLength(musicInfo1);
            var length2null = AudioHelper.GetMusicLength(musicInfo2);

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
        public static int SortByDifficulty(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new InvalidOperationException("You're not supposed to call this, pass this to the sorter as an argument instead!");
            }
            var hasMaps1 = Utils.GetAvailableMaps(musicInfo1, out var availableMaps1);
            var hasMaps2 = Utils.GetAvailableMaps(musicInfo2, out var availableMaps2);

            if (!hasMaps1)
            {
                if (!hasMaps2)
                {
                    return 0;
                }
                return -1;
            }
            else if (!hasMaps2)
            {
                return 1;
            }

            var difficulties1 = availableMaps1.Select(x => musicInfo1.GetMusicLevelStringByDiff(x, false)).Where(x => x.TryParseInt(out var t)).Select(x => int.Parse(x)).ToArray();
            var difficulties2 = availableMaps2.Select(x => musicInfo2.GetMusicLevelStringByDiff(x, false)).Where(x => x.TryParseInt(out var t)).Select(x => int.Parse(x)).ToArray();

            if (!difficulties1.Any())
            {
                if (!difficulties2.Any())
                {
                    return 0;
                }
                return -1;
            }
            else if (!difficulties2.Any())
            {
                return 1;
            }
            var result = difficulties1.OrderByDescending(x => x).Zip(difficulties2.OrderByDescending(x => x), (x, y) => x.CompareTo(y)).FirstOrDefault(x => x != 0);

            return result == 0
                ? difficulties1.Length.CompareTo(difficulties2.Length)
                : result;
        }

        internal static List<SorterInfo> _activeSorters = new();
    }
}