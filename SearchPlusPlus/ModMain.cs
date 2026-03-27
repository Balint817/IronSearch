using System.Collections.ObjectModel;
using System.Net;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppAssets.Scripts.UI.Controls;
using IronPython.Runtime;
using IronSearch.Patches;
using IronSearch.Tags;
using IronSearch.UI;
using MelonLoader;
using MelonLoader.Utils;
using PythonExpressionManager;

namespace IronSearch
{
    public partial class ModMain : MelonMod
    {
        static bool? _initStepTracker = null;
        public static bool InitSuccessful { get; internal set; } = false;
        public static bool CustomAlbumsLoaded { get; private set; }
        public static bool HeadquartersLoaded { get; private set; }
        public static bool PopupLibLoaded { get; private set; }

        private static bool ShownError = false;

        internal static Dictionary<string, bool> _hqChartDict = new();
        public static ReadOnlyDictionary<string, bool> HQChartDict => new(_hqChartDict);

        internal static MelonPreferences_Entry<Dictionary<string, string>> expressionEntry = null!;

        internal static MelonPreferences_Entry<Dictionary<string, string>> aliasEntry = null!;

        internal static MelonPreferences_Entry<Dictionary<string, string>> autoCompleteItems = null!;

        internal static MelonPreferences_Entry<double> waitMultiplierEntry = null!;

        internal static MelonPreferences_Entry<bool> enableHQSpam = null!;

        internal static MelonPreferences_Entry<string> startSearchStringEntry = null!;
        internal static string StartString
        {
            get
            {
                return startSearchStringEntry.Value ?? string.Empty;
            }
        }
        internal static bool EnableHQSpam
        {
            get
            {
                return enableHQSpam.Value;
            }
        }
        internal static Dictionary<string,string> Aliases
        {
            get
            {
                return aliasEntry.Value;
            }
        }
        internal static Dictionary<string, string> AutoCompleteItems
        {
            get
            {
                return autoCompleteItems.Value;
            }
        }
        internal static Dictionary<string, string> ExpressionEntry
        {
            get
            {
                return expressionEntry.Value;
            }
        }
        public override void OnApplicationQuit()
        {
            startSearchStringEntry.Category.SaveToFile(false);
            HQLoader.CreateBackupSync(_hqChartDict);
            AudioHelper.SaveCache();
        }
        public override void OnPreferencesLoaded()
        {
            if (!InitSuccessful)
            {
                return;
            }
            //MelonLogger.Msg(System.ConsoleColor.Magenta, "Re-loading aliases...");
            //LoadAliases();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Welcome")
            {
                if (!InitSuccessful && !ShownError)
                {
                    var s = "The mod failed basic initialization and has disabled itself.";
                    MelonLogger.Msg(System.ConsoleColor.DarkRed, s);
                    ShownError = true;
                    ShowText.ShowInfo(s);
                }
            }
            else if (sceneName == "UISystem_PC")
            {
                UISystemLoaded = true;
            }
            else
            {
                // ...
            }
            SearchFocusPatch.inputField = null;
        }

        public override void OnUpdate()
        {
            if (InitSuccessful)
            {
                AutoCompleteManager.Update();
            }
        }

        internal static bool UISystemLoaded { get; private set; }

        internal static bool IsFirstLengthCacheBuild { get; private set; } = true;
        internal static void BuildCacheIfNecessary()
        {
            if (InitSuccessful && IsFirstLengthCacheBuild)
            {
                IsFirstLengthCacheBuild = false;
                if (AudioHelper.VanillaCache?.IsEmpty ?? true)
                {
                    var s = "Re-building length cache, this may take a while!";
                    MelonLogger.Msg(System.ConsoleColor.Magenta, s);
                }
                AudioHelper.ForceBuildVanillaCache();
            }
        }

        static void LoadCinema()
        {
            try
            {
                MelonLogger.Msg("Checking charts for cinemas, this shouldn't take long...");
                BuiltIns.hasCinema = AlbumManager.LoadedAlbums.Values.Where(x => Utils.TryParseCinemaJson(x)).Select(x => x.Uid).ToHashSet();
                MelonLogger.Msg("Cinema tag initialized");
                BuiltIns.lastCheckedCinema = DateTime.UtcNow;
            }
            catch (Exception ex)
            {

                MelonLogger.Msg(System.ConsoleColor.Red, ex.ToString());
                MelonLogger.Msg(System.ConsoleColor.Yellow, "If you're seeing this, then I have absolutely 0 clue how. Either way, the cinema tag won't work. (e.g. please report lmao)");
            }
        }

        Task<Dictionary<string, bool>> HQLoadTask = null!;
        CancellationTokenSource cts = new();

        public override void OnEarlyInitializeMelon()
        {
            _initStepTracker = false;
            HQLoadTask = HQLoader.LoadHQ(cts.Token);
            var e = (MelonEvent)HarmonyLib.AccessTools.Field(typeof(MelonEvents), nameof(MelonHarmonyInit)).GetValue(null)!;
            e.Subscribe(MelonHarmonyInit);
            _initStepTracker = null;
        }

