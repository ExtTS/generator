using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtTs.Processors {
	public class TypeDefinitionsParser {
		protected Processor processor;
		protected internal TypeDefinitionsParser(Processor processor) {
			this.processor = processor;
		}
		protected internal ParsedTypes Parse (
			TypeDefinitionPlace typeDefinitionPlace, 
			string definitionFullPath, 
			string memberOrParamName,
			string rawTypesStr
		) {
			ParsedTypes result = new ParsedTypes() {
				CfgOrProp = new List<string>(),
				MethodOrEventParam = new List<string>(),
				MethodOrEventSpreadParam = new List<string>(),
				MethodOrEventReturn = new List<string>(),
			};
			// Return "any" type if type definition is empty or null:
			if (String.IsNullOrEmpty(rawTypesStr)) {
				this.addTypeToResult(
					ref result, typeDefinitionPlace, false, "any"
				);
				return result;
			}
			// Check for exception:
			rawTypesStr = this.checkExceptions(typeDefinitionPlace, definitionFullPath, memberOrParamName, rawTypesStr);
			// Process very basic replacements to fix syntax errors for proper parsing:
			rawTypesStr = this.sanitizeRawTypesSyntax(rawTypesStr, typeDefinitionPlace);
			// Complete boolean flag about if there is method param type definition:
			bool methodParamDefinition = (typeDefinitionPlace & TypeDefinitionPlace.ANY_PARAM) != 0;
			// Check if there is method param spread syntax and correct it by ext version if necessary:
			if (methodParamDefinition && rawTypesStr.Contains("..."))
				rawTypesStr = this.correctMethodParamSpreadSyntaxIfNecessary(
					typeDefinitionPlace, definitionFullPath, memberOrParamName, rawTypesStr
				);
			// Explode raw types into list of strings:
			List<string> rawTypesList = this.explodeRawTypes(rawTypesStr);
			// Complete result types collection(s):
			this.completeResultList(
				ref result, rawTypesList, typeDefinitionPlace, methodParamDefinition, definitionFullPath, memberOrParamName
			);
			return result;
		}
		protected string checkExceptions (
			TypeDefinitionPlace typeDefinitionPlace, 
			string definitionFullPath, 
			string memberOrParamName,
			string rawTypesStr
		) {
			string exceptionKey = "[" + TypeDefinitionSource.Names[typeDefinitionPlace] + "]"
				+ definitionFullPath + "." + memberOrParamName;
			if (this.processor.Store.TypesFixes.ContainsKey(exceptionKey))
				rawTypesStr = this.processor.Store.TypesFixes[exceptionKey];
			return rawTypesStr;
		}
		/**
		 * Complete result types collection(s):
		 */
		protected void completeResultList (
			ref ParsedTypes result,
			List<string> rawTypesList,
			TypeDefinitionPlace typeDefinitionPlace, 
			bool methodParamDefinition,
			string definitionFullPath, 
			string memberOrParamName
		) {
			string typeItem = "";
			string typeItemLower = "";
			int arrBracketsPos = 0;
			string arrayBrackets = "";
			bool functionParamSpreadSyntax;
			bool anyMatchedNormal = false;
			bool anyMatchedSpreadSyntax = false;
			for (int i = 0; i < rawTypesList.Count; i++) {
				// Trim type definition string:
				typeItem = rawTypesList[i].Trim(
					new char[] { ' ', '\t', '\v', '\r', '\n' }
				);
				// Check and correct the type if there is function param spread syntax detected
				functionParamSpreadSyntax = methodParamDefinition && typeItem.Contains("...");
				if (functionParamSpreadSyntax)
					typeItem = typeItem.Replace("...", "");
				// Take off temporary array brackets at the end if there are any:
				arrBracketsPos = typeItem.IndexOf("[]");
				if (arrBracketsPos > -1) {
					arrayBrackets = typeItem.Substring(arrBracketsPos);
					typeItem = typeItem.Substring(0, arrBracketsPos);
				} else {
					arrayBrackets = "";
				}
				// Create lowercase type definition variant to determinate primitive JS types and define correct typescript type form:
				if (this.processor.Store.ClassesFixes.ContainsKey(typeItem))
					typeItem = this.processor.Store.ClassesFixes[typeItem];
				typeItemLower = typeItem.ToLower();
				// Check internal JS priitives, internal js types, special wildcard definitions, 
				// function arguments type or any other type to register and chack later
				if (JavascriptInternals.IsJsPrimitiveTypescriptLower(typeItemLower)) {
					// Primitive JS types has to be lowercased for TypeScript:
					// https://www.typescriptlang.org/docs/handbook/declaration-files/do-s-and-don-ts.html
					typeItem = typeItemLower;
				} else if (typeItemLower == "arguments") {
					// If type is defined as function arguments object, 
					// correct it to TypeScript `IArguments` interface defined in `lib.es5.d.ts`:
					typeItem = "IArguments";
				} else if (InheritanceResolvers.Types.IsBrowserInternalType(typeItem)) {
					/*if (JavascriptInternals.JsGlobalsAlsoInExtNamespace.Contains(typeItem)) {
						typeItem = SpecialsGenerator.GLOBAL_CLASS_BASE
							.Replace("<browserGlobalClassName>", typeItem);
					} else*/ if (typeItemLower == "array") {
						// If type is internal JS type or internal EcmaScript DOM type, 
						// correct only `array` to `any[]` and do not register anything for later chacking:
						typeItem = "any[]";
					}
				} else if (typeItemLower == "mixed" || typeItemLower == "*" || typeItemLower == "type" /* always in private classes */) {
					// If type is any special JS Docs wildcard - corect it to TypeScript `any` type:
					typeItem = "any";
				} else {
					// If type is anything other - register this type for later checking process:
					if (
						!typeItem.StartsWith("'") &&							// if it is not start with "'"
						!typeItem.EndsWith("'") &&								// if it is not end with "'"
						!Regex.Match(typeItem, @"^([0-9\+\-\.]+)$").Success &&	// if it is not numeric
						!Regex.Match(typeItem, @"^\{(.*)\}$").Success			// if it is not direct object: { x:: Number, y: Number, ...}
					) {
						this.processor.Store.AddTypePlace(
							typeDefinitionPlace, definitionFullPath, memberOrParamName, typeItem
						);
					}
				}
				// Add array brackets back at the end if there were any:
				if (arrayBrackets.Length > 0) 
					typeItem += arrayBrackets;
				// If there is `any` TypeScript type matched - store this information later and ad this type at the result list end
				if (typeItem == "any") {
					if (functionParamSpreadSyntax) {
						anyMatchedSpreadSyntax = true;
					} else {
						anyMatchedNormal = true;
					};
				} else {
					this.addTypeToResult(
						ref result, typeDefinitionPlace, functionParamSpreadSyntax, typeItem
					);
				}
			}
			// add `any` TypeScript type always at the end:
			if (anyMatchedNormal) // add any always as last
				this.addTypeToResult(
					ref result, typeDefinitionPlace, false, "any"
				);
			if (anyMatchedSpreadSyntax) // add any always as last
				this.addTypeToResult(
					ref result, typeDefinitionPlace, true, "any"
				);
		}
		/**
		 * Add parsed type definition to proper collection by definition source code place.
		 */
		protected void addTypeToResult (
			ref ParsedTypes result,
			TypeDefinitionPlace typeDefinitionPlace,
			bool functionParamSpreadSyntax,
			string typeItem
		) {

			List<string> typesList = new List<string>();
			if ((typeDefinitionPlace & TypeDefinitionPlace.ANY_PROPERTY) != 0) {
				typesList = result.CfgOrProp;
			} else if ((typeDefinitionPlace & TypeDefinitionPlace.ANY_PARAM) != 0) {
				if (functionParamSpreadSyntax) { 
					typesList = result.MethodOrEventSpreadParam;
				} else {
					typesList = result.MethodOrEventParam;
				}
			} else if ((typeDefinitionPlace & TypeDefinitionPlace.ANY_RETURN) != 0) {
				typesList = result.MethodOrEventReturn;
			}
			typesList.Add(typeItem);
		}
		/**
		 * Process very basic replacements to fix syntax errors by Sencha developers.
		 */
		protected string sanitizeRawTypesSyntax (string rawTypesStr, TypeDefinitionPlace typeDefinitionPlace) {
			if (rawTypesStr == "undefined") { 
				if ((typeDefinitionPlace & TypeDefinitionPlace.ANY_RETURN) != 0) { 
					rawTypesStr = "void";
				} else {
					Debugger.Break(); // Ok, this is strange case:-)
				}
			}
			// Sometimes there are direct string values as type definitions - normalize double quotes to single quotes:
			rawTypesStr = rawTypesStr.Replace('"', '\'');
			// Sometimes there are really `,` and `|` separators in Ext js docs, so be carefull:
			rawTypesStr = rawTypesStr.Replace('|', '/');
			if (rawTypesStr.StartsWith("'") && rawTypesStr.EndsWith("'")) 
				rawTypesStr = rawTypesStr.Replace(',', '/');
			return rawTypesStr;
		}
		/**
		 * Explode raw types definition by slash `/`. Slas is specific separator for JS Docs type definitions in Ext.JS framework.
		 */
		protected List<string> explodeRawTypes (string rawTypesStr) {
			List<string> rawResult = new List<string>();
			List<string> result = new List<string>();
			if (rawTypesStr.IndexOf("/") > -1) {
				//String/String[] or String[]/Object
				rawResult = rawTypesStr.Split(
					new string[] { "/" }, 
					StringSplitOptions.RemoveEmptyEntries
				).ToList<string>();
			} else {
				rawResult.Add(rawTypesStr);
			}
			// Be carefull for type definitions like: `Ext.dataview.component.(Simple)ListItem`, 
			// fix them into: Ext.dataview.component.ListItem | Ext.dataview.component.SimpleListItem
			int openBracketPos;
			int closeBracketPos;
			foreach (string rawItem in rawResult) {
				openBracketPos = rawItem.IndexOf("(");
				closeBracketPos = rawItem.LastIndexOf(")");
				if (rawItem.Contains(".") && openBracketPos != -1 && closeBracketPos != -1) {
					result.Add(
						rawItem.Replace("(", "").Replace(")", "")
					);
					result.Add(
						rawItem.Substring(0, openBracketPos) + rawItem.Substring(closeBracketPos + 1)
					);
				} else {
					result.Add(rawItem);
				}
			}
			return result;
		}
		/**
		 * There are sometimes errors in JS Docs comments in function params spread syntax definitions written by Sencha developers.
		 * If there it is method param spread syntax - correct wrong definition by ext version if necessary.
		 * Register all spread syntax code places for development purposes.
		 */
		protected string correctMethodParamSpreadSyntaxIfNecessary (
			TypeDefinitionPlace typeDefinitionPlace, 
			string definitionFullPath, 
			string memberOrParamName,
			string rawTypesStr	
		) {
			string sourceCodeDefinitionFullPathKey = this.getSourceCodeDefinitionFullPathKey(
				typeDefinitionPlace	, definitionFullPath, memberOrParamName
			);
			this.processor.Store.AddSpreadSyntaxTypePlace(
				sourceCodeDefinitionFullPathKey, rawTypesStr
			);
			//if (this.processor.Store.ManuallyFixedMethodSpreadParams.ContainsKey(sourceCodeDefinitionFullPathKey))
			//	rawTypesStr = this.processor.Store.ManuallyFixedMethodSpreadParams[sourceCodeDefinitionFullPathKey];
			return rawTypesStr;
		}
		/**
		 * "[place]Namespace.full.path.ClassName.methodName:paramName"
		 */
		protected string getSourceCodeDefinitionFullPathKey (
			TypeDefinitionPlace typeDefinitionPlace, 
			string definitionFullPath, 
			string memberOrParamName
		) {
			return "[" + TypeDefinitionSource.Names[typeDefinitionPlace] + "]" + definitionFullPath + ":" + memberOrParamName;
		}
	}
}
