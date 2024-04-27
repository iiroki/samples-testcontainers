using DotNet.Testcontainers.Containers;

namespace Samples.Testcontainers.Tests.Xunit.Testcontainers;

public abstract class TestcontainersFixture : IAsyncLifetime
{
    protected readonly IContainer Container;

    protected TestcontainersFixture()
    {
        Container = CreateContainer();
    }

    /// <summary>
    /// Creates the container used as a test dependency.
    /// </summary>
    protected abstract IContainer CreateContainer();

    public async Task InitializeAsync() => await StartContainerAsync();

    public async Task DisposeAsync()
    {
        await StopContainerAsync();
        await Container.DisposeAsync();
    }

    // <summary>
    /// Starts the container if it's not running.
    /// Use this to restart the container in the middle of tests, if needed.
    /// </summary>
    public async Task StartContainerAsync()
    {
        if (Container.State is TestcontainersStates.Undefined or TestcontainersStates.Paused)
        {
            await Container.StartAsync();
        }
    }

    // <summary>
    /// Stops the container.
    /// Use this to stop the container in the middle of tests, if needed.
    /// </summary>
    public async Task StopContainerAsync() => await Container.StopAsync();
}
