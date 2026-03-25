using Il2CppAssets.Scripts.Database;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static Dictionary<string, string> validScenes = new Dictionary<string, string>
        {
            {"spacestation", "01"},
            {"space_station", "01"},

            {"retrocity", "02"},

            {"castle", "03"},

            {"rainynight", "04"},
            {"rainy_night", "04"},

            {"candyland", "05"},

            {"oriental", "06"},

            {"letsgroove", "07"},
            {"let'sgroove", "07"},
            {"lets_groove", "07"},
            {"let's_groove", "07"},

            {"touhou", "08"},
            {"gensokyo", "08"},
            {"danmaku", "08"},

            {"djmax", "09"},
            {"graveyard", "09"},

            {"miku", "10"},
            {"hatsunemiku", "10"},
            {"hatsune_miku", "10"},
            {"museland", "10"},
            {"mirrorland", "10"},

            {"warriorland", "11"},

            {"jadetemple", "12"},
            {"jade_temple", "12"},
        };

        internal static bool EvalScene(MusicInfo musicInfo, string value)
        {
            value = value.Trim(' ');
            string sceneFilter = null!;
            switch (value.Length)
            {
                case 0:
                    throw new SearchInputException("received an empty string as 'scene'");
                case 1:
                    if (!char.IsDigit(value[0]))
                    {
                        throw new SearchInputException("expected digit as single character input for 'scene'");
                    }
                    sceneFilter = '0' + value;
                    break;
                case 2:
                    if (!value.All(x => char.IsDigit(x)))
                    {
                        throw new SearchInputException("expected two digits as double character input for 'scene'");
                    }
                    sceneFilter = value;
                    break;
                default:
                    if (value.TryParseInt(out var n))
                    {
                        sceneFilter = value;
                        break;
                    }
                    var matches = validScenes
                        .Where(x => x.Key.Contains(value, StringComparison.OrdinalIgnoreCase))
                        .GroupBy(x => x.Value, x => x.Key).ToDictionary(x => x.Key, x => x.ToArray());
                    if (matches.Count > 1)
                    {
                        var t = matches.Values.Select(x => "("+string.Join(", ", x)+")");
                        throw new SearchInputException($"scene filter search \"{t}\" is ambiguous between {string.Join(", ", t.Reverse().Skip(1).Reverse().Select(x => '"' + x + '"'))} and \"{t.Last()}\"");
                    }
                    else if (matches.Count < 1)
                    {
                        throw new SearchInputException($"scene filter \"{value}\" couldn't be found");
                    }
                    sceneFilter = matches.Keys.First();
                    break;
            }
            if (musicInfo.scene[6..] == sceneFilter)
            {
                return true;
            }
            return false;
        }
        internal static bool EvalScene(MusicInfo musicInfo, int value)
        {
            return EvalScene(musicInfo, value.ToString());
        }
        internal static bool EvalScene(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            var arg1 = varArgs[0];
            switch (arg1)
            {
                case int n:
                    return EvalScene(M.I, n);
                case string s:
                    return EvalScene(M.I, s);
            }
            throw new SearchInputException("invalid scene input, expected scene name, ID, or number");

        }
    }
}
