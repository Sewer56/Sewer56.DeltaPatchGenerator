using System;
using System.IO;
using System.Reflection;

namespace Sewer56.DeltaPatchGenerator.Lib.Utility
{
    /// <summary>
    /// File path related utilities.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// The location where the current program is located.
        /// </summary>
        public static readonly string ProgramFolder = Path.GetDirectoryName(new Uri(AppContext.BaseDirectory).LocalPath);

        /// <summary>
        /// Retrieves a relative path for a file given a folder name.
        /// </summary>
        /// <param name="fullPath">Full path for the file.</param>
        /// <param name="folderPath">The folder to get the path relative to.</param>
        public static string GetRelativePath(string fullPath, string folderPath) => fullPath.Substring(folderPath.Length + 1);

        /// <summary>
        /// Appends a relative path to a given folder.
        /// </summary>
        /// <param name="relativePath">Relative path to the folder.</param>
        /// <param name="folderPath">Path to the folder.</param>
        public static string AppendRelativePath(string relativePath, string folderPath) => folderPath + "/" + relativePath;

        /// <summary>
        /// True if uses *nix path separator.
        /// </summary>
        public static bool UsesForwardSlashSeparator = Path.DirectorySeparatorChar == '/';

        /// <summary>
        /// Replaces the path separator if required.
        /// </summary>
        /// <param name="path">The path where the separator should be replaced.</param>
        /// <returns>Same or new path.</returns>
        public static string UsingForwardSlashIfNecessary(this string path)
        {
            return UsesForwardSlashSeparator ? path.UsingForwardSlash() : path;
        }

        /// <summary>
        /// Replaces the path separator from a backslash to a forward slash.
        /// </summary>
        /// <param name="path">The path where the separator should be replaced.</param>
        /// <returns>Path with backslashes separated with forward.</returns>
        public static string UsingForwardSlash(this string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
