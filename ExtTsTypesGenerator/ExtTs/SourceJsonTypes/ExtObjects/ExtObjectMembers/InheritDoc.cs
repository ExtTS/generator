using Newtonsoft.Json;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers {
	public class InheritDoc {
        [JsonProperty("tagname")]
        public string Tagname { get; set; }

        [JsonProperty("doc")]
        public object Doc { get; set; }
	}
}
