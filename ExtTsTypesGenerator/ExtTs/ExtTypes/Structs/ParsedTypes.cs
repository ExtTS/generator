using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Structs {
	/**
	 * `List<string>` types for single param or return type or property (in 4 different collections).
	 */
	[Serializable]
	public struct ParsedTypes {
		public List<string> CfgOrProp;
		public List<string> MethodOrEventParam;
		public List<string> MethodOrEventSpreadParam;
		public List<string> MethodOrEventReturn;
	}
}
