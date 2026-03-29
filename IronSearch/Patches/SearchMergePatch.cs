using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppPeroPeroGames.GlobalDefines;
using IronSearch.Exceptions;
using IronSearch.Records;
using IronSearch.Tags;
using MelonLoader;

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
                if (SearchPatch.isAdvancedSearch != true || SearchPatch.searchError != null)
                {
                    _activeSorters.Clear();
                    return;
                }

                _langIndex = Language.LanguageToIndex(SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language"));
                _randomDictionary.Clear();

                if (SearchPatch.searchCache.TryGetValue(SearchPatch.currentSearchText!, out var cache) && !(cache.Expiration is { } exp && exp < DateTime.UtcNow))
                {
                    if (!cache.ShouldSort)
                    {
                        return;
                    }
                    var buildLock = new MusicInfo[cache.Lock.Count];
                    var buildUnlock = new MusicInfo[cache.Unlock.Count];

                    List<MusicInfo> m_lock = __instance.m_LevelDesignerResult.m_Lock.ToSystem();
                    List<MusicInfo> m_Unlock = __instance.m_LevelDesignerResult.m_Unlock.ToSystem();

                    bool cacheValid = true;

                    foreach (var item in m_lock)
                    {
                        if (!cache.Lock.TryGetValue(item.uid, out var i))
                        {
                            cacheValid = false;
                            break;
                        }
                        buildLock[i] = item;
                    }
                    foreach (var item in m_Unlock)
                    {
                        if (!cache.Unlock.TryGetValue(item.uid, out var i))
                        {
                            cacheValid = false;
                            break;
                        }
                        buildUnlock[i] = item;
                    }

                    if (!cacheValid)
                    {
                        MelonLogger.Error("Something went horribly wrong with the sorting cache! This is likely a bug in the mod.");
                    }
                    else
                    {
                        __instance.m_LevelDesignerResult.m_Lock = buildLock.Where(x => x is not null).ToIL2CPP();
                        __instance.m_LevelDesignerResult.m_Unlock = buildUnlock.Where(x => x is not null).ToIL2CPP();
                    }
                    return;
                }

                if (_activeSorters.Count != 0)
                {

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
                    List<MusicInfo> m_Unlock = __instance.m_LevelDesignerResult.m_Unlock.ToSystem();

                    try
                    {
                        m_lock.Sort(comparer);
                        m_Unlock.Sort(comparer);

                        __instance.m_LevelDesignerResult.m_Lock = m_lock.ToIL2CPP();
                        __instance.m_LevelDesignerResult.m_Unlock = m_Unlock.ToIL2CPP();


                        DateTime? expirationTime = ModMain.EnablePersistentSearchCaching ? null : DateTime.UtcNow.AddSeconds(ModMain.MiniCacheTimeout);

                        SearchPatch.searchCache.TryAdd(SearchPatch.currentSearchText!, new SearchCache(m_lock, m_Unlock, true, expirationTime));
                    }
                    catch (Exception)
                    {
                        criticalFlag = false;
                        var s = "An error occured and sorting failed. This is likely a mistake in the search.";
                        ShowText.ShowInfo(s);
                        MelonLogger.Msg(ConsoleColor.Red, s);
                        throw;
                    }
                }
                else
                {
                    DateTime? expirationTime = ModMain.EnablePersistentSearchCaching ? null : DateTime.UtcNow.AddSeconds(ModMain.MiniCacheTimeout);

                    SearchPatch.searchCache.TryAdd(SearchPatch.currentSearchText!,
                        new SearchCache(
                            __instance.m_LevelDesignerResult.m_Lock.ToSystem(),
                            __instance.m_LevelDesignerResult.m_Unlock.ToSystem(),
                            false,
                            expirationTime
                            ));
                }

                

            }
            catch (Exception ex)
            {
                if (criticalFlag)
                {
                    ShowText.ShowInfo("A critical error occured and sorting failed. This is likely a bug in the mod.");
                }
                MelonLogger.Msg(ConsoleColor.Red, ex);
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
                throw new SearchCallNotAllowed("ByUID()()");
            }
            return musicInfo1.uid.CompareTo(musicInfo2.uid);
        }
        internal static int SortByName(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByName()()");
            }
            return musicInfo1.GetLocal(_langIndex).name.CompareTo(musicInfo2.GetLocal(_langIndex).name);
        }

        internal static int SortByAccuracy(MusicInfo m1, MusicInfo m2)
        {
            if (!sortingFlag)
                throw new SearchCallNotAllowed("ByAccuracy()()");

            var acc1 = GetMaxAccuracy(m1);
            var acc2 = GetMaxAccuracy(m2);

            return acc1.CompareTo(acc2);
        }

        private static float GetMaxAccuracy(MusicInfo musicInfo)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var maps))
                return float.MinValue;

            float max = float.MinValue;

            foreach (var map in maps)
            {
                string uid = musicInfo.uid + "_" + map;

                var score = RefreshPatch.highScores
                    .Where(x => x.Uid == uid)
                    .MaxByOrDefault(x => x.Accuracy, null);

                if (score != null && score.Accuracy > max)
                    max = score.Accuracy;
            }

            return max;
        }

        public static int SortByBPM(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByBPM()()");
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
                throw new SearchCallNotAllowed("ByRandom()()");
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
                throw new SearchCallNotAllowed("ByModified()()");
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
                throw new SearchCallNotAllowed("ByScene()()");
            }
            return musicInfo1.scene.CompareTo(musicInfo2.scene);
        }
        public static int SortByLength(MusicInfo musicInfo1, MusicInfo musicInfo2)
        {
            if (!sortingFlag)
            {
                throw new SearchCallNotAllowed("ByLength()()");
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
        public static int SortByDifficulty(MusicInfo m1, MusicInfo m2)
        {
            if (!sortingFlag)
                throw new SearchCallNotAllowed("ByDifficulty()()");

            var max1 = GetMaxDifficulty(m1);
            var max2 = GetMaxDifficulty(m2);

            return max1.CompareTo(max2);
        }

        private static int GetMaxDifficulty(MusicInfo musicInfo)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var maps))
                return int.MinValue;

            int max = int.MinValue;

            foreach (var map in maps)
            {
                var str = musicInfo.GetMusicLevelStringByDiff(map, false);
                if (Utils.TryParseInt(str, out var val) && val > max)
                    max = val;
            }

            return max;
        }

        internal static List<SorterInfo> _activeSorters = new();
    }
}