using System;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Member {
		public string Name;
		public string[] Doc;
		public NameInfo Owner;
		public bool OwnedByCurrent;
		public string[] Deprecated;
		public Member (string name = "", string[] doc = null, string ownerFullName = "", bool ownedByCurrent = false) {
			this.Name = name;
			this.Doc = doc;
			this.Owner = ownerFullName.Length > 0
				? new NameInfo(ownerFullName)
				: null;
			this.OwnedByCurrent = ownedByCurrent;
			this.Deprecated = null;
		}
	}
}
