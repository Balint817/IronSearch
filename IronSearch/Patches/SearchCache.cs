using Il2CppAssets.Scripts.Database;

namespace IronSearch.Patches
{
    internal class SearchCache
    {
        public readonly Dictionary<string, int> Lock = new();
        public readonly Dictionary<string, int> Unlock = new();
        public readonly HashSet<string> PassingUids = new();
        public readonly bool ShouldSort;
        public readonly DateTime? Expiration;

        public SearchCache(IList<MusicInfo> mLock, IList<MusicInfo> mUnlock, bool sort, DateTime? expiration = null)
        {
            Expiration = expiration;
            ShouldSort = sort;
            for (int i = 0; i < mLock.Count; i++)
            {
                var mi = mLock[i];
                Lock[mi.uid] = i;
                PassingUids.Add(mi.uid);
            }
            for (int i = 0; i < mUnlock.Count; i++)
            {
                var mi = mUnlock[i];
                Unlock[mi.uid] = i;
                PassingUids.Add(mi.uid);
            }
        }
    }
}