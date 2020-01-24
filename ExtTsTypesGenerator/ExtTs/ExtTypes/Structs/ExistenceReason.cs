using ExtTs.ExtTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Structs {
	[Serializable]
	public struct ExistenceReason {
		public ExistenceReasonType Type;
		public string CompatibilityReasonClassFullName;
		public ExistenceReason (
			ExistenceReasonType type = ExistenceReasonType.UNKNOWN, 
			string compatibilityReasonClassFullName = null
		) {
			this.Type = type;
			this.CompatibilityReasonClassFullName = compatibilityReasonClassFullName;
		}
	}
}
