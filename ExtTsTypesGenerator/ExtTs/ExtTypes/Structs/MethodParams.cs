using ExtTs.ExtTypes.ExtClasses;
using System;
using System.Collections.Generic;

namespace ExtTs.ExtTypes.Structs {
	[Serializable]
	public struct FuncParamsSyntaxCollections {
		public List<Param> StandardParamsSyntax;
		public List<Param> SpreadParamsSyntax;
		public bool SpreadParamFound;
	}
}
