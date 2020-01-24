using ExtTs.ExtTypes;
using System.Collections.Generic;
using ExtTs.ExtTypes.Enums;
using System;
using ExtTs.ExtTypes.Structs;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ExtTs.Processors {
	public class Store {
		private volatile bool insideLock = false;
		protected object addLock = new object { };
		
		protected internal string RootDirFullPath;
		protected internal string TmpFullPath;
		protected internal string SourceFullPath;
		protected internal List<PackageSource> PackagesData = new List<PackageSource>();

		// "numericExtClassIndex" => "ExtClass"
		protected internal List<ExtClass> ExtAllClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtStandardClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtAliasClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtMethodParamsClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtStaticsClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtConfigClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtDefinitionsClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtEventsClasses = new List<ExtClass>();
		protected internal List<ExtClass> ExtStandardSingletonClasses = new List<ExtClass>();
		protected internal Dictionary<string, ExtClass> ExtCallbackClasses = new Dictionary<string, ExtClass>();
		// "Full.class.Name" => "numericClassIndex"
		protected internal Dictionary<string, int> ExtClassesMap = new Dictionary<string, int>();
		// "Type.full.Name" => ["TypeDefinitionSource"]
		protected internal ConcurrentDictionary<string, List<TypeDefinitionSource>> TypesPlaces = new ConcurrentDictionary<string, List<TypeDefinitionSource>>();
		// "Raw.../Spread.../Params.../Value..." => ["Type.full.Names"]
		protected internal Dictionary<string, List<string>> SpreadTypesPlaces = new Dictionary<string, List<string>>();
		// [Unknown.type.full.Names] => "definition full paths comma separated"
		protected internal Dictionary<string, string> UnknownTypes = new Dictionary<string, string>();
		// Full.class.Name.STATIC_PROP_NAME => ["number", "string"...]
		protected internal Dictionary<string, List<string>> StaticPropsTypes = new Dictionary<string, List<string>>();
		// "numericParentsCount" => "numericClassesCountInModule" => "Class.full.Name" => "extClassIndex"
		protected internal Dictionary<int, Dictionary<int, Dictionary<string, int>>> ExtClassesParentsCounts = new Dictionary<int, Dictionary<int, Dictionary<string, int>>>();
		// "ExtJsPackage" => "numericNamespaceLevel" => ["nameslace1", "namespace2", "other"]
		protected internal Dictionary<ExtJsPackage, List<List<string>>> ExtBaseNs = new Dictionary<ExtJsPackage, List<List<string>>>();
		// "ExtJsPackage" => "optimalized.namespace.name" => [extClassIndexes]
		protected internal Dictionary<ExtJsPackage, Dictionary<string, List<int>>> ExtBaseNsClasses = new Dictionary<ExtJsPackage, Dictionary<string, List<int>>>();
		// [extClassIndexes]
		protected internal List<int> ExtClassesWithParent = new List<int>();
		// "packageName" => PkgCfg
		protected internal Dictionary<string, PkgCfg> SourcesPaths = new Dictionary<string, PkgCfg>();
		// "Wrong.full.path.ClassName" => "Correct.full.path.ClassName"
		protected internal Dictionary<string, string> ClassesFixes = new Dictionary<string, string>();
		// "[place]Namespace.full.path.ClassName.methodName:paramName" => "raw correct param type definition"
		protected internal Dictionary<string, string> TypesFixes = new Dictionary<string, string>();
		
		protected internal void AddExtClass (ExtClass extClass) {
			if (this.insideLock) {
				this._addExtClass(extClass);
			} else { 
				lock (this.addLock) {
					this.insideLock = true;
					this._addExtClass(extClass);
					this.insideLock = false;
				}
			}
		}
		protected internal void _addExtClass (ExtClass extClass) {
			//if (extClass == null) Debugger.Break();
			int index = this.ExtAllClasses.Count;
			this.ExtAllClasses.Add(extClass);
			this.ExtClassesMap.Add(extClass.Name.FullName, index);
			if (extClass.Extends != null)
				this.ExtClassesWithParent.Add(index);
			if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_STANDARD) {
				this.ExtStandardClasses.Add(extClass);
				if (extClass.Singleton)
					this.ExtStandardSingletonClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_ALIAS) {
				this.ExtAliasClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CALLBACK) {
				this.ExtCallbackClasses.Add(extClass.Name.FullName, extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CONF_OBJ) {
				this.ExtMethodParamsClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_STATICS) {
				this.ExtStaticsClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_CONFIGS) {
				this.ExtStaticsClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_DEFINITIONS) {
				this.ExtDefinitionsClasses.Add(extClass);
			} else if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_EVENTS) {
				this.ExtEventsClasses.Add(extClass);
			}
		}
		protected internal void AddTypePlace(TypeDefinitionPlace typeDefinitionPlace, string definitionFullPath, string memberOrParamName, string typeName) {
			if (this.insideLock) {
				this._addTypePlace(typeDefinitionPlace, definitionFullPath, memberOrParamName, typeName);
			} else {
				lock (this.addLock) {
					this.insideLock = true;
					this._addTypePlace(typeDefinitionPlace, definitionFullPath, memberOrParamName, typeName);
					this.insideLock = false;
				}
			}
		}
		protected internal void _addTypePlace(TypeDefinitionPlace typeDefinitionPlace, string definitionFullPath, string memberOrParamName, string typeName) {
			List<TypeDefinitionSource> defPlace;
			if (!this.TypesPlaces.ContainsKey(typeName))
				this.TypesPlaces.AddOrUpdate(
					typeName, (string key) => {
						return new List<TypeDefinitionSource>();
					}, (string key, List<TypeDefinitionSource> value) => {
						return value;
					}
				);
				/*this.TypesPlaces.Add(
					typeName, new List<TypeDefinitionSource>()
				);*/
			defPlace = this.TypesPlaces[typeName];
			defPlace.Add(new TypeDefinitionSource() {
				Type				= typeDefinitionPlace,
				DefinitionFullPath	= definitionFullPath,
				MemberOrParamName	= memberOrParamName,
			});
		}
		protected internal void AddSpreadSyntaxTypePlace(string sourceCodeDefinitionFullPathKey, string rawSpreadSyntaxTypeDefinitions) {
			if (this.insideLock) {
				this._addSpreadSyntaxTypePlace(sourceCodeDefinitionFullPathKey, rawSpreadSyntaxTypeDefinitions);
			} else { 
				lock (this.addLock) {
					this.insideLock = true;
					this._addSpreadSyntaxTypePlace(sourceCodeDefinitionFullPathKey, rawSpreadSyntaxTypeDefinitions);
					this.insideLock = false;
				}
			}
		}
		protected internal void _addSpreadSyntaxTypePlace(string sourceCodeDefinitionFullPathKey, string rawSpreadSyntaxTypeDefinitions) {
			if (this.SpreadTypesPlaces.ContainsKey(rawSpreadSyntaxTypeDefinitions)) {
				this.SpreadTypesPlaces[rawSpreadSyntaxTypeDefinitions].Add(sourceCodeDefinitionFullPathKey);
			} else {
				this.SpreadTypesPlaces[rawSpreadSyntaxTypeDefinitions] = new List<string>() { sourceCodeDefinitionFullPathKey };
			}
		}
		protected internal void AddUnknownType(string typeFullName, string definitionfullPaths) {
			if (this.insideLock) {
				this._addUnknownType(typeFullName, definitionfullPaths);
			} else {
				lock (this.addLock) {
					this.insideLock = true;
					this._addUnknownType(typeFullName, definitionfullPaths);
					this.insideLock = false;
				}
			}
		}
		protected internal void _addUnknownType(string typeFullName, string definitionfullPaths) {
			if (this.UnknownTypes.ContainsKey(typeFullName)) { 
				this.UnknownTypes[typeFullName] += ", " + definitionfullPaths;
			} else {
				this.UnknownTypes.Add(typeFullName, definitionfullPaths);
			}
		}
		protected internal ExtClass GetByFullName (string fullName) {
			if (!this.ExtClassesMap.ContainsKey(fullName)) return null;
			int classIndex = this.ExtClassesMap[fullName];
			return this.ExtAllClasses[classIndex];
		}
		protected internal ExtClass GetParentClassByCurrentClassFullName (string fullName) {
			if (!this.ExtClassesMap.ContainsKey(fullName))
				return null;
			int classIndex = this.ExtClassesMap[fullName];
			ExtClass parentExtClass = this.ExtAllClasses[classIndex];
			if (parentExtClass.Extends == null)
				return null;// Class has no parent:
			string parentClassFullName = parentExtClass.Extends.FullName;
			if (!this.ExtClassesMap.ContainsKey(parentClassFullName))
				return null;// Class has parent class between unknown types:
			classIndex = this.ExtClassesMap[parentClassFullName];
			return this.ExtAllClasses[classIndex];
		}
		protected internal ExtClass GetPossibleCallbackType (string callbackClassTypeFullName) {
			if (!this.ExtCallbackClasses.ContainsKey(callbackClassTypeFullName)) return null;
			return this.ExtCallbackClasses[callbackClassTypeFullName];
		}
	}
}
