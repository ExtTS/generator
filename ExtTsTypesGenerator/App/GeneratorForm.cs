using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using ExtTs;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection;
using ExtTs.Processors;
using System.Text.RegularExpressions;
using System.Threading;

namespace Generator {
    public partial class GeneratorForm : Form {
		protected static Dictionary<string, ExtJsToolkit> Toolkits = new Dictionary<string, ExtJsToolkit>() {
			{  "CLASSIC", ExtJsToolkit.CLASSIC },
			{  "MODERN", ExtJsToolkit.MODERN },
		};
		protected static Dictionary<ExtJsPackage, string> PackagesFields = new Dictionary<ExtJsPackage, string>() {
			{ ExtJsPackage.CORE,	"packageCore" },
			{ ExtJsPackage.AMF,		"packageAMF" },
			{ ExtJsPackage.CHARTS,	"packageCharts"	 },
			{ ExtJsPackage.GOOGLE,	"packageGoogle"	 },
			{ ExtJsPackage.LEGACY,	"packageLegacy"	 },
			{ ExtJsPackage.SOAP,	"packageSOAP" },
			{ ExtJsPackage.UX,		"packageUX" },
		};
		protected static Dictionary<string, ExtJsPackage> FieldsPackages = new Dictionary<string, ExtJsPackage>();
		protected Processor processor;
		protected string documentRoot;
		protected string sourceDir = null;
		protected string resultsDir = null;
		protected List<string> supportedVersions;

