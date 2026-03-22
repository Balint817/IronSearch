namespace IronSearch.Records.HQMD5
{

    public class MD5Response
    {
        public MD5Chart chart { get; set; } = null!;
        public string difficulty { get; set; } = null!;
        public int rankedDifficulty { get; set; }
        public string charter { get; set; } = null!;
        public string hash { get; set; } = null!;
        public int map { get; set; }
        public string id { get; set; } = null!;
    }
}
