using System;
using System.Collections.Generic;
using System.Text;

namespace Sewer56.DeltaPatchGenerator.Lib.Utility
{
    /// <summary>
    /// Helper classes related to throwing exceptions.
    /// </summary>
    public static class ThrowHelpers
    {
        /// <summary>
        /// Throws an exception if the parameter is null or empty.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void ThrowIfNullOrEmpty(string parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
                throw new ArgumentException($"Parameter {parameterName} is null or empty.");
        }

        /// <summary>
        /// Throws an exception for when file checksum verification fails.
        /// </summary>
        /// <param name="blurb">The text specifically tied with this exception.</param>
        /// <param name="missingFiles">List of missing files.</param>
        /// <param name="mismatchFiles">List of files with a wrong checksum.</param>
        public static void ThrowVerificationFailed(string blurb, List<string> missingFiles, List<string> mismatchFiles)
        {
            var message = new StringBuilder();
            message.AppendLine(blurb);

            if (missingFiles.Count > 0)
            {
                message.AppendLine("Missing Files:");
                foreach (var missingFile in missingFiles)
                    message.AppendLine(missingFile);
            }

            if (mismatchFiles.Count > 0)
            {
                message.AppendLine("Mismatched Files:");
                foreach (var mismatchFile in mismatchFiles)
                    message.AppendLine(mismatchFile);
            }

            throw new Exception(message.ToString());
        }
    }
}