		public GeneratorForm() {
            this.InitializeComponent();
		}
		private void GeneratorForm_Shown(object sender, EventArgs e) {
			this.initDocumentRoot();
			this.initFieldsPackages();
			this.initProcessorInstance();
			this.initFormControlsValues();
#if DEBUG
			/*
			this.versionSelect.Text = "6.0.1";
			this.toolkitSelect.Enabled = true;
			//this.toolkitSelect.Text = "Classic";
			this.toolkitSelect.Text = "Modern";
			this.generateSingleFile.Checked = true;
			this.packageAMF.Checked = true;
			this.packageCharts.Checked = true;
			this.packageCore.Checked = true;
			this.packageGoogle.Checked = true;
			this.packageLegacy.Checked = true;
			this.packageSOAP.Checked = true;
			this.packageUX.Checked = true;
			this.displayJsDuckErrors.Checked = true;
			this.sourceZipFullPath.Text = @"c:/Users/Administrator/Desktop/Ext.TS/gpl-zips/ext-6.0.1-gpl.zip";
			this.resultDirFullPath.Text = @"c:/Users/Administrator/Desktop/Ext.TS/example-project-601-modern/js/types/";
			this.checkInputs();
			//this.processor.SetDebuggingTmpDirDataUse(true);
			*/
#endif
		}
		protected void initDocumentRoot() {
			this.documentRoot = Assembly.GetEntryAssembly().Location;
			int lastSlashIndex = this.documentRoot.LastIndexOf('\\');
			if (lastSlashIndex != -1)
				this.documentRoot = this.documentRoot.Substring(0, lastSlashIndex);
		}
		protected void initFieldsPackages() {
			foreach (var item in GeneratorForm.PackagesFields)
				GeneratorForm.FieldsPackages.Add(item.Value, item.Key);
		}
		protected void initProcessorInstance () {
			this.processor = Processor.CreateNewInstance();
			this.Invoke((MethodInvoker)delegate {
				this.progressText.Text = "";
				this.percentageCurrentStep.Text = "0.00 %";
				this.progressStep.Value = 0;
				this.percentageTotal.Text = "0.00 %";
				this.progressTotal.Value = 0;
			});
		}
		protected void initFormControlsValues () {
			this.supportedVersions = this.processor.GetSupportedVersions();
			this.versionSelect.Items.AddRange(
				this.supportedVersions.ToArray<string>()
			);
			this.toolkitSelect.Items.AddRange(new string[] {
				"Classic", "Modern"
			});
			this.versionSelect.TextChanged += delegate (object o, EventArgs e) {
				this.checkInputs();
			};
			this.toolkitSelect.TextChanged += delegate (object o, EventArgs e) {
				this.checkInputs();
			};
			this.sourceZipFullPath.TextChanged += delegate (object o, EventArgs e) {
				int lastSlashIndex = this.sourceZipFullPath.Text.LastIndexOf('\\');
				if (lastSlashIndex != -1) {
					this.sourceDir = this.sourceZipFullPath.Text.Substring(0, lastSlashIndex);
				} else {
					this.sourceDir = this.sourceZipFullPath.Text;
				}
				this.sourceZipFullPath.Text = this.sourceZipFullPath.Text.Replace('\\', '/');
				this.checkInputs();
			};
			this.btnSelectSouzceZip.Click += delegate (object o, EventArgs e) {
				CommonOpenFileDialog fileDialog = new CommonOpenFileDialog();
				fileDialog.Filters.Add(new CommonFileDialogFilter(
					"Ext.JS ZIP package", "*.zip"
				));
				if (!String.IsNullOrEmpty(this.sourceDir)) {
					fileDialog.InitialDirectory = this.sourceDir;
				} else {
					fileDialog.InitialDirectory = this.documentRoot;
				}
				if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok) {
					this.sourceZipFullPath.Text = fileDialog.FileName.Replace('\\', '/');
					int lastSlashIndex = fileDialog.FileName.LastIndexOf('\\');
					if (lastSlashIndex != -1) {
						this.sourceDir = fileDialog.FileName.Substring(0, lastSlashIndex);
					} else {
						this.sourceDir = fileDialog.FileName;
					}
					this.checkInputs();
				}
			};
			this.resultDirFullPath.TextChanged += delegate (object o, EventArgs e) {
				this.resultsDir = this.resultDirFullPath.Text;
				this.resultDirFullPath.Text = this.resultDirFullPath.Text.Replace('\\', '/');
				this.checkInputs();
			};
			this.btnSelectResultDir.Click += delegate (object o, EventArgs e) {
				CommonOpenFileDialog dirDialog = new CommonOpenFileDialog();
				if (!String.IsNullOrEmpty(this.resultsDir)) {
					dirDialog.InitialDirectory = this.resultsDir;
				} else {
					dirDialog.InitialDirectory = this.documentRoot;
				}
				dirDialog.IsFolderPicker = true;
				if (dirDialog.ShowDialog() == CommonFileDialogResult.Ok) {
					this.resultDirFullPath.Text = dirDialog.FileName.Replace('\\', '/');
					this.resultsDir = dirDialog.FileName;
					this.checkInputs();
				}
			};
			this.btnGenerate.Click += delegate (object o, EventArgs e) {
				this.runProcessing();
			};
		}
		protected void checkInputs() {
			this.Invoke((MethodInvoker)delegate {
				this.progressText.Text = "";
				int requiredValuesCompleted = 0;
				int requiredValuesCount = 3;
				var versionStr = this.versionSelect.Text.Trim();
				Version versionObj = null;
				if (this.supportedVersions.Contains(versionStr)) {
					this.processor.SetVersion(versionStr);
					requiredValuesCompleted += 1;
					versionStr = Regex.Replace(versionStr, @"[^0-9\.]", "");
					versionObj = new Version(versionStr);
					bool toolkitNecessary = versionObj.Major >= 6;
					this.toolkitSelect.Invoke((MethodInvoker)delegate {
						this.toolkitSelect.Enabled = toolkitNecessary;
					});
					if (toolkitNecessary) {
						requiredValuesCount += 1;
						string toolkit = this.toolkitSelect.Text.Trim().ToUpper();
						if (GeneratorForm.Toolkits.ContainsKey(toolkit)) {
							this.processor.SetToolkit(GeneratorForm.Toolkits[toolkit]);
							requiredValuesCompleted += 1;
						}
					}
				}
				string sourceZipFullPath = this.sourceZipFullPath.Text.Trim();
				if (!String.IsNullOrEmpty(sourceZipFullPath)) {
					if (File.Exists(sourceZipFullPath)) {
						this.processor.SetSourcePackageFullPath(sourceZipFullPath);
						requiredValuesCompleted += 1;
					} else {
						this.progressText.Text += "No source ZIP package found in given path.";
					}
				}
				string resultDirFullPath = this.resultDirFullPath.Text.Trim();
				if (!String.IsNullOrEmpty(resultDirFullPath)) {
					if (Directory.Exists(resultDirFullPath)) { 
						this.processor.SetResultsDirFullPath(resultDirFullPath, this.overwriteExisting.Checked);
						requiredValuesCompleted += 1;
					} else {
						this.progressText.Text += "No results directory found in given path.";
					}
				}
				// Packages
				if (versionObj != null) {
					List<ExtJsPackage> versionPackages = this.processor.GetSupportedPackages().ToList<ExtJsPackage>();
					ExtJsPackage[] allPackages = GeneratorForm.PackagesFields.Keys.ToArray<ExtJsPackage>();
					ExtJsPackage versionPackage;
					string packageFieldName;
					FieldInfo packageField;
					CheckBox packageCheckbox;
					Type formType = this.GetType();
					bool enabled;
					for (int i = 0; i < allPackages.Length; i++) {
						versionPackage = allPackages[i];
						enabled = versionPackage != ExtJsPackage.CORE && versionPackages.Contains(versionPackage);
						packageFieldName = GeneratorForm.PackagesFields[versionPackage];
						packageField = formType.GetField(
							packageFieldName,
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
						);
						if (packageField != null) {
							packageCheckbox = packageField.GetValue(this) as CheckBox;
							packageCheckbox.Enabled = enabled;
							packageCheckbox.Invoke((MethodInvoker)delegate {
								packageCheckbox.Enabled = enabled;
							});
						}
					}
				}
				// Enable or disable generate btn:
				this.btnGenerate.Enabled = requiredValuesCompleted == requiredValuesCount;
			});
		}
		protected void runProcessing () {
			this.checkInputs();
			// Conf checkboxes:
			this.processor.SetGenerateJsDocs(this.generateDocs.Checked);
			this.processor.SetGenerateSingleFile(this.generateSingleFile.Checked);
			// Packages checkboxes:
			List<ExtJsPackage> versionPackages = this.processor.GetSupportedPackages().ToList<ExtJsPackage>();
			ExtJsPackage[] allPackages = GeneratorForm.PackagesFields.Keys.ToArray<ExtJsPackage>();
			ExtJsPackage versionPackage;
			string packageFieldName;
			FieldInfo packageField;
			CheckBox packageCheckbox;
			Type formType = this.GetType();
			bool enabled;
			List<ExtJsPackage> packagesToSet = new List<ExtJsPackage>() { ExtJsPackage.CORE };
			for (int i = 0; i < allPackages.Length; i++) {
				versionPackage = allPackages[i];
				enabled = versionPackage != ExtJsPackage.CORE && versionPackages.Contains(versionPackage);
				if (!enabled) continue;
				packageFieldName = GeneratorForm.PackagesFields[versionPackage];
				packageField = formType.GetField(
					packageFieldName,
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
				);
				if (packageField != null) {
					packageCheckbox = packageField.GetValue(this) as CheckBox;
					if (packageCheckbox.Checked) 
						packagesToSet.Add(versionPackage);
				}
			}
			this.processor.SetPackages(packagesToSet.ToArray<ExtJsPackage>());
			// Custom docs URL:
			string customDocsUrl = this.customDocsUrl.Text.Trim();
			if (!String.IsNullOrEmpty(customDocsUrl))
				this.processor.SetCustomDocsBaseUrl(customDocsUrl);
			// Clear progress text:
			this.progressText.Text = "";
			// Set handlers: 
			this.processor.SetDebuggingDisplayJsDuckErrors(this.displayJsDuckErrors.Checked);
			this.processor.SetUserPromptHandler(this.userPrompt);
			this.processor.SetProcessingInfoHandler(this.displayProgress);
			// Disable controls:
			this.setControlsEnabled(false);
			// Run:
			Thread processingThread = new Thread(new ThreadStart(delegate {
				this.processor.Process(
					this.processedHandler
				);
			}));
			processingThread.Priority = ThreadPriority.AboveNormal;
			processingThread.IsBackground = true;
			processingThread.Start();
		}
		protected string userPrompt(PromptInfo promptInfo) {
			string textContentStr = promptInfo.Question + "\n";
			string optionLine;
			foreach (var item in promptInfo.Options) {
				optionLine = $"\t'{item.Key}'\t– {item.Value}";
				if (item.Key == promptInfo.Default)
					optionLine += " (default)";
				textContentStr += "\n" + optionLine;
			}
			textContentStr += @"\n\nType one of the following options and press Enter key. "
				+@"\nPress only Enter key to choose default option.\n";
			Form prompt			= new Form() {
				Width			= 600,
				Height			= 330,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Text			= "Question",
				StartPosition	= FormStartPosition.CenterScreen
			};
			TextBox textContent	= new TextBox() {
				Left			= 20,
				Top				= 20,
				Width			= 540,
				Height			= 170,
				Text			= textContentStr.Replace("\n", "\r\n"),
				BackColor		= this.BackColor,
				BorderStyle		= BorderStyle.None,
				Multiline		= true,
				ReadOnly		= true
			};
			TextBox textInput	= new TextBox() {
				Left			= 20,
				Top				= 200,
				Width			= 540,
				Text			= promptInfo.Default
			};
			Button confirmBtn = new Button() {
				Text			= "Ok",
				Left			= 460,
				Width			= 100,
				Top				= 240,
				DialogResult	= DialogResult.OK
			};
			confirmBtn.Click += (sender, e) => {
				prompt.Close();
			};
			prompt.Controls.Add(textContent);
			textContent.DeselectAll();
			prompt.Controls.Add(textInput);
			prompt.Controls.Add(confirmBtn);
			textInput.Focus();
			textInput.SelectAll();
			prompt.AcceptButton = confirmBtn;
			string resultValue = prompt.ShowDialog() == DialogResult.OK 
				? textInput.Text.Trim()
				: "";
			if (String.IsNullOrEmpty(resultValue)) 
				resultValue = promptInfo.Default;
			if (promptInfo.Options.ContainsKey(resultValue))
				return resultValue;
			return this.userPrompt(promptInfo);
		}
		protected void displayProgress (ProcessingInfo processingInfo) {
			string progressText = String.Format("Stage {0} of {1}:",
				processingInfo.StageIndex, processingInfo.StagesCount
			);
			double totalPercentage = 0.0;
			if (processingInfo.StageIndex > 1) {
				totalPercentage = (
					(double)(processingInfo.StageIndex - 1) / (double)processingInfo.StagesCount * 100
				);
			}
			totalPercentage += (
				1.0 / (double)processingInfo.StagesCount * (processingInfo.Progress / 100.0) * 100.0
			);
			progressText += "\r\n   " + processingInfo.StageName;
			progressText += "\r\n" + processingInfo.InfoText.Replace(": ", ": \r\n   ");
			this.Invoke((MethodInvoker)delegate {
				this.progressText.Text = progressText;
				this.percentageCurrentStep.Text = processingInfo.Progress.ToString("0.00") + " %";
				this.progressStep.Value = (int)processingInfo.Progress;
				this.percentageTotal.Text = totalPercentage.ToString("0.00") + " %";
				this.progressTotal.Value = (int)totalPercentage;
			});
		}
		protected void processedHandler (bool success, ProcessingInfo processingInfo) {
			this.Invoke((MethodInvoker)delegate {
				List<string> jsDuckErrors = this.displayJsDuckErrors.Checked
					? this.processor.GetJsDuckErrors()
					: new List<string>();
				List<Exception> exceptions = this.processor.GetExceptions();
				StringBuilder textContentSb = new StringBuilder();
				string title = (success || processingInfo.StageIndex == processingInfo.StagesCount)
					? ((jsDuckErrors.Count == 0 && exceptions.Count == 0)
						? "Processing successfully finished."
						: "Processing finished with following errors:")
					: "Processing can NOT start due to those errors:";
				if (success && jsDuckErrors.Count == 0 && exceptions.Count == 0) {
					MessageBox.Show(title, "Finished");
				} else {
					if (jsDuckErrors.Count > 0) { 
						textContentSb.AppendLine(title);
						textContentSb.AppendLine("");
						foreach (string jsDuckError in jsDuckErrors) {
							textContentSb.AppendLine("");
							textContentSb.AppendLine("\t" + jsDuckError);
						}
					}
					if (exceptions.Count > 0) {
						textContentSb.AppendLine(title);
						textContentSb.AppendLine("");
						foreach (Exception ex in exceptions) {
							if (ex is ArgumentException) {
								textContentSb.AppendLine("");
								textContentSb.AppendLine("\t" + ex.Message);
							} else {
								textContentSb.AppendLine("");
								textContentSb.AppendLine(
									Desharp.Debug.Dump(ex, new Desharp.DumpOptions {
										SourceLocation = true,
										Return = true,
									}).Replace("\t", "   ")
								);
							}
						}
					}
					int width = 800;
					int height = 600;
					Form msgWindow = new Form() {
						Width = width,
						Height = height,
						FormBorderStyle = FormBorderStyle.FixedDialog,
						Text = success ? "Finished with errors." : "Not started due to errors.",
						StartPosition = FormStartPosition.CenterScreen
					};
					TextBox textContent = new TextBox() {
						Left = 20,
						Top = 20,
						Width = width - 60,
						Height = height - 60 - 30 - 30,
						Text = textContentSb.ToString().Replace("\n", "\r\n"),
						BackColor = this.BackColor,
						Multiline = true,
						ReadOnly = true,
					};
					Button confirmBtn = new Button() {
						Text = "Ok",
						Top = height - 60 - 30,
						Left = (width / 2) - (100 / 2),
						Width = 100,
						DialogResult = DialogResult.OK
					};
					confirmBtn.Click += (sender, e) => {
						msgWindow.Close();
					};
					msgWindow.Controls.Add(textContent);
					textContent.DeselectAll();
					msgWindow.Controls.Add(confirmBtn);
					msgWindow.AcceptButton = confirmBtn;
					msgWindow.ShowDialog();
				}
				this.setControlsEnabled(true);
				this.initProcessorInstance();
				this.checkInputs();
			});
		}
		protected void setControlsEnabled (bool enabled) {
			List<string> controls = new List<string>() {
				"btnGenerate",
				"versionSelect",
				"toolkitSelect",
				"generateDocs",
				"overwriteExisting",
				"generateSingleFile",
				"customDocsUrl",
				"sourceZipFullPath",
				"resultDirFullPath",
				"btnSelectSouzceZip",
				"btnSelectResultDir",
				"displayJsDuckErrors",
			};
			controls.AddRange(
				GeneratorForm.PackagesFields.Values.ToArray<string>()
			);
			FieldInfo field = null; ;
			Type formType = this.GetType();
			Control controlElement;
			foreach (string controlName in controls) {
				field = formType.GetField(
					controlName,
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
				);
				if (field != null) {
					controlElement = field.GetValue(this) as Control;
					controlElement.Invoke((MethodInvoker)delegate {
						controlElement.Enabled = enabled;
					});
				}
			}
		}
	}
}
