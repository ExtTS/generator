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
		protected void setBaseClassMethodsNotInherite (
			ref ExtClass extClass, 
			Dictionary<string, List<Member>> methodsCollection, 
			bool instanceMethodsProcessing
		) {
			string[] methodsCollectionKeys = methodsCollection.Keys.ToArray<string>();
			string methodName;
			List<Member> currentMethodVariants;
			Method currentMethodVariant;
			for (int i = 0; i < methodsCollectionKeys.Length; i++) {
				methodName = methodsCollectionKeys[i];
				currentMethodVariants = methodsCollection[methodName];
				for (int j = 0; j < currentMethodVariants.Count; j++) {
					currentMethodVariant = currentMethodVariants[j] as Method;
					currentMethodVariant.Inherited = false;
					currentMethodVariants[j] = currentMethodVariant;
				}
				methodsCollection[methodName] = currentMethodVariants;
			}
		}
		protected void resolveMethodsExtending(
			ref ExtClass currentExtClass, 
			Dictionary<string, List<Member>> methodsCollection, 
			bool instanceMethodsProcessing
		) {
			
			bool currentMethodVariantsAreNecessaryToDefine;
			string[] methodsCollectionKeys = methodsCollection.Keys.ToArray<string>();
			string methodName;
			List<Member> currentMethodVariants;
			Dictionary<string, List<Member>> methodsTree;
			Method firstMethodVariant;
			for (int i = 0; i < methodsCollectionKeys.Length; i++) {
				methodName = methodsCollectionKeys[i];
				currentMethodVariants = methodsCollection[methodName];
				firstMethodVariant = currentMethodVariants.FirstOrDefault<Member>() as Method;
				if (firstMethodVariant.IsChainable) {
					methodsCollection[methodName] = this.resolveMethodsExtendingChainableParents(
						ref currentExtClass, 
						ref currentMethodVariants,
						instanceMethodsProcessing, 
						methodName
					);
				} else {
					currentMethodVariantsAreNecessaryToDefine = firstMethodVariant.Renderable;
					methodsTree = new Dictionary<string, List<Member>>() {
						{ currentExtClass.Name.FullName, currentMethodVariants }
					};
					this.resolveMethodsExtendingTraverse(
						ref currentExtClass, 
						ref currentMethodVariants,
						ref methodsTree, 
						ref currentMethodVariantsAreNecessaryToDefine, 
						instanceMethodsProcessing, 
						methodName
					);
					methodsCollection[methodName] = this.resolveMethodsExtendingAccessModifiers(
						ref currentMethodVariants,
						ref methodsTree,
						instanceMethodsProcessing,
						currentMethodVariantsAreNecessaryToDefine
					);
				}
			}
		}
		/**
		 * If method is chanable, there is necessary to have for every class
		 * in inheritance tree to return it's `typeof Full.class.Name`.
		 * That's why all chanable methods are renderable by default.
		 */
		protected List<Member> resolveMethodsExtendingChainableParents (
			ref ExtClass currentExtClass, 
			ref List<Member> currentMethodVariants,
			bool instanceMethodsProcessing, 
			string methodName
		) {
			List<Member> result = new List<Member>();
			Dictionary<string, List<Member>> methodsTree = new Dictionary<string, List<Member>>() {
				{ currentExtClass.Name.FullName, currentMethodVariants }
			};
			ExtClass firstParentClass = null;
			Dictionary<string, List<Member>> parentMethods = null;
			foreach (ExtClass parentClass in currentExtClass.Parents) {
				parentMethods = instanceMethodsProcessing
					? parentClass.Members.Methods
					: parentClass.Members.MethodsStatic;
				if (!parentMethods.ContainsKey(methodName))
					continue;
				if (firstParentClass == null)
					firstParentClass = parentClass;
				methodsTree.Add(
					parentClass.Name.FullName,
					parentMethods[methodName]
				);
			}
			if (methodsTree.Count > 1) {
				this.resolveMethodsExtendingSetInherited(ref methodsTree, true);
			} else {
				this.resolveMethodsExtendingSetInherited(ref methodsTree, false);
			}
			bool parentClassHasVariants = false;
			if (firstParentClass != null) {
				parentMethods = instanceMethodsProcessing
					? firstParentClass.Members.Methods
					: firstParentClass.Members.MethodsStatic;
				parentClassHasVariants = parentMethods.ContainsKey(methodName);
			}
			// Check if method is in inheritance tree from parent class or from current class:
			AccessModifier highestAc = AccessModifier.NONE;
			if (parentClassHasVariants) {
				if (methodsTree.Count > 1) {
					highestAc = this.resolveMethodsExtendingHighestAccessModifier(
						ref methodsTree,
						instanceMethodsProcessing
					);
				}
			}
			Method currentMethodVariant;
			foreach (Member currentMethodMember in currentMethodVariants) {
				currentMethodVariant = (currentMethodMember as Method).Clone();
				currentMethodVariant.ReturnTypes = new List<string>() { currentExtClass.Name.FullName };
				currentMethodVariant.Renderable = true;
				currentMethodVariant.OwnedByCurrent = (currentMethodMember as Method).OwnedByCurrent;
				if (highestAc != AccessModifier.NONE)
					currentMethodVariant.AccessModTs = highestAc;
				result.Add(currentMethodVariant);
			}
			if (parentClassHasVariants) { 
				// if there is more variants in inheritance tree, method has to be protected at minimum:
				List<Member> parentMethodVariants = parentMethods[methodName];
				Method newChildCompatibleMethod;
				foreach (Member parentMethodMember in parentMethodVariants) {
					newChildCompatibleMethod = (parentMethodMember as Method).Clone();
					newChildCompatibleMethod.ExistenceReason = ExistenceReasonType.COMPATIBLE_CHAIN;
					newChildCompatibleMethod.Renderable = true;
					newChildCompatibleMethod.OwnedByCurrent = (parentMethodMember as Method).OwnedByCurrent;
					if (highestAc != AccessModifier.NONE)
						newChildCompatibleMethod.AccessModTs = highestAc;
					result.Add(newChildCompatibleMethod);
				}
			}
			return result;
		}
		/**
		 * Traverse over inheritance tree and check the same method name variants
		 * existence. If there are another method variants with the same name, solve 
		 * boolean about if the method variants are necessary to render also in current class.
		 * Then solve parent classes method variants inheritance compatibility and complete
		 * all method variants inheritance tree from parent classes list.
		 */
		protected void resolveMethodsExtendingTraverse (
			ref ExtClass currentExtClass, 
			ref List<Member> currentMethodVariants,
			ref Dictionary<string, List<Member>> methodsTree, 
			ref bool currentMethodVariantsAreNecessaryToDefine, 
			bool instanceMethodsProcessing, 
			string methodName
		) {
			Dictionary<string, List<Member>> parentClassMethodsCollection;
			List<Member> parentMethodVariants;
			List<Member> parentIncompatibleMethods;
			Method newChildCompatibleMethod;
			bool compatibleMethodVariantAdded = false;
			bool completeOnlyInInheritanceTree = false;
			bool childAndParentMethodVariantsAreDifferent = false;
			// Go throw all parent classes tree from childs to parents:
			foreach (ExtClass parentExtClass in currentExtClass.Parents) {
				// Process loop operations only if parent class has the same method name defined:
				parentClassMethodsCollection = instanceMethodsProcessing
					? parentExtClass.Members.Methods
					: parentExtClass.Members.MethodsStatic;
				// If parent class doesn't contain the same method definition, continue to next parent class:
				if (!parentClassMethodsCollection.ContainsKey(methodName))
					continue;
				parentMethodVariants = parentClassMethodsCollection[methodName];
				methodsTree.Add(parentExtClass.Name.FullName, parentMethodVariants);

				// If types are already solved, continue to ceck only private flags in next parent classes:
				if (completeOnlyInInheritanceTree) 
					continue;

				if (!compatibleMethodVariantAdded) {
					/**
					 * Add all detected incompatible methods from parent class into current class
					 * with compatible flag - recreate all method objects, do not copy them:
					 */
					parentIncompatibleMethods = this.resolveMethodsExtendingGetParentIncompatibleMembers(
						ref currentMethodVariants, ref parentMethodVariants
					);
					if (parentIncompatibleMethods.Count > 0) {
						foreach (Method parentIncompatibleMethod in parentIncompatibleMethods) {
							newChildCompatibleMethod = parentIncompatibleMethod.Clone();
							newChildCompatibleMethod.ExistenceReason = ExistenceReasonType.COMPATIBLE_TYPES;
							newChildCompatibleMethod.OwnedByCurrent = false;
							currentMethodVariants.Add(newChildCompatibleMethod);
						}
						compatibleMethodVariantAdded = true;
						currentMethodVariantsAreNecessaryToDefine = true;
					}
				}
				/**
				 * Method variants are also necessary to redefine in child if child variants
				 * are all assignable into parent types but child variants are different.
				 */
				if (!currentMethodVariantsAreNecessaryToDefine) { 
					childAndParentMethodVariantsAreDifferent = this.getChildAndParentMethodVariantsAreDifferent(
						ref currentMethodVariants, ref parentMethodVariants
					);
					if (childAndParentMethodVariantsAreDifferent)
						currentMethodVariantsAreNecessaryToDefine = true;
				}
				/**
				 * The cycle of traversing the parental classes runs from the classes 
				 * with the lowest number of parents to the classes with the highest 
				 * number of parents. This means that if I find a method variants in a parent 
				 * not contained in current class method variants, it is certain that 
				 * this parent method variants have been already processed earlier or further 
				 * in the inheritance tree, there are no other types to consider. 
				 * Therefore, the loop can now terminate.
				*/
				if (
					compatibleMethodVariantAdded || 
					childAndParentMethodVariantsAreDifferent
				)
					completeOnlyInInheritanceTree = true;
			}
		}
		/**
		 * Get `true`, if all current class method variants has the absolute same opposite 
		 * variant between parent class method variants. Else return `false`.
		 */
		protected bool getChildAndParentMethodVariantsAreDifferent(
			ref List<Member> currentMethodVariants,
			ref List<Member> parentMethodVariants
		) {
			if (currentMethodVariants.Count != parentMethodVariants.Count)
				return true;
			bool allMethodVariantsPartsAreTheSame = true;
			/**
			 * Try to break `true` statement above:
			 * 
			 * 1. Complete method variants imprints:
			 */
			List<string[]> currentMethodVariantsImprints = new List<string[]>();
			List<string[]> parentMethodVariantsImprints = new List<string[]>();
			foreach (Member currentMethodMember in currentMethodVariants) 
				currentMethodVariantsImprints.Add(
					this.getChildAndParentMethodVariantsAreDifferentImprints(
						currentMethodMember as Method
					)
				);
			foreach (Member parentClassMethodMember in parentMethodVariants) 
				parentMethodVariantsImprints.Add(
					this.getChildAndParentMethodVariantsAreDifferentImprints(
						parentClassMethodMember as Method
					)
				);
			/**
			 * Compare method imprints:
			 */
			string[] currentMethodVariantImprints;
			string[] parentMethodVariantImprints;
			int theSameVariantIndex = -1;
			for (int i = 0; i < currentMethodVariantsImprints.Count; i++) {
				currentMethodVariantImprints = currentMethodVariantsImprints[i];
				if (currentMethodVariantImprints.Length == 0) continue; // used in some previous loop
				theSameVariantIndex = -1;
				for (int j = 0; j < parentMethodVariantsImprints.Count; j++) {
					parentMethodVariantImprints = parentMethodVariantsImprints[j];
					if (parentMethodVariantImprints.Length == 0) continue; // used in some previous loop
					if (
						// compare params section:
						currentMethodVariantImprints[0] == parentMethodVariantImprints[0] &&
						// compare return section:
						currentMethodVariantImprints[1] == parentMethodVariantImprints[1]
					) {
						theSameVariantIndex = j;
						break;
					}
				}
				if (theSameVariantIndex == -1) {
					// The same method variant was not found - so the statement is broken:
					allMethodVariantsPartsAreTheSame = false;
					break;
				} else {
					// The same method variant has been found - so clean both variants for next loop:
					currentMethodVariantsImprints[i] = new string[] { };
					parentMethodVariantsImprints[theSameVariantIndex] = new string[] { };
				}
			}
			return allMethodVariantsPartsAreTheSame;
		}
		/**
		 * Get `string[2]` array with unique imprint of params section and unique imprint of return section.
		 */
		protected string[] getChildAndParentMethodVariantsAreDifferentImprints (Method methodVariant) {
			string returnTypesImprint;
			List<string> returnTypes;
			string paramsImprint;
			List<string> paramsImprints = new List<string>();
			string[] rawParamTypes;
			List<string> paramTypes;
			string paramImprint;
			foreach (Param param in methodVariant.Params) {
				rawParamTypes = param.Types.ToArray<string>();
				if (param.IsRest) 
					for (int i = 0; i < rawParamTypes.Length; i++) 
						rawParamTypes[i] = rawParamTypes[i] + "[]";
				paramTypes = new List<string>(rawParamTypes);
				paramTypes.Sort();
				paramImprint = (param.IsRest ? "..." : "")
					+ param.Name
					+ (param.Optional ? "?" : "")
					+ ": "
					+ String.Join(" | ", paramTypes);
				paramsImprints.Add(paramImprint);
			}
			paramsImprint = String.Join(", ", paramsImprints);
			returnTypes = new List<string>(methodVariant.ReturnTypes.ToArray<string>());
			returnTypes.Sort();
			returnTypesImprint = String.Join(" | ", returnTypes);
			return new string[] { paramsImprint, returnTypesImprint };
		}
		/**
		 * Get all parent method variants imcompatible to child method variants.
		 */
		protected List<Member> resolveMethodsExtendingGetParentIncompatibleMembers (
			ref List<Member> currentMethodVariants,
			ref List<Member> parentMethodVariants
		) {
			List<Member> parentMethodVariantsClone = new List<Member>(parentMethodVariants);
			List<Member> compatibleParentMethodVariants;
			/**
			 * Process all current class method variants and check, 
			 * if there is any parent class method variant compatible with
			 * this variant. If exists - remove the parent compatible variant
			 * from parent method variants clone.
			 */
			foreach (Member currentMethodVariant in currentMethodVariants) {
				compatibleParentMethodVariants = this.getCompatibleParentMethodVariants(
					currentMethodVariant as Method, ref parentMethodVariants
				);
				if (compatibleParentMethodVariants.Count > 0) 
					foreach (Member compatibleParentMethodVariant in compatibleParentMethodVariants) 
						if (parentMethodVariantsClone.Contains(compatibleParentMethodVariant))
							parentMethodVariantsClone.Remove(compatibleParentMethodVariant);
			}
			// Now all remaining method variants from parent class are incompatible:
			return parentMethodVariantsClone;
		}
		/**
		 * Set into current class method variants the boolean, if it is necessary
		 * to render the variants in current class again or not.
		 * Than solve also access modifier by parent class method variants
		 * and solve cross access modifier definitions.
		 */
		protected List<Member> resolveMethodsExtendingAccessModifiers (
			ref List<Member> currentMethodVariants,
			ref Dictionary<string, List<Member>> methodsTree,
			bool instanceMethodsProcessing,
			bool currentMethodVariantsAreNecessaryToDefine
		) {
			List<Member> newCurrentMethodVariants;
			Method currentMethodVariant;
			AccessModifier highestAc = AccessModifier.NONE;
			// Try to find most public access modifier:
			if (methodsTree.Count == 1) {
				// Render method variants if the method variants are defined for first time in inheritance tree:
				this.resolveMethodsExtendingSetInherited(ref methodsTree, false);
				currentMethodVariantsAreNecessaryToDefine = true;
			} else if (methodsTree.Count > 1) {
				this.resolveMethodsExtendingSetInherited(ref methodsTree, true);
				highestAc = this.resolveMethodsExtendingHighestAccessModifier(
					ref methodsTree,
					instanceMethodsProcessing
				);
			}
			if (currentMethodVariantsAreNecessaryToDefine) {
				// this method variants has to be generated:
				newCurrentMethodVariants = new List<Member>();
				foreach (Member currentMethodMember in currentMethodVariants) {
					currentMethodVariant = currentMethodMember as Method;
					currentMethodVariant.Renderable = true;
					if (highestAc != AccessModifier.NONE)
						currentMethodVariant.AccessModTs = highestAc;
					newCurrentMethodVariants.Add(currentMethodVariant);
				}
				currentMethodVariants = newCurrentMethodVariants;
			}
			return currentMethodVariants;
		}
		protected void resolveMethodsExtendingSetInherited (ref Dictionary<string, List<Member>> methodsTree, bool inherited) {
			Method methodsTreeVariant;
			List<Member> methodsTreeVariants;
			string[] methodsTreeKeys = methodsTree.Keys.ToArray<string>();
			string methodsTreeKey;
			for (int i = 0; i < methodsTreeKeys.Length; i++) {
				methodsTreeKey = methodsTreeKeys[i];
				methodsTreeVariants = methodsTree[methodsTreeKey];
				for (int j = 0; j < methodsTreeVariants.Count; j++) {
					methodsTreeVariant = (methodsTreeVariants[j] as Method);
					methodsTreeVariant.Inherited = inherited;
					methodsTreeVariants[j] = methodsTreeVariant;
				}
				methodsTree[methodsTreeKey] = methodsTreeVariants;
			}
		}
		/**
		 * If there are more methods in inheritance tree - it's necessary to have 
		 * TypeScript access modifier at protected at minimal.
		 */
		protected AccessModifier resolveMethodsExtendingHighestAccessModifier (
			ref Dictionary<string, List<Member>> methodsTree,
			bool instanceMethodsProcessing
		) {
			List<int> accessModsDependentMethodsClassesIndex;
			List<int> accessModsDependentMethodsClassesIndexClone;
			string[] methodsTreeKeys;
			List<Member> methodsTreeVariants;
			string classFullName;
			AccessModifier highestAccessModTs = AccessModifier.PROTECTED;
			Method firstMethodVariant;
			Method methodsTreeItemVariant;
			foreach (var treeItem1 in methodsTree) {
				firstMethodVariant = (treeItem1.Value.FirstOrDefault<Member>() as Method);
				if (
					highestAccessModTs == AccessModifier.PROTECTED &&
					firstMethodVariant.AccessModTs == AccessModifier.PUBLIC
				) highestAccessModTs = AccessModifier.PUBLIC;
				// PUBLIC modifier is highest, so it's possible to end the loop:
				if (highestAccessModTs == AccessModifier.PUBLIC)
					break;
			}
			accessModsDependentMethodsClassesIndex = new List<int>();
			foreach (var treeItem2 in methodsTree) {
				if (this.processor.Store.ExtClassesMap.ContainsKey(treeItem2.Key)) {
					accessModsDependentMethodsClassesIndex.Add(
						this.processor.Store.ExtClassesMap[treeItem2.Key]
					);
				} else {
					accessModsDependentMethodsClassesIndex.Add(-1);
				}
			}
			methodsTreeKeys = methodsTree.Keys.ToArray<string>();
			for (int index = 0; index < methodsTreeKeys.Length; index++) {
				classFullName = methodsTreeKeys[index];
				methodsTreeVariants = methodsTree[classFullName];
				accessModsDependentMethodsClassesIndexClone = new List<int>(accessModsDependentMethodsClassesIndex);
				accessModsDependentMethodsClassesIndexClone.RemoveAt(index);
				for (int subIndex = 0; subIndex < methodsTreeVariants.Count; subIndex++) {
					methodsTreeItemVariant = methodsTreeVariants[subIndex] as Method;
					methodsTreeItemVariant.SetResultAccessModTsWithDependentAccessModMethodsVariants(
						highestAccessModTs, 
						instanceMethodsProcessing, 
						accessModsDependentMethodsClassesIndexClone
					);
					methodsTree[classFullName][subIndex] = methodsTreeItemVariant;
				}
			}
			return highestAccessModTs;
		}

		/**
		 * Get all TypeScript compatible parent class method variants.
		 */
		protected List<Member> getCompatibleParentMethodVariants (
			Method currentClassMethodVariant,
			ref List<Member> parentClassMethodVariants
		) {
			List<Member> compatibleParentMethodVariants = new List<Member>();
			Method parentMethodVariant;
			bool returnTypesCompatible;
			bool paramsTypesCompatible;
			foreach (Member parentClassMethodVariant in parentClassMethodVariants) {
				parentMethodVariant = parentClassMethodVariant as Method;
				// check return types compatibility:
				returnTypesCompatible = this.isReturnTypesCompatible(
					currentClassMethodVariant.ReturnTypes,
					parentMethodVariant.ReturnTypes
				);
				if (!returnTypesCompatible) continue;
				// check params types compatibility:
				paramsTypesCompatible = this.isParamsTypesCompatible(
					currentClassMethodVariant.Params,
					parentMethodVariant.Params
				);
				if (!paramsTypesCompatible) continue;
				// if current class method variant is compatible with
				// parent class method variant - add the parent class
				// method variant into result list:
				compatibleParentMethodVariants.Add(parentClassMethodVariant);
			}
			return compatibleParentMethodVariants;
		}
		/**
		 * Check return types compatibility - child method variant is compatible 
		 * with parent method variant if all child return types are compatible 
		 * with parent method return types or if child method variant return 
		 * types contains `any` or if parent method variant return types contains `any`:
		 */
		protected bool isReturnTypesCompatible (
			List<string> currentClassMethodVariantReturnTypes,
			List<string> parentClassMethodVariantReturnTypes
		) {
			bool returnTypesCompatible = (
				currentClassMethodVariantReturnTypes.Contains("any") ||
				parentClassMethodVariantReturnTypes.Contains("any")
			);
			int compatibleTypesCount;
			bool childReturnTypeCompatible;
			if (!returnTypesCompatible) {
				compatibleTypesCount = 0;
				foreach (string childReturnType in currentClassMethodVariantReturnTypes) {
					childReturnTypeCompatible = false;
					if (parentClassMethodVariantReturnTypes.Contains(childReturnType)) {
						childReturnTypeCompatible = true;
					} else { 
						foreach (string parentReturnType in parentClassMethodVariantReturnTypes) {
							if (InheritanceResolvers.Types.IsTypeInheratedFrom(childReturnType, parentReturnType)) {
								childReturnTypeCompatible = true;
								break;
							}
						}
					}
					if (childReturnTypeCompatible)
						compatibleTypesCount += 1;
				}
				returnTypesCompatible = (
					compatibleTypesCount == currentClassMethodVariantReturnTypes.Count
				);
			}
			return returnTypesCompatible;
		}
		/**
		 * Check method params compatibility - child method variant is compatible 
		 * with parent method variant if all child params types are compatible 
		 * with parent method params types:
		 */
		protected bool isParamsTypesCompatible (
			List<Param> currentClassMethodVariantParams,
			List<Param> parentClassMethodVariantParams
		) {
			bool paramsAreCompatible = true;
			Param childParam;
			Param parentParam;
			for (int childParamIndex = 0; childParamIndex < currentClassMethodVariantParams.Count; childParamIndex += 1) {
				childParam = currentClassMethodVariantParams[childParamIndex];
				if (childParamIndex >= parentClassMethodVariantParams.Count) {
					if (childParam.Optional || childParam.IsRest) {
						// If child method has another param and it's optional or rest, than it doesn't 
						// matter which type the param is, it OK to continue to check next param:
						continue;
					} else {
						// If child method has another param and it's NOT optional, 
						// child method params are NOT compatible:
						paramsAreCompatible = false;
						break;
					}
				} else {
					parentParam = parentClassMethodVariantParams[childParamIndex];
				}
				// Check params types compatibility:
				if (!this.isParamsTypesCompatible(
					childParam, parentParam
				)) {
					paramsAreCompatible = false;
					break;
				};
				// Params optionality has no matter in inheritance.
			}
			return paramsAreCompatible;
		}
		/**
		 * Param types are compatible if all child class method variant param types
		 * are assignable (compatible) with any of parent class method variant param type.
		 */
		protected bool isParamsTypesCompatible(
			Param currentClassMethodVariantParam,
			Param parentClassMethodVariantParam
		) {
			int childParamTypesCompatibleCount = 0;
			ExtClass currentParamCallbackType;
			ExtClass parentParamCallbackType;
			bool currentParamCallbackTypeArr;
			bool parentParamCallbackTypeArr;
			string[] currentParamTypes = currentClassMethodVariantParam.Types.ToArray<string>();
			string[] parentParamTypes = parentClassMethodVariantParam.Types.ToArray<string>();
			string currentParamType;
			string parentParamType;
			for (int i = 0; i < currentParamTypes.Length; i++) {
				currentParamType = currentParamTypes[i];
				currentParamCallbackTypeArr = currentParamType.EndsWith("[]");
				currentParamCallbackType = currentParamCallbackTypeArr
					? this.processor.Store.GetPossibleCallbackType(
					      currentParamType.Substring(0, currentParamType.Length - 2)
					  )
					: this.processor.Store.GetPossibleCallbackType(currentParamType);
				if (currentParamCallbackType != null) 
					// Current param definition is callback function - check if parent types 
					// contains "Function", if contains, param types are compatible:
					currentParamType = "Function" + (currentParamCallbackTypeArr ? "[]" : "");
				// Current param type definition is simple type string:
				for (int j = 0; j < parentParamTypes.Length; j++) {
					parentParamType = parentParamTypes[j];
					parentParamCallbackTypeArr = parentParamType.EndsWith("[]");
					parentParamCallbackType = parentParamCallbackTypeArr
						? this.processor.Store.GetPossibleCallbackType(
							    parentParamType.Substring(0, parentParamType.Length - 2)
							)
						: this.processor.Store.GetPossibleCallbackType(parentParamType);
					if (parentParamCallbackType != null) 
						// Parent param definition is callback function - change the 
						// definition into simple `Function` type:
						parentParamType = "Function" + (parentParamCallbackTypeArr ? "[]" : "");
					// Current param type definition and parent param type definition are simple type strings:
					if (InheritanceResolvers.Types.IsTypeInheratedFrom(currentParamType, parentParamType)) {
						childParamTypesCompatibleCount += 1;
						break;
					}
				}
			}
			return childParamTypesCompatibleCount == currentClassMethodVariantParam.Types.Count;
		}
		/**
		 * Check params optionality:
		 * 
		 *   Params are compatible if:
		 *   - 1. Child method param is required and parent method param is required
		 *     (and the types are compatible).
		 *     
		 *   - 2. Child method param is optional param and parent method param is also optional
		 *     (and the types are compatible).
		 *     
		 *   - 3. Child method param is rest param and parent method param is also rest param
		 *     (and the types are compatible).
		 *   
		 *   - 4. Child method param is rest param and parent method param is optional  
		 *     (and the types are compatible (without array brackets `[]` from rest param)).
		 *     
		 *   - 5. Child method param is required and parent method param is optional or rest
		 *     (and the types are compatible (without array brackets `[]` from rest param)).
		 *     
		 *   - 6. Child method param is optional or rest and parent method param is required
		 *     (and the types are compatible (without array brackets `[]` from rest param)).
		 */
		/*protected bool isParamsOptionalityCompatible(
			Param currentClassMethodVariantParam,
			Param parentClassMethodVariantParam
		) {
			bool currentClassMethodVariantParamRequired = (
				!currentClassMethodVariantParam.Optional && 
				!currentClassMethodVariantParam.IsRest
			);
			bool parentClassMethodVariantParamRequired = (
				!parentClassMethodVariantParam.Optional && 
				!parentClassMethodVariantParam.IsRest
			);
			return (
				(
					// 1
					currentClassMethodVariantParamRequired &&
					parentClassMethodVariantParamRequired
				) || (
					// 2
					currentClassMethodVariantParam.Optional &&
					parentClassMethodVariantParam.Optional
				) || (
					// 3
					currentClassMethodVariantParam.IsRest &&
					parentClassMethodVariantParam.IsRest
				) || (
					// 4
					currentClassMethodVariantParam.IsRest &&
					parentClassMethodVariantParam.Optional
				) || (
					// 5
					currentClassMethodVariantParamRequired && (
						parentClassMethodVariantParam.Optional ||
						parentClassMethodVariantParam.IsRest
					)
				) || (
					// 6
					(
						currentClassMethodVariantParam.Optional ||
						currentClassMethodVariantParam.IsRest
					) &&
					parentClassMethodVariantParamRequired
				)
			);
		}*/
	}
}
