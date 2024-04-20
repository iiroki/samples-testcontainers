using Npgsql;

namespace Samples.Testcontainers.Services;

internal class DbService(string dbUrl) : IDbService
{
    private readonly NpgsqlDataSource _db = NpgsqlDataSource.Create(dbUrl);

    public Task InitAsync(bool createTables = false, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync() => _db.DisposeAsync();

    public Task<bool> InsertAsync(string key, string value, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(string key, string value, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
