using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.ExtTypes.Structs {
	public struct JavascriptInternals {
		internal static Dictionary<string, bool> JsPrimitives = new Dictionary<string, bool>() {
			{ "string"          , true },
			{ "boolean"         , true },
			{ "number"          , true },
			{ "symbol"          , true },
			{ "undefined"       , true },
			{ "null"			, true },
		};
		// https://www.typescriptlang.org/docs/handbook/declaration-files/do-s-and-don-ts.html
		internal static Dictionary<string, bool> JsPrimitivesTypescriptLower = new Dictionary<string, bool>() {
			{ "string"          , true },
			{ "boolean"         , true },
			{ "number"          , true },
			{ "symbol"          , true },
			{ "undefined"       , true },
			{ "null"			, true },
			{ "object"          , true },
			{ "any"				, true },
		};
		internal static Dictionary<string, bool> JsPrimitivesWithTypescriptInterfaces = new Dictionary<string, bool>() {
			{ "string"          , true },
			{ "boolean"         , true },
			{ "number"          , true },
			{ "symbol"          , true },
			{ "object"          , true },
		};
		internal static List<string> JsGlobalsAlsoInExtNamespace = new List<string>() {
			"Object",
			"Date",
			"Element",
			"Function",
			"Error",
		};
		internal static bool IsJsPrimitive (string rawJsTypeInLowerCase) {
			return JavascriptInternals.JsPrimitives.ContainsKey(rawJsTypeInLowerCase);
		}
		internal static bool IsJsPrimitiveTypescriptLower (string rawJsTypeInLowerCase) {
			return JavascriptInternals.JsPrimitivesTypescriptLower.ContainsKey(rawJsTypeInLowerCase);
		}
		internal static bool IsJsPrimitiveWithTypescriptInterface (string rawJsTypeInLowerCase) {
			return JavascriptInternals.JsPrimitivesWithTypescriptInterfaces.ContainsKey(rawJsTypeInLowerCase);
		}
	}
}
