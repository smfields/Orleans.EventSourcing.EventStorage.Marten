using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.EventSourcing.EventStorage.Marten;

/// <summary>
/// Factory for <see cref="MartenEventStorage"/>
/// </summary>
public static class MartenEventStorageFactory
{
    public static IEventStorage Create(IServiceProvider services, object? key)
    {
        var name = (string)key!;

        return ActivatorUtilities.CreateInstance<MartenEventStorage>(
            services,
            services.GetRequiredService<IOptionsMonitor<MartenOptions>>().Get(name),
            name
        );
    }
}