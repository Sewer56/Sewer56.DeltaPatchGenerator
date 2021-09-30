using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sewer56.DeltaPatchGenerator.Lib.Model
{
    public class FileHashSet
    {
        public const string FileName = "hashes.json";

        /// <summary>
        /// The number of files described by this hash set.
        /// </summary>
        public List<FileHashEntry> Files { get; set; } = new List<FileHashEntry>();

        /// <summary>
        /// Directory containing the files depicted in this hash set.
        /// </summary>
        [JsonIgnore]
        public string Directory { get; set; }

        public static FileHashSet FromDirectory(string inputFolder)
        {
            var path    = Path.Combine(inputFolder, FileName);
            var text    = File.ReadAllText(path);
            var hashSet = JsonSerializer.Deserialize<FileHashSet>(text);
            hashSet.Directory = inputFolder;
            return hashSet;
        }

        public void ToDirectory(string outputFolder, out string outputFileName)
        {
            outputFileName = Path.Combine(outputFolder, FileName);
            File.WriteAllText(outputFileName, JsonSerializer.Serialize(this));
        }
    }

    public struct FileHashEntry
    {
        public string RelativePath { get; set; }
        public ulong  Hash { get; set; }
    }
}
