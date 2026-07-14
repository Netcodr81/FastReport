using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastReport.Utils
{
    public static partial class ImageHelper
    {
internal static async Task<byte[]> LoadAsync(string fileName, CancellationToken cancellationToken)
{
    if (!String.IsNullOrEmpty(fileName))
        return await File.ReadAllBytesAsync(fileName, cancellationToken);

    return null;
}

internal static Task<byte[]> LoadURLAsync(Uri url, CancellationToken cancellationToken)
{
    return _httpClient.GetByteArrayAsync(url, cancellationToken);
}
    }
}
