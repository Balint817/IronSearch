using Il2CppGameLogic;
using Newtonsoft.Json;
using static CustomAlbums.Data.Bms;

namespace IronSearch.Records
{
    public readonly record struct NoteInfo(float Time, string Value, string Tone);
    public readonly record struct DialogEventInfo(float Time, string Text);

    public class MapData
    {
        public List<NoteInfo> Notes { get; private set; } = new();
        public float StartBPM { get; private set; }
        public string? MD5 { get; private set; }
        public Dictionary<string, List<DialogEventInfo>> DialogEvents { get; private set; } = new();
        public string StartingScene { get; private set; } = "";
        public MapData(List<NoteInfo> notes, float bpm, string? md5, Dictionary<string, List<DialogEventInfo>>? dialogEvents, string startingScene)
        {
            Notes = notes ?? new();
            StartBPM = bpm;
            MD5 = md5;
            DialogEvents = dialogEvents ?? new();
            StartingScene = startingScene ?? "";
        }
        public MapData()
        {

        }
    }

    public class ChartData
    {
        public Dictionary<int, MapData> Maps { get; private set; } = new();

        [JsonConverter(typeof(TimeSpanTicksConverter))]
        public TimeSpan? MaxLength { get; private set; }
        [JsonIgnore]
        public bool HasDialogue => Maps.Values.Any(m => m.DialogEvents.Values.Any(d => d.Count > 0));

        public ChartData(Dictionary<int, MapData> maps, TimeSpan? maxLength)
        {
            Maps = maps ?? new();
            MaxLength = maxLength;
        }
        public ChartData()
        {

        }
    }

    internal class TimeSpanTicksConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? ReadJson(JsonReader reader, Type objectType, TimeSpan? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var ticks = serializer.Deserialize<long?>(reader);
            return ticks.HasValue ? new TimeSpan(ticks.Value) : null;
        }

        public override void WriteJson(JsonWriter writer, TimeSpan? value, JsonSerializer serializer)
        {
            if (value.HasValue)
            {
                writer.WriteValue(value.Value.Ticks);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
