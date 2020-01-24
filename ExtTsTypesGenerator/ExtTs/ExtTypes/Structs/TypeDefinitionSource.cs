using ExtTs.ExtTypes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Structs {
	[Serializable]
	public struct TypeDefinitionSource {
		public static Dictionary<TypeDefinitionPlace, string> Names = new Dictionary<TypeDefinitionPlace, string>() {
			{ TypeDefinitionPlace.CONFIGURATION,				"config" },
			{ TypeDefinitionPlace.PROPERTY,						"property" },
			{ TypeDefinitionPlace.METHOD_PARAM,					"methodParam" },
			{ TypeDefinitionPlace.EVENT_PARAM,					"eventParam" },
			{ TypeDefinitionPlace.EVENT_RETURN,					"eventReturn" },
			{ TypeDefinitionPlace.METHOD_RETURN,				"methodReturn" },
			{ TypeDefinitionPlace.EVENT_PARAM_CALLBACK_PARAM,	"eventParamCallbackParam" },
			{ TypeDefinitionPlace.EVENT_PARAM_CALLBACK_RETURN,	"eventParamCallbackReturn" },
			{ TypeDefinitionPlace.EVENT_PARAM_CFGOBJ_PROP,		"eventParamConfigObjectProperty" },
			{ TypeDefinitionPlace.METHOD_PARAM_CALLBACK_PARAM,	"methodParamCallbackParam" },
			{ TypeDefinitionPlace.METHOD_PARAM_CALLBACK_RETURN,	"methodParamCallbackReturn" },
			{ TypeDefinitionPlace.METHOD_PARAM_CFGOBJ_PROP,		"methodParamConfigObjectProperty" },
		};
		public TypeDefinitionPlace Type;
		public string DefinitionFullPath;
		public string MemberOrParamName;
	}
}
