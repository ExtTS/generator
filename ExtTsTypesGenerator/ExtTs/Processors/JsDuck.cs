using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Text;

namespace ExtTs.Processors {
	public delegate void progressHandlerExtractingJsDocs(double percentage, int packageIndex);
	public class JsDuck {
		protected static string ASM = "jsduck-6.0.0-beta.exe";
		protected static List<string> ExternalTypes = new List<string>() {
			"XMLHttpRequest", 
			"HTMLElement", 
			"XMLElement", 
			"Float32Array",
			"Uint8Array", 
			"TextNode", 
			"Event", 
			"Window", 
			"NodeList", 
			"Promise", 
			"CSSStyleSheet", 
			"CSSStyleRule", 
			"google.maps.Map", 
			"google.maps.LatLng", 
			"FileList",
			"DataTransfer",
			"CanvasGradient",
			"CanvasRenderingContext2D",
			"FileSystem",
			"FileError"
		};
		protected static List<string> NotUnknownTypes = new List<string>() {
			"Boolean", "Boollean",
			"String", "Object", "Array", "Number", "Function",
			"HTMLELement", "HtmlElement"
		};
		protected Processor processor;
		protected progressHandlerExtractingJsDocs progressHandlerExtractingJsDocs;
		protected JsDuckFinishedHandler jsDuckFinishedHandler;
		protected int packageIndex;
		protected PackageSource packageData;
		protected string baseCommandPath;
		protected Process execProc = null;
		//protected StringBuilder execProcStdOut;
		protected StringBuilder execProcStdErr;
		protected List<Exception> exceptions = new List<Exception>();
		protected List<string> jsDuckErrors = new List<string>();

