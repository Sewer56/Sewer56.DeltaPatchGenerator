using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Lib.Model;

/// <summary>
/// A structure that encapsulates a set of files to be included in a delta patch.
/// </summary>
public class FileHashSet
{
    /// <summary>
    /// The standard file name used when serializing a hash set to a directory.
    /// </summary>
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

    /// <summary>
    /// Reads a hash set from a given directory.
    /// </summary>
    /// <param name="inputFolder">The folder from which the patch should be read.</param>
    /// <returns>Patched.</returns>
    public static FileHashSet FromDirectory(string inputFolder)
    {
        var path    = Path.Combine(inputFolder, FileName);
        var text    = File.ReadAllText(path);
        var hashSet = JsonSerializer.Deserialize<FileHashSet>(text);
        hashSet.Directory = inputFolder;
        hashSet.Initialise();
        return hashSet;
    }

    /// <summary>
    /// Writes a hash set to a given directory.
    /// </summary>
    /// <param name="outputFolder">The folder where the hash set should be output.</param>
    /// <param name="outputPath">The file path where the hash set was output.</param>
    public void ToDirectory(string outputFolder, out string outputPath)
    {
        outputPath = Path.Combine(outputFolder, FileName);
        File.WriteAllText(outputPath, JsonSerializer.Serialize(this));
    }

    private void Initialise()
    {
        // Patch for OS with forward slash separators.
        if (!Paths.UsesForwardSlashSeparator) 
            return;

        for (int x = 0; x < Files.Count; x++)
        {
            var file = Files[x];
            file.RelativePath = file.RelativePath.UsingForwardSlash();
            Files[x] = file;
        }
    }
}

/// <summary>
/// Represents an individual entry for a file in the set.
/// </summary>
public struct FileHashEntry
{
    /// <summary>
    /// Relative path of file.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// xxHash64 Hash.
    /// </summary>
    public ulong  Hash { get; set; }
}
