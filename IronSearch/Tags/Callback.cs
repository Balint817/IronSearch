using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalCallbackRange = new(1, 2);

        internal static bool EvalCallback(MusicInfo musicInfo, int n)
        {
            return EvalCallback(musicInfo, new Range(n));
        }

        internal static bool EvalCallback(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw SearchParseException.ForRange(s, "Callback()", "a callback level range");
            }
            return EvalCallback(musicInfo, r.AsMultiRange());
        }

        internal static bool EvalCallback(MusicInfo musicInfo, Range r)
        {
            return EvalCallback(musicInfo, r.AsMultiRange());
        }

        internal static bool EvalCallback(MusicInfo musicInfo, MultiRange mr)
        {
            return EvalCallback(musicInfo, mr, MultiRange.FullRange);
        }

        internal static bool EvalCallback(MusicInfo musicInfo, MultiRange callbackRange, MultiRange levelRange)
        {
            Utils.GetMapCallbacks(musicInfo, out var callbacks);

            if (callbacks.Length == 0)
            {
                return false;
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);

            if (levelRange == MultiRange.InvalidRange)
            {
                availableMaps = new() { availableMaps.Max() };
            }
            else
            {
                availableMaps = availableMaps.Where(x => levelRange.Contains(x)).ToHashSet();
            }

            foreach (var i in availableMaps)
            {
                var value = callbacks[i];
                if (callbackRange.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalCallback(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, evalCallbackRange);

            if (varArgs.Length == 1)
            {
                switch (varArgs[0])
                {
                    case int n:
                        return EvalCallback(M.I, n);
                    case string s:
                        return EvalCallback(M.I, s);
                    case Range r:
                        return EvalCallback(M.I, r);
                    case PythonRange pr:
                        return EvalCallback(M.I, (Range)pr);
                    case MultiRange mr:
                        return EvalCallback(M.I, mr);
                    default:
                        throw new SearchWrongTypeException("an integer, range string, Python range, or multi-range for callback", varArgs[0]?.GetType(), "Callback()");
                }
            }

            if (varArgs.Length == 2)
            {
                MultiRange callbackRange;
                switch (varArgs[0])
                {
                    case int n:
                        callbackRange = new Range(n).AsMultiRange();
                        break;
                    case string s:
                        if (!Utils.ParseRange(s, out var parsedRange))
                        {
                            throw SearchParseException.ForRange(s, "Callback()", "a callback level range");
                        }
                        callbackRange = parsedRange.AsMultiRange();
                        break;
                    case Range r:
                        callbackRange = r.AsMultiRange();
                        break;
                    case PythonRange pr:
                        callbackRange = ((Range)pr).AsMultiRange();
                        break;
                    case MultiRange mr:
                        callbackRange = mr;
                        break;
                    default:
                        throw new SearchWrongTypeException("an integer, range string, Python range, or multi-range for the callback value", varArgs[0]?.GetType(), "Callback()");
                }

                MultiRange levelRange;
                switch (varArgs[1])
                {
                    case int n:
                        levelRange = new Range(n).AsMultiRange();
                        break;
                    case string s:
                        if (!Utils.ParseRange(s, out var parsedRange))
                        {
                            throw SearchParseException.ForRange(s, "Callback()", "a callback level range");
                        }
                        levelRange = parsedRange.AsMultiRange();
                        break;
                    case Range r:
                        levelRange = r.AsMultiRange();
                        break;
                    case PythonRange pr:
                        levelRange = ((Range)pr).AsMultiRange();
                        break;
                    case MultiRange mr:
                        levelRange = mr;
                        break;
                    default:
                        throw new SearchWrongTypeException("an integer, range string, Python range, or multi-range for the chart/level index", varArgs[1]?.GetType(), "Callback()");
                }

                return EvalCallback(M.I, callbackRange, levelRange);
            }

            throw new SearchValidationException("Invalid callback arguments.", "Callback()");
        }
    }
}