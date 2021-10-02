using System;
using System.IO;
using VCDiff.Decoders;
using VCDiff.Encoders;
using VCDiff.Includes;

namespace Sewer56.DeltaPatchGenerator.Lib.Utility
{
    /// <summary>
    /// Functions used for generating and applying VCDiff based patches.
    /// </summary>
    public class VCDiff
    {
        public const int EncodeBufMiB = 16;

        /// <summary>
        /// Applies an vcdiff patch to a file.
        /// </summary>
        public static void Apply(ApplyOptions options)
        {
            // Validate Parameters
            ThrowHelpers.ThrowIfNullOrEmpty(options.Source, nameof(options.Source));
            ThrowHelpers.ThrowIfNullOrEmpty(options.Patch, nameof(options.Patch));
            ThrowHelpers.ThrowIfNullOrEmpty(options.Output, nameof(options.Output));

            using FileStream output = new FileStream(options.Output, FileMode.Create, FileAccess.Write);
            using FileStream source = new FileStream(options.Source, FileMode.Open, FileAccess.Read);
            using FileStream patch  = new FileStream(options.Patch, FileMode.Open, FileAccess.Read);

            using var decoder = new VcDecoder(source, patch, output, int.MaxValue);
            var result = decoder.Decode(out var bytesWritten);
            if (result != VCDiffResult.SUCCESS)
                throw new Exception("Failed to perform VCDiff Decoding.");
        }

        /// <summary>
        /// Creates a new vcdiff patch.
        /// </summary>
        public static void Compress(CompressOptions options)
        {
            // Validate Parameters
            ThrowHelpers.ThrowIfNullOrEmpty(options.Source, nameof(options.Source));
            ThrowHelpers.ThrowIfNullOrEmpty(options.Target, nameof(options.Target));
            ThrowHelpers.ThrowIfNullOrEmpty(options.Output, nameof(options.Output));

            // Create delta.
            using FileStream output = new FileStream(options.Output, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using FileStream source = new FileStream(options.Source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using FileStream target = new FileStream(options.Target, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using var coder = new VcEncoder(source, target, output, EncodeBufMiB, 32);
            var result = coder.Encode();
            if (result != VCDiffResult.SUCCESS)
                throw new Exception("Failed to perform VCDiff Encoding.");
        }
    }

    public class CompressOptions
    {
        public string Source;
        public string Target;
        public string Output;
    }

    public class ApplyOptions
    {
        public string Source;
        public string Patch;
        public string Output;
    }
}
