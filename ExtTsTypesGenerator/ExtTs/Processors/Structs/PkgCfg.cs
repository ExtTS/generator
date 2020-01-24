using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public struct PkgCfg {
		public string Source;
		public string SourceOverrides;
		public string Classic;
		public string ClassicOverrides;
		public string Modern;
		public string ModernOverrides;
		public bool Optional; // package could exist and doesn't exist in different subversions
		public PkgCfg (
			string src = "",
			string srcOverrides = "",
			string classic = "",
			string classicOverrides = "",
			string modern = "",
			string modernOverrides = "",
			bool optional = false
		) {
			this.Source = src;
			this.SourceOverrides = srcOverrides;
			this.Classic = classic;
			this.ClassicOverrides = classicOverrides;
			this.Modern = modern;
			this.ModernOverrides = modernOverrides;
			this.Optional = optional;
		}
	}
}
