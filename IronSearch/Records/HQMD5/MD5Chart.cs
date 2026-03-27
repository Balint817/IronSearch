namespace IronSearch.Records.HQMD5
{
    public class MD5Chart
    {
        public MD5Analytics analytics { get; set; } = null!;
        public string title { get; set; } = null!;
        public string titleRomanized { get; set; } = null!;
        public string artist { get; set; } = null!;
        public string charter { get; set; } = null!;
        public string bpm { get; set; } = null!;
        public double length { get; set; }
        public string owner { get; set; } = null!;
        public List<string> sheets { get; set; } = null!;
        public bool? ranked { get; set; }
        public List<string> tags { get; set; } = null!;
        public DateTime uploadedAt { get; set; }
        public DateTime rankedAt { get; set; }
        public string id { get; set; } = null!;
    }
}
