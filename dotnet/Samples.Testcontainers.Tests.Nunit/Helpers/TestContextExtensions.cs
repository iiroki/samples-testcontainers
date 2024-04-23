namespace Samples.Testcontainers.Tests.Nunit.Helpers;

public static class TestContextExtensions
{
    public static bool ShouldSkipInit(this TestContext ctx)
    {
        var categories = ctx.Test.Properties["Category"];
        return categories.Any(c => c is string str && str == SkipInitAttribute.Key);
    }
}
