using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public partial class ResultsGenerator {
		protected void generateKnownHeading() {
			this.resultLines
				.AppendLine("/**")
				.AppendLine(" * Ext.JS TypeScript definitions")
				.AppendLine(" * ")
				.AppendLine(" * @version " + this.processor.VersionStr);
			if (this.processor.Toolkit != null)
				this.resultLines.AppendLine(" * @toolkit " + this.processor.Toolkit);
			foreach (string packageName in this.processor.Packages)
				this.resultLines.AppendLine(" * @package " + packageName);
			this.resultLines
				.AppendLine(" * ")
				.AppendLine(" * @date    " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
				.AppendLine(" * @url     " + ResultsGenerator.HEADING_URL_PROJECT)
				.AppendLine(" * @author  " + ResultsGenerator.HEADING_URL_AUTHOR)
				.AppendLine(" */")
				.AppendLine("");
		}
		protected void generateUnknownHeading() {
			this.resultLines
				.AppendLine("/**")
				.AppendLine(" * Ext.JS TypeScript empty definitions")
				.AppendLine(" * for unknown types in packages:")
				.AppendLine(" * ")
				.AppendLine(" * @version " + this.processor.VersionStr);
			if (this.processor.Toolkit != null)
				this.resultLines.AppendLine(" * @toolkit " + this.processor.Toolkit);
			this.resultLines
				.AppendLine(" * @package unknown")
				.AppendLine(" */")
				.AppendLine("");
		}
	}
}
