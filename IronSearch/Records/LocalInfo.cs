using Il2CppAssets.Scripts.Database;
using Il2CppPeroPeroGames.GlobalDefines;

namespace IronSearch.Records
{
    public class LocalInfo
    {
        public readonly string author;
        public readonly string name;
        public LocalInfo(LocalALBUMInfo localAlbumInfo)
        {
            name = localAlbumInfo.name ?? "";
            author = localAlbumInfo.author ?? "";
        }

        public override string ToString()
        {
            return $"Title: {name}\n" +
                   $"Author: {author}\n";
        }
    }

}
