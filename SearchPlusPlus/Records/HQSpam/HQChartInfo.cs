namespace IronSearch.Records
{
    public class HQChartInfo
    {
        public string title { get; set; } = null!;
        public string titleRomanized { get; set; } = null!;
        public string artist { get; set; } = null!;
        public string charter { get; set; } = null!;
        public string bpm { get; set; } = null!;
        public double length { get; set; }
        public string owner { get; set; } = null!;
        public List<HQSheet> sheets { get; set; } = null!;
        public bool ranked { get; set; }
        public HQAnalytics analytics { get; set; } = null!;
        public List<string> tags { get; set; } = null!;
        public DateTime uploadedAt { get; set; }
        public DateTime? rankedAt { get; set; } = null!;
        public int __v { get; set; }
        public int likesCount { get; set; }
        public string id { get; set; } = null!;
    }
}
