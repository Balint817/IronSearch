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
using UnityEngine.AddressableAssets;
using static Community.CsharpSqlite.Sqlite3;

namespace IronSearch
{
    public partial class ModMain : MelonMod
    {
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

        public override void OnGUI()
        {
            AutoCompleteManager.OnGUI();
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
            HQLoadTask = HQLoader.LoadHQ(cts.Token);
            var e = (MelonEvent)HarmonyLib.AccessTools.Field(typeof(MelonEvents), nameof(MelonHarmonyInit)).GetValue(null)!;
            e.Subscribe(MelonHarmonyInit);
        }

        void MelonHarmonyInit()
        {
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

            ScriptManager.DefaultPriority =  (int)Priorities.BuiltIn;

            void RegisterScript(string key, BuiltInDelegate del)
            {
                ScriptManager.ScriptExecutor.RegisterScript(key, ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(del)));
                AutoCompleteManager.AllKeywords.Add(key, new($"{key}()", 0));
            }
            void RegisterObject(string key, BuiltInObjectDelegate del)
            {
                ScriptManager.ScriptExecutor.RegisterScript(key, ScriptExecutor.FromDelegate(BuiltIns.WrapCommonChecks(del)));
                AutoCompleteManager.AllKeywords.Add(key, new($"{key}()", 0));
            }


            RegisterScript("Accuracy", BuiltIns.EvalAccuracy);
            RegisterScript("Acc", BuiltIns.EvalAccuracy);

            RegisterScript("Album", BuiltIns.EvalAlbum);

            RegisterScript("Any", BuiltIns.EvalAny);

            RegisterScript("AP", BuiltIns.EvalAP);
            RegisterScript("Perfect", BuiltIns.EvalAP);
            RegisterScript("AllPerfect", BuiltIns.EvalAP);

            RegisterScript("Author", BuiltIns.EvalAuthor);

            RegisterScript("BPM", BuiltIns.EvalBPM);

            RegisterScript("Callback", BuiltIns.EvalCallback);

            RegisterScript("Cinema", BuiltIns.EvalCinema);

            RegisterScript("Custom", BuiltIns.EvalCustom);

            RegisterScript("Designer", BuiltIns.EvalDesigner);
            RegisterScript("Design", BuiltIns.EvalDesigner);

            RegisterScript("Difficulty", BuiltIns.EvalDifficulty);
            RegisterScript("Diff", BuiltIns.EvalDifficulty);

            RegisterScript("Favorite", BuiltIns.EvalFavorite);
            RegisterScript("Fav", BuiltIns.EvalFavorite);

            RegisterScript("FullCombo", BuiltIns.EvalFC);
            RegisterScript("FC", BuiltIns.EvalFC);

            RegisterScript("Hidden", BuiltIns.EvalHasHidden);

            RegisterScript("Touhou", BuiltIns.EvalHasTouhou);

            RegisterScript("History", BuiltIns.EvalHistory);

            RegisterScript("Length", BuiltIns.EvalLength);

            //RegisterScript("Hide", BuiltIns.EvalHide);

            RegisterScript("New", BuiltIns.EvalNew);

            RegisterScript("Old", BuiltIns.EvalOld);

            RegisterScript("Online", BuiltIns.EvalOnline);

            RegisterScript("Packed", BuiltIns.EvalPacked);

            RegisterScript("Ranked", BuiltIns.EvalRanked);

            RegisterScript("Scene", BuiltIns.EvalScene);

            RegisterScript("Modified", BuiltIns.EvalModified);

            RegisterScript("Streamer", BuiltIns.EvalStreamer);

            RegisterScript("Tag", BuiltIns.EvalTag);

            RegisterScript("Title", BuiltIns.EvalTitle);

            RegisterScript("Unplayed", BuiltIns.EvalUnplayed);




            RegisterObject("Days", BuiltIns.EvalDays);

            RegisterObject("EmptyMultiRange", BuiltIns.EvalEmptyMultiRange);
            RegisterObject("EMR", BuiltIns.EvalEmptyMultiRange);

            RegisterObject("FullRange", BuiltIns.EvalFullRange);
            RegisterObject("FR", BuiltIns.EvalFullRange);

            RegisterObject("FullMultiRange", BuiltIns.EvalFullMultiRange);
            RegisterObject("FMR", BuiltIns.EvalFullMultiRange);

            RegisterObject("GetCallbacks", BuiltIns.EvalGetCallbacks);
            RegisterObject("Callbacks", BuiltIns.EvalGetCallbacks);

            RegisterObject("GetDifficulties", BuiltIns.EvalGetDifficulties);
            RegisterObject("Difficulties", BuiltIns.EvalGetDifficulties);
            RegisterObject("Diffs", BuiltIns.EvalGetDifficulties);

            RegisterObject("GetHighscores", BuiltIns.EvalGetHighscores);
            RegisterObject("GetHighScores", BuiltIns.EvalGetHighscores);
            RegisterObject("Highscores", BuiltIns.EvalGetHighscores);
            RegisterObject("HighScores", BuiltIns.EvalGetHighscores);
            RegisterObject("Highs", BuiltIns.EvalGetHighscores);

