using ExtTs.ExtTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Event:Member {
		public List<Param> Params;
		public Event (
			string name = "", 
			List<Param> methodParams = null,
			string[] doc = null, 
			string ownerFullName = "",
			bool ownedByCurrent = false
		):base(name, doc, ownerFullName, ownedByCurrent)  {
			this.Params = methodParams;
		}

		public Event Clone () {
			List<Param> paramsClone = new List<Param>();
			foreach (Param param in this.Params) 
				paramsClone.Add(new Param(
					param.Name, 
					param.Docs,
					new List<string>(param.Types),
					param.Optional,
					param.IsRest
				));
			Event clone = new Event(
				this.Name,
				paramsClone,
				this.Doc,
				(this.Owner != null
					? this.Owner.FullName
					: null),
				false
			);
			return clone;
		}
	}
}
