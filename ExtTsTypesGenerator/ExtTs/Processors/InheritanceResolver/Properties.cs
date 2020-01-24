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
	/**
	 * Fix all completed standard classes extended from some other class in store 
	 * with TypeScript compatible properties and methods inheritance.
	 */
	public partial class InheritanceResolver {
		protected void setBaseClassPropsNotInherite (
			ref ExtClass extClass, 
			Dictionary<string, Member> propsCollection, 
			bool instancePropsProcessing
		) {
			string[] propNames = propsCollection.Keys.ToArray<string>();
			string propName;
			Property prop;
			for (int i = 0; i < propNames.Length; i++) {
				propName = propNames[i];
				prop = propsCollection[propName] as Property;
				prop.Inherited = false;
				propsCollection[propName] = prop;
			}
		}
		protected void resolvePropertiesExtending(
			ref ExtClass currentExtClass, 
			Dictionary<string, Member> propsCollection, 
			bool instancePropsProcessing
		) {
			Property prop;
			List<Property> propsTree;
			bool currentPropIsNecessaryToDefine;
			string[] propNames = propsCollection.Keys.ToArray<string>();
			string propName;
			for (int i = 0; i < propNames.Length; i++) {
				propName = propNames[i];
				prop = propsCollection[propName] as Property;
				currentPropIsNecessaryToDefine = /*prop.OwnedByCurrent ||*/ prop.Renderable;
				propsTree = new List<Property>() { prop };
				this.resolvePropertyExtendingTraverse(
					ref currentExtClass,
					ref prop,
					ref propsTree,
					ref currentPropIsNecessaryToDefine,
					instancePropsProcessing,
					propName
				);
				if (instancePropsProcessing && propName == "self") {
					prop.Inherited = propsTree.Count > 1;
					propsCollection[propName] = prop;
					continue;
				}
				propsCollection[propName] = this.resolvePropertyExtendingAccessModifiers(
					ref prop,
					ref propsTree,
					instancePropsProcessing,
					currentPropIsNecessaryToDefine
				);
			}
		}
		/**
		 * Traverse over inheritance tree and check the same property name
		 * existence. If there is another property with the same name, solve 
		 * boolean about if the property is necessary to render also in current class.
		 * Then solve parent classes property types inheritance and complete
		 * all properties inheritance tree from parent classes list.
		 */
		protected void resolvePropertyExtendingTraverse(
			ref ExtClass currentExtClass, 
			ref Property prop, 
			ref List<Property> propsTree, 
			ref bool currentPropIsNecessaryToDefine, 
			bool instancePropsProcessing, 
			string propName
		) {
			Property parentProp;
			Dictionary<string, Member> parentClassPropsCollection;
			bool currentPropContainsAny = prop.Types.ContainsKey("any");
			bool compatibleTypeAdded = false;
			bool checkOnlyInInheritanceTree = false;
			bool propertyChildAndParentTypesAreDifferent = false;
			bool propertyChildAndParentDefaultsAreDifferent = false;
			foreach (ExtClass parentExtClass in currentExtClass.Parents) {
				// Check property existence and correct types if necessary:
				parentClassPropsCollection = instancePropsProcessing
					? parentExtClass.Members.Properties
					: parentExtClass.Members.PropertiesStatic;
				// If parent class collection doesn't contain the same property, continue to next parent class:
				if (!parentClassPropsCollection.ContainsKey(propName))
					continue;
				parentProp = parentClassPropsCollection[propName] as Property;
				propsTree.Add(parentProp);
				/**
				 * Property is also necessary to redefine in child if child default value
				 * is different from parent default value.
				 */
				if (!propertyChildAndParentDefaultsAreDifferent) {
					propertyChildAndParentDefaultsAreDifferent = prop.DefaultValue != parentProp.DefaultValue;
					if (propertyChildAndParentDefaultsAreDifferent)
						currentPropIsNecessaryToDefine = true;
				}
				// If types are already solved, continue to ceck only private flags in next parent classes:
				if (checkOnlyInInheritanceTree) 
					continue;
				/**
				 * If there is `any` type defined in parent class, 
				 * child property type is always assignable into any type.
				 * If there is not, it's necessary to check all child types,
				 * if they are assignable into any of parent type(s).
				 */
				if (
					!compatibleTypeAdded && 
					!parentProp.Types.ContainsKey("any") && 
					(!instancePropsProcessing || (instancePropsProcessing && propName != "self"))
				) {
					/**
					 * If current class property types contains some type,
					 * which is not assignable into any of parent types, add compatible type.
					 * When class property contains any compatible member, it's necessary for 
					 * correct TypeScript definition to render `any` property type at the end
					 * of property types list. But it's thing for renderer to deal wth later
					 *
					 * This is for cases when parent has for example types: `string` and child
					 * has types: `string | number`. Than there is necessary to add type: `any` into child.
					 * Or if parent has types: `string | number` and child has types: `boolean` etc...
					 */
					foreach (var propTypeItem in prop.Types) {
						if (!this.isPropertyTypeCompatibleWithAnyParentTypes(
							propTypeItem.Key, parentProp.Types
						)) {
							// Child class prop types contains some more type not 
							// assignable into any parent types, so add `any` 
							// compatibility type from parent class into current 
							// class property types:
							if (!currentPropContainsAny) {
								prop.Types.Add(
									"any", new ExistenceReason(
										ExistenceReasonType.COMPATIBLE_TYPES,
										parentExtClass.Name.FullName
									)
								);
							}
							compatibleTypeAdded = true;
							currentPropIsNecessaryToDefine = true;
							break;
						}
					}
				}
				/**
				 * Property is also necessary to redefine in child if child types
				 * are all assignable into parent types but child types are different.
				 */
				propertyChildAndParentTypesAreDifferent = this.getPropertyChildAndParentTypesAreDifferent(
					prop.Types, parentProp.Types
				);
				if (propertyChildAndParentTypesAreDifferent)
					currentPropIsNecessaryToDefine = true;
				/**
				 * The cycle of traversing the parental classes runs from the classes 
				 * with the lowest number of parents to the classes with the highest 
				 * number of parents. This means that if I find a property in a parent 
				 * with other types than the current class property, it is certain that 
				 * this parent property has been already processed earlier or further 
				 * in the inheritance tree, there are no other types to consider. 
				 * Therefore, the loop can now terminate.
				*/
				if (
					compatibleTypeAdded || 
					propertyChildAndParentTypesAreDifferent || 
					propertyChildAndParentDefaultsAreDifferent
				)
					checkOnlyInInheritanceTree = true;
			}
		}
		/**
		 * Set into current class property the boolean, if it is necessary
		 * to render the property in current class again or not.
		 * Than solve also access modifier by parent class properties
		 * and solve cross access modifier definitions.
		 */
		protected Property resolvePropertyExtendingAccessModifiers(
			ref Property prop, 
			ref List<Property> propsTree, 
			bool instancePropsProcessing, 
			bool currentPropIsNecessaryToDefine
		) {
			List<int> accessModsDependentPropsClassesIndex;
			List<int> accessModsDependentPropsClassesIndexClone;
			int index;
			AccessModifier highestAccessModTs;
			// Try to find most public access modifier:
			if (propsTree.Count == 1) {
				// Render property if the property is defined for first time in inheritance tree:
				foreach (Property propsTreeItem in propsTree) 
					propsTreeItem.Inherited = false;
				currentPropIsNecessaryToDefine = true;
			} else if (propsTree.Count > 1) {
				foreach (Property propsTreeItem in propsTree) 
					propsTreeItem.Inherited = true;
				// If there are more properties in inheritance tree - it's necessary to have 
				// TypeScript access modifier at protected at minimal:
				highestAccessModTs = AccessModifier.PROTECTED;
				foreach (Property treeProp1 in propsTree) {
					if (
						highestAccessModTs == AccessModifier.PROTECTED &&
						treeProp1.AccessModTs == AccessModifier.PUBLIC
					) highestAccessModTs = AccessModifier.PUBLIC;
					// PUBLIC modifier is highest, so it's possible to end the loop:
					if (highestAccessModTs == AccessModifier.PUBLIC)
						break;
				}
				accessModsDependentPropsClassesIndex = new List<int>();
				foreach (Property treeProp2 in propsTree) {
					if (this.processor.Store.ExtClassesMap.ContainsKey(treeProp2.Owner.FullName)) {
						accessModsDependentPropsClassesIndex.Add(
							this.processor.Store.ExtClassesMap[treeProp2.Owner.FullName]
						);
					} else {
						accessModsDependentPropsClassesIndex.Add(-1);
					}
				}
				index = 0;
				foreach (Property treeProp3 in propsTree) {
					accessModsDependentPropsClassesIndexClone = new List<int>(accessModsDependentPropsClassesIndex);
					accessModsDependentPropsClassesIndexClone.RemoveAt(index);
					treeProp3.SetResultAccessModTsWithDependentAccessModProps(
						highestAccessModTs, 
						instancePropsProcessing, 
						accessModsDependentPropsClassesIndexClone
					);
					index += 1;
				}
			}
			if (currentPropIsNecessaryToDefine) 
				// this property has to be generated:
				prop.Renderable = true;
			return prop;
		}
		/**
		 * Return true if given current class property type is assignable 
		 * into any of parent class property types.
		 */
		protected bool isPropertyTypeCompatibleWithAnyParentTypes (
			string currentClassPropertyType,
			Dictionary<string, ExistenceReason> parentClassPropertyTypes
		) {
			bool result = false;
			foreach (var parentTypeItem in parentClassPropertyTypes) {
				if (InheritanceResolvers.Types.IsTypeInheratedFrom(currentClassPropertyType, parentTypeItem.Key)) {
					result = true;
					break;
				}
			}
			return result;
		}
		/**
		 * Return true if current class property types are different from parent class property types.
		 */
		protected bool getPropertyChildAndParentTypesAreDifferent (
			Dictionary<string, ExistenceReason> currentClassPropertyTypes,
			Dictionary<string, ExistenceReason> parentClassPropertyTypes
		) {
			List<string> currentClassPropTypes = new List<string>(currentClassPropertyTypes.Keys.ToArray<string>());
			currentClassPropTypes.Sort();
			List<string> parentClassPropTypes = new List<string>(parentClassPropertyTypes.Keys.ToArray<string>());
			parentClassPropTypes.Sort();
			string currentClassPropTypesStr = String.Join("|", currentClassPropTypes);
			string parentClassPropTypesStr = String.Join("|", parentClassPropTypes);
			return (
				currentClassPropTypesStr.Length != parentClassPropTypesStr.Length ||
				currentClassPropTypesStr != parentClassPropTypesStr
			);
		}
	}
}
