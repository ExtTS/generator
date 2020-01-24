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
		protected Processor processor;
		public InheritanceResolver(Processor processor) {
			this.processor = processor;
		}
		public void Resolve(ref ExtClass extClass) {
			bool hasParents = extClass.Extends == null;
			if (hasParents) {
				this.setBaseClassPropsAndMethodsNotInherite(ref extClass);
			} else {
				this.resolveMembersExtending(ref extClass);
			}
		}
		protected void resolveMembersExtending (ref ExtClass extClass) {
			this.resolvePropertiesExtending(
				ref extClass, extClass.Members.Properties, true
			);
			this.resolvePropertiesExtending(
				ref extClass, extClass.Members.PropertiesStatic, false
			);
			this.resolveMethodsExtending(
				ref extClass, extClass.Members.Methods, true
			);
			this.resolveMethodsExtending(
				ref extClass, extClass.Members.MethodsStatic, false
			);
		}
		protected void setBaseClassPropsAndMethodsNotInherite (ref ExtClass extClass) {
			this.setBaseClassPropsNotInherite(
				ref extClass, extClass.Members.Properties, true
			);
			this.setBaseClassPropsNotInherite(
				ref extClass, extClass.Members.PropertiesStatic, false
			);
			this.setBaseClassMethodsNotInherite(
				ref extClass, extClass.Members.Methods, true
			);
			this.setBaseClassMethodsNotInherite(
				ref extClass, extClass.Members.MethodsStatic, false
			);
		}
	}
}
