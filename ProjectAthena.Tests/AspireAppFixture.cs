using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ProjectAthena.Tests;

public class AspireAppFixture
{
    public DistributedApplication App { get; private set; } = null!;

    public AspireAppFixture()
    {
        var appHost = DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ProjectAthena_AppHost>().GetAwaiter().GetResult();

        App = appHost.BuildAsync().GetAwaiter().GetResult();
        App.StartAsync().GetAwaiter().GetResult();
    }
}

[CollectionDefinition("AspireApp")]
public class AspireAppCollection : ICollectionFixture<AspireAppFixture>
{
}