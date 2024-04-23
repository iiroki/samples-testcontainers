namespace Samples.Testcontainers.Tests.Nunit.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class SkipInitAttribute : CategoryAttribute
{
    public const string Key = "_SkipInit";

    public SkipInitAttribute()
        : base(Key)
    {
        // NOP
    }
}
