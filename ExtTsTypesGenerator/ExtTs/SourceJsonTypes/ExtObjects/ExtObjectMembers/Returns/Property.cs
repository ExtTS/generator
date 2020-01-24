using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.Returns {
	public class Property {
		[JsonProperty("tagname")]
        public string Tagname { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("doc")]
        public string Doc { get; set; }

        [JsonProperty("html_type")]
        public string HtmlType { get; set; }
	}
}
