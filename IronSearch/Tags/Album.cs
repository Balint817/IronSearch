using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static Dictionary<int, List<string>> albumNameLists { get; set; } = null!;
        public class AlbumEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Album";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                if (BuiltIns.albumNameLists.TryGetValue(musicInfo.m_MusicExInfo.m_AlbumUidIndex, out var albumNames))
                {
                    foreach (var item in RomanizationHelper.GetAllRomanizations(albumNames))
                    {
                        yield return item;
                    }
                }
            }
        }
        internal static bool EvalAlbum(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<AlbumEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
