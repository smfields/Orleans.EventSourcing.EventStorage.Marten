using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.Marten;
using Orleans.TestingHost;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public class MartenEventStorageSiloBuilderExtensionsTests
{
    private TestCluster Cluster { get; set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public void Marten_storage_can_be_registered_as_default()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
        Assert.That(eventStorage, Is.TypeOf<MartenEventStorage>());
    }

    [Test]
    public void Marten_storage_can_be_registered_by_name()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredKeyedService<IEventStorage>("MartenEventStorage");
        Assert.That(eventStorage, Is.TypeOf<MartenEventStorage>());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Cluster.StopAllSilosAsync();
    }

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMartenEventStorage("MartenEventStorage", opts =>
            {
                opts.StoreOptions = storeOptions =>
                {
                    storeOptions.Connection(MartenSetup.ConnectionString);
                };
            });
            siloBuilder.AddMartenEventStorageAsDefault(opts =>
            {
                opts.StoreOptions = storeOptions =>
                {
                    storeOptions.Connection(MartenSetup.ConnectionString);
                };            
            });
        }
    }
}