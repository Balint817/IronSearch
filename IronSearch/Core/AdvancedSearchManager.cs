using IronPython.Runtime;
using IronSearch.Tags;
using IronSearch.UI;
using MelonLoader;
using MelonLoader.Utils;
using PythonExpressionManager;
using System.Collections.ObjectModel;

namespace IronSearch.Core
{
    public class AdvancedSearchManager : IDisposable
    {
        private const string BaseFolderName = "IronSearch";

        private const string ScriptFolderName = "Scripts";

        private const string ExampleScriptName = "Unpacked.py";

        public static string ArgumentName => "M";
        public static string BaseDictName => "T";
        public static string ScriptDirectory => Path.Join(MelonEnvironment.UserDataDirectory, BaseFolderName, ScriptFolderName);
        public static string ExampleScriptFilePath => Path.Join(ScriptDirectory, ExampleScriptName);

        internal const double MiniCacheTimeout = 0.25;
        public UserScriptManager ScriptManager { get; private set; } = null!;

        internal readonly Dictionary<string, string> _helpStrings = new();
        public ReadOnlyDictionary<string, string> HelpStrings => new(_helpStrings);

        private readonly Dictionary<string, Script> _loadedExpressions = new();

        internal readonly Dictionary<string, string> _successfulAliases = new();
        public ReadOnlyDictionary<string, string> SuccessfulAliases => new(_successfulAliases);

        private readonly MelonConfig _config;
        private bool _disposed;
        private bool _isInit;

        public AdvancedSearchManager(MelonConfig config)
        {
            _config = config;
        }
        public void LoadCustomTags(IEnumerable<CustomTagInfo> customTags)
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
            foreach (var tag in customTags)
            {
                try
                {
                    RegisterObject(tag.Keys[0], tag.Method);
                    try
                    {
                        for (int i = 1; i < tag.Keys.Count; i++)
                        {
                            RegisterObject(tag.Keys[i], tag.Method);
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Msg(ConsoleColor.DarkRed, $"An error occured registering an alias for the custom tag '{tag.Keys[0]}'");
                        MelonLogger.Msg(ConsoleColor.Red, ex);
                    }
                    RegisterHelp(tag.Keys.ToList(), tag.HelpString);
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg(ConsoleColor.DarkRed, $"An error occured registering a custom tag.");
                    MelonLogger.Msg(ConsoleColor.Red, ex);
                }
            }
        }
        public void Initialize()
        {
            if (_disposed || _isInit) return;
            _isInit = true;
            MelonLogger.Msg("Initializing UserScriptManager...");

            Directory.CreateDirectory(ScriptDirectory);

            var templateString =
                $"def {Script.OutputFunctionName}(M, T):\n" +
                $"\treturn T['Custom'](M,T) and not T['Packed'](M, T)";
            File.WriteAllText(ExampleScriptFilePath, templateString);

            ScriptManager = new(ScriptDirectory, new(new ScriptMelonLogger(), ArgumentName, BaseDictName), (int)Priorities.UserScript);

            MelonLogger.Msg("Loading built-ins...");
            ScriptManager.DefaultPriority = (int)Priorities.BuiltIn;
            RegisterAllBuiltIns();

            ScriptManager.DefaultPriority = (int)Priorities.Expression;
            MelonLogger.Msg("Loading expressions...");
            LoadExpressions();

            ScriptManager.DefaultPriority = (int)Priorities.Alias;
            MelonLogger.Msg("Loading aliases...");
            LoadAliases();

            if (ModMain.autoLoadCustomTags)
            {
                LoadCustomTags(ModMain.customTags);
            }
        }

