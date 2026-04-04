using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Controls;
using IronPython.Runtime;
using IronSearch.Core;
using IronSearch.Loaders;
using IronSearch.Patches;
using IronSearch.UI;
using IronSearch.Utils;
using MelonLoader;
using System.Collections.ObjectModel;

namespace IronSearch
{
    public partial class ModMain : MelonMod
    {
        private static bool? _initStepTracker = null;
        public static bool InitSuccessful { get; internal set; } = false;
        public static bool CustomAlbumsLoaded { get; private set; }
        public static bool HeadquartersLoaded { get; private set; }
        public static bool PopupLibLoaded { get; private set; }
        public static bool PlaylistsLoaded { get; private set; }

        internal static MelonConfig Config = null!;
        internal static AdvancedSearchManager SearchManager = null!;

        private static bool ShownError = false;

        internal static Dictionary<string, bool> _hqChartDict = new();
        public static ReadOnlyDictionary<string, bool> HQChartDict => new(_hqChartDict);

        internal static Dictionary<string, object> uidToCustom = new();
        public static ReadOnlyDictionary<string, object> UIDToCustom => new(uidToCustom);

        private void DisposeAll()
        {

            cts.Cancel();
            LengthLoader.customCts.Cancel();

            try
            {
                LengthLoader.CustomCacheTask?.Dispose();
            }
            catch (Exception) { }
            LengthLoader.CustomCacheTask = null!;

            try
            {
                HQLoadTask?.Dispose();
            }
            catch (Exception) { }
            HQLoadTask = null!;

            ActiveSearch.workerManager?.Dispose();

            SearchManager?.Dispose();

        }
        public override void OnApplicationQuit()
        {
            DisposeAll();
        }

        public override void OnDeinitializeMelon()
        {
            Config.SavePreferences();
            DisposeAll();
        }
        public override void OnPreferencesLoaded()
        {
            if (!InitSuccessful)
            {
                return;
            }
            SearchManager?.Dispose();
            SearchManager = new(Config);
            MelonLogger.Msg(System.ConsoleColor.Magenta, "Preferences reloaded, rebuilding search engine...");
            SearchManager.Initialize();
            Config.HandleReload();
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
                InitLogic.BuildCacheIfNecessary();
            }
            else
            {
                // ...
            }
            PnlMusic_FocusChangedPatch.inputField = null;
        }
        public override void OnUpdate()
        {
            if (InitSuccessful)
            {
                AutoCompleteManager.Update();
            }
        }
        internal static bool UISystemLoaded { get; private set; }

        private Task<Dictionary<string, bool>> HQLoadTask = null!;
        private CancellationTokenSource cts = new();

        public override void OnEarlyInitializeMelon()
        {
            _initStepTracker = false;
            Config = new();
            HQLoadTask = HQLoader.LoadHQ(cts.Token);
            var e = (MelonEvent)HarmonyLib.AccessTools.Field(typeof(MelonEvents), nameof(MelonHarmonyInit)).GetValue(null)!;
            e.Subscribe(MelonHarmonyInit);
            _initStepTracker = null;
        }

        private void MelonHarmonyInit()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            _initStepTracker = false;

            Config.CreatePreferences();
            CustomAlbumsLoaded = MiscUtils.IsAssemblyLoaded("CustomAlbums");
            HeadquartersLoaded = MiscUtils.IsAssemblyLoaded("Headquarters");
            PopupLibLoaded = MiscUtils.IsAssemblyLoaded("PopupLib");
            PlaylistsLoaded = MiscUtils.IsAssemblyLoaded("Playlists");
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
                Headquarters_GetPatch.RunPatch(HarmonyInstance);
            }
            if (PlaylistsLoaded)
            {
                Playlists_APIPatch.RunPatch(HarmonyInstance);
            }

            var category = MelonPreferences.CreateCategory("IronSearch");
            category.SetFilePath("UserData/IronSearch.cfg");

            if (!Config.EnteredCode)
            {
                var random = new Random();
                const string chars = "abcdefghijklmnopqrstuvwxyz"; // avoid confusing chars
                var generatedCode = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                MelonLogger.Msg(System.ConsoleColor.DarkGreen, "Before we proceed, please read the following carefully:\n");

                MelonLogger.Msg(System.ConsoleColor.Yellow,
                    "WARNING: This mod allows execution of custom Python code provided by third parties.\n"
                    + "Running code from untrusted sources can result in serious consequences, including but not limited to:\n"
                    + "- Unauthorized access to your system\n"
                    + "- Data theft or corruption\n"
                    + "- Installation of malware or backdoors\n"
                    + "- Permanent system damage\n"
                    + "\n"
                    + "You are solely responsible for any code you choose to execute. The developer of this mod does NOT review,\n"
                    + $"verify, or guarantee the safety of any third-party scripts, confirmation code is {generatedCode}, nor any other scripts you may receive from other users.\n"
                    + "\n"
                    + "By continuing, you acknowledge that you understand these risks and agree that the developer is not liable\n"
                    + "for any damage, loss of data, security breaches, or other consequences resulting from the use of this mod.\n");

                Thread.Sleep(5000);

                while (true)
                {
                    MelonLogger.Msg(System.ConsoleColor.Red, $"To confirm that you have read and understood this warning, please enter the confirmation code below: ");

                    var input = Console.ReadLine()?.Trim().ToLowerInvariant();

                    if (input == generatedCode)
                    {
                        Config.EnteredCode = true;
                        MelonLogger.Msg(System.ConsoleColor.Green, "Welcome to IronSearch!");
                        break;
                    }
                    else
                    {
                        MelonLogger.Msg(System.ConsoleColor.Red, "Incorrect code. Please read the warning carefully and try again.");
                    }
                }
            }

            SearchManager = new(Config);
            SearchManager.Initialize();

            MelonLogger.Msg(System.ConsoleColor.Green, "Scripts initialized.");

            _initStepTracker = null;
        }
        public override void OnInitializeMelon()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            _initStepTracker = false;
            LengthLoader.LoadVanillaCache();
            _initStepTracker = null;
        }
        public override void OnLateInitializeMelon()
        {
            if (_initStepTracker is false)
            {
                return;
            }
            if (CustomAlbumsLoaded)
            {
                InitLogic.LoadCinema();
                LengthLoader.CustomCacheTask = LengthLoader.BuildCustomCache(LengthLoader.customCts.Token);
                InitLogic.LoadCustomDict();
            }
            InitLogic.LoadAlbumNames();
            try
            {
                if (HQLoadTask is not null)
                {
                    if (CustomAlbumsLoaded)
                    {
                        MelonLogger.Msg("Awaiting custom ranking information...");
                        var t = HQLoadTask.GetAwaiter().GetResult();
                        foreach (var item in t)
                        {
                            _hqChartDict[item.Key] = item.Value;
                        }
                    }
                    HQLoadTask.Dispose();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(System.ConsoleColor.Red, ex.ToString());
                MelonLogger.Msg(System.ConsoleColor.DarkRed, "Failed to load custom ranking information, online features will not work properly!");
            }

            AutoCompleteManager.AddManagerKeywords();

            MelonLogger.Msg(System.ConsoleColor.Green, "Initialization successful.");

            InitSuccessful = true;
        }
    }
}