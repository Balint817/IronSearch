using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;
using IronSearch.Core;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class RecentEvaluator : RangeArgumentEvaluator
        {
            public override string EvaluatorName => "Recent";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                if (EvalCustom(musicInfo))
                {
                    foreach (var item in GetDoublesCustom(musicInfo))
                    {
                        yield return item;
                    }
                    yield break;
                }


                for (int i = 1; i <= 5; i++)
                {
                    var playId = $"{musicInfo.uid}_{i}";
                    var idx = ModMain.playIds.IndexOf(playId);
                    if (idx != -1)
                    {
                        idx += 1; // 1-based index
                        yield return idx;
                        yield return -idx;
                    }
                }
            }

            private IEnumerable<double> GetDoublesCustom(MusicInfo musicInfo)
            {
                var sheets = ((Album)ModMain.uidToCustom[musicInfo.uid]).Sheets;
                for (int i = 1; i <= 5; i++)
                {
                    if (!sheets.TryGetValue(i, out var sheet) || string.IsNullOrEmpty(sheet.Md5))
                    {
                        continue;
                    }
                    var idx = ModMain.playIds.IndexOf(sheet.Md5);
                    if (idx != -1)
                    {
                        idx += 1; // 1-based index
                        yield return idx;
                        yield return -idx;
                    }
                }
            }
        }
        internal static bool EvalRecent(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<RecentEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
