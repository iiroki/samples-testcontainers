using System.Text.Json;
using Samples.Testcontainers.Models;

namespace Samples.Testcontainers.Services;

internal class CompositeService(IDbService db, IBlobService blob) : ICompositeService
{
    private readonly IDbService _db = db;
    private readonly IBlobService _blob = blob;

    public string Container { get; } = "composite";

    public async Task SaveAsync(Item item, CancellationToken ct = default)
    {
        var wasInserted = await _db.InsertAsync(item, ct);
        if (!wasInserted)
        {
            var wasUpdated = await _db.UpdateAsync(item, ct);
            if (!wasUpdated)
            {
                throw new AggregateException(
                    $"Item was neither inserted or updated to the DB: {JsonSerializer.Serialize(item)}"
                );
            }
        }

        await _blob.SaveAsync(Container, item, ct);
    }
}
