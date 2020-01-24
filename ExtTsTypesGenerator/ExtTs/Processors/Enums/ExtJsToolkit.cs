using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	[Flags]
	public enum ExtJsToolkit {
		UNKNOWN	= 0,
		CLASSIC	= 1,
		MODERN	= 2,
	}
}
