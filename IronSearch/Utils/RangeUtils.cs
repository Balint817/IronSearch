using IronSearch.Records;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Range = IronSearch.Records.Range;

namespace IronSearch.Utils
{
    public static class RangeUtils
    {

        internal static readonly Regex regexBPM = new Regex(@"^[0-9,]*\.?[0-9,]+[^0-9.,][0-9,]*\.?[0-9,]+$");

        internal static readonly Regex regexNonNumeric = new Regex(@"[^0-9.,]");
        public static bool DetectParseBPM(string input, [MaybeNullWhen(false)] out Range range, double min, double max)
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
        public static bool DetectParseBPM(string input, [MaybeNullWhen(false)] out Range range)
        {
            return DetectParseBPM(input, out range, double.NegativeInfinity, double.PositiveInfinity);
        }

        public static bool ParseRange(string expression, out Range range)
        {
            return ParseRange(expression, out range, double.NegativeInfinity, double.PositiveInfinity, out _) ?? false;
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
            if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                expression = expression[1..^1];
            }
            foreach (var substr in expression.Trim(' ').Split(' '))
            {
                if (string.IsNullOrEmpty(substr))
                {
                    continue;
                }

                var result = ParseRange(substr, out var r, min, max, out var subReason);
                if (result == true)
                {
                    l.Add(r);
                }
                else
                {
                    failureReason = $"In segment \"{substr}\": {subReason}";
                    return false;
                }
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
            if (string.IsNullOrEmpty(expression) || string.IsNullOrWhiteSpace(expression))
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
            if (hasLeadingPipe)
            {
                expression = expression[1..];
            }

            bool hasTrailingPipe = expression.EndsWith("|");
            if (hasTrailingPipe)
            {
                expression = expression[..^1];
            }

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
                    failureReason = $"Could not parse a number from \"{head}\" (the part before '+').";
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

        public static bool TryTimeStringRangeToTimeRange(this string s, [MaybeNullWhen(false)] out Range r)
        {
            r = null;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
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
            if (split.Length < 3 && (s.EndsWith('-') || split.Length == 1))
            {
                if (!TryTimeStringToTimeSpan(s.TrimEnd('-'), out var ts))
                {
                    return false;
                }
                var secs = ts.TotalSeconds;
                r = new Range(double.NegativeInfinity, secs + 0.5)
                {
                    ExclusiveEnd = true
                };
                return true;
            }

            if (split.Length == 2)
            {
                if (!TryTimeStringToTimeSpan(split[0], out var ts))
                {
                    return false;
                }
                var start = ts.TotalSeconds;
                if (split[1].Length == 0)
                {
                    r = new Range(double.NegativeInfinity, start + 0.5)
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
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
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
                if (!NumberUtils.TryParseLong(s[start..pos], out var value))
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
                return new Range[1] { new Range(double.NegativeInfinity, range.Start) { ExclusiveEnd = !range.ExclusiveStart } };
            }
            return new Range[2] { new Range(double.NegativeInfinity, range.Start) { ExclusiveEnd = !range.ExclusiveStart }, new Range(range.End, double.PositiveInfinity) { ExclusiveStart = !range.ExclusiveEnd } };
        }
    }
}
