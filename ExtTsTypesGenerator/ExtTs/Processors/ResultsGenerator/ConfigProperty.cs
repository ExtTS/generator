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
		protected void generateInterfacePropertyConfiguration (ExtClass extClass, ConfigProperty configProp) {
			if (this.processor.GenerateJsDocs) {
				// Generate TypeScript doc comments for standard property:
				this.generateConfigPropertyJsDocs(extClass, configProp);
			}

			// generate TypeScript definition code:
			string line = configProp.Name + (configProp.Required ? ": " : "?: ") 
				+ this.generateConfigPropertyTypes(configProp) 
				+ ";";
			this.writeResultLine(line);
		}
		protected string generateConfigPropertyTypes (ConfigProperty configProp) {
			// Add possible underlying type for type defined as static property of some class:
			Dictionary<string, ExistenceReason> allTypes = this.generateConfigPropertyUnderlyingTypes(configProp);
			List<string> resultCallbackTypes = new List<string>();
			List<string> resultFuncTypes = new List<string>();
			List<string> resultSimpleTypes = new List<string>();
			List<string> resultTypes = new List<string>();
			ExtClass configPropCallbackType;
			int configPropTypeEnd;
			bool configPropTypeArr;
			bool configPropTypeIsFunction;
			string configPropType;
			string compatibleComment;
			string compatibleClassFullName;
			ExtClass compatibleClass;
			Dictionary<string, Member> compatibleProps;
			Dictionary<string, ExistenceReason> compatibleTypes;
			string compatibleTypesDef;
			foreach (var allTypeItem in allTypes) {
				configPropType = allTypeItem.Key;
				configPropTypeArr = configPropType.EndsWith("[]");
				configPropTypeEnd = configPropType.Length - 2;
				configPropCallbackType = configPropTypeArr
					? this.processor.Store.GetPossibleCallbackType(
						configPropType.Substring(0, configPropTypeEnd)
					  )
					: this.processor.Store.GetPossibleCallbackType(configPropType);
				if (configPropCallbackType == null) {
					// Param type definition is simple type string:
					configPropTypeIsFunction = configPropTypeArr
						? configPropType.Substring(0, configPropTypeEnd) == "Function"
						: configPropType == "Function";
					if (configPropTypeIsFunction) {
						resultFuncTypes.Add(
							this.checkBrowserGlobalClass(configPropType)
						);
					} else {
						compatibleComment = "";
						if (allTypeItem.Value.Type == ExistenceReasonType.COMPATIBLE_TYPES) {
							compatibleClassFullName = allTypeItem.Value.CompatibilityReasonClassFullName;
							compatibleClass = this.processor.Store.GetByFullName(compatibleClassFullName);
							if (compatibleClass != null) {
								compatibleProps = compatibleClass.Members.Properties;
								compatibleTypes = (compatibleProps[configProp.Name] as Property).Types;
								compatibleTypesDef = "["+String.Join("|", compatibleTypes.Keys.ToArray<string>())+"]";
							} else {
								compatibleTypesDef = "";
							}
							if (this.processor.GenerateJsDocs)
								compatibleComment = "/* @compatible " 
									+ compatibleClassFullName + "." + configProp.Name
									+ compatibleTypesDef
									+ " */ ";

						}
						resultSimpleTypes.Add(
							compatibleComment + this.checkBrowserGlobalClass(configPropType)
						);
					}
				} else {
					// Param definition is callback function - render function directly inline:
					resultCallbackTypes.Add(
						this.generateMethodParamTypeCallback(
							configPropCallbackType as Callback
						)
					);
				}
			}
			if (resultSimpleTypes.Count > 0 || resultFuncTypes.Count > 0) {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add("(" + resultCallbackType + ")");
			} else {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add(resultCallbackType);
			}
			foreach (string resultFuncType in resultFuncTypes) 
				resultTypes.Add(resultFuncType);
			foreach (string resultSimpleType in resultSimpleTypes) 
				resultTypes.Add(resultSimpleType);
			
			return String.Join(" | ", resultTypes);
		}
		protected Dictionary<string, ExistenceReason> generateConfigPropertyUnderlyingTypes (ConfigProperty configProp) {
			Dictionary<string, ExistenceReason> allTypes = new Dictionary<string, ExistenceReason>(configProp.Types);
			string configPropType;
			List<string> staticPropTypes;
			List<string> underlyingTypes = new List<string>();
			foreach (var configPropTypeItem in configProp.Types) {
				configPropType = configPropTypeItem.Key;
				if (this.processor.Store.StaticPropsTypes.ContainsKey(configPropType)) {
					staticPropTypes = this.processor.Store.StaticPropsTypes[configPropType];
					foreach (string staticPropType in staticPropTypes) 
						if (!underlyingTypes.Contains(staticPropType)) 
							underlyingTypes.Add(staticPropType);
				}
			}
			if (underlyingTypes.Count > 0) 
				foreach (string underlyingType in underlyingTypes) 
					if (!allTypes.ContainsKey(underlyingType))
						allTypes.Add(underlyingType, new ExistenceReason() {
							CompatibilityReasonClassFullName = null,
							Type = ExistenceReasonType.NATURAL
						});
			return allTypes;
		}
		protected void generateConfigPropertyJsDocs (ExtClass extClass, ConfigProperty configProp) {
			List<string> docLines = new List<string>();
			if (configProp.Doc != null && configProp.Doc.Length > 0)
				docLines.AddRange(configProp.Doc);
			docLines.Add("@configuration");
			if (configProp.Required) { 
				docLines.Add("@required");
			} else {
				docLines.Add("@optional");
			}
			if (!String.IsNullOrEmpty(configProp.DefaultValue)) 
				docLines.Add(
					"@default " + configProp.DefaultValue.Replace("*/", "*\\/")
				);
			this.generateMemberDocCommentDeprecated(
				ref docLines, configProp
			);
			this.generatePropertyDocCommentTypes(
				ref docLines, configProp as Property
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
	}
}
