using Newtonsoft.Json;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers {
	public class Override {
		[JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
		/*
        [JsonProperty("id")]
        public string Id { get; set; }
		*/
	}
}
