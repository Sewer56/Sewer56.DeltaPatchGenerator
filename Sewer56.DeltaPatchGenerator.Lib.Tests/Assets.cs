﻿using System.IO;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Tests
{
    public static class Assets
    {
        public static readonly string AssetsFolder = Path.Combine(Paths.ProgramFolder, "Assets");

        public static readonly string SingleFileFolder = Path.Combine(AssetsFolder, "Single File Patch Test");
        public static readonly string SingleFileFolderOriginal = Path.Combine(SingleFileFolder, "Original");
        public static readonly string SingleFileFolderTarget = Path.Combine(SingleFileFolder, "Target");

        public static readonly string ManyFileFolder = Path.Combine(AssetsFolder, "Many File Patch Test");
        public static readonly string ManyFileFolderOriginal = Path.Combine(ManyFileFolder, "Original");
        public static readonly string ManyFileFolderTarget = Path.Combine(ManyFileFolder, "Target");

        public static readonly string MissingFileFolder = Path.Combine(AssetsFolder, "Missing File Test");
        public static readonly string MissingFileFolderOriginal = Path.Combine(MissingFileFolder, "Original");
        public static readonly string MissingFileFolderTarget = Path.Combine(MissingFileFolder, "New");

        public static readonly string MismatchFolder = Path.Combine(AssetsFolder, "Mismatch Test");
        public static readonly string MismatchFolderOriginal = Path.Combine(MismatchFolder, "Original");
        public static readonly string MismatchFolderTarget = Path.Combine(MismatchFolder, "Target");

        public static readonly string TempFolder = Path.Combine(Paths.ProgramFolder, "Temp");

        static Assets()
        {
            Directory.CreateDirectory(TempFolder);
        }
    }
}
