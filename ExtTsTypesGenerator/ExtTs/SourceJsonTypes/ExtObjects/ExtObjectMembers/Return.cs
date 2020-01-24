using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.Returns;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers {
	public class Return {
		[JsonProperty("type")]
        public string Type { get; set; }
		
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("doc")]
        public string Doc { get; set; }

        [JsonProperty("properties")]
        public IList<Property> Properties { get; set; }

        [JsonProperty("html_type")]
        public string HtmlType { get; set; }
	}
}
