using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

/// <summary>
/// Basic database service that can be tested.
/// </summary>
public interface IDbService : IAsyncDisposable
{
    /// <summary>
    /// Name of the target table.
    /// </summary>
    string Table { get; }

    /// <summary>
    /// Initializes the database by creating the target table if it does not yet exist.
    /// </summary>
    Task InitAsync(bool createTables = false, CancellationToken ct = default);

    Task<List<Item>> GetAsync(CancellationToken ct = default);

    Task<Item?> GetAsync(string key, CancellationToken ct = default);

    Task<bool> InsertAsync(Item item, CancellationToken ct = default);

    Task<bool> UpdateAsync(Item item, CancellationToken ct = default);

    Task<bool> RemoveAsync(string key, CancellationToken ct = default);
}
