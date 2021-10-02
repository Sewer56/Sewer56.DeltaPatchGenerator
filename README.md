# Delta Patch Generator Lib by Sewer56

This is an easy to comsume, fast library for generating Delta Patches.
Note: API is folder based. All operations act on folders and effect all files inside.
## Basic Usage: Verify Files

Generate Hash Set:
```csharp
// Generate a set of hashes for a directory.
var hashes = HashSet.Generate(Assets.MismatchFolderOriginal);
```

Verify:
```csharp
// missingFiles: Files missing from target folder.
// hashMismatches: Files with a non-matching hash.
bool matches = HashSet.Verify(hashes, targetFolder, out var missingFiles, out var hashMismatches)
```

## Basic Usage: Generate Patch

Generate:
```csharp
// Create a patch that transforms one folder into another.
// Save it into `patchFolder`
var patch  = Patch.Generate(originalFolder, targetFolder, patchFolder);
```

Apply:
```csharp
// Patches all files in folderToPatch. Patched files are moved to patchedFilesOutput.
Patch.Apply(patch, folderToPatch, patchedFilesOutput);
```

- Patches use VCDiff based Delta Compression using an optimized port of Google `open-vcdiff`.


## Basic Usage: Cleanup

Delta generator can also be used to remove any files outside of a provided list of files.

```csharp
// Make Hashes
var hashes = HashSet.Generate(Assets.AddMissingFileFolderOriginal);

// Remove extra files.
HashSet.Cleanup(hashes, folder);
```

You can supply an event/function that can be used to confirm whether a file can be deleted.

## Basic Usage: General

Results of most operations (e.g. `Patch.Generate()`, `HashSet.Generate()`) and be saved and loaded from disk.

## Limitations
- RAM: Memory usage explodes relative to file size. When creating a patch for a 1.5GB file, expect 6GB+ RAM usage. Applying patches is pretty much free though.

- File Size: Max file size for individual file is 2GB.

## Additional Credits

### VCDiff Implementation
- Folks at Google for creating open-vcdiff.
- Metric for porting open-vcdiff to .NET.
- SnowflakePowered (Ronny Chan) for optimizing the .NET port of open-vcdiff.

Additional optimisations by myself :)

### Hashing Implementation
- Yann Collet for creating the `xxHash` hashing algorithm.
- Melnik Alexander for high performance port of `xxHash`.

### Doki Doki Literature Club
- A Product of Team Salvato.
