# Samples: Testcontainers

[![.NET Test](https://github.com/iiroki/samples-testcontainers/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/iiroki/samples-testcontainers/actions/workflows/dotnet-test.yml)

[**Testcontainers**](https://testcontainers.com/) is an open source framework for
running test dependencies as Docker containers.

The repository contains few samples on how to get started with Testcontainers with various languages.

## Tools

The samples in the repository depend on the following tools:
- Docker (required by Testcontainers)
- Language-specific SDKs (see ["Languages"](#languages) below)

## Languages

### .NET

.NET samples are located under [`dotnet/`](./dotnet/).

#### Structure

The sample .NET solution consist of three projects:

- **[`Samples.Testcontainers`](./dotnet/Samples.Testcontainers/):** The actual project that's tested
- **[`Samples.Testcontainers.Tests.Nunit`](./dotnet/Samples.Testcontainers.Tests.Nunit/):**
  [NUnit](https://nunit.org/) samples:
    - Postgres
- **[`Samples.Testcontainers.Tests.Xunit`](./dotnet/Samples.Testcontainers.Tests.Xunit/):**
  [xUnit](https://xunit.net/) samples:
    - Postgres
    - Blob Storage (= Azurite)
    - Postgres + Blob Storage (both used at the same time)


Only test projects have references to the tested project, not the other way around!

#### Commands

- Run all tests from the solution:
    ```bash
    dotnet test
    ```

- Run tests from a specific project:
    ```bash
    dotnet test <project_name>
    ```

#### Notes

- The samples utilize `InternalsVisibleToAttribute` to expose internal objects to the test projects,
  see [`Samples.Testcontainers.csproj`](./dotnet/Samples.Testcontainers/Samples.Testcontainers.csproj) for the usage.

### Node/TypeScript

Node samples are located under [`node/`](./node/).

#### Structure

TODO

## CI

The repository also demonstrates how to use Testcontainers in CI pipelines with GitHub Actions.

Integrating Testcontainers to CI pipelines is as almost as easy it can get,
since the tests can just be executed with the chosen language's standard test commands
without any extra configuration!

The GitHub Actions CI pipelines are located under [`.github/workflows`](./.github/workflows/):
- **[`dotnet-test.yml`](./.github/workflows/dotnet-test.yml):** .NET CI pipeline
    - Runs NUnit and xUnit tests separately and then together at the same time.
- TODO: Node CI
