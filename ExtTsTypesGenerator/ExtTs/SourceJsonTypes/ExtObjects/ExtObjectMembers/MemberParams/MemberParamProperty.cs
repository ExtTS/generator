using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.MemberParams {
	public class MemberParamProperty {
		[JsonProperty("tagname")]
        public string Tagname { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("doc")]
        public string Doc { get; set; }
		
		[JsonProperty("default")]
        public string Default { get; set; }
		
		[JsonProperty("required")]
        public bool? Required { get; set; }
		
		[JsonProperty("deprecated")]
        public Deprecated Deprecated { get; set; }
		
        [JsonProperty("html_type")]
        public string HtmlType { get; set; }

		// Example:Ext.form.Basic.doAction
        [JsonProperty("properties")]
        public IList<MemberParamProperty> Properties { get; set; }
	}
}
