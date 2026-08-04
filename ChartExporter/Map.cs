using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChartExporter
{
    public class Chart
    {
        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore, MemberSerialization = MemberSerialization.OptOut)]
        public class Info
        {
            [JsonProperty("name", Required =Required.DisallowNull)]
            public string name;

            [JsonProperty("author", Required = Required.DisallowNull)]
            public string author;

            [JsonProperty("bpm", Required = Required.DisallowNull)]
            public string bpm;

            [JsonProperty("scene", Required = Required.DisallowNull)]
            public string scene;

            [JsonProperty("searchTags", NullValueHandling = NullValueHandling.Ignore)]
            public string[] searchTags;

            [JsonProperty("levelDesigner", Required = Required.DisallowNull)]
            public string levelDesigner;

            public string levelDesigner1 = "?";

            [JsonProperty("levelDesigner2", Required = Required.DisallowNull)]
            public string levelDesigner2;

            public string levelDesigner3 = "?";

            public string difficulty1;

            [JsonProperty("difficulty2", Required = Required.DisallowNull)]
            public string difficulty2;

            public string difficulty3 = "0";

            public string unlockLevel;

            public bool? streamer;
        }
        public class Map
        {
            
        }
        public Info info = new();
        public Map map = new();
    }
}
