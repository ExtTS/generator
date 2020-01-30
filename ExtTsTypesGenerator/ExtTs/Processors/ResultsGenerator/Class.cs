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
		protected void generateClass (ExtClass extClass) {
			ExtClass parentClass;
			//if (extClass.Name.FullName == "Ext.draw.TimingFunctions")
			//	Debugger.Break();
			if (
				this.processor.GenerateJsDocs && 
				!extClass.Private && 
				extClass.HasMembers && 
				extClass.ClassType != ClassType.CLASS_ALIAS
			)
				this.generateClassDocs(extClass);
			if (
				extClass.ClassType == ClassType.CLASS_STANDARD ||
				extClass.ClassType == ClassType.CLASS_DEFINITIONS
			) {
				// Singleton and non-singleton classes:
				if (extClass.HasMembers && !extClass.Private) {
					this.generateClassOpen(extClass);
					this.generateClassMembers(extClass);
					this.generateClassClose();
				} else {
					this.generateClassOpenClose(extClass);
				}
			} else if (extClass.ClassType == ClassType.CLASS_ALIAS) {
				// Check class alias parent to be a singleton or not. If parent is singleton 
				// (and not "Ext" class), render class alias as interface, else render as standard class alias:
				parentClass = extClass.Extends != null
					? this.processor.Store.GetByFullName(extClass.Extends.FullName)
					: null;
				if (parentClass != null && parentClass.Singleton && parentClass.Name.FullName != "Ext") {
					// Render singleton alias classes as interface
					if (extClass.HasMembers) {
						this.generateInterfaceOpen(extClass);
						this.generateInterfaceMembers(extClass);
						this.generateInterfaceClose();
					} else {
						this.generateInterfaceOpenClose(extClass);
					}
				} else {
					// Render standard non-singleton alias classes in standard way:
					if (extClass.HasMembers) {
						this.generateClassOpen(extClass);
						this.generateClassMembers(extClass);
						this.generateClassClose();
					} else {
						this.generateClassOpenClose(extClass);
					}
				}
			}
		}
		protected void generateClassDocs (ExtClass extClass, bool singletonInstancePlace = false) {
			if (
				(extClass.Docs != null && extClass.Docs.Length > 0) || 
				extClass.Private || 
				extClass.Singleton || 
				extClass.Deprecated != null
			) {
				this.writeResultLine("/** ");
				string classLinkHref = this.processor.Reader.GetLinkHrefForClass(
					extClass.Name.FullName
				);
				if (!extClass.Private && !extClass.Name.FullName.StartsWith("Ext.base."))
					this.writeResultLine(
						$" * [{extClass.Name.FullName}]({classLinkHref})"
					);
				for (int i = 0; i < extClass.Docs.Length; i++) 
					this.writeResultLine(" * " + extClass.Docs[i]);
				if (extClass.Private)
					this.writeResultLine(" * @private (class)");	
				if (extClass.Singleton)
					this.writeResultLine(" * @singleton " + (singletonInstancePlace ? "(instance)" : "(definition)"));
				if (extClass.Deprecated != null)
					this.writeResultLine(" * @deprecated");
				this.writeResultLine(" */");
			}
		}
		protected void generateClassOpen (ExtClass extClass) {
			string line = "";
			if (!extClass.Name.IsInModule)
				line += "declare ";
			if ((extClass.Singleton && extClass.Name.FullName != "Ext") || extClass.ClassType == ClassType.CLASS_DEFINITIONS) {
				// Render singleton classes as interfaces and public static props on classes named as it's namespace:
				line += "interface " + extClass.Name.ClassName;
			} else {
				// Render standard non-singleton classes in standard way:
				line += "class " + extClass.Name.ClassName;
			}
			if (extClass.Extends != null) 
				line += " extends " + this.checkBrowserGlobalClass(extClass.Extends.FullName);
			line += " {";
			this.writeResultLine(line);
			this.whileSpaceLevel += 1;
		}
		protected void generateClassOpenClose (ExtClass extClass) {
			string line = "";
			if (!extClass.Name.IsInModule)
				line += "declare ";
			if ((extClass.Singleton && extClass.Name.FullName != "Ext") || extClass.ClassType == ClassType.CLASS_DEFINITIONS) {
				// Render singleton classes as interfaces and public static props on classes named as it's namespace:
				line += "interface " + extClass.Name.ClassName;
			} else {
				// Render standard non-singleton classes in standard way:
				line += "class " + extClass.Name.ClassName;
			}
			if (extClass.Extends != null) { 
				string extGlobalClassName = SpecialsGenerator.GLOBAL_CLASS_BASE
					.Replace("<browserGlobalClassName>", extClass.Extends.FullName);
				if (
					extClass.ClassType == ClassType.CLASS_ALIAS && 
					extClass.Name.FullName == extGlobalClassName
				) {
					// Global browser object proxy class alias:
					line += " extends " + extClass.Extends.FullName;
				} else {
					line += " extends " + this.checkBrowserGlobalClass(extClass.Extends.FullName);
				}
			}
			line += " {}";
			this.writeResultLine(line);
		}
		protected void generateClassClose () {
			this.whileSpaceLevel -= 1;
			if (this.whileSpaceLevel == -1)
				Debugger.Break();
			this.writeResultLine("}");
		}
		protected void generateClassMembers (ExtClass extClass) {
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
			//this.generateClassProperties(extClass, false, clsProcessing, AccessModifier.PRIVATE, AccessModifier.PRIVATE);

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
		protected void generateProperties(ExtClass extClass, bool instanceMembers, bool classProcessing, AccessModifier typeScriptAccessMod, AccessModifier javascriptAccessMod) {
			Dictionary<string, Member> props = instanceMembers
				? extClass.Members.Properties
				: extClass.Members.PropertiesStatic;
			Dictionary<string, List<Member>> methods = instanceMembers
				? extClass.Members.Methods
				: extClass.Members.MethodsStatic;
			Property prop;
			bool renderedAsSingletonInterface = extClass.Singleton && extClass.Name.FullName != "Ext";
			foreach (var propItem in props) {
				prop = propItem.Value as Property;
				// Property is not necessary to render, is extended naturaly in the same form:
				if (!prop.Renderable) continue;
				// Property is not possible to render, because class is rendered as singleton interface,
				// where is not possible to render inherited properties:
				if (renderedAsSingletonInterface && prop.Inherited) continue;
				// If property has currently required access modifiers:
				if (
					(prop.AccessModTs & typeScriptAccessMod) != 0 &&
					(prop.AccessModJs & javascriptAccessMod) != 0
				) {
					// Mixed member with method will be rendered between methods, 
					// so continue to standard rendering only if not mixed:
					if (!methods.ContainsKey(prop.Name)) 
						// Standard property rendering:
						this.generateClassProperty(extClass, prop, classProcessing);
				}
			}
		}
		protected void generateMethods(ExtClass extClass, bool instanceMembers, bool classProcessing, AccessModifier typeScriptAccessMod, AccessModifier javascriptAccessMod) {
			Dictionary<string, Member> props = instanceMembers
				? extClass.Members.Properties
				: extClass.Members.PropertiesStatic;
			Dictionary<string, List<Member>> methods = instanceMembers
				? extClass.Members.Methods
				: extClass.Members.MethodsStatic;
			Method firstMethodVariant;
			Property mixedProp;
			bool renderedAsSingletonInterface = extClass.Singleton && extClass.Name.FullName != "Ext";
			foreach (var methodItem in methods) {
				// Method has no variants:
				if (methodItem.Value.Count == 0) continue;
				firstMethodVariant = methodItem.Value[0] as Method;
				// Method variants are not necessary to render, they are extended naturaly in the same form:
				if (!firstMethodVariant.Renderable) continue;
				// Method variants are not possible to render, because class is rendered as singleton interface,
				// where is not possible to render inherited method variants:
				if (renderedAsSingletonInterface && firstMethodVariant.Inherited) continue;
				// If method variants have currently required access modifiers:
				if (
					(firstMethodVariant.AccessModTs & typeScriptAccessMod) != 0 &&
					(firstMethodVariant.AccessModJs & javascriptAccessMod) != 0
				) {
					if (props.ContainsKey(firstMethodVariant.Name)) {
						// Mixed method variants with property - always caused by two mixins classes:
						mixedProp = props[firstMethodVariant.Name] as Property;
						this.generateClassMethodVariantsWithProperty(extClass, methodItem.Value, mixedProp, classProcessing);
					} else {
						// Standard method variants rendering:
						this.generateClassMethodVariants(extClass, methodItem.Value, classProcessing);
					}
				}
			}
		}
	}
}
