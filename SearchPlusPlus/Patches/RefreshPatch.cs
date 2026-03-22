using System.Collections.ObjectModel;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.Structs.Modules;
using IronSearch.Records;
using IronSearch.Tags;
using PythonExpressionManager;

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
            if (BuiltIns.isModified)
            {
                BuiltIns.lastChecked = DateTime.UtcNow;
            }
        }

        internal static bool FirstCall = true;
        internal static void Prefix(string keyword)
        {
            //favorites = DataHelper.collections
            //hiddenSongs = DataHelper.hides
            var text = keyword;
            if (FirstCall)
            {
                text = Utils.FindKeyword;
                FirstCall = false;
            }
            else if (!ModMain.InitFinished)
            {
                return;
            }
            if (ModMain.StartString == null)
            {
                return;
            }
            ModMain.LoadAlbumNames();
            BuiltIns.uniqueLogs.Clear();
            BuiltIns.logOnceIds.Clear();
            SearchPatch.searchError = null;
            BuiltIns.GlobalVariables.Clear();
            BuiltIns.LocalVariables.Clear();
            //string text = Utils.FindKeyword;

            //highScores = DataHelper.highest;

            //IData:
            //"uid" string
            //"evaluate" int
            //"score" int
            //"combo" int
            //"clear" int
            //"accuracyStr" string
            //"accuracy" float;

            if (!text.StartsWith(ModMain.StartString))
            {
                SearchPatch.isAdvancedSearch = false;
                NullifyAdvancedSearch();
                return;
            }
            //MelonLogger.Msg(Utils.Separator);

            //if (text.Length < SearchPatch.startString.Length + 1)
            //{
            //    SearchPatch.isAdvancedSearch = null;
            //    NullifyAdvancedSearch();
            //    MelonLogger.Msg(ConsoleColor.Red, "syntax error: advanced search was empty");
            //    return;
            //}
            text = text[ModMain.StartString.Length..].Trim(' ');

            CompiledScript parseResult;
            try
            {
                parseResult = ModMain.ScriptManager.ScriptExecutor.Compile(text);
            }
            catch (Exception ex)
            {
                new SearchResponse("failed to parse search (Code: {0})", ex, SearchResponse.Type.ParserError).PrintSearchError();
                return;
            }

            SearchPatch.tagGroups = parseResult;
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
            //DataHelper.hides.Clear();

            SearchPatch.isAdvancedSearch = true;
            //MelonLogger.Msg("Parsed tags: $" + string.Join(" ", SearchPatch.tagGroups.Select(x1 => string.Join("|", x1.Select(x2 => TermToString(x2))))) + '$');
        }

        private static void NullifyAdvancedSearch()
        {
            SearchPatch.tagGroups = null!;
        }
    }
}