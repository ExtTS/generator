using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExtTs.SourceJsonTypes.ExtObjects {
	public class Aliases {
        [JsonProperty("widget")]
        public IList<string> Widget { get; set; }
        [JsonProperty("request")]
        public IList<string> Request { get; set; }
	}
}
