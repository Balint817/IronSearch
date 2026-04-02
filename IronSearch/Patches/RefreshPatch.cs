using CustomAlbums.Data;
using CustomAlbums.Managers;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppPeroPeroGames.GlobalDefines;
using IronSearch.Records;
using IronSearch.Tags;
using MelonLoader;
using Newtonsoft.Json;
using PythonExpressionManager;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(SearchResults), "RefreshData")]
    internal static class RefreshPatch
    {

        internal static List<SorterInfo> _activeSorters = new();
        internal static int langIndex;


        internal static Dictionary<string, Highscore> highScores { get; set; } = new();

        internal static HashSet<string> fullCombos { get; set; } = new();

        internal static List<string> history { get; set; } = new();

        internal static HashSet<string> favorites { get; set; } = new();

        internal static HashSet<string> hides { get; set; } = new();

        internal static HashSet<string> streamer { get; set; } = new();
        public static ReadOnlyDictionary<string, Highscore> HighScores => new(highScores);
        public static ReadOnlyCollection<string> FullCombos => fullCombos.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> History => history.AsReadOnly();
        public static ReadOnlyCollection<string> Favorites => favorites.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> Hides => hides.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> Streamer => streamer.ToList().AsReadOnly();

        internal static readonly Dictionary<string, SearchCache> searchCache = new();

        internal static readonly Dictionary<string, Dictionary<int, LocalInfo>> localInfos = new();
        internal static bool? isAdvancedSearch { get; private set; } = false;

        //Singleton<TerminalManager>
        //DBMusicTagDefine.newMusicUids;
        private static Stopwatch _sw = new();

        internal static void Postfix()
        {
            isAdvancedSearch = false;
        }
        private static bool IsFirstCall = true;

        internal static MusicSearchWorkerManager? workerManager;
        internal static bool Prefix(SearchResults __instance, string keyword)
        {
            isAdvancedSearch = false;
            if (IsFirstCall || string.IsNullOrEmpty(keyword) || !keyword.StartsWith(ModMain.StartString))
            {
                IsFirstCall = false;
                return true;
            }

            isAdvancedSearch = true;

            string expression = keyword[ModMain.StartString.Length..].Trim();

            BuiltIns.GlobalVariables.Clear();
            BuiltIns.LocalVariables.Clear();
            BuiltIns.logOnceIds.Clear();
            BuiltIns.logUnique.Clear();
            BuiltIns.helpIds.Clear();
            BuiltIns.runOnceIds.Clear();
            __instance.musicResult.m_Unlock.Clear();
            __instance.musicResult.m_Lock.Clear();
            __instance.authorResult.m_Unlock.Clear();
            __instance.authorResult.m_Lock.Clear();
            __instance.levelDesignerResult.m_Unlock.Clear();
            __instance.levelDesignerResult.m_Lock.Clear();
            __instance.musicAlbumResults.Clear();

            var allMusic = new Il2CppSystem.Collections.Generic.List<MusicInfo>();
            GlobalDataBase.s_DbMusicTag.GetAllMusicInfo(allMusic);

            if (ModMain.EnablePersistentSearchCaching && searchCache.TryGetValue(expression, out var cache))
            {
                CachedSearch(__instance, cache, allMusic);
                return false;
            }
            else if (searchCache.TryGetValue(expression, out cache))
            {
                if (!(cache.Expiration is not { } exp || exp < DateTime.UtcNow))
                {
                    CachedSearch(__instance, cache, allMusic);
                    return false;
                }
                else
                {
                    searchCache.Remove(expression);
                }
            }

            _sw.Restart();

            PrepareSearchData(allMusic);

            CompiledScript compiledScript;
            try
            {
                compiledScript = ModMain.ScriptManager.ScriptExecutor.Compile(expression);
            }
            catch (Exception ex)
            {
                isAdvancedSearch = null;
                _sw.Stop();
                try
                {
                    if (!CompiledScript.TryConvertException(ex, ModMain.ScriptManager.ScriptExecutor.Engine))
                    {
                        throw;
                    }
                }
                catch (Exception ex2)
                {
                    new SearchResponse("Failed to parse search.", ex2, SearchResponse.Type.ParserError).PrintSearchError();
                }
                return false;
            }

            //using var threadLocalPs = new ThreadLocal<PeroString>(() => new PeroString(1000));
            //using var threadLocalArg = new ThreadLocal<SearchArgument>(() => new SearchArgument(null!, null!));

            var processorCount = Math.Clamp(
                UnityEngine.SystemInfo.processorCount,
                2,
                Math.Max(allMusic.Count / 100, 4)
            );

            workerManager ??= new(processorCount);

            if (!workerManager.Run(allMusic.ToSystem(), compiledScript, out var results))
            {
                _sw.Stop();
                isAdvancedSearch = null;
                return false;
            }

            // restore default order (for determinism)
            var matchedUids = results.Select(x => x.uid).ToHashSet();

            foreach (var item in allMusic)
            {
                if (matchedUids.Contains(item.uid))
                {
                    __instance.musicResult.m_Unlock.Add(item);
                }
            }

            _sw.Stop();

            MelonLogger.Msg($"Advanced search found {__instance.musicResult.m_Unlock.Count} songs in {_sw.Elapsed.TotalSeconds:F1}s.");
            _sw.Restart();


            SorterMethods.sortingFlag = true;
            bool criticalFlag = true;
            try
            {
                SorterMethods._randomDictionary.Clear();

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

                    List<MusicInfo> m_Unlock = __instance.musicResult.m_Unlock.ToSystem();

                    try
                    {
                        m_Unlock.Sort(comparer);

                        __instance.musicResult.m_Unlock = m_Unlock.ToIL2CPP();


                        DateTime? expirationTime = ModMain.EnablePersistentSearchCaching ? null : DateTime.UtcNow.AddSeconds(ModMain.MiniCacheTimeout);

                        searchCache.TryAdd(expression, new SearchCache(new List<MusicInfo>(), m_Unlock, true, expirationTime));
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

                    searchCache.TryAdd(expression,
                        new SearchCache(
                            new List<MusicInfo>(),
                            __instance.musicResult.m_Unlock.ToSystem(),
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
                if (_activeSorters.Count != 0)
                {
                    MelonLogger.Msg($"Sorting completed in {_sw.Elapsed.TotalSeconds:F1}s.");
                }
                SorterMethods.sortingFlag = false;
                _activeSorters.Clear();
                _sw.Stop();
            }


            return false;
        }

        private static void PrintCachedSearch(SearchCache searchCache)
        {
            if (searchCache.Expiration.HasValue)
            {
                MelonLogger.Msg("Skipped double-processing search.");
            }
            else
            {
                MelonLogger.Msg("Got search results from cache.");
            }
        }
        private static void CachedSearch(SearchResults __instance, SearchCache cache, Il2CppSystem.Collections.Generic.List<MusicInfo> allMusic)
        {
            foreach (var info in allMusic)
            {
                if (cache.PassingUids.Contains(info.uid))
                {
                    __instance.musicResult.m_Unlock.Add(info);
                }
            }

            if (!cache.ShouldSort)
            {
                PrintCachedSearch(cache);
                return;
            }
            var buildUnlock = new MusicInfo[cache.Unlock.Count];

            List<MusicInfo> m_Unlock = __instance.musicResult.m_Unlock.ToSystem();

            bool cacheValid = true;
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
                __instance.musicResult.m_Unlock = buildUnlock.Where(x => x != null).ToIL2CPP();
                PrintCachedSearch(cache);
            }
        }
        private static void LoadCustomData()
        {
            if (Utils.GetCustomAlbumsSave() is not CustomAlbumsSave save)
            {
                return;
            }
            if (save.History is not null)
            {
                history.AddRange(save.History);
            }
            if (save.Collections is not null)
            {
                foreach (var item in save.Collections)
                {
                    favorites.Add(item);
                }
            }
            if (save.Hides is not null)
            {
                foreach (var item in save.Hides)
                {
                    hides.Add(item);
                }
            }

            if (save.FullCombo is not null)
            {
                foreach (var kv in save.FullCombo)
                {
                    var albumName = kv.Key;
                    var album = AlbumManager.LoadedAlbums.Values.FirstOrDefault(a => a.AlbumName == albumName);
                    if (album is null)
                    {
                        continue;
                    }
                    var uidBase = album.Uid + '_';
                    foreach (var index in kv.Value)
                    {
                        fullCombos.Add(uidBase + index);
                    }
                }
            }

            if (save.Highest is not null)
            {
                foreach (var nameToScoreDict in save.Highest)
                {
                    var albumName = nameToScoreDict.Key;
                    var album = AlbumManager.LoadedAlbums.Values.FirstOrDefault(a => a.AlbumName == albumName);
                    if (album is null)
                    {
                        continue;
                    }
                    var uidBase = album.Uid + '_';
                    foreach (var indexToScore in nameToScoreDict.Value)
                    {
                        var score = indexToScore.Value;

                        var newScore = new Highscore()
                        {
                            Accuracy = score.Accuracy,
                            AccuracyStr = score.AccuracyStr,
                            Clears = (int)score.Clear,
                            Combo = score.Combo,
                            Evaluate = score.Evaluate,
                            Score = score.Score,
                            Uid = uidBase + indexToScore.Key,
                        };

                        highScores.Add(newScore.Uid, newScore);
                    }
                }
            }
        }

        private static void PrepareSearchDataFirstTime(Il2CppSystem.Collections.Generic.List<MusicInfo> allMusic)
        {
            try
            {
                foreach (var mi in allMusic)
                {
                    if (!localInfos.TryGetValue(mi.uid, out var dict))
                    {
                        localInfos[mi.uid] = dict = new();
                    }
                    for (int i = 1; i <= 5; i++)
                    {
                        dict[i] = new(mi.GetLocal(i));
                    }
                }

                File.WriteAllText("localInfos.json", JsonConvert.SerializeObject(localInfos));
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ex);
            }
        }

        private static bool firstPrepare = true;
        private static void PrepareSearchData(Il2CppSystem.Collections.Generic.List<MusicInfo> allMusic)
        {
            if (firstPrepare)
            {
                firstPrepare = false;
                PrepareSearchDataFirstTime(allMusic);
            }
            RefreshPatch.langIndex = Language.LanguageToIndex(SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language"));
            RefreshPatch.history = DataHelper.history.ToSystem();
            RefreshPatch.highScores = DataHelper.highest.ToSystem().Select(x => x.ScoresToObjects()).ToDictionary(x => x.Uid, x => x);
            RefreshPatch.fullCombos = DataHelper.fullComboMusic.ToSystem().ToHashSet();
            RefreshPatch.favorites = DataHelper.collections.ToSystem().ToHashSet();
            RefreshPatch.hides = DataHelper.hides.ToSystem().ToHashSet();

            try
            {
                RefreshPatch.streamer = Singleton<AnchorModule>.instance.m_DbAnchor.m_AnchorMusicInfos.ToSystem().Keys.ToHashSet();
            }
            catch
            {
                RefreshPatch.streamer = new();
            }

            if (ModMain.CustomAlbumsLoaded)
            {
                RefreshPatch.LoadCustomData();
            }
        }
    }
}