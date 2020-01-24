using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExtTs.SourceJsonTypes.ExtObjects {
	public class ExtObjectMember {
        [JsonProperty("tagname")]
		public string Tagname { get; set; }

		[JsonProperty("name")]
        public string Name { get; set; }

		[JsonProperty("autodetected")]
        public object Autodetected { get; set; }

		[JsonProperty("files")]
        public object Files { get; set; }

		[JsonProperty("doc")]
        public string Doc { get; set; }
		
        [JsonProperty("params")]
        public IList<MemberParam> Params { get; set; }
		
        [JsonProperty("return")]
        public Return Return { get; set; }
		
        [JsonProperty("type")]
        public string Type { get; set; }
		
        [JsonProperty("throws")]
        public object Throws { get; set; }

        [JsonProperty("default")]
        public string Default { get; set; }

		[JsonProperty("properties")]
        public IList<object> Properties { get; set; }
		
        [JsonProperty("private")]
        public bool? Private { get; set; }
		
        [JsonProperty("protected")]
        public bool? Protected { get; set; }
		
        [JsonProperty("chainable")]
        public bool? Chainable { get; set; }

        [JsonProperty("since")]
        public string Since { get; set; }
		
        [JsonProperty("inheritdoc")]
        public InheritDoc Inheritdoc { get; set; }

		[JsonProperty("static")]
        public bool? Static { get; set; }

		[JsonProperty("deprecated")]
        public Deprecated Deprecated { get; set; }
		
        [JsonProperty("inheritable")]
        public object Inheritable { get; set; }
		
        [JsonProperty("linenr")]
        public object Linenr { get; set; }
		
        [JsonProperty("fires")]
        public object Fires { get; set; }
		
        [JsonProperty("method_calls")]
        public object MethodCalls { get; set; }
		
        [JsonProperty("readonly")]
        public bool? Readonly { get; set; }
		
        [JsonProperty("required")]
		public bool? Required { get; set; }
		
        [JsonProperty("template")]
        public bool? Template { get; set; }
		
        [JsonProperty("id")]
        public string Id { get; set; }
		
        [JsonProperty("owner")]
        public string Owner { get; set; }
		
        [JsonProperty("overrides")]
        public IList<Override> Overrides { get; set; }
		
        [JsonProperty("short_doc")]
        public string ShortDoc { get; set; }
		
        [JsonProperty("html_type")]
        public string HtmlType { get; set; }
	}
}
