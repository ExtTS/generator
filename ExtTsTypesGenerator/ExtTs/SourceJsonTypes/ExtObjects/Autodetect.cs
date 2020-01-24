using Newtonsoft.Json;

namespace ExtTs.SourceJsonTypes.ExtObjects {
	public class Autodetect {
		[JsonProperty("aliases")]
        public bool? Aliases { get; set; }
        [JsonProperty("alternateClassNames")]
        public bool? AlternateClassNames { get; set; }
        [JsonProperty("extends")]
        public bool? Extends { get; set; }
        [JsonProperty("mixins")]
        public bool? Mixins { get; set; }
        [JsonProperty("requires")]
        public bool? Requires { get; set; }
        [JsonProperty("uses")]
        public bool? Uses { get; set; }
        [JsonProperty("members")]
        public bool? Members { get; set; }
        [JsonProperty("code_type")]
        public bool? CodeType { get; set; }
	}
}