		public JsDuck(Processor processor) {
			this.processor = processor;
		}
		internal void ExtractJsDocsFromAllPackageSources(
			progressHandlerExtractingJsDocs progressHandlerExtractingJsDocs, JsDuckFinishedHandler jsDuckFinishedHandler
		) {
			this.progressHandlerExtractingJsDocs = progressHandlerExtractingJsDocs;
			this.jsDuckFinishedHandler = jsDuckFinishedHandler;
			this.packageIndex = 0;
			this.processNextPackage();
		}
		protected void processNextPackage () {
			this.packageData = this.processor.Store.PackagesData[this.packageIndex];
			this.completeProcessingCommand();
			this.runCommandInBackground();
			this.monitorProgress();
			this.completeResultFilesCount();
			this.completeResultErrors();

			this.packageIndex += 1;
			if (this.packageIndex < this.processor.Store.PackagesData.Count) {
				this.processNextPackage();
			} else {
				if (this.exceptions.Count > 0)
					this.processor.Exceptions.AddRange(this.exceptions);
				if (this.jsDuckErrors.Count > 0)
					this.processor.JsDuckErrors.AddRange(this.jsDuckErrors);
				this.jsDuckFinishedHandler.Invoke(
					this.exceptions.Count == 0 && this.jsDuckErrors.Count == 0
				);
			}
		}
		protected void completeProcessingCommand() {
			string packageName = this.packageData.PackageName;
			string tmpFullPath = this.processor.Store.TmpFullPath;
			
			string jsonOutput = "json-" + packageName;
			this.packageData.JsonDataDirFullPath = tmpFullPath + "/" + jsonOutput;
			
			int procCount = Environment.ProcessorCount > 1
				? Environment.ProcessorCount - 1
				: 1;

			this.packageData.CommandFullPath = this.processor.Store.RootDirFullPath + "/" + JsDuck.ASM;
			List<string> cmdArgs = new List<string>() {
				$"\"{this.packageData.JsSourcesDirFullPath}\""
			};
			if (this.packageData.Type == ExtJsPackage.CORE) 
				cmdArgs.Add("--builtin-classes"); // Includes docs for JavaScript builtins.
			cmdArgs.AddRange(new string[] {
				"--warnings=-nodoc,-dup_member,-link_ambiguous",
				"--external=" + String.Join(",", JsDuck.ExternalTypes),
				"--ignore-global",
				"--export=full",
				//"--processes=" + procCount, // not for windows!
				$"--output \"{this.packageData.JsonDataDirFullPath}\"",
			});
			this.packageData.CommandArgs = String.Join(" ", cmdArgs);
		}
		protected void runCommandInBackground() {
			this.execProc = new Process();
			ProcessStartInfo procStartInfo = new ProcessStartInfo(
				this.packageData.CommandFullPath,
				this.packageData.CommandArgs
			);
			procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			procStartInfo.UseShellExecute = false;
			procStartInfo.WorkingDirectory = this.processor.Store.RootDirFullPath;
			procStartInfo.RedirectStandardError = true;
			procStartInfo.RedirectStandardOutput = true;
			procStartInfo.CreateNoWindow = true;
			//this.execProcStdOut = new StringBuilder();
			this.execProcStdErr = new StringBuilder();
			this.execProc.StartInfo = procStartInfo;
			this.execProc = Process.Start(procStartInfo);
			this.execProc.Start();
			//this.execProc.BeginOutputReadLine();
			this.execProc.BeginErrorReadLine();
			/*this.execProc.OutputDataReceived += new DataReceivedEventHandler(
				delegate (object o, DataReceivedEventArgs e) {
					this.execProcStdOut.AppendLine(e.Data);
				}
			);*/
			this.execProc.ErrorDataReceived += new DataReceivedEventHandler(
				delegate (object o, DataReceivedEventArgs e) {
					this.execProcStdErr.AppendLine(e.Data);
				}
			);
		}
		protected void monitorProgress() {
			DirectoryInfo di;
			FileInfo[] files;
			//Console.Clear();
			double percentage = 0.0;
			while (true) {
				if (Directory.Exists(this.packageData.JsonDataDirFullPath)) { 
					di = new DirectoryInfo(this.packageData.JsonDataDirFullPath);
					files = di.GetFiles();
					if (files.Length > 0) { 
						percentage = (double)files.Length / (double)this.packageData.JsSourcesFilesCount * 100.0;
					} else {
						percentage = 0.0;
					}
				}
				if (this.execProc.HasExited) {
					percentage = 100.0;
					break;
				}
				this.progressHandlerExtractingJsDocs.Invoke(percentage, this.packageIndex);
				Thread.Sleep(500);
			}
		}
		protected void completeResultFilesCount () {
			string[] allResultFiles = Directory.EnumerateFiles(
				this.packageData.JsonDataDirFullPath, "*.*", SearchOption.AllDirectories
			).ToArray<string>();
			this.packageData.JsonDataCount = allResultFiles.Length;
		}
		protected void completeResultErrors () {
			string allLines = this.execProcStdErr.ToString();
			List<string> resultLines = allLines
				.Split(new char[] { '\n' })
				.ToList<string>();
			int pos;
			string unknownTypesTitle = "Unknown type ";
			List<string> rawUnknownTypes;
			string rawUnknownType;
			List<string> unknownTypes;
			bool errorType;
			string resultLine;
			for (int i = 0; i < resultLines.Count; i++) {
				resultLine = resultLines[i];
				pos = resultLine.IndexOf("Warning: ");
				errorType = false;
				if (pos == -1) {
					pos = resultLine.IndexOf("Error: ");
					errorType = true;
				}
				if (pos == -1) continue;
				pos = resultLine.IndexOf(unknownTypesTitle);
				if (pos == -1 && !errorType) continue;
				unknownTypes = new List<string>();
				if (errorType) {
					this.jsDuckErrors.Add(
						$"JS Duck error: ({resultLine})."
					);
				} else {
					rawUnknownTypes = resultLine
						.Substring(pos + unknownTypesTitle.Length)
						.Replace(", ", "/").Replace(",", "/").Replace("|", "/").Replace("\r", "")
						.Split(new char[] { '/' })
						.ToList<string>();
					for (int j = 0; j < rawUnknownTypes.Count; j++) {
						rawUnknownType = rawUnknownTypes[j]
							.Replace("[", "")
							.Replace("]", "");
						if (
							JsDuck.ExternalTypes.Contains(rawUnknownType) ||
							JsDuck.NotUnknownTypes.Contains(rawUnknownType) || (
								rawUnknownType.Length > 4 &&
								rawUnknownType.Substring(0, 4) == "Ext."
							)
						) continue;
						unknownTypes.Add(rawUnknownType);
					}
					if (unknownTypes.Count > 0)
						this.jsDuckErrors.Add(
							$"JS Duck warning - unknown type(s) found - `{String.Join(", ", unknownTypes)}`: ({resultLine})."
						);
				}
			}
		}
	}
}