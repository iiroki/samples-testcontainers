using Npgsql;
using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

internal class DbService(string dbUrl) : IDbService
{
    private readonly NpgsqlDataSource _db = NpgsqlDataSource.Create(dbUrl);

    public string Table { get; } = "data";

    public ValueTask DisposeAsync() => _db.DisposeAsync();

    public async Task InitAsync(bool createTables = false, CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand(
            $"""
            CREATE TABLE IF NOT EXISTS {Table} (
                key VARCHAR(255) PRIMARY KEY,
                value TEXT
            );
            """
        );

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<Item>> GetAsync(CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand($"SELECT * FROM {Table};");
        var reader = await cmd.ExecuteReaderAsync(ct);

        List<Item> items = [];
        while (await reader.ReadAsync(ct))
        {
            items.Add(new() { Key = reader.GetString(0), Value = reader.GetString(1) });
        }

        return items;
    }

    public async Task<Item?> GetAsync(string key, CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand($"SELECT * FROM {Table} WHERE key = $1;");
        cmd.Parameters.AddWithValue(key);
        var reader = await cmd.ExecuteReaderAsync(ct);

        Item? item = null;
        while (await reader.ReadAsync(ct))
        {
            item = new() { Key = reader.GetString(0), Value = reader.GetString(1) };
        }

        return item;
    }

    public async Task<bool> InsertAsync(Item item, CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand(
            $"""
            INSERT INTO {Table}
            VALUES ($1, $2)
            ON CONFLICT DO NOTHING;
            """
        );

        cmd.Parameters.AddWithValue(item.Key);
        cmd.Parameters.AddWithValue(item.Value);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> UpdateAsync(Item item, CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand(
            $"""
            UPDATE {Table}
            SET value = $2
            WHERE key = $1;
            """
        );

        cmd.Parameters.AddWithValue(item.Key);
        cmd.Parameters.AddWithValue(item.Value);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        var cmd = _db.CreateCommand($"DELETE FROM {Table} WHERE key = $1;");
        cmd.Parameters.AddWithValue(key);
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
