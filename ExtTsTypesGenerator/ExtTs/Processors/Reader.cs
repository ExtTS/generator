using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ExtTs.SourceJsonTypes.ExtObjects;
using ExtTs.SourceJsonTypes;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers;

namespace ExtTs.Processors {
	public delegate void readProgressHandler(int allFilesCount, int readFilesCount, string readFileName);
	public partial class Reader {
		internal const string NS_EVENTS_PARAMS = ".eventsParams.";
		internal const string NS_METHOD_STATIC_PARAMS = ".staticMethodParams.";
		internal const string NS_METHOD_PARAMS = ".methodParams.";
		internal const string NS_METHOD_STATIC_CALLBACK_PARAMS = ".staticMethodCallbackParams.";
		internal const string NS_METHOD_CALLBACK_PARAMS = ".methodCallbackParams.";
		internal const string NS_METHOD_RETURN_OBJECT = ".methodReturns.";
		internal const string NS_METHOD_STATIC_RETURN_OBJECT = ".staticMethodReturns.";
		protected Processor processor;
		protected TypeDefinitionsParser typesParser;
		protected string[] extTypeJsonFiles;
		protected volatile int readingIndex;
		protected volatile int readFilesCount;
		protected string jsDocsUrlBase;
		protected bool jsDocsLinksNewerFormat;
		protected object readingLock;
		protected object progressHandlingLock;
		protected int readingThreadsCount;
		protected List<Thread> readingThreads;
		protected readProgressHandler progressHandler;
		protected ReadFinishedHandler readFinishedHandler;
		private PackageSource currentPackage;
		protected int allReadFilesCount;
		protected int packageIndex;
		//protected string srcJson;

