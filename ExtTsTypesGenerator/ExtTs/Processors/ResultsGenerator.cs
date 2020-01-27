using ExtTs.ExtTypes;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.Structs;
using ExtTs.Processors.InheritanceResolvers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtTs.Processors {
	public delegate void resultsGeneratorProgressHandler(int processedClassCount, string processedClass);
	public partial class ResultsGenerator {
		protected internal const string TYPE_DEFS_MASK = "ext-<version>[-<toolkit>]-<package>[-<nsGroup>].d.ts";
		protected internal const string HEADING_URL_PROJECT = "https://github.com/ExtTS";
		protected internal const string HEADING_URL_AUTHOR = "https://github.com/tomFlidr";
		protected Processor processor;
		protected resultsGeneratorProgressHandler progressHandler;
		protected int processedClassCount;
		protected string resultFileNameBase;
		protected string lastNamespace = null;
		protected List<string> generatedFileNames = new List<string>();

		protected internal ResultsGenerator(Processor processor) {
			this.processor = processor;
		}
		internal bool CheckResultsDirectory() {
			bool resultsDirExists = Directory.Exists(this.processor.ResultsDirFullPath);
			// If app is running as service or without user interactive - exit with error:
			if (!resultsDirExists) {
				if (!Environment.UserInteractive) {
					Directory.CreateDirectory(this.processor.ResultsDirFullPath);// it creates directory recursively
					resultsDirExists = Directory.Exists(this.processor.ResultsDirFullPath);
					if (!resultsDirExists)
						return this.addExceptionAndReturnFalse(String.Format(
							"Results directory is not possible to create: `{0}`.", this.processor.ResultsDirFullPath
						));
				} else {
					// Prompt user to create directory if necessary:
					string userResponse = this.processor.UserPromptHandler.Invoke(new PromptInfo() {
						Question = "Results directrory doesn't exist. Do you want to create it?",
						Options = new Dictionary<string, string>() {
							{ "y", "Yes, create it." },
							{ "n", "No, exit." }
						},
						Default = "y"
					});
					if (userResponse == "n")
						return this.addExceptionAndReturnFalse(String.Format(
							"Results directrory doesn't exist: `{0}`.", this.processor.ResultsDirFullPath
						));
					Directory.CreateDirectory(this.processor.ResultsDirFullPath);// it creates directory recursively
				}
			}
			// Add directory access for `Everyone`:
			try {
				DirectoryInfo dInfo = new DirectoryInfo(this.processor.ResultsDirFullPath);
				SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				DirectorySecurity dSecurity = dInfo.GetAccessControl();
				dSecurity.AddAccessRule(new FileSystemAccessRule(
					everyone,
					FileSystemRights.Modify | FileSystemRights.Synchronize,
					InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
					PropagationFlags.None,
					AccessControlType.Allow
				));
				dInfo.SetAccessControl(dSecurity);
			} catch (ArgumentException ex2) {
				this.processor.Exceptions.Add(ex2);
				return false;
			}
			// Check if any of result files exists and exit if necessary:
			// TODO: tohle bude jinak:
			if (!this.processor.OverwriteExistingFiles) {
				DirectoryInfo dInfo = new DirectoryInfo(this.processor.ResultsDirFullPath);
				FileInfo[] resultDirFiles = dInfo.GetFiles();
				FileInfo resultDirFile;
				string resultFileNamePattern = ResultsGenerator.TYPE_DEFS_MASK
					.Replace("<version>[-<toolkit>]", this.processor.VersionWithToolkitStr)
					.Replace("<package>[-<nsGroup>]", "<regExpGroup>")
					.Replace("-", "\\-")
					.Replace(".", "\\.")
					.Replace("<regExpGroup>", "([a-z0-9\\-]+)");
				resultFileNamePattern = "^" + resultFileNamePattern + "$";
				List<string> existingResultFiles = new List<string>();
				for (int i = 0; i < resultDirFiles.Length; i++) {
					resultDirFile = resultDirFiles[i];
					if (Regex.IsMatch(resultDirFile.Name, resultFileNamePattern)) 
						existingResultFiles.Add(resultDirFile.Name);
				}
				if (existingResultFiles.Count > 0)
					return this.addExceptionAndReturnFalse(
						"Some resulting files already exist: \n\t\t`" + String.Join("`\n\t\t`", existingResultFiles) + "`"
					);
			}
			return true;
		}
		protected bool addExceptionAndReturnFalse(string errorMsg) {
			try {
				throw new ArgumentException(errorMsg);
			} catch (Exception ex) {
				this.processor.Exceptions.Add(ex);
			}
			return false;
		}
		protected internal bool GenerateResults(resultsGeneratorProgressHandler progressHandler) {
			this.progressHandler = progressHandler;
			this.processedClassCount = 0;
			this.resultFileNameBase = ResultsGenerator.TYPE_DEFS_MASK
				.Replace("<version>[-<toolkit>]", this.processor.VersionWithToolkitStr);
			string resultFileName;

			// Generate known types:
			if (this.processor.GenerateSingleFile) {
				resultFileName = this.resultFileNameBase.Replace("<package>[-<nsGroup>]", "all");
				this.resultFileFullPath = this.processor.ResultsDirFullPath + "/" + resultFileName;
				this.generatedFileNames.Add(resultFileName);
				this.openResultFileStream();
			}
			this.generateKnown();
			if (this.processor.GenerateSingleFile) 
				this.closeResultFileStream();

			if (this.processor.Store.UnknownTypes.Count > 0) {
				// Generate unknown types:
				resultFileName = this.resultFileNameBase.Replace("<package>[-<nsGroup>]", "unknown");
				this.resultFileFullPath = this.processor.ResultsDirFullPath + "/" + resultFileName;
				this.generatedFileNames.Add(resultFileName);
				this.openResultFileStream();
				this.generateUnknown();
				this.closeResultFileStream();
			}

			if (!this.processor.GenerateSingleFile) {
				// Generate types linking file:
				resultFileName = this.resultFileNameBase.Replace("<package>[-<nsGroup>]", "all");
				this.resultFileFullPath = this.processor.ResultsDirFullPath + "/" + resultFileName;
				this.openResultFileStream();
				this.generateTrippleSlashLinksFile();
				this.closeResultFileStream();
			}
			
			return true;
		}
		protected void generateKnown() {
			this.generateKnownHeading();
			ExtJsPackage package;
			Dictionary<string, List<int>> classIndexesInOptimalizedNamespaces;
			List<int> classIndexes;
			string namespaceGroup;

			Dictionary<ExtJsPackage, string> packageOpenedModules = new Dictionary<ExtJsPackage, string>();
			foreach (var item1 in this.processor.Store.ExtBaseNsClasses) {
				package = item1.Key;
				classIndexesInOptimalizedNamespaces = item1.Value;
				foreach (var item2 in classIndexesInOptimalizedNamespaces) {
					namespaceGroup = item2.Key;
					classIndexes = item2.Value;
					this.lastNamespace = null;

					if (!this.processor.GenerateSingleFile) 
						this.openResultFileStreamForNewNamespaceGroup(package, namespaceGroup);
					
					// Generate standard classes:
					this.generateKnownClasses(classIndexes, ClassType.CLASS_STANDARD);
					// Generate static consts alias classes:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_CONSTANT_ALIAS);
					// Generate alias classes:
					this.generateKnownClasses(classIndexes, ClassType.CLASS_ALIAS);
					// Generate definitions classes:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_DEFINITIONS);
					// Generate classes statics interfaces:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_STATICS);
					// Generate classes methods params configuration objects interfaces:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_METHOD_PARAM_CONF_OBJ);
					// Generate classes methods returns objects interfaces:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_METHOD_RETURN_OBJECT);
					// Generate classes configuration interfaces:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_CONFIGS);
					// Generate classes events interfaces:
					this.generateKnownInterfaces(classIndexes, ClassType.CLASS_EVENTS);
					
					this.generateNamespaceClose(this.lastNamespace);
					this.writeResultLines();
					if (!this.processor.GenerateSingleFile)
						this.closeResultFileStream();
				}
			}
		}
		protected void openResultFileStreamForNewNamespaceGroup(ExtJsPackage extJsPackage, string groupedNamespace) {
			string packageAndNsGroup = extJsPackage.ToString().ToLower().Replace("_", "-");
			if (groupedNamespace.Length > 0)
				packageAndNsGroup += "-" + groupedNamespace.Replace(".", "-");
			string resultFileName = this.resultFileNameBase.Replace(
				"<package>[-<nsGroup>]", packageAndNsGroup
			);
			this.resultFileFullPath = this.processor.ResultsDirFullPath + "/" + resultFileName;
			this.generatedFileNames.Add(resultFileName);
			this.openResultFileStream();
		}
		protected void generateKnownClasses(List<int> classIndexes, ClassType classType) {
			ExtClass extClass;
			foreach (int classIndex in classIndexes) {
				extClass = this.processor.Store.ExtAllClasses[classIndex];
				if (extClass.ClassType == classType) {
					this.closeOldAndOpenNewNamespaceIfNecessary(extClass);
					this.generateClass(extClass);
					this.writeResultLines();
					this.progressHandler.Invoke(this.processedClassCount++, extClass.Name.FullName);
				}
			}
		}
		protected void generateKnownInterfaces(List<int> classIndexes, ClassType classType) {
			ExtClass extClass;
			foreach (int classIndex in classIndexes) {
				extClass = this.processor.Store.ExtAllClasses[classIndex];
				if (extClass.ClassType == classType) {
					this.closeOldAndOpenNewNamespaceIfNecessary(extClass);
					this.generateInterface(extClass);
					this.writeResultLines();
					this.progressHandler.Invoke(this.processedClassCount++, extClass.Name.FullName);
				}
			}
		}
		protected void closeOldAndOpenNewNamespaceIfNecessary (ExtClass extClass) {
			string currentClassNs = extClass.Name.NamespaceName;
			if (this.lastNamespace == null) {
				// first namespace in namespace group file:
				this.generateNamespaceOpen(currentClassNs);
				this.lastNamespace = currentClassNs;
			} else if (currentClassNs != this.lastNamespace) {
				this.generateNamespaceClose(this.lastNamespace);
				this.writeResultLines();
				this.generateNamespaceOpen(currentClassNs);
				this.lastNamespace = currentClassNs;
			}
		}
		protected void generateUnknown() {
			this.generateUnknownHeading();
			List<string> unknownTypeExploded;
			string unknownTypeNamespace;
			string unknownTypeName1;
			int lastItemIndex;
			List<string> unknownTypesInNamespace;
			Dictionary<string, List<string>> unknownNamespacedTypes = new Dictionary<string, List<string>>();
			string unknownType;
			string unknownTypePlaces;
			foreach (var unknownTypeItem in this.processor.Store.UnknownTypes) {
				unknownType = unknownTypeItem.Key;
				unknownTypePlaces = unknownTypeItem.Value;
				if (Types.IsBrowserInternalType(unknownType))
					continue;
				unknownTypeExploded = unknownType
					.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
					.ToList<string>();
				if (unknownTypeExploded.Count == 1) {
					unknownTypeNamespace = "";
					unknownTypeName1 = unknownType;
				} else {
					lastItemIndex = unknownTypeExploded.Count - 1;
					unknownTypeName1 = unknownTypeExploded[lastItemIndex];
					unknownTypeExploded.RemoveAt(lastItemIndex);
					unknownTypeNamespace = String.Join(".", unknownTypeExploded);
				}
				if (!unknownNamespacedTypes.ContainsKey(unknownTypeNamespace))
					unknownNamespacedTypes.Add(unknownTypeNamespace, new List<string>());
				unknownTypesInNamespace = unknownNamespacedTypes[unknownTypeNamespace];
				unknownTypesInNamespace.Add(unknownTypeName1);
			}
			unknownNamespacedTypes = (
				from item in unknownNamespacedTypes
				orderby item.Key ascending
				select item
			).ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>(
				item => item.Key,
				item => item.Value
			);
			string[] unknownTypeNamespaces = unknownNamespacedTypes.Keys.ToArray<string>();
			List<string> unknownTypes;
			bool noNamespace;
			string unknownTypeFullName;
			for (int i = 0; i < unknownTypeNamespaces.Length; i++) {
				unknownTypeNamespace = unknownTypeNamespaces[i];
				unknownTypes = unknownNamespacedTypes[unknownTypeNamespace];
				unknownTypes.Sort();
				noNamespace = unknownTypeNamespace == "";
				if (!noNamespace)
					this.generateNamespaceOpen(unknownTypeNamespace);
				foreach (string unknownTypeName2 in unknownTypes) {
					unknownTypeFullName = noNamespace
						? unknownTypeName2
						: unknownTypeNamespace + "." + unknownTypeName2;
					unknownTypePlaces = this.processor.Store.UnknownTypes[unknownTypeFullName];
					if (noNamespace) {
						if (unknownTypeName2 != "true" && unknownTypeName2 != "false") {
							this.writeResultLine("/** " + unknownTypePlaces + " */");
							this.writeResultLine("declare class " + unknownTypeName2 + " {}");
						}
					} else {
						this.writeResultLine("/** " + unknownTypePlaces + " */");
						if (
							unknownTypeName2 == SpecialsGenerator.STATICS_NAME_ADDITION || 
							unknownTypeName2 == SpecialsGenerator.CONFIGS_NAME_ADDITION || 
							unknownTypeName2 == SpecialsGenerator.EVENTS_NAME_ADDITION
						) {
							this.writeResultLine("interface " + unknownTypeName2 + " {}");
						} else {
							this.writeResultLine("class " + unknownTypeName2 + " {}");
						}
					}
					this.progressHandler.Invoke(this.processedClassCount++, unknownTypeName2);
				}
				if (!noNamespace)
					this.generateNamespaceClose(unknownTypeNamespace);
			}
			this.writeResultLines();
		}

		protected void generateTrippleSlashLinksFile () {
			foreach (string resultFileName in this.generatedFileNames) {
				this.writeResultLine($"/// <reference path=\"./{resultFileName}\" />");
			}
			this.writeResultLines();
		}

	}
}
