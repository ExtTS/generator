using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTs.Processors {
	public struct ExtJsPackages {
		internal static Dictionary<string, ExtJsPackage> Names = new Dictionary<string, ExtJsPackage>() {
			{ "amf"				, ExtJsPackage.AMF			},
			{ "core"			, ExtJsPackage.CORE			},
			{ "froala_editor"	, ExtJsPackage.FROALA_EDITOR},
			{ "google"			, ExtJsPackage.GOOGLE		},
			{ "charts"			, ExtJsPackage.CHARTS		},
			{ "legacy"			, ExtJsPackage.LEGACY		},
			{ "soap"			, ExtJsPackage.SOAP			},
			{ "ux"				, ExtJsPackage.UX			},
		};
	}
}