		protected internal Reader(Processor processor) {
			this.processor = processor;
			this.typesParser = new TypeDefinitionsParser(processor);
			this.readFilesCount = 0;
			this.readingIndex = 0;
			this.jsDocsUrlBase = "";
			this.jsDocsLinksNewerFormat = false;
			this.readingThreadsCount = Environment.ProcessorCount;
			this.readingThreadsCount = 1; // debuging
			this.readingThreads = new List<Thread>();
			this.readingLock = new object { };
			this.progressHandlingLock = new object { };
		}
		protected internal void ReadJsonTypesDirectories(
			readProgressHandler progressHandler,
			ReadFinishedHandler readFinishedHandler
		) {
			this.progressHandler = progressHandler;
			this.readFinishedHandler = readFinishedHandler;
			this.packageIndex = 0;
			this.allReadFilesCount = 0;
			foreach (PackageSource pkgSrc in this.processor.Store.PackagesData)
				this.allReadFilesCount += pkgSrc.JsonDataCount;
			this.readNextJsonTypesDir();
		}
		protected void readNextJsonTypesDir () {
			Thread readingThread;
			this.currentPackage = this.processor.Store.PackagesData[this.packageIndex];
			this.extTypeJsonFiles = Directory.GetFiles(
				this.currentPackage.JsonDataDirFullPath, "*.json",
				SearchOption.TopDirectoryOnly
			);
			this.readingIndex = 0;
			this.readFilesCount = 0;
			if (this.readingThreadsCount == 1) { 
				this.readNextFile(0);
			} else {
				for (int i = 0; i < this.readingThreadsCount; i++) {
					readingThread = this.getNewReadingThread(i);
					this.readingThreads.Add(readingThread);
					readingThread.Start();
				}
			}
		}
		protected Thread getNewReadingThread (int threadIndex) {
			Thread readingThread;
			readingThread = new Thread(new ThreadStart(delegate {
				this.readNextFile(threadIndex);
			}));
			readingThread.IsBackground = true;
			readingThread.Priority = ThreadPriority.BelowNormal;
			return readingThread;
		}
		protected void lastFileInPackageFinished(int threadIndex) {
			if (this.readingThreadsCount > 1)
				for (int i = 0; i < Math.Min(this.readingThreadsCount, this.readingThreads.Count); i++) 
					if (i != threadIndex)
						this.readingThreads[i].Abort();
			this.readingThreads = new List<Thread>();
			this.packageIndex += 1;
			if (this.packageIndex < this.processor.Store.PackagesData.Count) {
				this.readNextJsonTypesDir();
			} else {
				this.processor.Store.TypesPlaces = new System.Collections.Concurrent.ConcurrentDictionary<string, List<TypeDefinitionSource>>(
					(
						from item in this.processor.Store.TypesPlaces
						orderby item.Key ascending
						select item
					).ToDictionary<KeyValuePair<string, List<ExtTypes.Structs.TypeDefinitionSource>>, string, List<ExtTypes.Structs.TypeDefinitionSource>>(
						item => item.Key,
						item => item.Value
					)
				);
				this.readFinishedHandler.Invoke(true);
			}
		}
		protected void readNextFile (int threadIndex) {
			string extTypeJsonFullPath = null;
			string extTypeJsonFileName;
			int lastSlashPos;
			int readFilesCount;
			lock (this.readingLock) {
				if (this.readingIndex < this.extTypeJsonFiles.Length) { 
					extTypeJsonFullPath = this.extTypeJsonFiles[this.readingIndex];
					this.readingIndex += 1;
				}
			}
			if (String.IsNullOrEmpty(extTypeJsonFullPath))
				return;
				//Thread.CurrentThread.Abort();
			extTypeJsonFullPath = extTypeJsonFullPath.Replace('\\', '/');
			lastSlashPos = extTypeJsonFullPath.LastIndexOf("/");
			if (lastSlashPos > -1) {
				extTypeJsonFileName = extTypeJsonFullPath.Substring(lastSlashPos + 1);
			} else {
				extTypeJsonFileName = extTypeJsonFullPath;
			}
			this.readSourceTypeJsonFile(extTypeJsonFullPath);
			lock (this.readingLock) {
				this.readFilesCount += 1;
				readFilesCount = this.readFilesCount;
			}
			lock (this.progressHandlingLock)
				this.progressHandler.Invoke(
					this.allReadFilesCount, readFilesCount, extTypeJsonFileName
				);
			if (readFilesCount == this.extTypeJsonFiles.Length) {
				//Debugger.Break();
				this.lastFileInPackageFinished(threadIndex);
			} else {
				this.readNextFile(threadIndex);
			}
		}
		protected void readSourceTypeJsonFile (string extTypeJsonFullPath) {
			string rawFileContent;
			ExtObject extObject;
			ExtClass extClass;
			try {
				rawFileContent = System.IO.File.ReadAllText(extTypeJsonFullPath);
				extObject = this.parseDescriptingJsonTypesFile(rawFileContent);
				if (InheritanceResolvers.Types.IsBrowserInternalType(extObject.Name)) return;
				//this.srcJson = extTypeJsonFullPath.Substring(this.processor.Store.TmpFullPath.Length);
				extClass = this.readParsedJson(extObject);
				this.processor.Store.AddExtClass(extClass, true);
			} catch (Exception err) {
				this.processor.Exceptions.Add(err);
			}
		}
		protected ExtObject parseDescriptingJsonTypesFile(string rawFileContent) {
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
			jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
			jsonSettings.Error = new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>(
				delegate (object o, Newtonsoft.Json.Serialization.ErrorEventArgs errorEventArgs) {
					this.processor.Exceptions.Add(errorEventArgs.ErrorContext.Error);
				}
			);
			return (ExtObject)JsonConvert.DeserializeObject(
				rawFileContent, typeof(ExtObject), jsonSettings
			);
		}
		protected ExtClass readParsedJson(ExtObject extObject) {
			// If there is no parent class (in classes like: Ext, Ext.Base, Ext.Array, Ext.JSON...), 
			// than everything is from TypeScript `Object` interface:
			string extends = extObject.Extends != null
				? extObject.Extends
				: "";
			ExtClass result = new ExtClass(
				extObject.Name, extends, this.readJsDocs(
					extObject.Doc, JsDocsType.CLASS, extObject.Name
				)
			);
			result.Package = this.currentPackage.Type;
			//result.SrcJson = this.srcJson;
			result.Name.PackagedNamespace = this.GetPackagedNamespaceFromFullClassName(
				result.Name.FullName
			);
			if (extObject.Singleton.HasValue && extObject.Singleton.Value == true) {
				result.Singleton = true;
			}
			result.Deprecated = this.readJsDocsDeprecated(
				extObject.Deprecated, result.Name.FullName
			);
			if (extObject.Private.HasValue) {
				result.Private = extObject.Private.Value;
				//Debugger.Break();
				//Console.WriteLine(result.Name.FullName);
			}
			result.ClassType = ClassType.CLASS_STANDARD;
			if (
				extObject.AlternateClassNames != null && 
				extObject.AlternateClassNames.Count > 0
			) 
				foreach (string alternateClassName in extObject.AlternateClassNames) 
					result.AlternativeNames.Add(
						new NameInfo(alternateClassName)
					);
			this.readClassMembers(extObject, result);
			return result;
		}
		protected void readClassMembers (ExtObject extObject, ExtClass extClass) {
			bool selfPropMatched = false;
			bool staticsMethodMatched = false;
			ExtObjectMember selfProp = null;
			ExtObjectMember staticsMethod = null;
			foreach (ExtObjectMember member in extObject.Members) {
				if (this.isIdentifierNameWrong(member.Name, member))
					continue;
				if (member.Tagname == "cfg") {
					this.readAndAddCfgOrProp(ref extClass, member, extObject.Name, true);
				} else if (member.Tagname == "property") {
					if (member.Name == "self") {
						selfPropMatched = true;
						selfProp = member;
					} else { 
						this.readAndAddCfgOrProp(ref extClass, member, extObject.Name, false);
					}
				} else if (member.Tagname == "event") {
					this.readAndAddMethodOrEvent(ref extClass, member, extObject.Name, true);
				} else if (member.Tagname == "method") {
					if (member.Name == "statics") {
						staticsMethodMatched = true;
						staticsMethod = member;
					} else {
						this.readAndAddMethodOrEvent(ref extClass, member, extObject.Name, false);
					}
				} else {
					throw new Exception(String.Format(
						"Unknown Ext class member tagname: `{0}` (`{1}`).", 
						member.Tagname, extClass.Name.FullName
					));
				}
			}
			// Add customly typed `self` protected property for any class with self property:
			if (selfPropMatched || staticsMethodMatched) {
				bool extClassHasStaticMembers = (
					extClass.Members.PropertiesStatic.Count > 0 ||
					extClass.Members.MethodsStatic.Count > 0
				);
				if (selfPropMatched) {
					if (extClassHasStaticMembers) {
						// Add custom self type:
						this.readAndAddCfgOrProp(
							ref extClass, 
							new ExtObjectMember() {
								Name = "self",
								Doc = selfProp.Doc,
								Protected = true,
								Default = "Ext.Base",
								Static = false,
								Type = extClass.Name.FullName + SpecialsGenerator.STATICS_NAME_ADDITION,
								Owner = extClass.Name.FullName,
							}, 
							extObject.Name, 
							false
						);
					} else {
						this.readAndAddCfgOrProp(ref extClass, selfProp, extObject.Name, false);
					}
				}
				if (staticsMethodMatched) {
					if (extClassHasStaticMembers) {
						// Add custom statics() return type:
						this.readAndAddMethodOrEvent(
							ref extClass, 
							new ExtObjectMember() {
								Name = "statics",
								Doc = staticsMethod.Doc,
								Protected = true,
								Chainable = false,
								Template = false,
								Static = false,
								Params = staticsMethod.Params,
								Return = new Return() {
									Doc = null,
									Type = extClass.Name.FullName + SpecialsGenerator.STATICS_NAME_ADDITION
								},
								Owner = extClass.Name.FullName,
							}, 
							extObject.Name, 
							false
						);
					} else {
						this.readAndAddMethodOrEvent(ref extClass, staticsMethod, extObject.Name, false);
					}
				}
			}
		}
	}
}