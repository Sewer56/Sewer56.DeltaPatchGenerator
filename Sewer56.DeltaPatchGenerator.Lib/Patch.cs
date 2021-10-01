using System;
using System.Collections.Generic;
using System.IO;
using Sewer56.DeltaPatchGenerator.Lib.Model;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Lib
{
    /// <summary>
    /// Tools for generating patches from given directories.
    /// A patch will losslessly transform one directory into another directory.
    /// </summary>
    public static class Patch
    {
        /// <summary>
        /// Applies a given path to the game.
        /// </summary>
        /// <param name="patch">The patch to apply.</param>
        /// <param name="sourceFolder">The folder to be patched.</param>
        /// <param name="outputFolder">The folder to output the result to.</param>
        /// <param name="reportProgress">Function that receives information on the current progress.</param>
        public static void Apply(PatchData patch, string sourceFolder, string outputFolder, Events.ProgressCallback reportProgress = null)
        {
            Apply(new [] { patch }, sourceFolder, outputFolder, reportProgress);
        }

        /// <summary>
        /// Applies a given path to the game.
        /// </summary>
        /// <param name="patches">The patches to apply.</param>
        /// <param name="sourceFolder">The folder to be patched.</param>
        /// <param name="outputFolder">The folder to output the result to.</param>
        /// <param name="reportProgress">Function that receives information on the current progress.</param>
        public static void Apply(Span<PatchData> patches, string sourceFolder, string outputFolder, Events.ProgressCallback reportProgress = null)
        {
            bool extractToSource   = sourceFolder.Equals(outputFolder, StringComparison.OrdinalIgnoreCase);
            string actualOutFolder = extractToSource ? Paths.TempFolder : outputFolder;

            if (extractToSource)
                IOEx.TryEmptyDirectory(Paths.TempFolder);

            Apply_Internal(patches, sourceFolder, actualOutFolder, reportProgress);

            if (extractToSource)
            {
                IOEx.MoveDirectory(actualOutFolder, sourceFolder);
                IOEx.TryDeleteDirectory(Paths.TempFolder);
            }
        }
        
        private static void Apply_Internal(Span<PatchData> patches, string sourceFolder, string outputFolder, Events.ProgressCallback reportProgress = null)
        {
            sourceFolder = Path.GetFullPath(sourceFolder);
            var sourceFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
            var createdFolders = new HashSet<string>();

            for (var x = 0; x < sourceFiles.Length; x++)
            {
                var sourceFile     = sourceFiles[x];
                var relativePath   = Paths.GetRelativePath(sourceFile, sourceFolder);
                reportProgress?.Invoke(relativePath, (double)x / sourceFiles.Length);

                if (!TryFindPatch(patches, sourceFile, relativePath, out var patch)) 
                    continue;

                var patchFilePath  = Paths.AppendRelativePath(relativePath, patch.Directory);
                var outputFilePath = Paths.AppendRelativePath(relativePath, outputFolder);
                createdFolders.CreateFolderIfNotCreated(Path.GetDirectoryName(outputFilePath));

                Utility.VCDiff.Apply(new ApplyOptions()
                {
                    Source = sourceFile,
                    Patch = patchFilePath,
                    Output = outputFilePath
                });
            }

            // Add extra files from patch.
            foreach (var patch in patches)
            {
                foreach (var addedFileRelativePath in patch.AddedFilesSet)
                {
                    var patchFilePath  = Paths.AppendRelativePath(addedFileRelativePath, patch.Directory);
                    var outputFilePath = Paths.AppendRelativePath(addedFileRelativePath, outputFolder);
                    File.Copy(patchFilePath, outputFilePath, true);
                }
            }

            reportProgress?.Invoke("Done", 1);
        }

        /// <summary>
        /// Generates a game patch.
        /// </summary>
        /// <param name="sourceFolder">The folder you want to turn into the target folder.</param>
        /// <param name="targetFolder">The target folder.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="reportProgress">Function that receives information on the current progress.</param>
        public static PatchData Generate(string sourceFolder, string targetFolder, string outputFolder, Events.ProgressCallback reportProgress = null)
        {
            var patch       = new PatchData();

            sourceFolder    = Path.GetFullPath(sourceFolder);
            var sourceFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
            Directory.CreateDirectory(outputFolder);
            var createdFolders = new HashSet<string>();

            for (var x = 0; x < sourceFiles.Length; x++)
            {
                var src = sourceFiles[x];
                var relativePath = Paths.GetRelativePath(src, sourceFolder);
                var destinationPath = Paths.AppendRelativePath(relativePath, targetFolder);
                string outputPath;

                reportProgress?.Invoke(relativePath, (double) x / sourceFiles.Length);
                if (!File.Exists(destinationPath))
                {
                    // Add missing file.
                    outputPath = Paths.AppendRelativePath(relativePath, outputFolder);
                    createdFolders.CreateFolderIfNotCreated(Path.GetDirectoryName(outputPath));
                    patch.AddNewFile(relativePath);
                    File.Copy(src, outputPath, true);
                    continue;
                }

                ulong hashSource = Hashing.CalculateHash(src);
                ulong hashDestination = Hashing.CalculateHash(destinationPath);
                if (hashSource == hashDestination)
                    continue;

                outputPath = Paths.AppendRelativePath(relativePath, outputFolder);
                createdFolders.CreateFolderIfNotCreated(Path.GetDirectoryName(outputPath));
                Utility.VCDiff.Compress(new CompressOptions()
                {
                    Source = src,
                    Target = destinationPath,
                    Output = outputPath
                });

                patch.AddPatchFile(hashSource, relativePath);
            }

            reportProgress?.Invoke("Done", 1);
            patch.Directory = outputFolder;
            patch.ToDirectory(outputFolder, out _);
            return patch;
        }

        /// <summary>
        /// Tries to find a patch with a file matching both the hash and relative path.
        /// </summary>
        /// <param name="patches">The patches to search through.</param>
        /// <param name="sourceFilePath">The path to the source file.</param>
        /// <param name="relativePath">The relative path to search for.</param>
        /// <param name="patchData">The found tuple.</param>
        private static bool TryFindPatch(Span<PatchData> patches, string sourceFilePath, string relativePath, out PatchData patchData)
        {
            ulong? hash = null;
            foreach (var patch in patches)
            {
                if (!patch.FilePathSet.Contains(relativePath)) 
                    continue;

                // Calculate hash if needed.
                hash ??= Hashing.CalculateHash(sourceFilePath);

                // Check if file hash is present.
                if (!patch.HashToPatchDictionary.TryGetValue(hash.Value, out var expectedRelativePath))
                    continue;

                patchData = patch;
                return true;
            }

            patchData = default;
            return false;
        }

        private static void CreateFolderIfNotCreated(this HashSet<string> directories, string directory)
        {
            if (!directories.Contains(directory))
            {
                Directory.CreateDirectory(directory);
                directories.Add(directory);
            }
        }
    }
}
