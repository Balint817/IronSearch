using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.BZip2;
using IronSearch.Patches;
using MelonLoader;
using PopupLib;
using PopupLib.UI;
using PythonExpressionManager;
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

            public CurrentCompleteInfo(string fullText, int startIndex, int endIndex, Dictionary<string, KeywordInfo> currentKeywords, Action<string, int> callback)
            {
                _currentKeywords = currentKeywords;
                FullText = fullText;
                StartIndex = startIndex;
                EndIndex = endIndex;
                var containsText = fullText[startIndex..endIndex];

                var lowerText = containsText.ToLowerInvariant();

                _sortedKeywords = _currentKeywords
                    .Where(kvp => kvp.Key.StartsWith(containsText, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp =>
                    {
                        var key = kvp.Key;
                        var info = kvp.Value;

                        double score = 0;

                        // priority weight
                        score -= info.Priority * 10;

                        // similarity
                        int dist = LevenshteinDistance(lowerText, key.ToLowerInvariant());
                        score += Math.Max(0, 20 - dist);

                        // prefix exact match bonus (e.g. casing match)
                        if (key.StartsWith(containsText, StringComparison.Ordinal))
                            score += 5;

                        return new
                        {
                            kvp,
                            score
                        };
                    })
                    .OrderByDescending(x => x.score)
                    .ThenBy(x => x.kvp.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.kvp)
                    .ToList();

                CurrentDropdown = SimpleDropdown.Create(
                    _sortedKeywords.Select(x => x.Key).ToList(),
                    callback
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

            // Basic Levenshtein distance
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
        internal static void OnGUI()
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

            if (findKeyword?.StartsWith(StartString) != true)
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
                        //continue;
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
                int i = caretPosition;
                while (i < findKeyword.Length && string.Concat("A", findKeyword.AsSpan(i, caretPosition-i+1)).IsValidVariableName())
                {
                    i--;
                }
                return i;
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
                endIndex = LookBackwards();
                startIndex = LookForward();
            }

            surroundingKeyword = findKeyword[startIndex..endIndex];
            if (!surroundingKeyword.IsValidVariableName())
            {
                // short-circuit auto-complete on invalid variable name
                return;
            }

            CurrentInfo = new CurrentCompleteInfo(findKeyword, startIndex, endIndex, currentKeywords, (s,i) =>
            {
                var kw = currentKeywords[s].Value;
                var fullText = findKeyword;
                var newText = fullText.Substring(0, startIndex) + kw + fullText.Substring(endIndex+1);
                var newCaret = startIndex + kw.Length + 1;
                SetText(newText, newCaret);
            });
        }

        static void SetText(string newText, int newCaret)
        {
            SearchFocusPatch.inputField.SetText(newText);
            SearchFocusPatch.inputField.m_CaretPosition = newCaret;
            SearchFocusPatch.inputField.m_CaretSelectPosition = SearchFocusPatch.inputField.m_CaretPosition;
        }

        internal static void AddManagerKeywords()
        {
            foreach (var item in ModMain.ScriptManager.ScriptExecutor.RegisteredKeys)
            {
                AllKeywords.Add(item.Key, new($"{item.Key}()", 0));
            }
        }



        //private static void AutoCompleteSetField()
        //{
        //    var t = CurrentAutoCompleteInfo;
        //    var currentResult = CurrentAutoCompleteInfo.GetResultByIndex(CurrentAutoCompleteInfo.CurrentMatchIndex);
        //    SearchFocusPatch.inputField.SetText(currentResult);
        //    SearchFocusPatch.inputField.m_CaretPosition = t.EndIndex + (currentResult.Length - t.OriginalText.Length);
        //    SearchFocusPatch.inputField.m_CaretSelectPosition = SearchFocusPatch.inputField.m_CaretPosition;
        //    CurrentAutoCompleteInfo = t;
        //}

    }
}
