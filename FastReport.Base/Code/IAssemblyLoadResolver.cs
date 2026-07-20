using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace FastReport.Code;

public interface IAssemblyLoadResolver
{
    MetadataReference LoadManagedLibrary(AssemblyName assemblyName);

    Task<MetadataReference> LoadManagedLibraryAsync(AssemblyName assemblyName, CancellationToken ct);
}