using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class ConfigProperty:Property {
		//public Dictionary<string, ExistenceReason> Types;
		//public string DefaultValue;
		//public AccessModifier AccessModJs;
		//public AccessModifier AccessModTs;
		//public bool IsStatic;
		//public bool IsReadOnly;
		//public bool Renderable;
		//public bool Inherited;
		public bool Required;
		public ConfigProperty (
			string name = "", 
			List<string> types = null, 
			string[] doc = null, 
			string ownerFullName = "", 
			bool ownedByCurrent = false
		):base(name, types, doc, ownerFullName, ownedByCurrent) {
			this.Required = false;
			this.IsStatic = false;
			this.IsReadOnly = false;
		}
		public override Property Clone() {
			ConfigProperty clone = new ConfigProperty(
				this.Name,
				new List<string>(this.Types.Keys.ToList<string>()),
				this.Doc,
				(this.Owner != null
					? this.Owner.FullName
					: null),
				false
			);
			clone.DefaultValue = this.DefaultValue;
			clone.AccessModJs = this.AccessModJs;
			clone.AccessModTs = this.AccessModTs;
			clone.IsStatic = this.IsStatic;
			clone.IsReadOnly = this.IsReadOnly;
			clone.Renderable = this.Renderable;
			clone.Inherited = this.Inherited;
			clone.SingletonInstance = this.SingletonInstance;
			clone.Required = this.Required;
			return clone;
		}
	}
}
