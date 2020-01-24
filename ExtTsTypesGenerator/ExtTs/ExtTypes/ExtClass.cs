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
	}
}
