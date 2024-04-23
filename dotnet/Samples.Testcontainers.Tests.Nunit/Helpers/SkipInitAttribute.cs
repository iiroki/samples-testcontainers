namespace Samples.Testcontainers.Tests.Nunit.Helpers;

/// <summary>
/// Attributes for skipping test initialization.<br />
/// <br />
/// Particularly useful for testing initialization functionalities themselves.
/// </summary>
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
