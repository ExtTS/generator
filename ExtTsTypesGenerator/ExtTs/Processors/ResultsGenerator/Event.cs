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
		protected void generateInterfaceEvent (ExtClass extClass, Event eventVariant) {
			// Generate TypeScript doc comments for standard configuration:
			if (this.processor.GenerateJsDocs)
				this.generateEventJsDocs(extClass, eventVariant);
			// generate TypeScript definition code:
			string line = eventVariant.Name + "? ("
				+ this.generateMethodParams(extClass, eventVariant.Params)
			+ "): void;";
			this.writeResultLine(line);
		}
		protected void generateEventJsDocs (ExtClass extClass, Event eventVariant) {
			List<string> docLines = new List<string>();
			if (eventVariant.Doc != null && eventVariant.Doc.Length > 0)
				docLines.AddRange(eventVariant.Doc);
			docLines.Add("@event");
			this.generateMemberDocCommentDeprecated(
				ref docLines, eventVariant
			);
			this.generateEventJsDocsParams(eventVariant, ref docLines);
			if (docLines.Count == 1) {
				this.writeResultLine("/** " + docLines[0] + " */");
			} else {
				this.writeResultLine("/** ");
				foreach (string docLine in docLines)
					this.writeResultLine(" * " + docLine);
				this.writeResultLine(" */");
			}
		}
		protected void generateEventJsDocsParams (Event eventVariant, ref List<string> docLines) {
			// Add possible underlying type for type defined as static property of some class:
			List<List<string>> newDocLines = new List<List<string>>();
			List<string> newDocLine;
			List<string> paramRawTypes;
			List<string> paramFuncTypes;
			List<string> paramSimpleTypes;
			List<string> paramTypes;
			bool[] fixedOptionals = new bool[eventVariant.Params.Count];
			bool optional = true;
			Param methodVariantParam;
			ExtClass methodParamCallbackType;
			int methodParamTypeEnd;
			bool methodParamTypeArr;
			bool methodParamIsFunction;
			Callback methodParamCallback;
			for (int i = eventVariant.Params.Count - 1; i >= 0; i -= 1) {
				methodVariantParam = eventVariant.Params[i];
				optional = optional && methodVariantParam.Optional;
				fixedOptionals[i] = optional;
			}
			for (int i = 0; i < eventVariant.Params.Count; i++) {
				methodVariantParam = eventVariant.Params[i];
				optional = fixedOptionals[i];
				// @param
				newDocLine = new List<string>() { "@param" };
				// {...number[]|...string[]} or {number|string}
				paramRawTypes = this.generateMethodUnderlyingTypes(methodVariantParam.Types);
				paramFuncTypes = new List<string>();
				paramSimpleTypes = new List<string>();
				methodParamCallback = null;
				foreach (string paramRawType in paramRawTypes) {
					methodParamTypeArr = paramRawType.EndsWith("[]");
					methodParamTypeEnd = paramRawType.IndexOf("[]");
					methodParamCallbackType = methodParamTypeArr
						? this.processor.Store.GetPossibleCallbackType(
							paramRawType.Substring(0, methodParamTypeEnd)
						  )
						: this.processor.Store.GetPossibleCallbackType(paramRawType);
					if (methodParamCallbackType == null) {
						// Param type definition is simple type string:
						methodParamIsFunction = methodParamTypeArr
							? paramRawType.Substring(0, methodParamTypeEnd) == "Function"
							: paramRawType == "Function";
						if (methodParamIsFunction) {
							paramFuncTypes.Add(paramRawType);
						} else {
							paramSimpleTypes.Add(paramRawType);
						}
					} else {
						// Param definition is callback function - render function directly inline:
						paramFuncTypes.Add(
							"Function" + (methodParamTypeArr 
								? paramRawType.Substring(methodParamTypeEnd) 
								: "")
						);
						// Render separate callback params later:
						methodParamCallback = methodParamCallbackType as Callback;
					}
				}
				paramTypes = new List<string>();
				foreach (string paramFuncType in paramFuncTypes) 
					if (!paramTypes.Contains(paramFuncType))
						paramTypes.Add(paramFuncType);
				foreach (string paramSimpleType in paramSimpleTypes) 
					if (!paramTypes.Contains(paramSimpleType))
						paramTypes.Add(paramSimpleType);
				if (methodVariantParam.IsRest) {
					newDocLine.Add("{..." + String.Join("|...", paramTypes) + "}");
				} else {
					newDocLine.Add("{" + String.Join("|", paramTypes) + "}");
				}
				// [paramName] or paramName
				newDocLine.Add(
					optional
						? "[" + methodVariantParam.Name + "]"
						: methodVariantParam.Name
				);
				// Param docs text
				if (methodVariantParam.Docs != null)
					newDocLine.AddRange(methodVariantParam.Docs);
				// Add to complete result collection:
				newDocLines.Add(newDocLine);
				// Render callback params:
				if (methodParamCallback != null) { 
					foreach (Param callbackParam in methodParamCallback.Params) {
						newDocLine = new List<string>() { "@param" };
						newDocLine.Add("{" + String.Join("|", callbackParam.Types) + "}");
						newDocLine.Add(
							callbackParam.Optional
								? "[" + methodVariantParam.Name + "." + callbackParam.Name + "]"
								: methodVariantParam.Name + "." + callbackParam.Name
						);
						if (callbackParam.Docs != null)
							newDocLine.AddRange(callbackParam.Docs);
						newDocLines.Add(newDocLine);
					}
					newDocLine = new List<string>() { "@param" };
					newDocLine.Add("{" + String.Join("|", methodParamCallback.ReturnTypes) + "}");
					newDocLine.Add(
						methodVariantParam.Optional
							? "[" + methodVariantParam.Name + ".returns]"
							: methodVariantParam.Name + ".returns"
					);
					if (methodParamCallback.ReturnDocs != null)
						newDocLine.AddRange(methodParamCallback.ReturnDocs);
					newDocLines.Add(newDocLine);
				}
			}
			newDocLine = new List<string>() { "@returns", "{void}", "" };
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
