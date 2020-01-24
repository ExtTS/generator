using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Configuration:Member {
		public List<string> Types;
		public string DefaultValue;
		public bool Required;
		public Configuration (
			string name = "", 
			List<string> types = null, 
			string[] doc = null, 
			string ownerFullName = "", 
			bool ownedByCurrent = false
		):base(name, doc, ownerFullName, ownedByCurrent) {
			this.Types = types;
			this.DefaultValue = null;
			this.Required = false;
		}
		public Configuration Clone() {
			Configuration clone = new Configuration(
				this.Name,
				new List<string>(this.Types),
				this.Doc,
				(this.Owner != null
					? this.Owner.FullName
					: null),
				false
			);
			clone.DefaultValue = this.DefaultValue;
			clone.Required = this.Required;
			return clone;
		}
	}
}
