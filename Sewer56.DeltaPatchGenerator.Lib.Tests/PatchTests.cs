using System;
using System.IO;
using Sewer56.DeltaPatchGenerator.Lib;
using Xunit;

namespace Sewer56.DeltaPatchGenerator.Tests
{
    public class PatchTests
    {
        public string PatchFolder = Path.Combine(Assets.TempFolder, "Patch");
        public string ResultFolder = Path.Combine(Assets.TempFolder, "Result");

        public PatchTests()
        {
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
    }
}
