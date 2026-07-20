using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FastReport.Utils;

public static partial class ImageHelper
{
#pragma warning disable CS1998
    internal static async Task<byte[]> LoadAsync(string fileName, CancellationToken cancellationToken)
#pragma warning restore CS1998
    {
        if (!String.IsNullOrEmpty(fileName))
            return await File.ReadAllBytesAsync(fileName, cancellationToken);
        return null;
    }

    internal static async Task<byte[]> LoadURLAsync(Uri url, CancellationToken cancellationToken)
    {
        using (var httpClient = new HttpClient())
        {
            return await httpClient.GetByteArrayAsync(url, cancellationToken);
        }
    }
}