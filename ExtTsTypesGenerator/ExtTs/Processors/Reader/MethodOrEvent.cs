using System;
using System.Collections.Generic;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers;
using ExtTs.SourceJsonTypes.ExtObjects;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.MemberParams;
using System.Diagnostics;
using System.Linq;

namespace ExtTs.Processors {
	public partial class Reader {
		protected void readAndAddMethodOrEvent(ref ExtClass extClass, ExtObjectMember extObjectMember, string currentClassName, bool eventCompleting) {
			string funcOrEventName = this.sanitizeName(extObjectMember.Name);
			bool ownedByCurrent = extObjectMember.Owner.Length == currentClassName.Length && extObjectMember.Owner == currentClassName;
			bool isStatic = (extObjectMember.Static.HasValue && extObjectMember.Static.Value == true);
			FuncParamsSyntaxCollections funcParamsSyntaxCollections = this.readFunctionParams(
				extObjectMember, currentClassName, eventCompleting, funcOrEventName, isStatic, eventCompleting
			);
			if (eventCompleting) {
				this.readAndAddEvent(
					ref extClass, extObjectMember, ref funcParamsSyntaxCollections, 
					funcOrEventName, ownedByCurrent
				);
			} else {
				this.readAndAddMethod(
					ref extClass, extObjectMember, ref funcParamsSyntaxCollections, 
					currentClassName, funcOrEventName, ownedByCurrent, isStatic
				);
			}
		}
		protected void readAndAddEvent(ref ExtClass extClass, ExtObjectMember extObjectMember, ref FuncParamsSyntaxCollections funcParamsSyntaxCollections, string eventName, bool ownedByCurrent) {
			string[] docs = this.readJsDocs(extObjectMember.Doc, JsDocsType.EVENT, extClass.Name.FullName, eventName);
			Event newEventItem = new Event(
				eventName, funcParamsSyntaxCollections.StandardParamsSyntax, docs, extObjectMember.Owner, ownedByCurrent
			);
			newEventItem.Deprecated = this.readJsDocsDeprecated(
				extObjectMember.Deprecated, extClass.Name.FullName, eventName
			);
			/*if (
				funcParamsSyntaxCollections.StandardParamsSyntax.Count == 0 || (
					funcParamsSyntaxCollections.StandardParamsSyntax.Count > 0 &&
					funcParamsSyntaxCollections.StandardParamsSyntax[0].Name != "_this"
				)
			) {
				Debugger.Break();
			}*/
			if (extObjectMember.Return != null) {
				try { 
					throw new Exception(
						$"Event methods couldn't have a return type: `{extClass.Name.FullName}:eventName`."
					);
				} catch (Exception ex) {
					this.processor.Exceptions.Add(ex);
				}
			}
			extClass.AddMemberEvent(newEventItem);
			if (funcParamsSyntaxCollections.SpreadParamsSyntax.Count > 0) {
				// There is necessary to add second method form, 
				// because there was last param with standard param 
				// syntax mixed with spread param syntax:
				Event newEventItemClone = newEventItem.Clone();
				newEventItemClone.Params = funcParamsSyntaxCollections.SpreadParamsSyntax;
				extClass.AddMemberEvent(newEventItemClone);
			}
		}
		protected void readAndAddMethod(ref ExtClass extClass, ExtObjectMember extObjectMember, ref FuncParamsSyntaxCollections funcParamsSyntaxCollections, string currentClassName, string methodName, bool ownedByCurrent, bool isStatic) {
			string[] docs = this.readJsDocs(extObjectMember.Doc, JsDocsType.METHOD, currentClassName, methodName);
			Method item = new Method(
				methodName, funcParamsSyntaxCollections.StandardParamsSyntax, docs, extObjectMember.Owner, ownedByCurrent
			);
			item.Deprecated = this.readJsDocsDeprecated(
				extObjectMember.Deprecated, currentClassName, methodName
			);
			item.AccessModJs = this.readItemAccessModifier(extObjectMember);
			item.AccessModTs = item.AccessModJs;
			item.ExistenceReason = ExistenceReasonType.NATURAL;
			item.IsStatic = isStatic;
			item.IsChainable = (extObjectMember.Chainable.HasValue && extObjectMember.Chainable.Value == true);
			item.IsTemplate = (extObjectMember.Template.HasValue && extObjectMember.Template.Value == true);
			item.Renderable = extClass.Extends == null || extObjectMember.Name == "statics"; // force render if it is class without any parent
			if (item.IsChainable) {
				item.Renderable = true; // always render chanable method with it's return type
				item.ReturnTypes.Add(extClass.Name.FullName);
			} else {
				if (extObjectMember.Return != null) {
					item.ReturnTypes = this.readAndAddMethodReturn(
						ref extClass, extObjectMember, currentClassName, isStatic
					);
				} else {
					item.ReturnTypes.Add("void");
				}
			}
			if (extObjectMember.Return != null && extObjectMember.Return.Doc != null)
				item.ReturnDocs = this.readJsDocs(
					extObjectMember.Return.Doc, 
					JsDocsType.METHOD_RETURN, 
					currentClassName, 
					"return"
				);
			extClass.AddMemberMethod(item);
			if (funcParamsSyntaxCollections.SpreadParamsSyntax.Count > 0) {
				// There is necessary to add second method form, 
				// because there was last param with standard param 
				// syntax mixed with spread param syntax:
				Method itemClone = item.Clone();
				itemClone.Params = funcParamsSyntaxCollections.SpreadParamsSyntax;
				extClass.AddMemberMethod(itemClone);
			}
		}
		protected List<string> readAndAddMethodReturn (ref ExtClass extClass, ExtObjectMember extObjectMember, string currentClassName, bool isStatic) {
			List<string> returnTypes = new List<string>();
			List<string> rawReturnTypes = this.typesParser.Parse(
				TypeDefinitionPlace.METHOD_RETURN,
				currentClassName,
				extObjectMember.Name,
				extObjectMember.Return.Type
			).MethodOrEventReturn;
			// check if there is any directly written object:
			string rawReturnType;
			List<string> rawMembers;
			string rawMember;
			string[] rawMemberNameAndType;
			string rawMemberName;
			List<string> rawMemberTypes;
			string[] returnDocs = null;
			Property prop;
			if (extObjectMember.Return != null && extObjectMember.Return.Doc != null)
				returnDocs = this.readJsDocs(
					extObjectMember.Return.Doc, 
					JsDocsType.METHOD_RETURN, 
					currentClassName, 
					"return"
				);
			for (int i = 0; i < rawReturnTypes.Count; i++) {
				rawReturnType = rawReturnTypes[i];
				if (rawReturnType.Length > 2 && rawReturnType.StartsWith("{") && rawReturnType.EndsWith("}")) {
					// Ext.draw.engine.Canvas.getBBox(): {x: Number, y: Number, width: number, height: number}
					ExtClass returnClass = new ExtClass(
						this.extClassMethodReturnObjectPresudoClassName(
							currentClassName, extObjectMember.Name, isStatic
						),
						SpecialsGenerator.BASE_RETURNS_INTERFACE_NAME,
						returnDocs
					);
					returnClass.Package = extClass.Package;
					//returnClass.SrcJson = this.srcJson;
					returnClass.Name.PackagedNamespace = this.GetPackagedNamespaceFromFullClassName(
						extClass.Name.FullName
					);
					returnClass.ClassType = ClassType.CLASS_METHOD_RETURN_OBJECT;
					returnClass.Link = new string[] {
						currentClassName + "." + extObjectMember.Name,
						this.GetLinkHrefForClassMethod(
							currentClassName, isStatic, extObjectMember.Name
						)
					};
					//returnClass.AddMemberProperty()
					rawReturnType = rawReturnType.TrimStart('{').TrimEnd('}');
					rawMembers = rawReturnType.Split(',').ToList<string>();
					for (int j = 0; j < rawMembers.Count; j++) {
						rawMember = rawMembers[j].Trim();
						rawMemberNameAndType = rawMember.Split(':').ToArray<string>();
						if (rawMemberNameAndType.Length == 1) {
							rawMemberName = rawMemberNameAndType[0];
							rawMemberTypes = this.typesParser.Parse(
								TypeDefinitionPlace.METHOD_RETURN,
								currentClassName,
								extObjectMember.Name,
								extObjectMember.Return.Type
							).MethodOrEventReturn;
							// rawMemberNameAndType[1]
						} else if (rawMemberNameAndType.Length > 1) {
							rawMemberName = rawMemberNameAndType[0];
							rawMemberTypes = new List<string>() { "any" };
						} else {
							continue;
						}
						prop = new Property(
							rawMemberName, rawMemberTypes, null, returnClass.Name.FullName, true
						);
						prop.IsStatic = false;
						prop.IsReadOnly = false;
						prop.Inherited = false;
						prop.AccessModJs = AccessModifier.PUBLIC;
						prop.AccessModTs = AccessModifier.PUBLIC;
						returnClass.AddMemberProperty(prop);
					}
					this.processor.Store.AddExtClass(returnClass);
					returnTypes.Add(returnClass.Name.FullName);
				} else {
					returnTypes.Add(rawReturnType);
				}
			}
			return returnTypes;
		}
		protected FuncParamsSyntaxCollections readFunctionParams (ExtObjectMember extObjectMember, string currentClassName, bool eventCompleting, string eventOrMethodName, bool isStatic, bool eventsCompleting) {
			FuncParamsSyntaxCollections funcParamsSyntaxCollections = new FuncParamsSyntaxCollections() {
				SpreadParamsSyntax = new List<Param>(),
				StandardParamsSyntax = new List<Param>(),
				SpreadParamFound = false
			};
			ParsedTypes? lastParamTypes = null;
			MemberParam lastParam;
			int lastParamIndex = 0;
			if (extObjectMember.Params.Count > 0) {
				lastParamIndex = extObjectMember.Params.Count - 1;
				lastParam = extObjectMember.Params[lastParamIndex];
				lastParamTypes = this.typesParser.Parse(
					eventCompleting
						? TypeDefinitionPlace.EVENT_PARAM
						: TypeDefinitionPlace.METHOD_PARAM,
					currentClassName + "." + extObjectMember.Name,
					lastParam.Name,
					lastParam.Type
				);
			}
			int paramIndex = 0;
			List<string> readParamNames = new List<string>();
			foreach (MemberParam param in extObjectMember.Params) {
				// Somethimes there could be probles with duplicated params in JSDuck output, 
				// for example methods in version 6.2.0: Ext.util.Filter.isEqual? (filter: Ext.util.Filter, filter: Ext.util.Filter): boolean; ...
				if (readParamNames.Contains(param.Name)) continue;
				readParamNames.Add(param.Name);
				this.readFunctionParam(
					extObjectMember, param, ref funcParamsSyntaxCollections, ref lastParamTypes,
					eventCompleting, currentClassName, eventOrMethodName, isStatic, eventsCompleting, lastParamIndex,	paramIndex
				);
				paramIndex += 1;
			}
			return funcParamsSyntaxCollections;
		}
		/**
		 * Complete reference `funcParamsSyntaxCollections` with given `param` into proper collections.
		 */
		protected void readFunctionParam (ExtObjectMember extObjectMember, MemberParam param, ref FuncParamsSyntaxCollections funcParamsSyntaxCollections, ref ParsedTypes? lastParamTypes, bool eventCompleting, string currentClassName, string eventOrMethodName, bool isStatic, bool eventsCompleting, int lastParamIndex, int paramIndex) {
			ParsedTypes paramTypes;
			bool lastParamDefinition = paramIndex == lastParamIndex;
			// Parse param types - this types parsing result could have more result method forms:
			// - most often is form with normal params syntax
			// - but sometimes there could be another send method form 
			//   with params spread syntax or only this syntax
			if (lastParamDefinition) {
				paramTypes = lastParamTypes.Value;
			} else { 
				paramTypes = this.typesParser.Parse(
					eventCompleting 
						? TypeDefinitionPlace.EVENT_PARAM 
						: TypeDefinitionPlace.METHOD_PARAM, 
					currentClassName + "." + extObjectMember.Name, 
					param.Name, 
					param.Type
				);
			}
			// If param contains any sub-definitions, then param is probably
			// some kind of configuration object, which has all it's members
			// defined in Ext documentation. Then it's practicle to generate sub-interface:
			if (param.Properties != null && param.Properties.Count > 0) 
				this.readFunctionParamSpecials(
					param,
					ref paramTypes,
					eventCompleting,
					currentClassName,
					eventOrMethodName,
					isStatic,
					eventCompleting
				);
			//if (currentClassName == "Ext" && eventOrMethodName == "create")
			//	Debugger.Break();
			// Add completed param types into proper collection with name, param docs and all it's flags:
			this.readFunctionParamAddToParamsList(
				param,
				ref funcParamsSyntaxCollections,
				ref paramTypes,
				currentClassName,
				eventCompleting,
				lastParamDefinition
			);
		}
		protected void readFunctionParamSpecials (
			MemberParam param,
			ref ParsedTypes paramTypes,
			bool eventCompleting, 
			string currentClassName, 
			string eventOrMethodName,
			bool isStatic, 
			bool eventsCompleting
		) {
			string extParamsPseudoCallbackName;
			string extParamsPseudoClassName;
			SpecialParamTypes specialTypes = this.matchSpecStructParamInParamTypes(ref paramTypes);
			if ((specialTypes.Matches & SpecialParamMatch.ANY_FUNC) != 0) {
				// Described function callback as special class, not rendered later as type but directly:
				extParamsPseudoCallbackName = this.extClassMethodConfigObjectPresudoClassName(
					currentClassName, eventOrMethodName, isStatic, eventsCompleting, param.Name, false
				);
				// `extParamsPseudoCallbackName = 'Ext.AbstractManager.methodsCallbackParams.each.Fn';`
				// `extParamsPseudoCallbackName = 'Ext.Class.staticMethodsCallbackParams.registerPreprocessor.Fn';`
				this.readFunctionParamCallbackProperties(
					extParamsPseudoCallbackName,
					eventOrMethodName,
					param.Doc,
					param.Properties,
					eventCompleting
				);
				// Add described virtual callback type:
				if ((specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_FUNC) != 0) 
					paramTypes.MethodOrEventParam.Add(extParamsPseudoCallbackName);
				if ((specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_FUNC_ARR) != 0) 
					paramTypes.MethodOrEventParam.Add(extParamsPseudoCallbackName + "[]");
				if ((specialTypes.Matches & SpecialParamMatch.SPREAD_COLLECTION_FUNC) != 0) 
					paramTypes.MethodOrEventSpreadParam.Add(extParamsPseudoCallbackName);
			}
			if ((specialTypes.Matches & SpecialParamMatch.ANY_OBJECT) != 0) {
				// Described config object as special class, rendered later as type, 
				// extended from `Object` TypeScript interface:
				extParamsPseudoClassName = this.extClassMethodConfigObjectPresudoClassName(
					currentClassName, eventOrMethodName, isStatic, eventsCompleting, param.Name, true
				);
				// `extParamsPseudoClassName = 'Ext.Ajax.methodsParams.addListener.Options';`
				// `extParamsPseudoClassName = 'Ext.data.Model.staticMethodsParams.load.Options';`
				this.readFunctionParamConfObjectProperties(
					currentClassName,
					extParamsPseudoClassName,
					eventOrMethodName,
					"Object",
					param.Doc,
					param.Properties,
					eventCompleting,
					isStatic
				);
				if ((specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_OBJECT) != 0) 
					paramTypes.MethodOrEventParam.Add(extParamsPseudoClassName);
				if ((specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_OBJECT_ARR) != 0) 
					paramTypes.MethodOrEventParam.Add(extParamsPseudoClassName + "[]");
				if ((specialTypes.Matches & SpecialParamMatch.SPREAD_COLLECTION_OBJECT) != 0) 
					paramTypes.MethodOrEventSpreadParam.Add(extParamsPseudoClassName);
			}
			if ((specialTypes.Matches & SpecialParamMatch.ANY_INTERFACE) != 0) {
				// Described config object as special class, rendered later as type, 
				// extended from given interface(s):
				bool numberTypes = specialTypes.MethodOrEventAllParamTypes.Count > 1;
				int index = 1;
				bool specTypeIsArr;
				bool typeIsForStandardCollection;
				bool typeIsforSpreadCollection;
				foreach (string specialTypeParent in specialTypes.MethodOrEventAllParamTypes) {
					specTypeIsArr = specialTypeParent.EndsWith("[]");
					extParamsPseudoClassName = this.extClassMethodConfigObjectPresudoClassName(
						currentClassName, eventOrMethodName, isStatic, eventsCompleting, param.Name, true
					);
					// `extParamsPseudoClassName = 'Ext.button.Segmented.methodsParams.onFocusLeave.E';`
					if (numberTypes)
						extParamsPseudoClassName += index.ToString();
					this.readFunctionParamConfObjectProperties(
						currentClassName,
						extParamsPseudoClassName,
						eventOrMethodName,
						specTypeIsArr 
							? specialTypeParent.Substring(0, specialTypeParent.Length - 2) 
							: specialTypeParent,
						param.Doc,
						param.Properties,
						eventCompleting,
						isStatic
					);
					typeIsForStandardCollection = specialTypes.MethodOrEventParamTypes.Contains(specialTypeParent);
					typeIsforSpreadCollection = specialTypes.MethodOrEventSpreadParamTypes.Contains(specialTypeParent);
					if (
						typeIsForStandardCollection && 
						(specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_INTERFACE) != 0
					) 
						paramTypes.MethodOrEventParam.Add(extParamsPseudoClassName);
					if (
						typeIsForStandardCollection && 
						specTypeIsArr && 
						(specialTypes.Matches & SpecialParamMatch.STANDARD_COLLECTION_INTERFACE_ARR) != 0
					) 
						paramTypes.MethodOrEventParam.Add(extParamsPseudoClassName + "[]");
					if (
						typeIsforSpreadCollection && 
						(specialTypes.Matches & SpecialParamMatch.SPREAD_COLLECTION_INTERFACE) != 0
					) 
						paramTypes.MethodOrEventSpreadParam.Add(extParamsPseudoClassName);
					index += 1;
				}
			}
		}
		protected SpecialParamTypes matchSpecStructParamInParamTypes (ref ParsedTypes paramTypes) {
			SpecialParamTypes result = new SpecialParamTypes() {
				Matches = SpecialParamMatch.NONE,
				MethodOrEventParamTypes = new List<string>(),
				MethodOrEventSpreadParamTypes = new List<string>(),
				MethodOrEventAllParamTypes = new List<string>(),
			};
			SpecialParamTypes collectionResult = new SpecialParamTypes() {
				Matches = SpecialParamMatch.NONE,
				MethodOrEventAllParamTypes = new List<string>(),
			};
			if (paramTypes.MethodOrEventParam.Count > 0) { 
				collectionResult = this.matchSpecStructParamInParamTypesCollection(
					paramTypes.MethodOrEventParam, true
				);
				result.Matches |= collectionResult.Matches;
				if (collectionResult.MethodOrEventAllParamTypes.Count > 0) { 
					result.MethodOrEventParamTypes = new List<string>(collectionResult.MethodOrEventAllParamTypes);
					result.MethodOrEventAllParamTypes = new List<string>(collectionResult.MethodOrEventAllParamTypes);
				}
			}
			if (paramTypes.MethodOrEventSpreadParam.Count > 0) { 
				collectionResult = this.matchSpecStructParamInParamTypesCollection(
					paramTypes.MethodOrEventSpreadParam, false
				);
				result.Matches |= collectionResult.Matches;
				if (collectionResult.MethodOrEventAllParamTypes.Count > 0) { 
					result.MethodOrEventSpreadParamTypes = new List<string>(collectionResult.MethodOrEventAllParamTypes);
					foreach (string paramType in collectionResult.MethodOrEventAllParamTypes) 
						if (!result.MethodOrEventAllParamTypes.Contains(paramType))
							result.MethodOrEventAllParamTypes.Add(paramType);
				}
			}
			return result;
		}
		protected SpecialParamTypes matchSpecStructParamInParamTypesCollection (
			List<string> paramAllTypesCollection, 
			bool standardParamsCollection
		) {
			// Process params collection - clone and remove all primitive types, then decide:
			SpecialParamTypes result = new SpecialParamTypes() {
				Matches = SpecialParamMatch.NONE,
				MethodOrEventAllParamTypes = new List<string>(),
			};
			List<string> nonPrimitiveTypes = new List<string>();
			foreach (string paramType in paramAllTypesCollection)
				if (!JavascriptInternals.IsJsPrimitive(paramType.ToLower()))
					nonPrimitiveTypes.Add(paramType);
			if (nonPrimitiveTypes.Count > 0) {
				// Callback function:
				if (nonPrimitiveTypes.Contains("Function")) {
					result.Matches |= standardParamsCollection
						? SpecialParamMatch.STANDARD_COLLECTION_FUNC
						: SpecialParamMatch.SPREAD_COLLECTION_FUNC;
					nonPrimitiveTypes.Remove("Function");
				}
				if (standardParamsCollection && nonPrimitiveTypes.Contains("Function[]")) {
					result.Matches |= SpecialParamMatch.STANDARD_COLLECTION_FUNC_ARR;
					nonPrimitiveTypes.Remove("Function[]");
				}
				// Configuration object:
				if (nonPrimitiveTypes.Contains("Object")) { 
					result.Matches |= standardParamsCollection
						? SpecialParamMatch.STANDARD_COLLECTION_OBJECT
						: SpecialParamMatch.SPREAD_COLLECTION_OBJECT;
					nonPrimitiveTypes.Remove("Object");
				}
				if (nonPrimitiveTypes.Contains("object")) { 
					result.Matches |= standardParamsCollection
						? SpecialParamMatch.STANDARD_COLLECTION_OBJECT
						: SpecialParamMatch.SPREAD_COLLECTION_OBJECT;
					nonPrimitiveTypes.Remove("object");
				}
				if (standardParamsCollection && nonPrimitiveTypes.Contains("Object[]")) {
					result.Matches |= SpecialParamMatch.STANDARD_COLLECTION_OBJECT_ARR;
					nonPrimitiveTypes.Remove("Object[]");
				}
				if (standardParamsCollection && nonPrimitiveTypes.Contains("object[]")) {
					result.Matches |= SpecialParamMatch.STANDARD_COLLECTION_OBJECT_ARR;
					nonPrimitiveTypes.Remove("object[]");
				}
				// Any other like: Ext.event.Event:
				if (nonPrimitiveTypes.Count > 0) {
					bool arrayMatch = false;
					bool nonArrayMatch = false;
					foreach (string nonPrimitiveType in nonPrimitiveTypes) {
						if (nonPrimitiveType.EndsWith("[]")) {
							arrayMatch = true;
						} else {
							nonArrayMatch = true;
						}
						result.MethodOrEventAllParamTypes.Add(nonPrimitiveType);
					}
					if (standardParamsCollection && arrayMatch)
						result.Matches |= SpecialParamMatch.STANDARD_COLLECTION_INTERFACE_ARR;
					if (nonArrayMatch) 
						result.Matches |= standardParamsCollection
							? SpecialParamMatch.STANDARD_COLLECTION_INTERFACE
							: SpecialParamMatch.SPREAD_COLLECTION_INTERFACE;
				}
			}
			return result;
		}
		protected void readFunctionParamAddToParamsList (MemberParam extObjectMemberParam, ref FuncParamsSyntaxCollections functionParamsSyntaxCollections, ref ParsedTypes paramTypes, string currentClassName, bool eventCompleting, bool lastParamDefinition) {
			bool optional = (
				(extObjectMemberParam.Optional.HasValue && extObjectMemberParam.Optional.Value == true) ||
				(extObjectMemberParam.Doc != null && extObjectMemberParam.Doc.Contains("(optional)"))
			);
			string[] docs = this.readJsDocs(
				extObjectMemberParam.Doc, 
				(eventCompleting
					? JsDocsType.EVENT_PARAM
					: JsDocsType.METHOD_PARAM), 
				currentClassName, 
				extObjectMemberParam.Name
			);
			/*if (!lastParamDefinition) {
				functionParamsSyntaxCollections.StandardParamsSyntax.Add(new Param(
					this.sanitizeName(extObjectMemberParam.Name),
					docs,
					paramTypes.MethodOrEventParam,
					optional,
					false // not last param can NOT have spread syntax - so this could be fixed false
				));
			} else {*/
				// When last param types are parsed - it's necessary 
				// to determinate single or multiple method forms by spread 
				// and non-spread last param types

				if (
					!functionParamsSyntaxCollections.SpreadParamFound && 
					paramTypes.MethodOrEventParam.Count > 0 && 
					paramTypes.MethodOrEventSpreadParam.Count > 0
				) {
					// There will be two method forms:
					functionParamsSyntaxCollections.SpreadParamFound = true;
					functionParamsSyntaxCollections.SpreadParamsSyntax = new List<Param>(
						functionParamsSyntaxCollections.StandardParamsSyntax
					);
					functionParamsSyntaxCollections.SpreadParamsSyntax.Add(new Param(
						this.sanitizeName(extObjectMemberParam.Name),
						docs,
						paramTypes.MethodOrEventSpreadParam,
						optional,
						true
					));
					functionParamsSyntaxCollections.StandardParamsSyntax.Add(new Param(
						this.sanitizeName(extObjectMemberParam.Name),
						docs,
						paramTypes.MethodOrEventParam,
						optional,
						false
					));
				} else if (paramTypes.MethodOrEventParam.Count > 0) {
					// There will be only standard params method form:
					functionParamsSyntaxCollections.StandardParamsSyntax.Add(new Param(
						this.sanitizeName(extObjectMemberParam.Name),
						docs,
						paramTypes.MethodOrEventParam,
						optional,
						false
					));
				} else if (
					!functionParamsSyntaxCollections.SpreadParamFound && 
					paramTypes.MethodOrEventSpreadParam.Count > 0
				) {
					if (functionParamsSyntaxCollections.StandardParamsSyntax.Count > 0) {
						// There is something in standard param(s) from previous param - duplicate everything into spread params collection first:
						functionParamsSyntaxCollections.SpreadParamFound = true;
						functionParamsSyntaxCollections.SpreadParamsSyntax = new List<Param>(
							functionParamsSyntaxCollections.StandardParamsSyntax
						);
						functionParamsSyntaxCollections.SpreadParamsSyntax.Add(new Param(
							this.sanitizeName(extObjectMemberParam.Name),
							docs,
							paramTypes.MethodOrEventSpreadParam,
							optional,
							true
						));
					} else {
						// Add into spread params collection:
						functionParamsSyntaxCollections.SpreadParamFound = true;
						functionParamsSyntaxCollections.SpreadParamsSyntax.Add(new Param(
							this.sanitizeName(extObjectMemberParam.Name),
							docs,
							paramTypes.MethodOrEventSpreadParam,
							optional,
							true
						));
					}
				}
			//}
		}
	}
}
