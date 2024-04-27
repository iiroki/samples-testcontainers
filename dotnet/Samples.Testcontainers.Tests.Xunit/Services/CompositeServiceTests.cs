using Samples.Testcontainers.Models;
using Samples.Testcontainers.Services;
using Samples.Testcontainers.Tests.Xunit.Testcontainers;

namespace Samples.Testcontainers.Tests.Xunit.Services;

public sealed class CompositeServiceTests
    : IClassFixture<PostgresTestFixture>,
        IClassFixture<AzuriteTestFixture>,
        IDisposable
{
    private readonly PostgresTestFixture _dbFixture;
    private readonly AzuriteTestFixture _blobFixture;
    private readonly CompositeService _composite;
    private readonly DbService _db;
    private readonly BlobService _blob;

    public CompositeServiceTests(PostgresTestFixture dbFixture, AzuriteTestFixture blobFixture)
    {
        _dbFixture = dbFixture;
        _blobFixture = blobFixture;

        _db = new(_dbFixture.DbUrl);
        _blob = new(_blobFixture.Connection, _blobFixture.ApiVersion);
        _composite = new(_db, _blob);

        _db.InitAsync().Wait();
    }

    public void Dispose() => Task.WaitAll(_dbFixture.CleanDbAsync(), _blobFixture.CleanBlobStorageAsync());

    [Fact]
    public async Task CompositeService_Save_Ok()
    {
        // Arrange
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "composite" };

        // Act
        await _composite.SaveAsync(item);

        // Assert - Database
        var actualDb = await _db.GetAsync();
        Assert.Single(actualDb);

        // Assert - Blob
        var actualBlob = await _blobFixture.GetBlobsAsync(_composite.Container);
        Assert.Single(actualBlob);
    }
}
