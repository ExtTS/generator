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
		protected void generateClassMethodVariants(ExtClass extClass, List<Member> methodVariants, bool classProcessing = true) {
			// Generate params sections as dictionary keys and methods 
			// as dictionary values to merge them by params sections later:
			List<Method> mergedVariants = this.generateMethodVariantsMergeByParams(
				extClass, methodVariants
			);
			string previouslyRenderedParams = null;
			bool theSameParams;
			Dictionary<string, List<string>> theSameParamsReturns = this.generateMethodTheSameParamsReturns(
				mergedVariants
			);
			Method methodVariantClone;
			bool renderedAsSingletonInterface = (
				extClass.Singleton && extClass.Name.FullName != "Ext"
			);
			bool generateMethodFlags;
			string line;
			string delimiter;
			bool typeofReturn;
			foreach (Method methodVariant in mergedVariants) {
				// Generate TypeScript doc comments:
				theSameParams = (
					previouslyRenderedParams != null &&
					previouslyRenderedParams == methodVariant.ParamsRendered
				);
				previouslyRenderedParams = methodVariant.ParamsRendered;
				if (this.processor.GenerateJsDocs && !theSameParams) {
					if (methodVariant.OwnedByCurrent) { 
						methodVariantClone = methodVariant.Clone();
						methodVariantClone.ReturnTypes = theSameParamsReturns[methodVariant.ParamsRendered];
						this.generateMethodJsDocs(extClass, methodVariantClone);
					} else {
						this.writeResultLine("/** @inheritdoc */");
					}
				}
				// Generate TypeScript definition code:
				generateMethodFlags = (
					classProcessing && 
					!renderedAsSingletonInterface && 
					!methodVariant.IsConstructor
				);
				line = generateMethodFlags
					? this.generateMethodFlags(extClass, methodVariant)
					: "";
				//delimiter = classProcessing || methodVariant.IsConstructor ? " (" : "? (";
				delimiter = methodVariant.IsConstructor ? " (" : "? (";
				line += methodVariant.Name + delimiter + methodVariant.ParamsRendered + ")";
				if (!methodVariant.IsConstructor) {
					typeofReturn = methodVariant.IsChainable && methodVariant.IsStatic;
					line += ": " + this.generateMethodTypes(
						methodVariant.ReturnTypes, false, typeofReturn
					);
				}
				line += ";";
				this.writeResultLine(line);
			}
		}
		protected Dictionary<string, List<string>> generateMethodTheSameParamsReturns (List<Method> mergedVariants) {
			Dictionary<string, List<string>> theSameParamsReturns = new Dictionary<string, List<string>>();
			List<string> theSameParamsReturn = null;
			Method methodVariant;
			bool theSameParams;
			string previouslyRenderedParams = null;
			if (!this.processor.GenerateJsDocs)
				return theSameParamsReturns;
			// rendered params => return types
			for (int i = 0; i < mergedVariants.Count; i++) {
				methodVariant = mergedVariants[i];
				theSameParams = (
					previouslyRenderedParams != null &&
					previouslyRenderedParams == methodVariant.ParamsRendered
				);
				previouslyRenderedParams = methodVariant.ParamsRendered;
				if (!theSameParams) {
					theSameParamsReturn = theSameParamsReturn == null
						? new List<string>(methodVariant.ReturnTypes)
						: theSameParamsReturn;
					if (theSameParamsReturns.ContainsKey(methodVariant.ParamsRendered)) {
						foreach (string returnType in theSameParamsReturn)
							if (!theSameParamsReturns[methodVariant.ParamsRendered].Contains(returnType))
								theSameParamsReturns[methodVariant.ParamsRendered].Add(returnType);
					} else {
						theSameParamsReturns.Add(
							methodVariant.ParamsRendered, theSameParamsReturn
						);
					}
					theSameParamsReturn = new List<string>(methodVariant.ReturnTypes);
				} else {
					theSameParamsReturn = theSameParamsReturn == null
						? new List<string>()
						: theSameParamsReturn;
					foreach (string returnType in methodVariant.ReturnTypes) 
						if (!theSameParamsReturn.Contains(returnType))
							theSameParamsReturn.Add(returnType);
				}
			}
			if (theSameParamsReturn != null) {
				if (theSameParamsReturns.ContainsKey(previouslyRenderedParams)) {
					foreach (string returnType in theSameParamsReturn)
						if (!theSameParamsReturns[previouslyRenderedParams].Contains(returnType))
							theSameParamsReturns[previouslyRenderedParams].Add(returnType);
				} else {
					theSameParamsReturns.Add(
						previouslyRenderedParams, theSameParamsReturn
					);
				}
			}
			return theSameParamsReturns;
		}
		protected List<Method> generateMethodVariantsMergeByParams (ExtClass extClass, List<Member> methodVariants) {
			List<Method> variantsByParams = new List<Method>();
			Dictionary<string, int> renderedParamsAndVariantsIndexes;
			int variantIndex;
			Method methodVariant;
			Method previousMethodVariant;
			string renderedParamsSection;
			List<string> docsMerged;
			int methodVariantAcMod;
			int prevMethodVariantAcMod;
			bool compatibleReasonDetected = false;
			List<string>[] standardAndVoidReturns = new[] { new List<string>(), new List<string>() };
			for (int i = 0; i < methodVariants.Count; i++) {
				methodVariant = methodVariants[i] as Method;
				methodVariant.ParamsRendered = this.generateMethodParams(
					extClass, methodVariant.Params
				);
				foreach (string returnType in methodVariant.ReturnTypes) 
					standardAndVoidReturns[returnType == "void" ? 0 : 1].Add(returnType);
				methodVariants[i] = methodVariant;
				if (
					methodVariant.ExistenceReason == ExistenceReasonType.COMPATIBLE_CHAIN ||
					methodVariant.ExistenceReason == ExistenceReasonType.COMPATIBLE_TYPES
				)
					compatibleReasonDetected = true;
			}
			if (
				compatibleReasonDetected ||
				(standardAndVoidReturns[0].Count > 0 && standardAndVoidReturns[1].Count > 0)
			) {
				for (int i = 0; i < methodVariants.Count; i++) 
					variantsByParams.Add(methodVariants[i] as Method);
			} else {
				renderedParamsAndVariantsIndexes = new Dictionary<string, int>();
				foreach (Member methodVariantsItem in methodVariants) {
					methodVariant = methodVariantsItem as Method;
					//renderedParamsSection = this.generateMethodParams(extClass, methodVariant.Params);
					renderedParamsSection = methodVariant.ParamsRendered;
					if (!renderedParamsAndVariantsIndexes.ContainsKey(renderedParamsSection)) {
						renderedParamsAndVariantsIndexes.Add(renderedParamsSection, variantsByParams.Count);
						variantsByParams.Add(methodVariant);
					} else {
						variantIndex = renderedParamsAndVariantsIndexes[renderedParamsSection];
						previousMethodVariant = variantsByParams[variantIndex];
						// Merge return types:
						foreach (string methodVariantReturnType in methodVariant.ReturnTypes)
							if (!previousMethodVariant.ReturnTypes.Contains(methodVariantReturnType))
								previousMethodVariant.ReturnTypes.Add(methodVariantReturnType);
						// Merge return docs if necessary:
						if (
							previousMethodVariant.ReturnDocs != null &&
							methodVariant.ReturnDocs != null &&
							(!methodVariant.ReturnDocs.SequenceEqual(previousMethodVariant.ReturnDocs))
						) {
							docsMerged = new List<string>(previousMethodVariant.ReturnDocs);
							docsMerged.AddRange(methodVariant.ReturnDocs);
							previousMethodVariant.ReturnDocs = docsMerged.ToArray<string>();
						}
						// Merge deprecated docs if necessary:
						if (
							previousMethodVariant.Deprecated != null &&
							methodVariant.Deprecated != null &&
							(!methodVariant.Deprecated.SequenceEqual(previousMethodVariant.Deprecated))
						) {
							docsMerged = new List<string>(previousMethodVariant.Deprecated);
							docsMerged.AddRange(methodVariant.Deprecated);
							previousMethodVariant.Deprecated = docsMerged.ToArray<string>();
						}
						// Merge access modifiers if necessary:
						methodVariantAcMod = (int)methodVariant.AccessModJs;
						prevMethodVariantAcMod = (int)previousMethodVariant.AccessModJs;
						if (methodVariantAcMod > prevMethodVariantAcMod)
							previousMethodVariant.AccessModJs = methodVariant.AccessModJs;
						methodVariantAcMod = (int)methodVariant.AccessModTs;
						prevMethodVariantAcMod = (int)previousMethodVariant.AccessModTs;
						if (methodVariantAcMod > prevMethodVariantAcMod)
							previousMethodVariant.AccessModTs = methodVariant.AccessModTs;
						variantsByParams[variantIndex] = previousMethodVariant;
					}
				}

			}
			return variantsByParams;
		}
		protected void generateMethodJsDocs(ExtClass extClass, Method methodVariant) {
			List<string> docLines = new List<string>();
			this.generateMethodJsDocsCompatibleWarning(methodVariant, ref docLines);
			if (methodVariant.Doc != null && methodVariant.Doc.Length > 0)
				docLines.AddRange(methodVariant.Doc);
			docLines.Add("@method");
			if (methodVariant.AccessModJs != AccessModifier.NONE)
				docLines.Add("@" + AccessModifiers.Values[methodVariant.AccessModJs] + " (method)");
			if (methodVariant.IsStatic)
				docLines.Add("@static");
			if (methodVariant.IsTemplate)
				docLines.Add("@template");
			if (methodVariant.IsChainable)
				docLines.Add("@chainable");
			this.generateMemberDocCommentDeprecated(
				ref docLines, methodVariant
			);
			// Render params and return js docs:
			this.generateMethodJsDocsParamsAndReturn(methodVariant, ref docLines);
			if (docLines.Count == 1) {
				this.writeResultLine("/** " + docLines[0] + " */");
			} else {
				this.writeResultLine("/** ");
				foreach (string docLine in docLines)
					this.writeResultLine(" * " + docLine);
				this.writeResultLine(" */");
			}
		}
		protected void generateMethodJsDocsCompatibleWarning (Method methodVariant, ref List<string> docLines) {
			if ((methodVariant.ExistenceReason & ExistenceReasonType.COMPATIBLE_TYPES) != 0) {
				// Render class method compatibility reason:
				string linkText = "["
					+ methodVariant.Owner.FullName + "." + methodVariant.Name
				+ "]("
					+ this.processor.Reader.GetLinkHrefForClassMethod(
						methodVariant.Owner.FullName, methodVariant.IsStatic, methodVariant.Name
					)
				+ ")";
				docLines.Add("@compatible DO NOT USE THIS METHOD VARIANT. It's only compatibility for class " + linkText + ".");
			}
		}
		protected void generateMethodJsDocsParamsAndReturn (Method methodVariant, ref List<string> docLines) {
			// Add possible underlying type for type defined as static property of some class:
			List<List<string>> newDocLines = new List<List<string>>();
			List<string> newDocLine;
			List<string> paramRawTypes;
			List<string> paramFuncTypes;
			List<string> paramSimpleTypes;
			List<string> paramTypes;
			bool[] fixedOptionals = new bool[methodVariant.Params.Count];
			bool optional = true;
			Param methodVariantParam;
			ExtClass methodParamCallbackType;
			int methodParamTypeEnd;
			bool methodParamTypeArr;
			bool methodParamIsFunction;
			Callback methodParamCallback;
			for (int i = methodVariant.Params.Count - 1; i >= 0; i -= 1) {
				methodVariantParam = methodVariant.Params[i];
				optional = optional && methodVariantParam.Optional;
				fixedOptionals[i] = optional;
			}
			for (int i = 0; i < methodVariant.Params.Count; i++) {
				methodVariantParam = methodVariant.Params[i];
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
			if (!methodVariant.IsConstructor) { 
				newDocLine = new List<string>() { "@returns" };
				newDocLine.Add("{" + String.Join("|", methodVariant.ReturnTypes) + "}");
				newDocLine.Add("");
				if (methodVariant.ReturnDocs != null && methodVariant.ReturnDocs.Length > 0) { 
					newDocLine.AddRange(methodVariant.ReturnDocs);
				} else {
					newDocLine.Add("");
				}
				newDocLines.Add(newDocLine);
			}
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
		protected string generateMethodJsDocsParamsAndReturnTypes (List<string> methodParamOrReturnTypes, bool methodParamIsRest = false, bool typeofReturn = false) {
			// Add possible underlying type for type defined as static property of some class:
			List<string> allTypes = this.generateMethodUnderlyingTypes(methodParamOrReturnTypes);
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
						resultFuncTypes.Add(methodParamOrReturnType);
					} else {
						resultSimpleTypes.Add(methodParamOrReturnType);
					}
				} else {
					// Param definition is callback function - render function directly inline:
					resultCallbackTypes.Add(
						this.generateMethodParamTypeCallback(
							methodParamCallbackType as Callback
						)
					);
				}
			}
			string restParamArrBrackets = methodParamIsRest ? "[]" : "";
			if (resultSimpleTypes.Count > 0 || resultFuncTypes.Count > 0) {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add("(" + resultCallbackType + ")" + restParamArrBrackets);
			} else {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add(resultCallbackTypes + restParamArrBrackets);
			}
			foreach (string resultFuncType in resultFuncTypes) 
				resultTypes.Add(resultFuncType + restParamArrBrackets);
			foreach (string resultSimpleType in resultSimpleTypes) 
				resultTypes.Add(resultSimpleType + restParamArrBrackets);
			if (typeofReturn) { 
				return "typeof " + String.Join(" | typeof ", resultTypes);
			} else {
				return String.Join(" | ", resultTypes);
			}
		}
		protected string generateMethodFlags(ExtClass extClass, Method methodVariant) {
			List<string> flags = new List<string>();

			// UPDATE for constructing .Def classes: do not render access modifiers anywhere!
			/*
			if (methodVariant.AccessModTs == AccessModifier.NONE)
				methodVariant.AccessModTs = methodVariant.AccessModJs;
			if (methodVariant.AccessModTs == AccessModifier.NONE)
				methodVariant.AccessModTs = AccessModifier.PUBLIC;
			flags.Add(AccessModifiers.Values[methodVariant.AccessModTs]);
			*/

			if (methodVariant.IsStatic || extClass.Name.FullName == "Ext")
				flags.Add("static");
			string result = String.Join(" ", flags);
			if (flags.Count > 0) result += " ";
			return result;
		}
		protected string generateMethodParams(ExtClass extClass, List<Param> methodVariantParams) {
			List<string> result = new List<string>();
			string resultItem;
			bool[] fixedOptionals = new bool[methodVariantParams.Count];
			bool optional = false;
			Param methodVariantParam;
			for (int i = 0; i < methodVariantParams.Count; i += 1) {
				methodVariantParam = methodVariantParams[i];
				if (optional && !methodVariantParam.Optional) { 
					fixedOptionals[i] = true;
				} else if (methodVariantParam.Optional && !optional) { 
					fixedOptionals[i] = true;
					optional = true;
				} else {
					fixedOptionals[i] = optional;
				}
			}
			for (int i = 0; i < methodVariantParams.Count; i++) {
				methodVariantParam = methodVariantParams[i];
				optional = fixedOptionals[i];
				resultItem = methodVariantParam.Name;
				if (methodVariantParam.IsRest) {
					resultItem = "..." + resultItem + ": ";
				} else {
					resultItem += (optional ? "?: " : ": ");
				}
				resultItem += this.generateMethodTypes(methodVariantParam.Types, methodVariantParam.IsRest, false);
				result.Add(resultItem);
			}
			return String.Join(", ", result);
		}
		protected string generateMethodTypes (List<string> methodParamOrReturnTypes, bool methodParamIsRest = false, bool typeofReturn = false) {
			// Add possible underlying type for type defined as static property of some class:
			List<string> allTypes = this.generateMethodUnderlyingTypes(methodParamOrReturnTypes);
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
					resultCallbackTypes.Add(
						this.generateMethodParamTypeCallback(
							methodParamCallbackType as Callback
						)
					);
				}
			}
			string restParamArrBrackets = methodParamIsRest ? "[]" : "";
			if (resultSimpleTypes.Count > 0 || resultFuncTypes.Count > 0) {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add("(" + resultCallbackType + ")" + restParamArrBrackets);
			} else {
				foreach (string resultCallbackType in resultCallbackTypes) 
					resultTypes.Add(resultCallbackType + restParamArrBrackets);
			}
			foreach (string resultFuncType in resultFuncTypes) 
				resultTypes.Add(resultFuncType + restParamArrBrackets);
			foreach (string resultSimpleType in resultSimpleTypes) 
				resultTypes.Add(resultSimpleType + restParamArrBrackets);
			if (typeofReturn) { 
				return "typeof " + String.Join(" | typeof ", resultTypes);
			} else {
				return String.Join(" | ", resultTypes);
			}
		}
		// Add possible underlying type for type defined as static property of some class:
		protected List<string> generateMethodUnderlyingTypes (List<string> methodParamOrReturnTypes) {
			List<string> staticPropTypes;
			List<string> underlyingTypes = new List<string>();
			List<string> allTypes = new List<string>(methodParamOrReturnTypes);
			foreach (string methodParamOrReturnType in methodParamOrReturnTypes) {
				if (this.processor.Store.StaticPropsTypes.ContainsKey(methodParamOrReturnType)) {
					staticPropTypes = this.processor.Store.StaticPropsTypes[methodParamOrReturnType];
					foreach (string staticPropType in staticPropTypes) 
						if (!underlyingTypes.Contains(staticPropType)) 
							underlyingTypes.Add(staticPropType);
				}
			}
			if (underlyingTypes.Count > 0) 
				foreach (string underlyingType in underlyingTypes) 
					if (!allTypes.Contains(underlyingType))
						allTypes.Add(underlyingType);
			return allTypes;
		}
		protected string generateMethodParamTypeCallback(Callback methodParamCallback) {
			return "("
				+ this.generateMethodParams(methodParamCallback, methodParamCallback.Params)
			+ ") => " 
			+ this.generateMethodTypes(methodParamCallback.ReturnTypes, false);
		}
	}
}
