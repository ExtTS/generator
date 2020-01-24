using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes.Structs;
using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class Method:Member {
		public bool IsConstructor;
		public bool IsStatic;
		public bool IsChainable;
		public bool IsTemplate;
		public AccessModifier AccessModJs;
		public AccessModifier AccessModTs;
		public ExistenceReasonType ExistenceReason;
		public List<Param> Params;
		public string ParamsRendered;
		public List<string> ReturnTypes;
		public string[] ReturnDocs;
		public bool Renderable;
		public bool Inherited;
		protected Dictionary<AccessModifier, List<int>> dependentMethods;
		public Method (
			string name = "", 
			List<Param> methodParams = null,
			string[] doc = null, 
			string ownerFullName = "",
			bool ownedByCurrent = false
		) :base(name, doc, ownerFullName, ownedByCurrent) {
			this.Params = methodParams;
			this.ParamsRendered = null;
			this.IsStatic = false;
			this.IsChainable = false;
			this.IsTemplate = false;
			this.IsConstructor = this.Name.Length == 11 && name == "constructor";
			this.AccessModJs = AccessModifier.PUBLIC;
			this.AccessModTs = AccessModifier.PUBLIC;
			this.ExistenceReason = ExistenceReasonType.NATURAL;
			this.ReturnTypes = new List<string>();
			this.ReturnDocs = new string[] { };
			this.Renderable = false;
			this.Inherited = false;
			this.dependentMethods = new Dictionary<AccessModifier, List<int>>();
		}
		public Method Clone () {
			List<Param> paramsClone = new List<Param>();
			foreach (Param param in this.Params)
				paramsClone.Add(param.Clone());
			Method clone = new Method(
				this.Name,
				paramsClone,
				this.Doc,
				(this.Owner != null
					? this.Owner.FullName
					: null),
				false
			);
			clone.AccessModJs = this.AccessModJs;
			clone.AccessModTs = this.AccessModTs;
			clone.ExistenceReason = this.ExistenceReason;
			clone.IsStatic = this.IsStatic;
			clone.IsChainable = this.IsChainable;
			clone.IsTemplate = this.IsTemplate;
			clone.IsConstructor = this.IsConstructor;
			clone.ReturnTypes = new List<string>(this.ReturnTypes);
			clone.ReturnDocs = this.ReturnDocs;
			clone.Renderable = this.Renderable;
			clone.Inherited = this.Inherited;
			return clone;
		}
		public void SetResultAccessModTsWithDependentAccessModMethodsVariants(
			AccessModifier newAccessModTs, 
			bool instancePropsProcessing, 
			List<int> newAccessModsDependentPropsClassIndexes = null
		) {
			// If there is change to higher value, it's necessary also change all props with lower value:
			ExtClass protectedAccessModDependentMethodsClass;
			Dictionary<string, List<Member>> protectedAccessModDependentMethodsVariants;
			List<Member> protectedAccessModDependentMethodVariants;
			Method protectedAccessModDependentMethodVariant;
			List<int> protectedAccessDependentMethods;
			List<int> previousAccessModsDependentMethodsClassIndexes;
			Store store;
			if (newAccessModsDependentPropsClassIndexes != null) {
				if (!this.dependentMethods.ContainsKey(newAccessModTs)) { 
					this.dependentMethods.Add(newAccessModTs, newAccessModsDependentPropsClassIndexes);
				} else {
					previousAccessModsDependentMethodsClassIndexes = this.dependentMethods[newAccessModTs];
					foreach (int newAccessModsDependentMethodsClassIndex in newAccessModsDependentPropsClassIndexes) 
						if (
							newAccessModsDependentMethodsClassIndex != -1 && 
							!previousAccessModsDependentMethodsClassIndexes.Contains(
								newAccessModsDependentMethodsClassIndex
							)
						)
							previousAccessModsDependentMethodsClassIndexes.Add(
								newAccessModsDependentMethodsClassIndex
							);
				}
			}
			if (newAccessModTs == AccessModifier.PUBLIC && this.AccessModTs == AccessModifier.PROTECTED) {
				this.AccessModTs = newAccessModTs;
				if (this.dependentMethods.ContainsKey(AccessModifier.PROTECTED)) {
					protectedAccessDependentMethods = this.dependentMethods[AccessModifier.PROTECTED];
					store = Processor.GetInstance().Store;
					foreach (int protectedAccessModDependentMethodsClassIndex in protectedAccessDependentMethods) {
						if (protectedAccessModDependentMethodsClassIndex == -1) continue;
						protectedAccessModDependentMethodsClass = store.ExtAllClasses[protectedAccessModDependentMethodsClassIndex];
						protectedAccessModDependentMethodsVariants = instancePropsProcessing
							? protectedAccessModDependentMethodsClass.Members.Methods
							: protectedAccessModDependentMethodsClass.Members.MethodsStatic;
						protectedAccessModDependentMethodVariants = protectedAccessModDependentMethodsVariants[this.Name];
						foreach (Member protectedAccessModDependentMethodMember in protectedAccessModDependentMethodVariants) {
							protectedAccessModDependentMethodVariant = protectedAccessModDependentMethodMember as Method;
							protectedAccessModDependentMethodVariant.SetResultAccessModTsWithDependentAccessModMethodsVariants(
								AccessModifier.PUBLIC, instancePropsProcessing
							);
						}
					}
				}
			} else { 
				this.AccessModTs = newAccessModTs;
			}
		}
	}
}
