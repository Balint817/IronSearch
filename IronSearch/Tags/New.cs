using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Core;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static List<string>? sortedByLastModified;

        public class NewEvaluator : RangeArgumentEvaluator
        {
            public override string EvaluatorName => "New";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                InitNewIfNeeded();
                var idx = sortedByLastModified!.IndexOf(musicInfo.uid);
                if (idx == -1)
                {
                    yield break;
                }
                yield return (double)idx;
            }
        }
        internal static bool EvalNew(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<NewEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
        internal static void InitNewIfNeeded()
        {
            if (!ModMain.CustomAlbumsLoaded)
            {
                return;
            }
            InitNewIfNeededInternal();
        }
        internal static void InitNewIfNeededInternal()
        {
            sortedByLastModified ??= AlbumManager.LoadedAlbums.Values
                .OrderByDescending(x => File.GetLastWriteTimeUtc(x.Path))
                .Select(x => x.Uid).ToList();
        }
    }
}
