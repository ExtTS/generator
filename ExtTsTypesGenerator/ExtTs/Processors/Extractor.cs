using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using ICSharpCode.SharpZipLib.Core;

namespace ExtTs.Processors {
	public delegate void ExtractProgressHandler(double percentage, int baseDirIndex, int baseDirsCount, string baseDir);
	public class Extractor {
		protected internal static string tmpDirRelPath = "tmp";
		protected Processor processor;
		protected List<string> baseDirs;
		protected int extractingBaseDirIndex;
		protected string extractingBaseDirName;
		protected ExtractProgressHandler extractProgressHandler;
		protected long zipBytesAll;
		protected long zipBytesRead;

		public Extractor (Processor processor) {
			this.processor = processor;
			string entryAsmFullPath = Assembly.GetEntryAssembly().Location.Replace('\\', '/').TrimEnd('/');
			int lastSlashPos = entryAsmFullPath.LastIndexOf('/');
			if (lastSlashPos == -1) lastSlashPos = 0;
			entryAsmFullPath = entryAsmFullPath.Substring(0, lastSlashPos);
			this.processor.Store.RootDirFullPath = entryAsmFullPath;
			this.processor.Store.TmpFullPath = entryAsmFullPath + "/" + Extractor.tmpDirRelPath;
		}
		internal bool CheckTmpDirectory() {
			bool tmpDirExists = Directory.Exists(this.processor.Store.TmpFullPath);
			// If tmp dir is not possible to create - exit with error:
			if (!tmpDirExists) {
				Directory.CreateDirectory(this.processor.Store.TmpFullPath);// it creates directory recursively
				tmpDirExists = Directory.Exists(this.processor.Store.TmpFullPath);
				if (!tmpDirExists)
					return this.addExceptionAndReturnFalse(String.Format(
						"Temporary directory is not possible to create: `{0}`.", this.processor.Store.TmpFullPath
					));
			}
			// Empty whole tmp directory:
			try {
				DirectoryInfo dInfo = new DirectoryInfo(this.processor.Store.TmpFullPath);
				foreach (FileInfo file in dInfo.GetFiles()) 
					file.Delete(); 
				foreach (DirectoryInfo dir in dInfo.GetDirectories()) 
					dir.Delete(true);
			} catch (Exception ex) {
				this.processor.Exceptions.Add(ex);
				this.addExceptionAndReturnFalse(String.Format(
					"Temporary directory is not possible to clear before processing: `{0}`.", this.processor.Store.TmpFullPath
				));
			}
			return true;
		}
		protected bool addExceptionAndReturnFalse (string errorMsg) {
			try {
				throw new ArgumentException(errorMsg);
			} catch (Exception ex) {
				this.processor.Exceptions.Add(ex);
			}
			return false;
		}
		protected internal void ExtractSourcePackage (ExtractProgressHandler extractProgressHandler) {
			this.extractProgressHandler = extractProgressHandler;
			this.completeBaseDirs();
			this.extractBaseDirs();
			this.setUpSourceFullPath();
		}
		protected void completeBaseDirs () {
			this.baseDirs = new List<string>();
			PkgCfg packageCfg;
			bool extrackToolkitDirs = this.processor.Version.Major >= 6;
			foreach (var item in this.processor.Store.SourcesPaths) {
				if (!this.processor.Packages.Contains(item.Key)) continue;
				packageCfg = item.Value;
				this.completeBaseDirAndAddIfNotExist(packageCfg.Source);
				this.completeBaseDirAndAddIfNotExist(packageCfg.SourceOverrides);
				if (!extrackToolkitDirs) continue;
				if (this.processor.Toolkit == "classic") { 
					this.completeBaseDirAndAddIfNotExist(packageCfg.Classic);
					this.completeBaseDirAndAddIfNotExist(packageCfg.ClassicOverrides);
				} else if (this.processor.Toolkit == "modern") { 
					this.completeBaseDirAndAddIfNotExist(packageCfg.Modern);
					this.completeBaseDirAndAddIfNotExist(packageCfg.ModernOverrides);
				}
			}
		}
		protected void completeBaseDirAndAddIfNotExist (string packageDirRelPath) {
			if (String.IsNullOrEmpty(packageDirRelPath)) return;
			packageDirRelPath = packageDirRelPath.TrimEnd('/');
			if (!this.baseDirs.Contains(packageDirRelPath))
				this.baseDirs.Add(packageDirRelPath);
		}
		protected void extractBaseDirs () {
			this.zipBytesAll = new FileInfo(this.processor.SourcePackageFullPath).Length;
			this.zipBytesRead = 0;
			int index = 0;
			foreach (string baseDir in this.baseDirs) {
				this.extractingBaseDirIndex = index;
				this.extractingBaseDirName = baseDir;
				this.extractBaseDir();
				index += 1;
			}
			this.extractProgressHandler.Invoke(
				100.0, index, this.baseDirs.Count, this.extractingBaseDirName
			);
		}
		protected void extractBaseDir () {
			string zipFullPath = this.processor.SourcePackageFullPath;
			string targetDir = this.processor.Store.TmpFullPath;
			FastZipEvents events = new FastZipEvents();
			events.Progress = this.unzipProgressHandler;
			FastZip fastZip = new FastZip(events);
			fastZip.ExtractZip(
				zipFullPath, 
				targetDir, 
				@"ext([^/]+)/" + this.extractingBaseDirName + @"/(.*)\.js$"
			);
		}
		protected void unzipProgressHandler(object sender, ProgressEventArgs e) {
			this.zipBytesRead += e.Processed;
			double percentage = 0.0;
			if (this.zipBytesRead > 0) 
				percentage = ((double)this.zipBytesRead) / ((double)this.zipBytesAll) * 100.0;
			this.extractProgressHandler.Invoke(
				percentage,
				this.extractingBaseDirIndex + 1, 
				this.baseDirs.Count, 
				this.extractingBaseDirName
			);
		}
		protected void setUpSourceFullPath () {
			DirectoryInfo tmpDirInfo = new DirectoryInfo(this.processor.Store.TmpFullPath);
			foreach (DirectoryInfo subDir in tmpDirInfo.GetDirectories()) { 
				if (subDir.Name.Substring(0, 4) == "ext-") {
					this.processor.Store.SourceFullPath = this.processor.Store.TmpFullPath + "/" + subDir.Name;
					break;
				}
			}
		}
	}
}