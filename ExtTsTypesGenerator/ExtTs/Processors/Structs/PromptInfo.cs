using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.Processors {
	public struct PromptInfo {
		public string Question;
		public Dictionary<string, string> Options;
		public string Default;
	}
}
