using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

internal class BlobService(
    string connection,
    BlobClientOptions.ServiceVersion version = BlobClientOptions.ServiceVersion.V2023_11_03
) : IBlobService
{
    private static readonly JsonSerializerOptions InternalJsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly BlobServiceClient _service = new(connection, new BlobClientOptions(version));

    public JsonSerializerOptions JsonOptions => InternalJsonOptions;

    public async Task SaveAsync(string container, Item item, CancellationToken ct = default)
    {
        var containerClient = _service.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobName = GetBlobName(item);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobContent = GetBlobContent(item);
        await blobClient.UploadAsync(blobContent, ct);
    }

    private static string GetBlobName(Item item) => $"{item.Key}.json";

    private BinaryData GetBlobContent(object obj) => new(Encoding.UTF8.GetBytes(JsonStringify(obj)));

    private string JsonStringify(object obj) => JsonSerializer.Serialize(obj, JsonOptions);
}
