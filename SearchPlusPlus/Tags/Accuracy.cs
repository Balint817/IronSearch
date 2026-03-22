using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        // TODO:
        // find alternative method for customs

        static readonly Range evalAccArgCount = new(1, 2);
        static readonly int[] evalAccDiffs = new[] { 1, 2, 3, 4 };

        internal static bool EvalAccuracy(MusicInfo musicInfo, string value)
        {
            var splitValue = value.Trim(' ').Split(' ').Where(x => x != "").ToArray();

            Range accRange;
            Range diffRange;

            if (splitValue.Length == 1)
            {
                if (!Utils.ParseRange(splitValue[0], out accRange))
                {
                    throw new SearchInputException($"failed to parse range \"{splitValue[0]}\"");
                }

                //accRange.Update(accRange.Start / 100, accRange.End / 100);
                return EvalAccuracy(musicInfo, accRange.AsMultiRange(), MultiRange.InvalidRange);
            }
            else if (splitValue.Length == 2)
            {
                if (splitValue[0] == "?")
                {
                    throw new SearchInputException("wildcard '?' is not allowed in this context");
                }

                if (!Utils.ParseRange(splitValue[0], out accRange))
                {
                    throw new SearchInputException($"failed to parse range \"{splitValue[0]}\"");
                }

                //accRange.Update(accRange.Start / 100, accRange.End / 100);

                if (!Utils.ParseRange(splitValue[1], out diffRange))
                {
                    throw new SearchInputException($"failed to parse range \"{splitValue[1]}\"");
                }
                if (diffRange == Range.InvalidRange)
                {
                    // wildcard → highest diff only
                    if (!Utils.GetAvailableMaps(musicInfo, out var maps) || !maps.Intersect(evalAccDiffs).Any())
                        return false;

                    var max = maps.Intersect(evalAccDiffs).Max();
                    return EvalAccuracy(musicInfo, accRange, new Range(max, max));
                }

                return EvalAccuracy(musicInfo, accRange, diffRange);
            }

            throw new SearchInputException($"invalid input \"{value}\"");
        }

        internal static bool EvalAccuracy(MusicInfo musicInfo, Range accRange)
        {
            return EvalAccuracy(musicInfo, accRange.AsMultiRange(), MultiRange.InvalidRange);
        }

        internal static bool EvalAccuracy(MusicInfo musicInfo, Range accRange, Range diffRange)
        {
            return EvalAccuracy(musicInfo, accRange.AsMultiRange(), diffRange.AsMultiRange());
        }

        internal static bool EvalAccuracy(MusicInfo musicInfo, MultiRange accRange, MultiRange diffRange)
        {
            if (accRange == MultiRange.InvalidRange)
            {
                throw new SearchInputException("wildcard '?' or invalid range is not allowed in this context");
            }
            foreach (var r in accRange.Ranges)
            {
                r.Update(r.Start / 100, r.End / 100);
            }


            if (!Utils.GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            if (diffRange != MultiRange.InvalidRange)
            {
                availableMaps = availableMaps.Where(x => diffRange.Contains(x)).ToHashSet();
            }
            else
            {
                if (!availableMaps.Intersect(evalAccDiffs).Any())
                {
                    return false;
                }
                availableMaps = new HashSet<int>() { availableMaps.Intersect(evalAPDiffs).Max() };
            }

            if (availableMaps.Count == 0)
            {
                return false;
            }

            foreach (var diff in availableMaps)
            {
                string s = musicInfo.uid + "_" + diff;

                if (!RefreshPatch.highScores.Any(x => x.Uid == s && accRange.Contains(x.Accuracy)))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool EvalAccuracy(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotInRange(varArgs, evalAccArgCount);

            switch (varArgs.Length)
            {
                case 1:
                    {
                        switch (varArgs[0])
                        {
                            case string s:
                                return EvalAccuracy(M.I, s);
                            case Range r:
                                return EvalAccuracy(M.I, r);
                            case MultiRange mr:
                                return EvalAccuracy(M.I, mr, MultiRange.InvalidRange);
                        }
                    }
                    break;

                case 2:
                    {
                        if (varArgs[0] is string s)
                        {
                            if (!Utils.ParseRange(s, out var r))
                            {
                                throw new SearchInputException($"failed to parse range \"{varArgs[0]}\"");
                            }
                            if (r == Range.InvalidRange)
                            {
                                throw new SearchInputException("wildcard '?' is not allowed in this context");
                            }
                            varArgs[0] = r;
                        }

                        if (varArgs[1] is string s2)
                        {
                            if (!Utils.ParseRange(s2, out var r))
                            {
                                throw new SearchInputException($"failed to parse range \"{varArgs[0]}\"");
                            }
                            varArgs[1] = r;
                        }


                        if (varArgs[0] is Range r0)
                        {
                            varArgs[0] = r0.AsMultiRange();
                        }

                        if (varArgs[1] is Range r1)
                        {
                            varArgs[1] = r1.AsMultiRange();
                        }

                        if (varArgs[0] is MultiRange mr1 && varArgs[1] is MultiRange mr2)
                            return EvalAccuracy(M.I, mr1, mr2);
                    }
                    break;
            }
            throw new SearchInputException("invalid argument types for accuracy");

        }




    }
}
