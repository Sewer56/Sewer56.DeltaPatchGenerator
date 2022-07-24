using System;
using System.Diagnostics;
using System.IO;
using Sewer56.DeltaPatchGenerator.Lib;
using Sewer56.DeltaPatchGenerator.Lib.Model;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Xunit;
using Xunit.Abstractions;

namespace Sewer56.DeltaPatchGenerator.Tests
{
    public class PatchTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public string PatchFolder = Path.Combine(Assets.TempFolder, "Patch");
        public string ResultFolder = Path.Combine(Assets.TempFolder, "Result");

        public PatchTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            try
            {
                Directory.Delete(Assets.TempFolder, true);
            }
            catch (Exception e)
            {
                /* Ignore */
            }
        }
        
        [Fact]
        public void PatchSingleFile()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.SingleFileFolderTarget);
            var patch  = Patch.Generate(Assets.SingleFileFolderOriginal, Assets.SingleFileFolderTarget, PatchFolder);
            
            // Apply Patch
            Patch.Apply(patch, Assets.SingleFileFolderOriginal, ResultFolder);

            // Verify Files
            Assert.True(HashSet.Verify(hashes, ResultFolder, out var missingFiles, out var hashMismatches));
        }

        [Fact]
        public void PatchManyFiles()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.ManyFileFolderTarget);
            var patch  = Patch.Generate(Assets.ManyFileFolderOriginal, Assets.ManyFileFolderTarget, PatchFolder);

            // Apply Patch
            Patch.Apply(patch, Assets.ManyFileFolderOriginal, ResultFolder);

            // Verify Files
            Assert.True(HashSet.Verify(hashes, ResultFolder, out var missingFiles, out var hashMismatches));
        }

        [Fact]
        public void PatchAddsMissingFiles()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.AddMissingFileFolderTarget);
            var patch  = Patch.Generate(Assets.AddMissingFileFolderOriginal, Assets.AddMissingFileFolderTarget, PatchFolder);

            // Apply Patch
            Patch.Apply(patch, Assets.AddMissingFileFolderOriginal, ResultFolder, null, true);

            // Verify Files
            Assert.True(HashSet.Verify(hashes, ResultFolder, out var missingFiles, out var hashMismatches));
        }

        [Fact]
        public void PatchRemovesExtraFiles()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.AddMissingFileFolderOriginal);

            // Apply Patch
            IOEx.CopyDirectory(Assets.AddMissingFileFolderTarget, ResultFolder);

            // Verify Files
            Assert.True(HashSet.Verify(hashes, ResultFolder, out var missingFiles, out var hashMismatches));

            // Remove extra files.
            int GetFileCount() => Directory.GetFiles(ResultFolder, "*.*", SearchOption.AllDirectories).Length;
            var fileCount = GetFileCount();
            HashSet.Cleanup(hashes, ResultFolder);
            Assert.Equal(fileCount - 1, GetFileCount());
        }

        [Fact]
        public void FindsMissingFiles()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.MissingFileFolderOriginal);

            // Fucking Monikammmmmmmmmmmmmmmm
            Assert.False(HashSet.Verify(hashes, Assets.MissingFileFolderTarget, out var missingFiles, out var hashMismatches));
            Assert.Single(missingFiles);
            Assert.Empty(hashMismatches);
        }

        [Fact]
        public void FindsHashMismatch()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.MismatchFolderOriginal);

            // Fucking Monikammmmmmmmmmmmmmmm
            Assert.False(HashSet.Verify(hashes, Assets.MismatchFolderTarget, out var missingFiles, out var hashMismatches));
            Assert.Single(hashMismatches);
            Assert.Empty(missingFiles);
        }

        [Fact]
        public void PatchWithDuplicateHashes()
        {
            // Make Hashes and Patch
            var hashes = HashSet.Generate(Assets.DuplicateHashesTarget);
            var patch = Patch.Generate(Assets.DuplicateHashesOriginal, Assets.DuplicateHashesTarget, PatchFolder);

            // Serialize and de-serialize to reset internal state
            using var tempDir = new TemporaryFolderAllocation();
            patch.ToDirectory(tempDir.FolderPath, out var filePath);
            patch = PatchData.FromDirectory(tempDir.FolderPath);

            // Apply Patch
            Patch.Apply(patch, Assets.DuplicateHashesOriginal, ResultFolder);

            // Verify Files
            Assert.True(HashSet.Verify(hashes, ResultFolder, out var missingFiles, out var hashMismatches));
        }
    }
}
