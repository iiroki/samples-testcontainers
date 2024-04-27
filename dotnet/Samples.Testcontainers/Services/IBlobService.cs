using System.Text.Json;
using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

/// <summary>
/// Service for saving blobs.
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// JSON serialization options used for saving blobs.
    /// </summary>
    JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// Saves the item to the given container.
    /// </summary>
    Task SaveAsync(string container, Item item, CancellationToken ct = default);
}