        void MelonHarmonyInit()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            _initStepTracker = false;
            CustomAlbumsLoaded = Utils.IsAssemblyLoaded("CustomAlbums");
            HeadquartersLoaded = Utils.IsAssemblyLoaded("Headquarters");
            PopupLibLoaded = Utils.IsAssemblyLoaded("PopupLib");
            if (!PopupLibLoaded)
            {
                MelonLogger.Msg(System.ConsoleColor.Magenta, "PopupLib and/or KeybindManager is missing, certain features of the mod will be disabled.");
            }
            if (!CustomAlbumsLoaded)
            {
                MelonLogger.Msg(System.ConsoleColor.Magenta, "CustomAlbums is missing, certain features of the mod will be disabled.");
                try
                {
                    cts.Cancel();
                }
                catch (Exception)
                {
                    // catch silently
                }
            }
            if (HeadquartersLoaded)
            {
                HeadquartersPatch.RunPatch(this.HarmonyInstance);
            }

            var category = MelonPreferences.CreateCategory("IronSearch");
            category.SetFilePath("UserData/IronSearch.cfg");

            enableHQSpam = category.CreateEntry<bool>("EnableHQSpam", false, "EnableHQSpam", "\nEnables searching for uploaded & ranked custom charts,\nbut unfortunately requires spamming the server.\nA fast connection is recommended.");
            waitMultiplierEntry = category.CreateEntry<double>("WaitMultiplier", 2.5, "WaitMultiplier", "\nIncreases the amount of time that must pass after search text changes before the search is refreshed.\nThe multiplier affects ONLY advanced searches, normal searches are unaffected.");
            startSearchStringEntry = category.CreateEntry<string>("StartSearchText", "search:", "StartSearchText", "\nThe text that your search needs to start with in order for this mod to be enabled.\nMay be left empty if you want the mod to always use advanced search.\nFor obvious reasons, this is not a good idea.");

            if (startSearchStringEntry.Value == null)
            {
                startSearchStringEntry.Value = startSearchStringEntry.DefaultValue;
            }


            ServicePointManager.DefaultConnectionLimit = 100;
            WebRequest.DefaultWebProxy = null;



            LoadUserScripts();
            _initStepTracker = null;
        }



        const string scriptFolderName = "Scripts";
        const string exampleScriptName = "Unpacked.py";
        public static string ArgumentName => "M";
        public static string BaseDictName => "T";
        public static string ScriptDirectory => Path.Join(MelonEnvironment.UserDataDirectory, scriptFolderName);
        public static string ExampleScriptFilePath => Path.Join(ScriptDirectory, exampleScriptName);
        public static UserScriptManager ScriptManager { get; private set; } = null!;
        public static float WaitMultiplierFloat
        {
            get
            {
                return (float)WaitMultiplier;
            }
        }
        public static double WaitMultiplier
        {
            get
            {
                return waitMultiplierEntry.Value;
            }
        }
        public static ReadOnlyDictionary<string, string> HelpStrings => new(_helpString);

        internal static readonly Dictionary<string, string> _helpString = new();

