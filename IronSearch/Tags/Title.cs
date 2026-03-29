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
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.name))
                {
                    yield return item;
                }

                if (EvalCustom(musicInfo))
                {
                    foreach (var item in GetStringsCustom_Title(musicInfo))
                    {
                        yield return item;
                    }
                }
                for (int i = 1; i <= 5; i++)
                {
                    foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).name))
                    {
                        yield return item;
                    }
                }
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
