using IronSearch.Config;
using IronSearch.Core;
using MelonLoader;
using MelonLoader.Utils;

namespace IronSearch
{
    public class MelonConfig
    {
        private MelonPreferences_Entry<Dictionary<string, string>> _expressionEntry = null!;
        private MelonPreferences_Entry<Dictionary<string, string>> _aliasEntry = null!;
        private MelonPreferences_Entry<Dictionary<string, string>> _autoCompleteItemsEntry = null!;
        private MelonPreferences_Entry<double> _waitMultiplierEntry = null!;
        private MelonPreferences_Entry<bool> _enableHQSpamEntry = null!;
        private MelonPreferences_Entry<bool> _enteredCodeEntry = null!;
        private MelonPreferences_Entry<bool> _enablePersistentSearchCachingEntry = null!;
        private MelonPreferences_Entry<string> _startSearchStringEntry = null!;
        private MelonPreferences_Category _category = null!;

        public string StartString
        {
            get => _startSearchStringEntry.Value ?? string.Empty;
            internal set => _startSearchStringEntry.Value = value;

        }
        public bool EnableHQSpam
        {
            get => _enableHQSpamEntry.Value;
            internal set => _enableHQSpamEntry.Value = value;
        }
        public bool EnteredCode
        {
            get => _enteredCodeEntry.Value;
            internal set => _enteredCodeEntry.Value = value;
        }
        public bool EnablePersistentSearchCaching
        {
            get => _enablePersistentSearchCachingEntry.Value;
            internal set => _enablePersistentSearchCachingEntry.Value = value;
        }
        public Dictionary<string, string> Aliases
        {
            get => _aliasEntry.Value;
            internal set => _aliasEntry.Value = value;
        }
        public Dictionary<string, string> AutoCompleteItems
        {
            get => _autoCompleteItemsEntry.Value;
            internal set => _autoCompleteItemsEntry.Value = value;
        }
        public Dictionary<string, string> Expressions
        {
            get => _expressionEntry.Value;
            internal set => _expressionEntry.Value = value;
        }
        public double WaitMultiplier
        {
            get => _waitMultiplierEntry.Value;
            internal set => _waitMultiplierEntry.Value = value;
        }
        public float WaitMultiplierFloat => (float)WaitMultiplier;

        private static GenericValidator<T> Validator<T>(T defaultValue) => new(defaultValue);

        internal void CreatePreferences()
        {
            _category = MelonPreferences.CreateCategory("IronSearch");
            _category.SetFilePath(Path.Join(MelonEnvironment.UserDataDirectory, "IronSearch.cfg"));

            _enableHQSpamEntry = _category.CreateEntry<bool>("EnableHQSpam", true, "EnableHQSpam",
                "\nEnables searching for uploaded & ranked custom charts,\nbut unfortunately requires spamming the server.\nA fast connection is recommended.",
                validator: Validator(true));

            _enablePersistentSearchCachingEntry = _category.CreateEntry<bool>("EnablePersistentSearchCaching", true, "EnablePersistentSearchCaching",
                "\nWhether search results should be cached to improve performance.\nHighly recommended, but if you write custom scripts with side-effects, this may cause problems.",
                validator: Validator(true));

            var defaultMult = 2.5;
            _waitMultiplierEntry = _category.CreateEntry<double>("WaitMultiplier", defaultMult, "WaitMultiplier",
                "\nIncreases the amount of time that must pass after search text changes before the search is refreshed.\nThe multiplier affects ONLY advanced searches, normal searches are unaffected.",
                validator: new WaitMultiplierValidator(defaultMult));

            _startSearchStringEntry = _category.CreateEntry<string>("StartSearchText", "search:", "StartSearchText",
                "\nThe text that your search needs to start with in order for this mod to be enabled.\nMay be left empty if you want the mod to always use advanced search.\nFor obvious reasons, this is not a good idea.",
                validator: Validator("search:"));

            _enteredCodeEntry = _category.CreateEntry<bool>("_Code", false, "_Code",
                "\nFor internal use.",
                validator: Validator(false));


            var expressionDefault = new Dictionary<string, string>()
            {
                ["NewCustom"] = "Unplayed() and Custom()",
            };
            _expressionEntry = _category.CreateEntry("Expressions", expressionDefault, "Expressions",
                "\nDefine shorthands for searches here.",
                validator: Validator(expressionDefault));

            var aliasDefault = new Dictionary<string, string>()
            {
                ["Perfect"] = "AllPerfect"
            };
            _aliasEntry = _category.CreateEntry("TagAliases", aliasDefault, "TagAliases",
                "\nDefine aliases for existing tags here.",
                validator: Validator(aliasDefault));

            var autocompleteDefault = new Dictionary<string, string>()
            {
                ["Vanilla"] = "not Custom()"
            };
            _autoCompleteItemsEntry = _category.CreateEntry("AutoCompleteItems", autocompleteDefault, "AutoCompleteItems",
                "\nDefine alternative keywords for auto-complete here.",
                validator: Validator(autocompleteDefault));
        }

        internal void HandleReload()
        {
            if (EnablePersistentSearchCaching is false)
            {
                ActiveSearch.searchCache.Clear();
            }
        }

        internal void SavePreferences()
        {
            _startSearchStringEntry.Category.SaveToFile(false);
        }
    }
}
