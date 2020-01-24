using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.Processors {
	public class PackageSource {
		public ExtJsPackage Type;
		public string PackageName;
		public string JsSourcesDirFullPath;
		public int JsSourcesFilesCount;
		public string JsonDataDirFullPath;
		public int JsonDataCount;
		public string CommandFullPath;
		public string CommandArgs;
	}
}
