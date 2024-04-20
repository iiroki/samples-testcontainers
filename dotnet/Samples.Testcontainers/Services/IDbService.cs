namespace Samples.Testcontainers.Services;

/// <summary>
/// Basic database service that can be tested.
/// </summary>
public interface IDbService : IAsyncDisposable
{
    Task InitAsync(bool createTables = false, CancellationToken ct = default);

    Task<bool> InsertAsync(string key, string value, CancellationToken ct = default);

    Task<bool> UpdateAsync(string key, string value, CancellationToken ct = default);

    Task<bool> RemoveAsync(string key, CancellationToken ct = default);
}
