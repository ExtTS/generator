using ExtTs.ExtTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Structs {
	public struct AccessModifiers {
		public static Dictionary<AccessModifier, string> Values = new Dictionary<AccessModifier, string>() {
			{ AccessModifier.NONE						, "" },
			{ AccessModifier.PRIVATE					, "private" },
			{ AccessModifier.PROTECTED					, "protected" },
			{ AccessModifier.PUBLIC						, "public" }
		};
	}
}
