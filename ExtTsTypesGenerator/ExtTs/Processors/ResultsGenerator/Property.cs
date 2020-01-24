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
		protected void generateClassProperty (ExtClass extClass, Property prop, bool classProcessing = true) {
			if (this.processor.GenerateJsDocs) { 
				if (prop.SingletonInstance != null) { 
					// Generate TypeScript doc comments for singleton class instance:
					this.generateClassDocs(prop.SingletonInstance, true);
				} else {
					// Generate TypeScript doc comments for standard property:
					this.generatePropertyJsDocs(extClass, prop, classProcessing);
				}
			}
			// generate TypeScript definition code:
			//string delimiter = classProcessing ? ": " : "?: ";
			string delimiter = "?: ";
			string line = this.generatePropertyFlags(extClass, prop, classProcessing);
			if (prop.Name == "self" && !prop.IsStatic) {
				line += prop.Name + delimiter + this.generateAllParentsPropertyTypes(extClass, prop) + ";";
			} else {
				line += prop.Name + delimiter + this.generatePropertyTypes(prop) + ";";
			}
			this.writeResultLine(line);
		}
		protected void generatePropertyJsDocs (ExtClass extClass, Property prop, bool classProcessing) {
			List<string> docLines = new List<string>();
			if (prop.Doc != null && prop.Doc.Length > 0)
				docLines.AddRange(prop.Doc);
			docLines.Add("@property");
			if (prop.AccessModJs != AccessModifier.NONE)
				docLines.Add("@" + AccessModifiers.Values[prop.AccessModJs] + " (property)");
			if (prop.IsStatic)
				docLines.Add("@static");
			if (prop.IsReadOnly)
				docLines.Add("@readonly");
			if (!String.IsNullOrEmpty(prop.DefaultValue)) 
				docLines.Add(
					"@default " + prop.DefaultValue.Replace("*/", "*\\/")
				);
			this.generateMemberDocCommentDeprecated(
				ref docLines, prop
			);
			this.generatePropertyDocCommentTypes(
				ref docLines, prop
			);
			if (docLines.Count == 1) {
				this.writeResultLine("/** " + docLines[0] + " */");
			} else {
				this.writeResultLine("/** ");
				foreach (string docLine in docLines)
					this.writeResultLine(" * " + docLine);
				this.writeResultLine(" */");
			}
		}
		protected void generatePropertyDocCommentTypes (ref List<string> docLines, Property prop) {
			JsDocsPropTypes typeCollections = new JsDocsPropTypes() {
				Current = new List<string>(),
				Parent = new List<string>(),
				AnyCompatible = null,
			};
			Dictionary<string, ExistenceReason> compatibleTypes;
			string compatibleClassFullName;
			ExtClass compatibleClass;
			Dictionary<string, Member> compatibleProps;
			foreach (var item in prop.Types) {
				if (item.Value.Type == ExistenceReasonType.COMPATIBLE_TYPES) { // it could be only "any":
					compatibleClassFullName = item.Value.CompatibilityReasonClassFullName;
					typeCollections.AnyCompatible = compatibleClassFullName;
					compatibleClass = this.processor.Store.GetByFullName(compatibleClassFullName);
					if (compatibleClass != null) {
						compatibleProps = prop.IsStatic
							? compatibleClass.Members.PropertiesStatic
							: compatibleClass.Members.Properties;
						compatibleTypes = (compatibleProps[prop.Name] as Property).Types;
						typeCollections.Parent = new List<string>(
							compatibleTypes.Keys.ToArray<string>()
						);
					}
				} else {
					typeCollections.Current.Add(item.Key);
				}
			}
			// If there are any parent class types (because of compatibility),
			// filter them for current types.
			if (typeCollections.Parent.Count > 0 && typeCollections.Current.Count > 0) 
				foreach (string currentType in typeCollections.Current) 
					if (typeCollections.Parent.Contains(currentType))
						typeCollections.Parent.Remove(currentType);
			Dictionary<string, string> result = new Dictionary<string, string>();
			if (typeCollections.Current.Count > 0)
				result.Add(
					"@type {" + String.Join("|", typeCollections.Current) + "}",
					"In current class."
				);
			if (typeCollections.Parent.Count > 0 && typeCollections.AnyCompatible != null) { 
				result.Add(
					"@type {" + String.Join("|", typeCollections.Parent) + "}", 
					"In parent class `" + typeCollections.AnyCompatible + "`."
				);
				result.Add(
					"@type {any}", 
					"TS compatibility for types in parent class: `" + typeCollections.AnyCompatible + "`."
				);
			}
			// Try to find longest type definition value
			if (result.Count == 1) {
				docLines.Add(result.Keys.ToList<string>().FirstOrDefault<string>());
			} else {
				int longestKeyValue = 0;
				foreach (var item in result)
					if (item.Key.Length > longestKeyValue)
						longestKeyValue = item.Key.Length;
				foreach (var item in result)
					docLines.Add(
						item.Key
						+ " "
						+ "".PadLeft(longestKeyValue - item.Key.Length, ' ')
						+ item.Value
					);
			}
		}
		protected string generatePropertyFlags (ExtClass extClass, Property prop, bool classProcessing) {
			List<string> flags = new List<string>();
			bool renderedAsSingletonInterface = extClass.Singleton && extClass.Name.FullName != "Ext";
			if (classProcessing && !renderedAsSingletonInterface) {
				
				// UPDATE for constructing .Def classes: do not render access modifiers anywhere!
				/*
				if (prop.AccessModTs == AccessModifier.NONE)
					prop.AccessModTs = prop.AccessModJs;
				if (prop.AccessModTs == AccessModifier.NONE)
					prop.AccessModTs = AccessModifier.PUBLIC;
				flags.Add(AccessModifiers.Values[prop.AccessModTs]);
				*/

				if (prop.IsStatic || extClass.Name.FullName == "Ext")
					flags.Add("static");
				if (prop.IsReadOnly)
					flags.Add("readonly");
			}
			string result = String.Join(" ", flags);
			if (flags.Count > 0) result += " ";
			return result;
		}
		protected string generatePropertyTypes (Property prop) {
			List<string> items = new List<string>();
			string compatibleComment = "";
			Dictionary<string, ExistenceReason> compatibleTypes;
			string compatibleClassFullName;
			ExtClass compatibleClass;
			Dictionary<string, Member> compatibleProps;
			string compatibleTypesDef;
			List<string> staticPropTypes;
			Dictionary<string, string> underlyingTypes = new Dictionary<string, string>();
			Dictionary<string, ExistenceReason> allTypes = new Dictionary<string, ExistenceReason>(prop.Types);
			foreach (var item in prop.Types) {
				if (this.processor.Store.StaticPropsTypes.ContainsKey(item.Key)) {
					staticPropTypes = this.processor.Store.StaticPropsTypes[item.Key];
					foreach (string staticPropType in staticPropTypes) {
						if (underlyingTypes.ContainsKey(staticPropType)) {
							underlyingTypes[staticPropType] += ", " + item.Key;
						} else {
							underlyingTypes.Add(staticPropType, item.Key);
						}
					}
				}
			}
			if (underlyingTypes.Count > 0) {
				foreach (var item in underlyingTypes) {
					if (allTypes.ContainsKey(item.Key)) continue;
					allTypes.Add(item.Key, new ExistenceReason() {
						Type = ExistenceReasonType.COMPATIBLE_TYPES,
						CompatibilityReasonClassFullName = item.Value
					});
				}
			}
			string propType;
			foreach (var item in allTypes) {
				propType = this.checkBrowserGlobalClass(item.Key);
				compatibleComment = "";
				if (item.Value.Type == ExistenceReasonType.COMPATIBLE_TYPES) {
					compatibleClassFullName = item.Value.CompatibilityReasonClassFullName;
					compatibleClass = this.processor.Store.GetByFullName(compatibleClassFullName);
					if (compatibleClass != null) {
						compatibleProps = prop.IsStatic
							? compatibleClass.Members.PropertiesStatic
							: compatibleClass.Members.Properties;
						compatibleTypes = (compatibleProps[prop.Name] as Property).Types;
						compatibleTypesDef = "["+String.Join("|", compatibleTypes.Keys.ToArray<string>())+"]";
					} else {
						compatibleTypesDef = "";
					}
					if (this.processor.GenerateJsDocs)
						compatibleComment = "/* @compatible " 
							+ compatibleClassFullName + "." + prop.Name
							+ compatibleTypesDef
							+ " */ ";

				}
				items.Add(compatibleComment + propType);
			}
			return String.Join(" | ", items);
		}
		protected string generateAllParentsPropertyTypes (ExtClass extClass, Property prop) {
			List<string> allTypes = new List<string>(prop.Types.Keys.ToArray<string>());
			ExtClass currentClass = extClass;
			ExtClass parentClass = null;
			Store store = this.processor.Store;
			Dictionary<string, Member> propsCollection;
			Property parentProp;
			while (true) {
				if (currentClass.Extends == null)
					break;
				parentClass = store.GetParentClassByCurrentClassFullName(currentClass.Name.FullName);
				if (parentClass == null)
					break;
				propsCollection = prop.IsStatic
					? parentClass.Members.PropertiesStatic
					: parentClass.Members.Properties;
				if (propsCollection.ContainsKey(prop.Name)) {
					parentProp = propsCollection[prop.Name] as Property;
					foreach (var item in parentProp.Types) {
						if (allTypes.Contains(item.Key)) continue;
						allTypes.Add(item.Key);
					}
				}
				currentClass = parentClass;
			}
			return String.Join(" | ", allTypes);
		}
	}
}
