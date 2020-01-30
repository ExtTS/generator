using System;
using System.Collections.Generic;
using ExtTs.SourceJsonTypes.ExtObjects;
using ExtTs.ExtTypes.ExtClasses;
using ExtTs.ExtTypes.Structs;
using ExtTs.ExtTypes.Enums;
using ExtTs.ExtTypes;
using System.Diagnostics;

namespace ExtTs.Processors {
	public partial class Reader {
		protected void readAndAddCfgOrProp(ref ExtClass extClass, ExtObjectMember member, string currentClassName, bool cfgsCompleting) {
			string name = this.sanitizeName(member.Name);
			ParsedTypes types = this.typesParser.Parse(
				cfgsCompleting
					? TypeDefinitionPlace.CONFIGURATION
					: TypeDefinitionPlace.PROPERTY,
				currentClassName,
				member.Name,
				member.Type
			);
			bool ownedByCurrent = member.Owner.Length == currentClassName.Length && member.Owner == currentClassName;
			string rawDocs = String.IsNullOrEmpty(member.Doc) ? "" : member.Doc;
			bool required = member.Required.HasValue
				? member.Required == true
				: rawDocs.Contains("(required)");
			if (cfgsCompleting) {
				this.readAndAddCfg(
					ref extClass, member, types, ownedByCurrent, required
				);
			} else {
				this.readAndAddProp(
					ref extClass, member, types, ownedByCurrent, required
				);
			}
		}
		protected void readAndAddCfg(ref ExtClass extClass, ExtObjectMember member, ParsedTypes types, bool ownedByCurrent, bool required) {
			string name = this.sanitizeName(member.Name);
			string[] docs = this.readJsDocs(member.Doc, JsDocsType.CONFIGURATION, extClass.Name.FullName, name);
			Configuration newCfgItem = new Configuration(
				name, types.CfgOrProp, docs, member.Owner, ownedByCurrent
			);
			newCfgItem.Required = required;
			//newCfgItem.DefaultValue = String.IsNullOrEmpty(member.Default) ? "" : member.Default;
			newCfgItem.DefaultValue = member.Default == "" ? "''" : member.Default;
			newCfgItem.Deprecated = this.readJsDocsDeprecated(
				member.Deprecated, extClass.Name.FullName, name
			);
			extClass.AddMemberConfiguration(newCfgItem);
		}
		protected void readAndAddProp(ref ExtClass extClass, ExtObjectMember member, ParsedTypes types, bool ownedByCurrent, bool required) {
			string name = this.sanitizeName(member.Name);
			string[] docs = this.readJsDocs(member.Doc, JsDocsType.PROPERTY, extClass.Name.FullName, name);
			//if (extClass.Name.FullName == "Ext.Base")
			//	Debugger.Break();
			Property newPropItem = new Property(
				name, types.CfgOrProp, docs, member.Owner, ownedByCurrent
			);
			//newPropItem.DefaultValue = String.IsNullOrEmpty(member.Default) ? "" : member.Default;
			newPropItem.DefaultValue = member.Default == "" ? "''" : member.Default;
			newPropItem.Deprecated = this.readJsDocsDeprecated(
				member.Deprecated, extClass.Name.FullName, name
			);
			newPropItem.AccessModJs = this.readItemAccessModifier(member);
			newPropItem.AccessModTs = newPropItem.AccessModJs;
			newPropItem.IsStatic = (member.Static.HasValue && member.Static.Value == true);
			newPropItem.IsReadOnly = (member.Readonly.HasValue && member.Readonly.Value == true);
			if (!newPropItem.IsReadOnly) {
				string nameUpper = name.ToUpper();
				if (name == nameUpper && types.CfgOrProp.Count == 1)
					newPropItem.IsReadOnly = true; // it's looking like some contant:
			}
			newPropItem.Renderable = extClass.Extends == null || name == "self"; // force render if it is class without any parent
			extClass.AddMemberProperty(newPropItem);
		}
	}
}
