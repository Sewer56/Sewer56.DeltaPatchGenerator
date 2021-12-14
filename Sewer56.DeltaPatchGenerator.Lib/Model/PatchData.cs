using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Lib.Model
{
    public class PatchData
    {
        public const string FileName = "patch.json";

        /// <summary>
        /// Dictionary matching individual patches to hashes.
        /// </summary>
        public Dictionary<ulong, string> HashToPatchDictionary { get; set; } = new Dictionary<ulong, string>();

        /// <summary>
        /// Contains a set of all files to be added to patch from old to new.
        /// </summary>
        public HashSet<string> AddedFilesSet { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Contains a set of all file paths available in this patch.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> FilePathSet { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Directory containing the files depicted in this hash set.
        /// </summary>
        [JsonIgnore]
        public string Directory { get; set; }

        /// <summary>
        /// Gets patches from a directory containing a number of directories, each of which is a patch.
        /// </summary>
        public static List<PatchData> FromDirectories(string patchesDir)
        {
            var directories = System.IO.Directory.GetDirectories(patchesDir);
            var patches = new List<PatchData>();

            foreach (var directory in directories)
                patches.Add(FromDirectory(directory));

            return patches;
        }

        public static PatchData FromDirectory(string inputFolder)
        {
            var path  = Path.Combine(inputFolder, FileName);
            var text  = File.ReadAllText(path);
            var patch = JsonSerializer.Deserialize<PatchData>(text);
            patch.Initialize(inputFolder);

            return patch;
        }

        public void ToDirectory(string outputFolder, out string outputFilePath)
        {
            outputFilePath = Path.Combine(outputFolder, FileName);
            File.WriteAllText(outputFilePath, JsonSerializer.Serialize(this));
        }

        public void AddPatchFile(ulong hash, string path)
        {
            HashToPatchDictionary[hash] = path;
            FilePathSet.Add(path);
        }

        public void AddNewFile(string path)
        {
            AddedFilesSet.Add(path);
            FilePathSet.Add(path);
        }

        public void Initialize(string inputFolder)
        {
            Directory = inputFolder;
            if (FilePathSet.Count != 0) 
                return;

            // Linux & OSX. Enforce forward slash if patch was made on Windows.
            if (Paths.UsesForwardSlashSeparator)
            {
                foreach (var dictEntry in HashToPatchDictionary.ToArray())
                    HashToPatchDictionary[dictEntry.Key] = dictEntry.Value.UsingForwardSlashIfNecessary();

                var allAddedFiles = AddedFilesSet.ToArray();
                foreach (var addedFile in allAddedFiles)
                {
                    AddedFilesSet.Remove(addedFile);
                    AddedFilesSet.Add(addedFile.UsingForwardSlashIfNecessary());
                }
            }

            // Add to file path set.
            foreach (var item in HashToPatchDictionary)
                FilePathSet.Add(item.Value);

            foreach (var item in AddedFilesSet)
                FilePathSet.Add(item);
        }

        public FileHashSet ToFileHashSet()
        {
            var result = new FileHashSet();
            foreach (var dictItem in HashToPatchDictionary)
            {
                result.Files.Add(new FileHashEntry()
                {
                    Hash = dictItem.Key,
                    RelativePath = dictItem.Value
                });
            }

            return result;
        }
    }
}
