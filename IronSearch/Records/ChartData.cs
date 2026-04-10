using IronPython.Runtime;
using Newtonsoft.Json;

namespace IronSearch.Records
{
    public readonly record struct NoteInfo(float Time, string Value, string Tone);
    public readonly record struct DialogEventInfo(float Time, string Text);
    public readonly record struct SceneSwitch(float Time, string Scene);

    public class MapData
    {
        public List<NoteInfo> Notes { get; private set; } = new();
        public float StartBPM { get; private set; }
        public string? MD5 { get; private set; }
        public Dictionary<string, List<DialogEventInfo>> DialogEvents { get; private set; } = new();
        public string StartingScene { get; private set; } = "";
        public List<SceneSwitch> SceneChanges { get; private set; } = new();
        public MapData(List<NoteInfo> notes, float bpm, string? md5, Dictionary<string, List<DialogEventInfo>>? dialogEvents, string startingScene, List<SceneSwitch> sceneChanges)
        {
            Notes = notes ?? new();
            StartBPM = bpm;
            MD5 = md5;
            DialogEvents = dialogEvents ?? new();
            StartingScene = startingScene ?? "";
            SceneChanges = sceneChanges ?? new();
        }
        public MapData()
        {

        }
        public Dictionary<string, double> SceneTimes { get; private set; } = new();
        public void InitSceneData()
        {
            if (Notes is null || Notes.Count == 0)
            {
                return;
            }
            var times = new Dictionary<string, double>();
            var lastTime = 0.0;
            var lastScene = StartingScene;
            var sceneChangesSorted = SceneChanges.OrderBy(x => x.Time).ToList();

            float currentTime = 0;
            double accumulatedTime;

            foreach (var sceneChange in SceneChanges)
            {
                var currentScene = sceneChange.Scene;
                if (currentScene == lastScene)
                {
                    continue;
                }

                currentTime = sceneChange.Time;
                if (!times.TryGetValue(lastScene, out accumulatedTime))
                {
                    accumulatedTime = 0;
                }
                times[lastScene] = accumulatedTime + (currentTime - lastTime);
                lastScene = currentScene;
            }

            // account for the last scene until the end of the map

            var maxTime = Notes.Max(x => x.Time);
            if (!times.TryGetValue(lastScene, out accumulatedTime))
            {
                accumulatedTime = 0;
            }
            times[lastScene] = accumulatedTime + (maxTime - currentTime);

            var totalTime = times.Values.Sum();
            foreach (var kvp in times)
            {
                times[kvp.Key] = kvp.Value / totalTime;
            }

            SceneTimes = times;
        }
    }

    public class ChartData
    {
        public Dictionary<int, MapData> Maps { get; private set; } = new();

        [JsonConverter(typeof(TimeSpanTicksConverter))]
        public TimeSpan? MaxLength { get; private set; }
        public bool HasDialogue { get; private set; }
        public Dictionary<string, double> SceneTimes { get; private set; } = new();

        public ChartData(Dictionary<int, MapData> maps, TimeSpan? maxLength)
        {
            Maps = maps ?? new();
            MaxLength = maxLength;
            HasDialogue = Maps.Values.Any(m => m.DialogEvents.Values.Any(d => d.Count > 0));
            InitSceneData();
        }
        public void InitSceneData()
        {
            foreach (var map in Maps.Values)
            {
                map.InitSceneData();
            }
            var validMapSceneTimes = Maps.Values.Select(x => x.SceneTimes).Where(x => x is not null && x.Count != 0).ToArray();

            var sceneTimes = new Dictionary<string, double>();

            foreach (var map in validMapSceneTimes)
            {
                foreach (var kvp in map)
                {
                    if (!sceneTimes.TryGetValue(kvp.Key, out var time))
                    {
                        time = 0;
                    }
                    sceneTimes[kvp.Key] = time + kvp.Value / validMapSceneTimes.Length;
                }
            }
            SceneTimes = sceneTimes;
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
