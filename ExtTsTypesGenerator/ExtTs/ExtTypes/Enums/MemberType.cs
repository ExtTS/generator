using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Enums {
	[Serializable]
	public enum MemberType {
		UNKNOWN						= 0,
		CONFIGURATION				= 2,
		PROPERTY					= 4,
		METHOD						= 8,
		EVENT						= 128,
	}
}
