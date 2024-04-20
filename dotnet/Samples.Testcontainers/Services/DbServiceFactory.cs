namespace Samples.Testcontainers.Services;

public static class DbServiceFactory
{
    public static IDbService Create(string dbUrl) => new DbService(dbUrl);
}
