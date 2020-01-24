using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public partial class ResultsGenerator {
		protected static List<string> whileSpaces = new List<string>() {
		//  0    1     2       3         4           5             6               7
			"", "\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t", "\t\t\t\t\t\t", "\t\t\t\t\t\t\t"
		};
		protected string resultFileFullPath;
		protected int whileSpaceLevel = 0;
		protected FileStream resultFileStream;
		protected Dictionary<string, FileStream> resultFileStreams;
		protected StringBuilder resultLines = new StringBuilder();
		protected ResultsGenerator writeResultLine (string line) {
			this.resultLines.AppendLine(
				ResultsGenerator.whileSpaces[this.whileSpaceLevel] + line
			);
			return this;
		}
		protected void openResultFileStream() {
			if (this.processor.OverwriteExistingFiles && File.Exists(this.resultFileFullPath))
				File.Delete(this.resultFileFullPath);
			this.resultFileStream = File.Create(this.resultFileFullPath);
		}
		protected void writeResultLines () {
			this.writeResultFileStream(this.resultLines);
			this.resultLines.Clear();
		}
		protected void writeResultFileStream(StringBuilder sb) {
			this.writeResultFileStream(sb.ToString());
		}
		protected void writeResultFileStream(string str) {
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
			this.resultFileStream.Write(bytes, 0, bytes.Length);
			this.resultFileStream.Flush();
		}
		protected void closeResultFileStream() {
			this.resultFileStream.Flush();
			this.resultFileStream.Close();
		}
	}
}
