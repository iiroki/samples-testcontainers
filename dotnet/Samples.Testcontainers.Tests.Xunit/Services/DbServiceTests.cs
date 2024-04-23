using Samples.Testcontainers.Models;
using Samples.Testcontainers.Services;
using Samples.Testcontainers.Tests.Xunit.Testcontainers;

namespace Samples.Testcontainers.Tests.XUnit.Services;

public sealed class DbServiceTests : IClassFixture<PostgresTestFixture>, IDisposable
{
    private readonly PostgresTestFixture _dbFixture;
    private readonly DbService _db = null!;

    public DbServiceTests(PostgresTestFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _db = new DbService(_dbFixture.DbUrl);
        _db.InitAsync().Wait(); // Run initialization by default
    }

    public void Dispose() => _dbFixture.CleanDbAsync().Wait();

    [Fact]
    public async Task DbService_Init_Ok()
    {
        // Arrange
        await _dbFixture.CleanDbAsync();

        // Act
        await _db.InitAsync();

        // Assert
        List<string> tables = await _dbFixture.GetTablesAsync();
        Assert.Single(tables);
        Assert.Equal(_db.Table, tables.First());
    }

    [Fact]
    public async Task DbService_Get_Many_Ok()
    {
        // Arrange
        var items = Enumerable
            .Range(1, 100)
            .Select(i => new Item { Key = Guid.NewGuid().ToString(), Value = $"value-{i}" })
            .ToList();

        await SeedAsync(items);

        // Act
        var actual = await _db.GetAsync();

        // Assert
        Assert.Equivalent(items, actual); // "Equivalent" = Order doesn't matter
    }

    [Fact]
    public async Task DbService_Get_One_Existing_Ok()
    {
        // Arrange
        var items = Enumerable
            .Range(1, 100)
            .Select(i => new Item { Key = Guid.NewGuid().ToString(), Value = $"value-{i}" })
            .ToList();

        await SeedAsync(items);

        // Act
        var expected = items[items.Count / 2];
        var actual = await _db.GetAsync(expected.Key);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task DbService_Get_One_NonExistent_Ok()
    {
        // Arrange
        var items = Enumerable
            .Range(1, 100)
            .Select(i => new Item { Key = Guid.NewGuid().ToString(), Value = $"value-{i}" })
            .ToList();

        await SeedAsync(items);

        // Act
        var actual = await _db.GetAsync(Guid.NewGuid().ToString()); // The key shouldn't match to anything

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task DbService_Insert_New_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "inserted" };
        var wasInserted = await _db.InsertAsync(item);

        // Assert
        Assert.True(wasInserted);

        var actual = await GetActualAsync();
        Assert.Single(actual);
        Assert.Equal(item, actual.First());
    }

    private async Task<List<Item>> GetActualAsync()
    {
        await using var db = _dbFixture.CreateDbSource();
        var cmd = db.CreateCommand($"SELECT * FROM {_db.Table};");
        var reader = await cmd.ExecuteReaderAsync();

        List<Item> actual = [];
        while (await reader.ReadAsync())
        {
            actual.Add(new() { Key = reader.GetString(0), Value = reader.GetString(1) });
        }

        return actual;
    }

    [Fact]
    public async Task DbService_Insert_Duplicate_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "inserted" };
        var wasInserted = await _db.InsertAsync(item);
        var wasDuplicateInserted = await _db.InsertAsync(item);

        // Assert
        Assert.Multiple(() => Assert.True(wasInserted), () => Assert.False(wasDuplicateInserted));
    }

    [Fact]
    public async Task DbService_Update_Existing_Ok()
    {
        // Arrange
        var original = new Item { Key = Guid.NewGuid().ToString(), Value = "original" };
        await SeedAsync(original);

        // Act
        var updated = original with
        {
            Value = "updated"
        };

        var wasUpdated = await _db.UpdateAsync(updated);

        // Assert
        Assert.True(wasUpdated);

        var actual = await GetActualAsync(original.Key);
        Assert.Equal(updated, actual);
    }

    [Fact]
    public async Task DbService_Update_NonExistent_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "non-existent" };
        var wasUpdated = await _db.UpdateAsync(item);

        // Assert
        Assert.False(wasUpdated);

        var actual = await GetActualAsync();
        Assert.Empty(actual);
    }

    [Fact]
    public async Task DbService_Remove_Existing_Ok()
    {
        // Arrange
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "removed" };
        await SeedAsync(item);

        // Act
        var wasRemoved = await _db.RemoveAsync(item.Key);

        // Assert
        Assert.True(wasRemoved);

        var actual = await GetActualAsync();
        Assert.Empty(actual);
    }

    [Fact]
    public async Task DbService_Remove_NonExistent_Ok()
    {
        // Act
        var wasRemoved = await _db.RemoveAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.False(wasRemoved);
    }

    private async Task<Item?> GetActualAsync(string key) => (await GetActualAsync()).FirstOrDefault(i => i.Key == key);

    private async Task SeedAsync(IEnumerable<Item> items)
    {
        await using var db = _dbFixture.CreateDbSource();
        foreach (var i in items)
        {
            var cmd = db.CreateCommand($"INSERT INTO {_db.Table} VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(i.Key);
            cmd.Parameters.AddWithValue(i.Value);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task SeedAsync(Item item) => await SeedAsync([item]);
}
