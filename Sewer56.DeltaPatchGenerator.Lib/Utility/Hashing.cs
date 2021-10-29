using System.IO;
using Standart.Hash.xxHash;

namespace Sewer56.DeltaPatchGenerator.Lib.Utility
{
    public static class Hashing
    {
        /// <summary>
        /// Max buffer size for calculating hashes.
        /// </summary>
        public const int HashBufferSize = 64_000_000; // 64 MB

        /// <summary>
        /// Calculates the hash for a file from a given filepath.
        /// </summary>
        /// <param name="filePath">The filepath.</param>
        /// <param name="seed">The seed to use for hashing.</param>
        public static ulong CalculateHash(string filePath, ulong seed = 0)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return xxHash64.ComputeHash(fileStream, HashBufferSize, seed);
        }
    }
}
