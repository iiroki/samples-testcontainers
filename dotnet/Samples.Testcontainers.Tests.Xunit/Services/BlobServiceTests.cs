using System.Text.Json;
using Samples.Testcontainers.Models;
using Samples.Testcontainers.Services;
using Samples.Testcontainers.Tests.Xunit.Testcontainers;

namespace Samples.Testcontainers.Tests.Xunit.Services;

public sealed class BlobServiceTests : IClassFixture<AzuriteTestFixture>, IDisposable
{
    private readonly AzuriteTestFixture _blobFixture;
    private readonly BlobService _blob;

    public BlobServiceTests(AzuriteTestFixture blobFixture)
    {
        _blobFixture = blobFixture;
        _blob = new(_blobFixture.Connection, _blobFixture.ApiVersion);
    }

    public void Dispose() => _blobFixture.CleanBlobStorageAsync().Wait();

    [Fact]
    public async Task BlobService_Save_Ok()
    {
        // Arrange
        var container = Guid.NewGuid().ToString();
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "blob" };

        // Act
        await _blob.SaveAsync(container, item);

        // Assert - Blob count
        var blobs = await _blobFixture.GetBlobsAsync(container);
        Assert.Single(blobs);
        var blob = blobs.First();

        // Assert - Blob name
        Assert.StartsWith(item.Key, blob.Name);

        // Assert - Blob data
        var data = await _blobFixture.GetBlobDataAsync(container, blob.Name);
        var expected = JsonSerializer.Serialize(item, _blob.JsonOptions);
        var actual = data.ToString();
        Assert.Equal(actual, expected);
    }
}
