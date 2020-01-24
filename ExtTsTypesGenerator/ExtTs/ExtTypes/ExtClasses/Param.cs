using System;
using System.Collections.Generic;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public struct Param {
		public string Name;
		public string[] Docs;
		public List<string> Types;
		public bool Optional;
		public bool IsRest;
		public Param (
			string paramName =  "", 
			string[] docs = null,
			List<string> types = null, 
			bool optional = false, 
			bool isRest = false
		) {
			this.Name = paramName;
			this.Docs = docs;
			this.Types = types;
			this.Optional = optional;
			this.IsRest = isRest;
		}
		public Param Clone () {
			return new Param(
				this.Name,
				this.Docs,
				new List<string>(this.Types),
				this.Optional,
				this.IsRest
			);
		}
	}
}
