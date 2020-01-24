using ExtTs.ExtTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public class InvalidJsDocsFormatException:Exception {
		public JsDocsType JsDocsType;
		public string ClassFullName;
		public string MemberName = null;
	}
}
