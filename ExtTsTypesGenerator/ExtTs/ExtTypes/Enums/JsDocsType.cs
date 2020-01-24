using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Enums {
	[Serializable]
	public enum JsDocsType {
		UNKNOWN								= 0,
		CLASS								= 1,
		CONFIGURATION						= 2,
		PROPERTY							= 4,
		EVENT								= 8,
		METHOD								= 16,
		EVENT_PARAM							= 32,
		EVENT_PARAM_CONF_OBJ				= 64,
		EVENT_PARAM_CONF_OBJ_PROP			= 128,
		EVENT_PARAM_CALLBACK				= 256,
		EVENT_PARAM_CALLBACK_PARAM			= 512,
		EVENT_PARAM_CALLBACK_RETURN			= 1024,
		METHOD_PARAM						= 2048,
		METHOD_PARAM_CONF_OBJ				= 4086,
		METHOD_PARAM_CONF_OBJ_PROP			= 8172,
		METHOD_PARAM_CALLBACK				= 16344,
		METHOD_PARAM_CALLBACK_PARAM			= 32688,
		METHOD_PARAM_CALLBACK_RETURN		= 65376,
		METHOD_RETURN						= 130752,
		DEPRECATED							= 261504,
	}
}
