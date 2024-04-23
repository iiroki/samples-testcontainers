using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;

namespace Samples.Testcontainers.Tests.Xunit.Testcontainers;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    private const int PostgresPort = 5432; // 5432 is the default Postgres port

    public const string DbName = "_testcontainers";
    public const string DbPassword = "Passw0rd!";

    private readonly IContainer DbContainer = new ContainerBuilder()
        .WithImage("postgres:16")
        .WithEnvironment("POSTGRES_DB", DbName)
        .WithEnvironment("POSTGRES_PASSWORD", DbPassword)
        .WithPortBinding(PostgresPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PostgresPort))
        .Build();

    public string DbUrl =>
        string.Join(
            ';',
            $"Host=localhost:{DbContainer.GetMappedPublicPort(PostgresPort)}",
            $"Database={DbName}",
            "Username=postgres",
            $"Password={DbPassword}",
            "Pooling=false"
        );

    public async Task InitializeAsync() => await StartDbContainerAsync();

    public async Task DisposeAsync()
    {
        await StopDbContainerAsync();
        await DbContainer.DisposeAsync();
    }

    // <summary>
    /// Starts the container if it's not running.
    /// Use this to restart the container in the middle of tests, if needed.
    /// </summary>
    public async Task StartDbContainerAsync()
    {
        if (DbContainer.State is TestcontainersStates.Undefined or TestcontainersStates.Paused)
        {
            await DbContainer.StartAsync();
        }
    }

    /// <summary>
    /// Cleans the database after each test case by dropping all the created tables.
    /// </summary>
    /// <returns></returns>
    public async Task CleanDbAsync()
    {
        var tables = await GetTablesAsync();

        await using var db = CreateDbSource();
        foreach (var table in tables)
        {
            var cmd = db.CreateCommand($"DROP TABLE {table};");
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Stops the DB container.
    /// Use this to stop the container in the middle of tests, if needed.
    /// </summary>
    public async Task StopDbContainerAsync() => await DbContainer.StopAsync();

    /// <summary>
    ///  Creates Npgsql data source that can be used for validation.
    /// </summary>
    public NpgsqlDataSource CreateDbSource() => NpgsqlDataSource.Create(DbUrl);

    /// <summary>
    /// Gets table names from the database.
    /// </summary>
    public async Task<List<string>> GetTablesAsync()
    {
        await using var db = CreateDbSource();
        var cmd = db.CreateCommand(
            """
            SELECT tablename
            FROM pg_catalog.pg_tables
            WHERE schemaname != 'pg_catalog' AND  schemaname != 'information_schema';
            """
        );

        var reader = await cmd.ExecuteReaderAsync();

        List<string> tables = [];
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
