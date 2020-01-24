using System;
using System.Collections.Generic;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.MemberParams;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes;

namespace ExtTs.Processors {
	public partial class Reader {
		protected void readFunctionParamCallbackProperties (string pseudoCallbackName, string eventOrMethodName, string pseudoClassDoc, IList<MemberParamProperty> funcParamCallbackProps, bool eventCompleting) {
			string[] docs;
			string[] returnDocs = new string[] { };
			List<Param> funcParams = new List<Param>();
			List<string> returnTypes = new List<string>();
			// `pseudoCallbackName = 'Ext.Array.staticMethodsParams.each.Fn';`
			ParsedTypes paramTypes;
			foreach (MemberParamProperty funcParamCallbackProp in funcParamCallbackProps) {
				if (funcParamCallbackProp.Name == "return") {
					returnDocs = this.readJsDocs(
						funcParamCallbackProp.Doc, 
						(eventCompleting
							? JsDocsType.EVENT_PARAM_CALLBACK_RETURN
							: JsDocsType.METHOD_PARAM_CALLBACK_RETURN), 
						pseudoCallbackName, 
						funcParamCallbackProp.Name
					);
					returnTypes = this.typesParser.Parse(
						eventCompleting
							? TypeDefinitionPlace.EVENT_PARAM_CALLBACK_RETURN
							: TypeDefinitionPlace.METHOD_PARAM_CALLBACK_RETURN,
						pseudoCallbackName,
						"return",
						funcParamCallbackProp.Type
					).MethodOrEventReturn;
				} else {
					docs = this.readJsDocs(
						funcParamCallbackProp.Doc, 
						(eventCompleting
							? JsDocsType.EVENT_PARAM_CALLBACK_PARAM
							: JsDocsType.METHOD_PARAM_CALLBACK_PARAM), 
						pseudoCallbackName, 
						funcParamCallbackProp.Name
					);
					paramTypes = this.typesParser.Parse(
						eventCompleting
							? TypeDefinitionPlace.EVENT_PARAM_CALLBACK_PARAM
							: TypeDefinitionPlace.METHOD_PARAM_CALLBACK_PARAM,
						pseudoCallbackName,
						funcParamCallbackProp.Name,
						funcParamCallbackProp.Type
					);
					if (paramTypes.MethodOrEventSpreadParam.Count > 0) {
						try {
							throw new Exception("Spread params syntax is not supported in method param type: `callback`.");
						} catch (Exception ex) {
							this.processor.Exceptions.Add(ex);
						}
					}
					funcParams.Add(new Param(
						this.sanitizeName(funcParamCallbackProp.Name),
						docs,
						paramTypes.MethodOrEventParam,
						true,
						false
					));
				}

			}
			if (returnTypes.Count == 0)
				returnTypes.Add("void");
			ExtClass extParamCallback = new Callback(
				pseudoCallbackName, 
				this.readJsDocs(
					pseudoClassDoc, 
					eventCompleting
						? JsDocsType.EVENT_PARAM_CALLBACK
						: JsDocsType.METHOD_PARAM_CALLBACK, 
					pseudoCallbackName
				),
				returnDocs,
				funcParams,
				returnTypes
			);
			extParamCallback.ClassType = ClassType.CLASS_METHOD_PARAM_CALLBACK;
			this.processor.Store.AddExtClass(extParamCallback);
		}
	}
}
