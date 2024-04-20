using Samples.Testcontainers.Services;
using Samples.Testcontainers.Tests.Nunit.Testcontainers;

namespace Samples.Testcontainers.Tests.Nunit.Services;

[TestFixture]
public class DbServiceTests : PostgresTestContext
{
    private IDbService _db = null!;

    [SetUp]
    public void Setup()
    {
        _db = DbServiceFactory.Create(DbUrl);
    }

    [TearDown]
    public async Task DestroyAsync() => await _db.DisposeAsync();

    [Test]
    public async Task DbService_Init_Ok()
    {
        await _db.InitAsync();
    }
}
