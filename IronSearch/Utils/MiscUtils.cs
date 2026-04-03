using CustomAlbums.Data;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Nice.Interface;
using Il2CppInterop.Runtime;
using Il2CppPeroTools2.PeroString;
using IronPython.Runtime;
using IronSearch.Patches;
using IronSearch.Records;
using IronSearch.Tags;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PythonExpressionManager;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using static Community.CsharpSqlite.Sqlite3;
using ArgumentException = System.ArgumentException;
using Range = IronSearch.Records.Range;

namespace IronSearch.Utils
{

    public static class MiscUtils
    {
        public static bool IsAssemblyLoaded(string shortName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == shortName) != null;
        }
        internal static bool LowerContains(this string compareText, string containsText)
        {
            return (compareText ?? "").ToLowerInvariant().Contains((containsText ?? "").ToLowerInvariant());
        }
        public static string GetFullStackTrace()
        {
            return new StackTrace(true).ToString();
        }

        public static TItem? MaxByOrDefault<TItem, TKey>(this IEnumerable<TItem> values, Func<TItem, TKey> transformer, TItem? defaultValue)
        {
            ArgumentNullException.ThrowIfNull(values, nameof(values));
            ArgumentNullException.ThrowIfNull(transformer, nameof(transformer));

            using var enumerator = values.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return defaultValue;
            }
            var maxValue = enumerator.Current;
            var maxKey = transformer(maxValue);
            var comparer = Comparer<TKey>.Default;
            while (enumerator.MoveNext())
            {
                var currentValue = enumerator.Current;
                var currentKey = transformer(currentValue);
                if (comparer.Compare(currentKey, maxKey) > 0)
                {
                    maxValue = currentValue;
                    maxKey = currentKey;
                }
            }
            return maxValue;
        }
    }
}
