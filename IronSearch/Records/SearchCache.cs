using Il2CppAssets.Scripts.Database;

namespace IronSearch.Records
{
    internal class SearchCache
    {
        public readonly Dictionary<string, int> UIDToIndex = new();
        public readonly bool ShouldSort;
        public readonly DateTime? Expiration;

        public SearchCache(IList<MusicInfo> mUnlock, bool sort, DateTime? expiration = null)
        {
            Expiration = expiration;
            ShouldSort = sort;
            for (int i = 0; i < mUnlock.Count; i++)
            {
                var mi = mUnlock[i];
                UIDToIndex[mi.uid] = i;
            }
        }
    }
}