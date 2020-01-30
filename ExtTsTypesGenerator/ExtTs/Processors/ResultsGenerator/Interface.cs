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
	public partial class ResultsGenerator {
		protected void generateInterface (ExtClass extClass) {
			//if (extClass.Name.FullName == "Ext.draw.TimingFunctions")
			//	Debugger.Break();
			if (extClass.ClassType == ClassType.CLASS_CONSTANT_ALIAS) {
				this.generateInterfaceOpenClose(extClass);
			} else {
				if (
					this.processor.GenerateJsDocs && 
					!extClass.Private && 
					extClass.HasMembers
				)
					this.generateInterfaceDocs(extClass);
				this.generateInterfaceOpen(extClass);
				this.generateInterfaceMembers(extClass);
				this.generateInterfaceClose();
			}
		}
		protected void generateInterfaceDocs (ExtClass extClass) {
			string classLinkText;
			string classLinkHref;
			if (
				/*extClass.ClassType == ClassType.CLASS_CONFIGS ||
				extClass.ClassType == ClassType.CLASS_STATICS ||
				extClass.ClassType == ClassType.CLASS_DEFINITIONS ||
				extClass.ClassType == ClassType.CLASS_EVENTS ||
				extClass.ClassType == ClassType.CLASS_METHOD_RETURN_OBJECT ||
				extClass.ClassType == ClassType.CLASS_METHOD_PARAM_CONF_OBJ*/
				extClass.Link != null
			) {
				classLinkText = extClass.Link[0];
				classLinkHref = extClass.Link[1];
			} else {
				classLinkText = extClass.Name.FullName;
				classLinkHref = this.processor.Reader.GetLinkHrefForClass(
					extClass.Name.FullName
				);
			}
			this.writeResultLine("/** ");
			if (extClass.ClassType == ClassType.CLASS_CONFIGS) {
				this.writeResultLine( " * @configurations");
				this.writeResultLine( " * Config interface to create class: ");
			} else if (extClass.ClassType == ClassType.CLASS_STATICS) {
				this.writeResultLine( " * @statics");
				this.writeResultLine( " * Class static members interface: ");
			} else if (extClass.ClassType == ClassType.CLASS_DEFINITIONS) {
				this.writeResultLine( " * @definitions");
				this.writeResultLine( " * Config interface to declare class: ");
			} else if (extClass.ClassType == ClassType.CLASS_EVENTS) {
				this.writeResultLine( " * @events");
				this.writeResultLine( " * Config interface for class listeners: ");
			} else if (extClass.ClassType == ClassType.CLASS_METHOD_PARAM_CONF_OBJ) {
				this.writeResultLine( " * @params");
				this.writeResultLine( " * Config interface to call method: ");
			} else if (extClass.ClassType == ClassType.CLASS_METHOD_RETURN_OBJECT) {
				this.writeResultLine( " * @returns");
				this.writeResultLine( " * Method return interface: ");
			}
			if (!extClass.Name.FullName.StartsWith("Ext.base."))
				this.writeResultLine($" * [{classLinkText}]({classLinkHref})");
			if (extClass.Docs != null && extClass.Docs.Length > 0)
				for (int i = 0; i < extClass.Docs.Length; i++) 
					this.writeResultLine(" * " + extClass.Docs[i]);
			this.writeResultLine(" */");
		}
		protected void generateInterfaceOpen (ExtClass extClass) {
			string line;
			/*if (extClass.ClassType == ClassType.CLASS_DEFINITIONS) {
				line = "class " + extClass.Name.ClassName;
			} else {*/
				line = "interface " + extClass.Name.ClassName;
			//}
			if (extClass.Extends != null) 
				line += " extends " + this.checkBrowserGlobalClass(extClass.Extends.FullName);
			line += " {";
			this.writeResultLine(line);
			this.whileSpaceLevel += 1;
		}
		protected void generateInterfaceOpenClose (ExtClass extClass) {
			string line = "interface " + extClass.Name.ClassName;
			if (extClass.Extends != null) 
				line += " extends " + this.checkBrowserGlobalClass(extClass.Extends.FullName);
			line += " {}";
			this.writeResultLine(line);
		}
		protected void generateInterfaceClose () {
			this.whileSpaceLevel -= 1;
			if (this.whileSpaceLevel == -1)
				Debugger.Break();
			this.writeResultLine("}");
		}
		protected void generateInterfaceMembers(ExtClass extClass) {
			foreach (var indexerItem in extClass.Members.Indexers) 
				this.generateMemberIndexer(extClass, indexerItem.Value as Indexer);

			if (extClass.ClassType == ClassType.CLASS_CONFIGS) {
				this.generateInterfaceConfigs(extClass);
				// Render template methods and basic methods from Ext.Base:
				this.generateInterfaceInstanceMethodsNotInConfigs(extClass);

			} else if (extClass.ClassType == ClassType.CLASS_DEFINITIONS) {
				this.generateInterfaceDefinitions(extClass);

			} else if (extClass.ClassType == ClassType.CLASS_STATICS) { 
				this.generateInterfaceStaticPropertiesAndMethods(extClass);

			} else if (extClass.ClassType == ClassType.CLASS_EVENTS) { 
				this.generateInterfaceEvents(extClass);
				// This is only for interface Ext.base.EventConfig
				this.generateInterfaceEventConfigProps(extClass);

			} else if (extClass.ClassType == ClassType.CLASS_METHOD_PARAM_CONF_OBJ) { 
				this.generateInterfaceMethodParamConfigObject(extClass);

			} else if (extClass.ClassType == ClassType.CLASS_METHOD_RETURN_OBJECT) { 
				this.generateInterfaceMethodReturnObject(extClass);
			}
		}

		protected void generateInterfaceConfigs(ExtClass extClass) {
			string cfgName;
			Configuration cfgItem;
			Dictionary<string, Member> instanceProps = extClass.Members.Properties;
			Dictionary<string, List<Member>> instanceMethods = extClass.Members.Methods;
			List<Member> methodVariants;
			Method firstMethodVariant;
			foreach (var item in extClass.Members.Configations) {
				cfgName = item.Key;
				cfgItem = item.Value as Configuration;
				if (instanceProps.ContainsKey(cfgName) && instanceMethods.ContainsKey(cfgName)) {
					// Configuration name exists also between instance properties and also between instance methods:
					throw new Exception(
						$"Rendering for mixed member between configurations, properties and methods is not implemented."
					);
				} else if (instanceProps.ContainsKey(cfgName)) {
					// Configuration name exists also between instance properties:
					throw new Exception(
						$"Rendering for mixed member between configurations and properties is not implemented."
					);
				} else if (instanceMethods.ContainsKey(cfgName)) {
					// Configuration name exists also between instance methods:
					methodVariants = instanceMethods[cfgName];
					firstMethodVariant = methodVariants.FirstOrDefault<Member>() as Method;
					if (firstMethodVariant.IsTemplate) {
						// TODO: bude nutné implementovat merging template 
						// metody a konfigurační vlastnosti? je to někde vůbec?
						Debugger.Break();
					}
				} else {
					// Standard configuration rendering:
					this.generateInterfaceConfiguration(
						extClass, cfgItem
					);
				}
			}
		}
		protected void generateInterfaceInstanceMethodsNotInConfigs (ExtClass extClass) {
			// Generate all non-private instance methods not in configs (without any access modifier, because we are inside interface):
			Dictionary<string, Member> configs = extClass.Members.Configations;
			Dictionary<string, List<Member>> methods = extClass.Members.Methods;
			Method firstMethodVariant;
			foreach (var methodItem in methods) {
				// Method has no variants:
				if (methodItem.Value.Count == 0) continue;
				firstMethodVariant = methodItem.Value[0] as Method;
				// If method is constructor - do not render it in config interface object between other methods:
				if (firstMethodVariant.IsConstructor) continue;
				// Mixed template method with config will be rendered between configs, 
				// so continue to standard rendering only if not mixed:
				if (!configs.ContainsKey(firstMethodVariant.Name)) 
					// Standard method variants rendering:
					this.generateClassMethodVariants(extClass, methodItem.Value, false);
			}
		}

		protected void generateInterfaceDefinitions(ExtClass extClass) {
			bool clsProcessing = true;
			bool priv = this.processor.GeneratePrivateMembers;
			// Generate indexers:
			foreach (var indexerItem in extClass.Members.Indexers) 
				this.generateMemberIndexer(extClass, indexerItem.Value as Indexer);

			// Generate static properties:
			this.generateProperties(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateProperties(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateProperties(extClass, false, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, false, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassProperties(extClass, false, clsProcessing, AccessModifier.PRIVATE, AccessModifier.PRIVATE);

			// Generate instance properties:
			this.generateProperties(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateProperties(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateProperties(extClass, true, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, true, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassProperties(extClass, true, clsProcessing, AccessModifier.PRIVATE, AccessModifier.PRIVATE);

			// Generate static methods:
			this.generateMethods(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateMethods(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, false, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateMethods(extClass, false, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, false, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassMethods(extClass, false, clsProcessing, AccessModifier.PRIVATE, AccessModifier.PRIVATE);

			// Generate instance methods:
			this.generateMethods(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateMethods(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, true, clsProcessing, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateMethods(extClass, true, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, true, clsProcessing, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassMethods(extClass, true, clsProcessing, AccessModifier.PRIVATE, AccessModifier.PRIVATE);
		}

		protected void generateInterfaceStaticPropertiesAndMethods(ExtClass extClass) {
			bool priv = this.processor.GeneratePrivateMembers;
			// Generate static properties:
			this.generateProperties(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateProperties(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateProperties(extClass, false, false, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateProperties(extClass, false, false, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassProperties(extClass, false, false,AccessModifier.PRIVATE, AccessModifier.PRIVATE);
			
			// Generate static methods:
			this.generateMethods(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
			this.generateMethods(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, false, false, AccessModifier.PUBLIC, AccessModifier.PRIVATE);
			this.generateMethods(extClass, false, false, AccessModifier.PROTECTED, AccessModifier.PROTECTED);
			if (priv) this.generateMethods(extClass, false, false, AccessModifier.PROTECTED, AccessModifier.PRIVATE);
			// this.generateClassMethods(extClass, false, false, AccessModifier.PRIVATE, AccessModifier.PRIVATE);
		}

		protected void generateInterfaceEvents(ExtClass extClass) {
			string evntName;
			List<Member> evntVariants;
			Event firstEventVariant;
			//Dictionary<string, Member> instanceProps = extClass.Members.Properties;
			//Dictionary<string, List<Member>> instanceMethods = extClass.Members.Methods;
			foreach (var item in extClass.Members.Events) {
				evntName = item.Key;
				evntVariants = item.Value;
				if (evntVariants.Count == 0) continue;
				firstEventVariant = evntVariants.FirstOrDefault<Member>() as Event;
				/*if (instanceProps.ContainsKey(evntName) && instanceMethods.ContainsKey(evntName)) {
					// Event name exists also between instance properties and also between instance methods:
					throw new Exception(
						$"Rendering for mixed member between events, properties and methods is not implemented."
					);
				} else if (instanceProps.ContainsKey(evntName)) {
					// Event name exists also between instance properties:
					//Debugger.Break();
				} else if (instanceMethods.ContainsKey(evntName)) {
					// Event name exists also between instance methods:
					//Debugger.Break();
				} else {*/
					// Standard events rendering:
					foreach (Member evntVariant in evntVariants) 
						this.generateInterfaceEvent(
							extClass, evntVariant as Event
						);
				//}
			}
		}
		protected void generateInterfaceEventConfigProps (ExtClass extClass) {
			// Generate all configurations not in events (only in interface Ext.base.EventConfig):
			Dictionary<string, List<Member>> events = extClass.Members.Events;
			Configuration cfg;
			foreach (var cfgItem in extClass.Members.Configations) {
				cfg = cfgItem.Value as Configuration;
				// Mixed configuration with event is not rendered, 
				// so continue to standard rendering only if not mixed:
				if (!events.ContainsKey(cfg.Name)) 
					// Standard configuration rendering:
					this.generateInterfaceConfiguration(extClass, cfg);
			}
		}

		protected void generateInterfaceMethodParamConfigObject(ExtClass extClass) {
			// This class type has only configuration members:
			// TODO: tady renderovat config props:
			string cfgPropName;
			ConfigProperty cfgPropItem;
			foreach (var item in extClass.Members.Properties) {
				cfgPropName = item.Key;
				cfgPropItem = item.Value as ConfigProperty;
				// Standard configuration rendering:
				this.generateInterfacePropertyConfiguration(
					extClass, cfgPropItem
				);
			}
		}

		protected void generateInterfaceMethodReturnObject(ExtClass extClass) {
			// This class type has only public instance properties and public static properties:
			this.generateProperties(extClass, true, false, AccessModifier.PUBLIC, AccessModifier.PUBLIC);
		}
	}
}
