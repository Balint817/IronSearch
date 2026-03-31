using Il2CppAssets.Scripts.Database;

namespace IronSearch.Records
{
    public class LocalInfo
    {
        public readonly string Author;
        public readonly string Name;
        public LocalInfo(LocalALBUMInfo localAlbumInfo)
        {
            Name = localAlbumInfo.name ?? "";
            Author = localAlbumInfo.author ?? "";
        }

        public override string ToString()
        {
            return $"Title: {Name}\n" +
                   $"Author: {Author}\n";
        }
    }

}
