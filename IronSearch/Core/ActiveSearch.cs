using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppPeroPeroGames.GlobalDefines;
using IronSearch.Infrastructure;
using IronSearch.Records;
using IronSearch.Tags;
using IronSearch.Utils;
using MelonLoader;
using Newtonsoft.Json;
using PythonExpressionManager;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace IronSearch.Core
{
    public static class ActiveSearch
    {
        internal static ConcurrentDictionary<string, ConcurrentDictionary<int, SorterInfo>> _activeSorters = new();
        internal static int langIndex;
        internal static Dictionary<string, Highscore> highScores { get; set; } = new();

        internal static HashSet<string> fullCombos { get; set; } = new();

        internal static List<string> history { get; set; } = new();

        internal static HashSet<string> favorites { get; set; } = new();

        internal static HashSet<string> hides { get; set; } = new();

        private static bool firstPrepare = true;
        internal static HashSet<string> streamer { get; set; } = new();
        public static ReadOnlyDictionary<string, Highscore> HighScores => new(highScores);
        public static ReadOnlyCollection<string> FullCombos => fullCombos.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> History => history.AsReadOnly();
        public static ReadOnlyCollection<string> Favorites => favorites.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> Hides => hides.ToList().AsReadOnly();
        public static ReadOnlyCollection<string> Streamer => streamer.ToList().AsReadOnly();

        internal static readonly Dictionary<string, SearchCache> searchCache = new();

        internal static readonly Dictionary<string, Dictionary<int, LocalInfo>> localInfos = new();
        internal static bool? isAdvancedSearch = false;
        internal static bool SkipNextCall = true;

        internal static MusicSearchWorkerManager? workerManager;

        private static Stopwatch _sw = new();

        private static object _lock = new object();
        public static SearchResult Run(string keyword, out List<MusicInfo> finalResult)
        {
            SearchResult result;
            lock (_lock)
            {
                result = RunPrivate(keyword, out finalResult);
            }
            return result;
        }
        private static SearchResult RunPrivate(string keyword, out List<MusicInfo> finalResult)
        {
            finalResult = null!;
            if (SkipNextCall || string.IsNullOrEmpty(keyword) || !keyword.StartsWith(ModMain.Config.StartString, StringComparison.InvariantCultureIgnoreCase))
            {
                SkipNextCall = false;
                return SearchResult.Vanilla;
            }

            isAdvancedSearch = true;

            string expression = keyword[ModMain.Config.StartString.Length..].Trim();

            BuiltIns.GlobalVariables.Clear();
            BuiltIns.LocalVariables.Clear();
            BuiltIns.logOnceIds.Clear();
            BuiltIns.logUnique.Clear();
            BuiltIns.helpIds.Clear();
            BuiltIns.runOnceIds.Clear();

            var allMusic = new Il2CppSystem.Collections.Generic.List<MusicInfo>();
            GlobalDataBase.s_DbMusicTag.GetAllMusicInfo(allMusic);
            finalResult = new();

            if (ModMain.Config.EnablePersistentSearchCaching && searchCache.TryGetValue(expression, out var cache))
            {
                CachedSearch(finalResult, cache, allMusic);
                return SearchResult.OK;
            }
            else if (searchCache.TryGetValue(expression, out cache))
            {
                if (!(cache.Expiration is not { } exp || exp < DateTime.UtcNow))
                {
                    CachedSearch(finalResult, cache, allMusic);
                    return SearchResult.OK;
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
                compiledScript = ModMain.SearchManager.ScriptManager.ScriptExecutor.Compile(expression);
            }
            catch (Exception ex)
            {
                isAdvancedSearch = null;
                _sw.Stop();
                try
                {
                    if (!CompiledScript.TryConvertException(ex, ModMain.SearchManager.ScriptManager.ScriptExecutor.Engine))
                    {
                        throw;
                    }
                }
                catch (Exception ex2)
                {
                    new SearchResponse("Failed to parse search.", ex2, SearchResponse.Type.ParserError).PrintSearchError();
                }
                return SearchResult.Error;
            }

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
                return SearchResult.Error;
            }

            // restore default order (for determinism)
            var matchedUids = results.Select(x => x.uid).ToHashSet();

            foreach (var item in allMusic)
            {
                if (matchedUids.Contains(item.uid))
                {
                    finalResult.Add(item);
                }
            }

            _sw.Stop();

            MelonLogger.Msg($"Advanced search found {finalResult.Count} songs in {_sw.Elapsed.TotalSeconds:F1}s.");
            _sw.Restart();


            SorterMethods.sortingFlag = true;
            bool criticalFlag = true;
            try
            {
                SorterMethods._randomDictionary.Clear();

                if (!_activeSorters.IsEmpty)
                {
                    var sorterInfos = _activeSorters.Values.SelectMany(x => x.Values).ToList();

                    sorterInfos.Sort();

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


                    try
                    {
                        finalResult.Sort(comparer);

                        DateTime? expirationTime = ModMain.Config.EnablePersistentSearchCaching ? null : DateTime.UtcNow.AddSeconds(AdvancedSearchManager.MiniCacheTimeout);

                        searchCache.TryAdd(expression, new SearchCache(finalResult, true, expirationTime));
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
                    DateTime? expirationTime = ModMain.Config.EnablePersistentSearchCaching ? null : DateTime.UtcNow.AddSeconds(AdvancedSearchManager.MiniCacheTimeout);

                    searchCache.TryAdd(expression,
                        new SearchCache(
                            finalResult,
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
                return SearchResult.Error;
            }
            finally
            {
                if (!_activeSorters.IsEmpty)
                {
                    MelonLogger.Msg($"Sorting completed in {_sw.Elapsed.TotalSeconds:F1}s.");
                }
                SorterMethods.sortingFlag = false;
                _activeSorters.Clear();
                _sw.Stop();
            }


            return SearchResult.OK;
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
        private static void CachedSearch(List<MusicInfo> buffer, SearchCache cache, Il2CppSystem.Collections.Generic.List<MusicInfo> allMusic)
        {
            if (!cache.ShouldSort)
            {
                foreach (var info in allMusic)
                {
                    if (cache.UIDToIndex.ContainsKey(info.uid))
                    {
                        buffer.Add(info);
                    }
                }
                PrintCachedSearch(cache);
                return;
            }

            var buildUnlock = new MusicInfo[cache.UIDToIndex.Count];

            foreach (var mi in allMusic)
            {
                if (cache.UIDToIndex.TryGetValue(mi.uid, out var i))
                {
                    buildUnlock[i] = mi;
                }
            }

            buffer = buildUnlock.Where(x => x is not null).ToList();
            PrintCachedSearch(cache);
            return;
        }
        private static void LoadCustomData()
        {
            if (MapUtils.GetCustomAlbumsSave() is not CustomAlbumsSave save)
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
        private static void PrepareSearchData(Il2CppSystem.Collections.Generic.List<MusicInfo> allMusic)
        {
            if (firstPrepare)
            {
                firstPrepare = false;
                PrepareSearchDataFirstTime(allMusic);
            }
            langIndex = Language.LanguageToIndex(SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language"));
            history = DataHelper.history.ToSystem();
            highScores = DataHelper.highest.ToSystem().Select(x => x.ScoresToObjects()).ToDictionary(x => x.Uid, x => x);
            fullCombos = DataHelper.fullComboMusic.ToSystem().ToHashSet();
            favorites = DataHelper.collections.ToSystem().ToHashSet();
            hides = DataHelper.hides.ToSystem().ToHashSet();

            try
            {
                streamer = Singleton<AnchorModule>.instance.m_DbAnchor.m_AnchorMusicInfos.ToSystem().Keys.ToHashSet();
            }
            catch
            {
                streamer = new();
            }

            if (ModMain.CustomAlbumsLoaded)
            {
                LoadCustomData();
            }
        }
    }
}