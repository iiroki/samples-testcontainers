using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

/// <summary>
/// Service for saving data to multiple storages.<br />
/// <br />
/// For demo purposes, the service implementation should
/// depend on multiple externals dependencies.
/// </summary>
public interface ICompositeService
{
    string Container { get; }

    /// <summary>
    /// Saves the item to database and Blob Storage.
    /// </summary>
    Task SaveAsync(Item item, CancellationToken ct = default);
}
