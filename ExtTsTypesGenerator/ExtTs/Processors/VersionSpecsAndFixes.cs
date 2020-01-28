using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public class VersionSpecsAndFixes {
		public static Dictionary<string, string> DocsUrls = new Dictionary<string, string>() {
			// Old URLs like:
			//				 https://docs.sencha.com/extjs/4.2.6/#!/api/Ext.Base-static-method-addStatics	
			//{ "1.1.0",	"https://docs.sencha.com/extjs/1.1.0/" }, // not supported
			//{ "2.3.0",	"https://docs.sencha.com/extjs/2.3.0/" }, // not supported
			//{ "3.4.0",	"https://docs.sencha.com/extjs/3.4.0/" }, // not supported
			{ "4.0.0",		"https://docs.sencha.com/extjs/4.0.0/" },
			{ "4.0.1",		"https://docs.sencha.com/extjs/4.0.1/" },
			{ "4.0.2",		"https://docs.sencha.com/extjs/4.0.2/" },
			{ "4.0.3",		"https://docs.sencha.com/extjs/4.0.3/" },
			{ "4.0.4",		"https://docs.sencha.com/extjs/4.0.4/" },
			{ "4.0.5",		"https://docs.sencha.com/extjs/4.0.5/" },
			{ "4.0.6",		"https://docs.sencha.com/extjs/4.0.6/" },
			{ "4.0.7",		"https://docs.sencha.com/extjs/4.0.7/" },
			{ "4.1.0",		"https://docs.sencha.com/extjs/4.1.0/" },
			{ "4.1.1",		"https://docs.sencha.com/extjs/4.1.1/" },
			{ "4.1.2",		"https://docs.sencha.com/extjs/4.1.2/" },
			{ "4.1.3",		"https://docs.sencha.com/extjs/4.1.3/" },
			{ "4.2.0",		"https://docs.sencha.com/extjs/4.2.0/" },
			{ "4.2.1",		"https://docs.sencha.com/extjs/4.2.1/" },
			{ "4.2.2",		"https://docs.sencha.com/extjs/4.2.2/" },
			{ "4.2.3",		"https://docs.sencha.com/extjs/4.2.3/" },
			{ "4.2.4",		"https://docs.sencha.com/extjs/4.2.4/" },
			{ "4.2.5",		"https://docs.sencha.com/extjs/4.2.5/" },
			{ "4.2.6",		"https://docs.sencha.com/extjs/4.2.6/" },
			// Newer URLs like:
			//				 https://docs.sencha.com/extjs/5.0.0/api/Ext.Base.html#static-method-addStatics
			{ "5.0.0",		"https://docs.sencha.com/extjs/5.0.0/" },
			{ "5.0.1",		"https://docs.sencha.com/extjs/5.0.1/" },
			{ "5.1.0",		"https://docs.sencha.com/extjs/5.1.0/" },
			{ "5.1.1",		"https://docs.sencha.com/extjs/5.1.1/" },
			{ "5.1.2",		"https://docs.sencha.com/extjs/5.1.2/" },
			{ "5.1.3",		"https://docs.sencha.com/extjs/5.1.3/" },
			{ "5.1.4",		"https://docs.sencha.com/extjs/5.1.4/" },
			// Newest URLs (with toolkit) like:
			//				 https://docs.sencha.com/extjs/6.0.0/classic/Ext.Base.html#static-method-addStatics
			//				 https://docs.sencha.com/extjs/6.0.0/modern/Ext.Base.html#static-method-addStatics
			{ "6.0.0",		"https://docs.sencha.com/extjs/6.0.0/" },
			{ "6.0.1",		"https://docs.sencha.com/extjs/6.0.1/" },
			{ "6.0.2",		"https://docs.sencha.com/extjs/6.0.2/" },
			{ "6.2.0",		"https://docs.sencha.com/extjs/6.2.0/" },
			{ "6.2.1",		"https://docs.sencha.com/extjs/6.2.1/" },
			{ "6.5.0",		"https://docs.sencha.com/extjs/6.5.0/" },
			{ "6.5.1",		"https://docs.sencha.com/extjs/6.5.1/" },
			{ "6.5.2",		"https://docs.sencha.com/extjs/6.5.2/" },
			{ "6.5.3",		"https://docs.sencha.com/extjs/6.5.3/" },
			{ "6.6.0",		"https://docs.sencha.com/extjs/6.6.0/" },
			{ "6.6.0-CE",	"https://docs.sencha.com/extjs/6.6.0-CE/" },
			{ "6.7.0",		"https://docs.sencha.com/extjs/6.7.0/" },
			{ "6.7.0-CE",	"https://docs.sencha.com/extjs/6.7.0-CE/" },
			{ "7.0.0",		"https://docs.sencha.com/extjs/7.0.0/" },
			{ "7.0.0-CE",	"https://docs.sencha.com/extjs/7.0.0-CE/" },
		};


		// Major version supported packages
		public static Dictionary<int, ExtJsPackage[]> SuportedPackages = new Dictionary<int, ExtJsPackage[]>() {
			{ 4, new [] { ExtJsPackage.CORE } },
			{ 5, new [] { ExtJsPackage.AMF, ExtJsPackage.CORE, ExtJsPackage.CHARTS, ExtJsPackage.SOAP } },
			{ 6, new [] { ExtJsPackage.AMF, ExtJsPackage.CORE, ExtJsPackage.GOOGLE, ExtJsPackage.CHARTS, ExtJsPackage.LEGACY, ExtJsPackage.SOAP, ExtJsPackage.UX } },
			{ 7, new [] { ExtJsPackage.AMF, ExtJsPackage.CORE, ExtJsPackage.FROALA_EDITOR, ExtJsPackage.GOOGLE, ExtJsPackage.CHARTS, ExtJsPackage.LEGACY, ExtJsPackage.SOAP, ExtJsPackage.UX } },
		};


		// Major version source paths
		public static Dictionary<int, Dictionary<string, PkgCfg>> SourcesPaths = new Dictionary<int, Dictionary<string, PkgCfg>>() {
			{ 4, new Dictionary<string, PkgCfg>() {
				{ "core",               new PkgCfg {
					Source				= "src/",
					SourceOverrides		= ""
				} }
			} },
			{ 5, new Dictionary<string, PkgCfg>() {
				{ "amf",                new PkgCfg {
					Source				= "packages/sencha-amf/src-ext/",
					SourceOverrides		= ""
				} },
				{ "core",               new PkgCfgAdv {
					Source              = new [] { "packages/sencha-core/src/", "src/" },
					SourceOverrides		= "overrides/",
				} },
				{ "charts",             new PkgCfg {
					Source				= "packages/sencha-charts/src/",
					SourceOverrides		= "packages/sencha-charts/src-ext/",
				} },
				{ "soap",               new PkgCfg {
					Source				= "packages/sencha-soap/src/",
					SourceOverrides		= "",
				} }
			} },
			{ 6, new Dictionary<string, PkgCfg>() {
				{ "amf",                new PkgCfg {
					Source				= "packages/amf/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "",
				} },
				{ "core",               new PkgCfg {
					Source				= "packages/core/src/",
					SourceOverrides		= "packages/core/overrides/",
					Classic				= "classic/classic/src/",
					ClassicOverrides	= "classic/classic/overrides/",
					Modern				= "modern/modern/src/",
					ModernOverrides		= "modern/modern/overrides/"
				} },
				{ "google",             new PkgCfg {
					Optional			= true,	// is presented only in higher six version
					Source				= "packages/google/src/",
					SourceOverrides		= "",
					Classic				= "packages/google/classic/src/",
					Modern				= "packages/google/modern/src/",
				} },
				{ "charts",             new PkgCfg {
					Source				= "packages/charts/src/",
					SourceOverrides		= "",
					Classic				= "packages/charts/classic/src/",
					Modern				= "packages/charts/modern/src/",
				} },
				{ "legacy",             new PkgCfg {
					Source				= "packages/legacy/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "packages/legacy/modern/src/",
				} },
				{ "soap",               new PkgCfg {
					Source				= "packages/soap/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "",
				} },
				{ "ux",					new PkgCfg {
					Source				= "packages/ux/src/",
					SourceOverrides		= "",
					Classic				= "packages/ux/classic/src/",
					Modern				= "",
				} }
			} },
			{ 7, new Dictionary<string, PkgCfg>() {
				{ "amf",                new PkgCfg {
					Source				= "packages/amf/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "",
				} },
				{ "core",               new PkgCfg {
					Source				= "packages/core/src/",
					SourceOverrides		= "packages/core/overrides/",
					Classic				= "classic/classic/src/",
					ClassicOverrides	= "classic/classic/overrides/",
					Modern				= "modern/modern/src/",
					ModernOverrides		= "modern/modern/overrides/",
				} },
				{ "frola-editor",       new PkgCfg {
					Source				= "packages/froala-editor/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "",
				} },
				{ "google",             new PkgCfg {
					Source				= "packages/google/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "packages/google/modern/src/",
				} },
				{ "charts",             new PkgCfg {
					Source				= "packages/charts/src/",
					SourceOverrides		= "",
					Classic				= "packages/charts/classic/src/",
					Modern				= "packages/charts/modern/src/",
				} },
				{ "legacy",             new PkgCfg {
					Source				= "packages/legacy/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "packages/legacy/modern/src/",
				} },
				{ "soap",               new PkgCfg {
					Source				= "packages/soap/src/",
					SourceOverrides		= "",
					Classic				= "",
					Modern				= "",
				} },
				{ "ux",					new PkgCfg {
					Source				= "packages/ux/src/",
					SourceOverrides		= "packages/ux/overrides/",
					Classic				= "packages/ux/classic/src/",
					ClassicOverrides	= "",
					Modern				= "packages/ux/modern/src/",
					ModernOverrides		= "",
				} }
			} },
		};


		// extNumericMajorVersion => "Wrong.full.path.ClassName" => "Correct.full.path.ClassName"
		public static Dictionary<int, Dictionary<string, string>> ClassesFixes = new Dictionary<int, Dictionary<string, string>>() {
			{ 5, new Dictionary<string, string>() {
				{ "HtmlElement",											"HTMLElement"},
				{ "XMLElement",												"Element"},
				{ "TextNode",												"Text"},
				{ "Class",													"Ext.Class"},
				{ "Ext.data.session.Session",								"Ext.data.Session"},
				{ "Ext.grid.filter.Filter",									"Ext.grid.filters.filter.Base"},
				{ "Ext.grid.filters.filter.Filter",							"Ext.grid.filters.filter.Base"},
			} },
			{ 6, new Dictionary<string, string>() {
				// Ext.drag.Constraint
				{ "HTMLELement",											"HTMLElement"},
				// Ext.tip.ToolTip.showOnTap
				{ "Boollean",												"boolean"},
				// Ext.amf.data.XmlDecoder
				{ "XMLElement",												"Element"},
				// Ext.dom.Helper.insertHtml
				{ "TextNode",												"Text"},
				// Ext.data.Connection.abort
				{ "Ext.ajax.Request",										"Ext.data.request.Ajax"},
			} },
			{ 7, new Dictionary<string, string>() {
				// Ext.drag.Constraint
				{ "HTMLELement",											"HTMLElement"},
				// Ext.tip.ToolTip.showOnTap
				{ "Boollean",												"boolean"},
				// Ext.amf.data.XmlDecoder
				{ "XMLElement",												"Element"},
				// Ext.dom.Helper.insertHtml
				{ "TextNode",												"Text"},
				// Ext.data.Connection.abort
				{ "Ext.ajax.Request",										"Ext.data.request.Ajax"},
			} }
		};


		 // extNumericMajorVersion => "[place]Namespace.full.path.ClassName.methodName:paramName" => "raw correct param type definition"
		public static Dictionary<int, Dictionary<string, string>> TypesFixes = new Dictionary<int, Dictionary<string, string>>() {
			{ 5, new Dictionary<string, string>() {
				// 5.0.1:
				{ "[methodParam]Ext.grid.NavigationModel.focusItem.item"			, "Ext.Component" },
			} },
			{ 6, new Dictionary<string, string>() {
				// 6.2.0 classic+modern:
				{ "[methodParam]Ext.grid.plugin.RowWidget.getWidget.view"           , "any" },
				{ "[methodParam]Ext.grid.plugin.RowWidget.getWidget.record"			, "any" },
				{ "[methodReturn]Ext.grid.plugin.RowWidget.getWidget"				, "any" },

				{ "[methodParam]Ext.view.AbstractView.setItemsDraggable.draggable"	, "any" },

				{ "[methodParam]Ext.util.Region.setPosition.x"						, "number" },
				{ "[methodParam]Ext.util.Region.setPosition.y"						, "number" },

				{ "[methodParam]Ext.util.Region.exclude.other"						, "Ext.util.Region" },
				{ "[methodParam]Ext.util.Region.exclude.inside"						, "Ext.util.Region" },

				{ "[methodParam]Ext.util.Point.exclude.other"						, "Ext.util.Region" },
				{ "[methodParam]Ext.util.Point.exclude.inside"						, "Ext.util.Region" },
				
				{ "[methodParam]Ext.drag.Source.beforeDragStart.The"				, "Ext.drag.Info" },

				{ "[methodParam]Ext.data.NodeInterface.copy.session"				, "Ext.data.Session" },
			} },
			{ 7, new Dictionary<string, string>() {
				// 7.0.0 classic:
				{ "[property]Ext.grid.feature.Feature.Element"          									, "Ext.dom.Element" },

				{ "[methodParam]Ext.ComponentManager.doHandleDocumentMouseDown.e"							, "any" },

				{ "[methodParam]Ext.grid.plugin.RowWidget.getWidget.view"									, "any" },
				{ "[methodParam]Ext.grid.plugin.RowWidget.getWidget.record"									, "any" },
				{ "[methodReturn]Ext.grid.plugin.RowWidget.getWidget"										, "any" },

				{ "[methodParam]Ext.util.Region.exclude.other"												, "Ext.util.Region" },
				{ "[methodParamConfigObjectProperty]Ext.util.Region.methodParams.exclude.Options.inside"	, "Ext.util.Region" },

				{ "[methodParam]Ext.util.Point.exclude.other"												, "Ext.util.Region" },
				{ "[methodParamConfigObjectProperty]Ext.util.Point.methodParams.exclude.Options.inside"		, "Ext.util.Region" },
				
				{ "[methodParam]Ext.view.AbstractView.setItemsDraggable.draggable"							, "any" },
				
				{ "[methodParam]Ext.data.NodeInterface.copy.session"										, "Ext.data.Session" },

				{ "[config]Ext.drag.Source.proxy"															, "Ext.drag.proxy.None" },
				{ "[methodParam]Ext.drag.Source.getProxy.proxy"												, "Ext.drag.proxy.None" },
				{ "[methodParam]Ext.drag.Source.setProxy.proxy"												, "Ext.drag.proxy.None" },

				{ "[methodParam]Ext.grid.Location.getUpdatedLocation.targetRowIndex"						, "number" },
			} },
		};
	}
}