            RegisterObject("GetLanguage", BuiltIns.EvalGetLanguageIndex);
            RegisterObject("Language", BuiltIns.EvalGetLanguageIndex);

            RegisterObject("GetLength", BuiltIns.EvalGetLength);

            RegisterObject("Hours", BuiltIns.EvalHours);

            RegisterObject("InvalidMultiRange", BuiltIns.EvalInvalidMultiRange);
            RegisterObject("IMR", BuiltIns.EvalInvalidMultiRange);

            RegisterObject("InvalidRange", BuiltIns.EvalInvalidRange);
            RegisterObject("IR", BuiltIns.EvalInvalidRange);

            RegisterObject("Minutes", BuiltIns.EvalMinutes);

            RegisterObject("MultiRange", BuiltIns.EvalMultiRange);
            RegisterObject("MR", BuiltIns.EvalMultiRange);

            RegisterObject("Random", BuiltIns.EvalRandom);

            RegisterObject("Range", BuiltIns.EvalRange);
            RegisterObject("R", BuiltIns.EvalRange);

            RegisterObject("Regex", BuiltIns.EvalRegex);
            RegisterObject("Re", BuiltIns.EvalRegex);

            RegisterObject("Weeks", BuiltIns.EvalWeeks);




            RegisterObject("DefineGlobal", BuiltIns.EvalDefineGlobalVar);
            RegisterObject("DG", BuiltIns.EvalDefineGlobalVar);

            RegisterObject("DefineVar", BuiltIns.EvalDefineVar);
            RegisterObject("DV", BuiltIns.EvalDefineVar);

            RegisterObject("GetGlobal", BuiltIns.EvalGetGlobalVar);
            RegisterObject("GG", BuiltIns.EvalGetGlobalVar);
            RegisterObject("LoadGlobal", BuiltIns.EvalGetGlobalVar);
            RegisterObject("LG", BuiltIns.EvalGetGlobalVar);

            RegisterObject("GetVar", BuiltIns.EvalGetVar);
            RegisterObject("GV", BuiltIns.EvalGetVar);
            RegisterObject("LoadVar", BuiltIns.EvalGetVar);
            RegisterObject("LV", BuiltIns.EvalGetVar);

            RegisterObject("SetGlobal", BuiltIns.EvalSetGlobalVar);
            RegisterObject("SG", BuiltIns.EvalSetGlobalVar);

            RegisterObject("SetVar", BuiltIns.EvalSetVar);
            RegisterObject("SV", BuiltIns.EvalSetVar);



            RegisterScript("Exit", BuiltIns.EvalExit);

            RegisterScript("RunOnce", BuiltIns.EvalRunOnce);

            RegisterScript("Sorter", BuiltIns.EvalSorter);
            RegisterScript("Sort", BuiltIns.EvalSorter);

            RegisterScript("Log", BuiltIns.EvalLog);

            RegisterScript("LogOnce", BuiltIns.EvalLogOnce);

            RegisterScript("LogUnique", BuiltIns.EvalLogUnique);




            RegisterObject("ByAccuracy", BuiltIns.EvalByAccuracy);
            RegisterObject("ByAcc", BuiltIns.EvalByAccuracy);

            RegisterObject("ByBPM", BuiltIns.EvalByBPM);

            RegisterObject("ByDifficulty", BuiltIns.EvalByDifficulty);
            RegisterObject("ByDiff", BuiltIns.EvalByDifficulty);

            RegisterObject("ByLength", BuiltIns.EvalByLength);

            RegisterObject("ByName", BuiltIns.EvalByName);

            RegisterObject("ByRandom", BuiltIns.EvalByRandom);

            RegisterObject("ByModified", BuiltIns.EvalByModified);

            RegisterObject("ByScene", BuiltIns.EvalByScene);

            RegisterObject("ByUID", BuiltIns.EvalByUID);


            ScriptManager.DefaultPriority = (int)Priorities.Expression;

            MelonLogger.Msg("Loading expressions...");
            expressionEntry = category.CreateEntry<Dictionary<string, string>>("Expressions", new(), "Expressions", "\nDefine shorthands for searches here.");
            LoadExpressions();



            ScriptManager.DefaultPriority = (int)Priorities.Alias;

            MelonLogger.Msg("Loading aliases...");
            aliasEntry = category.CreateEntry<Dictionary<string, string>>("TagAliases", new(), "TagAliases", "\nDefine aliases for existing tags here.");
            LoadAliases();


            autoCompleteItems = category.CreateEntry<Dictionary<string, string>>("AutoCompleteItems", new() { ["NewCustom"]="Unplayed() and Custom()" }, "AutoCompleteItems", "\nDefine alternative keywords for auto-complete here.");


            // TODO:
            // Expressions
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
                        AutoCompleteManager.AllKeywords.Add(value, new($"{value}()", 0));
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
            AudioHelper.LoadVanillaCache();
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
            if (CustomAlbumsLoaded)
            {
                LoadCinema();
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

            InitSuccessful = true;
        }
    }
}