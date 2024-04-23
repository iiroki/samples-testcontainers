namespace Samples.Testcontainers.Models;

public record Item
{
    public required string Key { get; init; }

    public required string Value { get; init; }
}
