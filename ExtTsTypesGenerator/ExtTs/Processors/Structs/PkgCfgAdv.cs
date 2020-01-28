using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public class PkgCfgAdv: PkgCfg {
		public new string[] Source;
		public PkgCfgAdv (
			string[] src = null,
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
