using ExtTs.ExtTypes;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public delegate void typesCheckerProgressHandler(int processedClassCount, string processedClass);
	public class TypesChecker {
		protected Processor processor;
		public TypesChecker(Processor processor) {
			this.processor = processor;
		}
		protected internal bool CheckAllTypesExistence (typesCheckerProgressHandler progressHandler) {
			int extClassesIndex = 0;
			// check all class parents definitions existence:
			ExtClass extClass;
			string parentClassFullName;
			foreach (int classWithParentIndex in this.processor.Store.ExtClassesWithParent) {
				extClass = this.processor.Store.ExtAllClasses[classWithParentIndex];
				// Function callbacks are rendered directly, not as class types:
				if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CALLBACK) 
					continue;
				parentClassFullName = extClass.Extends.FullName;
				if (!this.processor.Store.ExtClassesMap.ContainsKey(parentClassFullName)) 
					// add parent class name definition into unknown types:
					this.processor.Store.AddUnknownType(
						parentClassFullName, extClass.Name.FullName
					);
				extClassesIndex += 1;
				progressHandler.Invoke(
					extClassesIndex,
					extClass.Name.FullName
				);
			}
			// check all class members types existence:
			string[] allDefinedTypes = this.processor.Store.TypesPlaces.Keys.ToArray<string>();
			string typeFullName;
			List<TypeDefinitionSource> definedPlaces;
			List<string> definedPlacesStr;
			for (int i = 0; i < allDefinedTypes.Length; i++) {
				typeFullName = allDefinedTypes[i];
				if (!this.processor.Store.ExtClassesMap.ContainsKey(typeFullName))
					if (!this.tryToGetTypeExistenceAsClassStaticProperty(typeFullName)) {
						definedPlaces = this.processor.Store.TypesPlaces[typeFullName];
						definedPlacesStr = new List<string>();
						foreach (TypeDefinitionSource definedPlace in definedPlaces)
							definedPlacesStr.Add(definedPlace.DefinitionFullPath);
						// add parent class name definition into unknown types:
						this.processor.Store.AddUnknownType(
							typeFullName, String.Join(", ", definedPlacesStr)
						);
					}
			}
			return true;
		}
		// Ext.dom.Element.CLIP
		protected bool tryToGetTypeExistenceAsClassStaticProperty (string typeFullName) {
			ExtClass extClass;
			List<string> typeFullNameExploded = typeFullName.Split('.').ToList();
			int lastExplodedIndex = typeFullNameExploded.Count - 1;
			string statPropName = typeFullNameExploded[lastExplodedIndex];
			typeFullNameExploded.RemoveAt(lastExplodedIndex);
			string typeFullNameStatProp = String.Join(".", typeFullNameExploded);
			if (this.processor.Store.ExtClassesMap.ContainsKey(typeFullNameStatProp)) {
				extClass = this.processor.Store.GetByFullName(typeFullNameStatProp);
				if (
					extClass != null && 
					extClass.Members.PropertiesStatic.ContainsKey(statPropName)
				) {
					// there is static property - check if it has only single type and create interface for it
					Property staticProp = extClass.Members.PropertiesStatic[statPropName] as Property;
					if (staticProp.Types.Count == 1) {
						string altClassName = extClass.Name.FullName + "." + statPropName;
						string[] propTypes = staticProp.Types.Keys.ToArray<string>();
						string propType = propTypes[0];
						string propTypeUc;
						if (JavascriptInternals.IsJsPrimitiveWithTypescriptInterface(propType.ToLower())) { 
							propTypeUc = propType.Substring(0, 1).ToUpper() + propType.Substring(1);
						} else {
							propTypeUc = propType;
						}
						// declare namespace Ext.dom.Element { interface CLIP extends Number {} }
						ExtClass alternativeAliasClass = new ExtClass(
							altClassName, propTypeUc, new string[] { }
						);
						alternativeAliasClass.Package = extClass.Package;
						//alternativeAliasClass.SrcJson = extClass.SrcJson;
						alternativeAliasClass.Name.PackagedNamespace = this.processor.Reader.GetPackagedNamespaceFromFullClassName(
							extClass.Name.FullName
						);
						alternativeAliasClass.ClassType = ExtTypes.Enums.ClassType.CLASS_CONSTANT_ALIAS;
						this.processor.Store.AddExtClass(alternativeAliasClass);
						// add into method, where is type defined also the base type
						if (!this.processor.Store.StaticPropsTypes.ContainsKey(altClassName)) 
							this.processor.Store.StaticPropsTypes.Add(altClassName, new List<string>());
						List<string> staticPropTypes = this.processor.Store.StaticPropsTypes[altClassName];
						if (!staticPropTypes.Contains(propType))
							staticPropTypes.Add(propType);
					}
					return true;
				}
			}
			return false;
		}
		protected internal bool CheckAllSingletonClasses(typesCheckerProgressHandler progressHandler) {
			int extClassesIndex = 0;
			// check all class parents definitions existence:
			bool namespaceClassRenderedAsClass;
			ExtClass namespaceClass;
			ExtClass namespaceClassParent;
			ExtClass namespaceAliasClass;
			Property singletonAliasProp;
			foreach (ExtClass extClass in this.processor.Store.ExtStandardSingletonClasses) {
				if (extClass.Name.FullName == "Ext") continue;
				// Try to find the same class name as singleton namespace is:
				if (this.processor.Store.ExtClassesMap.ContainsKey(extClass.Name.NamespaceName)) {
					namespaceClass = this.processor.Store.GetByFullName(extClass.Name.NamespaceName);
					// Check if class is standard, non singleton, or singleton but non "Ext" or it could be alias without such parent:
					namespaceClassRenderedAsClass = false;
					if (namespaceClass.ClassType == ExtTypes.Enums.ClassType.CLASS_STANDARD) {
						namespaceClassRenderedAsClass = (
							namespaceClass.Singleton == false || (
								namespaceClass.Singleton &&
								namespaceClass.Name.FullName == "Ext"
							)
						);
					} else if (
						namespaceClass.ClassType == ExtTypes.Enums.ClassType.CLASS_ALIAS &&
						namespaceClass.Extends == null
					) {
						namespaceClassRenderedAsClass = true;
					} else if (
						namespaceClass.ClassType == ExtTypes.Enums.ClassType.CLASS_ALIAS &&
						namespaceClass.Extends != null
					) {
						namespaceClassParent = this.processor.Store.GetParentClassByCurrentClassFullName(
							namespaceClass.Extends.FullName
						);
						namespaceClassRenderedAsClass = (
							namespaceClassParent.Singleton == false || (
								namespaceClassParent.Singleton &&
								namespaceClassParent.Name.FullName == "Ext"
							)
						);
					}
					singletonAliasProp = new Property(
						extClass.Name.ClassName,
						new List<string>() { extClass.Name.FullName },
						null,
						extClass.Name.FullName,
						false
					);
					singletonAliasProp.Renderable = true;
					singletonAliasProp.SingletonInstance = extClass;
					if (namespaceClassRenderedAsClass) {
						// Add public static readonly property:
						singletonAliasProp.AccessModJs = ExtTypes.Enums.AccessModifier.PUBLIC;
						singletonAliasProp.AccessModTs = ExtTypes.Enums.AccessModifier.PUBLIC;
						singletonAliasProp.IsStatic = true;
						singletonAliasProp.IsReadOnly = true;
					}/* else {
						// Add property only (rendered in interface):
					}*/
					namespaceClass.AddMemberProperty(singletonAliasProp);
				} else {
					// create alias class like "declare namespace Ext { class dom { public static readonly Helper; } }":
					namespaceAliasClass = new ExtClass(
						extClass.Name.NamespaceName, "", null
					);
					namespaceAliasClass.Package = extClass.Package;
					//namespaceAliasClass.SrcJson = extClass.SrcJson;
					namespaceAliasClass.Name.PackagedNamespace = this.processor.Reader.GetPackagedNamespaceFromFullClassName(
						extClass.Name.FullName
					);
					namespaceAliasClass.ClassType = ExtTypes.Enums.ClassType.CLASS_ALIAS;
					singletonAliasProp = new Property(
						extClass.Name.ClassName,
						new List<string>() { extClass.Name.FullName },
						null,
						extClass.Name.FullName,
						false
					);
					singletonAliasProp.Renderable = true;
					singletonAliasProp.AccessModJs = ExtTypes.Enums.AccessModifier.PUBLIC;
					singletonAliasProp.AccessModTs = ExtTypes.Enums.AccessModifier.PUBLIC;
					singletonAliasProp.IsStatic = true;
					singletonAliasProp.IsReadOnly = true;
					singletonAliasProp.SingletonInstance = extClass;
					namespaceAliasClass.AddMemberProperty(singletonAliasProp);
					this.processor.Store.AddExtClass(namespaceAliasClass);
				}
				//
				extClassesIndex += 1;
				progressHandler.Invoke(
					extClassesIndex,
					extClass.Name.FullName
				);
			}
			// check all class members types existence:
			string[] allDefinedTypes = this.processor.Store.TypesPlaces.Keys.ToArray<string>();
			string typeFullName;
			List<TypeDefinitionSource> definedPlaces;
			List<string> definedPlacesStr;
			for (int i = 0; i < allDefinedTypes.Length; i++) {
				typeFullName = allDefinedTypes[i];
				if (!this.processor.Store.ExtClassesMap.ContainsKey(typeFullName)) 
					if (!this.tryToGetTypeExistenceAsClassStaticProperty(typeFullName)) {
						definedPlaces = this.processor.Store.TypesPlaces[typeFullName];
						definedPlacesStr = new List<string>();
						foreach (TypeDefinitionSource definedPlace in definedPlaces)
							definedPlacesStr.Add(definedPlace.DefinitionFullPath); 
						// add parent class name definition into unknown types:
						this.processor.Store.AddUnknownType(
							typeFullName, String.Join(", ", definedPlacesStr)
						);
					}
			}
			return true;
		}
	}
}
