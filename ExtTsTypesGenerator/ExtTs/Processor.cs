using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs {
	public delegate void ProcessingInfoHandler(ProcessingInfo processingInfo);
	public delegate string UserPromptHandler(PromptInfo promptInfo);
	public delegate void FinishedHandler(bool success, ProcessingInfo processingInfo);
	public delegate void ReadFinishedHandler(bool success);
	public delegate void JsDuckFinishedHandler(bool success);
	public class Processor {
		protected internal static Processor instance = null;
		protected internal Store Store;
		protected internal Extractor Extractor;
		protected internal Preparer Preparer;
		protected internal JsDuck JsDuck;
		protected internal Reader Reader;
		protected internal Consolidator Consolidator;
		protected internal SpecialsGenerator SpecialsGenerator;
		protected internal TypesChecker TypesChecker;
		protected internal ResultsGenerator ResultsGenerator;
		protected internal ProcessingInfo ProcessingInfo;
		protected internal System.Version Version = null;
		protected internal string VersionStr = null;
		protected internal string VersionWithToolkitStr = null;
		protected internal bool ComunityEdition = false;
		protected internal string Toolkit = null;
		protected internal List<string> Packages = new List<string>();
		protected internal string SourcePackageFullPath = null;
		protected internal string ResultsDirFullPath = null;
		protected internal List<Exception> Exceptions;
		protected internal List<string> JsDuckErrors;
		protected internal ProcessingInfoHandler ProcessingInfoHandler = null;
		protected internal UserPromptHandler UserPromptHandler = null;
		protected internal bool OverwriteExistingFiles = false;
		protected internal string DocsBaseUrl = null;
		protected internal ExtJsPackage[] SuportedPackages = null;
		protected internal bool GenerateJsDocs = true;
		protected internal bool GenerateSingleFile = true;
		protected internal bool DebuggingDisplayJsDuckErrors = false;
		protected internal bool DebuggingTmpDirDataUse = false;
		protected FinishedHandler finishedHandler;
		protected double allClassesCount = 0.0;

		public static Processor CreateNewInstance () {
			Processor.instance = new Processor();
			return Processor.instance;
		}
		protected internal static Processor GetInstance () {
			if (Processor.instance == null)
				Processor.instance = Processor.CreateNewInstance();
			return Processor.instance;
		}
		protected Processor () {
			this.Store = new Store();
			this.Extractor = new Extractor(this);
			this.Preparer = new Preparer(this);
			this.JsDuck = new JsDuck(this);
			this.Reader = new Reader(this);
			this.Consolidator = new Consolidator(this);
			this.SpecialsGenerator = new SpecialsGenerator(this);
			this.TypesChecker = new TypesChecker(this);
			this.ResultsGenerator = new ResultsGenerator(this);
			this.Exceptions = new List<Exception>();
			this.JsDuckErrors = new List<string>();
			this.ProcessingInfo = new ProcessingInfo();
		}

		public List<string> GetSupportedVersions () {
			return VersionSpecsAndFixes.DocsUrls.Keys.ToList<string>();
		}
		public ExtJsPackage[] GetSupportedPackages () {
			if (this.Version == null)
				throw new ArgumentException(
					"To get supported packages, it's necessary to set Ext version first."
				);
			return this.SuportedPackages;
		}
		public Processor SetVersion (string version) {
			int dashPos = version.IndexOf("-");
			if (dashPos > -1) {
				string ceFlag = version.Substring(dashPos + 1).ToLower();
				if (ceFlag == "ce") this.ComunityEdition = true;
				version = version.Substring(0, dashPos);
			}
			this.Version = System.Version.Parse(version);
			if (this.Version.Major < 4 && this.Version.Major > 7)
				throw new ArgumentException(
					"Ext version is not implemented, it only implements versions from 4 to 7 inc."
				);
			this.VersionStr = this.Version.ToString() + (
				this.ComunityEdition ? "-CE" : ""
			);
			this.SuportedPackages = VersionSpecsAndFixes.SuportedPackages[this.Version.Major];
			return this;
		}
		public Processor SetToolkit (ExtJsToolkit extJsToolkit) {
			if (extJsToolkit == ExtJsToolkit.CLASSIC) {
				this.Toolkit = "classic";
			} else if (extJsToolkit == ExtJsToolkit.MODERN) {
				this.Toolkit = "modern";
			} else if (extJsToolkit == (ExtJsToolkit.CLASSIC | ExtJsToolkit.MODERN)) {
				this.addExeption("Processing both toolkits together is not possible.");
			}
			return this;
		}
		public Processor SetPackages (ExtJsPackage extJsPackages) {
			this.Packages = extJsPackages.ToString().Replace(" ","").Replace("_", "-").ToLower().Split(',').ToList<string>();
			return this;
		}
		public Processor SetPackages (ExtJsPackage[] extJsPackages) {
			this.Packages = new List<string>();
			ExtJsPackage extJsPackage;
			string extJsPackageStr;
			for (int i = 0, l = extJsPackages.Length; i < l; i += 1) {
				extJsPackage = extJsPackages[i];
				extJsPackageStr = extJsPackage.ToString();
				extJsPackageStr = extJsPackageStr.Replace(" ", "").Replace("_", "-").ToLower();
				if (ExtJsPackages.Names.ContainsKey(extJsPackageStr))
					this.Packages.Add(extJsPackageStr);
			}
			return this;
		}
		/// <summary>
		/// Set custom docs url with the version and slash at the end like:
		/// https://docs.sencha.com/extjs/1.1.0/ or https://docs.sencha.com/extjs/7.0.0-CE/
		/// </summary>
		public Processor SetCustomDocsBaseUrl (string docsBaseUrl) {
			this.DocsBaseUrl = docsBaseUrl;
			return this;
		}
		public Processor SetGenerateJsDocs (bool generateJsDocs) {
			this.GenerateJsDocs = generateJsDocs;
			return this;
		}
		public Processor SetGenerateSingleFile (bool generateSingleFile) {
			this.GenerateSingleFile = generateSingleFile;
			return this;
		}
		public Processor SetSourcePackageFullPath (string setSourcePackageFullPath) {
			this.SourcePackageFullPath = setSourcePackageFullPath.Replace('\\', '/').TrimEnd('/');
			return this;
		}
		public Processor SetResultsDirFullPath (string resultsDirFullPath, bool overwriteExistingFiles = false) {
			this.ResultsDirFullPath = resultsDirFullPath.Replace('\\', '/').TrimEnd('/');
			this.OverwriteExistingFiles = overwriteExistingFiles;
			return this;
		}
		public Processor SetUserPromptHandler (UserPromptHandler userPromptHandler) {
			this.UserPromptHandler = userPromptHandler;
			return this;
		}
		public Processor SetProcessingInfoHandler (ProcessingInfoHandler processingInfoHandler) {
			this.ProcessingInfoHandler = processingInfoHandler;
			return this;
		}
		public Processor SetDebuggingDisplayJsDuckErrors(bool debuggingDisplayJsDuckErrors = true) {
			this.DebuggingDisplayJsDuckErrors = debuggingDisplayJsDuckErrors;
			return this;
		}
		public Processor SetDebuggingTmpDirDataUse (bool debuggingTmpDirDataUse = true) {
			this.DebuggingTmpDirDataUse = debuggingTmpDirDataUse;
			return this;
		}
		public List<string> GetJsDuckErrors() {
			return this.JsDuckErrors;
		}
		public List<Exception> GetExceptions() {
			return this.Exceptions;
		}
		public void Process(FinishedHandler finishedHandler) {
			this.finishedHandler = finishedHandler;

			if (!this.processStart()) return;

			if (!this.DebuggingTmpDirDataUse) 
				if (!this.processZipPackage()) return;

			if (!this.DebuggingTmpDirDataUse) {
				this.processJsDuckExtraction(delegate (bool processingSuccess) {
					// Do not end processing if there are uknown types - it's better to fix it manually later:
					/*if (!processingSuccess) { 
						this.finishedHandler.Invoke(false);
						return;
					}*/
					this.processReadJsDuckData();
				});
			} else {
				this.processInitParsedDebugTmpData();
				this.processReadJsDuckData();
			}
		}
		protected bool processStart () {
			this.ProcessingInfo.StagesCount = 17;
			this.ProcessingInfo.StageIndex = 1;
			this.ProcessingInfo.Progress = 0.0;
			this.ProcessingInfo.InfoText = "Starting...";
			//
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
			if (!this.setUpCheckInputs()) {
				this.finishedHandler.Invoke(false, this.ProcessingInfo);
				return false;
			}
			if (!this.setUpVersionSpecifics()) {
				this.finishedHandler.Invoke(false, this.ProcessingInfo);
				return false;
			}
			return true;
		}
		protected bool processZipPackage () {
			bool result = true;
			try {
				this.processExtractSourcePackage();
				this.processCompleteSourceFiles();
			} catch (Exception ex) {
				this.Exceptions.Add(ex);
				this.finishedHandler.Invoke(false, this.ProcessingInfo);
				result = false;
			}
			return result;
		} 
		protected void processReadJsDuckData () {
			this.processReadFiles(delegate (bool readSuccess) {
				if (!readSuccess) {
					this.finishedHandler.Invoke(false, this.ProcessingInfo);
					return;
				}
				this.processTypeScriptResults();
			});
		}
		protected void processTypeScriptResults () {
			try {
				this.processConsolidateAlternativeClasses();
				this.processGenerateConfigurationsInterfaces();
				this.processGenerateEventsInterfaces();
				this.processConsolidateParentsCountsOrder();
				this.processGenerateStandardClassesCompatibleMembers();
				this.processGenerateStaticsInterfaces();
				this.processGenerateDefinitionsClasses();
				this.processCheckExistingTypeDefinitions();
				this.processSingletonClassess();
				this.processConsolidateModules();
				this.processGenerateResults();
			} catch (Exception ex) {
				this.Exceptions.Add(ex);
			}
			this.finishedHandler.Invoke(
				this.Exceptions.Count == 0,
				this.ProcessingInfo
			);
		}

		protected internal bool setUpCheckInputs () {
			if (this.Version == null)
				return this.addExeption("No Ext framework version defined.");
			if (!VersionSpecsAndFixes.DocsUrls.ContainsKey(this.VersionStr))
				return this.addExeption("Ext version `"+this.VersionStr+"` is not supported.");
			if (this.Version.Major >= 6 && String.IsNullOrEmpty(this.Toolkit))
				return this.addExeption("No Ext framework toolkit defined.");
			if (this.Packages.Count == 0)
				return this.addExeption("No Ext framework packages defined.");
			if (String.IsNullOrEmpty(this.SourcePackageFullPath))
				return this.addExeption("No source ZIP package defined.");
			if (String.IsNullOrEmpty(this.ResultsDirFullPath))
				return this.addExeption("No results directory defined.");
			if (this.ProcessingInfoHandler == null)
				return this.addExeption("No processing info handler defined.");
			if (this.UserPromptHandler == null)
				return this.addExeption("No user prompt handler defined.");
			return true;
		}
		protected bool setUpVersionSpecifics () {
			this.VersionWithToolkitStr = this.VersionStr;
			if (this.Version.Major >= 6)
				this.VersionWithToolkitStr += "-" + this.Toolkit;
			if (VersionSpecsAndFixes.ClassesFixes.ContainsKey(this.Version.Major)) 
				this.Store.ClassesFixes = VersionSpecsAndFixes.ClassesFixes[this.Version.Major];
			if (VersionSpecsAndFixes.TypesFixes.ContainsKey(this.Version.Major)) 
				this.Store.TypesFixes = VersionSpecsAndFixes.TypesFixes[this.Version.Major];
			if (VersionSpecsAndFixes.SourcesPaths.ContainsKey(this.Version.Major)) 
				this.Store.SourcesPaths = VersionSpecsAndFixes.SourcesPaths[this.Version.Major];
			if (!this.ResultsGenerator.CheckResultsDirectory())
				return false;
			if (!this.DebuggingTmpDirDataUse) 
				if (!this.Extractor.CheckTmpDirectory())
					return false;
			this.Reader.SetUpJsDocsBaseUrl();
			return true;
		}
		protected bool addExeption (string errorMsg) {
			try {
				throw new ArgumentException(errorMsg);
			} catch (ArgumentException ex) {
				this.Exceptions.Add(ex);
			}
			return false;
		}

		protected void progressHandlerExtractingZipFiles (double percentage, int baseDirIndex, int baseDirsCount, string baseDir) {
			this.ProcessingInfo.InfoText = String.Format(
				"Extracting directory {0} of {1}: `{2}`.",
				baseDirIndex, baseDirsCount, baseDir
			);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}
		protected void progressHandlerPreparing (double percentage, int dirTransferIndex, int dirTransfersCounts, string sourceDirRelPath) {
			this.ProcessingInfo.InfoText = String.Format(
				"Preparing JS sources directory {0} of {1}: `{2}`.",
				dirTransferIndex + 1, dirTransfersCounts, sourceDirRelPath
			);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}
		protected void progressHandlerExtractingJsDocs (double percentage, int packageIndex) {
			PackageSource pkgSrc = this.Store.PackagesData[packageIndex];
			this.ProcessingInfo.InfoText = String.Format(
				"Extracting Js Docs data from package {0} of {1}: `{2}`.",
				packageIndex + 1, this.Store.PackagesData.Count, pkgSrc.PackageName
			);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}
		protected void progressHandlerReadFiles (int allFilesCount, int readFilesCount, string readFileName) {
			this.ProcessingInfo.InfoText = String.Format(
				"Read file {0} of {1}: `{2}`.",
				readFilesCount, allFilesCount, readFileName
			);
			double percentage = (((double)readFilesCount / (double)allFilesCount) * 100);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}
		protected void progressHandlerCycleProcessing (int processedClassCount, string processedClass) {
			this.ProcessingInfo.InfoText = String.Format(
				"Processed class {0} of {1}: `{2}`.",
				processedClassCount, this.allClassesCount, processedClass
			);
			double percentage = (((double)processedClassCount / this.allClassesCount) * 100);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}
		protected void progressHandlerOrderingNsGroups (double percentage, int packageIndex) {
			this.ProcessingInfo.InfoText = String.Format(
				"Ordering Js Docs data from package {0} of {1}: `{2}`.",
				packageIndex + 1, this.Store.PackagesData.Count, this.Store.PackagesData[packageIndex].PackageName
			);
			if (percentage > 100.0) percentage = 100.0;
			this.ProcessingInfo.Progress = percentage;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);
		}

		protected void processExtractSourcePackage() {
			this.ProcessingInfo.StageIndex = 1;
			this.ProcessingInfo.StageName = "Extracting JS source files from ZIP package.";
			this.Extractor.ExtractSourcePackage(
				this.progressHandlerExtractingZipFiles
			);
		}
		protected void processCompleteSourceFiles() {
			this.Extractor = null;
			this.ProcessingInfo.StageIndex = 2;
			this.ProcessingInfo.StageName = "Copying JS files and assembling packages.";
			this.Preparer.PreparePackages(
				this.progressHandlerPreparing
			);
		}
		protected void processJsDuckExtraction(JsDuckFinishedHandler jsDuckFinishedHandler) {
			this.Preparer = null;
			this.ProcessingInfo.StageIndex = 3;
			this.ProcessingInfo.StageName = "Extracting JS Docs data by JsDuck (this could take a way long to start).";
			this.JsDuck.ExtractJsDocsFromAllPackageSources(
				this.progressHandlerExtractingJsDocs,
				jsDuckFinishedHandler
			);
		}
		protected void processReadFiles (ReadFinishedHandler readFinishedHandler) {
			this.JsDuck = null;
			this.ProcessingInfo.StageIndex = 4;
			this.ProcessingInfo.StageName = "Reading source directory json files.";
			this.Reader.ReadJsonTypesDirectories(
				this.progressHandlerReadFiles,
				readFinishedHandler
			);
		}
		protected void processConsolidateAlternativeClasses() {
			this.ProcessingInfo.StageIndex = 5;
			this.ProcessingInfo.StageName = "Consolidating classes alternative names.";
			this.allClassesCount = (double)this.Store.ExtStandardClasses.Count;
			this.SpecialsGenerator.GenerateAlternativeClasses(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processGenerateConfigurationsInterfaces() {
			this.ProcessingInfo.StageIndex = 6;
			this.ProcessingInfo.StageName = "Generating special configuration interfaces for class configuration objects.";
			this.allClassesCount = (double)this.Store.ExtStandardClasses.Count;
			this.SpecialsGenerator.GenerateConfigurationsInterfaces(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processGenerateEventsInterfaces() {
			this.ProcessingInfo.StageIndex = 7;
			this.ProcessingInfo.StageName = "Generating special configuration interfaces for listeners.";
			this.allClassesCount = (double)this.Store.ExtStandardClasses.Count;
			this.SpecialsGenerator.GenerateEventsInterfaces(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processConsolidateParentsCountsOrder() {
			this.ProcessingInfo.StageIndex = 8;
			this.ProcessingInfo.StageName = "Consolidating classes parents counts order.";
			this.allClassesCount = (double)this.Store.ExtAllClasses.Count;
			this.Consolidator.ConsolidateParentsCountsOrder(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processGenerateStandardClassesCompatibleMembers () {
			this.ProcessingInfo.StageIndex = 9;
			this.ProcessingInfo.StageName = "Generating classes inheritance TypeScript compatibility members.";
			this.allClassesCount = (double)this.Store.ExtAllClasses.Count;
			this.SpecialsGenerator.GenerateStandardClassesInheritanceCompatibleMembers(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processGenerateStaticsInterfaces() {
			this.ProcessingInfo.StageIndex = 10;
			this.ProcessingInfo.StageName = "Generating special configuration interfaces for statics.";
			this.allClassesCount = (double)this.Store.ExtStandardClasses.Count;
			this.SpecialsGenerator.GenerateStaticsInterfaces(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processGenerateDefinitionsClasses() {
			this.ProcessingInfo.StageIndex = 11;
			this.ProcessingInfo.StageName = "Generating special definition interfaces for class extending.";
			this.allClassesCount = (double)this.Store.ExtStandardClasses.Count;
			this.SpecialsGenerator.GenerateDefinitionsClasses(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processCheckExistingTypeDefinitions () {
			this.SpecialsGenerator = null;
			this.ProcessingInfo.StageIndex = 12;
			this.ProcessingInfo.StageName = "Checking existing or external type definitions.";
			this.allClassesCount = (
				(double)this.Store.ExtClassesWithParent.Count + 
				(double)this.Store.TypesPlaces.Keys.Count
			);
			this.TypesChecker.CheckAllTypesExistence(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processSingletonClassess () {
			this.ProcessingInfo.StageIndex = 13;
			this.ProcessingInfo.StageName = "Processing singleton classes.";
			this.allClassesCount = (double)this.Store.ExtStandardSingletonClasses.Count;
			this.TypesChecker.CheckAllSingletonClasses(
				this.progressHandlerCycleProcessing
			);
		}
		protected void processConsolidateModules() {
			this.TypesChecker = null;
			this.allClassesCount = (double)this.Store.ExtAllClasses.Count;
			this.ProcessingInfo.StageIndex = 14;
			this.ProcessingInfo.StageName = "Optimizing classes into namespace groups.";
			this.Consolidator.OptimalizeNamespacesIntoGroups(
				this.progressHandlerCycleProcessing
			);
			this.ProcessingInfo.StageIndex = 15;
			this.ProcessingInfo.StageName = "Consolidating classes into optimalized namespace groups.";
			this.Consolidator.ConsolidateClassesIntoNsGroups(
				this.progressHandlerCycleProcessing
			);
			this.ProcessingInfo.StageIndex = 16;
			this.ProcessingInfo.StageName = "Ordering classes in optimalized namespace groups.";
			this.Consolidator.OrderClassesInNsGroupsByModuleNames(
				this.progressHandlerOrderingNsGroups
			);
		}
		protected void processGenerateResults() {
			this.Consolidator = null;
			this.ProcessingInfo.StageIndex = 17;
			this.ProcessingInfo.StageName = "Generating result TypeScript type definitions files.";
			this.allClassesCount = (double)(
				(this.Store.ExtAllClasses.Count - this.Store.ExtCallbackClasses.Count)
				+ this.Store.UnknownTypes.Count
			);
			this.ResultsGenerator.GenerateResults(
				this.progressHandlerCycleProcessing
			);
			
			this.ProcessingInfo.InfoText = "Finished.";
			this.ProcessingInfo.Progress = 100.0;
			this.ProcessingInfoHandler.Invoke(this.ProcessingInfo);

			this.Reader = null;
			this.ResultsGenerator = null;
			this.Store = null;
		}
		
		// for debuging purposes to skip first 3 steps with filled tmp dir:
		protected void processInitParsedDebugTmpData () {
			string tmpPath = this.Store.TmpFullPath;
			string jsSrcFullPath;
			string jsonFullPath;
			int jsSourcesCount;
			int jsonDataCount;
			foreach (string packageName in this.Packages) {
				jsSrcFullPath = tmpPath + "/src-" + packageName;
				jsonFullPath = tmpPath + "/json-" + packageName;// + "-small";
				jsSourcesCount = Directory.EnumerateFiles(
					jsSrcFullPath, "*.*", SearchOption.AllDirectories
				).ToArray<string>().Length;
				jsonDataCount = Directory.EnumerateFiles(
					jsSrcFullPath, "*.*", SearchOption.AllDirectories
				).ToArray<string>().Length;
				this.Store.PackagesData.Add(new PackageSource {
					PackageName = packageName,
					Type = ExtJsPackages.Names[packageName],
					JsSourcesDirFullPath = jsSrcFullPath,
					JsSourcesFilesCount = jsSourcesCount,
					JsonDataDirFullPath = jsonFullPath,
					JsonDataCount = jsonDataCount
				});
			}
		}
		
	}
}
