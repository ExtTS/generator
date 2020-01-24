using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.ExtClasses {
	[Serializable]
	public class NameInfo {
		public string FullName;
		public string ClassName;
		public string NamespaceName;
		public string PackagedNamespace;
		public bool IsInModule;
		public NameInfo (string fullName = "") {
			this.FullName = fullName;
			List<string> fullNameExploded = fullName.Split('.').ToList<string>();
			this.IsInModule = fullNameExploded.Count > 1;
			int lastArrIndex;
			this.NamespaceName = "";
			if (this.IsInModule) {
				// Ext.Array | Ext.event.Event | Ext.tree.plugin.TreeViewDragDrop
				lastArrIndex = fullNameExploded.Count - 1;
				// Array | Event | TreeViewDragDrop
				this.ClassName = fullNameExploded[lastArrIndex];
				fullNameExploded.RemoveAt(lastArrIndex);
				// Ext | Ext.event | Ext.tree.plugin
				this.NamespaceName = String.Join(".", fullNameExploded);
			} else {
				// Ext
				this.ClassName = fullName;
			}
		}
	}
}
