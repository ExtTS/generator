using ExtTs.ExtTypes.Enums;
using ExtTs.SourceJsonTypes.ExtObjects.ExtObjectMembers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtTs.Processors {
	public partial class Reader {
		public void SetUpJsDocsBaseUrl () {
			if (String.IsNullOrEmpty(this.processor.DocsBaseUrl)) {
				this.jsDocsUrlBase = VersionSpecsAndFixes.DocsUrls[this.processor.VersionStr];
			} else {
				this.jsDocsUrlBase = this.processor.DocsBaseUrl;
			}
			int majorVersion = this.processor.Version.Major;
			if (majorVersion >= 6) {
				// newest url form with toolkit:
				this.jsDocsUrlBase += this.processor.Toolkit + "/";
			} else if (majorVersion >= 5) {
				// newer url form witout toolkit:
				this.jsDocsUrlBase += "api/";
			} else {
				// oldest url form:
				this.jsDocsUrlBase += "#!/api/";
			}
			if (majorVersion >= 5)
				this.jsDocsLinksNewerFormat = true;
		}
		public string GetLinkHrefForClass (string classFullName) {
			// https://docs.sencha.com/extjs/6.0.1/classic/Ext.Base.html
			return (
				this.jsDocsUrlBase + classFullName + (this.jsDocsLinksNewerFormat
					? ".html"
					: "")
			);
		}
		public string GetLinkHrefForClassMethod (string classFullName, bool isStatic, string methodName) {
			// https://docs.sencha.com/extjs/6.0.1/classic/Ext.Base.html#method-callParent
			string result = this.jsDocsUrlBase + classFullName;
			result += this.jsDocsLinksNewerFormat
				? ".html#"
				: "-";
			result += isStatic
				? "static-method-" + methodName
				: "method-" + methodName;
			return result;
		}
		public string GetLinkHrefForClassEvent (string classFullName, string eventName) {
			// https://docs.sencha.com/extjs/6.0.1/classic/Ext.Component.html#event-activate
			string result = this.jsDocsUrlBase + classFullName;
			result += this.jsDocsLinksNewerFormat
				? ".html#event-" + eventName
				: "-event-" + eventName;
			return result;
		}
		protected string[] readJsDocs (string rawDocs, JsDocsType jsDocsType, string classFullName, string memberName = null) {
			List<string> result = new List<string>();
			if (rawDocs == null || !this.processor.GenerateJsDocs)
				return null;
			try {
				result = this.readJsDocsConvertHtmlToTextLines(
					ref rawDocs
				);
			} catch (InvalidJsDocsFormatException ex) {
				ex.JsDocsType = jsDocsType;
				ex.ClassFullName = classFullName;
				if (memberName != null)
					ex.MemberName = memberName;
				this.processor.Exceptions.Add(ex);
			}
			return result != null 
				? result.ToArray<string>()
				: null;
		}
		protected string[] readJsDocsDeprecated(Deprecated deprecatedInfo, string classFullName, string memberName = null) {
			List<string> result = new List<string>();
			if (deprecatedInfo == null || !this.processor.GenerateJsDocs)
				return null;
			if (!String.IsNullOrEmpty(deprecatedInfo.Version))
				result.Add("Since version " + deprecatedInfo.Version + ".");
			if (String.IsNullOrEmpty(deprecatedInfo.Text))
				return result.Count == 0
					? null
					: result.ToArray<string>();
			try {
				string deprecatedInfoText = deprecatedInfo.Text;
				List<string> deprecatedInfoTextLines = this.readJsDocsConvertHtmlToTextLines(
					ref deprecatedInfoText
				);
				if (deprecatedInfoTextLines.Count > 0)
					result.AddRange(deprecatedInfoTextLines);
			} catch (InvalidJsDocsFormatException ex) {
				ex.JsDocsType = JsDocsType.DEPRECATED;
				ex.ClassFullName = classFullName;
				if (memberName != null)
					ex.MemberName = memberName;
				this.processor.Exceptions.Add(ex);
			}
			return result.ToArray<string>();		
		}
		protected List<string> readJsDocsConvertHtmlToTextLines (ref string rawDocs) {
			List<string> result = new List<string>();
			this.readJsDocsFixPossibleEditorFolds(ref rawDocs);
			this.readJsDocsFixPossibleNonHtmlLists(ref rawDocs);
			List<JsDocsSection> sections = this.readJsDocsExplodeSections(ref rawDocs);
			sections = this.readJsDocsFixTextSections(sections);
			sections = this.readJsDocsMergeTheSameSections(sections);
			string sectionValue;
			string[] sectionLines;
			//string sectionLine;
			bool codeSection;
			string indent = "";
			JsDocsSection section;
			JsDocsSection? prevSection = null;
			for (int i = 0, l = sections.Count; i < l; i += 1) {
				section = sections[i];
				codeSection = section.Type == JsDocsSectionType.CODE;
				if (codeSection) {
					sectionValue = section.Value
						.Replace("\r\n", "\n").Replace("\r", "\n");
					sectionValue = sectionValue.Trim(new char[] { '\r', '\n', '\t', '\v' });
				} else {
					sectionValue = this.readJsDocsProcessTextSectionReplacements(ref section);
					sectionValue = sectionValue.Trim(new char[] { ' ', '\r', '\n', '\t', '\v' });
				}
				sectionValue = this.readJsDocsConvertHtmlLinksToJsDocsInlineLinks(
					ref sectionValue, codeSection	
				);
				sectionLines = sectionValue.Split(new char[] {'\n'});
				if (codeSection) {
					/*for (int i = 0; i < sectionLines.Length; i++) {
						sectionLine = sectionLines[i];
						if (sectionLine.Substring(0, 1) == " ")
							sectionLines[i] = sectionLine.Substring(1);
					}*/
					if (prevSection.HasValue && prevSection.Value.EndingListIndent > 0)
						indent = "".PadLeft(prevSection.Value.EndingListIndent, ' ');
					result.Add("");
				} else {
					indent = "";
				}
				for (int j = 0; j < sectionLines.Length; j++)
					result.Add(indent + sectionLines[j]);
				if (codeSection) result.Add("");
				prevSection = section;
			}
			return result;
		}
		protected void readJsDocsFixPossibleEditorFolds(ref string rawDocs) {
			string editorFoldOpenTagBegin = "<editor-fold";
			int editorFoldOpenTagBeginPos = rawDocs.IndexOf(editorFoldOpenTagBegin);
			if (editorFoldOpenTagBeginPos != -1) {
				// normalize <hr> and <hr/> tags into <hr /> tag:
				rawDocs = rawDocs.Replace("<hr>", "<hr />").Replace("<hr/>", "<hr />");
				// search for last <hr /> tag:
				int lastHrTagPos = rawDocs.LastIndexOf("<hr />", editorFoldOpenTagBeginPos);
				if (lastHrTagPos == -1) {
					// remove everything from beginning to <editor-ford> tag definition (including the tag):
					int editorFoldOpenTagEnd = rawDocs.IndexOf(">", editorFoldOpenTagBeginPos);
					if (editorFoldOpenTagEnd != -1) 
						rawDocs = rawDocs.Substring(editorFoldOpenTagEnd - 1);
				} else {
					// remove everything from beginning to <editor-ford> or last <hr /> tag definition:
					rawDocs = rawDocs.Substring(lastHrTagPos + 6);
				}
			}
			string editorFoldCloseTag = "</editor-fold>";
			int editorFoldCloseTagPos = rawDocs.IndexOf(editorFoldCloseTag);
			if (editorFoldCloseTagPos != -1) 
				// remove everything from beginning to editor fold ending definition:
				rawDocs = rawDocs.Substring(editorFoldCloseTagPos + editorFoldCloseTag.Length);
		}
		protected void readJsDocsFixPossibleNonHtmlLists (ref string rawDocs) {
			int index = 0;
			string uncovertedLi = "\n- ";
			string twoNewLines = "\n\n";
			string nextParagraphClose = "</p>";
			int uncovertedLiPos;
			int nextTwoNewLinesPos;
			int nextParagraphPos;
			int ulEndPos;
			string uncovertedUl;
			while (index < rawDocs.Length) { 
				uncovertedLiPos = rawDocs.IndexOf(uncovertedLi, index);
				if (uncovertedLiPos == -1)
					break;
				nextParagraphPos = rawDocs.IndexOf(twoNewLines, uncovertedLiPos + uncovertedLi.Length);
				nextTwoNewLinesPos = rawDocs.IndexOf(nextParagraphClose, uncovertedLiPos + uncovertedLi.Length);
				if (nextParagraphPos != -1 && nextTwoNewLinesPos != -1) {
					ulEndPos = Math.Min(nextParagraphPos, nextTwoNewLinesPos);
				} else if (nextParagraphPos != -1 && nextTwoNewLinesPos == -1) {
					ulEndPos = nextParagraphPos;
				} else if (nextParagraphPos == -1 && nextTwoNewLinesPos != -1) {
					ulEndPos = nextTwoNewLinesPos;
				} else {
					ulEndPos = rawDocs.Length;
				}
				uncovertedUl = rawDocs.Substring(
					uncovertedLiPos, ulEndPos - uncovertedLiPos
				);
				uncovertedUl = "<ul>\n" + uncovertedUl.Replace(uncovertedLi, "</li>\n<li>") + "</li>\n</ul>";
				uncovertedUl = uncovertedUl.Replace("<ul>\n</li>", "<ul>");
				rawDocs = rawDocs.Substring(0, uncovertedLiPos)
					+ uncovertedUl
					+ rawDocs.Substring(ulEndPos);
				index = ulEndPos;
			}
		}
		protected string readJsDocsProcessTextSectionReplacements (ref JsDocsSection section) {
			string sectionValue = section.Value
				.Replace("\r\n", "\n").Replace("\r", "\n")
				.Replace("<h1>", "\n\n# ").Replace("</h1>", "")
				.Replace("<h2>", "\n\n## ").Replace("</h2>", "")
				.Replace("<h3>", "\n\n### ").Replace("</h3>", "")
				.Replace("<h4>", "\n\n#### ").Replace("</h4>", "")
				.Replace("<h5>", "\n\n##### ").Replace("</h5>", "")
				.Replace("<h6>", "\n\n###### ").Replace("</h3>", "")
				.Replace("<hr>", "\n\n---\n\n").Replace("<hr/>", "\n\n---\n\n").Replace("<hr />", "\n\n---\n\n")
				.Replace("<p>", "\n\n").Replace("</p>", "\n")
				.Replace("<i>", "_").Replace("</i>", "_")
				.Replace("<em>", "_").Replace("</em>", "_")
				.Replace("<b>", "**").Replace("</b>", "**")
				.Replace("<strong>", "**").Replace("</strong>", "**")
				.Replace("<s>", "~~").Replace("</s>", "~~")
				.Replace("<del>", "~~").Replace("</del>", "~~")
				.Replace("<br>", "\n").Replace(")<br/>", "\n").Replace("<br />", "\n");
			sectionValue = Regex.Replace(
				sectionValue, 
				@"([^\n])\n(\s*)([\#]+) ([^\n]+)\n", 
				"$1\n\n$2$3 $4\n"
			);
			// Process relative links: {@link #somethingElse}

			// process <ul> sections:
			this.readJsDocsProcessBulletSections(ref section, ref sectionValue);
			// process <ol> sections:
			this.readJsDocsProcessNumberedSections(ref section, ref sectionValue);
			// process <img> tags:
			this.readJsDocsProcessTextImages(ref sectionValue);
			sectionValue = Regex.Replace(
				sectionValue, 
				@"\n\n+", 
				"\n\n"
			);
			return sectionValue;
		}
		protected void readJsDocsProcessBulletSections(ref JsDocsSection section, ref string sectionValue) {
			int index = 0;
			int ulTagOpenPos;
			int ulTagClosePos;
			string ulSection;
			List<string> liItems;
			int liTagOpenPos;
			int liTagClosePos;
			string liSection;
			string[] liSectionLines;
			string indent;
			string restValue;
			while (true) {
				ulTagOpenPos = sectionValue.IndexOf("<ul>", index);
				if (ulTagOpenPos == -1) break;
				ulTagClosePos = sectionValue.IndexOf("</ul>", ulTagOpenPos + 4);
				if (ulTagClosePos == -1) break;
				ulSection = sectionValue.Substring(ulTagOpenPos + 4, ulTagClosePos - (ulTagOpenPos + 4));
				liItems = new List<string>();
				index = 0;
				while (true) {
					liTagOpenPos = ulSection.IndexOf("<li>", index);
					if (liTagOpenPos == -1) break;
					liTagClosePos = ulSection.IndexOf("</li>", liTagOpenPos + 4);
					if (liTagClosePos == -1) break;
					liSection = ulSection.Substring(liTagOpenPos + 4, liTagClosePos - (liTagOpenPos + 4));
					liSectionLines = liSection.Split(
						new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries
					).ToArray<string>();
					indent = "- ";
					for (int i = 0; i < liSectionLines.Length; i++) {
						liSectionLines[i] = indent + liSectionLines[i].Trim();
						indent = "  ";// 2 spaces
					}
					liItems.Add(String.Join("\n", liSectionLines));
					index = liTagClosePos + 5;
				}
				ulSection = "\n\n" + String.Join("\n", liItems) + "\n\n";
				restValue = this.readJsDocsTrimSectionValue(
					sectionValue.Substring(ulTagClosePos + 5), false
				);
				if (restValue.Length == 0) 
					section.EndingListIndent = 2;
				sectionValue = sectionValue.Substring(0, ulTagOpenPos)
					+ ulSection
					+ sectionValue.Substring(ulTagClosePos + 5);
				index = ulTagOpenPos + ulSection.Length;
			}
		}
		protected void readJsDocsProcessNumberedSections(ref JsDocsSection section, ref string sectionValue) {
			int index = 0;
			int olTagOpenPos;
			int olTagClosePos;
			string olSection;
			List<string[]> liItemsArrs;
			List<string> liItems;
			int liTagOpenPos;
			int liTagClosePos;
			string liSection;
			string[] liSectionLines;
			int indentLength;
			string indent;
			string indentBase;
			int counter;
			string restValue;
			while (true) {
				olTagOpenPos = sectionValue.IndexOf("<ol>", index);
				if (olTagOpenPos == -1) break;
				olTagClosePos = sectionValue.IndexOf("</ol>", olTagOpenPos + 4);
				if (olTagClosePos == -1) break;
				
				olSection = sectionValue.Substring(olTagOpenPos + 4, olTagClosePos - (olTagOpenPos + 4));
				liItemsArrs = new List<string[]>();
				index = 0;
				while (true) {
					liTagOpenPos = olSection.IndexOf("<li>", index);
					if (liTagOpenPos == -1) break;
					liTagClosePos = olSection.IndexOf("</li>", liTagOpenPos + 4);
					if (liTagClosePos == -1) break;
					liSection = olSection.Substring(liTagOpenPos + 4, liTagClosePos - (liTagOpenPos + 4));
					liSectionLines = liSection.Split(
						new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries
					).ToArray<string>();
					for (int i = 0; i < liSectionLines.Length; i++) 
						liSectionLines[i] = liSectionLines[i].Trim();
					liItemsArrs.Add(liSectionLines);
					index = liTagClosePos + 5;
				}
				if (liItemsArrs.Count < 10) {
					indentLength = 3;
					indentBase = "   "; // 3 spaces
				} else { 
					indentLength = 4;
					indentBase = "    "; // 4 spaces
				}
				counter = 1;
				liItems = new List<string>();
				foreach (string[] liItem in liItemsArrs) {
					indent = counter.ToString() + "." 
						+ "".PadLeft(indentLength - counter.ToString().Length - 1, ' ');
					for (int j = 0; j < liItem.Length; j++) {
						liItem[j] = indent + liItem[j];
						indent = indentBase;
					}
					liItems.Add(String.Join("\n", liItem));
					counter += 1;
				}

				olSection = "\n\n" + String.Join("\n", liItems) + "\n\n";

				restValue = this.readJsDocsTrimSectionValue(
					sectionValue.Substring(olTagClosePos + 5), false
				);
				if (restValue.Length == 0) 
					section.EndingListIndent = indentLength;

				sectionValue = sectionValue.Substring(0, olTagOpenPos)
					+ olSection
					+ sectionValue.Substring(olTagClosePos + 5);
				index = olTagOpenPos + olSection.Length;
			}
		}
		protected void readJsDocsProcessTextImages(ref string sectionValue) {
			int index = 0;
			int imgTagOpenPos;
			int imgTagClosePos;
			string rawImgTag;
			int imgTagSrcOpenPos;
			int imgTagSrcClosePos;
			int imgTagAltOpenPos;
			int imgTagAltClosePos;
			string imgSrc;
			string imgAlt;
			string jsDocsImage;
			while (true) {
				imgTagOpenPos = sectionValue.IndexOf("<img ", index);
				if (imgTagOpenPos == -1) break;
				imgTagClosePos = sectionValue.IndexOf(">", imgTagOpenPos + 5);
				if (imgTagClosePos == -1) break;
				// <img src="images/Ext.toolbar.Paging/Ext.toolbar.Paging.png" alt="Ext.toolbar.Paging component" width="455" height="235">
				rawImgTag = sectionValue.Substring(imgTagOpenPos, (imgTagClosePos + 1) - imgTagOpenPos);
				imgTagSrcOpenPos = rawImgTag.IndexOf("src=\"");
				if (imgTagSrcOpenPos == -1) {
					index = imgTagClosePos + 1;
					continue;
				};
				imgTagSrcOpenPos += 5;
				imgTagSrcClosePos = rawImgTag.IndexOf("\"", imgTagSrcOpenPos);
				if (imgTagSrcClosePos == -1) {
					index = imgTagClosePos + 1;
					continue;
				}
				imgTagAltOpenPos = rawImgTag.IndexOf("alt=\"");
				if (imgTagAltOpenPos == -1) {
					index = imgTagClosePos + 1;
					continue;
				};
				imgTagAltOpenPos += 5;
				imgTagAltClosePos = rawImgTag.IndexOf("\"", imgTagAltOpenPos);
				if (imgTagAltClosePos == -1) {
					index = imgTagClosePos + 1;
					continue;
				}
				imgSrc = rawImgTag.Substring(imgTagSrcOpenPos, imgTagSrcClosePos - imgTagSrcOpenPos);
				imgAlt = rawImgTag.Substring(imgTagAltOpenPos, imgTagAltClosePos - imgTagAltOpenPos);
				jsDocsImage = this.jsDocsUrlBase + "!["+imgAlt+"]("+imgSrc+")";
				sectionValue = sectionValue.Substring(0, imgTagOpenPos)
					+ jsDocsImage
					+ sectionValue.Substring(imgTagClosePos + 1);
				index = imgTagOpenPos + jsDocsImage.Length;
			}
		}
		protected List<JsDocsSection> readJsDocsExplodeSections (ref string rawText) {
			List<JsDocsSection> sections = new List<JsDocsSection>();
			int index = 0;
			int preTagOpenBeginPos;
			int preTagOpenEndPos;
			int preTagClosePos;
			string sectionValue;
			while (true) {
				preTagOpenBeginPos = rawText.IndexOf("<pre", index);
				// There is no more code section - so all rest value is text section + break:
				if (preTagOpenBeginPos == -1) {
					sectionValue = this.readJsDocsTrimSectionValue(
						rawText.Substring(index), false
					);
					if (sectionValue.Length > 0)
						sections.Add(new JsDocsSection() {
							Value = sectionValue,
							Type  = JsDocsSectionType.TEXT,
							EndingListIndent = 0
						});
					break;
				}
				preTagOpenEndPos = rawText.IndexOf(">", preTagOpenBeginPos + 4);
				// There is wrongly closed opening tag for code section - so all rest value is text section + break:
				if (preTagOpenEndPos == -1) {
					sectionValue = this.readJsDocsTrimSectionValue(
						rawText.Substring(index), false
					);
					if (sectionValue.Length > 0)
						sections.Add(new JsDocsSection() {
							Value = sectionValue,
							Type  = JsDocsSectionType.TEXT,
							EndingListIndent = 0
						});
					break;
				}
				preTagOpenEndPos += 1;
				preTagClosePos = rawText.IndexOf("</pre>", preTagOpenEndPos);
				// Code section is opened but not ended - so all rest value is code section + break:
				if (preTagClosePos == -1) {
					sectionValue = this.readJsDocsTrimSectionValue(
						rawText.Substring(index), true
					);
					if (sectionValue.Length > 0)
						sections.Add(new JsDocsSection() {
							Value = "   "+sectionValue.Replace("\n", "\n   "),
							Type  = JsDocsSectionType.CODE,
							EndingListIndent = 0
						});
					break;
				}
				// There is opened code section somewhere and later closed.
				// So there could be text, code and text.
				sectionValue = this.readJsDocsTrimSectionValue(
					rawText.Substring(index, preTagOpenBeginPos - index), false
				);
				if (sectionValue.Length > 0)
					sections.Add(new JsDocsSection() {
						Value = sectionValue,
						Type  = JsDocsSectionType.TEXT,
						EndingListIndent = 0
					});
				sectionValue = this.readJsDocsTrimSectionValue(
					rawText.Substring(preTagOpenEndPos, preTagClosePos - preTagOpenEndPos), true
				);
				if (sectionValue.Length > 0)
					sections.Add(new JsDocsSection() {
						Value = "   "+sectionValue.Replace("\n", "\n   "),
						Type  = JsDocsSectionType.CODE,
						EndingListIndent = 0
					});
				index = preTagClosePos + 6;
				if (index >= rawText.Length)
					break;
			}
			return sections;
		}
		// fix some text sections still containing code sections
		protected List<JsDocsSection> readJsDocsFixTextSections(List<JsDocsSection> sections) {
			List<JsDocsSection> newSections = new List<JsDocsSection>();
			string sectionValue;
			string sectionLine;
			string[] sectionLines;
			List<string> newSectionLines;
			JsDocsSectionType newSectionType;
			string newSectionValue;
			foreach (JsDocsSection section in sections) {
				if (section.Type == JsDocsSectionType.CODE) {
					newSections.Add(section);
					continue;
				}
				sectionValue = section.Value.Replace("<p>", "\n\n").Replace("</p>", "\n");
				if (!Regex.Match(sectionValue, @"\n[ ]{3}([^\n]+)\n[ ]{3}").Success) {
					newSections.Add(section);
				} else {
					// there is some code section - it's necessary to explode it by line beginning spaces:
					sectionLines = sectionValue.Split(
						new char[] { '\n' }
					).ToArray<string>();
					newSectionType = JsDocsSectionType.TEXT;
					newSectionLines = new List<string>();
					for (int i = 0; i < sectionLines.Length; i++) {
						sectionLine = sectionLines[i];
						if (sectionLine.Length > 3 && sectionLine.Substring(0, 3) == "   ") {
							// line is code section:
							if (newSectionType == JsDocsSectionType.CODE) {
								newSectionLines.Add(sectionLine);
							} else {
								// complete previous lines into text section:
								newSectionValue = this.readJsDocsTrimSectionValue(
									String.Join("\n", newSectionLines), false
								);
								if (newSectionValue.Length > 0)
									newSections.Add(new JsDocsSection() {
										Type = JsDocsSectionType.TEXT,
										Value = newSectionValue,
										EndingListIndent = 0
									});
								// begin new code section completing:
								newSectionType = JsDocsSectionType.CODE;
								newSectionLines = new List<string>() { sectionLine };
							}
						} else {
							// line is text section:
							if (newSectionType == JsDocsSectionType.TEXT) {
								newSectionLines.Add(sectionLine);
							} else {
								// complete previous lines into code section:
								newSectionValue = this.readJsDocsTrimSectionValue(
									" "+String.Join("\n ", newSectionLines), true
								);
								if (newSectionValue.Length > 0)
									newSections.Add(new JsDocsSection() {
										Type = JsDocsSectionType.CODE,
										Value = newSectionValue,
										EndingListIndent = 0
									});
								// begin new text section completing:
								newSectionType = JsDocsSectionType.TEXT;
								newSectionLines = new List<string>() { sectionLine };
							}
						}
					}
					if (newSectionLines.Count > 0) {
						newSectionValue = this.readJsDocsTrimSectionValue(
							" "+String.Join("\n ", newSectionLines),
							newSectionType == JsDocsSectionType.CODE
						);
						if (newSectionValue.Length > 0)
							newSections.Add(new JsDocsSection() {
								Type = newSectionType,
								Value = newSectionValue,
								EndingListIndent = 0
							});
					}
				}
			}
			return newSections;
		}
		protected List<JsDocsSection> readJsDocsMergeTheSameSections (List<JsDocsSection> sections) {
			List<JsDocsSection> newSections = new List<JsDocsSection>();
			if (sections.Count <= 1)
				return sections;
			newSections = new List<JsDocsSection>() { sections[0] };
			JsDocsSection newSection;
			JsDocsSection lastResSection;
			int lastResSecsIndex = 0;
			for (int i = 1; i < sections.Count; i++) {
				newSection = sections[i];
				lastResSection = newSections[lastResSecsIndex];
				if (newSection.Type == lastResSection.Type) {
					// merge this section with last result section
					newSections[lastResSecsIndex] = new JsDocsSection() {
						Type = lastResSection.Type,
						Value = lastResSection.Value += "\n" + newSection.Value,
						EndingListIndent = 0
					};
				} else {
					// add the section
					newSections.Add(newSection);
					lastResSecsIndex += 1;
				}
			}
			return newSections;
		}
		protected string readJsDocsTrimSectionValue (string rawSectionValue, bool codeSection) {
			if (codeSection) {
				int codeTagOpenBeginPos = rawSectionValue.IndexOf("<code");
				if (codeTagOpenBeginPos != -1) {
					int codeTagOpenEndPos = rawSectionValue.IndexOf(">", codeTagOpenBeginPos + 5);
					if (codeTagOpenEndPos != -1) 
						rawSectionValue = rawSectionValue.Substring(codeTagOpenBeginPos + 6);
				}
				int codeTagClosePos = rawSectionValue.LastIndexOf("</code>");
				if (codeTagClosePos != -1) 
					rawSectionValue = rawSectionValue.Substring(0, codeTagClosePos);
				return rawSectionValue.Trim('\r', '\n', '\t', '\v');
			} else {
				rawSectionValue = rawSectionValue
					.Replace("<code>", "`")
					.Replace("</code>", "`");
				return rawSectionValue.Trim(' ', '\r', '\n', '\t', '\v');
			}
		}
		/**
		 * Convert HTML links into Js Docs inline links.
		 * 
		 * HTML links could be in forms:
		 *	<a href="#!/api/Ext.data.Store-event-update" rel="Ext.data.Store-event-update" class="docClass">Store.update</a>
		 *	<a href="#!/api/Ext.data.Store-event-update" rel="Ext.data.Store-event-update" class="docClass">Store.update</a>
		 *	<a href="#!/api/Ext.data.reader.Xml-property-rawData" rel="Ext.data.reader.Xml-property-rawData" class="docClass">rawData</a>
		 *	<a href="#!/api/Ext.event.Event-property-currentTarget" rel="Ext.event.Event-property-currentTarget" class="docClass">currentTarget</a>
		 *	<a href="#!/api/Ext.form.field.Text-property-inputWrap" rel="Ext.form.field.Text-property-inputWrap" class="docClass">inputWrap</a>
		 *	
		 * Result JS Docs links has to be in form:
		 *	[Link visible text](https://docs.sencha.com/extjs/6.0.1/classic/Ext.data.Store.html#event-update)
		 */
		protected string readJsDocsConvertHtmlLinksToJsDocsInlineLinks (ref string rawJsDocs, bool separateLinkTextOnly) {
			int index = 0;
			int aTagOpenPosBegin;
			int aTagClosePos;
			int aTagOpenPosEnd;
			int hrefAttrOpenPos;
			int hrefAttrClosePos;
			string hrefAttrValue;
			int dashPos;
			string linkText;
			string newLinkValue;
			while (true) {
				aTagOpenPosBegin = rawJsDocs.IndexOf("<a ", index);
				if (aTagOpenPosBegin == -1) break;
				aTagClosePos = rawJsDocs.IndexOf("</a>", aTagOpenPosBegin);
				if (aTagClosePos == -1) break;
				aTagOpenPosEnd = rawJsDocs.IndexOf(">", aTagOpenPosBegin);
				if (aTagOpenPosEnd == -1) break;
				aTagOpenPosEnd += 1;
				hrefAttrOpenPos = rawJsDocs.IndexOf("href=\"", aTagOpenPosBegin);
				if (hrefAttrOpenPos == -1) break;
				hrefAttrOpenPos += 6;
				hrefAttrClosePos = rawJsDocs.IndexOf('"', hrefAttrOpenPos);
				if (hrefAttrClosePos == -1) break;
				hrefAttrValue = rawJsDocs.Substring(hrefAttrOpenPos, hrefAttrClosePos - hrefAttrOpenPos);
				if (hrefAttrValue.Length > 6 && hrefAttrValue.Substring(0, 7) == "#!/api/") {
					hrefAttrValue = hrefAttrValue.Substring(7);
					if (this.jsDocsLinksNewerFormat) {
						dashPos = hrefAttrValue.IndexOf("-");
						if (dashPos == -1) {
							hrefAttrValue += ".html";
						} else {
							hrefAttrValue = hrefAttrValue.Substring(0, dashPos)
								+ ".html#"
								+ hrefAttrValue.Substring(dashPos + 1);
						}
					}
					hrefAttrValue = this.jsDocsUrlBase + hrefAttrValue;
				}
				linkText = rawJsDocs.Substring(aTagOpenPosEnd, aTagClosePos - aTagOpenPosEnd);
				if (aTagOpenPosBegin > 0 && rawJsDocs.Substring(aTagOpenPosBegin - 1, 1) == "`") {
					linkText = "`" + linkText;
					aTagOpenPosBegin -= 1;
				}
				if (rawJsDocs.Length > aTagClosePos + 4 && rawJsDocs.Substring(aTagClosePos + 4, 1) == "`") {
					linkText += "`";
					aTagClosePos += 1;
				}
				newLinkValue = separateLinkTextOnly
					? linkText
					: "[" + linkText + "](" + hrefAttrValue + ")";
				rawJsDocs = rawJsDocs.Substring(0, aTagOpenPosBegin)
					+ newLinkValue
					+ rawJsDocs.Substring(aTagClosePos + 4);
				index = aTagOpenPosBegin + newLinkValue.Length;
			}
			return rawJsDocs;
		}
	}
}
