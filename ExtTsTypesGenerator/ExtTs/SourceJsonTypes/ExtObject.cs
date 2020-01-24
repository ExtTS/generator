using ExtTs.SourceJsonTypes.ExtObjects;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExtTs.SourceJsonTypes
{
    public class ExtObject {
        [JsonProperty("tagname")]
		public string Tagname { get; set; }

		[JsonProperty("name")]
        public string Name { get; set; }

		[JsonProperty("autodetect")]
        public object Autodetect { get; set; }

		[JsonProperty("files")]
        public IList<File> Files { get; set; }

		[JsonProperty("doc")]
        public string Doc { get; set; }

        [JsonProperty("aliases")]
        public Aliases Aliases { get; set; }

		[JsonProperty("alternateClassNames")]
        public IList<string> AlternateClassNames { get; set; }

        [JsonProperty("extends")]
        public string Extends { get; set; }

		[JsonProperty("mixins")]
        public IList<object> Mixins { get; set; }

        [JsonProperty("requires")]
        public IList<string> Requires { get; set; }

		[JsonProperty("uses")]
        public IList<object> Uses { get; set; }
		
		[JsonProperty("singleton")]
        public bool? Singleton { get; set; }
		
		[JsonProperty("private")]
        public bool? Private { get; set; }
		
		[JsonProperty("enum")]
        public object Enum { get; set; }

		[JsonProperty("members")]
        public List<ExtObjectMember> Members { get; set; }

		[JsonProperty("code_type")]
        public string CodeType { get; set; }

		[JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("component")]
        public bool Component { get; set; }

		[JsonProperty("short_doc")]
        public string ShortDoc { get; set; }

		[JsonProperty("deprecated")]
        public Deprecated Deprecated { get; set; }

		/*
		[JsonProperty("override")]
        public object Override { get; set; }
		
        [JsonProperty("inheritable")]
        public object Inheritable { get; set; }

        [JsonProperty("inheritdoc")]
        public object Inheritdoc { get; set; }

		[JsonProperty("private")]
        public object Private { get; set; }

		[JsonProperty("linenr")]
        public int Linenr { get; set; }

        [JsonProperty("statics")]
        public Statics Statics { get; set; }

        [JsonProperty("superclasses")]
        public IList<string> Superclasses { get; set; }

        [JsonProperty("subclasses")]
        public IList<string> Subclasses { get; set; }

        [JsonProperty("mixedInto")]
        public IList<object> MixedInto { get; set; }

        [JsonProperty("parentMixins")]
        public IList<string> ParentMixins { get; set; }
		*/
    }
}
