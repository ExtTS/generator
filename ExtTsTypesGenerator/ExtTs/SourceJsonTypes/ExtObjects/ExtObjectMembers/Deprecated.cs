using Newtonsoft.Json;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers {
	public class Deprecated {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
	}
}
