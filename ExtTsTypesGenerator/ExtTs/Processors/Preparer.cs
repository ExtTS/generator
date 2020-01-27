using ExtTs.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtTs.Processors {
	public delegate void PreparerProgressHandler(double percentage, int dirTransferIndex, int dirTransfersCounts, string sourceDirRelPath);
	public class Preparer {
		protected Processor processor;
		protected PreparerProgressHandler preparerProgressHandler;
		protected int processedPackageIndex;
		protected internal List<DirTransfer> dirTransfers;
		public Preparer(Processor processor) {
			this.processor = processor;
		}
		internal void PreparePackages(PreparerProgressHandler preparerProgressHandler) {
			this.preparerProgressHandler = preparerProgressHandler;
			bool extrackToolkitDirs = this.processor.Version.Major >= 6;
			this.dirTransfers = new List<DirTransfer>();
			List<string> packagesClone = new List<string>(this.processor.Packages);
			for (int i = 0; i < this.processor.Packages.Count; i++) 
				this.completePackageTransferDirs(ref packagesClone, this.processor.Packages[i], extrackToolkitDirs);
			this.processor.Packages = packagesClone;
			this.processedPackageIndex = 0;
			DirTransfer dirTransfer;
			for (int j = 0; j < this.dirTransfers.Count; j++) {
				dirTransfer = this.dirTransfers[j];
				this.processDirTransfer(
					dirTransfer.SrcDirFullPath,
					dirTransfer.TargetDirFullPath,
					dirTransfer.AppendToExisting
				);
				this.processedPackageIndex += 1;
			}
			string[] allPackageJsFiles;
			PackageSource packageSource;
			for (int k = 0; k < this.processor.Store.PackagesData.Count; k++) {
				packageSource = this.processor.Store.PackagesData[k];
				allPackageJsFiles = Directory.EnumerateFiles(
					packageSource.JsSourcesDirFullPath, "*.*", SearchOption.AllDirectories
				).ToArray<string>();
				packageSource.JsSourcesFilesCount = allPackageJsFiles.Length;
				this.processor.Store.PackagesData[k] = packageSource;
			}
		}
		internal void completePackageTransferDirs (ref List<string> packagesClone, string packageName, bool extrackToolkitDirs) {
			if (!this.processor.Store.SourcesPaths.ContainsKey(packageName))
				throw new Exception($"Package name `{packageName}` is not configured.");
			PkgCfg pkgCfg = this.processor.Store.SourcesPaths[packageName];
			// Complete package source files dir in tmp:
			string targetDirFullPath = this.processor.Store.TmpFullPath + "/src-" + packageName;
			// Create the directory if doesn't exist:
			if (!Directory.Exists(targetDirFullPath))
				Directory.CreateDirectory(targetDirFullPath);
			// Complete source dir full path - it always exist for any package:
			string packageBaseSourceDir = this.processor.Store.SourceFullPath + "/" + pkgCfg.Source;
			if (!Directory.Exists(packageBaseSourceDir)) {
				if (pkgCfg.Optional) {
					packagesClone.Remove(packageName); // not in this version
					return;
				}
				throw new Exception(
					$"Package `{packageName}` source directory `{pkgCfg.Source}` not found in given ZIP file."
				);
			}
			this.dirTransfers.Add(new DirTransfer {
				TargetDirFullPath = targetDirFullPath,
				SrcDirFullPath = packageBaseSourceDir,
				AppendToExisting = false
			});
			// Complete source overrides dir full path - it doesn't exist for all package:
			if (!String.IsNullOrEmpty(pkgCfg.SourceOverrides)) {
				this.dirTransfers.Add(new DirTransfer {
					TargetDirFullPath = targetDirFullPath,
					SrcDirFullPath = this.processor.Store.SourceFullPath + "/" + pkgCfg.SourceOverrides,
					AppendToExisting = true
				});
			}
			if (extrackToolkitDirs) { 
				// Complete toolkit dir full path - it doesn't exist for all package:
				bool classicToolkit = this.processor.Toolkit == "classic";
				string toolkitRelPath = classicToolkit 
					? (String.IsNullOrEmpty(pkgCfg.Classic) ? "" : pkgCfg.Classic)
					: (String.IsNullOrEmpty(pkgCfg.Modern) ? "" : pkgCfg.Modern);
				if (!String.IsNullOrEmpty(toolkitRelPath)) {
					this.dirTransfers.Add(new DirTransfer {
						TargetDirFullPath = targetDirFullPath,
						SrcDirFullPath = this.processor.Store.SourceFullPath + "/" + toolkitRelPath,
						AppendToExisting = false
					});
				}
				string toolkitOverridesRelPath = classicToolkit 
					? (String.IsNullOrEmpty(pkgCfg.ClassicOverrides) ? "" : pkgCfg.ClassicOverrides)
					: (String.IsNullOrEmpty(pkgCfg.ModernOverrides) ? "" : pkgCfg.ModernOverrides);
				if (!String.IsNullOrEmpty(toolkitOverridesRelPath)) {
					this.dirTransfers.Add(new DirTransfer {
						TargetDirFullPath = targetDirFullPath,
						SrcDirFullPath = this.processor.Store.SourceFullPath + "/" + toolkitOverridesRelPath,
						AppendToExisting = false
					});
				}
			}
			// Complete store package source data record:
			ExtJsPackage extJsPackage = ExtJsPackages.Names[packageName];
			this.processor.Store.PackagesData.Add(new PackageSource {
				PackageName = packageName,
				Type = extJsPackage,
				JsSourcesDirFullPath = targetDirFullPath,
				JsSourcesFilesCount = 0 // will be completed after transfer
			});
		}
		internal void processDirTransfer (string srcBaseDirFullPath, string targetBaseDirFullPath, bool appendToExisting) {
			string[] srcFullPaths = Directory.EnumerateFiles(
				srcBaseDirFullPath, "*.*", SearchOption.AllDirectories
			).ToArray<string>();
			int srcDirFullPathLength = srcBaseDirFullPath.TrimEnd('/').Length + 1;
			targetBaseDirFullPath = targetBaseDirFullPath.TrimEnd('/') + "/";
			string sourceDirRelPath = srcBaseDirFullPath.Substring(this.processor.Store.SourceFullPath.Length + 1);
			string srcFullPath;
			string srcRelPath;
			int lastSlashIndex;
			string targetFullPath;
			string targetDirFullPath;
			Stream srcStream;
			Stream targetStream;
			byte[] twoNewLinesBytes = Encoding.UTF8.GetBytes("\n\n");
			double progress;
			for (int i = 0, l = srcFullPaths.Length; i < l; i++) {
				srcFullPath = srcFullPaths[i].Replace('\\', '/');
				srcRelPath = srcFullPath.Substring(srcDirFullPathLength);
				targetFullPath = targetBaseDirFullPath + srcRelPath;
				lastSlashIndex = targetFullPath.LastIndexOf('/');
				if (lastSlashIndex != -1) {
					targetDirFullPath = targetFullPath.Substring(0, lastSlashIndex);
					if (!Directory.Exists(targetDirFullPath))
						Directory.CreateDirectory(targetDirFullPath); // it creates dir recursively
				}
				if (!appendToExisting) { 
					File.Copy(srcFullPath, targetFullPath);
				} else if (!File.Exists(targetFullPath)) {
					File.Copy(srcFullPath, targetFullPath);
				} else {
					srcStream = File.OpenRead(srcFullPath);
					targetStream = new FileStream(
						targetFullPath, FileMode.Append, FileAccess.Write, FileShare.None
					);
					targetStream.Write(twoNewLinesBytes, 0, twoNewLinesBytes.Length);
					srcStream.CopyTo(targetStream);
				}
				progress = 0.0;
				if (i > 0)
					progress =  (double)i / (double)l * 100.0;
				this.preparerProgressHandler(
					progress, this.processedPackageIndex, this.dirTransfers.Count, sourceDirRelPath
				);
			}
		}
	}
}