using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.ExtClasses;
using System;
using System.Collections.Generic;

namespace ExtTs.ExtTypes.Structs {
	[Serializable]
	public struct Members {
		//public Dictionary<string, Dictionary<MemberType, List<string>>> All;
		public Dictionary<string, Member> Configations;
		public Dictionary<string, Member> Properties;
		public Dictionary<string, Member> PropertiesStatic;
		public Dictionary<string, List<Member>> Methods;
		public Dictionary<string, List<Member>> MethodsStatic;
		public Dictionary<string, List<Member>> Events;
		public Dictionary<string, Member> Indexers;
	}
}
