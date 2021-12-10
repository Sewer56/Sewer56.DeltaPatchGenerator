using System;
using System.Collections.Generic;
using System.IO;
using Sewer56.DeltaPatchGenerator.Lib.Model;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Lib
{
    /// <summary>
    /// Tools for generating hash sets from given directories.
    /// A hash set can be used as future reference saying what files should be contained in a folder and
    /// for checking that every file is correct.
    /// </summary>
    public static class HashSet
    {
        /// <summary>
        /// Removes any files in the hash set not present in the target folder.
        /// </summary>
        /// <param name="fileSet">The set of files to use as reference.</param>
        /// <param name="sourceFolder">The folder to be cleaned.</param>
        public static void Cleanup(FileHashSet fileSet, string sourceFolder)
        {
            Cleanup(fileSet, sourceFolder, s => true);
        }

        /// <summary>
        /// Removes any files in the hash set not present in the target folder.
        /// </summary>
        /// <param name="fileSet">The set of files to use as reference.</param>
        /// <param name="sourceFolder">The folder to be cleaned.</param>
        /// <param name="shouldDeleteFile">Allows you to override whether the file should be deleted.</param>
        public static void Cleanup(FileHashSet fileSet, string sourceFolder, Events.ShouldDeleteFileCallback shouldDeleteFile)
        {
            sourceFolder = Path.GetFullPath(sourceFolder);
            var allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
            var hashSet  = BuildHashSet(fileSet);

            foreach (var file in allFiles)
            {
                var relativePath = Paths.GetRelativePath(file, sourceFolder);
                if (!hashSet.Contains(relativePath) && shouldDeleteFile(relativePath)) 
                    File.Delete(file);
            }
        }

        /// <summary>
        /// Verifies that a given directory matches all the hashes with 
        /// </summary>
        /// <param name="fileSet">The set of files to verify.</param>
        /// <param name="sourceFolder">The folder to be verified.</param>
        /// <param name="missingFiles">Files that were missing from the folder.</param>
        /// <param name="hashMismatchFiles">Files with a mismatching hash.</param>
        /// <param name="reportProgress">Function that receives information on the current progress.</param>
        /// <returns></returns>
        public static bool Verify(FileHashSet fileSet, string sourceFolder, out List<string> missingFiles, out List<string> hashMismatchFiles, Events.ProgressCallback reportProgress = null)
        {
            sourceFolder      = Path.GetFullPath(sourceFolder);
            missingFiles      = new List<string>();
            hashMismatchFiles = new List<string>();

            for (var x = 0; x < fileSet.Files.Count; x++)
            {
                var file           = fileSet.Files[x];
                var targetFilePath = Paths.AppendRelativePath(file.RelativePath, sourceFolder);
                reportProgress?.Invoke(file.RelativePath, (double) x / fileSet.Files.Count);

                if (!File.Exists(targetFilePath))
                {
                    missingFiles.Add(targetFilePath);
                }
                else
                {
                    var hash = Hashing.CalculateHash(targetFilePath);
                    if (hash != file.Hash)
                        hashMismatchFiles.Add(targetFilePath);
                }
            }

            reportProgress?.Invoke("Done", 1);
            return missingFiles.Count == 0 && hashMismatchFiles.Count == 0;
        }

        /// <summary>
        /// Generates a file hash set.
        /// </summary>
        /// <param name="sourceFolder">The source folder to create a list of hashes from.</param>
        /// <param name="reportProgress">Function that receives information on the current progress.</param>
        /// <param name="shouldIgnoreFile">Returns true if a file should be ignored.</param>
        public static FileHashSet Generate(string sourceFolder, Events.ProgressCallback reportProgress = null, Events.ShouldDeleteFileCallback shouldIgnoreFile = null)
        {
            sourceFolder    = Path.GetFullPath(sourceFolder);
            var sourceFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
            var hashSet     = new FileHashSet();
            hashSet.Files   = new List<FileHashEntry>(sourceFiles.Length);

            for (var x = 0; x < sourceFiles.Length; x++)
            {
                var sourceFile   = sourceFiles[x];
                var relativePath = Paths.GetRelativePath(sourceFile, sourceFolder);
                reportProgress?.Invoke(relativePath, (double)x / sourceFiles.Length);
                
                // Conditionally ignore file.
                if (shouldIgnoreFile != null && shouldIgnoreFile(relativePath))
                    continue;

                var hash = Hashing.CalculateHash(sourceFile);
                hashSet.Files.Add(new FileHashEntry()
                {
                    Hash = hash,
                    RelativePath = relativePath
                });
            }

            reportProgress?.Invoke("Done", 1);
            hashSet.Directory = sourceFolder;
            return hashSet;
        }

        private static HashSet<string> BuildHashSet(FileHashSet fileSet)
        {
            var allRelativeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in fileSet.Files)
                allRelativeFiles.Add(file.RelativePath);

            return allRelativeFiles;
        }
    }
}
