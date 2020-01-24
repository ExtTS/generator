using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using System;
using System.Collections.Generic;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Callback: ExtClass {
		public List<Param> Params;
		public List<string> ReturnTypes;
		public string[] ReturnDocs;
		public Callback (
			string fullName = "", 
			string[] docs = null,
			string[] returnDocs = null,
			List<Param> funcParams = null,
			List<string> returnTypes = null
		) {
			this.Name = new NameInfo(fullName);
			this.Extends = new NameInfo("Function");
			this.Docs = docs;
			this.ClassType = ClassType.CLASS_METHOD_PARAM_CALLBACK;
			this.Params = funcParams;
			this.ReturnTypes = returnTypes;
			this.ReturnDocs = returnDocs;
		}
	}
}
