using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	[Flags]
	public enum ExtJsPackage {
		UNKNOWN			= 0,
		AMF				= 1,	// Action Message Format
		CORE			= 2,	// ext-all.js
		FROALA_EDITOR	= 4,	// Froala Editor
		GOOGLE			= 8,	// Google
		CHARTS			= 16,	// Sencha Charts
		LEGACY			= 32,	// Legacy / Deprecated Classes
		SOAP			= 64,	// SOAP Data Support
		UX				= 128,	// User eXtensions
	}
}
