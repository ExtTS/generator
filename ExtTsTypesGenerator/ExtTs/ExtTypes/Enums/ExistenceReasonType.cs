using System;

namespace ExtTs.ExtTypes.Enums {
	[Flags]
	[Serializable]
	public enum ExistenceReasonType {
		UNKNOWN				= 0,
		NATURAL				= 1,
		COMPATIBLE_TYPES	= 2,
		COMPATIBLE_CHAIN	= 4,
	}
}
