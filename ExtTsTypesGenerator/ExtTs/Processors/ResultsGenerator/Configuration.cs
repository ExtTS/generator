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
		protected void generateInterfaceConfiguration (ExtClass extClass, Configuration config) {
			// Generate TypeScript doc comments for standard configuration:
			if (this.processor.GenerateJsDocs)
				this.generateConfigurationJsDocs(extClass, config);
			// generate TypeScript definition code:
			string line = config.Name + (config.Required ? ": " : "?: ") 
				+ this.generateMethodTypes(config.Types, false, false) 
				+ ";";
			/*string line = config.Name + "?: "
				+ this.generateMethodTypes(config.Types, false, false) 
				+ ";";*/
			this.writeResultLine(line);
		}
		protected void generateConfigurationJsDocs (ExtClass extClass, Configuration config) {
			List<string> docLines = new List<string>();
			if (config.Doc != null && config.Doc.Length > 0)
				docLines.AddRange(config.Doc);
			if (extClass.ClassType == ClassType.CLASS_CONFIGS)
				docLines.Add("@configuration");
			if (config.Required) { 
				docLines.Add("@required");
			} else {
				docLines.Add("@optional");
			}
			if (extClass.ClassType == ClassType.CLASS_CONFIGS)
				if (!String.IsNullOrEmpty(config.DefaultValue)) 
					docLines.Add(
						"@default " + config.DefaultValue.Replace("*/", "*\\/")
					);
			this.generateMemberDocCommentDeprecated(
				ref docLines, config
			);
			this.generateConfigurationDocCommentTypes(
				ref docLines, config
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
		protected void generateConfigurationDocCommentTypes (ref List<string> docLines, Configuration config) {
			// Add possible underlying type for type defined as static property of some class:
			List<string> allTypes = this.generateMethodUnderlyingTypes(config.Types);
			List<string> resultCallbackTypes = new List<string>();
			List<string> resultFuncTypes = new List<string>();
			List<string> resultSimpleTypes = new List<string>();
			List<string> resultTypes = new List<string>();
			ExtClass methodParamCallbackType;
			int methodParamTypeEnd;
			bool methodParamTypeArr;
			bool methodParamIsFunction;
			foreach (string methodParamOrReturnType in allTypes) {
				methodParamTypeArr = methodParamOrReturnType.EndsWith("[]");
				methodParamTypeEnd = methodParamOrReturnType.Length - 2;
				methodParamCallbackType = methodParamTypeArr
					? this.processor.Store.GetPossibleCallbackType(
						methodParamOrReturnType.Substring(0, methodParamTypeEnd)
					  )
					: this.processor.Store.GetPossibleCallbackType(methodParamOrReturnType);
				if (methodParamCallbackType == null) {
					// Param type definition is simple type string:
					methodParamIsFunction = methodParamTypeArr
						? methodParamOrReturnType.Substring(0, methodParamTypeEnd) == "Function"
						: methodParamOrReturnType == "Function";
					if (methodParamIsFunction) {
						resultFuncTypes.Add(
							this.checkBrowserGlobalClass(methodParamOrReturnType)
						);
					} else {
						resultSimpleTypes.Add(
							this.checkBrowserGlobalClass(methodParamOrReturnType)
						);
					}
				} else {
					// Param definition is callback function - render function directly inline:
					this.generateConfigurationDocCommentTypesForCallback(
						ref docLines, methodParamCallbackType as Callback
					);
				}
			}
			if (resultSimpleTypes.Count > 0) { 
				// Render doc comments for primitive or class type:
				foreach (string resultFuncType in resultFuncTypes) 
					resultTypes.Add(resultFuncType);
				foreach (string resultSimpleType in resultSimpleTypes) 
					resultTypes.Add(resultSimpleType);
				docLines.Add(
					"@type {" + String.Join("|", resultTypes) + "}"
				);
			}
		}
		protected void generateConfigurationDocCommentTypesForCallback (ref List<string> docLines, Callback callback) {
			List<List<string>> newDocLines = new List<List<string>>();
			List<string> newDocLine;
			foreach (Param callbackParam in callback.Params) {
				newDocLine = new List<string>() { "@param" };
				newDocLine.Add("{" + String.Join("|", callbackParam.Types) + "}");
				newDocLine.Add(
					callbackParam.Optional
						? "[" + callbackParam.Name + "]"
						: callbackParam.Name
				);
				if (callbackParam.Docs != null)
					newDocLine.AddRange(callbackParam.Docs);
				newDocLines.Add(newDocLine);
			}
			newDocLine = new List<string>() { "@returns" };
			newDocLine.Add("{" + String.Join("|", callback.ReturnTypes) + "}");
			if (callback.ReturnDocs != null && callback.ReturnDocs.Length > 0) { 
				newDocLine.AddRange(callback.ReturnDocs);
			} else {
				newDocLine.Add("");
			}
			newDocLines.Add(newDocLine);
			// Compute indent spaces:
			int highestTagsLength = "@returns ".Length;
			int highestTypesLength = 0;
			int highestArgNamesLength = 0;
			foreach (List<string> newDocLinesItem in newDocLines) {
				if (newDocLinesItem[1].Length > highestTypesLength)
					highestTypesLength = newDocLinesItem[1].Length;
				if (newDocLinesItem[2].Length > highestArgNamesLength)
					highestArgNamesLength = newDocLinesItem[2].Length;
			}
			highestTypesLength += 1;
			highestArgNamesLength += 1;
			// Render result with indent spaces:
			string docLine;
			foreach (List<string> newDocLinesItem in newDocLines) {
				// @param | @returns
				docLine = newDocLinesItem[0]
					+ "".PadLeft(highestTagsLength - newDocLinesItem[0].Length, ' ');
				// {types}
				docLine += newDocLinesItem[1]
					+ "".PadLeft(highestTypesLength - newDocLinesItem[1].Length, ' ');
				// [param.names]
				docLine += newDocLinesItem[2] + (newDocLinesItem.Count == 4
					? "".PadLeft(highestArgNamesLength - newDocLinesItem[2].Length, ' ')
					: "");
				if (newDocLinesItem.Count == 4) {
					docLine += newDocLinesItem[3];
					docLines.Add(docLine);
				} else if (newDocLinesItem.Count > 4) {
					docLines.Add(docLine);
					for (int i = 3; i < newDocLinesItem.Count; i++) 
						docLines.Add(newDocLinesItem[i]);
				} else {
					docLines.Add(docLine);
				}
			}
		}
	}
}
