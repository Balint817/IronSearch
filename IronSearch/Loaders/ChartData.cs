namespace IronSearch.Loaders
{
    public readonly record struct NoteInfo(float Time, string Value, string Tone);
    //public readonly record struct DialogEventInfo(float Time, string Text);

    public class MapData
    {
        public List<NoteInfo> Notes { get; } = new();
        public float Bpm { get; }
        public string? Md5 { get; }

        public MapData(List<NoteInfo> notes, float bpm, string? md5)
        {
            Notes = notes ?? new();
            Bpm = bpm;
            Md5 = md5;
        }
        public MapData()
        {
            
        }
    }

    public class ChartData
    {
        public Dictionary<int, MapData> Maps { get; } = new();
        public TimeSpan? MaxLength { get; }

        public ChartData(Dictionary<int, MapData> maps, TimeSpan? length)
        {
            Maps = maps ?? new();
            MaxLength = length;
        }
        public ChartData()
        {
            
        }
    }
}
