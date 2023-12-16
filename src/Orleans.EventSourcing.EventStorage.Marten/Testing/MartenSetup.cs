using Testcontainers.PostgreSql;

// ReSharper disable once CheckNamespace
namespace Orleans;

[SetUpFixture]
public class MartenSetup
{
    public static string ConnectionString { get; private set; } = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var container = new PostgreSqlBuilder().Build();
        await container.StartAsync();

        ConnectionString = container.GetConnectionString();
    }
}