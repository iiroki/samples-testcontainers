using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Samples.Testcontainers.Extensions;

namespace Samples.Testcontainers.Tests.Xunit.Testcontainers;

public sealed class AzuriteTestFixture : TestcontainersFixture
{
    private const string AzuriteVersion = "3.30.0"; // = Docker tag
    private const int BlobPort = 10000; // 10000 is the default Blob Storage port
    private BlobServiceClient? _cached;

    /// <summary>
    /// Default Azurite HTTP connection string with a custom port.
    /// </summary>
    public string Connection =>
        string.Join(
            ';',
            "DefaultEndpointsProtocol=http",
            "AccountName=devstoreaccount1",
            "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==",
            $"BlobEndpoint=http://127.0.0.1:{Container.GetMappedPublicPort(BlobPort)}/devstoreaccount1"
        );

    /// <summary>
    /// API version 2023-11-03 is the latest one compatible with Azurite 3.30.0.
    /// </summary>
    public BlobClientOptions.ServiceVersion ApiVersion = BlobClientOptions.ServiceVersion.V2023_11_03;

    // Returns the cached client or creates it if not already cached.
    public BlobServiceClient Service => _cached ??= new(Connection, new(ApiVersion));

    protected override IContainer CreateContainer() =>
        new ContainerBuilder()
            .WithImage($"mcr.microsoft.com/azure-storage/azurite:{AzuriteVersion}")
            .WithPortBinding(BlobPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(BlobPort))
            .Build();

    public async Task CleanBlobStorageAsync(CancellationToken ct = default)
    {
        var containers = await Service.GetContainerNamesAsync(ct);
        var tasks = containers.Select(c => Service.DeleteBlobContainerAsync(c)).ToList();
        await Task.WhenAll(tasks);
    }

    public async Task<List<BlobItem>> GetBlobsAsync(string container, CancellationToken ct = default)
    {
        var containers = await Service.GetContainerNamesAsync(ct);

        List<BlobItem> blobs = [];
        foreach (var c in containers)
        {
            await foreach (var b in Service.GetBlobContainerClient(c).GetBlobsAsync(cancellationToken: ct))
            {
                blobs.Add(b);
            }
        }

        return blobs;
    }

    public async Task<BinaryData> GetBlobDataAsync(string container, string blob, CancellationToken ct = default)
    {
        var blobClient = Service.GetBlobContainerClient(container).GetBlobClient(blob);
        return (await blobClient.DownloadContentAsync(ct)).Value.Content;
    }
}
