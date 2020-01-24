using ExtTs.ExtTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public partial class ResultsGenerator {
		protected void generateNamespaceOpen (string namespaceName) {
			if (namespaceName.Length == 0) return;
			this.writeResultLine("declare namespace " + namespaceName + " {");
			this.whileSpaceLevel += 1;
		}
		protected void generateNamespaceClose (string namespaceName) {
			if (namespaceName.Length == 0) return;
			this.whileSpaceLevel -= 1;
			if (this.whileSpaceLevel == -1)
				Debugger.Break();
			this.writeResultLine("}");
		}
	}
}
