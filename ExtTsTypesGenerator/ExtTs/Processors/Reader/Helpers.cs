using ExtTs.SourceJsonTypes.ExtObjects;
using ExtTs.ExtTypes.Enums;
using System.Collections.Generic;
using System.Linq;

namespace ExtTs.Processors {
	public partial class Reader {
		protected internal string GetPackagedNamespaceFromFullClassName (string fullClassName) {
			// Ext										=> ""
			// Array									=> ""
			// CanvasRenderingContext2D_				=> ""
			// Ext.Array								=> ""
			// Ext.Ajax									=> ""
			// Ext.dom.Element							=> "dom"
			// Ext.event.gesture.DoubleTap				=> "event.gesture"
			// Ext.dd.DragDropManager.ElementWrapper	=> "dd"
			List<string> exploded = fullClassName.Split(new char[] { '.' }).ToList<string>();
			if (exploded.Count == 1) 
				return ""; // Ext, Array, CanvasRenderingContext2D_ => ""
			// Remove ending class name:
			exploded.RemoveAt(exploded.Count - 1);
			if (exploded.Count == 1)
				return ""; // Ext.Array, Ext.Ajax => ""
			if (exploded[0] == "Ext")
				exploded.RemoveAt(0);
			// Ext.dom.Element							=> "dom"
			// Ext.event.gesture.DoubleTap				=> "event.gesture"
			// Ext.dd.DragDropManager.ElementWrapper	=> "dd.DragDropManager"
			string explodedLastItem;
			string firstLetter;
			while (exploded.Count > 0) {
				explodedLastItem = exploded[exploded.Count - 1];
				firstLetter = explodedLastItem.Substring(0, 1);
				if (firstLetter.ToUpper() == firstLetter) { 
					exploded.RemoveAt(exploded.Count - 1);
				} else {
					break;
				}
			}
			// Ext.dom.Element							=> "dom"
			// Ext.event.gesture.DoubleTap				=> "event.gesture"
			// Ext.dd.DragDropManager.ElementWrapper	=> "dd"
			return string.Join(".", exploded);
		}
		protected string sanitizeName (string rawName) {
			if (rawName == "this")	return "_this";
			if (rawName == "class")	return "_class";
			if (rawName == "new")	return "_new";
			return rawName.Replace('-', '_');
		}
		protected bool isIdentifierNameWrong(string name, ExtObjectMember member) {
			if (name.Length == 0) {
				/*try {
					throw new Exception(
						"Empty identifier in class: `" + member.Owner + "`, member id: " + member.Id
					);
				} catch (Exception e) {
					this._exceptions.Add(e);
				}*/
				return true;
			}
			if (
				name.Contains("!") ||
				name.Contains("%") ||
				name.Contains("=") ||
				name.Contains(".")

			) return true;
			return false;
		}
		protected AccessModifier readItemAccessModifier(ExtObjectMember member) {
			if (member.Private.HasValue && member.Private.Value == true) {
				return AccessModifier.PRIVATE;
			} else if (member.Protected.HasValue && member.Protected.Value == true) {
				return AccessModifier.PROTECTED;
			} else {
				return AccessModifier.PUBLIC;
			}
		}
		protected string extClassMethodReturnObjectPresudoClassName (
			string currentClassName,
			string methodName, 
			bool methodIsStatic
		) {
			if (methodIsStatic) {
				currentClassName += Reader.NS_METHOD_STATIC_RETURN_OBJECT; // ".staticMethodReturns.";
			} else {
				currentClassName += Reader.NS_METHOD_RETURN_OBJECT; // ".methodReturns."
			}
			return currentClassName + methodName;
		}
		protected string extClassMethodConfigObjectPresudoClassName (
			string currentClassName, 
			string eventOrMethodName, 
			bool methodIsStatic, 
			bool eventsCompleting, 
			string paramName,
			bool paramIsObject
		) {
			string paramPascalCase = paramName.Substring(0, 1).ToUpper() + paramName.Substring(1);
			if (eventsCompleting) {
				currentClassName += Reader.NS_EVENTS_PARAMS; // ".eventsParams."
			} else {
				if (paramIsObject) {
					if (methodIsStatic) {
						if (!currentClassName.Contains(Reader.NS_METHOD_STATIC_PARAMS))
							currentClassName += Reader.NS_METHOD_STATIC_PARAMS; // ".staticMethodParams.";
					} else {
						if (!currentClassName.Contains(Reader.NS_METHOD_PARAMS))
							currentClassName += Reader.NS_METHOD_PARAMS; // ".methodParams."
					}
				} else {
					// those namespaces are never rendered, it exists only as virtual callback describtions:
					if (methodIsStatic) {
						if (!currentClassName.Contains(Reader.NS_METHOD_STATIC_CALLBACK_PARAMS))
							currentClassName += Reader.NS_METHOD_STATIC_CALLBACK_PARAMS; // ".staticMethodCallbackParams."
					} else {
						if (!currentClassName.Contains(Reader.NS_METHOD_CALLBACK_PARAMS))
							currentClassName += Reader.NS_METHOD_CALLBACK_PARAMS; // ".methodCallbackParams."
					}
				}
			}
			return currentClassName + (eventOrMethodName.Length > 0 ? eventOrMethodName + "." : "") + paramPascalCase;
			//return currentClassName + (methodIsStatic ? ".static." : ".") + methodName + ".params." + paramPascalCase;
		}
	}
}
