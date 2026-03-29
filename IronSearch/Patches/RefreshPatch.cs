using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.Structs.Modules;
using IronSearch.Records;
using IronSearch.Tags;
using MelonLoader;
using PythonExpressionManager;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace IronSearch.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(SearchResults), "RefreshData")]
    internal static class RefreshPatch
    {

        internal static List<Highscore> highScores { get; set; } = new();

        internal static List<string> fullCombos { get; set; } = new();

        internal static List<string> history { get; set; } = new();

        internal static List<string> favorites { get; set; } = new();

        internal static List<string> hides { get; set; } = new();

        internal static List<string> streamer { get; set; } = new();
        public static ReadOnlyCollection<Highscore> HighScores => highScores.AsReadOnly();
        public static ReadOnlyCollection<string> FullCombos => fullCombos.AsReadOnly();
        public static ReadOnlyCollection<string> History => history.AsReadOnly();
        public static ReadOnlyCollection<string> Favorites => favorites.AsReadOnly();
        public static ReadOnlyCollection<string> Hides => hides.AsReadOnly();
        public static ReadOnlyCollection<string> Streamer => streamer.AsReadOnly();

        //Singleton<TerminalManager>
        //DBMusicTagDefine.newMusicUids;
        private static Stopwatch _sw = new();
        internal static void Postfix()
        {
            if (SearchPatch.isAdvancedSearch != true)
            {
                return;
            }

            ////var _hides = DataHelper.hides;
            ////foreach (var item in hides)
            ////{
            ////    _hides.Add(item);
            ////}
            BuiltIns.sortedByLastModified = null;
            if (SearchPatch.searchError != null)
            {
                SearchPatch.isAdvancedSearch = false;
                SearchPatch.searchError.PrintSearchError();
            }
            else
            {
                _sw.Stop();
                MelonLogger.Msg($"Advanced search completed in {_sw.Elapsed.TotalSeconds:F1} seconds.");
            }
            if (BuiltIns.isCinemaModified)
            {
                BuiltIns.lastCheckedCinema = DateTime.UtcNow;
            }
        }

        internal static bool FirstCall = true;

        internal static void Prefix(string keyword)
        {
            SearchPatch.currentSearchText = null;
            SearchPatch.currentCache = null;
            if (ModMain.UISystemLoaded && ModMain.IsFirstLengthCacheBuild)
            {
                ModMain.BuildCacheIfNecessary();
            }
            //favorites = DataHelper.collections
            //hiddenSongs = DataHelper.hides
            var text = keyword;
            if (FirstCall)
            {
                text = Utils.FindKeyword;
                FirstCall = false;
            }
            else if (!ModMain.InitSuccessful)
            {
                return;
            }
            if (ModMain.StartString == null)
            {
                return;
            }
            ModMain.LoadAlbumNames();
            BuiltIns.logUnique.Clear();
            BuiltIns.logOnceIds.Clear();
            BuiltIns.helpIds.Clear();
            BuiltIns.helpEnabled = true;
            SearchPatch.searchError = null;
            BuiltIns.GlobalVariables.Clear();
            BuiltIns.LocalVariables.Clear();

            if (!text.StartsWith(ModMain.StartString))
            {
                SearchPatch.isAdvancedSearch = false;
                NullifyAdvancedSearch();
                return;
            }

            text = text[ModMain.StartString.Length..].Trim(' ');

            _sw.Restart();
            CompiledScript parseResult;
            try
            {
                parseResult = ModMain.ScriptManager.ScriptExecutor.Compile(text);
            }
            catch (Exception ex)
            {
                _sw.Stop();
                new SearchResponse("Failed to parse search.", ex, SearchResponse.Type.ParserError).PrintSearchError();
                return;
            }

            SearchPatch.currentSearchText = text;
            SearchPatch.compiledScript = parseResult;
            history = DataHelper.history.ToSystem();
            highScores = DataHelper.highest.ToSystem().Select(x => x.ScoresToObjects()).ToList();
            fullCombos = DataHelper.fullComboMusic.ToSystem();
            favorites = DataHelper.collections.ToSystem();
            hides = DataHelper.hides.ToSystem();
            try
            {
                streamer = Singleton<AnchorModule>.instance.m_DbAnchor.m_AnchorMusicInfos.ToSystem().Keys.ToList();
            }
            catch (Exception)
            {
                streamer = new List<string>();
            }

            if (ModMain.CustomAlbumsLoaded)
            {
                try
                {
                    LoadCustomData();
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg(ConsoleColor.Red, "Failed to load custom album data: " + ex);
                }
            }
            //DataHelper.hides.Clear();

            SearchPatch.isAdvancedSearch = true;

            if (SearchPatch.searchCache.TryGetValue(SearchPatch.currentSearchText, out var cache) && !(cache.Expiration is { } exp && exp < DateTime.UtcNow))
            {
                SearchPatch.currentCache = cache;
            }
            //MelonLogger.Msg("Parsed tags: $" + string.Join(" ", SearchPatch.tagGroups.Select(x1 => string.Join("|", x1.Select(x2 => TermToString(x2))))) + '$');
        }

        private static void LoadCustomData()
        {
            var save = Utils.GetCustomAlbumsSave() as CustomAlbumsSave;
            if (save is null)
            {
                return;
            }
            if (save.History is not null)
            {
                history.AddRange(save.History);
            }
            if (save.Collections is not null)
            {
                favorites.AddRange(save.Collections);
            }
            if (save.Hides is not null)
            {
                hides.AddRange(save.Hides);
            }

            if (save.FullCombo is not null)
            {
                fullCombos.AddRange(save.FullCombo.SelectMany(kv =>
                {
                    var albumName = kv.Key;
                    var album = AlbumManager.LoadedAlbums.Values.FirstOrDefault(a => a.AlbumName == albumName);
                    if (album is null)
                    {
                        return Array.Empty<string>();
                    }
                    var uidBase = album.Uid + '_';
                    return kv.Value.Select(index => uidBase + index);
                }));
            }

            if (save.Highest is not null)
            {
                highScores.AddRange(save.Highest.SelectMany(nameToScoreDict =>
                {
                    var albumName = nameToScoreDict.Key;
                    var album = AlbumManager.LoadedAlbums.Values.FirstOrDefault(a => a.AlbumName == albumName);
                    if (album is null)
                    {
                        return Array.Empty<Highscore>();
                    }
                    var uidBase = album.Uid + '_';
                    return nameToScoreDict.Value.Select(indexToScore =>
                    {
                        var score = indexToScore.Value;
                        return new Highscore()
                        {
                            Accuracy = score.Accuracy,
                            AccuracyStr = score.AccuracyStr,
                            Clear = (int)score.Clear,
                            Combo = score.Combo,
                            Evaluate = score.Evaluate,
                            Score = score.Score,
                            Uid = uidBase + indexToScore.Key,
                        };

                    });
                }));
            }
        }

        private static void NullifyAdvancedSearch()
        {
            SearchPatch.compiledScript = null!;
        }
    }
}