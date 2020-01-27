
using ExtTs;
using ExtTs.ExtTypes.Structs;
using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cli {
	class Program {
		static void Main(string[] args) {
			// THIS CLI APP IS FOR DEVELOPMENT PURPOSES ONLY:
			Processor proc = Processor.CreateNewInstance()
				//.SetDebuggingTmpDirDataUse(true) // to skip first 3 slowest steps and use TMP dir data
				.SetVersion("6.0.1")
				.SetToolkit(ExtJsToolkit.MODERN)
				.SetPackages(
					//ExtJsPackage.CORE
					//ExtJsPackage.CORE | ExtJsPackage.UX
					//ExtJsPackage.CORE | ExtJsPackage.CHARTS
					// all:
					ExtJsPackage.CORE|ExtJsPackage.AMF|ExtJsPackage.CHARTS|ExtJsPackage.GOOGLE|ExtJsPackage.LEGACY|ExtJsPackage.SOAP|ExtJsPackage.UX
				)
				.SetGenerateJsDocs(true)
				.SetGenerateSingleFile(true)
				.SetSourcePackageFullPath(@"c:/Users/Administrator/Desktop/Ext.TS/gpl-zips/ext-6.0.1-gpl.zip")
				.SetResultsDirFullPath(@"c:/Users/Administrator/Desktop/Ext.TS/example-project-601-classic/js/types/", true)
				.SetUserPromptHandler(Program.userPrompt)
				.SetProcessingInfoHandler(Program.displayProgress);
			proc.Process(delegate (bool success, ProcessingInfo processingInfo) {
				List<Exception> errors = proc.GetExceptions();
				string title = (success || processingInfo.StageIndex == processingInfo.StagesCount)
					? (errors.Count == 0
						? "Processing finished."
						: "Processing finished with following errors:")
					: "Processing can NOT start due to those errors:";
				Program.displayResult(title, errors);
			});
			Console.ReadLine();
		}
		protected static string userPrompt(PromptInfo promptInfo) {
			Console.Clear();
			Console.WriteLine(promptInfo.Question);
			Console.WriteLine();
			string optionLine;
			foreach (var item in promptInfo.Options) {
				optionLine = $"\t'{item.Key}'\t– {item.Value}";
				if (item.Key == promptInfo.Default)
					optionLine += " (default)";
				Console.WriteLine(optionLine);
			}
			Console.WriteLine("\nType one of the following options and press Enter key. \nPress only Enter key to choose default option.\n");
			string resultValue = Console.ReadLine().Trim();
			if (String.IsNullOrEmpty(resultValue)) 
				resultValue = promptInfo.Default;
			if (promptInfo.Options.ContainsKey(resultValue))
				return resultValue;
			return Program.userPrompt(promptInfo);
		}
		protected static void displayProgress (ProcessingInfo processingInfo) {
			//return;
			Console.Clear();
			Console.WriteLine(String.Format("Stage {0} of {1}: {2}", 
				processingInfo.StageIndex, processingInfo.StagesCount, processingInfo.StageName
			));
			Console.WriteLine(processingInfo.InfoText);
			Console.WriteLine(processingInfo.Progress.ToString("0.00") + " %");
		}
		protected static void displayResult (string title, List<Exception> errors) {
			//return;
			Console.Clear();
			Console.WriteLine(title);
			if (errors.Count == 0)
				return;
			foreach (Exception err in errors) {
				if (err is ArgumentException) {
					Console.WriteLine("");
					Console.WriteLine("\t"+err.Message);
				} else {
					Console.WriteLine("");
					Console.WriteLine(Desharp.Debug.Dump(err, new Desharp.DumpOptions {
						SourceLocation = true,
						Return = true,
					}));
				}
			}
		}
	}
}
