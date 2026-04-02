using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class TitleEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Title";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                var result = new List<string>();

                result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.name));

                if (EvalCustom(musicInfo))
                {
                    result.AddRange(GetStringsCustom_Title(musicInfo));
                }

                for (int i = 1; i <= 5; i++)
                {
                    result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.GetLocalSafe(i).Name));
                }

                return result;
            }
            private static IEnumerable<string> GetStringsCustom_Title(MusicInfo mi)
            {
                return RomanizationHelper.GetAllRomanizations(((Album)ModMain.uidToAlbum[mi.uid]).Info.NameRomanized);
            }
        }
        internal static bool EvalTitle(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<TitleEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
