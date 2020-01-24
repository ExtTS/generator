using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.Structs;
using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Property:Member {
		public Dictionary<string, ExistenceReason> Types;
		public string DefaultValue;
		public AccessModifier AccessModJs;
		public AccessModifier AccessModTs;
		public bool IsStatic;
		public bool IsReadOnly;
		public bool Renderable;
		public bool Inherited;
		public ExtClass SingletonInstance;
		protected Dictionary<AccessModifier, List<int>> dependentProps;
		public Property (
			string name = "", 
			List<string> types = null, 
			string[] doc = null, 
			string ownerFullName = "", 
			bool ownedByCurrent = false
		) :base(name, doc, ownerFullName) {
			this.Types = new Dictionary<string, ExistenceReason>();
			foreach (string type in types)
				this.Types.Add(type, new ExistenceReason(ExistenceReasonType.NATURAL));
			this.DefaultValue = null;
			this.AccessModJs = AccessModifier.PUBLIC;
			this.AccessModTs = AccessModifier.PUBLIC;
			this.IsStatic = false;
			this.IsReadOnly = false;
			this.Renderable = false;
			this.Inherited = false;
			this.SingletonInstance = null;
			this.dependentProps = new Dictionary<AccessModifier, List<int>>();
		}
		public virtual Property Clone () {
			Property clone = new Property(
				this.Name,
				new List<string>(this.Types.Keys.ToList<string>()),
				this.Doc,
				(this.Owner != null
					? this.Owner.FullName
					: null),
				false
			);
			clone.DefaultValue = this.DefaultValue;
			clone.AccessModJs = this.AccessModJs;
			clone.AccessModTs = this.AccessModTs;
			clone.IsStatic = this.IsStatic;
			clone.IsReadOnly = this.IsReadOnly;
			clone.Renderable = this.Renderable;
			clone.Inherited = this.Inherited;
			clone.SingletonInstance = this.SingletonInstance;
			return clone;
		}
		public void SetResultAccessModTsWithDependentAccessModProps(
			AccessModifier newAccessModTs, 
			bool instancePropsProcessing, 
			List<int> newAccessModsDependentPropsClassIndexes = null
		) {
			// If there is change to higher value, it's necessary also change all props with lower value:
			ExtClass protectedAccessModDependentPropClass;
			Dictionary<string, Member> protectedAccessModDependentProps;
			Property protectedAccessModDependentProp;
			List<int> protectedAccessDependentProps;
			List<int> previousAccessModsDependentPropsClassIndexes;
			Store store;
			if (newAccessModsDependentPropsClassIndexes != null) {
				if (!this.dependentProps.ContainsKey(newAccessModTs)) { 
					this.dependentProps.Add(newAccessModTs, newAccessModsDependentPropsClassIndexes);
				} else {
					previousAccessModsDependentPropsClassIndexes = this.dependentProps[newAccessModTs];
					foreach (int newAccessModsDependentPropsClassIndex in newAccessModsDependentPropsClassIndexes) 
						if (
							newAccessModsDependentPropsClassIndex != -1 && 
							!previousAccessModsDependentPropsClassIndexes.Contains(
								newAccessModsDependentPropsClassIndex
							)
						)
							previousAccessModsDependentPropsClassIndexes.Add(
								newAccessModsDependentPropsClassIndex
							);
				}
			}
			if (newAccessModTs == AccessModifier.PUBLIC && this.AccessModTs == AccessModifier.PROTECTED) {
				this.AccessModTs = newAccessModTs;
				if (this.dependentProps.ContainsKey(AccessModifier.PROTECTED)) {
					protectedAccessDependentProps = this.dependentProps[AccessModifier.PROTECTED];
					store = Processor.GetInstance().Store;
					foreach (int protectedAccessModDependentPropClassIndex in protectedAccessDependentProps) {
						if (protectedAccessModDependentPropClassIndex == -1) continue;
						protectedAccessModDependentPropClass = store.ExtAllClasses[protectedAccessModDependentPropClassIndex];
						protectedAccessModDependentProps = instancePropsProcessing
							? protectedAccessModDependentPropClass.Members.Properties
							: protectedAccessModDependentPropClass.Members.PropertiesStatic;
						protectedAccessModDependentProp = protectedAccessModDependentProps[this.Name] as Property;
						protectedAccessModDependentProp.SetResultAccessModTsWithDependentAccessModProps(
							AccessModifier.PUBLIC, instancePropsProcessing
						);
					}
				}
			} else { 
				this.AccessModTs = newAccessModTs;
			}
		}
	}
}
