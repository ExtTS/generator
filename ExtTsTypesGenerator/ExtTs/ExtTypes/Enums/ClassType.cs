using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.ExtTypes.Enums {
	[Flags]
	[Serializable]
	public enum ClassType {
		UNKNOWN						= 0, 
		CLASS_STANDARD				= 1,	// rendered as class, if it is not singleton
		CLASS_ALIAS					= 2,	// rendered as class, if parent is not singleton
		CLASS_DEFINITIONS			= 4,	// rendered as class, if parent is not singleton
		CLASS_CONSTANT_ALIAS		= 8,	// rendered as interface
		CLASS_METHOD_PARAM_CALLBACK	= 16,	// rendered directly as callback: `(param: object) => void`
		CLASS_METHOD_RETURN_OBJECT	= 32,	// rendered as interface
		CLASS_METHOD_PARAM_CONF_OBJ	= 64,	// rendered as interface
		CLASS_STATICS				= 128,	// rendered as interface
		CLASS_CONFIGS				= 256,	// rendered as interface
		CLASS_EVENTS				= 512,	// rendered as interface
	}
}
