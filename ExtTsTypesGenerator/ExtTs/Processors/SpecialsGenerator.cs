using ExtTs.ExtTypes;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public delegate void specialsGeneratorProgressHandler(int processedClassCount, string processedClass);
	public class SpecialsGenerator {
		protected internal const string GLOBAL_CLASS_BASE = "ExtGlobal<browserGlobalClassName>";

		protected internal const string BASE_CONFIGS_INTERFACE_NAME			= "Ext.base.Configs";
		protected internal const string BASE_EVENTS_INTERFACE_NAME			= "Ext.base.Events";
		protected internal const string BASE_EVENTS_CFG_INTERFACE_NAME		= "Ext.base.EventConfig";
		protected internal const string BASE_STATICS_INTERFACE_NAME			= "Ext.base.Statics";
		protected internal const string BASE_DEFINITIONS_INTERFACE_NAME		= "Ext.base.Definitions";
		protected internal const string BASE_PARAMS_INTERFACE_NAME			= "Ext.base.Params";
		protected internal const string BASE_RETURNS_INTERFACE_NAME			= "Ext.base.Returns";

		protected internal const string CONFIGS_NAME_ADDITION				= ".Cfg";
		protected internal const string EVENTS_NAME_ADDITION				= ".Events";
		protected internal const string STATICS_NAME_ADDITION				= ".Statics";
		protected internal const string DEFINITIONS_NAME_ADDITION			= ".Def";

		protected Processor processor;
		protected internal SpecialsGenerator(Processor processor) {
			this.processor = processor;
		}
		protected internal bool GenerateAlternativeClasses(consolidateProgressHandler progressHandler) {
			// generate empty classes to use classes alternative names:
			int extClassesIndex = 0;
			ExtClass alternativeAliasClass;
			Store store = this.processor.Store;
			Reader reader = this.processor.Reader;
			foreach (ExtClass extClass in store.ExtStandardClasses) {
				if (extClass.AlternativeNames.Count > 0) {
					foreach (NameInfo altClassNameInfo in extClass.AlternativeNames) {
						alternativeAliasClass = new ExtClass(
							altClassNameInfo, extClass.Name
						);
						alternativeAliasClass.Package = extClass.Package;
						alternativeAliasClass.Name.PackagedNamespace = reader.GetPackagedNamespaceFromFullClassName(
							extClass.Name.FullName
						);
						alternativeAliasClass.ClassType = ClassType.CLASS_ALIAS;
						if (store.ExtClassesMap.ContainsKey(alternativeAliasClass.Name.FullName)) {
							try {
								throw new Exception(
									$"There was not possible to create alias class `{alternativeAliasClass.Name.FullName}` for class `{extClass.Name.FullName}`. Class with this name already exists."
								);
							} catch (Exception e) {
								this.processor.Exceptions.Add(e);
							}
						} else { 
							store.AddExtClass(alternativeAliasClass);
						}
					}
				}
				extClassesIndex += 1;
				progressHandler.Invoke(
					extClassesIndex,
					extClass.Name.FullName
				);
			}
			// Generate proxy classes for global browser classes like:
			// Object, Array, Date, Element, Function, Error, Promise:
			foreach (string jsGlobalAlsoInExtNs in JavascriptInternals.JsGlobalsAlsoInExtNamespace)
				this.generateBrowserGlobalProxyClass(jsGlobalAlsoInExtNs);
			this.generateBaseMethodParamsAndReturnsInterfaces();
			
			return true;
		}
		protected void generateBrowserGlobalProxyClass (string browserGlobalClass, bool addAnyOtherMemberIndexer = false) {
			// Generate global class extended from browser global "Object" to be able 
			// extend this global browser Object from any namespace:
			string extGlobalClassName = SpecialsGenerator.GLOBAL_CLASS_BASE
				.Replace("<browserGlobalClassName>", browserGlobalClass);
			ExtClass baseObjectClass = new ExtClass(
				extGlobalClassName, browserGlobalClass, new string[] {
					$"Browser's global `{browserGlobalClass}` definition proxy class ",
					$"for classes in `Ext.*` namespace(s) to not pointing to `Ext.{browserGlobalClass}`."
				}
			);
			baseObjectClass.Package = ExtJsPackage.CORE;
			baseObjectClass.Name.PackagedNamespace = this.processor.Reader.GetPackagedNamespaceFromFullClassName(
				extGlobalClassName
			);
			baseObjectClass.ClassType = ClassType.CLASS_ALIAS;
			if (addAnyOtherMemberIndexer) 
				this.addBaseIndexer(ref baseObjectClass);
			this.processor.Store.AddExtClass(baseObjectClass);
		}
		protected void generateBaseMethodParamsAndReturnsInterfaces () {
			// generate function param config object base interface class:
			ExtClass baseMethodParamsInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_PARAMS_INTERFACE_NAME,
				"Base interface for method config params.",
				ClassType.CLASS_METHOD_PARAM_CONF_OBJ
			);
			this.addBaseIndexer(ref baseMethodParamsInterface);
			// generate function return config object base interface class:
			ExtClass baseMethodReturnsInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_RETURNS_INTERFACE_NAME,
				"Base interface for method return objects.",
				ClassType.CLASS_METHOD_RETURN_OBJECT
			);
			this.addBaseIndexer(ref baseMethodReturnsInterface);
		}
		protected internal bool GenerateConfigurationsInterfaces(specialsGeneratorProgressHandler progressHandler) {
			ExtClass baseConfigInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_CONFIGS_INTERFACE_NAME,
				"Base configurations interface.",
				ClassType.CLASS_CONFIGS
			);
			this.addBaseIndexer(ref baseConfigInterface);
			int processedClassesCount = 0;
			ExtClass configClass;
			Store store = this.processor.Store;
			ExtClass extBaseClass = store.GetByFullName("Ext.Base");
			if (extBaseClass == null) throw new Exception("Class `Ext.Base` not found!");
			foreach (ExtClass standardClass in store.ExtStandardClasses) {
				if (standardClass.Members.Configations.Count > 0) {
					configClass = new ExtClass(
						standardClass.Name.FullName + SpecialsGenerator.CONFIGS_NAME_ADDITION,
						SpecialsGenerator.BASE_CONFIGS_INTERFACE_NAME,
						standardClass.Docs
					);
					configClass.Package = standardClass.Package;
					configClass.Name.PackagedNamespace = standardClass.Name.PackagedNamespace;
					configClass.ClassType = ClassType.CLASS_CONFIGS;
					configClass.Link = new string[] {
						standardClass.Name.FullName,
						this.processor.Reader.GetLinkHrefForClass(
							standardClass.Name.FullName
						)
					};
					foreach (var confMemberItem in standardClass.Members.Configations) 
						configClass.Members.Configations.Add(
							confMemberItem.Key,
							(confMemberItem.Value as Configuration).Clone()
						);
					// Add only template methods:
					this.generateConfigurationsInterfacesTemplateMethods(
						standardClass, ref configClass
					);
					// Add only instance methods from Ext.Base:
					this.generateConfigurationsInterfacesExtBaseMethods(
						standardClass, extBaseClass, ref configClass
					);
					configClass.HasMembers = true;
					store.AddExtClass(configClass);
				}
				processedClassesCount += 1;
				progressHandler.Invoke(processedClassesCount, standardClass.Name.FullName);
			}
			return true;
		}
		protected void generateConfigurationsInterfacesTemplateMethods (ExtClass standardClass, ref ExtClass configClass) {
			List<Member> methodMemberVariants;
			Method firstMethodVariant;
			foreach (var methodMemberItem in standardClass.Members.Methods) {
				firstMethodVariant = methodMemberItem.Value.FirstOrDefault<Member>() as Method;
				if (!firstMethodVariant.IsTemplate)
					continue;
				methodMemberVariants = new List<Member>();
				foreach (var methodVariant in methodMemberItem.Value)
					methodMemberVariants.Add((methodVariant as Method).Clone());
				configClass.Members.Methods.Add(
					methodMemberItem.Key,
					methodMemberVariants
				);
			}
		}
		protected void generateConfigurationsInterfacesExtBaseMethods (ExtClass standardClass, ExtClass extBaseClass, ref ExtClass configClass) {
			List<Member> methodMemberVariants;
			Method firstMethodVariant;
			Method newMethodVariant;
			string methodName;
			bool standardClassHasStaticsInterface = (
				standardClass.Members.MethodsStatic.Count > 0 || 
				standardClass.Members.PropertiesStatic.Count > 0
			);
			foreach (var methodMemberItem in extBaseClass.Members.Methods) {
				firstMethodVariant = methodMemberItem.Value.FirstOrDefault<Member>() as Method;
				if (
					firstMethodVariant.AccessModJs == ExtTypes.Enums.AccessModifier.PRIVATE ||
					firstMethodVariant.IsConstructor
				)
					continue;
				methodMemberVariants = new List<Member>();
				methodName = methodMemberItem.Key;
				if (
					standardClassHasStaticsInterface && (
						firstMethodVariant.IsChainable ||
						methodName == "statics"
					)
				) {
					foreach (var methodVariant in methodMemberItem.Value) {
						newMethodVariant = (methodVariant as Method).Clone();
						newMethodVariant.ReturnTypes = new List<string>() {
							standardClass.Name.FullName + SpecialsGenerator.STATICS_NAME_ADDITION
						};
						methodMemberVariants.Add(newMethodVariant);
					}
				} else {
					foreach (var methodVariant in methodMemberItem.Value) 
						methodMemberVariants.Add((methodVariant as Method).Clone());
				}
				// Check if there is already any previous template method:
				if (configClass.Members.Methods.ContainsKey(methodName))
					throw new Exception(
						$"It's not possible to add instance method from `Ext.Base` into configuration interface. \n"+
						"There is already template method added between configuration interface methods: `{methodName}`."
					);
				configClass.Members.Methods.Add(
					methodName, methodMemberVariants
				);
			}
		}
		protected internal bool GenerateEventsInterfaces(specialsGeneratorProgressHandler progressHandler) {
			this.generateEventsInterfacesBases();
			int processedClassesCount = 0;
			ExtClass eventClass;
			List<Member> eventMemberVariants;
			foreach (ExtClass standardClass in this.processor.Store.ExtStandardClasses) {
				if (standardClass.Members.Events.Count > 0 && !standardClass.Private) {
					eventClass = new ExtClass(
						standardClass.Name.FullName + SpecialsGenerator.EVENTS_NAME_ADDITION,
						SpecialsGenerator.BASE_EVENTS_INTERFACE_NAME,
						standardClass.Docs
					);
					eventClass.Package = standardClass.Package;
					eventClass.Name.PackagedNamespace = standardClass.Name.PackagedNamespace;
					eventClass.ClassType = ClassType.CLASS_EVENTS;
					eventClass.Link = new string[] {
						standardClass.Name.FullName,
						this.processor.Reader.GetLinkHrefForClass(
							standardClass.Name.FullName
						)
					};
					foreach (var eventMemberItem in standardClass.Members.Events) {
						eventMemberVariants = new List<Member>();
						foreach (var eventVariant in eventMemberItem.Value)
							eventMemberVariants.Add((eventVariant as Event).Clone());
						eventClass.Members.Events.Add(
							eventMemberItem.Key,
							eventMemberVariants
						);
						eventClass.HasMembers = true;
					}
					this.processor.Store.AddExtClass(eventClass);
				}
				processedClassesCount += 1;
				progressHandler.Invoke(processedClassesCount, standardClass.Name.FullName);
			}
			return true;
		}
		protected void generateEventsInterfacesBases () {
			// Complete and add base Events interface:
			ExtClass baseEventsInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_EVENTS_INTERFACE_NAME,
				"Base events interface.",
				ClassType.CLASS_EVENTS
			);
			string[] baseObjectClassIndexerDocs = this.processor.GenerateJsDocs
				? new string[] {
					"Any event method or event config.",
				}
				: null;
			Indexer baseIndexer = new Indexer(
				"events",
				new List<string>() { "string" },
				new List<string>() {
					SpecialsGenerator.BASE_EVENTS_CFG_INTERFACE_NAME,
					"Function"
				},
				baseObjectClassIndexerDocs,
				baseEventsInterface.Name.FullName,
				true
			);
			baseEventsInterface.AddMemberIndexer(baseIndexer);
			// Complete and add base EventConfig interface:
			ExtClass baseEventCfgInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_EVENTS_CFG_INTERFACE_NAME,
				"Event config interface.",
				ClassType.CLASS_EVENTS
			);
			Configuration fnCfg = new Configuration(
				"fn", new List<string>() { "Function" }, 
				new string[] { "Event handler.", "@required" }, 
				baseEventCfgInterface.Name.FullName, true
			);
			//fnCfg.Required = true;
			baseEventCfgInterface.AddMemberConfiguration(fnCfg);
			baseEventCfgInterface.AddMemberConfiguration(new Configuration(
				"scope", new List<string>() { "any" }, 
				new string[] { "Event handler `this` context value." }, 
				baseEventCfgInterface.Name.FullName, true
			));
			baseEventCfgInterface.AddMemberConfiguration(new Configuration(
				"buffer", new List<string>() { "number" }, 
				new string[] {
					"Handler is invoked only once every `n` miliseconds in `buffer`, ",
					"regardless of how many times user fire it."
				}, 
				baseEventCfgInterface.Name.FullName, true
			));
			baseEventCfgInterface.AddMemberConfiguration(new Configuration(
				"single", new List<string>() { "boolean" }, 
				new string[] {
					"Handler is invoked only once."
				}, 
				baseEventCfgInterface.Name.FullName, true
			));
		}
		protected internal bool GenerateStandardClassesInheritanceCompatibleMembers(specialsGeneratorProgressHandler progressHandler) {
			int processedClassesCount = 0;
			int[] parentsCountsKeys = this.processor.Store.ExtClassesParentsCounts.Keys.ToArray<int>();
			int parentsCountsKey;
			Dictionary<int, Dictionary<string, int>> parentsCountsRecords;
			int[] moduleNameComplexityKeys;
			int moduleNameComplexityKey;
			Dictionary<string, int> parentsCountsRecord;
			string[] classNames;
			string className;
			int classIndex;
			ExtClass extClass;
			InheritanceResolver resolver = new InheritanceResolver(this.processor);
			for (int i = 0; i < parentsCountsKeys.Length; i++) {
				parentsCountsKey = parentsCountsKeys[i];
				parentsCountsRecords = this.processor.Store.ExtClassesParentsCounts[parentsCountsKey];
				moduleNameComplexityKeys = parentsCountsRecords.Keys.ToArray<int>();
				for (int j = 0; j < moduleNameComplexityKeys.Length; j++) {
					moduleNameComplexityKey = moduleNameComplexityKeys[j];
					parentsCountsRecord = parentsCountsRecords[moduleNameComplexityKey];
					classNames = parentsCountsRecord.Keys.ToArray<string>();
					for (int k = 0; k < classNames.Length; k++) {
						className = classNames[k];
						classIndex = parentsCountsRecord[className];
						extClass = this.processor.Store.ExtAllClasses[classIndex];
						if (
							extClass.ClassType == ClassType.CLASS_STANDARD || (
								extClass.ClassType == ClassType.CLASS_METHOD_PARAM_CONF_OBJ &&
								extClass.Extends != null &&
								extClass.Extends.FullName != SpecialsGenerator.BASE_PARAMS_INTERFACE_NAME
							)
						) {
							// Only standard classes and method config object params classes extended 
							// from something higher than base config interface are resolved here:
							resolver.Resolve(ref extClass);
						}
						processedClassesCount += 1;
						progressHandler.Invoke(processedClassesCount, extClass.Name.FullName);
					}
				}
			}
			return true;
		}
		protected internal bool GenerateStaticsInterfaces (specialsGeneratorProgressHandler progressHandler) {
			ExtClass baseStaticsInterface = this.addBaseInterface(
				SpecialsGenerator.BASE_STATICS_INTERFACE_NAME,
				"Base static members interface.",
				ClassType.CLASS_STATICS
			);
			this.addBaseIndexer(ref baseStaticsInterface);
			int processedClassesCount = 0;
			ExtClass staticsClass;
			List<Member> methodMemberVariants;
			Property prop;
			Method methodVariant;
			foreach (ExtClass standardClass in this.processor.Store.ExtStandardClasses) {
				if (
					standardClass.Members.MethodsStatic.Count > 0 || 
					standardClass.Members.PropertiesStatic.Count > 0
				) {
					staticsClass = new ExtClass(
						standardClass.Name.FullName + SpecialsGenerator.STATICS_NAME_ADDITION,
						SpecialsGenerator.BASE_STATICS_INTERFACE_NAME,
						standardClass.Docs
					);
					staticsClass.Package = standardClass.Package;
					staticsClass.Name.PackagedNamespace = standardClass.Name.PackagedNamespace;
					staticsClass.ClassType = ClassType.CLASS_STATICS;
					staticsClass.Link = new string[] {
						standardClass.Name.FullName,
						this.processor.Reader.GetLinkHrefForClass(
							standardClass.Name.FullName
						)
					};
					foreach (var propMemberItem in standardClass.Members.PropertiesStatic) {
						prop = propMemberItem.Value as Property;
						if (prop.Renderable)
							staticsClass.Members.PropertiesStatic.Add(
								propMemberItem.Key,
								prop.Clone()
							);
					}
					foreach (var methodMemberItem in standardClass.Members.MethodsStatic) {
						methodMemberVariants = new List<Member>();
						foreach (Member methodMember in methodMemberItem.Value) {
							methodVariant = methodMember as Method;
							if (methodVariant.Renderable)
								methodMemberVariants.Add(methodVariant.Clone());
						}
						if (methodMemberVariants.Count > 0)
							staticsClass.Members.MethodsStatic.Add(
								methodMemberItem.Key,
								methodMemberVariants
							);
					}
					if (
						staticsClass.Members.PropertiesStatic.Count > 0 ||
						staticsClass.Members.MethodsStatic.Count > 0
					) { 
						staticsClass.HasMembers = true;
						this.processor.Store.AddExtClass(staticsClass);
					}
				}
				processedClassesCount += 1;
				progressHandler.Invoke(processedClassesCount, standardClass.Name.FullName);
			}
			return true;
		}
		protected internal bool GenerateDefinitionsClasses (specialsGeneratorProgressHandler progressHandler) {
			int processedClassesCount = 0;
			ExtClass definitionsClass;
			Store store = this.processor.Store;
			ExtClass extClassCfg = store.GetByFullName("Ext.Class" + SpecialsGenerator.CONFIGS_NAME_ADDITION);
			if (extClassCfg == null) throw new Exception("Configuration interface for `Ext.Class` not found!");
			List<Configuration> extClassConfigs = (
				from item in extClassCfg.Members.Configations
				orderby item.Key ascending
				select item.Value as Configuration
			).ToList<Configuration>();
			foreach (ExtClass standardClass in this.processor.Store.ExtStandardClasses) {
				if (!standardClass.Singleton && !standardClass.Private) {
					definitionsClass = new ExtClass(
						standardClass.Name.FullName + SpecialsGenerator.DEFINITIONS_NAME_ADDITION,
						standardClass.Name.FullName,
						standardClass.Docs
					);
					definitionsClass.Package = standardClass.Package;
					definitionsClass.Name.PackagedNamespace = standardClass.Name.PackagedNamespace;
					definitionsClass.ClassType = ClassType.CLASS_DEFINITIONS;
					definitionsClass.Link = new string[] {
						standardClass.Name.FullName,
						this.processor.Reader.GetLinkHrefForClass(
							standardClass.Name.FullName
						)
					};
					// Add all configuration elements as public instance properties:
					this.generateDefinitionsClassesAddDefsClassConfigItems(
						extClassConfigs, standardClass, ref definitionsClass
					);
					definitionsClass.HasMembers = true;
					store.AddExtClass(definitionsClass);
				}
				processedClassesCount += 1;
				progressHandler.Invoke(processedClassesCount, standardClass.Name.FullName);
			}
			return true;
		}
		protected void generateDefinitionsClassesAddDefsClassConfigItems (List<Configuration> extClassConfigs, ExtClass standardClass, ref ExtClass definitionsClass) {
			Dictionary<string, Member> standardClassProps = standardClass.Members.Properties;
			Dictionary<string, List<Member>> standardClassMethods = standardClass.Members.Methods;
			Property newProp;
			List<string> newPropTypes;
			bool standardClassHasConfigsInterface = (
				standardClass.Members.Configations.Count > 0
			);
			bool standardClassHasStaticsInterface = (
				standardClass.Members.MethodsStatic.Count > 0 || 
				standardClass.Members.PropertiesStatic.Count > 0
			);
			Property mixedStandardClassOrParentProperty;
			ExtClass mixedStandardClassOrParentMethodsClass;
			List<Member> mixedStandardClassOrParentMethods;
			Method mixedStandardClassOrParentMethod;
			Method mixedStandardClassOrParentMethodClone;
			foreach (Configuration extClassConfig in extClassConfigs) {
				mixedStandardClassOrParentProperty = null;
				mixedStandardClassOrParentMethods = null;
				if (standardClassHasConfigsInterface && extClassConfig.Name == "config") {
					newPropTypes = new List<string>() {
						standardClass.Name.FullName + SpecialsGenerator.CONFIGS_NAME_ADDITION
					};
				} else if (standardClassHasStaticsInterface && extClassConfig.Name == "statics") {
					newPropTypes = new List<string>() {
						standardClass.Name.FullName + SpecialsGenerator.STATICS_NAME_ADDITION
					};
				} else {
					newPropTypes = new List<string>(extClassConfig.Types);
				}
				newProp = new Property(
					extClassConfig.Name,
					newPropTypes,
					extClassConfig.Doc,
					extClassConfig.Owner.FullName,
					false
				);
				newProp.AccessModJs = AccessModifier.PROTECTED;
				newProp.AccessModTs = AccessModifier.PROTECTED;
				newProp.DefaultValue = extClassConfig.DefaultValue;
				newProp.Deprecated = extClassConfig.Deprecated;
				newProp.Inherited = true;
				newProp.IsReadOnly = false;
				newProp.IsStatic = false;
				newProp.Renderable = true;
				mixedStandardClassOrParentProperty = this.generateDefinitionsClassesTryToFindTheSameProp(
					standardClass, extClassConfig.Name
				);
				mixedStandardClassOrParentMethodsClass = this.generateDefinitionsClassesTryToFindTheSameMethodClass(
					standardClass, extClassConfig.Name
				);
				if (mixedStandardClassOrParentProperty != null) {
					// mix with already defined property - merge new property types and make parent property renderable
					this.generateDefinitionsClassesMergeProperties(
						ref mixedStandardClassOrParentProperty, ref newProp
					);
				} else if (mixedStandardClassOrParentMethodsClass != null) {
					// mix with already defined methods
					mixedStandardClassOrParentMethods = mixedStandardClassOrParentMethodsClass.Members.Methods[extClassConfig.Name];
					if (mixedStandardClassOrParentMethodsClass.Name.FullName == standardClass.Name.FullName) {
						// mexid method is in the same class - mixed method + property will be rendered at the end,
						// so only make method renderable for sure
						for (int i = 0; i < mixedStandardClassOrParentMethods.Count; i++) {
							mixedStandardClassOrParentMethod = mixedStandardClassOrParentMethods[i] as Method;
							mixedStandardClassOrParentMethod.Renderable = true;
							mixedStandardClassOrParentMethods[i] = mixedStandardClassOrParentMethod;
							mixedStandardClassOrParentMethodClone = mixedStandardClassOrParentMethod.Clone();
							mixedStandardClassOrParentMethodClone.ExistenceReason = ExistenceReasonType.NATURAL;
							definitionsClass.AddMemberMethod(mixedStandardClassOrParentMethodClone);
						}
						definitionsClass.AddMemberProperty(newProp);
					} else {
						// mexid method is in some parent class - add this method to current class, make it renderable 
						// and mixed prop andmethod will be rendered at the end
						for (int i = 0; i < mixedStandardClassOrParentMethods.Count; i++) {
							mixedStandardClassOrParentMethod = mixedStandardClassOrParentMethods[i] as Method;
							mixedStandardClassOrParentMethod.Renderable = true;
							mixedStandardClassOrParentMethods[i] = mixedStandardClassOrParentMethod;
							mixedStandardClassOrParentMethodClone = mixedStandardClassOrParentMethod.Clone();
							mixedStandardClassOrParentMethodClone.ExistenceReason = ExistenceReasonType.NATURAL;
							definitionsClass.AddMemberMethod(mixedStandardClassOrParentMethodClone);
								
						}
						definitionsClass.AddMemberProperty(newProp);
					}
				} else {
					definitionsClass.AddMemberProperty(newProp);
				}
			}
		}
		protected Property generateDefinitionsClassesTryToFindTheSameProp (ExtClass standardClass, string propName) {
			ExtClass currentClass = standardClass;
			ExtClass parentClass;
			Store store = this.processor.Store;
			Property result = null;
			while (true) {
				if (currentClass.Members.Properties.ContainsKey(propName)) {
					result = currentClass.Members.Properties[propName] as Property;
					break;
				}
				if (currentClass.Extends == null) break;
				parentClass = store.GetParentClassByCurrentClassFullName(currentClass.Name.FullName);
				if (parentClass == null) break;
				currentClass = parentClass;
			}
			return result;
		}
		protected ExtClass generateDefinitionsClassesTryToFindTheSameMethodClass (ExtClass standardClass, string methodName) {
			ExtClass currentClass = standardClass;
			ExtClass parentClass;
			Store store = this.processor.Store;
			ExtClass result = null;
			while (true) {
				if (currentClass.Members.Methods.ContainsKey(methodName)) {
					result = currentClass;
					break;
				}
				if (currentClass.Extends == null) break;
				parentClass = store.GetParentClassByCurrentClassFullName(currentClass.Name.FullName);
				if (parentClass == null) break;
				currentClass = parentClass;
			}
			return result;
		}
		protected void generateDefinitionsClassesMergeProperties (ref Property mixedStandardClassOrParentProperty, ref Property newProp) {
			string newPropType;
			foreach (var item in mixedStandardClassOrParentProperty.Types) {
				newPropType = item.Key;
				if (!newProp.Types.ContainsKey(newPropType)) {
					// add new type and compatibility reason:
					newProp.Types.Add(
						newPropType,
						new ExistenceReason() {
							CompatibilityReasonClassFullName = item.Value.CompatibilityReasonClassFullName,
							Type = item.Value.Type
						}
					);
				}
			}
			int standardClassPropAccessMod = (int)mixedStandardClassOrParentProperty.AccessModTs;
			int newPropAccessModTs = (int)newProp.AccessModTs;
			if (newPropAccessModTs > standardClassPropAccessMod)
				mixedStandardClassOrParentProperty.SetResultAccessModTsWithDependentAccessModProps(
					newProp.AccessModTs, true, null
				);
			mixedStandardClassOrParentProperty.Renderable = true;
		}
		protected ExtClass addBaseInterface (string interfaceFullName, string interfaceDocs, ClassType classType) {
			ExtClass baseInterface = new ExtClass(
				interfaceFullName, 
				"", 
				new string[] { interfaceDocs }
			);
			baseInterface.Package = ExtJsPackage.CORE;
			baseInterface.Name.PackagedNamespace = this.processor.Reader.GetPackagedNamespaceFromFullClassName(
				interfaceFullName
			);
			baseInterface.ClassType = classType;
			this.processor.Store.AddExtClass(baseInterface);
			return baseInterface;
		}
		protected Indexer addBaseIndexer (ref ExtClass baseObjectClass) {
			string[] baseObjectClassIndexerDocs = this.processor.GenerateJsDocs
				? new string[] {
					"Any other property or method.",
					"@see https://github.com/microsoft/TypeScript/issues/3755"
				}
				: null;
			Indexer baseIndexer = new Indexer(
				"others",
				new List<string>() { "string" },
				new List<string>() { "any" },
				baseObjectClassIndexerDocs,
				baseObjectClass.Name.FullName,
				true
			);
			baseObjectClass.AddMemberIndexer(baseIndexer);
			return baseIndexer;
		}
	}
}
