using Azure.Storage.Blobs;

namespace Samples.Testcontainers.Extensions;

public static class BlobExtensions
{
    public static async Task<List<string>> GetContainerNamesAsync(
        this BlobServiceClient service,
        CancellationToken ct = default
    )
    {
        List<string> containers = [];
        await foreach (var container in service.GetBlobContainersAsync(cancellationToken: ct))
        {
            containers.Add(container.Name);
        }

        return containers;
    }
}
