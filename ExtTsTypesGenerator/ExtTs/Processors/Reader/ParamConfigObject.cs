using System;
using System.Linq;
using System.Collections.Generic;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers.MemberParams;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes;
using System.Diagnostics;

namespace ExtTs.Processors {
	public partial class Reader {
		protected void readFunctionParamConfObjectProperties (string currentClassName, string extParamsPseudoClassName, string eventOrMethodName, string baseClassOrInterfaceFullName, string pseudoClassDoc, IList<MemberParamProperty> funcParamConfObjectProps, bool eventsCompleting, bool isStatic) {
			ConfigProperty funcParamConfObjectPropOther;
			ExtClass extParamConfObject = new ExtClass(
				extParamsPseudoClassName, 
				(baseClassOrInterfaceFullName == "Object"
					? SpecialsGenerator.BASE_PARAMS_INTERFACE_NAME
					: baseClassOrInterfaceFullName), 
				this.readJsDocs(
					pseudoClassDoc, 
					eventsCompleting
						? JsDocsType.EVENT_PARAM_CONF_OBJ
						: JsDocsType.METHOD_PARAM_CONF_OBJ, 
					extParamsPseudoClassName
				)
			);
			extParamConfObject.Package = this.currentPackage.Type;
			extParamConfObject.Name.PackagedNamespace = this.GetPackagedNamespaceFromFullClassName(
				currentClassName
			);
			if (eventsCompleting) {
				extParamConfObject.Link = new string[] {
					currentClassName + "." + eventOrMethodName,
					this.GetLinkHrefForClassEvent(
						currentClassName, eventOrMethodName
					)
				};
			} else {
				extParamConfObject.Link = new string[] {
					currentClassName + "." + eventOrMethodName,
					this.GetLinkHrefForClassMethod(
						currentClassName, isStatic, eventOrMethodName
					)
				};
			}
			extParamConfObject.ClassType = ClassType.CLASS_METHOD_PARAM_CONF_OBJ;
			foreach (MemberParamProperty funcParamConfObjectProp in funcParamConfObjectProps) {
				funcParamConfObjectPropOther = this.readFunctionParamConfObjectProperty(
					extParamsPseudoClassName, 
					pseudoClassDoc, 
					funcParamConfObjectProp, 
					eventOrMethodName,
					eventsCompleting,
					isStatic
				);
				extParamConfObject.AddMemberProperty(funcParamConfObjectPropOther);
			}
			this.processor.Store.AddExtClass(extParamConfObject);
		}
		protected ConfigProperty readFunctionParamConfObjectProperty (
			string pseudoClassName, 
			string pseudoClassDoc, 
			MemberParamProperty funcParamConfObjectProp, 
			string eventOrMethodName, 
			bool eventCompleting, 
			bool isStatic
		) {
			string extParamsPseudoClassName;
			string funcParamConfObjectPropType;
			string funcParamPropName = this.sanitizeName(funcParamConfObjectProp.Name);
			List<string> types = this.typesParser.Parse(
				eventCompleting
					? TypeDefinitionPlace.EVENT_PARAM_CFGOBJ_PROP
					: TypeDefinitionPlace.METHOD_PARAM_CFGOBJ_PROP, 
				pseudoClassName, 
				funcParamConfObjectProp.Name, 
				funcParamConfObjectProp.Type
			).CfgOrProp;
			if (
				funcParamConfObjectProp.Properties != null &&
				funcParamConfObjectProp.Properties.Count >= 0
			) {
				// Param objet item has subdeclarations:
				extParamsPseudoClassName = pseudoClassName + "." + funcParamConfObjectProp.Name;
				funcParamConfObjectPropType = "|" + (
					funcParamConfObjectProp.Type
						.Replace("/", "|")
						.Replace(" ", "")
				) + "|";
				if (funcParamConfObjectPropType.Contains("|Function|")) {
					// event/method config object has property with callback definition:
					this.readFunctionParamCallbackProperties(
						extParamsPseudoClassName,
						funcParamConfObjectProp.Name,
						funcParamConfObjectProp.Doc,
						funcParamConfObjectProp.Properties,
						eventCompleting
					);
					types.Add(extParamsPseudoClassName);
				} else {
					// event/method config object has only property with simple type definition:
					this.readFunctionParamConfObjectProperties(
						pseudoClassName,
						extParamsPseudoClassName,
						eventOrMethodName,
						"Object",
						funcParamConfObjectProp.Doc,
						funcParamConfObjectProp.Properties,
						eventCompleting,
						isStatic
					);
					types.Add(extParamsPseudoClassName);
				}
			}
			string[] docs = this.readJsDocs(
				funcParamConfObjectProp.Doc, 
				eventCompleting
					? JsDocsType.EVENT_PARAM_CONF_OBJ_PROP
					: JsDocsType.METHOD_PARAM_CONF_OBJ_PROP, 
				pseudoClassName, 
				funcParamPropName
			);
			bool required = funcParamConfObjectProp.Required.HasValue
				? funcParamConfObjectProp.Required == true
				: funcParamConfObjectProp.Doc.Contains("(required)");
			ConfigProperty funcParamConfObjectPropOther = new ConfigProperty(
				funcParamPropName, 
				types, 
				docs, 
				pseudoClassName, 
				true
			);
			funcParamConfObjectPropOther.Required = required;
			funcParamConfObjectPropOther.Deprecated = this.readJsDocsDeprecated(
				funcParamConfObjectProp.Deprecated, pseudoClassName, funcParamPropName
			);
			funcParamConfObjectPropOther.DefaultValue = String.IsNullOrEmpty(funcParamConfObjectProp.Default) 
				? "" 
				: funcParamConfObjectProp.Default;
			return funcParamConfObjectPropOther;
		}
	}
}
