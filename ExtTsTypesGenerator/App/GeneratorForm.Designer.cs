namespace Generator
{
    partial class GeneratorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeneratorForm));
			this.btnGenerate = new System.Windows.Forms.Button();
			this.progressTotal = new System.Windows.Forms.ProgressBar();
			this.resultDirFullPath = new System.Windows.Forms.TextBox();
			this.progressStep = new System.Windows.Forms.ProgressBar();
			this.totalProgressLabel = new System.Windows.Forms.Label();
			this.progressGroupBox = new System.Windows.Forms.GroupBox();
			this.percentageTotal = new System.Windows.Forms.TextBox();
			this.percentageCurrentStep = new System.Windows.Forms.TextBox();
			this.progressText = new System.Windows.Forms.TextBox();
			this.currentStepProgressLabel = new System.Windows.Forms.Label();
			this.settingsGroupBox = new System.Windows.Forms.GroupBox();
			this.toolkitLabel = new System.Windows.Forms.Label();
			this.toolkitSelect = new System.Windows.Forms.ComboBox();
			this.overwriteExisting = new System.Windows.Forms.CheckBox();
			this.generateSingleFile = new System.Windows.Forms.CheckBox();
			this.generateDocs = new System.Windows.Forms.CheckBox();
			this.versionLabel = new System.Windows.Forms.Label();
			this.versionSelect = new System.Windows.Forms.ComboBox();
			this.pathsGroupBox = new System.Windows.Forms.GroupBox();
			this.sourceZipFullPathLabel = new System.Windows.Forms.Label();
			this.resultDirFullPathLabel = new System.Windows.Forms.Label();
			this.btnSelectResultDir = new System.Windows.Forms.Button();
			this.btnSelectSouzceZip = new System.Windows.Forms.Button();
			this.sourceZipFullPath = new System.Windows.Forms.TextBox();
			this.packageCore = new System.Windows.Forms.CheckBox();
			this.packageAMF = new System.Windows.Forms.CheckBox();
			this.packageCharts = new System.Windows.Forms.CheckBox();
			this.packageGoogle = new System.Windows.Forms.CheckBox();
			this.packageLegacy = new System.Windows.Forms.CheckBox();
			this.packageUX = new System.Windows.Forms.CheckBox();
			this.packageSOAP = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.customDocsUrlGroupBox = new System.Windows.Forms.GroupBox();
			this.customDocsUrlLabel = new System.Windows.Forms.Label();
			this.customDocsUrl = new System.Windows.Forms.TextBox();
			this.progressGroupBox.SuspendLayout();
			this.settingsGroupBox.SuspendLayout();
			this.pathsGroupBox.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.customDocsUrlGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnGenerate
			// 
			this.btnGenerate.Enabled = false;
			this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnGenerate.Location = new System.Drawing.Point(15, 27);
			this.btnGenerate.Margin = new System.Windows.Forms.Padding(4);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(250, 106);
			this.btnGenerate.TabIndex = 1;
			this.btnGenerate.Text = "Generate TS Definitions";
			this.btnGenerate.UseVisualStyleBackColor = true;
			// 
			// progressTotal
			// 
			this.progressTotal.Location = new System.Drawing.Point(280, 104);
			this.progressTotal.Margin = new System.Windows.Forms.Padding(4);
			this.progressTotal.Name = "progressTotal";
			this.progressTotal.Size = new System.Drawing.Size(249, 28);
			this.progressTotal.TabIndex = 2;
			// 
			// resultDirFullPath
			// 
			this.resultDirFullPath.Location = new System.Drawing.Point(15, 101);
			this.resultDirFullPath.Name = "resultDirFullPath";
			this.resultDirFullPath.Size = new System.Drawing.Size(420, 22);
			this.resultDirFullPath.TabIndex = 6;
			// 
			// progressStep
			// 
			this.progressStep.Location = new System.Drawing.Point(280, 47);
			this.progressStep.Margin = new System.Windows.Forms.Padding(4);
			this.progressStep.Name = "progressStep";
			this.progressStep.Size = new System.Drawing.Size(249, 28);
			this.progressStep.TabIndex = 7;
			// 
			// totalProgressLabel
			// 
			this.totalProgressLabel.AutoSize = true;
			this.totalProgressLabel.Location = new System.Drawing.Point(279, 84);
			this.totalProgressLabel.Name = "totalProgressLabel";
			this.totalProgressLabel.Size = new System.Drawing.Size(44, 17);
			this.totalProgressLabel.TabIndex = 8;
			this.totalProgressLabel.Text = "Total:";
			// 
			// progressGroupBox
			// 
			this.progressGroupBox.Controls.Add(this.percentageTotal);
			this.progressGroupBox.Controls.Add(this.percentageCurrentStep);
			this.progressGroupBox.Controls.Add(this.btnGenerate);
			this.progressGroupBox.Controls.Add(this.progressText);
			this.progressGroupBox.Controls.Add(this.currentStepProgressLabel);
			this.progressGroupBox.Controls.Add(this.progressStep);
			this.progressGroupBox.Controls.Add(this.totalProgressLabel);
			this.progressGroupBox.Controls.Add(this.progressTotal);
			this.progressGroupBox.Location = new System.Drawing.Point(12, 452);
			this.progressGroupBox.Name = "progressGroupBox";
			this.progressGroupBox.Size = new System.Drawing.Size(545, 270);
			this.progressGroupBox.TabIndex = 9;
			this.progressGroupBox.TabStop = false;
			this.progressGroupBox.Text = "5. Generating and progress:";
			// 
			// percentageTotal
			// 
			this.percentageTotal.BackColor = System.Drawing.SystemColors.Control;
			this.percentageTotal.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.percentageTotal.Location = new System.Drawing.Point(430, 86);
			this.percentageTotal.Name = "percentageTotal";
			this.percentageTotal.Size = new System.Drawing.Size(99, 15);
			this.percentageTotal.TabIndex = 12;
			this.percentageTotal.Text = "0.00 %";
			this.percentageTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// percentageCurrentStep
			// 
			this.percentageCurrentStep.BackColor = System.Drawing.SystemColors.Control;
			this.percentageCurrentStep.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.percentageCurrentStep.Location = new System.Drawing.Point(430, 29);
			this.percentageCurrentStep.Name = "percentageCurrentStep";
			this.percentageCurrentStep.Size = new System.Drawing.Size(99, 15);
			this.percentageCurrentStep.TabIndex = 11;
			this.percentageCurrentStep.Text = "0.00 %";
			this.percentageCurrentStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// progressText
			// 
			this.progressText.BackColor = System.Drawing.SystemColors.Control;
			this.progressText.Location = new System.Drawing.Point(16, 148);
			this.progressText.Multiline = true;
			this.progressText.Name = "progressText";
			this.progressText.Size = new System.Drawing.Size(513, 106);
			this.progressText.TabIndex = 10;
			// 
			// currentStepProgressLabel
			// 
			this.currentStepProgressLabel.AutoSize = true;
			this.currentStepProgressLabel.Location = new System.Drawing.Point(279, 27);
			this.currentStepProgressLabel.Name = "currentStepProgressLabel";
			this.currentStepProgressLabel.Size = new System.Drawing.Size(90, 17);
			this.currentStepProgressLabel.TabIndex = 9;
			this.currentStepProgressLabel.Text = "Current step:";
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.toolkitLabel);
			this.settingsGroupBox.Controls.Add(this.toolkitSelect);
			this.settingsGroupBox.Controls.Add(this.overwriteExisting);
			this.settingsGroupBox.Controls.Add(this.generateSingleFile);
			this.settingsGroupBox.Controls.Add(this.generateDocs);
			this.settingsGroupBox.Controls.Add(this.versionLabel);
			this.settingsGroupBox.Controls.Add(this.versionSelect);
			this.settingsGroupBox.Location = new System.Drawing.Point(12, 12);
			this.settingsGroupBox.Name = "settingsGroupBox";
			this.settingsGroupBox.Size = new System.Drawing.Size(265, 187);
			this.settingsGroupBox.TabIndex = 10;
			this.settingsGroupBox.TabStop = false;
			this.settingsGroupBox.Text = "1. Configuration:";
			// 
			// toolkitLabel
			// 
			this.toolkitLabel.AutoSize = true;
			this.toolkitLabel.Location = new System.Drawing.Point(13, 66);
			this.toolkitLabel.Name = "toolkitLabel";
			this.toolkitLabel.Size = new System.Drawing.Size(108, 17);
			this.toolkitLabel.TabIndex = 6;
			this.toolkitLabel.Text = "Toolkit (>6.0.0):";
			// 
			// toolkitSelect
			// 
			this.toolkitSelect.Enabled = false;
			this.toolkitSelect.FormattingEnabled = true;
			this.toolkitSelect.Location = new System.Drawing.Point(121, 63);
			this.toolkitSelect.Name = "toolkitSelect";
			this.toolkitSelect.Size = new System.Drawing.Size(128, 24);
			this.toolkitSelect.TabIndex = 5;
			// 
			// overwriteExisting
			// 
			this.overwriteExisting.AutoSize = true;
			this.overwriteExisting.Checked = true;
			this.overwriteExisting.CheckState = System.Windows.Forms.CheckState.Checked;
			this.overwriteExisting.Location = new System.Drawing.Point(16, 125);
			this.overwriteExisting.Name = "overwriteExisting";
			this.overwriteExisting.Size = new System.Drawing.Size(231, 21);
			this.overwriteExisting.TabIndex = 4;
			this.overwriteExisting.Text = "Overwrite existing TS definitions";
			this.overwriteExisting.UseVisualStyleBackColor = true;
			// 
			// generateSingleFile
			// 
			this.generateSingleFile.AutoSize = true;
			this.generateSingleFile.Location = new System.Drawing.Point(16, 152);
			this.generateSingleFile.Name = "generateSingleFile";
			this.generateSingleFile.Size = new System.Drawing.Size(236, 21);
			this.generateSingleFile.TabIndex = 3;
			this.generateSingleFile.Text = "Generate single file TS definition";
			this.generateSingleFile.UseVisualStyleBackColor = true;
			// 
			// generateDocs
			// 
			this.generateDocs.AutoSize = true;
			this.generateDocs.Checked = true;
			this.generateDocs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.generateDocs.Location = new System.Drawing.Point(16, 98);
			this.generateDocs.Name = "generateDocs";
			this.generateDocs.Size = new System.Drawing.Size(207, 21);
			this.generateDocs.TabIndex = 2;
			this.generateDocs.Text = "Generate JS Doc comments";
			this.generateDocs.UseVisualStyleBackColor = true;
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Location = new System.Drawing.Point(13, 29);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(101, 17);
			this.versionLabel.TabIndex = 1;
			this.versionLabel.Text = "Ext.JS version:";
			// 
			// versionSelect
			// 
			this.versionSelect.FormattingEnabled = true;
			this.versionSelect.Location = new System.Drawing.Point(121, 26);
			this.versionSelect.Name = "versionSelect";
			this.versionSelect.Size = new System.Drawing.Size(128, 24);
			this.versionSelect.TabIndex = 0;
			// 
			// pathsGroupBox
			// 
			this.pathsGroupBox.Controls.Add(this.sourceZipFullPathLabel);
			this.pathsGroupBox.Controls.Add(this.resultDirFullPathLabel);
			this.pathsGroupBox.Controls.Add(this.btnSelectResultDir);
			this.pathsGroupBox.Controls.Add(this.btnSelectSouzceZip);
			this.pathsGroupBox.Controls.Add(this.sourceZipFullPath);
			this.pathsGroupBox.Controls.Add(this.resultDirFullPath);
			this.pathsGroupBox.Location = new System.Drawing.Point(12, 305);
			this.pathsGroupBox.Name = "pathsGroupBox";
			this.pathsGroupBox.Size = new System.Drawing.Size(545, 139);
			this.pathsGroupBox.TabIndex = 11;
			this.pathsGroupBox.TabStop = false;
			this.pathsGroupBox.Text = "4. Source and results directory:";
			// 
			// sourceZipFullPathLabel
			// 
			this.sourceZipFullPathLabel.AutoSize = true;
			this.sourceZipFullPathLabel.Location = new System.Drawing.Point(12, 28);
			this.sourceZipFullPathLabel.Name = "sourceZipFullPathLabel";
			this.sourceZipFullPathLabel.Size = new System.Drawing.Size(198, 17);
			this.sourceZipFullPathLabel.TabIndex = 11;
			this.sourceZipFullPathLabel.Text = "Source  ZIP package full path:";
			// 
			// resultDirFullPathLabel
			// 
			this.resultDirFullPathLabel.AutoSize = true;
			this.resultDirFullPathLabel.Location = new System.Drawing.Point(13, 81);
			this.resultDirFullPathLabel.Name = "resultDirFullPathLabel";
			this.resultDirFullPathLabel.Size = new System.Drawing.Size(424, 17);
			this.resultDirFullPathLabel.TabIndex = 10;
			this.resultDirFullPathLabel.Text = "Results directory full path for generated TypeScript definition files:";
			// 
			// btnSelectResultDir
			// 
			this.btnSelectResultDir.Location = new System.Drawing.Point(442, 100);
			this.btnSelectResultDir.Name = "btnSelectResultDir";
			this.btnSelectResultDir.Size = new System.Drawing.Size(87, 23);
			this.btnSelectResultDir.TabIndex = 9;
			this.btnSelectResultDir.Text = "Select DIR";
			this.btnSelectResultDir.UseVisualStyleBackColor = true;
			// 
			// btnSelectSouzceZip
			// 
			this.btnSelectSouzceZip.Location = new System.Drawing.Point(442, 46);
			this.btnSelectSouzceZip.Name = "btnSelectSouzceZip";
			this.btnSelectSouzceZip.Size = new System.Drawing.Size(87, 23);
			this.btnSelectSouzceZip.TabIndex = 8;
			this.btnSelectSouzceZip.Text = "Select ZIP";
			this.btnSelectSouzceZip.UseVisualStyleBackColor = true;
			// 
			// sourceZipFullPath
			// 
			this.sourceZipFullPath.Location = new System.Drawing.Point(14, 47);
			this.sourceZipFullPath.Name = "sourceZipFullPath";
			this.sourceZipFullPath.Size = new System.Drawing.Size(420, 22);
			this.sourceZipFullPath.TabIndex = 7;
			// 
			// packageCore
			// 
			this.packageCore.AutoSize = true;
			this.packageCore.Checked = true;
			this.packageCore.CheckState = System.Windows.Forms.CheckState.Checked;
			this.packageCore.Enabled = false;
			this.packageCore.Location = new System.Drawing.Point(18, 31);
			this.packageCore.Name = "packageCore";
			this.packageCore.Size = new System.Drawing.Size(81, 21);
			this.packageCore.TabIndex = 5;
			this.packageCore.Text = "Ext core";
			this.packageCore.UseVisualStyleBackColor = true;
			// 
			// packageAMF
			// 
			this.packageAMF.AutoSize = true;
			this.packageAMF.Location = new System.Drawing.Point(18, 58);
			this.packageAMF.Name = "packageAMF";
			this.packageAMF.Size = new System.Drawing.Size(58, 21);
			this.packageAMF.TabIndex = 6;
			this.packageAMF.Text = "AMF";
			this.packageAMF.UseVisualStyleBackColor = true;
			// 
			// packageCharts
			// 
			this.packageCharts.AutoSize = true;
			this.packageCharts.Location = new System.Drawing.Point(18, 85);
			this.packageCharts.Name = "packageCharts";
			this.packageCharts.Size = new System.Drawing.Size(71, 21);
			this.packageCharts.TabIndex = 7;
			this.packageCharts.Text = "Charts";
			this.packageCharts.UseVisualStyleBackColor = true;
			// 
			// packageGoogle
			// 
			this.packageGoogle.AutoSize = true;
			this.packageGoogle.Location = new System.Drawing.Point(18, 112);
			this.packageGoogle.Name = "packageGoogle";
			this.packageGoogle.Size = new System.Drawing.Size(76, 21);
			this.packageGoogle.TabIndex = 8;
			this.packageGoogle.Text = "Google";
			this.packageGoogle.UseVisualStyleBackColor = true;
			// 
			// packageLegacy
			// 
			this.packageLegacy.AutoSize = true;
			this.packageLegacy.Location = new System.Drawing.Point(134, 31);
			this.packageLegacy.Name = "packageLegacy";
			this.packageLegacy.Size = new System.Drawing.Size(76, 21);
			this.packageLegacy.TabIndex = 9;
			this.packageLegacy.Text = "Legacy";
			this.packageLegacy.UseVisualStyleBackColor = true;
			// 
			// packageUX
			// 
			this.packageUX.AutoSize = true;
			this.packageUX.Location = new System.Drawing.Point(134, 85);
			this.packageUX.Name = "packageUX";
			this.packageUX.Size = new System.Drawing.Size(49, 21);
			this.packageUX.TabIndex = 11;
			this.packageUX.Text = "UX";
			this.packageUX.UseVisualStyleBackColor = true;
			// 
			// packageSOAP
			// 
			this.packageSOAP.AutoSize = true;
			this.packageSOAP.Location = new System.Drawing.Point(134, 58);
			this.packageSOAP.Name = "packageSOAP";
			this.packageSOAP.Size = new System.Drawing.Size(68, 21);
			this.packageSOAP.TabIndex = 10;
			this.packageSOAP.Text = "SOAP";
			this.packageSOAP.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.packageUX);
			this.groupBox1.Controls.Add(this.packageCore);
			this.groupBox1.Controls.Add(this.packageSOAP);
			this.groupBox1.Controls.Add(this.packageAMF);
			this.groupBox1.Controls.Add(this.packageLegacy);
			this.groupBox1.Controls.Add(this.packageCharts);
			this.groupBox1.Controls.Add(this.packageGoogle);
			this.groupBox1.Location = new System.Drawing.Point(292, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(265, 187);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "2. Additional packages:";
			// 
			// customDocsUrlGroupBox
			// 
			this.customDocsUrlGroupBox.Controls.Add(this.customDocsUrlLabel);
			this.customDocsUrlGroupBox.Controls.Add(this.customDocsUrl);
			this.customDocsUrlGroupBox.Location = new System.Drawing.Point(12, 210);
			this.customDocsUrlGroupBox.Name = "customDocsUrlGroupBox";
			this.customDocsUrlGroupBox.Size = new System.Drawing.Size(545, 87);
			this.customDocsUrlGroupBox.TabIndex = 13;
			this.customDocsUrlGroupBox.TabStop = false;
			this.customDocsUrlGroupBox.Text = "3. Custom documentation URL (optional)";
			// 
			// customDocsUrlLabel
			// 
			this.customDocsUrlLabel.AutoSize = true;
			this.customDocsUrlLabel.Location = new System.Drawing.Point(13, 30);
			this.customDocsUrlLabel.Name = "customDocsUrlLabel";
			this.customDocsUrlLabel.Size = new System.Drawing.Size(379, 17);
			this.customDocsUrlLabel.TabIndex = 12;
			this.customDocsUrlLabel.Text = "URL has to be in form: `https://your/custom/path/7.0.0-CE/`";
			// 
			// customDocsUrl
			// 
			this.customDocsUrl.Location = new System.Drawing.Point(15, 50);
			this.customDocsUrl.Name = "customDocsUrl";
			this.customDocsUrl.Size = new System.Drawing.Size(514, 22);
			this.customDocsUrl.TabIndex = 12;
			// 
			// GeneratorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(571, 735);
			this.Controls.Add(this.customDocsUrlGroupBox);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.pathsGroupBox);
			this.Controls.Add(this.settingsGroupBox);
			this.Controls.Add(this.progressGroupBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "GeneratorForm";
			this.Text = "Ext JS TypeScript Generator";
			this.Shown += new System.EventHandler(this.GeneratorForm_Shown);
			this.progressGroupBox.ResumeLayout(false);
			this.progressGroupBox.PerformLayout();
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.pathsGroupBox.ResumeLayout(false);
			this.pathsGroupBox.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.customDocsUrlGroupBox.ResumeLayout(false);
			this.customDocsUrlGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion
		protected System.Windows.Forms.TextBox resultDirFullPath;
		protected System.Windows.Forms.ProgressBar progressStep;
		protected System.Windows.Forms.Button btnGenerate;
		protected System.Windows.Forms.ProgressBar progressTotal;
		protected System.Windows.Forms.GroupBox progressGroupBox;
		protected System.Windows.Forms.GroupBox settingsGroupBox;
		protected System.Windows.Forms.TextBox sourceZipFullPath;
		private System.Windows.Forms.TextBox progressText;
		private System.Windows.Forms.ComboBox versionSelect;
		protected System.Windows.Forms.Label versionLabel;
		protected System.Windows.Forms.Label totalProgressLabel;
		protected System.Windows.Forms.Button btnSelectSouzceZip;
		protected System.Windows.Forms.Label currentStepProgressLabel;
		protected System.Windows.Forms.Label sourceZipFullPathLabel;
		protected System.Windows.Forms.Label resultDirFullPathLabel;
		protected System.Windows.Forms.Button btnSelectResultDir;
		protected System.Windows.Forms.TextBox percentageTotal;
		protected System.Windows.Forms.TextBox percentageCurrentStep;
		protected System.Windows.Forms.GroupBox pathsGroupBox;
		protected System.Windows.Forms.CheckBox generateSingleFile;
		protected System.Windows.Forms.CheckBox generateDocs;
		protected System.Windows.Forms.CheckBox overwriteExisting;
		protected System.Windows.Forms.CheckBox packageUX;
		protected System.Windows.Forms.CheckBox packageSOAP;
		protected System.Windows.Forms.CheckBox packageLegacy;
		protected System.Windows.Forms.CheckBox packageGoogle;
		protected System.Windows.Forms.CheckBox packageCharts;
		protected System.Windows.Forms.CheckBox packageAMF;
		protected System.Windows.Forms.CheckBox packageCore;
		private System.Windows.Forms.GroupBox groupBox1;
		protected System.Windows.Forms.TextBox customDocsUrl;
		protected System.Windows.Forms.GroupBox customDocsUrlGroupBox;
		protected System.Windows.Forms.Label customDocsUrlLabel;
		protected System.Windows.Forms.Label toolkitLabel;
		private System.Windows.Forms.ComboBox toolkitSelect;
	}
}

