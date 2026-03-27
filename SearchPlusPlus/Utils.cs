using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CustomAlbums.Data;
using CustomAlbums.Managers;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Nice.Interface;
using Il2CppPeroTools2.PeroString;
using IronPython.Runtime;
using IronSearch.Records;
using IronSearch.Tags;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using UnityEngine;
using ArgumentException = System.ArgumentException;
using Range = IronSearch.Records.Range;
using PythonExpressionManager;

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
                        try
                        {
                            CompiledScript.ConvertException(response.Exception);
                        }
                        catch (Exception ex)
                        {
                            if (ex is PythonException pe)
                            {
                                MelonLogger.Msg(ConsoleColor.Red, ex.Message);
                            }
                            else
                            {
                                MelonLogger.Msg(ConsoleColor.Red, ex);
                            }
                        }
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

        //no music txt: 30~35
        //docs: 50~60
        public static string AddBreaksPerCount(string text, int lineLength)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            if (lineLength < 1) throw new ArgumentOutOfRangeException(nameof(lineLength));

            string output = "";
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int count = 0;
                var words = line.Split(' ');
                for (int j = 0; j < words.Length; j++)
                {
                    string word = words[j];
                    count += word.Length;
                    if (count > lineLength)
                    {
                        output += "\n";
                        count = word.Length % lineLength;
                    }
                    output += word;
                    if (j != words.Length - 1)
                    {
                        output += " ";
                        count++;
                    }
                }
                if (i != words.Length - 1)
                {
                    output += "\n";
                }
            }
            return output;
        }
        public static MultiRange Invert(this Range range)
        {
            return new MultiRange(range.InvertArray());
        }
        internal static Range[] InvertArray(this Range range)
        {
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

        internal static readonly string Separator = "--------------------------------";

        internal static readonly List<int> DifficultyResultAll = Enumerable.Range(1,5).ToList();
        internal static readonly List<int> DifficultyResultEmpty = new List<int>();
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

        public static T GetResultOrDefault<T>(this IVariable data)
        {
            try
            {
                return VariableUtils.GetResult<T>(data);
            }
            catch (Exception)
            {
                try
                {
                    return (T)(object)((double)(object)default! - 1);
                }
                catch (Exception)
                {
                    return default!;
                }
            }
        }

        public static Highscore ScoresToObjects(this IData data)
        {
            return new Highscore
            {
                Uid = data.fields["uid"].GetResultOrDefault<string>(),
                Evaluate = data.fields["evaluate"].GetResultOrDefault<int>(),
                Score = data.fields["score"].GetResultOrDefault<int>(),
                Combo = data.fields["combo"].GetResultOrDefault<int>(),
                Clear = data.fields["clear"].GetResultOrDefault<int>(),
                AccuracyStr = data.fields["accuracyStr"].GetResultOrDefault<string>(),
                Accuracy = data.fields["accuracy"].GetResultOrDefault<float>()
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


        internal static byte[] GetRequestBytes(string uri)
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.KeepAlive = false;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                return ReadFully(stream);
            }

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


        internal static string RequestToString(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        internal static HttpWebRequest GetRequestInstance(string uri)
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            request.KeepAlive = false;
            request.CookieContainer = null;
            request.PreAuthenticate = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }

        internal static HttpWebRequest GetRequestInstance(string uri, DateTime dt)
        {
            var request = GetRequestInstance(uri);
            request.IfModifiedSince = dt;
            return request;
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
        public static bool DetectParseBPM(string input, out Range range, double min, double max)
        {
            input = input.Trim();
            if (ParseRange(input, out range, min, max) ?? false)
            {
                return true;
            }
            if (!regexBPM.IsMatch(input))
            {
                return false;
            }
            return ParseRange(input.Replace(regexNonNumeric.Match(input).Value, "-"), out range, min, max) ?? false;
        }
        public static bool DetectParseBPM(string input, out Range range)
        {
            return DetectParseBPM(input, out range, double.NegativeInfinity, double.PositiveInfinity);
        }
        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        internal static string GetRequestString(string uri)
        {
            return RequestToString(GetRequestInstance(uri));
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

        public static readonly char[] parseSplitChars = new char[] { '-' };
        public static bool ParseRange(string expression, out Range range)
        {
            return ParseRange(expression, out range, double.NegativeInfinity, double.PositiveInfinity, out _) ?? false;
        }
        public static bool TryParseDouble(this string s, out double x)
        {
            return double.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this string s, out long x)
        {
            return long.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this string s, out int x)
        {
            return int.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this string s, out uint x)
        {
            return uint.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this ReadOnlySpan<char> s, out double x)
        {
            return double.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this ReadOnlySpan<char> s, out long x)
        {
            return long.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this ReadOnlySpan<char> s, out int x)
        {
            return int.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this ReadOnlySpan<char> s, out uint x)
        {
            return uint.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this Span<char> s, out double x)
        {
            return double.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this Span<char> s, out long x)
        {
            return long.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this Span<char> s, out int x)
        {
            return int.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this Span<char> s, out uint x)
        {
            return uint.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x);
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

            if (min > max) (max, min) = (min, max);

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

            // 1. Capture pipe positions
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

            // 2. Parse the logic
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
                exclusiveStart = hasLeadingPipe; // Pipe at start belongs to 'start'
                hasTrailingPipe = false;
            }
            else
            {
                // Case: A is negative
                if (expression.StartsWith('-'))
                {
                    expression = expression[1..];

                    int separatorIndex = expression.IndexOf('-', 1);
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
                            // EXCEPTION: In your ToString, |A- means A (the end) is exclusive
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
                    int separatorIndex = expression.IndexOf('-', 1);
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
                            // EXCEPTION: In your ToString, |A- means A (the end) is exclusive
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

            // 3. Handle potential value swaps (e.g., input "10-5")
            if (start > end)
            {
                (start, end) = (end, start);
                (exclusiveStart, exclusiveEnd) = (exclusiveEnd, exclusiveStart);
            }

            // 4. Bounds Check
            if (!(min <= end && end <= max) || !(min <= start && start <= max))
            {
                failureReason = $"The values ({start} to {end}) are outside the allowed range [{min}, {max}].";
                return false;
            }

            // 5. Logical validity (cannot be exclusive on a single point)
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
            return AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets.Where(x => !string.IsNullOrEmpty(x.Value.Md5)).Select(x => x.Key).ToHashSet();
        }






        public static bool GetMapDifficulties(MusicInfo musicInfo, out string[] availableMaps)
        {
            return GetMapDifficulties(musicInfo, out availableMaps, out _);
        }

        public static bool GetMapDifficulties(MusicInfo musicInfo, out string[] availableMaps, out bool isCustom)
        {
            isCustom = BuiltIns.EvalCustom(musicInfo);
            bool any = false;
            if (isCustom)
            {
                availableMaps = GetCustomDifficulties(musicInfo);
            }
            else
            {
                availableMaps = new string[5];
                for (int i = 1; i < 6; i++)
                {
                    var musicDiff = musicInfo.GetMusicLevelStringByDiff(i, false);
                    if (!(string.IsNullOrEmpty(musicDiff) || musicDiff == "0"))
                    {
                        availableMaps[i - 1] = musicDiff;
                        any = true;
                    }
                }
            }
            return any;
        }

        private static string[] GetCustomDifficulties(MusicInfo musicInfo)
        {
            var maps = new string[5];
            foreach (var item in AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets)
            {
                maps[item.Key] = musicInfo.GetMusicLevelStringByDiff(item.Key, false);
            }
            return maps;
        }



        public static bool GetMapCallbacks(MusicInfo musicInfo, out int[] availableMaps)
        {
            return GetMapCallbacks(musicInfo, out availableMaps, out _);
        }

        public static bool GetMapCallbacks(MusicInfo musicInfo, out int[] availableMaps, out bool isCustom)
        {
            isCustom = BuiltIns.EvalCustom(musicInfo);
            bool any = false;
            if (isCustom)
            {
                availableMaps = GetCustomCallbacks(musicInfo);
            }
            else
            {
                availableMaps = new int[5];
                for (int i = 1; i < 6; i++)
                {
                    var musicDiff = musicInfo.GetCallBackMusicLevelIntByDiff(i, false);
                    if (musicDiff != 0)
                    {
                        availableMaps[i - 1] = musicDiff;
                        any = true;
                    }
                }
            }
            return any;
        }

        private static int[] GetCustomCallbacks(MusicInfo musicInfo)
        {
            var maps = new int[5];
            foreach (var item in AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets)
            {
                maps[item.Key] = musicInfo.GetCallBackMusicLevelIntByDiff(item.Key, false);
            }
            return maps;
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
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.DarkRed, ex.ToString());
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
            Text text = inputField.textComponent;
            int caretIndex = inputField.caretPosition;

            var gen = text.cachedTextGenerator;
            if (gen.characterCount == 0)
                return Vector2.zero;

            caretIndex = Mathf.Clamp(caretIndex, 0, gen.characterCount - 1);

            UICharInfo charInfo = gen.characters[caretIndex];

            Vector2 pos = charInfo.cursorPos;

            Vector3 worldPos = text.transform.TransformPoint(pos);

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);

            return screenPos;
        }

        public static bool TryTimeStringToTicks(this string s, out long l)
        {
            l = 0;
            // AI
            
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
                    //throw new SearchValidationException("expected time offset (integer) as 'modified' argument", "modified");
                }
                if (!long.TryParse(s[start..pos], out var value))
                {
                    return false;
                    //throw new SearchValidationException("invalid number in 'modified' argument", "modified");
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
            l = totalTicks;
            return true;
            
        }
    }
}
