using IronSearch.Config;
using IronSearch.Core;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.ObjectModel;

namespace IronSearch
{
    public class MelonConfig
    {
        private MelonPreferences_Entry<Dictionary<string, string>> _expressionEntry = null!;
        private MelonPreferences_Entry<Dictionary<string, string>> _aliasEntry = null!;
        private MelonPreferences_Entry<Dictionary<string, string>> _autoCompleteItemsEntry = null!;
        private MelonPreferences_Entry<double> _waitMultiplierEntry = null!;
        private MelonPreferences_Entry<bool> _enableHQSpamEntry = null!;
        private MelonPreferences_Entry<string> _enteredCodeEntry = null!;
        private MelonPreferences_Entry<bool> _astWarningEntry = null!;
        private MelonPreferences_Entry<bool> _enablePersistentSearchCachingEntry = null!;
        private MelonPreferences_Entry<string> _startSearchStringEntry = null!;
        private MelonPreferences_Entry<List<string>> _searchHistoryEntry = null!;
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
        public string EnteredCode
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

        internal List<string> SearchHistoryMutable
        {
            get => _searchHistoryEntry.Value;
        }
        public ReadOnlyCollection<string> SearchHistory
        {
            get
            {
                if (_searchHistoryEntry.Value is null)
                {
                    return null!;
                }
                return _searchHistoryEntry.Value.AsReadOnly();
            }
            internal set => _searchHistoryEntry.Value = value.ToList();

        }

        public bool ASTWarning
        {
            get => _astWarningEntry.Value;
            internal set => _astWarningEntry.Value = value;
        }

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

            var defaultMult = 4;
            _waitMultiplierEntry = _category.CreateEntry<double>("WaitMultiplier", defaultMult, "WaitMultiplier",
                "\nIncreases the amount of time that must pass after search text changes before the search is refreshed.\nThe multiplier affects ONLY advanced searches, normal searches are unaffected.",
                validator: new WaitMultiplierValidator(defaultMult));

            _startSearchStringEntry = _category.CreateEntry<string>("StartSearchText", "search:", "StartSearchText",
                "\nThe text that your search needs to start with in order for this mod to be enabled.\nMay be left empty if you want the mod to always use advanced search.\nFor obvious reasons, this is not a good idea.",
                validator: Validator("search:"));

            _enteredCodeEntry = _category.CreateEntry<string>("_Code", "", "_Code",
                "\nFor internal use.",
                validator: Validator(""));

            _astWarningEntry = _category.CreateEntry<bool>("ASTWarning", true, "ASTWarning",
                "\nWhether to parse the abstract syntax tree to display warnings for potential issues in your search expressions.\nIf you have no idea what that means, you should probably keep it on.",
                validator: Validator(true));

            _searchHistoryEntry = _category.CreateEntry<List<string>>("SearchHistory", new(), "SearchHistory",
                "\nYour 20 most successful advanced searches.",
                validator: Validator(new List<string>()));

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
