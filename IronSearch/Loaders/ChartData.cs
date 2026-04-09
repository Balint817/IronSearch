namespace IronSearch.Loaders
{
    public readonly record struct NoteInfo(float Time, string Value, string Tone);

    public class MapData
    {
        public List<NoteInfo> Notes { get; }
        public float Bpm { get; }
        public string? Md5 { get; }

        internal MapData(List<NoteInfo> notes, float bpm, string? md5)
        {
            Notes = notes;
            Bpm = bpm;
            Md5 = md5;
        }
    }

    public class ChartData
    {
        public Dictionary<int, MapData> Maps { get; }
        public TimeSpan? Length { get; }

        internal ChartData(Dictionary<int, MapData> maps, TimeSpan? length)
        {
            Maps = maps;
            Length = length;
        }

        internal static ChartData FromLength(TimeSpan length)
        {
            return new ChartData(new Dictionary<int, MapData>(), length);
        }
    }
}
