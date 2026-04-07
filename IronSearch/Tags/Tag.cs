using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using IronSearch.Core;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class TagEvaluator : ContainsEvaluator
        {
            public override string EvaluatorName => "Tag";
            public override IEnumerable<string> GetStrings(MusicInfo musicInfo)
            {
                var uidToInfo = Singleton<ConfigManager>.instance
                    .GetConfigObject<DBConfigMusicSearchTag>(0).m_Dictionary;
                if (!uidToInfo.TryGetValue(musicInfo.uid, out var tagInfo))
                {
                    yield break;
                }

                var tags = tagInfo?.tag;
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        foreach (var item in RomanizationHelper.GetAllRomanizations(tag))
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
        internal static bool EvalTag(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<TagEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