        void RegisterScript(string key, BuiltInDelegate del)
        {
            ScriptManager.ScriptExecutor.RegisterScript(key, ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(del)));
        }
        void RegisterObject(string key, BuiltInObjectDelegate del)
        {
            ScriptManager.ScriptExecutor.RegisterScript(key, ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(del)));
        }

        public static void RegisterHelp(List<string> keys, string helpString)
        {
            if (keys is null || keys.Count is 0)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Count > 1)
            {
                helpString = $"Alternatives: {string.Join(", ", keys)}\n" + helpString;
            }
            foreach (var key in keys)
            {
                _helpString.Add(key, helpString);
            }
        }

        private void LoadUserScripts()
        {
            var category = startSearchStringEntry.Category;
            MelonLogger.Msg("Initalizing UserScriptManager...");

            MelonLogger.Msg("Checking user script directory...");

            Directory.CreateDirectory(ScriptDirectory);
            //var templateString =
            //    $"def {Script.OutputFunctionName}(M, T):\n" +
            //    $"\treturn T['Custom']() and not T['Packed']()";
            var templateString =
                $"def {Script.OutputFunctionName}(M, T):\n" +
                $"\treturn T['Custom'](M,T) and not T['Packed'](M, T)";
            File.WriteAllText(ExampleScriptFilePath, templateString);

            ScriptManager = new(ScriptDirectory, new(new ScriptMelonLogger(), ArgumentName, BaseDictName), (int)Priorities.UserScript);

            MelonLogger.Msg("Loading built-ins...");

            ScriptManager.DefaultPriority = (int)Priorities.BuiltIn;

            RegisterScript("Accuracy", BuiltIns.EvalAccuracy);
            RegisterScript("Acc", BuiltIns.EvalAccuracy);
            RegisterHelp(new() { "Accuracy", "Acc" },
                "Usage: Accuracy(accuracyRange) or Accuracy(accuracyRange, levelRange)\n\n"
                + "Checks if the music has a score in the specified accuracy range, and optionally, specify which difficulties to match as well.\n"
                + "The level range can be a range from 1-5, specifying which levels are matched, or the wildcard '?' to select the highest level."
            );


            RegisterScript("Album", BuiltIns.EvalAlbum);
            RegisterHelp(new() { "Album" },
                "Usage: Album(albumName) or Album(regex)\n\n"
                + "Checks if the music belongs to an album with the specified name.\n"
                + "Album names are determined by the game and may differ from what you see in-game."
            );


            RegisterScript("Any", BuiltIns.EvalAny);
            RegisterHelp(new() { "Any" },
                "Usage: Any(text) or Any(regex)\n\n"
                + "Checks if the music matches any of the following tags with the given input:\n"
                + "- Album\n"
                + "- Author\n"
                + "- Designer\n"
                + "- Tag\n"
                + "- Title\n"
            );

            RegisterScript("AP", BuiltIns.EvalAP);
            RegisterScript("AllPerfect", BuiltIns.EvalAP);
            RegisterHelp(new() { "AP", "AllPerfect" },
                "Usage: AP() or AP(difficultyRange)\n\n"
                + "Checks if the music has an all perfect in the specified difficulty range.\n"
                + "The difficulty range can be a range from 1-5, specifying which difficulties are matched, or the wildcard '?' to select the highest difficulty."
            );

            RegisterScript("Author", BuiltIns.EvalAuthor);
            RegisterHelp(new() { "Author" },
                "Usage: Author(authorName) or Author(regex)\n\n"
                + "Checks if the music's author matches the specified name.\n"
                );

            RegisterScript("BPM", BuiltIns.EvalBPM);
            RegisterHelp(new() { "BPM" },
                "Usage: BPM(bpmRange)\n\n"
                + "Checks if the music's BPM is in the specified range."
            );

            RegisterScript("Callback", BuiltIns.EvalCallback);
            RegisterHelp(new() { "Callback" },
                "Usage: Callback(callbackRange) or Callback(callbackRange, levelRange)\n\n"
                + "Checks if the music has the specified callback difficulty.\n"
                + "'Callback difficulty' refers to the difficulty that's actually sent to the game servers, rather than displayed.\n"
                + "Unlike normal difficulty, it is always an integer, and cannot be '?', 'E', etc.\n"
                + "The level range can be a range from 1-5, specifying which levels are matched, or the wildcard '?' to select the highest level."
            );

            RegisterScript("Cinema", BuiltIns.EvalCinema);
            RegisterScript("Video", BuiltIns.EvalCinema);
            RegisterHelp(new() { "Cinema", "Video" },
                "Usage: Cinema()\n\n"
                + "Checks if the music is a custom chart that has a video background."
            );

            RegisterScript("Custom", BuiltIns.EvalCustom);
            RegisterHelp(new() { "Custom" },
                "Usage: Custom()\n\n"
                + "Checks if the music is a custom chart."
            );

            RegisterScript("Designer", BuiltIns.EvalDesigner);
            RegisterScript("Design", BuiltIns.EvalDesigner);
            RegisterScript("LevelDesign", BuiltIns.EvalDesigner);
            RegisterScript("LevelDesigner", BuiltIns.EvalDesigner);
            RegisterHelp(new() { "Designer", "Design", "LevelDesign", "LevelDesigner" },
                "Usage: Designer(designerName) or Designer(regex)\n\n"
                + "Checks if the chart's level designer matches the specified name.\n"
                );

            RegisterScript("Difficulty", BuiltIns.EvalDifficulty);
            RegisterScript("Diff", BuiltIns.EvalDifficulty);
            RegisterHelp(new() { "Difficulty", "Diff" },
                "Usage: Difficulty(difficultyRange, levelRange)\n\n"
                + "Checks if the music has a difficulty in the specified range, and optionally specify which levels to match.\n"
                + "The level range can be a range from 1-5, specifying which levels are matched, or the wildcard '?' to select the highest level."
            );

            RegisterScript("Favorite", BuiltIns.EvalFavorite);
            RegisterScript("Fav", BuiltIns.EvalFavorite);
            RegisterHelp(new() { "Favorite", "Fav" },
                "Usage: Favorite()\n\n"
                + "Checks if the music is in your favorites."
            );

            RegisterScript("FullCombo", BuiltIns.EvalFC);
            RegisterScript("FC", BuiltIns.EvalFC);
            RegisterHelp(new() { "FullCombo", "FC" },
                "Usage: FullCombo() or FullCombo(levelRange)\n\n"
                + "Checks if the music has a full combo in the specified level range.\n"
                + "The level range can be a range from 1-5, specifying which levels are matched, or the wildcard '?' to select the highest level."
            );

            RegisterScript("Hidden", BuiltIns.EvalHasHidden);
            RegisterHelp(new() { "Hidden" },
                "Usage: Hidden()\n\n"
                + "Checks if the music has a hidden difficulty"
            );

            RegisterScript("Touhou", BuiltIns.EvalHasTouhou);
            RegisterHelp(new() { "Touhou" },
                "Usage: Touhou()\n\n"
                + "Checks if the music has a Touhou difficulty"
            );

            RegisterScript("History", BuiltIns.EvalHistory);
            RegisterHelp(new() { "History" },
                "Usage: History()\n\n"
                + "Checks if the music is in your chart history (last played charts)"
            );

            RegisterScript("Length", BuiltIns.EvalLength);
            RegisterHelp(new() { "Length" },
                "Usage: Length(lengthRange)\n\n"
                + "Checks if the music's length is in the specified range.\n"
                + "This can be a normal range (in seconds), or time formatted like '1m30s'."
            );

            //RegisterScript("Hide", BuiltIns.EvalHide);

            RegisterScript("New", BuiltIns.EvalNew);
            RegisterHelp(new() { "New" },
                "Usage: New(topRange)\n\n"
                + "Checks if the music is the Nth last added custom chart."
            );

            RegisterScript("Old", BuiltIns.EvalOld);
            RegisterHelp(new() { "Old" },
                "Usage: Old(bottomRange)\n\n"
                + "Checks if the music is the Nth earliest added custom chart."
            );

            RegisterScript("Online", BuiltIns.EvalOnline);
            RegisterHelp(new() { "Online" },
                "Usage: Online()\n\n"
                + "Checks if the music is an online chart (custom from the website, can be ranked or unranked).\n"
                + "DISCLAIMER, this tag will not work if 'EnableHQSpam' is disabled, since querying HQ is required to access this information."
            );

            RegisterScript("Packed", BuiltIns.EvalPacked);
            RegisterHelp(new() { "Packed" },
                "Usage: Packed()\n\n"
                + "Checks if the music is a packed custom chart. (e.g., it's a '.mdm' file, and not a folder)"
            );

            RegisterScript("Ranked", BuiltIns.EvalRanked);
            RegisterHelp(new() { "Ranked" },
                "Usage: Ranked()\n\n"
                + "Checks if the music is a ranked custom chart.\n"
                + "DISCLAIMER, this tag will not work if 'EnableHQSpam' is disabled, since querying HQ is required to access this information.\n"
                + "Additionally, due to technical reasons, ranking information may not update instantly if a chart has been ranked AFTER this setting was enabled."
            );

            RegisterScript("Scene", BuiltIns.EvalScene);
            RegisterHelp(new() { "Scene" },
                "Usage: Scene(sceneName) or Scene(sceneIndex)\n\n"
                + "Checks if the music is in a scene with the specified name or index.\n"
                + "For example, 'candyland', 'castle', etc.\n"
                + "If the match is ambiguous, (for example typing 'r' and matching 'rainynight' and 'retrocity'), the search fails."
            );

            RegisterScript("Modified", BuiltIns.EvalModified);
            RegisterHelp(new() { "Modified" },
                "Usage: Modified(timeRange) or Modified(timeTicks)\n\n"
                + "Checks if the music was last modified in the specified time range.\n"
                + "The time range can be something like '7d' for 7 days, '24h' for 24 hours, etc., or given as time ticks, for example, Modified(Days(7))"
            );

            RegisterScript("Streamer", BuiltIns.EvalStreamer);
            RegisterHelp(new() { "Streamer" },
                "Usage: Streamer()\n\n"
                + "Checks if the music is in the streamer list (e.g. copyright-safe music.)\n"
                + "Customs do not support this feature."
            );

            RegisterScript("Tag", BuiltIns.EvalTag);
            RegisterHelp(new() { "Tag" },
                "Usage: Tag(tagName) or Tag(regex)\n\n"
                + "Checks if the music has a tag that matches the specified name or regex."
            );

            RegisterScript("Title", BuiltIns.EvalTitle);
            RegisterHelp(new() { "Title" },
                "Usage: Title(title) or Title(regex)\n\n"
                + "Checks if the music's title matches the specified name or regex."
            );

            RegisterScript("Unplayed", BuiltIns.EvalUnplayed);
            RegisterHelp(new() { "Unplayed" },
                "Usage: Unplayed() or Unplayed(levelRange)\n\n"
                + "Checks if the music is unplayed (no score on any difficulty).\n"
                + "Alternatively, checks if the music hasn't been played on the specified levels.\n"
                + "The '?' wildcard for the highest difficulty is supported."
            );




            RegisterObject("Days", BuiltIns.EvalDays);
            RegisterHelp(new() { "Days" },
                "Usage: Days(dayRange)\n\n"
                + "Returns a time range (in ticks) representing the specified number of days.\n"
                + "For example, Days(7) would represent the time range of the last 7 days."
            );

            RegisterObject("EmptyMultiRange", BuiltIns.EvalEmptyMultiRange);
            RegisterObject("EMR", BuiltIns.EvalEmptyMultiRange);
            RegisterHelp(new() { "EmptyMultiRange", "EMR" },
                "Usage: EmptyMultiRange()\n\n"
                + "Returns an empty multi-range, which matches nothing."
            );

            RegisterObject("FullRange", BuiltIns.EvalFullRange);
            RegisterObject("FR", BuiltIns.EvalFullRange);
            RegisterHelp(new() { "FullRange", "FR" },
                "Usage: FullRange()\n\n"
                + "Returns a full range, which matches everything. Equivalent to the wildcard '*'"
            );

            RegisterObject("FullMultiRange", BuiltIns.EvalFullMultiRange);
            RegisterObject("FMR", BuiltIns.EvalFullMultiRange);
            RegisterHelp(new() { "FullMultiRange", "FMR" },
                "Usage: FullMultiRange()\n\n"
                + "Returns a full multi-range, which matches everything. Equivalent to the wildcard '*'"
            );

            RegisterObject("GetBPM", BuiltIns.EvalGetBPM);
            RegisterHelp(new() { "GetBPM" },
                "Usage: GetBPM()\n\n"
                + "Returns the BPM of the song as a Range (or None if BPM parsing fails)"
            );
            RegisterObject("GetModified", BuiltIns.EvalGetModified);
            RegisterHelp(new() { "GetModified" },
                "Usage: GetModified()\n\n"
                + "Returns the last-modified time of the song (or None if the request is not valid)."
            );

            RegisterObject("GetCallbacks", BuiltIns.EvalGetCallbacks);
            RegisterObject("Callbacks", BuiltIns.EvalGetCallbacks);
            RegisterHelp(new() { "GetCallbacks", "Callbacks" },
                "Usage: GetCallbacks() or Callbacks()\n\n"
                + "Returns a list of the music's callback difficulties."
            );

            RegisterObject("GetDifficulties", BuiltIns.EvalGetDifficulties);
            RegisterObject("Difficulties", BuiltIns.EvalGetDifficulties);
            RegisterObject("Diffs", BuiltIns.EvalGetDifficulties);
            RegisterHelp(new() { "GetDifficulties", "Difficulties", "Diffs" },
                "Usage: GetDifficulties() or Difficulties() or Diffs()\n\n"
                + "Returns a list of the music's difficulties."
            );

            RegisterObject("GetHighscores", BuiltIns.EvalGetHighscores);
            RegisterObject("GetHighScores", BuiltIns.EvalGetHighscores);
            RegisterObject("Highscores", BuiltIns.EvalGetHighscores);
            RegisterObject("HighScores", BuiltIns.EvalGetHighscores);
            RegisterObject("Highs", BuiltIns.EvalGetHighscores);
            RegisterHelp(new() { "GetHighscores", "GetHighScores", "Highscores", "HighScores", "Highs" },
                "Usage: GetHighscores() or GetHighScores() or Highscores() or HighScores() or Highs()\n\n"
                + "Returns a list of the music's highscores."
            );

            RegisterObject("GetLanguage", BuiltIns.EvalGetLanguageIndex);
            RegisterObject("Language", BuiltIns.EvalGetLanguageIndex);
            RegisterHelp(new() { "GetLanguage", "Language" },
                "Usage: GetLanguage() or Language()\n\n"
                + "Returns the music's language as an index (0 for Japanese, 1 for English, etc.)"
            );

            RegisterObject("GetLength", BuiltIns.EvalGetLength);
            RegisterHelp(new() { "GetLength" },
                "Usage: GetLength() or Length()\n\n"
                + "Returns the music's length in seconds."
            );

            RegisterObject("Hours", BuiltIns.EvalHours);
            RegisterHelp(new() { "Hours" },
                "Usage: Hours(hourRange)\n\n"
                + "Returns a time range (in ticks) representing the specified number of hours.\n"
                + "For example, Hours(24) would represent the time range of the last 24 hours."
            );

            RegisterObject("InvalidMultiRange", BuiltIns.EvalInvalidMultiRange);
            RegisterObject("IMR", BuiltIns.EvalInvalidMultiRange);
            RegisterHelp(new() { "InvalidMultiRange", "IMR" },
                "Usage: InvalidMultiRange()\n\n"
                + "Returns an invalid multi-range, which matches nothing.\n"
                + "Equivalent to the '?' wildcard. Just like the '?' wildcard, not useable everywhere."
            );

            RegisterObject("InvalidRange", BuiltIns.EvalInvalidRange);
            RegisterObject("IR", BuiltIns.EvalInvalidRange);
            RegisterHelp(new() { "InvalidRange", "IR" },
                "Usage: InvalidRange()\n\n"
                + "Returns an invalid range, which matches nothing.\n"
                + "Equivalent to the '?' wildcard. Just like the ',' wildcard, not useable everywhere."
            );

            RegisterObject("Minutes", BuiltIns.EvalMinutes);
            RegisterHelp(new() { "Minutes" },
                "Usage: Minutes(minuteRange)\n\n"
                + "Returns a time range (in ticks) representing the specified number of minutes.\n"
                + "For example, Minutes(90) would represent the time range of the last 90 minutes."
            );

            RegisterObject("MultiRange", BuiltIns.EvalMultiRange);
            RegisterObject("MR", BuiltIns.EvalMultiRange);
            RegisterHelp(new() { "MultiRange", "MR" },
                "Usage: MultiRange(range1, range2, ...)\n\n"
                + "Returns a multi-range that matches if any of the given ranges match. The ranges can be range strings.\n"
                + "For example, MultiRange('0-3', '5-7') would match if the value is between 0 and 3, or between 5 and 7.\n"
                + "The range strings can be in the following formats:\n"
                + "- 'A-B' for a range from A to B (inclusive)\n"
                + "- '*' for a wildcard that matches everything\n"
                + "- '?' for an invalid range that matches nothing\n"
                + "- 'A+' for a range from A to infinity\n"
                + "- 'A-' for a range from A to negative infinity\n"
                + "- '|A-B' to make the A side exclusive\n"
                + "- 'A-B|' to make the B side exclusive\n"
                + "This function also allows you to do the following for multiple ranges: MultiRange('0-1 5-7')"
            );

            RegisterObject("Random", BuiltIns.EvalRandom);
            RegisterHelp(new() { "Random" },
                "Usage: Random() or Random(start, end)\n\n"
                + "Random() returns a random real number between 0.0 and 1.0,\n"
                + "Random(start, end) returns a random integer in the range 'start-end|'\n"
            );

            RegisterObject("Range", BuiltIns.EvalRange);
            RegisterObject("R", BuiltIns.EvalRange);
            // help string should be based on the range parsing in Utils.ParseRange
            RegisterHelp(new() { "Range", "R" },
                "Usage: Range(rangeString)\n\n"
                + "Parses a range string and returns a range object that can be used in other functions.\n"
                + "The range string can be in the following formats:\n"
                + "- 'A-B' for a range from A to B (inclusive)\n"
                + "- '*' for a wildcard that matches everything\n"
                + "- '?' for an invalid range that matches nothing\n"
                + "- 'A+' for a range from A to infinity\n"
                + "- 'A-' for a range from A to negative infinity\n"
                + "- '|A-B' to make the A side exclusive\n"
                + "- 'A-B|' to make the B side exclusive"
            );

            RegisterObject("Regex", BuiltIns.EvalRegex);
            RegisterObject("Re", BuiltIns.EvalRegex);
            RegisterHelp(new() { "Regex", "Re" },
                "Usage: Regex(pattern)\n\n"
                + "Parses a regex pattern and returns a regex object that can be used in other functions.\n"
                + "The regex syntax is the same as Python's regex syntax.\n"
                + "Teaching regex syntax is out of scope for this help string. If you don't know how to use it, don't worry, you probably won't need it."
            );

            RegisterObject("Weeks", BuiltIns.EvalWeeks);
            RegisterHelp(new() { "Weeks" },
                "Usage: Weeks(weekRange)\n\n"
                + "Returns a time range (in ticks) representing the specified number of weeks.\n"
                + "For example, Weeks(2) would represent the time range of the last 2 weeks."
            );




            RegisterObject("DefineGlobal", BuiltIns.EvalDefineGlobalVar);
            RegisterObject("DG", BuiltIns.EvalDefineGlobalVar);
            RegisterHelp(new() { "DefineGlobal", "DG" },
                "Usage: DefineGlobal(varName, value)\n\n"
                + "If the global variable doesn't already exist, initializes a global variable with the specified name and value that can be passed between songs.\n"
            );

            RegisterObject("DefineVar", BuiltIns.EvalDefineVar);
            RegisterObject("DV", BuiltIns.EvalDefineVar);
            RegisterHelp(new() { "DefineVar", "DV" },
                "Usage: DefineVar(varName, value)\n\n"
                + "If the variable doesn't already exist, initializes a variable with the specified name and value that can be accessed in the current song's filter.\n"
            );

            RegisterObject("GetGlobal", BuiltIns.EvalGetGlobalVar);
            RegisterObject("GG", BuiltIns.EvalGetGlobalVar);
            RegisterObject("LoadGlobal", BuiltIns.EvalGetGlobalVar);
            RegisterObject("LG", BuiltIns.EvalGetGlobalVar);
            RegisterHelp(new() { "GetGlobal", "GG", "LoadGlobal", "LG" },
                "Usage: GetGlobal(varName)\n\n"
                + "Returns the value of the global variable with the specified name that was defined with DefineGlobal or SetGlobal.\n"
                + "The search fails if an undefined global variable is accessed."
            );

            RegisterObject("GetVar", BuiltIns.EvalGetVar);
            RegisterObject("GV", BuiltIns.EvalGetVar);
            RegisterObject("LoadVar", BuiltIns.EvalGetVar);
            RegisterObject("LV", BuiltIns.EvalGetVar);
            RegisterHelp(new() { "GetVar", "GV", "LoadVar", "LV" },
                "Usage: GetVar(varName)\n\n"
                + "Returns the value of the variable with the specified name for the current song's filter.\n"
                + "The variable must have been created with either SetVar or DefineVar in the current song's filter\n"
                + "The search fails if an undefined variable is accessed."
            );

            RegisterObject("SetGlobal", BuiltIns.EvalSetGlobalVar);
            RegisterObject("SG", BuiltIns.EvalSetGlobalVar);
            RegisterHelp(new() { "SetGlobal", "SG" },
                "Usage: SetGlobal(varName, value)\n\n"
                + "Sets the value of the global variable with the specified name to the given value that can be passed between songs.\n"
                + "If the variable doesn't exist, it is created. If it already exists, it's value is overwritten.\n"
            );

            RegisterObject("SetVar", BuiltIns.EvalSetVar);
            RegisterObject("SV", BuiltIns.EvalSetVar);
            RegisterHelp(new() { "SetVar", "SV" },
                "Usage: SetVar(varName, value)\n\n"
                + "Sets the value of the variable with the specified name to the given value for the current song's filter.\n"
                + "If the variable doesn't exist, it is created. If it already exists, it's value is overwritten.\n"
            );



            RegisterScript("Exit", BuiltIns.EvalExit);
            // short-circuits the search and returns the specified boolean value.
            RegisterHelp(new() { "Exit" },
                "Usage: Exit(returnValue)\n\n"
                + "Exits the search and returns the specified boolean value.\n"
                + "For example, Exit(True) would make the search succeed instantly if it is executed, while Exit(False) would make it instantly fail."
            );

            RegisterScript("RunOnce", BuiltIns.EvalRunOnce);
            RegisterHelp(new() { "RunOnce" },
                "Usage: RunOnce(function, id)\n\n"
                + "Runs the specified script only the first time it is encountered in a search, and returns True. Every subsequent time it is encountered, it returns False.\n"
                + "For example, if you want to check if a song has a difficulty above 3, but only want to check it once for performance reasons, you could do: RunOnce(Difficulty('3+')) and then check for the difficulty in the rest of the search."
            );

            RegisterScript("Sorter", BuiltIns.EvalSorter);
            RegisterScript("Sort", BuiltIns.EvalSorter);
            RegisterHelp(new() { "Sorter", "Sort" },
                "Usage: Sorter(comparerFunction)\n\n"
                + "Sorts the search results based on the specified comparer function.\n"
                + "The comparer function is a function that takes the music as input and returns a value that is used for sorting.\n"
                + "For example, Sorter(ByLength) would sort the search results based on the chart length.\n"
                + "To create your own comparer, pass a function that takes 2 arguments, let's call them A and B, and returns:\n"
                + "- if A is smaller, -1\n"
                + "- if A equals B, 0\n"
                + "- if A is greater, 1\n"
            );

            RegisterScript("Log", BuiltIns.EvalLog);
            RegisterHelp(new() { "Log" },
                "Usage: Log(value)\n\n"
                + "purposes.\n"
                + "Logs the specified value to the console for debugging purposes.\n"
                + "For example, Log(M.name) would log the title of each music in the search results.\n"
                + "DISCLAIMER: It's preferred to use LogOnce (or in certain cases LogUnique) instead of Log due to practical and technical issues.\n"
                + "This should be used with caution as logging too much at once may not only flood your console (not very useful),\n"
                + "but this may also overload MelonLoader's console and crash your game."
            );

            RegisterScript("LogOnce", BuiltIns.EvalLogOnce);
            RegisterHelp(new() { "LogOnce" },
                "Usage: LogOnce(value, id='someId')\n\n"
                + "Logs the specified value to the console for debugging purposes, but only the first time it's encountered with the specified id."
            );

            RegisterScript("LogUnique", BuiltIns.EvalLogUnique);
            RegisterHelp(new() { "LogUnique" },
                "Usage: LogUnique(value)\n\n"
                + "Logs the specified value to the console for debugging purposes, but only once for each unique value.\n"
                + "For example, LogUnique(M.author) would log the name of each unique author in the search results,\n"
                + "but if there are multiple songs by the same author, their name would only be logged once."
            );




            RegisterObject("ByAccuracy", BuiltIns.EvalByAccuracy);
            RegisterObject("ByAcc", BuiltIns.EvalByAccuracy);
            RegisterHelp(new() { "ByAccuracy", "ByAcc" },
                "Usage: ByAccuracy()\n\n"
                + "Returns a key function that can be used in Sorter to sort by accuracy."
            );

            RegisterObject("ByBPM", BuiltIns.EvalByBPM);
            RegisterHelp(new() { "ByBPM" },
                "Usage: ByBPM()\n\n"
                + "Returns a key function that can be used in Sorter to sort by BPM."
            );

            RegisterObject("ByDifficulty", BuiltIns.EvalByDifficulty);
            RegisterObject("ByDiff", BuiltIns.EvalByDifficulty);
            RegisterHelp(new() { "ByDifficulty", "ByDiff" },
                "Usage: ByDifficulty()\n\n"
                + "Returns a key function that can be used in Sorter to sort by difficulty."
            );

            RegisterObject("ByLength", BuiltIns.EvalByLength);
            RegisterHelp(new() { "ByLength" },
                "Usage: ByLength()\n\n"
                + "Returns a key function that can be used in Sorter to sort by length."
            );

            RegisterObject("ByName", BuiltIns.EvalByName);
            RegisterHelp(new() { "ByName" },
                "Usage: ByName()\n\n"
                + "Returns a key function that can be used in Sorter to sort by the music's title."
            );

            RegisterObject("ByRandom", BuiltIns.EvalByRandom);
            RegisterHelp(new() { "ByRandom" },
                "Usage: ByRandom()\n\n"
                + "Returns a key function that can be used in Sorter to sort randomly."
            );

            RegisterObject("ByModified", BuiltIns.EvalByModified);
            RegisterHelp(new() { "ByModified" },
                "Usage: ByModified()\n\n"
                + "Returns a key function that can be used in Sorter to sort by last modified time."
            );

            RegisterObject("ByScene", BuiltIns.EvalByScene);
            RegisterHelp(new() { "ByScene" },
                "Usage: ByScene()\n\n"
                + "Returns a key function that can be used in Sorter to sort by scene."
            );

            RegisterObject("ByUID", BuiltIns.EvalByUID);
            RegisterHelp(new() { "ByUID" },
                "Usage: ByUID()\n\n"
                + "Returns a key function that can be used in Sorter to sort by the music's UID."
            );


            ScriptManager.DefaultPriority = (int)Priorities.Expression;

            MelonLogger.Msg("Loading expressions...");
            expressionEntry = category.CreateEntry<Dictionary<string, string>>("Expressions", new()
            {
                ["NewCustom"] = "Unplayed() and Custom()",

            }, "Expressions", "\nDefine shorthands for searches here.");
            LoadExpressions();



            ScriptManager.DefaultPriority = (int)Priorities.Alias;

            MelonLogger.Msg("Loading aliases...");
            aliasEntry = category.CreateEntry<Dictionary<string, string>>("TagAliases", new()
            {
                ["Perfect"]= "AllPerfect"
            }, "TagAliases", "\nDefine aliases for existing tags here.");
            LoadAliases();


            autoCompleteItems = category.CreateEntry<Dictionary<string, string>>("AutoCompleteItems", new() {
                ["Vanilla"]="not Custom()"
            }, "AutoCompleteItems", "\nDefine alternative keywords for auto-complete here.");
        }

        static readonly Dictionary<string, Script> LoadedExpressions = new();
        private static void LoadExpressions()
        {
            try
            {
                if (ExpressionEntry is null)
                {
                    return;
                }
                foreach (var kv in ExpressionEntry)
                {
                    try
                    {
                        var key = kv.Key;
                        var script = ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(LoadExpression(kv.Value)));
                        LoadedExpressions.Add(key, script);
                        ScriptManager.ScriptExecutor.RegisterScript(key, script);
                        AutoCompleteManager.AllKeywords.Add(key, new($"{key}()", 0));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Msg(System.ConsoleColor.Red, ex);
                        MelonLogger.Msg(System.ConsoleColor.Magenta, $"An error occured while loading the expression: '{kv.Key}'");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(System.ConsoleColor.Red, ex);
                MelonLogger.Msg(System.ConsoleColor.Magenta, $"An error occured while loading the expressions.");
            }
        }
        private static ExpressionDelegate LoadExpression(string expression)
        {
            var compiled = ScriptManager.ScriptExecutor.Compile(expression);
            ExpressionDelegate baseDel = (SearchArgument M, PythonTuple varArgs, PythonDictionary varKwargs) =>
            {
                var wrappedArgs = new ExpressionSearchArgument(M, varArgs, varKwargs);

                return ScriptManager.ScriptExecutor.EvaluateObject(wrappedArgs, compiled);
            };
            return baseDel;
        }

        private void LoadAliases()
        {
            try
            {
                var aliases = Aliases;
                if (aliases is null)
                {
                    return;
                }
                var executor = ScriptManager.ScriptExecutor;
                foreach (var item in aliases)
                {
                    try
                    {
                        var key = item.Key;
                        var value = item.Value;
                        executor.RegisterAlias(key, value);
                        AutoCompleteManager.AllKeywords.Add(key, new($"{key}()", 0));
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Msg(System.ConsoleColor.Red, ex);
                        MelonLogger.Msg(System.ConsoleColor.Magenta, $"An error occured while loading the alias: '{item.Key}'");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(System.ConsoleColor.Red, ex);
                MelonLogger.Msg(System.ConsoleColor.Magenta, $"An error occured while loading aliases.");
            }
        }

        public override void OnInitializeMelon()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            _initStepTracker = false;
            AudioHelper.LoadVanillaCache();
            _initStepTracker = null;
        }

        internal static void LoadAlbumNames()
        {
            var localAlbums = Singleton<ConfigManager>.instance.GetConfigObject<DBConfigAlbums>(0).m_LocalDic.Values;

            var baseAlbums = Singleton<ConfigManager>.instance.GetConfigObject<DBConfigAlbums>(0).list;

            var t = new Dictionary<int, HashSet<string>>();

            foreach (var localAlbum in localAlbums)
            {
                for (int i = 0; i < baseAlbums.Count; i++)
                {
                    if (!t.TryGetValue(baseAlbums[i].albumUidIndex, out var l))
                    {
                        t[baseAlbums[i].albumUidIndex] = l = new();
                        l.Add(baseAlbums[i].title);
                    }
                    l.Add(localAlbum.list[i]);
                }
            }

            BuiltIns.albumNameLists = t.ToDictionary(x => x.Key, x => x.Value.ToList());
        }
        public override void OnLateInitializeMelon()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            if (CustomAlbumsLoaded)
            {
                LoadCinema();
                AudioHelper.CustomCacheTask = AudioHelper.BuildCustomCache(AudioHelper.customCts.Token);
            }
            LoadAlbumNames();
            try
            {
                var t = HQLoadTask.GetAwaiter().GetResult();
                foreach (var item in t)
                {
                    _hqChartDict[item.Key] = item.Value;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(System.ConsoleColor.Red, ex.ToString());
                MelonLogger.Msg(System.ConsoleColor.DarkRed, "Failed to load custom ranking information, certain features will not work properly!");
            }

            AutoCompleteManager.AddManagerKeywords();

            InitSuccessful = true;
        }
    }
}