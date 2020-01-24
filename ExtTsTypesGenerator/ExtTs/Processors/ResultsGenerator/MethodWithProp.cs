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
		protected void generateClassMethodVariantsWithProperty(ExtClass extClass, List<Member> methodVariants, Property mixedProp, bool classProcessing = true) {
			// Generate params sections as dictionary keys and methods 
			// as dictionary values to merge them by params sections later:
			List<Method> mergedVariants = this.generateMethodVariantsMergeByParams(extClass, methodVariants);
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
			string lineMethodTypePart;
			string linePropTypePart;
			foreach (Method methodVariant in mergedVariants) {
				// Generate TypeScript doc comments:
				theSameParams = (
					previouslyRenderedParams != null &&
					previouslyRenderedParams == methodVariant.ParamsRendered
				);
				previouslyRenderedParams = methodVariant.ParamsRendered;
				if (this.processor.GenerateJsDocs && !theSameParams) {
					methodVariantClone = methodVariant.Clone();
					methodVariantClone.ReturnTypes = theSameParamsReturns[methodVariant.ParamsRendered];
					this.generateClassMethodWithPropertyJsDocs(extClass, methodVariantClone, mixedProp);
				}
				// Generate TypeScript definition code:
				// Generate method type part:
				lineMethodTypePart = "(" + methodVariant.ParamsRendered + ")";
				if (!methodVariant.IsConstructor) {
					typeofReturn = methodVariant.IsChainable && methodVariant.IsStatic;
					lineMethodTypePart += " => " + this.generateMethodTypes(
						methodVariant.ReturnTypes, false, typeofReturn
					);
				}
				// Generate property type part:
				linePropTypePart = this.generatePropertyTypes(mixedProp);
				// Generate access modifier flag:
				generateMethodFlags = (
					classProcessing && 
					!renderedAsSingletonInterface &&
					!methodVariant.IsConstructor
				);
				line = generateMethodFlags
					? this.generateMethodWithPropFlags(extClass, methodVariant, mixedProp)
					: "";
				//delimiter = classProcessing || methodVariant.IsConstructor ? ": " : "?: ";
				delimiter = methodVariant.IsConstructor ? ": " : "?: ";
				line += methodVariant.Name + delimiter + "(" + lineMethodTypePart + ") | " + linePropTypePart + " | any;";
				this.writeResultLine(line);
			}
		}
		protected void generateClassMethodWithPropertyJsDocs(ExtClass extClass, Method methodVariant, Property mixedProp) {
			List<string> docLines = new List<string>();
			string propOrCfgStr = extClass.ClassType == ClassType.CLASS_DEFINITIONS
				? "configuration"
				: "property";
			// Method docs:
			docLines.Add("@mixed");
			this.generateMethodJsDocsCompatibleWarning(methodVariant, ref docLines);
			if (methodVariant.Doc != null && methodVariant.Doc.Length > 0) {
				docLines.Add("@method");
				docLines.AddRange(methodVariant.Doc);
			}
			if (mixedProp.Doc != null && mixedProp.Doc.Length > 0) { 
				docLines.Add("@" + propOrCfgStr);
				docLines.AddRange(mixedProp.Doc);
			}
			// if ()
			string methodAcMod = methodVariant.AccessModJs != AccessModifier.NONE
				? "@" + AccessModifiers.Values[methodVariant.AccessModJs]
				: "@public";
			string propAcMod = mixedProp.AccessModJs != AccessModifier.NONE
				? "@" + AccessModifiers.Values[mixedProp.AccessModJs]
				: "@public";
			if (methodAcMod == propAcMod) {
				docLines.Add(propAcMod + $" (method+{propOrCfgStr})");
			} else {
				docLines.Add(methodAcMod + " (method)");
				docLines.Add(propAcMod + $" ({propOrCfgStr})");
			}
			if (methodVariant.IsStatic && mixedProp.IsStatic) { 
				docLines.Add("@static");
			} else {
				if (methodVariant.IsStatic)
					docLines.Add("@static (method)");
				if (mixedProp.IsStatic)
					docLines.Add($"@static ({propOrCfgStr})");
			}
			if (methodVariant.IsTemplate)
				docLines.Add("@template (method)");
			if (methodVariant.IsChainable)
				docLines.Add("@chainable (method)");
			if (mixedProp.IsReadOnly)
				docLines.Add($"@readonly ({propOrCfgStr})");
			if (mixedProp.DefaultValue != null) 
				docLines.Add(
					$"@default ({propOrCfgStr}) " + mixedProp.DefaultValue.Replace("*/", "*\\/")
				);
			this.generateMemberDocCommentDeprecated(
				ref docLines, methodVariant
			);
			this.generateMemberDocCommentDeprecated(
				ref docLines, mixedProp
			);
			// Render params and return js docs:
			this.generateMethodJsDocsParamsAndReturn(methodVariant, ref docLines);
			// Prop types js docs:
			this.generatePropertyDocCommentTypes(
				ref docLines, mixedProp
			);
			// join together
			this.writeResultLine("/** ");
			foreach (string docLine in docLines)
				this.writeResultLine(" * " + docLine);
			this.writeResultLine(" */");
		}
		protected string generateMethodWithPropFlags (ExtClass extClass, Method methodVariant, Property mixedProp) {
			string result = "";

			// UPDATE for constructing .Def classes: do not render access modifiers anywhere!
			/*
			if (methodVariant.AccessModTs == AccessModifier.NONE)
				methodVariant.AccessModTs = methodVariant.AccessModJs;
			if (methodVariant.AccessModTs == AccessModifier.NONE)
				methodVariant.AccessModTs = AccessModifier.PUBLIC;
			if (mixedProp.AccessModTs == AccessModifier.NONE)
				mixedProp.AccessModTs = mixedProp.AccessModJs;
			if (mixedProp.AccessModTs == AccessModifier.NONE)
				mixedProp.AccessModTs = AccessModifier.PUBLIC;
			int methodVariantAcVal = (int)methodVariant.AccessModTs;
			int mixedPropAcVal = (int)mixedProp.AccessModTs;
			if (methodVariantAcVal > mixedPropAcVal) {
				result = methodVariant.AccessModTs.ToString();
			} else if (mixedPropAcVal > methodVariantAcVal) {
				result = mixedProp.AccessModTs.ToString();
			} else {
				result = mixedProp.AccessModTs.ToString();
			}
			result = result.ToLower();
			if (methodVariant.IsStatic || mixedProp.IsStatic)
				result += " static";
			*/

			if (methodVariant.IsStatic || mixedProp.IsStatic)
				result = "static";
			return result;
		}
	}
}
