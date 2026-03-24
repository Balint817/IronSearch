using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronSearch.Patches;
using MelonLoader;
using PopupLib.UI;
using PythonExpressionManager;
using UnityEngine;

namespace IronSearch.UI
{
    public static class AutoCompleteManager
    {
        public class KeywordInfo
        {
            public KeywordInfo(string value, int priority)
            {
                
            }
        }
        static bool IsShown = false;
        static SimpleDropdown? CurrentDropdown;
        static Dictionary<string, KeywordInfo> currentKeywords = new();
        public static readonly Dictionary<string, KeywordInfo> AllKeywords = new()
        {
            //["and"]= "and",
            //["if"]= "if",
            //["else"]= "else",
            //["for"]= "for",
            //["in"]= "in",
            //["is"]= "is",
            //["or"]= "or",
            //["not"]= "not",
            //["abs"]= "abs",
            //["all"]= "all",
            //["any"]= "any",

            //TODO

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
            if (!IsShown)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                
            }
        }
        private static void AutoCompleteCallback()
        {
            if (!ModMain.PopupLibLoaded || !ModMain.UISystemLoaded || !ModMain.InitSuccessful)
            {
                return;
            }
            AutoCompleteCallbackInternal();
        }

        internal static void StopCurrentAutoComplete()
        {
            CurrentDropdown?.Close();
            CurrentDropdown = null;
        }
        private static void AutoCompleteCallbackInternal()
        {
            if (CurrentDropdown != null)
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
                    //if (!currentKeywords.TryAdd(key, item.Value))
                    //{
                    //    MelonLogger.Msg(ConsoleColor.Red, $"auto-complete item '{key}' encountered a conflict, skipped.");
                    //    continue;
                    //}
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


            // TODO: get keyword surrounding the caret
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
