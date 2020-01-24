using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.MemberParams;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers {
	public class MemberParam {
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

        [JsonProperty("default")]
        public string Default { get; set; }
		
        [JsonProperty("optional")]
        public bool? Optional { get; set; }
		
        [JsonProperty("properties")]
        public IList<MemberParamProperty> Properties { get; set; }
	}
}
