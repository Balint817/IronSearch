using Il2CppAssets.Scripts.Database;
using IronSearch.Exceptions;
using IronSearch.Utils;

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

        internal static bool EvalScene(MusicInfo musicInfo, string value, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            value = value.Trim(' ');
            string sceneFilter = null!;
            switch (value.Length)
            {
                case 0:
                    throw new SearchValidationException("Scene filter cannot be empty.", "Scene", varArgs, varKwargs);
                case 1:
                    if (!char.IsDigit(value[0]))
                    {
                        throw new SearchValidationException("For a one-character scene filter, use a single digit (1–9).", "Scene", varArgs, varKwargs);
                    }
                    sceneFilter = '0' + value;
                    break;
                case 2:
                    if (!value.All(x => char.IsDigit(x)))
                    {
                        throw new SearchValidationException("For a two-character scene filter, use two digits (e.g. 01).", "Scene", varArgs, varKwargs);
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
                        throw new SearchValidationException($"Scene filter \"{value}\" matches more than one scene name; be more specific.", "Scene", varArgs, varKwargs);
                    }
                    else if (matches.Count < 1)
                    {
                        throw new SearchValidationException($"No scene matches \"{value}\".", "Scene", varArgs, varKwargs);
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
        internal static bool EvalScene(MusicInfo musicInfo, int value, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return EvalScene(musicInfo, value.ToString(), varArgs, varKwargs);
        }
        internal static bool EvalScene(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "Scene", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Scene", varArgs, varKwargs);
            var arg1 = varArgs[0];
            switch (arg1)
            {
                case int n:
                    return EvalScene(M.I, n, varArgs, varKwargs);
                case string s:
                    return EvalScene(M.I, s, varArgs, varKwargs);
            }
            throw new SearchWrongTypeException("a scene name, numeric ID, or string digits", arg1?.GetType(), "Scene", varArgs, varKwargs);

        }
    }
}
