using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            patch.Directory = inputFolder;
            patch.PopulatePathSet();

            return patch;
        }

        public void ToDirectory(string outputFolder, out string outputFilePath)
        {
            outputFilePath = Path.Combine(outputFolder, FileName);
            File.WriteAllText(outputFilePath, JsonSerializer.Serialize(this));
        }

        public void Add(ulong hash, string path)
        {
            HashToPatchDictionary[hash] = path;
            FilePathSet.Add(path);
        }

        private void PopulatePathSet()
        {
            foreach (var item in HashToPatchDictionary) 
                FilePathSet.Add(item.Value);
        }
    }
}
