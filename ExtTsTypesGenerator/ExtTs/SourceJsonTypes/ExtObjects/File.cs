using Newtonsoft.Json;

namespace ExtTs.SourceJsonTypes.ExtObjects {
	public class File {
        [JsonProperty("filename")]
        public string filename { get; set; }
        [JsonProperty("linenr")]
        public int Line { get; set; }
	}
}
