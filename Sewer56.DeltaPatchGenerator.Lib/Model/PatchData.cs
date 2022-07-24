using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sewer56.DeltaPatchGenerator.Lib.Utility;

namespace Sewer56.DeltaPatchGenerator.Lib.Model;

/// <summary>
/// Stores information for patching specific directories.
/// </summary>
public class PatchData
{
    private static JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    /// <summary>
    /// Standard filename used for patch data.
    /// </summary>
    public const string FileName = "patch.json";

    /// <summary>
    /// Dictionary matching individual patches to hashes.
    /// </summary>
    public Dictionary<ulong, string> HashToPatchDictionary { get; set; } = new Dictionary<ulong, string>();

    /// <summary>
    /// Dictionary containing additional patches for a given hash not specified in <see cref="HashToPatchDictionary"/>.  
    /// </summary>
    /// <remarks>
    ///     This property exists for backwards compatibility and space saving reasons.
    ///     Backwards compatibility, as to not break older library versions expecting a mapping to string in <see cref="HashToPatchDictionary"/>.
    ///     Space saving, as multiple files for single hash is uncommon, and wastes space in resulting JSON file.
    /// </remarks>
    public Dictionary<ulong, List<string>> DuplicateHashToPatchDictionary { get; set; } = new Dictionary<ulong, List<string>>();

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

    /// <summary>
    /// Reads a patch file from a given directory.
    /// </summary>
    /// <param name="inputFolder">The folder containing the patch file.</param>
    /// <returns>The patch information.</returns>
    public static PatchData FromDirectory(string inputFolder)
    {
        var path  = Path.Combine(inputFolder, FileName);
        var text  = File.ReadAllText(path);
        var patch = JsonSerializer.Deserialize<PatchData>(text, _options);
        patch!.Initialize(inputFolder);

        return patch;
    }

    /// <summary>
    /// Writes a patch file to a given directory.
    /// </summary>
    /// <param name="outputFolder">The folder where the patch data should be saved.</param>
    /// <param name="outputFilePath">The file path where.</param>
    public void ToDirectory(string outputFolder, out string outputFilePath)
    {
        outputFilePath = Path.Combine(outputFolder, FileName);
        var originalDuplicatePatch = DuplicateHashToPatchDictionary;
        try
        {
            // Set duplicate patches to null if not present
            // (common case), to avoid needless serialization.
            if (DuplicateHashToPatchDictionary.Count <= 0)
                DuplicateHashToPatchDictionary = null;

            File.WriteAllText(outputFilePath, JsonSerializer.Serialize(this, _options));
        }
        finally
        {
            DuplicateHashToPatchDictionary = originalDuplicatePatch;
        }
    }

    /// <summary>
    /// Adds a file for patching post process.
    /// </summary>
    /// <param name="hash">The XXH64 hash of the file.</param>
    /// <param name="path">Relative path of the file.</param>
    public void AddPatchFile(ulong hash, string path)
    {
        if (HashToPatchDictionary.ContainsKey(hash))
            GetOrCreateDuplicateDictionaryList(hash).Add(path);
        else
            HashToPatchDictionary[hash] = path;

        FilePathSet.Add(path);
    }

    /// <summary>
    /// Adds a new file to be tracked by the patch data.
    /// </summary>
    /// <param name="path">Relative path of the file.</param>
    public void AddNewFile(string path)
    {
        AddedFilesSet.Add(path);
        FilePathSet.Add(path);
    }

    /// <summary>
    /// Performs post processing operations on a deserialized instance of the class.
    /// i.e. Fills in fields that can be computed from other fields.
    /// </summary>
    /// <param name="inputFolder">The directory on disk where this patch is stored.</param>
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

        foreach (var item in DuplicateHashToPatchDictionary)
        foreach (var file in item.Value)
            FilePathSet.Add(file);

        foreach (var item in AddedFilesSet)
            FilePathSet.Add(item);
    }

    /// <summary>
    /// [Utility Function] Converts all known patch files in this patch into a <see cref="FileHashSet"/>.  
    /// </summary>
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

    private List<string> GetOrCreateDuplicateDictionaryList(ulong hash)
    {
        if (!DuplicateHashToPatchDictionary.TryGetValue(hash, out var list))
        {
            list = new List<string>();
            DuplicateHashToPatchDictionary[hash] = list;
        }

        return list;
    }
}
