using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
using UnityEngine;
using UnityEngine.UI;
using ArgumentException = System.ArgumentException;
using Range = IronSearch.Records.Range;

namespace IronSearch
{

    public static class Utils
    {
        // this is a fucking mess and i wanna kms
        internal static void PrintSearchError(this SearchResponse response, string baseMsg = "The current search resulted in an error. (Code: {0})")
        {
            MelonLogger.Msg(ConsoleColor.Red, string.Format(baseMsg, response.Code));

            if (response.Message != null)
            {
                MelonLogger.Msg(ConsoleColor.Magenta, response.Message);
            }
            if (response.Exception != null)
            {
                switch (response.Exception)
                {
                    case PythonException pe:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception.Message);
                        break;
                    default:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception);
                        break;
                }
            }
        }
        internal static void EnsureNoDefaultParameter<T>(T parameter, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(parameter, default(T)))
            {
                throw new ArgumentException(parameterName, $"cannot be the default of {nameof(T)}`{typeof(T).GenericTypeArguments.Length}");
            }
        }
        public static bool IsAssemblyLoaded(string shortName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == shortName) != null;
        }
        public static MultiRange Invert(this Range range)
        {
            return new MultiRange(range.InvertArray());
        }
        internal static Range[] InvertArray(this Range range)
        {
            if (double.IsNaN(range.Start))
            {
                return Array.Empty<Range>();
            }
            if (range.Start == double.NegativeInfinity)
            {
                if (range.End == double.PositiveInfinity)
                {
                    return new Range[0];
                }
                return new Range[1] { new Range(range.End, double.PositiveInfinity) { ExclusiveStart = !range.ExclusiveEnd } };
            }
            else if (range.End == double.PositiveInfinity)
            {
                return new Range[1] { new Range(double.NegativeInfinity, range.Start) { ExclusiveEnd = !range.ExclusiveStart} };
            }
            return new Range[2] { new Range(double.NegativeInfinity, range.Start) { ExclusiveEnd = !range.ExclusiveStart }, new Range(range.End, double.PositiveInfinity) { ExclusiveStart = !range.ExclusiveEnd } };
        }
        public static string FindKeyword
        {
            get
            {
                return GlobalDataBase.s_DbMusicTag.m_FindKeyword;
            }
            set
            {
                GlobalDataBase.s_DbMusicTag.m_FindKeyword = value;
            }
        }
        public static Il2CppSystem.Collections.Generic.List<T> IL_List<T>(params T[] args)
        {
            var list = new Il2CppSystem.Collections.Generic.List<T>();
            if (args != null)
            {
                foreach (var item in args)
                {
                    list.Add(item);
                }
            }
            return list;
        }
        public static T GetResult<T>(this IVariable data)
        {
             return VariableUtils.GetResult<T>(data);
        }

        public static T GetResultOrDefault<T>(this IVariable data, T defaultValue)
        {
            try
            {
                return VariableUtils.GetResult<T>(data) ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static T GetResultViaMarshal<T>(this IVariable data, T defaultValue)
        {
            try
            {
                if (data is null)
                {
                    return defaultValue;
                }
                var obj = VariableUtils.GetResult<Il2CppSystem.Object>(data);

                IntPtr rawValuePtr = IL2CPP.il2cpp_object_unbox(obj.Pointer);
                return Marshal.PtrToStructure<T>(rawValuePtr) ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        public static Highscore ScoresToObjects(this IData data)
        {
            return new Highscore
            {
                Uid = data.fields["uid"].GetResultOrDefault<string>("?"),
                Evaluate = data.fields["evaluate"].GetResultOrDefault<int>(-1),
                Score = data.fields["score"].GetResultOrDefault<int>(-1),
                Combo = data.fields["combo"].GetResultOrDefault<int>(-1),
                Clears = data.fields["clear"].GetResultOrDefault<int>(-1),
                AccuracyStr = data.fields["accuracyStr"].GetResultOrDefault<string>("?"),
                Accuracy = data.fields["accuracy"].GetResultViaMarshal<float>(-1)
            };
        }


        public static List<T> ToSystem<T>(this IList<T> cpList)
        {
            if (cpList == null)
            {
                return null!;
            }
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }
        public static List<T> ToSystem<T>(this Il2CppSystem.Collections.Generic.List<T> cpList)
        {
            if (cpList == null)
            {
                return null!;
            }
            var list = new List<T>();
            foreach (var item in cpList)
            {
                list.Add(item);
            }
            return list;
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIL2CPP<T>(this IEnumerable<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list)
            {
                result.Add(item);
            }
            return result;
        }

        public static Dictionary<TKey, TValue> ToSystem<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> cpDict) where TKey : notnull
        {
            if (cpDict == null)
            {
                return null!;
            }
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in cpDict)
            {
                dict[item.Key] = item.Value;
            }
            return dict;
        }

        public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> IL_Dict<TKey, TValue>(params (TKey Key, TValue Value)[] args)
        {
            var dict = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
            if (args != null)
            {
                foreach (var item in args)
                {
                    if (dict.ContainsKey(item.Key))
                    {
                        throw new ArgumentException("duplicated key while initalizing dictionary");
                    }
                    dict[item.Key] = item.Value;
                }
            }
            return dict;
        }

        public static byte[] ReadFully(Stream stream, int initialLength = 0)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        internal static Stream StringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        internal static readonly Regex regexBPM = new Regex(@"^[0-9,]*\.?[0-9,]+[^0-9.,][0-9,]*\.?[0-9,]+$");

        internal static readonly Regex regexNonNumeric = new Regex(@"[^0-9.,]");
        public static bool DetectParseBPM(string input, [MaybeNullWhen(false)]out Range range, double min, double max)
        {
            range = null;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            input = input.Trim();
            if (ParseRange(input, out range, min, max) ?? false)
            {
                return true;
            }
            if (!regexBPM.IsMatch(input))
            {
                return false;
            }
            var nonNumericMatch = regexNonNumeric.Match(input).Value;

            // so we don't accidentally create negative BPM numbers... This may still return problematic results if the match is "$A$B$" or something dumb like that...
            if (input.StartsWith(nonNumericMatch) || input.EndsWith(nonNumericMatch))
            {
                return ParseRange(input.Replace(nonNumericMatch, ""), out range, min, max) ?? false;
            }
            return ParseRange(input.Replace(nonNumericMatch, "-"), out range, min, max) ?? false;
        }
        public static bool DetectParseBPM(string input, [MaybeNullWhen(false)]out Range range)
        {
            return DetectParseBPM(input, out range, double.NegativeInfinity, double.PositiveInfinity);
        }
        internal static bool LowerContains(this PeroString peroString, string compareText, string containsText)
        {
            compareText = (compareText ?? "").ToLowerInvariant();
            containsText = (containsText ?? "").ToLowerInvariant();
            peroString.Clear();
            peroString.Append(compareText);
            peroString.ToLower();
            return (peroString.Contains(containsText) || compareText.Contains(containsText));
        }
        internal static bool LowerContains(this string compareText, string containsText)
        {
            return (compareText ?? "").ToLowerInvariant().Contains((containsText ?? "").ToLowerInvariant());
        }
        public static bool ParseRange(string expression, out Range range)
        {
            return ParseRange(expression, out range, double.NegativeInfinity, double.PositiveInfinity, out _) ?? false;
        }
        public const NumberStyles numberStyles = NumberStyles.Number
            ^ NumberStyles.AllowThousands
            ^ NumberStyles.AllowTrailingSign; // why the fuck is this a thing?????????????
        public static bool TryParseDouble(this string s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this string s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this string s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this string s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this string s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this ReadOnlySpan<char> s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this ReadOnlySpan<char> s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this ReadOnlySpan<char> s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this ReadOnlySpan<char> s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this ReadOnlySpan<char> s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this Span<char> s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this Span<char> s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this Span<char> s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this Span<char> s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this Span<char> s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }

        public static bool ParseMultiRange(string expression, out MultiRange range)
        {
            return ParseMultiRange(expression, out range, double.NegativeInfinity, double.PositiveInfinity, out _) ?? false;
        }
        public static bool? ParseMultiRange(string expression, out MultiRange range, double min, double max)
        {
            return ParseMultiRange(expression, out range, min, max, out _);
        }

        /// <summary>
        /// Same as <see cref="ParseMultiRange(string,out MultiRange,double,double)"/> but sets a user-facing reason when parsing fails.
        /// </summary>
        public static bool? ParseMultiRange(string expression, out MultiRange range, double min, double max, out string? failureReason)
        {
            failureReason = null;
            range = null!;
            var l = new List<Range>();
            var nullFlag = true;
            if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                expression = expression[1..^1];
            }
            foreach (var substr in expression.Trim(' ').Split(' '))
            {
                if (string.IsNullOrWhiteSpace(substr))
                {
                    continue;
                }

                var result = ParseRange(substr, out var r, min, max, out var subReason);
                if (result == true)
                {
                    l.Add(r);
                    nullFlag = false;
                }
                else
                {
                    failureReason = $"In segment \"{substr}\": {subReason}";
                    return false;
                }
            }
            if (nullFlag)
            {
                failureReason = "No valid range segments were found (empty or whitespace only).";
                return null;
            }
            range = new(l.ToArray());
            return true;
        }
        public static bool? ParseRange(string expression, out Range range, double min, double max)
        {
            return ParseRange(expression, out range, min, max, out _);
        }

        /// <summary>
        /// Same as <see cref="ParseRange(string,out Range,double,double)"/> but sets a user-facing reason when parsing fails.
        /// </summary>
        public static bool? ParseRange(string expression, out Range range, double min, double max, out string? failureReason)
        {
            failureReason = null;
            range = null!;
            if (string.IsNullOrWhiteSpace(expression))
            {
                failureReason = "The value is empty or only whitespace.";
                return null;
            }

            if (min > max)
            {
                (max, min) = (min, max);
            }

            expression = expression.Replace(" ", "");

            if (expression == "*")
            {
                range = new Range(min, max);
                return true;
            }
            if (expression == "?")
            {
                range = Range.InvalidRange;
                return true;
            }

            bool hasLeadingPipe = expression.StartsWith("|");
            if (hasLeadingPipe) expression = expression[1..];

            bool hasTrailingPipe = expression.EndsWith("|");
            if (hasTrailingPipe) expression = expression[..^1];

            if (expression.Length == 0)
            {
                failureReason = "The rest of the range expression was empty after handling exclusivity.";
                return null;
            }

            double start, end;
            bool exclusiveStart = false;
            bool exclusiveEnd = false;

            if (expression.EndsWith("+"))
            {
                // Case: "A+" -> [A, max]
                var head = expression.AsSpan(0, expression.Length - 1);
                if (!head.TryParseDouble(out start))
                {
                    failureReason = $"Could not parse a number from \"{head.ToString()}\" (the part before '+').";
                    return null;
                }
                end = max;
                exclusiveStart = hasLeadingPipe;
                hasTrailingPipe = false;
            }
            else
            {
                // Case: A is negative
                if (expression.StartsWith('-'))
                {
                    expression = expression[1..];

                    int separatorIndex = expression.IndexOf('-');
                    if (separatorIndex != -1)
                    {
                        var span1 = expression.AsSpan(0, separatorIndex);
                        var span2 = expression.AsSpan(separatorIndex + 1);

                        if (span2.Length == 0)
                        {
                            // Case: "A-" -> [min, A]
                            if (!span1.TryParseDouble(out end))
                            {
                                failureReason = $"Could not parse a number from \"{span1.ToString()}\" (before '-').";
                                return null;
                            }
                            end *= -1;
                            start = min;
                            exclusiveEnd = hasLeadingPipe;
                            hasTrailingPipe = false;
                        }
                        else
                        {
                            // Case: "A-B"
                            if (!span1.TryParseDouble(out start))
                            {
                                failureReason = $"Could not parse the start value from \"{span1.ToString()}\".";
                                return null;
                            }
                            start *= -1;
                            if (!span2.TryParseDouble(out end))
                            {
                                failureReason = $"Could not parse the end value from \"{span2.ToString()}\".";
                                return null;
                            }
                            exclusiveStart = hasLeadingPipe;
                            exclusiveEnd = hasTrailingPipe;
                        }
                    }
                    else
                    {
                        // Case: "A"
                        if (!expression.AsSpan().TryParseDouble(out start))
                        {
                            failureReason = $"Could not parse a number from \"{expression}\".";
                            return null;
                        }
                        end = (start *= -1);
                        exclusiveEnd = hasLeadingPipe;
                        exclusiveStart = hasTrailingPipe;
                    }
                }
                else
                {
                    int separatorIndex = expression.IndexOf('-');
                    if (separatorIndex != -1)
                    {
                        var span1 = expression.AsSpan(0, separatorIndex);
                        var span2 = expression.AsSpan(separatorIndex + 1);

                        if (span2.Length == 0)
                        {
                            // Case: "A-" -> [min, A]
                            if (!span1.TryParseDouble(out end))
                            {
                                failureReason = $"Could not parse a number from \"{span1.ToString()}\" (before '-').";
                                return null;
                            }
                            start = min;
                            exclusiveEnd = hasLeadingPipe;
                            hasTrailingPipe = false;
                        }
                        else
                        {
                            // Case: "A-B"
                            if (!span1.TryParseDouble(out start))
                            {
                                failureReason = $"Could not parse the start value from \"{span1.ToString()}\".";
                                return null;
                            }
                            if (!span2.TryParseDouble(out end))
                            {
                                failureReason = $"Could not parse the end value from \"{span2.ToString()}\".";
                                return null;
                            }
                            exclusiveStart = hasLeadingPipe;
                            exclusiveEnd = hasTrailingPipe;
                        }
                    }
                    else
                    {
                        // Case: "A"
                        if (!expression.AsSpan().TryParseDouble(out start))
                        {
                            failureReason = $"Could not parse a number from \"{expression}\".";
                            return null;
                        }
                        end = start;
                        exclusiveEnd = hasLeadingPipe;
                        exclusiveStart = hasTrailingPipe;
                    }
                }
            }

            if (start > end)
            {
                (start, end) = (end, start);
                (exclusiveStart, exclusiveEnd) = (exclusiveEnd, exclusiveStart);
            }

            if (!(min <= end && end <= max) || !(min <= start && start <= max))
            {
                failureReason = $"The values ({start} to {end}) are outside the allowed range [{min}, {max}].";
                return false;
            }

            if (start == end && (exclusiveStart || exclusiveEnd))
            {
                failureReason = "A single value cannot use exclusive bounds (|) on the start or end.";
                return false;
            }

            range = new Range(start, end) { ExclusiveStart = exclusiveStart, ExclusiveEnd = exclusiveEnd };
            return true;
        }

        public static bool GetAvailableMaps(MusicInfo musicInfo, out HashSet<int> availableMaps)
        {
            return GetAvailableMaps(musicInfo, out availableMaps, out _);
        }

        public static bool GetAvailableMaps(MusicInfo musicInfo, out HashSet<int> availableMaps, out bool isCustom)
        {
            isCustom = BuiltIns.EvalCustom(musicInfo);
            if (isCustom)
            {
                availableMaps = GetCustomMaps(musicInfo);
            }
            else
            {
                availableMaps = new HashSet<int>();
                for (int i = 1; i < 6; i++)
                {
                    var musicDiff = musicInfo.GetMusicLevelStringByDiff(i, false);
                    if (!(string.IsNullOrEmpty(musicDiff) || musicDiff == "0"))
                    {
                        availableMaps.Add(i);
                    }
                }
            }
            if (availableMaps.Count == 0)
            {
                return false;
            }
            return true;
        }

        private static HashSet<int> GetCustomMaps(MusicInfo musicInfo)
        {
            return ((Album)ModMain.uidToAlbum[musicInfo.uid]).Sheets.Where(x => !string.IsNullOrEmpty(x.Value.Md5)).Select(x => x.Key).ToHashSet();
        }

        public static bool GetMapDifficulties(MusicInfo musicInfo, out string[] difficulties)
        {
            difficulties = null!;
            if (!GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            difficulties = new string[5];

            if (availableMaps.Contains(1))
                difficulties[0] = musicInfo.difficulty1;
            if (availableMaps.Contains(2))
                difficulties[1] = musicInfo.difficulty2;
            if (availableMaps.Contains(3))
                difficulties[2] = musicInfo.difficulty3;
            if (availableMaps.Contains(4))
                difficulties[3] = musicInfo.difficulty4;
            if (availableMaps.Contains(5))
                difficulties[4] = musicInfo.difficulty5;

            return true;
        }

        public static bool GetMapCallbacks(MusicInfo musicInfo, out int[] difficulties)
        {
            difficulties = null!;
            if (!GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            difficulties = new int[5];

            difficulties[0] = availableMaps.Contains(1) ? musicInfo.callBackDifficulty1 : int.MinValue;
            difficulties[1] = availableMaps.Contains(2) ? musicInfo.callBackDifficulty2 : int.MinValue;
            difficulties[2] = availableMaps.Contains(3) ? musicInfo.callBackDifficulty3 : int.MinValue;
            difficulties[3] = availableMaps.Contains(4) ? musicInfo.callBackDifficulty4 : int.MinValue;
            difficulties[4] = availableMaps.Contains(5) ? musicInfo.callBackDifficulty5 : int.MinValue;

            return true;
        }

        internal static LocalInfo GetLocalSafe(this MusicInfo mi, int language)
        {
            return RefreshPatch.localInfos[mi.uid][language];
        }


        internal static bool TryParseCinemaJson(Album album, bool skipUnpackaged = true)
        {
            string path = album.Path;
            JObject items;

            try
            {
                if (album.IsPackaged)
                {
                    using var fs = File.OpenRead(path);
                    using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
                    var entry = archive.GetEntry("cinema.json");
                    if (entry == null)
                        return false;

                    using (var reader = new StreamReader(entry.Open()))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        items = JObject.Load(jsonReader);
                    }

                    string fileName = (string)items["file_name"]!;
                    if (archive.GetEntry(fileName) == null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (skipUnpackaged)
                    {
                        return false;
                    }
                    items = JsonConvert.DeserializeObject<JObject>(
                        File.ReadAllText(Path.Combine(path, "cinema.json"))
                    )!;

                    if (!File.Exists(Path.Combine(path, (string)items["file_name"]!)))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // catch silently
            }

            return false;
        }
        public static bool IsCallable(dynamic obj)
        {
            try
            {
                if (obj is Delegate)
                {
                    return true;
                }
                var engine = IronPython.Hosting.Python.CreateEngine();
                return engine.Operations.IsCallable(obj);
            }
            catch
            {
                return false;
            }
        }
        public static int GetPythonArgCount(dynamic func)
        {
            if (func is PythonFunction pyFunc)
            {
                return pyFunc.__code__.co_argcount;
            }
            return -1;
        }

        public static MemoryStream CopyToMemory(this Stream input)
        {
            var ms = new MemoryStream();
            input.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        public static object? GetCustomAlbumsSave()
        {
            if (!ModMain.CustomAlbumsLoaded)
            {
                return null;
            }
            return GetCustomAlbumsSaveInternal();
        }

        static object? GetCustomAlbumsSaveInternal()
        {
            var saveManagerType = AccessTools.TypeByName("CustomAlbums.Managers.SaveManager");
            var saveDataField = AccessTools.Field(saveManagerType, "SaveData");
            return saveDataField.GetValue(null);
        }

        public static T? MaxByOrDefault<T, TOut>(this IEnumerable<T> values, Func<T, TOut> transformer, T? defaultValue)
        {
            var v = values.ToArray();
            if (v.Length == 0)
            {
                return defaultValue;
            }
            return v.MaxBy(transformer);
        }
        public static string GetFullStackTrace()
        {
            return new StackTrace(true).ToString();
        }
        public static Vector2 GetCaretVectorPosition(this InputField inputField)
        {
            if (inputField == null || inputField.textComponent == null)
                return GetFallback(inputField);

            var text = inputField.textComponent;
            var gen = text.cachedTextGenerator;

            float x;

            if (gen != null && gen.characterCountVisible > 0 && inputField.caretPosition > 0)
            {
                int index = Mathf.Clamp(inputField.caretPosition - 1, 0, gen.characterCountVisible - 1);
                var ch = gen.characters[index];

                x = ch.cursorPos.x + ch.charWidth;
            }
            else
            {
                x = 0f;
            }

            // Convert X from local text space → world
            Vector3 worldX = text.rectTransform.TransformPoint(new Vector3(x, 0f, 0f));

            // Get bottom-left of input field (for Y)
            RectTransform rt = inputField.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];

            // Combine X from caret + Y from input field bottom
            Vector3 finalWorld = new Vector3(worldX.x, bottomLeft.y, 0f);

            Canvas canvas = text.canvas;
            Camera? cam = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            var sc = RectTransformUtility.WorldToScreenPoint(cam, finalWorld);
            return sc;
        }

        private static Vector2 GetFallback(InputField? inputField)
        {
            if (inputField == null)
                return Vector2.zero;

            RectTransform rt = inputField.GetComponent<RectTransform>();
            if (rt == null)
                return Vector2.zero;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];

            Canvas canvas = inputField.GetComponentInParent<Canvas>();
            Camera? cam = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            var sc = RectTransformUtility.WorldToScreenPoint(cam, bottomLeft);
            return sc;
        }

        public static bool TryTimeStringRangeToTimeRange(this string s, [MaybeNullWhen(false)]out Range r)
        {
            r = null;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            if (s.EndsWith("+"))
            {
                if (!TryTimeStringToTimeSpan(s[..^1], out var ts))
                {
                    return false;
                }
                var secs = ts.TotalSeconds;
                r = new Range(secs - 0.5, double.PositiveInfinity);
                return true;
            }

            var split = s.Split('-');
            if (split.Length == 1)
            {
                if (!TryTimeStringToTimeSpan(split[0], out var ts))
                {
                    return false;
                }
                var secs = ts.TotalSeconds;
                r = new Range(secs - 0.5, secs + 0.5)
                {
                    ExclusiveEnd = true
                };
                return true;
            }
            else if (split.Length == 2)
            {
                if (!TryTimeStringToTimeSpan(split[0], out var ts))
                {
                    return false;
                }
                var start = ts.TotalSeconds;
                if (split[1].Length == 0)
                {
                    r = new Range(double.NegativeInfinity, start+0.5)
                    {
                        ExclusiveEnd = true
                    };
                    return true;
                }
                if (!TryTimeStringToTimeSpan(split[1], out ts))
                {
                    return false;
                }
                var end = ts.TotalSeconds;
                if (end < start)
                {
                    (start, end) = (end, start);
                }
                r = new Range(start - 0.5, end + 0.5)
                {
                    ExclusiveEnd = true
                };
                return true;
            }

            return false;
        }

        public static bool TryTimeStringToTimeSpan(this string s, out TimeSpan ts)
        {
            ts = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }


            s = s.Replace(" ", "").ToLowerInvariant();
            long totalTicks = 0;
            int pos = 0;
            while (pos < s.Length)
            {
                // parse number
                int start = pos;
                while (pos < s.Length && char.IsDigit(s[pos]))
                {
                    pos++;
                }
                if (start == pos)
                {
                    return false;
                }
                if (!long.TryParse(s[start..pos], out var value))
                {
                    return false;
                }
                // parse unit
                if (pos >= s.Length)
                {
                    return false;
                }
                char unit = s[pos];
                pos++;
                try
                {
                    totalTicks = unit switch
                    {
                        's' => checked(totalTicks + value * TimeSpan.TicksPerSecond),
                        'm' => checked(totalTicks + value * TimeSpan.TicksPerMinute),
                        'h' => checked(totalTicks + value * TimeSpan.TicksPerHour),
                        'd' => checked(totalTicks + value * TimeSpan.TicksPerDay),
                        'w' => checked(totalTicks + value * TimeSpan.TicksPerDay * 7),
                        _ => throw new Exception(),
                    };
                }
                catch (Exception)
                {
                    return false;
                }
            }
            ts = new TimeSpan(totalTicks);
            return true;
            
        }


        //TODO: romanized contains check method for Korean/Japanese/Chinese names
        //Romanization.NET or Kawazu maybe?
    }
}
