using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.Enums {
	[Flags]
	[Serializable]
	public enum AccessModifier {
		NONE		= 0,
		PRIVATE		= 1,
		PROTECTED	= 2,
		PUBLIC		= 4,
	}
}
