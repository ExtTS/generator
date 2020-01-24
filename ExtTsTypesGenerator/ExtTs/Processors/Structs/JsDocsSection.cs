using System;

namespace ExtTs.Processors {
	[Serializable]
	public struct JsDocsSection {
		public JsDocsSectionType Type;
		public string Value;
		public int EndingListIndent;
	}
}
