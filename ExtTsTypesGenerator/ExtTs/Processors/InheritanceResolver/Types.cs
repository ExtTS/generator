using ExtTs.ExtTypes;
using ExtTs.ExtTypes.ExtClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtTs.Processors.InheritanceResolvers {
	public class Types {
		internal protected static Dictionary<string, List<string>> JsInheritance = new Dictionary<string, List<string>>() {
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Value_properties
			 */
			{ "null"						, new List<string> { "object","any" }},
			{ "undefined"					, new List<string> { "object","any" }},
			{ "void"						, new List<string> { "any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Fundamental_objects
			 */
			{ "object"						, new List<string> { "any" }},
			{ "Object"						, new List<string> { "any" }},
			{ "function"					, new List<string> { "object","any" }},
			{ "Function"					, new List<string> { "object","any" }},
			{ "boolean"						, new List<string> { "any" }},
			{ "Boolean"						, new List<string> { "any" }},
			{ "symbol"						, new List<string> { "any" }},
			{ "Symbol"						, new List<string> { "any" }},
			{ "Error"						, new List<string> { "object","any" }},
			{ "AggregateError"				, new List<string> { "Error","object","any" }},
			{ "EvalError"					, new List<string> { "Error","object","any" }},
			{ "InternalError" 				, new List<string> { "Error","object","any" }},
			{ "RangeError"					, new List<string> { "Error","object","any" }},
			{ "ReferenceError"				, new List<string> { "Error","object","any" }},
			{ "SyntaxError"					, new List<string> { "Error","object","any" }},
			{ "TypeError"					, new List<string> { "Error","object","any" }},
			{ "URIError"					, new List<string> { "Error","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Numbers_and_dates
			 */
			{ "number"						, new List<string> { "any" }},
			{ "Number"						, new List<string> { "any" }},
			{ "Date"						, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Text_processing
			 */
			{ "string"						, new List<string> { "any" }},
			{ "String"						, new List<string> { "any" }},
			{ "RegExp"						, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Indexed_collections
			 */
			{ "Array"						, new List<string> { "object","any" }},
			{ "Array<any>"					, new List<string> { "object","any" }},
			{ "Int8Array"					, new List<string> { "object","any" }},
			{ "Uint8Array"					, new List<string> { "object","any" }},
			{ "Uint8ClampedArray"			, new List<string> { "object","any" }},
			{ "Int16Array"					, new List<string> { "object","any" }},
			{ "Uint16Array"					, new List<string> { "object","any" }},
			{ "Int32Array"					, new List<string> { "object","any" }},
			{ "Uint32Array"					, new List<string> { "object","any" }},
			{ "Float32Array"				, new List<string> { "object","any" }},
			{ "Float64Array"				, new List<string> { "object","any" }},
			{ "BigInt64Array"				, new List<string> { "object","any" }},
			{ "BigUint64Array"				, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Keyed_collections
			 */
			{ "Map"							, new List<string> { "object","any" }},
			{ "Set"							, new List<string> { "object","any" }},
			{ "WeakMap"						, new List<string> { "object","any" }},
			{ "WeakSet"						, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Structured_data
			 */
			{ "ArrayBuffer"					, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "SharedArrayBuffer" 			, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "Atomics" 					, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "DataView"					, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "JSON"						, new List<string> { "object","any" }}, // TODO: not sure about this
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Control_abstraction_objects
			 */
			{ "Promise"						, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "Generator"					, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "GeneratorFunction"			, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "AsyncFunction"				, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "Iterator" 					, new List<string> { "object","any" }}, // TODO: not sure about this
			{ "AsyncIterator"				, new List<string> { "object","any" }}, // TODO: not sure about this
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Reflection
			 */
			{ "Reflect"						, new List<string> { "object","any" }},
			{ "Proxy"						, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects#Other
			 */
			{ "IArguments"					, new List<string> { "object","any" }},
			{ "Arguments"					, new List<string> { "IArguments","object","any" }},
			{ "arguments"					, new List<string> { "IArguments","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/EventTarget
			 */
			{ "EventTarget"					, new List<string> { "object,any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D
			 */
			{ "CanvasRenderingContext2D"	, new List<string> { "object,any" } },
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/XMLDocument
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Document
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Node
			 */
			{ "XMLDocument"					, new List<string> { "Document","Node","EventTarget","object","any" }},
			{ "Document"					, new List<string> { "Node","EventTarget","object","any" }},
			{ "Node"						, new List<string> { "EventTarget","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequestEventTarget
			 */
			{ "XMLHttpRequest"				, new List<string> { "XMLHttpRequestEventTarget","EventTarget","object","any" }},
			{ "XMLHttpRequestEventTarget"	, new List<string> { "EventTarget","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Element
			 */
			{ "HTMLElement"					, new List<string> { "Element","Node","EventTarget","object","any" }},
			//{ "XMLElement"					, new List<string> { "Element","Node","EventTarget","object","any" }},	// Doesn't exist, but used in: Ext.data.reader.Xml.extractData();
			{ "Element"						, new List<string> { "Node","EventTarget","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Text
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Comment
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/ProcessingInstruction
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CharacterData
			 */
			{ "Text"						, new List<string> { "CharacterData","Node","EventTarget","object","any" }},
			{ "Comment"						, new List<string> { "CharacterData","Node","EventTarget","object","any" }},
			{ "ProcessingInstruction"		, new List<string> { "CharacterData","Node","EventTarget","object","any" }},
			{ "CharacterData"				, new List<string> { "Node","EventTarget","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Event
			 */
			{ "Event"						, new List<string> { "object","any" }},
			{ "AnimationEvent"				, new List<string> { "Event","object","any" }},
			{ "AudioProcessingEvent"		, new List<string> { "Event","object","any" }},
			{ "BeforeInputEvent"			, new List<string> { "Event","object","any" }},
			{ "BeforeUnloadEvent"			, new List<string> { "Event","object","any" }},
			{ "BlobEvent"					, new List<string> { "Event","object","any" }},
			{ "ClipboardEvent"				, new List<string> { "Event","object","any" }},
			{ "CloseEvent"					, new List<string> { "Event","object","any" }},
			{ "CompositionEvent"			, new List<string> { "Event","object","any" }},
			{ "CSSFontFaceLoadEvent"		, new List<string> { "Event","object","any" }},
			{ "CustomEvent"					, new List<string> { "Event","object","any" }},
			{ "DeviceLightEvent"			, new List<string> { "Event","object","any" }},
			{ "DeviceMotionEvent"			, new List<string> { "Event","object","any" }},
			{ "DeviceOrientationEvent"		, new List<string> { "Event","object","any" }},
			{ "DeviceProximityEvent"		, new List<string> { "Event","object","any" }},
			{ "DOMTransactionEvent"			, new List<string> { "Event","object","any" }},
			{ "DragEvent"					, new List<string> { "Event","object","any" }},
			{ "EditingBeforeInputEvent"		, new List<string> { "Event","object","any" }},
			{ "ErrorEvent"					, new List<string> { "Event","object","any" }},
			{ "FetchEvent"					, new List<string> { "Event","object","any" }},
			{ "FocusEvent"					, new List<string> { "Event","object","any" }},
			{ "GamepadEvent"				, new List<string> { "Event","object","any" }},
			{ "HashChangeEvent"				, new List<string> { "Event","object","any" }},
			{ "IDBVersionChangeEvent"		, new List<string> { "Event","object","any" }},
			{ "InputEvent"					, new List<string> { "Event","object","any" }},
			{ "KeyboardEvent"				, new List<string> { "Event","object","any" }},
			{ "MediaStreamEvent"			, new List<string> { "Event","object","any" }},
			{ "MessageEvent"				, new List<string> { "Event","object","any" }},
			{ "MouseEvent"					, new List<string> { "Event","object","any" }},
			{ "MutationEvent"				, new List<string> { "Event","object","any" }},
			{ "OfflineAudioCompletionEvent"	, new List<string> { "Event","object","any" }},
			{ "OverconstrainedError"		, new List<string> { "Event","object","any" }},
			{ "PageTransitionEvent"			, new List<string> { "Event","object","any" }},
			{ "PaymentRequestUpdateEvent"	, new List<string> { "Event","object","any" }},
			{ "PointerEvent"				, new List<string> { "Event","object","any" }},
			{ "PopStateEvent"				, new List<string> { "Event","object","any" }},
			{ "ProgressEvent"				, new List<string> { "Event","object","any" }},
			{ "RelatedEvent"				, new List<string> { "Event","object","any" }},
			{ "RTCDataChannelEvent"			, new List<string> { "Event","object","any" }},
			{ "RTCIdentityErrorEvent"		, new List<string> { "Event","object","any" }},
			{ "RTCIdentityEvent"			, new List<string> { "Event","object","any" }},
			{ "RTCPeerConnectionIceEvent"	, new List<string> { "Event","object","any" }},
			{ "SensorEvent"					, new List<string> { "Event","object","any" }},
			{ "StorageEvent"				, new List<string> { "Event","object","any" }},
			{ "SVGEvent"					, new List<string> { "Event","object","any" }},
			{ "SVGZoomEvent"				, new List<string> { "Event","object","any" }},
			{ "TimeEvent"					, new List<string> { "Event","object","any" }},
			{ "TouchEvent"					, new List<string> { "Event","object","any" }},
			{ "TrackEvent"					, new List<string> { "Event","object","any" }},
			{ "TransitionEvent"				, new List<string> { "Event","object","any" }},
			{ "UIEvent"						, new List<string> { "Event","object","any" }},
			{ "UserProximityEvent"			, new List<string> { "Event","object","any" }},
			{ "WebGLContextEvent"			, new List<string> { "Event","object","any" }},
			{ "WheelEvent"					, new List<string> { "Event","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Window
			 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/globalThis
			 */
			{ "Window"						, new List<string> { "globalThis","object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/NodeList
			 */
			{ "NodeList"					, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleRule
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CSSRule
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CSSRuleList
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration
			 */
			{ "CSSStyleSheet"				, new List<string> { "StyleSheet","object","any" }},
			{ "StyleSheet"					, new List<string> { "object","any" }},
			{ "CSSStyleRule"				, new List<string> { "CSSRule","object","any" }},
			{ "CSSRule"						, new List<string> { "object","any" }},
			{ "CSSRuleList"					, new List<string> { "object","any" }},
			{ "CSSStyleDeclaration"			, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/File
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/Blob
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/FileList
			 */
			{ "File"						, new List<string> { "Blob","object","any" }},
			{ "Blob"						, new List<string> { "object","any" }},
			{ "FileList"					, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/DataTransfer
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/CanvasGradient
			 */
			{ "DataTransfer"				, new List<string> { "object","any" }},
			{ "CanvasGradient"				, new List<string> { "object","any" }},
			/**
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/FileSystem
			 * @see https://developer.mozilla.org/en-US/docs/Web/API/FileError
			 */
			{ "FileSystem"					, new List<string> { "object","any" }},
			{ "FileError"					, new List<string> { "Error", "object","any" }},
			/**
			 * @see https://developers.google.com/maps/documentation/javascript/reference/map#Map
			 */
			{ "google.maps.Map"				, new List<string> { "google.maps.MVCObject","object","any" }},
			/**
			 * @see https://developers.google.com/maps/documentation/javascript/reference/coordinates#LatLng
			 */
			{ "google.maps.LatLng"			, new List<string> { "object","any" }},
		};
		protected static Dictionary<string, Types> cache = new Dictionary<string, Types>();
		protected string typeFullName;
		protected List<string> inheritedFromTypes;
		protected internal Types (string typeFullName) {
			ExtClass extClass;
			List<string> parentTypes;
			this.typeFullName = typeFullName;
			List<string> typeFullNameExploded;
			int lastExplodedIndex;
			string statPropName;
			string typeFullNameStatProp;
			Property staticProp;
			if (Types.JsInheritance.ContainsKey(typeFullName)) {
				this.inheritedFromTypes = Types.JsInheritance[typeFullName];
			} else {
				Processors.Store store = ExtTs.Processor.GetInstance().Store;
				if (
					!store.ExtClassesMap.ContainsKey(typeFullName) &&
					store.ClassesFixes.ContainsKey(typeFullName)
				) {
					typeFullName = store.ClassesFixes[typeFullName];
				}
				if (store.ExtClassesMap.ContainsKey(typeFullName)) {
					// type exists between completed Ext classes:
					extClass = store.GetByFullName(typeFullName);
					parentTypes = this.completeParentTypes(store, extClass);
					Types.JsInheritance[typeFullName] = parentTypes;
					this.inheritedFromTypes = parentTypes;
				} else if (typeFullName.StartsWith("'") && typeFullName.EndsWith("'")) {
					// Type is string value definition:
					parentTypes = new List<string>() { "string", "any" };
					Types.JsInheritance[typeFullName] = parentTypes;
					this.inheritedFromTypes = parentTypes;
				} else if (Regex.Match(typeFullName, @"^([0-9\+\-\.]+)$").Success) {
					// Type is string value definition:
					parentTypes = new List<string>() { "number", "any" };
					Types.JsInheritance[typeFullName] = parentTypes;
					this.inheritedFromTypes = parentTypes;
				} else {
					// Try to remove the last item and try to search static property:
					// example: Ext.dom.Element.VISIBILITY
					typeFullNameExploded = typeFullName.Split('.').ToList();
					lastExplodedIndex = typeFullNameExploded.Count - 1;
					statPropName = typeFullNameExploded[lastExplodedIndex];
					typeFullNameExploded.RemoveAt(lastExplodedIndex);
					typeFullNameStatProp = String.Join(".", typeFullNameExploded);
					if (store.ExtClassesMap.ContainsKey(typeFullNameStatProp)) {
						extClass = store.GetByFullName(typeFullNameStatProp);
						if (
							extClass != null && 
							extClass.Members.PropertiesStatic.ContainsKey(statPropName)
						) {
							staticProp = extClass.Members.PropertiesStatic[statPropName] as Property;
							parentTypes = new List<string>(
								staticProp.Types.Keys.ToList<string>()	
							);
							Types.JsInheritance[typeFullName] = parentTypes;
							this.inheritedFromTypes = parentTypes;
						} else {
							this.addParentTypesForUnknownType(typeFullName);
						}
					} else {
						this.addParentTypesForUnknownType(typeFullName);
					}
				}
			}
		}
		protected void addParentTypesForUnknownType (string typeFullName) {
			// we have no info about unknown class type, 
			// so parent class type could be probably only object or any:
			this.inheritedFromTypes = new List<string>() { "object", "any" };
		}
		protected List<string> completeParentTypes (Store store, ExtClass currentExtClass) {
			List<string> result = new List<string>();
			string parentClassName;
			int parentClassIndex;
			while (true) {
				if (currentExtClass.Extends == null)
					break;
				parentClassName = currentExtClass.Extends.FullName;
				result.Add(parentClassName);
				if (!store.ExtClassesMap.ContainsKey(parentClassName))
					break;
				parentClassIndex = store.ExtClassesMap[parentClassName];
				currentExtClass = store.ExtAllClasses[parentClassIndex];
			}
			result.AddRange(new string[] { "object", "any" });
			return result;
		}
		protected static int getTypeArrayLevel (string typeFullName) {
			int arrayTypePos = typeFullName.IndexOf("[]");
			if (arrayTypePos == -1) return 0;
			return (int)((int)(typeFullName.Length - arrayTypePos) / 2);
		}
		protected static bool isNonArrayTypeInheratedFrom(string checkTypeFullName, string parentTypeFullName) {
			if (checkTypeFullName == "any")
				return true;
			if (!Types.cache.ContainsKey(checkTypeFullName)) 
				Types.cache[checkTypeFullName] = new Types(checkTypeFullName);
			Types instance = Types.cache[checkTypeFullName];
			return instance.inheritedFromTypes.Contains(parentTypeFullName);
		}
		public static bool IsBrowserInternalType (string rawJsType) {
			return Types.JsInheritance.ContainsKey(rawJsType);
		}
		public static bool IsTypeInheratedFrom (string checkTypeFullName, string parentTypeFullName) {
			if (
				checkTypeFullName.Length == parentTypeFullName.Length &&
				checkTypeFullName == parentTypeFullName
			) return true;
			int checkTypeArrayLevel = Types.getTypeArrayLevel(checkTypeFullName);
			int parentTypeArrayLevel = Types.getTypeArrayLevel(parentTypeFullName);
			int checkTypeBracketsLength = checkTypeArrayLevel * 2;
			int parentTypeBracketsLength = parentTypeArrayLevel * 2;
			/**
			 * FALSE
			 * checkTypeFullName	= Date[]	1
			 * parentTypeFullName	= Date[][]	2
			 * 
			 * TRUE
			 * checkTypeFullName	= Date[][]	2
			 * parentTypeFullName	= any[]		1
			 */
			if (checkTypeArrayLevel == parentTypeArrayLevel) {
				/**
				 * Remove the same amout of `[]` brackets from both types:
				 * 
				 * 
				 * TRUE results:
				 *	checkTypeFullName	= Date			0
				 *	parentTypeFullName	= Date			0
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= Date			0
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= Date[]		1
				 * 
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= object[]		1
				 * 
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= any[]			1
				 * 
				 * 
				 * FALSE results:
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= string[]		1
				 */
				return Types.isNonArrayTypeInheratedFrom(
					checkTypeFullName.Substring(0, checkTypeFullName.Length - checkTypeBracketsLength), 
					parentTypeFullName.Substring(0, parentTypeFullName.Length - parentTypeBracketsLength)
				);

			} else if (checkTypeArrayLevel < parentTypeArrayLevel) {
				/**
				 * Remove `[]` brackets from `checkTypeFullName` and check, 
				 * if the child type is type `any`. If child is not
				 * the type `any`, types are not compatible.
				 * 
				 * 
				 * TRUE results:
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= Date[]		1
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= object[]		1
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= any[]			1
				 *	
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= any[][]		2
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= Date[][]		2
				 *	
				 *	checkTypeFullName	= any			0
				 *	parentTypeFullName	= object[][]	2
				 *	
				 *	
				 *	checkTypeFullName	= any[]			1
				 *	parentTypeFullName	= Date[][]		2
				 *	
				 *	checkTypeFullName	= any[]			1
				 *	parentTypeFullName	= object[][]	2
				 *	
				 *	checkTypeFullName	= any[]			1
				 *	parentTypeFullName	= any[][]		2
				 *	
				 *	
				 * FALSE results:
				 *	checkTypeFullName	= Date			0
				 *	parentTypeFullName	= Date[]		1
				 *	
				 *	checkTypeFullName	= Date			0
				 *	parentTypeFullName	= object[]		1
				 *	
				 *	checkTypeFullName	= Date			0
				 *	parentTypeFullName	= any[]			1
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= Date[][]		2
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= object[][]	2
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= any[][]		2
				 *	
				 *	
				 *	checkTypeFullName	= object		1
				 *	parentTypeFullName	= object[]		2
				 *	
				 *	checkTypeFullName	= object[]		1
				 *	parentTypeFullName	= object[][]	2
				 */
				return checkTypeFullName.Substring(0, checkTypeFullName.Length - checkTypeBracketsLength) == "any";

			} else /*if (checkTypeArrayLevel > parentTypeArrayLevel)*/ {
				/**
				 * If the child type has more array levels, parent and child types
				 * are compatible only if parent type without `[]` brackets is 
				 * type `object` or `any`. If not, types are not compatible.
				 * 
				 * TRUE results:
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= object[]		1
				 *	
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= any[]			1
				 *	
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= object		0
				 *	
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= any			0
				 *	
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= object		0
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= any			0
				 *	
				 *	
				 * TRUE results:
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= Date[]		1
				 *	
				 *	checkTypeFullName	= Date[][]		2
				 *	parentTypeFullName	= Date			0
				 *	
				 *	
				 *	checkTypeFullName	= Date[]		1
				 *	parentTypeFullName	= Date			0
				 */
				parentTypeFullName = parentTypeFullName.Substring(
					0, parentTypeFullName.Length - parentTypeBracketsLength
				);
				return parentTypeFullName == "any" || parentTypeFullName == "object";
			}
		}
	}
}
