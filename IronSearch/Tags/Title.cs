using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class TitleEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Title";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                // for some reason using yield here causes a System.AccessViolationException in GetLocal, so for now just return a list instead
                var result = new List<string>();

                result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.name));

                if (EvalCustom(musicInfo))
                {
                    result.AddRange(GetStringsCustom_Title(musicInfo));
                }

                for (int i = 1; i <= 5; i++)
                {
                    result.AddRange(RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).name));
                }

                return result;
            }
            static IEnumerable<string> GetStringsCustom_Title(MusicInfo mi)
            {
                return RomanizationHelper.GetAllRomanizations(AlbumManager.LoadedAlbums.Values.First(x => x.Uid == mi.uid).Info.NameRomanized);
            }
        }
        internal static bool EvalTitle(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<TitleEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
