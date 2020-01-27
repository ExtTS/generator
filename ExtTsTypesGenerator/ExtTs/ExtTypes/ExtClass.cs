using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.Processors;
using System;
using System.Collections.Generic;

namespace ExtTs.ExtTypes {
	[Serializable]
	public class ExtClass {
		public NameInfo Name;
		public NameInfo Extends;
		public string[] Docs;
		public ClassType ClassType;
		public List<NameInfo> AlternativeNames;
		public Members Members;
		public bool Singleton;
		public bool Private;
		public string[] Deprecated;
		public List<ExtClass> Parents;
		public bool HasMembers;
		public ExtJsPackage Package;
		public string[] Link; // used only for class types: ClassType.CLASS_METHOD_PARAM_CONF_OBJ;
		//public string SrcJson; // debugging purposes only
		public ExtClass (string fullName = "", string extendsFullName = "", string[] docs = null) {
			this.Name = new NameInfo(fullName);
			if (extendsFullName.Length > 0) { 
				this.Extends = new NameInfo(extendsFullName);
			} else {
				this.Extends = null;
			}
			this.Docs = docs;
			this.ClassType = ClassType.CLASS_STANDARD;
			this.AlternativeNames = new List<NameInfo>();
			this.Members = new Members() {
				//All				= new Dictionary<string, Dictionary<MemberType, List<string>>>(),
				Configations	= new Dictionary<string, Member>(),
				Properties		= new Dictionary<string, Member>(),
				PropertiesStatic= new Dictionary<string, Member>(),
				Methods			= new Dictionary<string, List<Member>>(),
				MethodsStatic	= new Dictionary<string, List<Member>>(),
				Events			= new Dictionary<string, List<Member>>(),
				Indexers		= new Dictionary<string, Member>(),
			};
			this.Singleton = false;
			this.Private = false;
			this.Deprecated = null;
			this.Parents = new List<ExtClass>();
			this.HasMembers = false;
			this.Link = null;
			//this.SrcJson = null;
		}
		public ExtClass (NameInfo nameInfo, NameInfo extendsNameInfo) {
			this.Name = nameInfo;
			this.Extends = extendsNameInfo;
			this.Docs = null;
			this.ClassType = ClassType.CLASS_ALIAS;
			this.AlternativeNames = new List<NameInfo>();
			this.Members = new Members() {
				//All				= new Dictionary<string, Dictionary<MemberType, List<string>>>(),
				Configations	= new Dictionary<string, Member>(),
				Properties		= new Dictionary<string, Member>(),
				PropertiesStatic= new Dictionary<string, Member>(),
				Methods			= new Dictionary<string, List<Member>>(),
				MethodsStatic	= new Dictionary<string, List<Member>>(),
				Events			= new Dictionary<string, List<Member>>(),
				Indexers		= new Dictionary<string, Member>(),
			};
			this.Singleton = false;
			this.Private = false;
			this.Deprecated = null;
			this.Parents = new List<ExtClass>();
			this.HasMembers = false;
			this.Link = null;
			//this.SrcJson = null;
		}
		public void AddMemberIndexer (Indexer member) {
			this.Members.Indexers.Add(member.Name, member);
			this.HasMembers = true;
		}
		public void AddMemberConfiguration (Configuration member) {
			this.Members.Configations.Add(member.Name, member);
			this.HasMembers = true;
		}
		public void AddMemberProperty (Property member) {
			Dictionary<string, Member> propsMemers = member.IsStatic
				? this.Members.PropertiesStatic
				: this.Members.Properties;
			propsMemers.Add(member.Name, member);
			this.HasMembers = true;
		}
		public void AddMemberMethod (Method member) {
			Dictionary<string, List<Member>> methodsMemers = member.IsStatic
				? this.Members.MethodsStatic
				: this.Members.Methods;
			if (methodsMemers.ContainsKey(member.Name)) {
				methodsMemers[member.Name].Add(member);
			} else {
				methodsMemers.Add(member.Name, new List<Member>() {
					member
				});
			}
			this.HasMembers = true;
		}
		public void AddMemberEvent (Event member) {
			if (this.Members.Events.ContainsKey(member.Name)) {
				this.Members.Events[member.Name].Add(member);
			} else {
				this.Members.Events.Add(member.Name, new List<Member>() {
					member
				});
			}
			this.HasMembers = true;
		}
		public void MergeWithMembers(ExtClass otherExtClass) {
			Configuration currentCfg;
			Configuration otherCfg;
			Property currentProp;
			Property otherProp;
			// Cfgs:
			if (otherExtClass.Members.Configations.Count > 0) {
				foreach (var cfgItem in otherExtClass.Members.Configations) {
					if (!this.Members.Configations.ContainsKey(cfgItem.Key)) {
						this.Members.Configations.Add(cfgItem.Key, cfgItem.Value);
					} else {
						currentCfg = this.Members.Configations[cfgItem.Key] as Configuration;
						otherCfg = cfgItem.Value as Configuration;
						foreach (string otherCfgType in otherCfg.Types) {
							if (!currentCfg.Types.Contains(otherCfgType)) {
								currentCfg.Types.Add(
									otherCfgType
								);
							}
						}
					}
				}
			}
			// Props:
			if (otherExtClass.Members.PropertiesStatic.Count > 0) {
				foreach (var propStaticItem in otherExtClass.Members.PropertiesStatic) {
					if (!this.Members.PropertiesStatic.ContainsKey(propStaticItem.Key)) {
						this.Members.PropertiesStatic.Add(propStaticItem.Key, propStaticItem.Value);
					} else {
						currentProp = this.Members.PropertiesStatic[propStaticItem.Key] as Property;
						otherProp = propStaticItem.Value as Property;
						foreach (var otherPropStaticTypeItem in otherProp.Types) {
							if (!currentProp.Types.ContainsKey(otherPropStaticTypeItem.Key)) {
								currentProp.Types.Add(
									otherPropStaticTypeItem.Key,
									otherPropStaticTypeItem.Value
								);
							}
						}
					}
				}
			}
			if (otherExtClass.Members.Properties.Count > 0) {
				foreach (var propItem in otherExtClass.Members.Properties) {
					if (!this.Members.Properties.ContainsKey(propItem.Key)) {
						this.Members.Properties.Add(propItem.Key, propItem.Value);
					} else {
						currentProp = this.Members.Properties[propItem.Key] as Property;
						otherProp = propItem.Value as Property;
						foreach (var otherPropTypeItem in otherProp.Types) {
							if (!currentProp.Types.ContainsKey(otherPropTypeItem.Key)) {
								currentProp.Types.Add(
									otherPropTypeItem.Key,
									otherPropTypeItem.Value
								);
							}
						}
					}
				}
			}
			// Methods:
			if (otherExtClass.Members.MethodsStatic.Count > 0) {
				foreach (var methodStaticItem in otherExtClass.Members.MethodsStatic) {
					if (!this.Members.MethodsStatic.ContainsKey(methodStaticItem.Key)) {
						this.Members.MethodsStatic.Add(methodStaticItem.Key, methodStaticItem.Value);
					} else {
						foreach (Member otherMethodVariant in methodStaticItem.Value) 
							this.Members.MethodsStatic[methodStaticItem.Key].Add(otherMethodVariant);
					}
				}
			}
			if (otherExtClass.Members.Methods.Count > 0) {
				foreach (var methodItem in otherExtClass.Members.Methods) {
					if (!this.Members.Methods.ContainsKey(methodItem.Key)) {
						this.Members.Methods.Add(methodItem.Key, methodItem.Value);
					} else {
						foreach (Member otherMethodVariant in methodItem.Value) 
							this.Members.Methods[methodItem.Key].Add(otherMethodVariant);
					}
				}
			}
			// Events:
			if (otherExtClass.Members.Events.Count > 0) {
				foreach (var methodItem in otherExtClass.Members.Events) {
					if (!this.Members.Events.ContainsKey(methodItem.Key)) {
						this.Members.Events.Add(methodItem.Key, methodItem.Value);
					} else {
						foreach (Member otherMethodVariant in methodItem.Value) 
							this.Members.Events[methodItem.Key].Add(otherMethodVariant);
					}
				}
			}
		}
	}
}
