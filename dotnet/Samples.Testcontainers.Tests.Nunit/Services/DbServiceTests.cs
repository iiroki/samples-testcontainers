using Samples.Testcontainers.Models;
using Samples.Testcontainers.Services;
using Samples.Testcontainers.Tests.Nunit.Helpers;
using Samples.Testcontainers.Tests.Nunit.Testcontainers;

namespace Samples.Testcontainers.Tests.Nunit.Services;

[TestFixture]
public class DbServiceTests : PostgresTestContext
{
    private IDbService _db = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _db = DbServiceFactory.Create(DbUrl);
        if (!TestContext.CurrentContext.ShouldSkipInit())
        {
            await _db.InitAsync();
        }
    }

    [TearDown]
    public async Task DestroyAsync() => await _db.DisposeAsync();

    [Test]
    [SkipInit]
    public async Task DbService_Init_Ok()
    {
        // Act
        await _db.InitAsync();

        // Assert
        List<string> tables = await GetTablesAsync();
        Assert.That(tables, Has.Count.EqualTo(1));
        Assert.That(tables.First(), Is.EqualTo(_db.Table));
    }

    [Test]
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
        Assert.That(actual, Is.EquivalentTo(items)); // "Equivalent" = Order doesn't matter
    }

    [Test]
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
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
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
        Assert.That(actual, Is.Null);
    }

    [Test]
    public async Task DbService_Insert_New_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "inserted" };
        var wasInserted = await _db.InsertAsync(item);

        // Assert
        Assert.That(wasInserted, Is.True);

        var actual = await GetActualAsync();
        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual.First(), Is.EqualTo(item));
    }

    [Test]
    public async Task DbService_Insert_Duplicate_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "inserted" };
        var wasInserted = await _db.InsertAsync(item);
        var wasDuplicateInserted = await _db.InsertAsync(item);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(wasInserted, Is.True);
            Assert.That(wasDuplicateInserted, Is.False);
        });
    }

    [Test]
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
        Assert.That(wasUpdated, Is.True);

        var actual = await GetActualAsync(original.Key);
        Assert.That(actual, Is.EqualTo(updated));
    }

    [Test]
    public async Task DbService_Update_NonExistent_Ok()
    {
        // Act
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "non-existent" };
        var wasUpdated = await _db.UpdateAsync(item);

        // Assert
        Assert.That(wasUpdated, Is.False);

        var actual = await GetActualAsync();
        Assert.That(actual, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task DbService_Remove_Existing_Ok()
    {
        // Arrange
        var item = new Item { Key = Guid.NewGuid().ToString(), Value = "removed" };
        await SeedAsync(item);

        // Act
        var wasRemoved = await _db.RemoveAsync(item.Key);

        // Assert
        Assert.That(wasRemoved, Is.True);

        var actual = await GetActualAsync();
        Assert.That(actual, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task DbService_Remove_NonExistent_Ok()
    {
        // Act
        var wasRemoved = await _db.RemoveAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.That(wasRemoved, Is.False);
    }

    private async Task<List<Item>> GetActualAsync()
    {
        await using var db = CreateDbSource();
        var cmd = db.CreateCommand($"SELECT * FROM {_db.Table};");
        var reader = await cmd.ExecuteReaderAsync();

        List<Item> actual = [];
        while (await reader.ReadAsync())
        {
            actual.Add(new() { Key = reader.GetString(0), Value = reader.GetString(1) });
        }

        return actual;
    }

    private async Task<Item?> GetActualAsync(string key) => (await GetActualAsync()).FirstOrDefault(i => i.Key == key);

    private async Task SeedAsync(IEnumerable<Item> items)
    {
        await using var db = CreateDbSource();
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
