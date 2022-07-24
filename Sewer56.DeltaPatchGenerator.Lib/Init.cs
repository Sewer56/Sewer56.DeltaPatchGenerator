#if NET5_0_OR_GREATER
using Sewer56.DeltaPatchGenerator.Lib.Model;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sewer56.DeltaPatchGenerator.Lib;

internal class Init
{
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    public static void Initialize()
    {
        PreserveMe<PatchData>();
        PreserveMe<FileHashSet>();
    }

    /// <summary>
    /// Dummy method used for telling the IL Linker to preserve the type in its entirety.
    /// </summary>
    /// <typeparam name="T">Type to preserve all implementation for.</typeparam>
    public static void PreserveMe<
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        T>() { }
}
#endif