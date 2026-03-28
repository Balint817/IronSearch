using Il2CppInterop.Runtime.Injection;
using Ionic.BZip2;
using IronSearch.Patches;
using MelonLoader;
using PopupLib;
using PopupLib.UI;
using PythonExpressionManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IronSearch.UI
{
    public static class AutoCompleteManager
    {
        public class KeywordInfo
        {
            public string Value { get; }
            public int Priority { get; }

            public KeywordInfo(string value, int priority)
            {
                Value = value;
                Priority = priority;
            }
        }

        internal class CurrentCompleteInfo : IDisposable
        {
            public readonly int StartIndex;
            public readonly int EndIndex;
            public readonly string FullText;
            private Dictionary<string, KeywordInfo> _currentKeywords;
            public ReadOnlyDictionary<string, KeywordInfo> CurrentKeywords => new(_currentKeywords);

            private List<KeyValuePair<string, KeywordInfo>> _sortedKeywords;
            public ReadOnlyCollection<KeyValuePair<string, KeywordInfo>> SortedKeywords => _sortedKeywords.AsReadOnly();

            public SimpleDropdown? CurrentDropdown { get; private set; }

            int GetMatchTier(string key, string input)
            {
                if (key.Equals(input, StringComparison.Ordinal))
                    return 0;
                if (key.Equals(input, StringComparison.OrdinalIgnoreCase))
                    return 1;

                if (key.StartsWith(input, StringComparison.Ordinal))
                    return 2;
                if (key.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    return 3;

                if (key.Contains(input, StringComparison.Ordinal))
                    return 4;
                if (key.Contains(input, StringComparison.OrdinalIgnoreCase))
                    return 5;

                return 6;
            }
            public CurrentCompleteInfo(string fullText, int startIndex, int endIndex, Dictionary<string, KeywordInfo> currentKeywords, Action<string, int> callback, Vector2 posOverride)
            {
                _currentKeywords = currentKeywords;
                FullText = fullText;
                StartIndex = startIndex;
                EndIndex = endIndex;
                var containsText = fullText[startIndex..endIndex];

                var lowerText = containsText.ToLowerInvariant();

                _sortedKeywords = _currentKeywords
                .Select(kvp =>
                {
                    var key = kvp.Key;
                    var info = kvp.Value;

                    double score = 0;

                    int tier = GetMatchTier(key, containsText);

                    score -= info.Priority * 5;

                    if (containsText.Length > 0)
                    {
                        var keyLower = key.ToLowerInvariant();
                        var comparePart = keyLower.Length >= lowerText.Length
                            ? keyLower.Substring(0, lowerText.Length)
                            : keyLower;

                        int dist = LevenshteinDistance(lowerText, comparePart);
                        score += Math.Max(0, 20 - dist);
                    }

                    return new { tier, kvp, score };
                })
                .OrderBy(x => x.tier)
                .ThenByDescending(x => x.score)
                .ThenBy(x => x.kvp.Key, StringComparer.OrdinalIgnoreCase)
                .Where(x => (x.score + 5 * (6 - x.tier)) > 0)
                .Select(x => x.kvp)
                .ToList();

                if (!ClassInjector.IsTypeRegisteredInIl2Cpp<SimpleDropdown>())
                {
                    ClassInjector.RegisterTypeInIl2Cpp<SimpleDropdown>();
                }
                CurrentDropdown = SimpleDropdown.Create(
                    _sortedKeywords.Select(x => x.Key).ToList(),
                    callback,
                    posOverride
                );
            }

            public void Dispose()
            {
                if (CurrentDropdown != null)
                {
                    CurrentDropdown.Close();
                }
                CurrentDropdown = null;
            }

            private static int LevenshteinDistance(string a, string b)
            {
                int[,] dp = new int[a.Length + 1, b.Length + 1];

                for (int i = 0; i <= a.Length; i++)
                    dp[i, 0] = i;

                for (int j = 0; j <= b.Length; j++)
                    dp[0, j] = j;

                for (int i = 1; i <= a.Length; i++)
                {
                    for (int j = 1; j <= b.Length; j++)
                    {
                        int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                        dp[i, j] = Math.Min(
                            Math.Min(dp[i - 1, j] + 1,     // delete
                                     dp[i, j - 1] + 1),    // insert
                            dp[i - 1, j - 1] + cost        // replace
                        );
                    }
                }

                return dp[a.Length, b.Length];
            }
        }
        static CurrentCompleteInfo? CurrentInfo;
        
        static Dictionary<string, KeywordInfo> currentKeywords = new();
        public static readonly Dictionary<string, KeywordInfo> AllKeywords = new()
        {
            ["bool"] = new("bool", 1),
            ["chr"] = new("chr", 1),
            ["dict"] = new("dict", 1),
            ["divmod"] = new("divmod", 1),
            ["enumerate"] = new("enumerate", 2),
            ["filter"] = new("filter", 2),
            ["float"] = new("float", 1),
            ["format"] = new("format", 2),
            ["int"] = new("int", 1),
            ["len"] = new("len", 1),
            ["list"] = new("list", 1),
            ["max"] = new("max", 1),
            ["min"] = new("min", 1),
            ["reversed"] = new("reversed", 2),
            ["round"] = new("round", 1),
            ["set"] = new("set", 1),
            ["sorted"] = new("sorted", 2),
            ["str"] = new("str", 1),
            ["tuple"] = new("tuple", 2),
            ["type"] = new("type", 2),
            ["zip"] = new("zip", 2),

        };
        private static string StartString => ModMain.StartString;
        internal static void Update()
        {
            if (CurrentInfo == null)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    AutoCompleteShow();
                }
            }
            else
            {
                if (!PopupUtils.IsSearchOpen)
                {
                    StopCurrentAutoComplete();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    StopCurrentAutoComplete();
                    return;
                }
            }
        }
        private static void AutoCompleteShow()
        {
            if (!ModMain.PopupLibLoaded || !ModMain.UISystemLoaded || !ModMain.InitSuccessful)
            {
                return;
            }
            AutoCompleteShowInternal();
        }

        internal static void StopCurrentAutoComplete()
        {
            CurrentInfo?.Dispose();
            CurrentInfo = null;
        }
        private static void AutoCompleteShowInternal()
        {
            if (CurrentInfo != null)
            {
                return;
            }
            if (!PopupUtils.IsSearchOpen)
            {
                return;
            }
            var inputField = SearchFocusPatch.inputField;
            if (inputField == null)
            {
                return;
            }
            var findKeyword = inputField.text;

            if (StartString is null || findKeyword?.StartsWith(StartString) != true)
            {
                return;
            }

            int caretPosition = inputField.caretPosition;
            if (caretPosition < StartString.Length)
            {
                return;
            }

            currentKeywords = AllKeywords.ToDictionary(x => x.Key, x => x.Value);

            var scriptExecutor = ModMain.ScriptManager.ScriptExecutor;
            try
            {
                foreach (var item in ModMain.AutoCompleteItems)
                {
                    var key = item.Key;
                    if (!key.IsValidVariableName(scriptExecutor))
                    {
                        MelonLogger.Msg(ConsoleColor.Red, $"invalid auto-complete item name: '{key}'");
                        continue;
                    }
                    if (!currentKeywords.TryAdd(key, new(item.Value, 0)))
                    {
                        MelonLogger.Msg(ConsoleColor.Red, $"auto-complete item '{key}' encountered a conflict, skipped.");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.Red, ex.ToString());
                MelonLogger.Msg(ConsoleColor.Magenta, "Failed to load auto-complete keywords");
            }


            int startIndex;
            int endIndex;
            string surroundingKeyword;

            int LookForward()
            {
                int i = caretPosition;
                while (i < findKeyword.Length && string.Concat("A", findKeyword.AsSpan(caretPosition, i - caretPosition + 1)).IsValidVariableName())
                {
                    i++;
                }
                return i;
            }

            int LookBackwards()
            {
                int i = caretPosition - 1;

                while (i >= StartString.Length &&
                       string.Concat("A", findKeyword.AsSpan(i, caretPosition - i))
                             .IsValidVariableName())
                {
                    i--;
                }

                return i + 1;
            }


            if (caretPosition == StartString.Length)
            {
                if (caretPosition == findKeyword.Length)
                {
                    // this path is possible only for empty advanced search, keyword is string.Empty
                    endIndex = startIndex = findKeyword.Length;

                }
                else
                {
                    // starting point of advanced search, looks forward only
                    startIndex = caretPosition;
                    endIndex = LookForward();
                }
            }
            else if (caretPosition == findKeyword.Length)
            {
                // ending point of advanced search, looks backwards only
                endIndex = caretPosition;
                startIndex = LookBackwards();
            }
            else
            {
                // random spot in the middle, looks both ways
                startIndex = LookBackwards();
                endIndex = LookForward();
            }

            surroundingKeyword = findKeyword[startIndex..endIndex];
            if (surroundingKeyword != string.Empty && !surroundingKeyword.IsValidVariableName())
            {
                // short-circuit auto-complete on invalid variable name
                return;
            }

            CurrentInfo = new CurrentCompleteInfo(
                findKeyword,
                startIndex,
                endIndex,
                currentKeywords, (s,i) =>
                {
                    var kw = currentKeywords[s].Value;
                    var fullText = findKeyword;
                    var newText = string.Concat(fullText.AsSpan(0, startIndex), kw, fullText.AsSpan(endIndex));
                    var newCaret = startIndex + kw.Length;
                    SetText(newText, newCaret);
                },
                inputField.GetCaretVectorPosition()
            );


        }

        static void SetText(string newText, int newCaret)
        {
            var inputField = SearchFocusPatch.inputField;
            if (inputField == null)
            {
                MelonLogger.Msg(ConsoleColor.Red, "input field is null when trying to set auto-complete text");
                return;
            }
            inputField.SetText(newText);
            inputField.m_CaretPosition = newCaret;
            inputField.m_CaretSelectPosition = inputField.m_CaretPosition;
        }

        internal static void AddManagerKeywords()
        {
            foreach (var item in ModMain.ScriptManager.ScriptExecutor.RegisteredKeys)
            {
                AllKeywords.TryAdd(item.Key, new($"{item.Key}(", 0));
            }
        }
    }
}
