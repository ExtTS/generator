using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Indexer:Member {
		public List<string> KeyTypes;
		public Dictionary<string, ExistenceReason> Types;
		public AccessModifier AccessModJs;
		public AccessModifier AccessModTs;
		public bool IsStatic;
		public bool IsReadOnly;
		public Indexer (
			string name = "", 
			List<string> keyTypes = null, 
			List<string> types = null, 
			string[] doc = null, 
			string ownerFullName = "", 
			bool ownedByCurrent = false
		) :base(name, doc, ownerFullName) {
			this.KeyTypes = keyTypes;
			this.Types = new Dictionary<string, ExistenceReason>();
			foreach (string type in types)
				this.Types.Add(type, new ExistenceReason(ExistenceReasonType.NATURAL));
			this.AccessModJs = AccessModifier.PUBLIC; // Indexer is always public
			this.AccessModTs = AccessModifier.PUBLIC;
			this.IsStatic = false;
			this.IsReadOnly = false;
		}
	}
}