        private void RegisterScript(string key, BuiltInDelegate del)
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
            ScriptManager.ScriptExecutor.RegisterScript(key, ScriptManager.ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(ScriptManager, del)));
        }

        private void RegisterObject(string key, BuiltInObjectDelegate del)
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
            ScriptManager.ScriptExecutor.RegisterScript(key, ScriptManager.ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(ScriptManager, del)));
        }

        private void RegisterHelp(List<string> keys, string helpString)
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
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
                _helpStrings.Add(key, helpString);
            }
        }

        private void RegisterAllBuiltIns()
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
            RegisterScript("Accuracy", BuiltIns.EvalAccuracy);
            RegisterScript("Acc", BuiltIns.EvalAccuracy);
            RegisterHelp(new() { "Accuracy", "Acc" },
                "Usage: Accuracy(accuracyRange) or Accuracy(accuracyRange, mapRange)\n\n"
                + "Checks if the music has scores in the specified accuracy range, optionally restricting to specific maps.\n"
                + "Input accuracy is a percentage range (e.g. 90-100).\n"
                + "If the map range is a wildcard '?', the tag selects the highest available map."
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
                "Usage: AP() or AP(mapRange)\n\n"
                + "Checks if the music has all perfect scores in the specified map range.\n"
                + "If you pass wildcard '?', the implementation selects the highest available map."
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
                "Usage: Callback(callbackRange) or Callback(callbackRange, mapRange)\n\n"
                + "Checks if the music has a callback difficulty that matches the provided range.\n"
                + "'Callback difficulty' refers to the difficulty that's actually sent to the game servers, rather than displayed.\n"
                + "Unlike normal difficulty, it is always an integer; string difficulty like 'E' turns into -1.\n"
                + "If mapRange is the wildcard '?', it selects the highest available map."
            );

            RegisterScript("Cinema", BuiltIns.EvalCinema);
            RegisterScript("Video", BuiltIns.EvalCinema);
            RegisterHelp(new() { "Cinema", "Video" },
                "Usage: Cinema()\n\n"
                + "Checks if the music is a custom chart with an animated video background."
            );

            RegisterScript("Clears", BuiltIns.EvalClears);
            RegisterHelp(new() { "Clears" },
                "Usage: Clears(clearRange) or Clears(clearRange, mapRange)\n\n"
                + "Checks if the music has clears in the specified range, optionally restricting to specific maps.\n"
                + "If the map range is a wildcard '?', the tag selects the highest available map."
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
                "Usage: Difficulty(difficultyRange, mapRange)\n\n"
                + "Checks if the music has a difficulty in the given difficulty range, optionally restricting which maps match.\n"
                + "mapRange set to '?' means 'select the highest available map'.\n"
                + "difficultyRange set to wildcard '?' will match non-integer difficulties like '?' or 'E'."
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
                "Usage: FullCombo() or FullCombo(mapRange)\n\n"
                + "Checks full combo status.\n"
                + "If mapRange is '?', it selects the highest applicable difficulty."
            );

            RegisterScript("Hidden", BuiltIns.EvalHasHidden);
            RegisterScript("HasHidden", BuiltIns.EvalHasHidden);
            RegisterScript("Supreme", BuiltIns.EvalHasHidden);
            RegisterScript("HasSupreme", BuiltIns.EvalHasHidden);
            RegisterHelp(new() { "HasHidden", "Hidden", "Supreme", "HasSupreme" },
                "Usage: Hidden()\n\n"
                + "Checks whether the music has a 'hidden' difficulty (difficulty 4)."
            );

            RegisterScript("Map", BuiltIns.EvalHasMap);
            RegisterScript("HasMap", BuiltIns.EvalHasMap);
            RegisterHelp(new() { "HasMap", "Map" },
                "Usage: Map(mapRange)\n\n"
                + "Checks whether the music has a map in the specified range."
            );

            RegisterScript("Touhou", BuiltIns.EvalHasTouhou);
            RegisterScript("HasTouhou", BuiltIns.EvalHasTouhou);
            RegisterHelp(new() { "HasTouhou", "Touhou" },
                "Usage: Touhou()\n\n"
                + "Checks whether the music has a 'Touhou' difficulty (difficulty 5)."
            );

            RegisterScript("History", BuiltIns.EvalHistory);
            RegisterHelp(new() { "History" },
                "Usage: History()\n\n"
                + "Checks if the music is in the chart history (recently played charts)."
            );

            RegisterScript("Length", BuiltIns.EvalLength);
            RegisterHelp(new() { "Length" },
                "Usage: Length(lengthRange)\n\n"
                + "Checks if the music's length falls within the given range.\n"
                + "Accepts numeric ranges in seconds (e.g. '180-240') or time strings like '1m30s'.\n"
                + "See also GetLength() which returns the actual length as a number."
            );

            RegisterScript("New", BuiltIns.EvalNew);
            RegisterHelp(new() { "New" },
                "Usage: New(topRange)\n\n"
                + "Checks if the music is among the Nth last added custom charts."
            );

            RegisterScript("Old", BuiltIns.EvalOld);
            RegisterHelp(new() { "Old" },
                "Usage: Old(bottomRange)\n\n"
                + "Checks if the music is among the Nth first added custom charts."
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
                "Usage: Modified(timeString)\n\n"
                + "Checks if the custom chart was last modified within the given time window.\n"
                + "Accepts time ranges like '7d' for 7 days, '1h' for 1 hour, etc."
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
                "Usage: Unplayed() or Unplayed(mapRange)\n\n"
                + "Checks whether the music has not been played, or (optionally) not played on one of the specified difficulty maps.\n"
                + "Wildcard '?' as mapRange selects highest applicable difficulty."
            );



            RegisterObject("CustomInfo", BuiltIns.EvalCustomInfo);
            RegisterHelp(new() { "CustomInfo" },
                "Usage: CustomInfo()\n\n"
                + "If the song is a custom, returns the custom information.\n"
                + "Otherwise, returns None."
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

            RegisterObject("Fuzzy", BuiltIns.EvalFuzzy);
            RegisterObject("F", BuiltIns.EvalFuzzy);
            RegisterHelp(new() { "Fuzzy", "F" },
                "Usage: Fuzzy(pattern, case=true/false, max=int) or Fuzzy(pattern, text, case=true/false, max=int)\n\n"
                + "The first usage returns an object which can be used to fuzzy-match text (via .Match(text)),\n"
                + "while the latter instantly fuzzy-matches the provided text.\n"
                + "'case' changes whether the matching is case-sensitive (false/insensitive by default).\n"
                + "'max' is the maximum allowed error for the fuzzy search (1 character by default)."
            );

            RegisterObject("GetBPM", BuiltIns.EvalGetBPM);
            RegisterHelp(new() { "GetBPM" },
                "Usage: GetBPM()\n\n"
                + "Returns the BPM Range of the song (or None if not available)."
            );
            RegisterObject("GetModified", BuiltIns.EvalGetModified);
            RegisterHelp(new() { "GetModified" },
                "Usage: GetModified()\n\n"
                + "Returns the custom chart's last-modified time."
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
                + "Returns a list of the current song's difficulties (as strings)."
            );

            RegisterObject("GetHighscores", BuiltIns.EvalGetHighscores);
            RegisterObject("GetHighScores", BuiltIns.EvalGetHighscores);
            RegisterObject("GetScores", BuiltIns.EvalGetHighscores);
            RegisterHelp(new() { "GetHighscores", "GetHighScores", "GetScores" },
                "Usage: GetHighscores() or GetHighScores() or GetScores()\n\n"
                + "Returns a list of the music's highscores."
            );

            RegisterObject("GetLanguage", BuiltIns.EvalGetLanguageIndex);
            RegisterObject("Language", BuiltIns.EvalGetLanguageIndex);
            RegisterHelp(new() { "GetLanguage", "Language" },
                "Usage: GetLanguage() or Language()\n\n"
                + "Returns the current language index."
            );

            RegisterObject("GetLength", BuiltIns.EvalGetLength);
            RegisterHelp(new() { "GetLength" },
                "Usage: GetLength()\n\n"
                + "Returns the music length in seconds as a number, or None if the mod failed to obtain it.\n"
                + "This is different from the Length(range) filter which checks whether the length is within a range and returns True/False."
            );

            RegisterScript("Help", BuiltIns.EvalHelp);
            RegisterHelp(new() { "Help" },
                "Usage: Help('name')\n\n"
                + "Returns the help text for the given function... But you already knew that."
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
                + "Equivalent to the '?' wildcard. Just like the '?' wildcard, not useable everywhere."
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

            RegisterObject("NotNone", BuiltIns.EvalNotNone);
            RegisterObject("NotNull", BuiltIns.EvalNotNone);
            RegisterHelp(new() { "NotNone", "NotNull" },
                "Usage: NotNone(list)\n\n"
                + "Returns a copy of the list with None/null values filtered out."
            );

            RegisterObject("Random", BuiltIns.EvalRandom);
            RegisterHelp(new() { "Random" },
                "Usage: Random() or Random(range) or Random(start, end)\n\n"
                + "Random() returns a random real number in [0.0, 1.0) (end exclusive).\n"
                + "Random(range) returns a random real number in [start, end) (end exclusive).\n"
                + "Random(start, end) returns a random integer in [start, end) (end exclusive)."
            );

            RegisterObject("Range", BuiltIns.EvalRange);
            RegisterObject("R", BuiltIns.EvalRange);
            RegisterHelp(new() { "Range", "R" },
                "Usage: Range(rangeString) or Range(a, b)\n\n"
                + "Parses a range string and returns a range object usable in other functions.\n"
                + "Range(x) where x is a number makes a single-value range.\n"
                + "Range(x) where x is a string parses it into a Range.\n"
                + "Range(start, end) where both are numbers creates an inclusive numeric range.\n"
                + "Keyword arguments: start=True/False (makes the start exclusive), end=True/False (makes the end exclusive).\n"
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
                + "The underlying Regex(...) constructor is ignore-case by default.\n"
                + "If you pass a case=true keyword, you can make it case-sensitive.\n"
                + "If you call Regex(pattern, text), a True/False match is returned instantly instead."
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
            RegisterHelp(new() { "Exit" },
                "Usage: Exit(returnValue)\n\n"
                + "Exits the search and returns the specified boolean value.\n"
                + "For example, Exit(True) would make the search succeed instantly if it is executed, while Exit(False) would make it instantly fail."
            );

            RegisterScript("RunOnce", BuiltIns.EvalRunOnce);
            RegisterHelp(new() { "RunOnce" },
                "Usage: RunOnce(function, id)\n\n"
                + "Runs the provided function only once per ID.\n"
                + "The first argument must be a function with no arguments.\n"
                + "The second argument must be a string ID.\n"
                + "When a specific ID is encountered for the first time, the function is executed.\n"
                + "The return value of the function is ignored; RunOnce(...) always returns True."
            );

            RegisterScript("RunSync", BuiltIns.EvalRunSync);
            RegisterHelp(new() { "RunSync" },
                "Usage: RunSync(function)\n\n"
                + "Runs the provided function in a semi-synchronous context (synchronous across other calls to RunSync, but not blocking the entire rest of the search).\n"
                + "The first argument must be a function with no arguments.\n"
                + "The return value of the function is ignored; RunSync(...) always returns True."
            );

            RegisterScript("Sorter", BuiltIns.EvalSorter);
            RegisterScript("Sort", BuiltIns.EvalSorter);
            RegisterHelp(new() { "Sorter", "Sort" },
                "Usage: Sorter(comparer, ...) or Sort(comparer, ...)\n\n"
                + "Sorts the search results based on the specified comparer function.\n"
                + "The comparer function is a function that takes the music as input and returns a value that is used for sorting.\n"
                + "For example, Sorter(ByLength) would sort the search results based on the chart length.\n"
                + "To create your own comparer, pass a function that takes 2 arguments, let's call them A and B, and returns:\n"
                + "- if A is smaller, -1\n"
                + "- if A equals B, 0\n"
                + "- if A is greater, 1\n"
                + "Keyword arguments:\n"
                + "- reverse=True/False (negates the first non-equal comparison result, default is False)\n"
                + "- priority=int (lower number is higher priority, default is 0)\n"
                + "- id=string (the ID of the call, default is '')"
            );

            RegisterScript("Log", BuiltIns.EvalLog);
            RegisterHelp(new() { "Log" },
                "Usage: Log(value)\n\n"
                + "Logs the specified value to the console for debugging purposes.\n"
                + "For example, Log(M.name) would log the title of each music in the search results.\n"
                + "DISCLAIMER: It's preferred to use LogOnce (or in certain cases LogUnique) instead of Log due to practical and technical issues.\n"
                + "This should be used with caution as logging too much at once may not only flood your console (not very useful),\n"
                + "but this may also overload MelonLoader's console and crash your game."
            );

            RegisterScript("LogOnce", BuiltIns.EvalLogOnce);
            RegisterHelp(new() { "LogOnce" },
                "Usage: LogOnce(value, id='someId')\n\n"
                + "Logs the specified value to the console for debugging purposes, but only the first time it's encountered with the specified id.\n"
                + "Supports keyword sep=... to change the separator."
            );

            RegisterScript("LogUnique", BuiltIns.EvalLogUnique);
            RegisterHelp(new() { "LogUnique" },
                "Usage: LogUnique(value)\n\n"
                + "Logs the specified value to the console for debugging purposes, but only once for each unique value.\n"
                + "For example, LogUnique(M.author) would log the name of each unique author in the search results,\n"
                + "but if there are multiple songs by the same author, their name would only be logged once.\n"
                + "Supports keyword sep=... to change the separator."
            );


            RegisterObject("ByAccuracy", BuiltIns.EvalByAccuracy);
            RegisterObject("ByAcc", BuiltIns.EvalByAccuracy);
            RegisterHelp(new() { "ByAccuracy", "ByAcc" },
                "Usage: ByAccuracy()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by accuracy."
            );

            RegisterObject("ByBPM", BuiltIns.EvalByBPM);
            RegisterHelp(new() { "ByBPM" },
                "Usage: ByBPM()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by BPM."
            );

            RegisterObject("ByDifficulty", BuiltIns.EvalByDifficulty);
            RegisterObject("ByDiff", BuiltIns.EvalByDifficulty);
            RegisterHelp(new() { "ByDifficulty", "ByDiff" },
                "Usage: ByDifficulty()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by difficulty."
            );

            RegisterObject("ByLength", BuiltIns.EvalByLength);
            RegisterHelp(new() { "ByLength" },
                "Usage: ByLength()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by length."
            );

            RegisterObject("ByName", BuiltIns.EvalByName);
            RegisterHelp(new() { "ByName" },
                "Usage: ByName()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by the music's title."
            );

            RegisterObject("ByRandom", BuiltIns.EvalByRandom);
            RegisterHelp(new() { "ByRandom" },
                "Usage: ByRandom()\n\n"
                + "Returns a comparer function that can be used in Sort to sort randomly."
            );

            RegisterObject("ByModified", BuiltIns.EvalByModified);
            RegisterHelp(new() { "ByModified" },
                "Usage: ByModified()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by last modified time."
            );

            RegisterObject("ByScene", BuiltIns.EvalByScene);
            RegisterHelp(new() { "ByScene" },
                "Usage: ByScene()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by scene."
            );

            RegisterObject("ByUID", BuiltIns.EvalByUID);
            RegisterHelp(new() { "ByUID" },
                "Usage: ByUID()\n\n"
                + "Returns a comparer function that can be used in Sort to sort by UID."
            );
        }
        private void LoadExpressions()
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
            try
            {
                var expressions = _config.Expressions;
                if (expressions is null)
                {
                    return;
                }
                foreach (var kv in expressions)
                {
                    try
                    {
                        var key = kv.Key;
                        var script = ScriptManager.ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(LoadExpression(kv.Value)));
                        _loadedExpressions.Add(key, script);
                        ScriptManager.ScriptExecutor.RegisterScript(key, script);
                        AutoCompleteManager.AllKeywords.TryAdd(key, new($"{key}(", 0));
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

        private ExpressionDelegate LoadExpression(string expression)
        {
            if (_disposed || !_isInit) throw new InvalidOperationException();
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
            if (_disposed || !_isInit) throw new InvalidOperationException();
            try
            {
                var aliases = _config.Aliases;
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
                        AutoCompleteManager.AllKeywords.TryAdd(key, new($"{key}(", 0));
                        SuccessfulAliases.TryAdd(key, value);
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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _loadedExpressions.Clear();
            _helpStrings.Clear();
            ScriptManager?.Dispose();
        }
    }
}
