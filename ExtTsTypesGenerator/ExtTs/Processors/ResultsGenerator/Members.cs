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
	public partial class ResultsGenerator {
		// used only in class `ExtGlobalObject` and in interfaces `Ext.base.(Cfg|Params|Statics|Events)`
		protected void generateMemberIndexer (ExtClass extClass, Indexer indexer) {
			if (this.processor.GenerateJsDocs)
				this.writeResultLine("/** @indexer */");
			string line = indexer.IsReadOnly ? "readonly " : "";

			line += "[" + indexer.Name + ": " + String.Join(" | ", indexer.KeyTypes) + "]: " 
				+ this.generateMemberIndexerTypes(indexer) + ";";
			this.writeResultLine(line);
		}
		protected string generateMemberIndexerTypes (Indexer indexer) {
			List<string> items = new List<string>();
			string compatibleComment = "";
			Dictionary<string, ExistenceReason> compatibleTypes;
			string compatibleClassFullName;
			ExtClass compatibleClass;
			Dictionary<string, Member> compatibleIndexers;
			string compatibleTypesDef;
			string indexerType;
			foreach (var item in indexer.Types) {
				indexerType = this.checkBrowserGlobalClass(item.Key);
				compatibleComment = "";
				if (item.Value.Type == ExistenceReasonType.COMPATIBLE_TYPES) {
					compatibleClassFullName = item.Value.CompatibilityReasonClassFullName;
					compatibleClass = this.processor.Store.GetByFullName(compatibleClassFullName);
					if (compatibleClass != null) {
						compatibleIndexers = compatibleClass.Members.Indexers;
						compatibleTypes = (compatibleIndexers[indexer.Name] as Indexer).Types;
						compatibleTypesDef = "["+String.Join("|", compatibleTypes.Keys.ToArray<string>())+"]";
					} else {
						compatibleTypesDef = "";
					}
					if (this.processor.GenerateJsDocs)
						compatibleComment = "/* @compatible " 
							+ compatibleClassFullName + "." + indexer.Name
							+ compatibleTypesDef
							+ " */ ";

				}
				items.Add(compatibleComment + indexerType);
			}
			return String.Join(" | ", items);
		}
		protected void generateMemberDocCommentDeprecated(ref List<string> docLines, Member extClassMember) {
			if (extClassMember.Deprecated == null) return;
			string deprecatedTag = "@deprecated";
			string deprecatedPadd = "".PadLeft(deprecatedTag.Length, ' ');
			if (extClassMember.Deprecated.Length == 0) {
				docLines.Add(deprecatedTag);
			} else {
				foreach (string deprecatedLine in extClassMember.Deprecated) {
					docLines.Add(deprecatedTag + " " + deprecatedLine);
					deprecatedTag = deprecatedPadd;
				}
			}
		}
		protected string checkBrowserGlobalClass (string fullTypeName) {
			if (JavascriptInternals.JsGlobalsAlsoInExtNamespace.Contains(fullTypeName))
				fullTypeName = SpecialsGenerator.GLOBAL_CLASS_BASE
					.Replace("<browserGlobalClassName>", fullTypeName);
			return fullTypeName;
		}
	}
}
