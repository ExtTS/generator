using System;

namespace ExtTs.Processors {
	[Serializable]
	[Flags]
	public enum JsDocsSectionType {
		NONE	= 0,
		TEXT	= 1,
		CODE	= 2
	}
}
