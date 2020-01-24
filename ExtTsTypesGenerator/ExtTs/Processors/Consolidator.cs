using ExtTs.ExtTypes;
using ExtTs.ExtTypes.ExtClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExtTs.Processors {
	public delegate void consolidateProgressHandler(int processedClassCount, string processedClass);
	public delegate void orderProgressHandler(double percentage, int packageIndex);
	public class Consolidator {
		protected static List<string> otherNamespaceNames = new List<string>() {
			"other",
			//"remain", "further", "extra", "additional", "others", "another", "anothers", "remaining", "extras", "additionals"
		};
		protected static string[] additionalNamespaces = new string[] {
			Reader.NS_EVENTS_PARAMS,
			Reader.NS_METHOD_STATIC_PARAMS,
			Reader.NS_METHOD_PARAMS,
			Reader.NS_METHOD_STATIC_CALLBACK_PARAMS,
			Reader.NS_METHOD_CALLBACK_PARAMS,
		};
		protected static int otherNsCountMin = 20;
		protected static int otherNsCountMax = 50;
		protected consolidateProgressHandler progressHandler;
		protected orderProgressHandler orderProgressHandler;
		protected Processor processor;

		protected internal Consolidator(Processor processor) {
			this.processor = processor;
		}
		protected internal bool ConsolidateParentsCountsOrder(consolidateProgressHandler progressHandler) {
			this.progressHandler = progressHandler;
			// dct key: parents counts, dct value: (dct key: modules parts count, dct value: (dct with class names as keys and class indexes as values))
			Dictionary<int, Dictionary<int, Dictionary<string, int>>> orderStore = new Dictionary<int, Dictionary<int, Dictionary<string, int>>>();
			ExtClass extClass;
			int processedClassesCount = 0;
			for (int i = 0; i < this.processor.Store.ExtAllClasses.Count; i++) {
				extClass = this.processor.Store.ExtAllClasses[i];
				// check if class is not only the method params function callback class object,
				// because those classes are rendered directly, not as types:
				if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CALLBACK) {
					processedClassesCount += 1;
					this.progressHandler.Invoke(processedClassesCount, extClass.Name.FullName);
					continue;
				}
				this.consolidateParentsCountsOrderForClass(
					ref orderStore, ref extClass
				);
				processedClassesCount += 1;
				this.progressHandler.Invoke(processedClassesCount, extClass.Name.FullName);
			}
			this.consolidateParentsCountsOrderSortCollections(
				orderStore, processedClassesCount
			);
			return true;
		}
		protected void consolidateParentsCountsOrderForClass (
			ref Dictionary<int, Dictionary<int, Dictionary<string, int>>> orderStore,
			ref ExtClass extClass
		) {
			Dictionary<string, int> extClassesMap = this.processor.Store.ExtClassesMap;
			List<ExtClass> extAllClasses = this.processor.Store.ExtAllClasses;
			int parentsCount = 0;
			int modulePartsCount = 0;
			Dictionary<int, Dictionary<string, int>> parentsCountsRecord;
			Dictionary<string, int> classesInModuleRecord;
			int classMapIndex;
			ExtClass recursiveClassItem;
			// detect how many parent classes this class have:
			recursiveClassItem = extClass;
			while (true) {
				if (recursiveClassItem.Extends == null)
					break;
				parentsCount += 1;
				if (!extClassesMap.ContainsKey(recursiveClassItem.Extends.FullName)) {
					// class is extended from some unknown class type, 
					// so it's not possible to determinate more parent classes:
					break;
				}
				classMapIndex = extClassesMap[recursiveClassItem.Extends.FullName];
				recursiveClassItem = extAllClasses[classMapIndex];
				extClass.Parents.Add(recursiveClassItem);
			}
			// detect how complicated is module name:
			if (extClass.Name.IsInModule) 
				modulePartsCount = extClass.Name.NamespaceName.Split('.').Length;
			// prepare order store place:
			if (!orderStore.ContainsKey(parentsCount))
				orderStore.Add(parentsCount, new Dictionary<int, Dictionary<string, int>>());
			parentsCountsRecord = orderStore[parentsCount];
			if (!parentsCountsRecord.ContainsKey(modulePartsCount))
				parentsCountsRecord.Add(modulePartsCount, new Dictionary<string, int>());
			classesInModuleRecord = parentsCountsRecord[modulePartsCount];
			classesInModuleRecord.Add(
				extClass.Name.FullName, 
				extClassesMap[extClass.Name.FullName]
			);
		}
		protected void consolidateParentsCountsOrderSortCollections (
			Dictionary<int, Dictionary<int, Dictionary<string, int>>> orderStore,
			int processedClassesCount
		) {
			Dictionary<int, Dictionary<string, int>> parentsCountsRecord;
			int parentsCount;
			// order all records in order store by module parts count ascendently:
			this.progressHandler.Invoke(
				processedClassesCount, "Ordering all classes in order sore by module name complexity."
			);
			int[] orderStoreKeys = orderStore.Keys.ToArray<int>();
			for (int i = 0; i < orderStoreKeys.Length; i++) {
				parentsCount = orderStoreKeys[i];
				parentsCountsRecord = orderStore[parentsCount];
				// order by module name complexity ascendently:
				orderStore[parentsCount] = (
					from item1 in parentsCountsRecord
					orderby item1.Key ascending
					select item1
				).ToDictionary<KeyValuePair<int, Dictionary<string, int>>, int, Dictionary<string, int>>(
					item => item.Key,
					item => item.Value
				);
			}
			// order order store by parents counts ascendently
			this.progressHandler.Invoke(
				processedClassesCount, "Ordering all classes in order sore by parents counts."
			);
			this.processor.Store.ExtClassesParentsCounts = (
				from item2 in orderStore
				orderby item2.Key ascending
				select item2
			).ToDictionary<KeyValuePair<int, Dictionary<int, Dictionary<string, int>>>, int, Dictionary<int, Dictionary<string, int>>>(
				item => item.Key,
				item => item.Value
			);
		}

		protected internal bool OptimalizeNamespacesIntoGroups(consolidateProgressHandler progressHandler) {
			this.progressHandler = progressHandler;
			int extClassesIndex = 0;
			ExtJsPackage package;
			Dictionary<ExtJsPackage, List<Dictionary<string, int>>> allPackagesBaseNamespaces = new Dictionary<ExtJsPackage, List<Dictionary<string, int>>>();
			List<Dictionary<string, int>> packageBaseNamespaces = new List<Dictionary<string, int>>();
			ExtClass extClass;
			for (int i = 0; i < this.processor.Store.ExtAllClasses.Count; i++) {
				extClass = this.processor.Store.ExtAllClasses[i];
				// Check if class is not only the method params function callback class object,
				// because those classes are rendered directly, not as types:
				if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CALLBACK) {
					extClassesIndex += 1;
					this.progressHandler.Invoke(extClassesIndex, extClass.Name.FullName);
					continue;
				}
				package = extClass.Package;
				if (!allPackagesBaseNamespaces.ContainsKey(package))
					allPackagesBaseNamespaces.Add(package, new List<Dictionary<string, int>>());
				packageBaseNamespaces = allPackagesBaseNamespaces[package];
				this.consolidateClassesPackagedNamespaces(extClass, ref packageBaseNamespaces);
				extClassesIndex += 1;
				this.progressHandler.Invoke(extClassesIndex, extClass.Name.FullName);
			}
			// Consolidate namespaces with lowest class counts into small packages:
			foreach (var item in allPackagesBaseNamespaces)
				this.processor.Store.ExtBaseNs.Add(
					item.Key,
					/*this.consolidateClassNamespacesNotOptimalized(
						item.Value
					)*/
					this.consolidateClassesPackagedNamespacesOptimalized(
						item.Value
					)
				);
			return true;
		}
		protected void consolidateClassesPackagedNamespaces(ExtClass extClass, ref List<Dictionary<string, int>> allBaseNamespaces) {
			string packagedNamespace = extClass.Name.PackagedNamespace;
			List<string> packageNsExploded = extClass.Name.PackagedNamespace.Split('.').ToList<string>();
			int packageNsIndex = packageNsExploded.Count;
			if (packageNsIndex + 1 > allBaseNamespaces.Count) 
				for (int i = allBaseNamespaces.Count; i < packageNsIndex + 1; i++) 
					allBaseNamespaces.Add(new Dictionary<string, int>());
			Dictionary<string, int> packagedNamespaces = allBaseNamespaces[packageNsIndex];
			if (packagedNamespaces.ContainsKey(packagedNamespace)) {
				packagedNamespaces[packagedNamespace] += 1;
			} else {
				packagedNamespaces.Add(packagedNamespace, 1);
			}
		}
		protected List<List<string>> consolidateClassesPackagedNamespacesOptimalized (List<Dictionary<string, int>> allBaseNamespaces) {
			List<List<string>> result = new List<List<string>>();
			List<Dictionary<string, int>> optimalizedNamespaces = new List<Dictionary<string, int>>();
			Dictionary<string, int> optimalizedNsInLevel;
			try {
				// level 0 with and empty string and base level classes counts:
				optimalizedNamespaces.Add(allBaseNamespaces[0]);
				if (allBaseNamespaces.Count > 1) {
					// level 1 with single string namespace levels:
					optimalizedNsInLevel = this.getSecondNsLevelOptimalized(
						allBaseNamespaces[1]
					);
					if (optimalizedNsInLevel.Count > 0) { 
						optimalizedNamespaces.Add(optimalizedNsInLevel);
					} else {
						optimalizedNamespaces.Add(allBaseNamespaces[1]);
					}
				}
				// all next levels - try to group by penultimate namespace part
				if (allBaseNamespaces.Count > 2) {
					for (int i = 2; i < allBaseNamespaces.Count; i++) {
						optimalizedNsInLevel = this.getLastButOneNsLevelOptimalized(
							allBaseNamespaces[i]
						);
						if (optimalizedNsInLevel.Count > 0) { 
							optimalizedNamespaces.Add(optimalizedNsInLevel);
						} else {
							optimalizedNamespaces.Add(allBaseNamespaces[i]);
						}
					}
				}
			} catch (Exception ex) {
				this.processor.Exceptions.Add(ex);
				optimalizedNamespaces = allBaseNamespaces;
			}
			for (int i = 0; i < optimalizedNamespaces.Count; i++) 
				result.Add(optimalizedNamespaces[i].Keys.ToList<string>());
			return result;
		}
		
		
		protected Dictionary<string, int> getSecondNsLevelOptimalized (Dictionary<string, int> secondNsLevel) {
			Dictionary<string, int> result = new Dictionary<string, int>();
			Dictionary<string, int> secondNsLevelClone = new Dictionary<string, int>(secondNsLevel);
			Dictionary<string, int> secondNsLevelLast = new Dictionary<string, int>(secondNsLevelClone);
			string[] secondNsLevelKeys;
			string secondNsLevelKey;
			int secondNsLevelValue;
			string otherNamespaceName;
			int otherNsCount = 0;
			int otherNsCountLast = 0;
			int otherNsAmountView = 1;
			int groupedNsCount = 0;
			int groupedNsCountLast = 0;
			while (true) {
				secondNsLevelKeys = secondNsLevelClone.Keys.ToArray<string>();
				for (int i = 0; i < secondNsLevelKeys.Length; i++) {
					secondNsLevelKey = secondNsLevelKeys[i];
					secondNsLevelValue = secondNsLevelClone[secondNsLevelKey];
					if (secondNsLevelValue == otherNsAmountView) {
						secondNsLevelClone.Remove(secondNsLevelKey);
						otherNsCount += secondNsLevelValue;
						groupedNsCount += 1;
					}
				}
				if (otherNsCount > Consolidator.otherNsCountMax) {
					secondNsLevelClone = new Dictionary<string, int>(secondNsLevelLast);
					otherNsCount = otherNsCountLast;
					groupedNsCount = otherNsCountLast;
					break;
				}
				if (otherNsCount >= Consolidator.otherNsCountMin) 
					break;
				if (secondNsLevelClone.Count == 0)
					break;
				otherNsAmountView += 1;
				groupedNsCountLast = groupedNsCount;
				otherNsCountLast = otherNsCount;
				secondNsLevelLast = new Dictionary<string, int>(secondNsLevelClone);
			}
			if (groupedNsCount < 2) {
				// Only one namespace or no namespace have been optimalized:
				result = secondNsLevelClone;
			} else {
				// Something has been optimalized:
				foreach (var remainingItem in secondNsLevelClone)
					result.Add(remainingItem.Key, remainingItem.Value);
				if (otherNsCount > 0) {
					otherNamespaceName = this.getNonUsedOtherNamespaceName("", result.Keys.ToList<string>());
					result.Add(otherNamespaceName, otherNsCount);
				}
			}
			return result;
		}
		protected Dictionary<string, int> getLastButOneNsLevelOptimalized (Dictionary<string, int> nextNsLevel) {
			Dictionary<string, int> result = new Dictionary<string, int>();
			Dictionary<string, int> optGroupItems;
			// try to complete groups: app.bind, app.route => app etc...
			Dictionary<string, List<string>> nsGroups = new Dictionary<string, List<string>>();
			List<string> nsExploded;
			string groupNs;
			foreach (var nextNsLevelItem in nextNsLevel) {
				nsExploded = nextNsLevelItem.Key.Split('.').ToList<string>();
				nsExploded.RemoveAt(nsExploded.Count - 1); // remove the last one part
				groupNs = String.Join(".", nsExploded);
				if (!nsGroups.ContainsKey(groupNs))
					nsGroups.Add(groupNs, new List<string>());
				nsGroups[groupNs].Add(nextNsLevelItem.Key);
			}
			foreach (var nsGroupItem in nsGroups) {
				optGroupItems = this.getLastButOneNsGroupOptimalized(
					nsGroupItem.Key, nsGroupItem.Value, nextNsLevel
				);
				foreach (var item in optGroupItems) {
					if (result.ContainsKey(item.Key))
						Debugger.Break();
					result.Add(item.Key, item.Value);
				}
			}
			return result;
		}
		protected Dictionary<string, int> getLastButOneNsGroupOptimalized (string groupName, List<string> groupNamespaces, Dictionary<string, int> nextNsLevel) {
			Dictionary<string, int> result = new Dictionary<string, int>();
			List<string> groupNamespacesClone = new List<string>(groupNamespaces);
			List<string> groupNamespacesLast = new List<string>(groupNamespacesClone);
			string[] groupNamespacesKeys;
			string groupNamespace;
			int namespaceValue;
			string otherNamespaceName;
			int otherNsCount = 0;
			int otherNsCountLast = 0;
			int otherNsAmountView = 1;
			int groupedNsCount = 0;
			int groupedNsCountLast = 0;
			if (groupNamespaces.Count < 2) {
				// Group is too small:
				foreach (string remainingGroupNamespace in groupNamespaces) 
					result.Add(remainingGroupNamespace, nextNsLevel[remainingGroupNamespace]);
				return result;
			}
			while (true) {
				groupNamespacesKeys = groupNamespacesClone.ToArray<string>();
				for (int i = 0; i < groupNamespacesKeys.Length; i++) {
					groupNamespace = groupNamespacesKeys[i];
					namespaceValue = nextNsLevel[groupNamespace];
					if (namespaceValue == otherNsAmountView) {
						groupNamespacesClone.Remove(groupNamespace);
						otherNsCount += namespaceValue;
						groupedNsCount += 1;
					}
				}
				if (otherNsCount > Consolidator.otherNsCountMax) {
					groupNamespacesClone = new List<string>(groupNamespacesLast);
					otherNsCount = otherNsCountLast;
					groupedNsCount = otherNsCountLast;
					break;
				}
				if (otherNsCount >= Consolidator.otherNsCountMin)
					break;
				if (groupNamespacesClone.Count == 0)
					break;
				otherNsAmountView += 1;
				groupedNsCountLast = groupedNsCount;
				otherNsCountLast = otherNsCount;
				groupNamespacesLast = new List<string>(groupNamespacesClone);
			}
			if (groupedNsCount < 2) {
				// Only one namespace or no namespace have been optimalized:
				foreach (string remainingGroupNamespace in groupNamespaces) 
					result.Add(remainingGroupNamespace, nextNsLevel[remainingGroupNamespace]);
			} else {
				// Something more has been optimalized:
				foreach (string remainingGroupNamespace in groupNamespacesClone) 
					result.Add(remainingGroupNamespace, nextNsLevel[remainingGroupNamespace]);
				if (otherNsCount > 0) {
					otherNamespaceName = this.getNonUsedOtherNamespaceName(groupName, result.Keys.ToList<string>());
					result.Add(otherNamespaceName, otherNsCount);
				}
			}
			return result;
		}
		protected List<List<string>> consolidateClassNamespacesNotOptimalized(List<Dictionary<string, int>> allBaseNamespaces) {
			List<List<string>> result = new List<List<string>>();
			foreach (Dictionary<string, int> item in allBaseNamespaces)
				result.Add(new List<string>(item.Keys.ToList<string>()));
			return result;
		}
		
		protected internal bool ConsolidateClassesIntoNsGroups(consolidateProgressHandler progressHandler) {
			this.progressHandler = progressHandler;
			int extClassesIndex = 0;
			string optimalizedNamespaceName;
			bool indexesRecordExist;
			Dictionary<string, List<int>> extBaseNsClasses;
			List<int> extClassesIndexes;
			// Consolidate classes into optimalized package namespaces groups:
			ExtClass extClass;
			ExtJsPackage package;
			for (int i = 0; i < this.processor.Store.ExtAllClasses.Count; i++) {
				//if (i == 160) Debugger.Break();
				extClass = this.processor.Store.ExtAllClasses[i];
				// Check if class is not only the method params function callback class object,
				// because those classes are rendered directly, not as types:
				if (extClass.ClassType == ExtTypes.Enums.ClassType.CLASS_METHOD_PARAM_CALLBACK) {
					extClassesIndex += 1;
					this.progressHandler.Invoke(extClassesIndex, extClass.Name.FullName);
					continue;
				}
				package = extClass.Package;
				// Consolidate class index into optimalized namespace group:
				optimalizedNamespaceName = this.getClassOptimalizedNamespaceName(
					extClass, this.processor.Store.ExtBaseNs[package]
				);
				if (!this.processor.Store.ExtBaseNsClasses.ContainsKey(package))
					this.processor.Store.ExtBaseNsClasses.Add(package, new Dictionary<string, List<int>>());
				extBaseNsClasses = this.processor.Store.ExtBaseNsClasses[package];
				indexesRecordExist = extBaseNsClasses.ContainsKey(optimalizedNamespaceName);
				if (indexesRecordExist) {
					extClassesIndexes = extBaseNsClasses[optimalizedNamespaceName];
				} else {
					extClassesIndexes = new List<int>();
				}
				extClassesIndexes.Add(extClassesIndex);
				if (indexesRecordExist) {
					extBaseNsClasses[optimalizedNamespaceName] = extClassesIndexes;
				} else {
					extBaseNsClasses.Add(optimalizedNamespaceName, extClassesIndexes);
				}
				//
				extClassesIndex += 1;
				this.progressHandler.Invoke(extClassesIndex, extClass.Name.FullName);
			}
			return true;
		}
		protected string getClassOptimalizedNamespaceName (ExtClass extClass, List<List<string>> packageOptimalizedNs) {
			string packagedNs = extClass.Name.PackagedNamespace;
			List<string> packagedNsExploded = packagedNs.Split('.').ToList<string>();
			// Select namespaces level to use by namespace parts count:
			int namespacesIndex = packagedNsExploded.Count;
			// Select optimalized namespaces:
			List<string> packageOptimalizedNsInLevel = packageOptimalizedNs[namespacesIndex];
			// Check if optimalized namespaces in level has current class namespace
			// and if not, use the last namespace, because it's always the other namespace
			if (packageOptimalizedNsInLevel.Contains(packagedNs)) 
				return packagedNs;
			// If there is no optimalized amespace for class, try 
			// to remove tha last namespace part and try to find
			// the "name.space.other" namespace for class:
			return this.getNsLevelOtherNamespace(
				packagedNsExploded, packageOptimalizedNsInLevel, extClass.Name.FullName
			);
		}

		protected internal bool OrderClassesInNsGroupsByModuleNames(orderProgressHandler orderProgressHandler) {
			this.orderProgressHandler = orderProgressHandler;
			// Order classes into optimalized namespaces by modules:
			ExtJsPackage[] extBaseNsClassesKeys = this.processor.Store.ExtBaseNsClasses.Keys.ToArray<ExtJsPackage>();
			ExtJsPackage extBaseNsClassesKey;
			string[] extOptimNamespaces;
			string extOptimNamespace;
			List<int> classIndexes;
			Dictionary<string, List<int>> extBaseNsClasses;
			for (int j = 0; j < extBaseNsClassesKeys.Length; j++) {
				extBaseNsClassesKey = extBaseNsClassesKeys[j];
				extBaseNsClasses = this.processor.Store.ExtBaseNsClasses[extBaseNsClassesKey];
				extOptimNamespaces = extBaseNsClasses.Keys.ToArray<string>();
				for (int k = 0; k < extOptimNamespaces.Length; k++) {
					extOptimNamespace = extOptimNamespaces[k];
					classIndexes = extBaseNsClasses[extOptimNamespace];
					this.processor.Store.ExtBaseNsClasses[extBaseNsClassesKey][extOptimNamespace] 
						= this.getClassIndexesOrderedByNamespaces(
							classIndexes
						);
					this.orderProgressHandler.Invoke(
						((double)j / (double)extBaseNsClassesKeys.Length) * 
						((double)k / (double)extOptimNamespaces.Length),
						j
					);
				}
			}
			return true;
		}
		protected List<int> getClassIndexesOrderedByNamespaces (List<int> extClassesIndexes) {
			Dictionary<string, int> store = new Dictionary<string, int>();
			string extClassFullName;
			foreach (int classIndex in extClassesIndexes) {
				extClassFullName = this.processor.Store.ExtAllClasses[classIndex].Name.FullName;
				store.Add(extClassFullName, classIndex);
			}
			return (
				from item in store
				orderby item.Key
				select item.Value
			).ToList<int>();

		}

		
		
		protected string getNonUsedOtherNamespaceName (string nsBaseNamespace, List<string> nsLevelUsedNames) {
			string result = "";
			int otherNamespaceNamesIndex = 0;
			string otherNamespace;
			while (otherNamespaceNamesIndex < Consolidator.otherNamespaceNames.Count) {
				otherNamespace = Consolidator.otherNamespaceNames[otherNamespaceNamesIndex];
				if (nsBaseNamespace.Length > 0)
					otherNamespace = nsBaseNamespace + "." + otherNamespace;
				if (!nsLevelUsedNames.Contains(otherNamespace)) {
					result = otherNamespace;
					break;
				}
				otherNamespaceNamesIndex += 1;
			}
			if (result.Length == 0)
				throw new Exception("There was not possible to optimalize result namespaces.");
			return result;
		}
		protected string getNsLevelOtherNamespace (List<string> namespaceExploded, List<string> packageOptimalizedNsInLevel, string fullClassName) {
			string result = "";
			string classOtherNs;
			string otherNamespace;
			int otherNamespaceNamesIndex = 0;
			List<string> namespaceExplodedClone = new List<string>(namespaceExploded);
			namespaceExplodedClone.RemoveAt(namespaceExplodedClone.Count - 1);
			while (otherNamespaceNamesIndex < Consolidator.otherNamespaceNames.Count) {
				otherNamespace = Consolidator.otherNamespaceNames[otherNamespaceNamesIndex];
				classOtherNs = namespaceExplodedClone.Count > 0
					? String.Join(".", namespaceExplodedClone) + "." + otherNamespace
					: otherNamespace;
				if (packageOptimalizedNsInLevel.Contains(classOtherNs)) {
					result = classOtherNs;
					break;
				}
				otherNamespaceNamesIndex += 1;
			}
			if (result.Length == 0)
				throw new Exception($"There was not possible to find optimalized namespace name for class: `{fullClassName}`.");
			return result;
		}
	}
}
